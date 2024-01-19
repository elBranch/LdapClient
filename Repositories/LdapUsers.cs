using LdapClient.Common;
using LdapClient.Configuration;
using LdapClient.SearchParameters;
using Microsoft.Extensions.Logging;
using Novell.Directory.Ldap.Utilclass;

namespace LdapClient.Repositories;

/// <summary>
///     Provides a User repository for LDAP
/// </summary>
public class LdapUsers : GenericRepository
{
    /// <summary>
    ///     Initialize an LdapUsers repository
    /// </summary>
    /// <param name="settings">LDAP app settings</param>
    /// <param name="loggerFactory">ILoggerFactory compatible logger</param>
    public LdapUsers(LdapSettings settings, ILoggerFactory loggerFactory) : base(settings)
    {
        Log = loggerFactory.CreateLogger(typeof(LdapUsers));
    }

    /// <summary>
    ///     Get user by Dn
    /// </summary>
    /// <param name="userDn">A Novell DN</param>
    /// <typeparam name="T">Type of profile to return</typeparam>
    /// <returns>Requested profile or null</returns>
    public new async Task<T?> GetAsync<T>(Dn? userDn)
    {
        if (userDn is null) return default;
        return await base.GetAsync<T>(userDn);
    }

    /// <summary>
    ///     Get user by ANR
    /// </summary>
    /// <param name="name">Ambiguous Name</param>
    /// <param name="token">Cancellation token</param>
    /// <typeparam name="T">Type of profile to return</typeparam>
    /// <returns>Requested profile or null</returns>
    public async Task<T?> GetAsync<T>(AmbiguousName name, CancellationToken token = default)
    {
        var filter = $"(&(objectCategory=person)(objectClass=user)(anr={name}))";
        return await QueryAsync<T>(filter, 1, token).SingleOrDefaultAsync(token);
    }

    /// <summary>
    ///     Get user by SAM Account Name
    /// </summary>
    /// <param name="name">Ambiguous Name</param>
    /// <param name="token">Cancellation token</param>
    /// <typeparam name="T">Type of profile to return</typeparam>
    /// <returns>Requested profile or null</returns>
    public async Task<T?> GetAsync<T>(SamAccountName name, CancellationToken token = default)
    {
        var filter = $"(&(objectCategory=person)(objectClass=user)(sAMAccountName={name}))";
        return await QueryAsync<T>(filter, 1, token).SingleOrDefaultAsync(token);
    }

    /// <summary>
    ///     Search LDAP using an ANR
    /// </summary>
    /// <param name="name">User ANR</param>
    /// <param name="maxResults">Maximum number of results to return per configuration provided OU</param>
    /// <param name="token">Cancellation token</param>
    /// <returns>UserIdentity</returns>
    public IAsyncEnumerable<T?> SearchAsync<T>(AmbiguousName name, int maxResults = 100,
        CancellationToken token = default)
    {
        var filter = $"(&(objectCategory=person)(objectClass=user)(anr={name}))";
        return QueryAsync<T>(filter, maxResults, token);
    }

    /// <summary>
    ///     Allows searching LDAP using raw LDAP filters. No sanitization is taking place and must be handled by author.
    /// </summary>
    /// <param name="filter">LDAP filter string</param>
    /// <param name="maxResults">Maximum number of results</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>User Identity</returns>
    public new IAsyncEnumerable<T> QueryAsync<T>(string filter, int maxResults = 100, CancellationToken ct = default)
    {
        return base.QueryAsync<T>(filter, maxResults, ct);
    }
}