namespace LdapClient.Configuration;

/// <summary>
///     Settings for the LDAP Client
/// </summary>
public class LdapSettings
{
    /// <summary>
    ///     Server host name
    /// </summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>
    ///     Server port
    /// </summary>
    public int Port { get; set; } = 389;

    /// <summary>
    ///     LDAP username
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    ///     LDAP password
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    ///     List of OUs to query
    /// </summary>
    public string[] BaseOu { get; set; } = Array.Empty<string>();
}