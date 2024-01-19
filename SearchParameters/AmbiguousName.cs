using LdapClient.Common.Helpers;

namespace LdapClient.SearchParameters;

/// <summary>
///     Parse a LDAP ambiguous name
/// </summary>
public class AmbiguousName(string value) : SimpleStringParameter(value)
{
    /// <summary>
    ///     Parse an LDAP Ambiguous Name
    /// </summary>
    /// <param name="value">The user's name</param>
    /// <returns>AmbiguousName object</returns>
    public static AmbiguousName Parse(string value)
    {
        return new AmbiguousName(value);
    }
}