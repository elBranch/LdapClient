namespace LdapClient.SearchParameters;

/// <summary>
///     SAM Account Name
/// </summary>
/// <param name="value">SAM Account Name value</param>
public class SamAccountName(string value) : SimpleStringParameter(value)
{
    /// <summary>
    ///     Parse an LDAP Ambiguous Name
    /// </summary>
    /// <param name="value">The user's name</param>
    /// <returns>AmbiguousName object</returns>
    public static SamAccountName Parse(string value)
    {
        return new SamAccountName(value);
    }
}