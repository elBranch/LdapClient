using System.Text;

namespace LdapClient.Common.Helpers;

/// <summary>
///     Provides helper methods for handling LDAP strings
/// </summary>
public static class LdapStringHelpers
{
    /// <summary>
    ///     Escapes a string to be used in an LDAP filter
    /// </summary>
    /// <param name="value"></param>
    /// <returns>A sanitized LDAP value to apply to a filter</returns>
    public static string EscapeFilterString(string value)
    {
        var builder = new StringBuilder();
        foreach (var curChar in value)
            switch (curChar)
            {
                case '\\':
                    builder.Append("\\5c");
                    break;
                case '*':
                    builder.Append("\\2a");
                    break;
                case '(':
                    builder.Append("\\28");
                    break;
                case ')':
                    builder.Append("\\29");
                    break;
                case '\u0000':
                    builder.Append("\\00");
                    break;
                default:
                    builder.Append(curChar);
                    break;
            }

        return builder.ToString();
    }
}