using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Novell.Directory.Ldap;
using Novell.Directory.Ldap.Utilclass;

namespace LdapClient.Common.Mappings;

/// <summary>
///     Provides mapping functionality for anonymous types
/// </summary>
/// <typeparam name="TEntity">Type with LdapAttribute attributes on properties</typeparam>
internal static class LdapEntryMapper<TEntity>
{
    /// <summary>
    ///     Attempts to map an anonymous type with LdapAttribute attributes on properties within the type to LDAP attributes in
    ///     a query
    /// </summary>
    /// <param name="entry"></param>
    /// <returns></returns>
    /// <exception cref="ApplicationException"></exception>
    public static TEntity Map(LdapEntry entry)
    {
        //if (entry is null) return default;

        var instance = Activator.CreateInstance<TEntity>();
        if (instance is null) throw new ApplicationException("Unable to create instance of object");

        var anonymousType = instance.GetType();
        var properties = anonymousType.GetProperties();

        foreach (var property in properties)
        {
            var attribute = property.GetCustomAttributes(false).OfType<LdapAttributeAttribute>().FirstOrDefault();
            if (attribute is null) continue;

            LdapAttribute? ldapAttribute;

            try
            {
                ldapAttribute = entry.GetAttribute(attribute.Name);
            }
            catch (KeyNotFoundException)
            {
                continue;
            }

            if (TryGetContainerElementType(property.PropertyType, out var type))
            {
                var values = ldapAttribute.StringValues;

                var constructedList = typeof(List<>).MakeGenericType(type);
                var list = Activator.CreateInstance(constructedList);

                if (list is null) throw new ApplicationException($"Failed to create list of {type}");

                while (values.MoveNext())
                {
                    if (values.Current is null) continue;
                    var value = ParsePrimitive(type, values.Current);
                    if (value is not null) ((IList) list).Add(value);
                }

                property.SetValue(instance, list);
            }
            else
            {
                property.SetValue(instance, ParsePrimitive(property.PropertyType, ldapAttribute.StringValue));
            }
        }

        return instance;
    }

    private static object? ParsePrimitive(Type property, string value)
    {
        // string
        if (property == typeof(string)) return value;

        // DateTime
        if (property == typeof(DateTime))
        {
            var parsedValue = ParseDateTime(value);
            return parsedValue ?? default(DateTime);
        }

        if (property == typeof(DateTime?)) return ParseDateTime(value);
        // int
        if (property == typeof(int)) return int.Parse(value);

        if (property == typeof(int?))
        {
            if (int.TryParse(value, out var parsedValue)) return parsedValue;
        }

        // bool
        else if (property == typeof(bool))
        {
            return ParseBoolean(value) ?? default(bool);
        }
        else if (property == typeof(bool?))
        {
            return ParseBoolean(value);
        }

        // Novel DN
        else if (property == typeof(Dn))
        {
            return new Dn(value);
        }

        // char
        else if (property == typeof(char))
        {
            return value[0];
        }
        else if (property == typeof(char?))
        {
            return value.Length > 0 ? default : value[0];
        }

        // Not 
        // TypeCode.Empty
        // TypeCode.Object
        // TypeCode.DBNull

        throw new NotImplementedException($"Unable to parse type {property}");
    }

    private static bool? ParseBoolean(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        switch (value.Trim().ToUpperInvariant())
        {
            case "1":
            case "Y":
            case "YES":
            case "T":
            case "TRUE":
                return true;

            case "0":
            case "N":
            case "NO":
            case "F":
            case "FALSE":
                return false;

            default:
                return null;
        }
    }

    private static DateTime? ParseDateTime(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        if (long.TryParse(value, out var numericFileTime))
            return DateTime.FromFileTime(numericFileTime);

        if (DateTime.TryParseExact(value, "yyyyMMddHHmmss.fZ", DateTimeFormatInfo.InvariantInfo,
                DateTimeStyles.AssumeLocal, out var dateTime))
            return dateTime;

        return null;
    }

    /// <summary>
    ///     Attempts to identify if an object is a container, what type of container it is, and returning the element type
    /// </summary>
    /// <param name="container">The containing object</param>
    /// <param name="elementType">Type of object within the container</param>
    /// <returns>Element type and if object is container</returns>
    private static bool TryGetContainerElementType(Type container, [NotNullWhen(true)] out Type? elementType)
    {
        if (typeof(string).IsAssignableFrom(container))
        {
            elementType = typeof(string);
            return false;
        }

        foreach (var i in container.GetInterfaces())
            if (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                elementType = i.GetGenericArguments()[0];
                return true;
            }

        if (typeof(IDictionary).IsAssignableFrom(container))
        {
            elementType = typeof(DictionaryEntry);
            return true;
        }

        if (typeof(IEnumerable).IsAssignableFrom(container))
            foreach (var property in container.GetProperties())
            {
                if (property.Name != "Item" || property.PropertyType == typeof(object)) continue;

                var parameters = property.GetIndexParameters();
                if (parameters.Length != 1 || parameters[0].ParameterType != typeof(int)) continue;

                elementType = property.PropertyType;
                return true;
            }

        if (typeof(ICollection).IsAssignableFrom(container))
            foreach (var method in container.GetMethods())
            {
                if (method.Name != "Add") continue;

                var parameters = method.GetParameters();
                if (1 != parameters.Length || typeof(object) == parameters[0].ParameterType) continue;

                elementType = parameters[0].ParameterType;
                return true;
            }

        if (typeof(IEnumerable).IsAssignableFrom(container))
        {
            elementType = typeof(object);
            return true;
        }

        elementType = null;
        return false;
    }
}