using LdapClient.Common.Helpers;

namespace LdapClient.SearchParameters;

/// <summary>
///     Generic parameter that is a simple string.
/// </summary>
public abstract class SimpleStringParameter
{
    /// <summary>
    ///     Initialize a parameter that is a simple string.
    /// </summary>
    /// <param name="value">SAM Account Name value</param>
    protected SimpleStringParameter(string value)
    {
        Value = LdapStringHelpers.EscapeFilterString(value.Trim());
    }

    /// <summary>
    ///     Parsed string.
    /// </summary>
    private string Value { get; }

    /// <summary>
    ///     Returns the value of the object.
    /// </summary>
    /// <returns>Set value</returns>
    public override string ToString()
    {
        return Value;
    }
}