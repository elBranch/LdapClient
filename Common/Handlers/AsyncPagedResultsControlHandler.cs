using System.Runtime.CompilerServices;
using Novell.Directory.Ldap;

namespace LdapClient.Common.Handlers;

/// <summary>
///     Helper to perform paged searches with <see cref="AsyncPagedResultsControl" />.
/// </summary>
public class AsyncPagedResultsControlHandler
{
    private const string ControlNotFound = $"{nameof(AsyncPagedResultsControl)} unavailable";
    private readonly ILdapConnection _ldapConnection;

    /// <summary>
    /// </summary>
    /// <param name="ldapConnection"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public AsyncPagedResultsControlHandler(ILdapConnection ldapConnection)
    {
        _ldapConnection = ldapConnection ?? throw new ArgumentNullException(nameof(ldapConnection));
    }

    /// <summary>
    /// </summary>
    /// <param name="converter"></param>
    /// <param name="options"></param>
    /// <param name="pageSize"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public async IAsyncEnumerable<T> QueryAsync<T>(
        Func<LdapEntry, T> converter,
        SearchOptions options,
        int pageSize,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var searchConstraints = options.SearchConstraints ?? _ldapConnection.SearchConstraints;
        var isNextPageAvailable = QueueNextPage(null, pageSize, true, ref searchConstraints);
        while (isNextPageAvailable)
        {
            var asyncSearchResults = await _ldapConnection.SearchAsync(
                options.SearchBase,
                LdapConnection.ScopeSub,
                options.Filter,
                options.TargetAttributes,
                false,
                searchConstraints
            );

            await using (var enumerator = asyncSearchResults.GetAsyncEnumerator(cancellationToken))
            {
                var hasResult = true;
                while (hasResult)
                {
                    // MoveNextAsync will throw an exception if we reach our requested maximum 
                    try
                    {
                        hasResult = await enumerator.MoveNextAsync();
                    }
                    catch (LdapException ex) when (ex.ResultCode is 4)
                    {
                        yield break;
                    }

                    // TODO: Is this okay? It was returning duplicate results when querying 1 user. I assume this was broken because the enumerator didn't actually move and we weren't even checking if it had
                    if (!hasResult) yield break;

                    yield return converter.Invoke(enumerator.Current);
                }
            }

            var responseControls = asyncSearchResults.ResponseControls;

            isNextPageAvailable = QueueNextPage(responseControls, pageSize, false, ref searchConstraints);
        }
    }

    private static bool QueueNextPage(
        IEnumerable<LdapControl>? responseControls,
        int pageSize,
        bool isInitialCall,
        ref LdapSearchConstraints constraints)
    {
        var cookie = AsyncPagedResultsControl.GetEmptyCookie;
        if (!isInitialCall)
        {
            if (responseControls is null) throw new LdapException(ControlNotFound);

            var resultsControl = responseControls.SingleOrDefault(x => x is AsyncPagedResultsControl);
            if (resultsControl is AsyncPagedResultsControl control)
            {
                if (control.IsEmptyCookie()) return false;
                cookie = control.Cookie;
            }
            else
            {
                throw new LdapException(ControlNotFound);
            }
        }

        // if (cookie is null) throw new LdapException("Cookie unavailable");

        var nextControl = new AsyncPagedResultsControl(pageSize, cookie);
        constraints.BatchSize = 1;
        constraints.SetControls(nextControl);
        return true;
    }
}