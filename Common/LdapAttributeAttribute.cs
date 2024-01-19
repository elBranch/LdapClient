namespace LdapClient.Common;

/// <summary>
///     Provides the ability to specify the name of an LDAP field of a backend system
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class LdapAttributeAttribute : Attribute
{
    /// <summary>
    ///     Name of the LDAP attribute
    /// </summary>
    public readonly string Name;

    /// <summary>
    ///     Signifies the property is an LDAP attribute field
    /// </summary>
    /// <param name="name">
    ///     <inheritdoc cref="Name" />
    /// </param>
    public LdapAttributeAttribute(string name)
    {
        Name = name;
    }
}