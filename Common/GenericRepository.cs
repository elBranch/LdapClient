using System.Reflection;
using System.Runtime.CompilerServices;
using LdapClient.Common.Handlers;
using LdapClient.Common.Helpers;
using LdapClient.Common.Mappings;
using LdapClient.Configuration;
using Microsoft.Extensions.Logging;
using Novell.Directory.Ldap;
using Novell.Directory.Ldap.Utilclass;

namespace LdapClient.Common;

/// <summary>
///     Generalized LDAP repository for a baseline.
/// </summary>
public abstract class GenericRepository : IDisposable
{
    private readonly LdapConnection _ldapConnection;
    private readonly LdapSettings _settings;

    /// <summary>
    ///     .NET supported Logger
    /// </summary>
    protected ILogger? Log;

    /// <summary>
    ///     Initializes a generic repository to base other repositories on
    /// </summary>
    /// <param name="settings">LDAP app settings</param>
    protected GenericRepository(LdapSettings settings)
    {
        _settings = settings;
        var ldapConnection = new LdapConnection();
        ldapConnection.ConnectAsync(settings.Host, settings.Port).Wait();
        ldapConnection.BindAsync(settings.Username, settings.Password).Wait();

        _ldapConnection = ldapConnection;
    }

    /// <summary>
    ///     Dispose the used resources on this class
    /// </summary>
    public void Dispose()
    {
        _ldapConnection.Dispose();
    }

    /// <summary>
    ///     Retrieve an entry from directory via distinguished named
    /// </summary>
    /// <param name="dn">Distinguished name of object</param>
    /// <typeparam name="T">Type of object to return</typeparam>
    /// <returns>T</returns>
    protected async Task<T?> GetAsync<T>(Dn dn)
    {
        Log?.LogDebug("Retrieving record {dn}", dn);
        var attributes = typeof(T).GetCustomAttributes(false)
            .OfType<LdapAttributeAttribute>().Select(s => s.Name).ToArray();

        return LdapEntryMapper<T>.Map(await _ldapConnection.ReadAsync(dn.ToString(), attributes));
    }

    /// <summary>
    ///     Search LDAP using standard LDAP filter returning results asynchronously as they are received
    /// </summary>
    /// <param name="filter">Query string to poll the LDAP server with</param>
    /// <param name="maxResults">Maximum number of responses, server side</param>
    /// <param name="ct">Cancellation token</param>
    /// <typeparam name="T">Type with LdapAttribute applied to fields you wish to return</typeparam>
    /// <returns>
    ///     Enumerable T
    /// </returns>
    protected async IAsyncEnumerable<T> QueryAsync<T>(string filter, int maxResults,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        Log?.LogDebug("Searching for records with {filter}", filter);
        var baseOu = _settings.BaseOu;
        var attributes = (from property in typeof(T).GetProperties()
            from attribute in property.GetCustomAttributes().OfType<LdapAttributeAttribute>()
            select attribute.Name).ToArray();

        var constraints = new LdapSearchConstraints
        {
            BatchSize = 1,
            Dereference = LdapSearchConstraints.DerefNever,
            MaxResults = maxResults
        };

        var tasks = new List<IAsyncEnumerable<T>>();

        if (baseOu.Length == 0)
            throw new LdapException("At least one BaseOU must be specified in app settings");

        tasks.AddRange(baseOu.Select(s => QueryAsync<T>(s, filter, attributes, constraints, ct)));

        var records = new MergedAsyncEnumerable<T>([.. tasks]);
        await foreach (var record in records) yield return record;
    }

    /// <summary>
    ///     Search LDAP by filter, returning results in an asynchronous manner
    /// </summary>
    /// <param name="baseOu">Base path to search</param>
    /// <param name="filter">Filter to search with</param>
    /// <param name="attributes">Array of attribute names to return</param>
    /// <param name="constraints">Search Constraints</param>
    /// <param name="cancellationToken">Cancellation Token</param>
    /// <typeparam name="T">Type of object to return</typeparam>
    /// <returns>Requested results</returns>
    private async IAsyncEnumerable<T> QueryAsync<T>(string baseOu, string filter, string[] attributes,
        LdapSearchConstraints constraints, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var handler = new AsyncPagedResultsControlHandler(_ldapConnection);
        var options = new SearchOptions(baseOu, LdapConnection.ScopeBase, filter, attributes, false, constraints);
        await foreach (var entity in handler.QueryAsync(LdapEntryMapper<T>.Map, options, 100, cancellationToken))
            yield return entity;
    }
}