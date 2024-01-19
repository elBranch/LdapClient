using LdapClient.Common;

namespace LdapClient.Entities;

/// <summary>
///     Provides a basic record to identify an individual
/// </summary>
public record UserIdentity
{
    /// <summary>
    ///     The display name for an object.
    /// </summary>
    [LdapAttribute("displayName")]
    public string? DisplayName { get; init; }

    /// <summary>
    ///     The list of email addresses for a contact.
    /// </summary>
    [LdapAttribute("mail")]
    public string? EmailAddress { get; init; }

    /// <summary>
    ///     Contains the given name (first name) of the user.
    /// </summary>
    [LdapAttribute("givenName")]
    public string? GivenName { get; init; }

    /// <summary>
    ///     The logon name used to support clients and servers running earlier versions of the operating system, such as
    ///     Windows NT 4.0, Windows 95, Windows 98, and LAN Manager.
    /// </summary>
    [LdapAttribute("sAMAccountName")]
    public required string SamAccountName { get; init; }

    /// <summary>
    ///     This attribute contains the family or last name for a user.
    /// </summary>
    [LdapAttribute("sn")]
    public string? Surname { get; init; }
}