using LdapClient.Common;
using LdapClient.Common.Enums;
using Novell.Directory.Ldap.Utilclass;

namespace LdapClient.Entities;

/// <summary>
///     Provides a full profile of a user in LDAP
/// </summary>
public record UserProfile
{
    /// <summary>
    ///     The user's company name.
    /// </summary>
    [LdapAttribute("company")]
    public string? Company { get; init; }

    /// <summary>
    ///     Contains the name for the department in which the user works.
    /// </summary>
    [LdapAttribute("department")]
    public string? Department { get; init; }

    /// <summary>
    ///     The display name for an object.
    /// </summary>
    [LdapAttribute("displayName")]
    public string? DisplayName { get; init; }

    /// <summary>
    ///     The user's division.
    /// </summary>
    [LdapAttribute("division")]
    public string? Division { get; init; }

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
    ///     Contains the distinguished name of the user who is the user's manager.
    /// </summary>
    [LdapAttribute("manager")]
    public Dn? Manager { get; init; }

    /// <summary>
    ///     List of groups the user is part of
    /// </summary>
    [LdapAttribute("memberOf")]
    public ICollection<Dn>? MemberOf { get; init; }

    /// <summary>
    ///     Mobile number
    /// </summary>
    [LdapAttribute("mobile")]
    public string? MobilePhone { get; init; }

    /// <summary>
    ///     The date and time that the password for this account was last changed.
    /// </summary>
    [LdapAttribute("pwdLastSet")]
    public DateTime? PasswordLastSet { get; init; }

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

    /// <summary>
    ///     Telephone number
    /// </summary>
    [LdapAttribute("telephoneNumber")]
    public string? Telephone { get; init; }

    /// <summary>
    ///     Contains the user's job title. This property is commonly used to indicate the formal job title, such as Senior
    ///     Programmer, rather than occupational class, such as programmer.
    /// </summary>
    [LdapAttribute("title")]
    public string? Title { get; init; }

    /// <summary>
    ///     Flags that control the behavior of the user account.
    /// </summary>
    [LdapAttribute("userAccountControl")]
    private int UserAccountControl { get; }

    /// <summary>
    ///     The date when this object was changed.
    /// </summary>
    [LdapAttribute("whenChanged")]
    public DateTime? WhenChanged { get; init; }

    /// <summary>
    ///     The date when this object was created.
    /// </summary>
    [LdapAttribute("whenCreated")]
    public DateTime? WhenCreated { get; init; }

    /// <summary>
    ///     Computed value to determine if the account is enabled based on User Account Control
    /// </summary>
    public bool IsEnabled => !Convert.ToBoolean(UserAccountControl & (int)UserFlag.ADS_UF_ACCOUNTDISABLE);
}