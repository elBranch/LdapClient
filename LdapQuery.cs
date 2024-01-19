﻿using LdapClient.Configuration;
using LdapClient.Repositories;
using Microsoft.Extensions.Logging;

namespace LdapClient;

/// <summary>
///     LDAP client
/// </summary>
/// <param name="settings">Platform configuration</param>
/// <param name="loggerFactory">ILoggerFactory compatible logger</param>
public sealed class LdapQuery(LdapSettings settings, ILoggerFactory loggerFactory) : IDisposable
{
    private LdapUsers? _users;

    /// <summary>
    ///     Users repository
    /// </summary>
    public LdapUsers Users => _users ??= new LdapUsers(settings, loggerFactory);

    /// <summary>
    ///     Dispose LDAP connection if necessary
    /// </summary>
    public void Dispose()
    {
        Users.Dispose();
        loggerFactory.Dispose();
    }
}