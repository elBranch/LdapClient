using Novell.Directory.Ldap;
using Novell.Directory.Ldap.Asn1;

namespace LdapClient.Common.Handlers;

/// <summary>
///     An LDAP client application that needs to control the rate at which results are returned
///     MAY specify on the searchRequest a <see cref="AsyncPagedResultsControl" /> with size set
///     to the desired page size and cookie set to the zero-length string. The page size specified
///     MAY be greater than zero and less than the sizeLimit value specified in the
///     searchRequest. [RFC 2696].
/// </summary>
public class AsyncPagedResultsControl : LdapControl
{
    private const string RequestOid = "1.2.840.113556.1.4.319";
    private const string DecodedNotInteger = "Decoded value is not an integer, but should be";
    private const string DecodedNotOctetString = "Decoded value is not an octet string, but should be";

    private const string DecodedNotSequence =
        $"Failed to construct {nameof(AsyncPagedResultsControl)}: provided values might not be decoded as {nameof(Asn1Sequence)}";


    static AsyncPagedResultsControl()
    {
        try
        {
            Register(RequestOid, typeof(AsyncPagedResultsControl));
        }
        catch (Exception ex)
        {
            Logger.Log.LogWarning($"Failed to bind oid <{RequestOid}> to control <{nameof(AsyncPagedResultsControl)}>",
                ex);
        }
    }

    /// <summary>
    ///     Constructs <see cref="AsyncPagedResultsControl" /> control.
    /// </summary>
    /// <param name="size">Initial size of cookie</param>
    /// <param name="cookie">Initial cookie</param>
    public AsyncPagedResultsControl(int size, byte[]? cookie) : base(RequestOid, true, null)
    {
        Size = size;
        Cookie = cookie ?? GetEmptyCookie;

        var request = new Asn1Sequence(2);
        request.Add(new Asn1Integer(Size));
        request.Add(new Asn1OctetString(Cookie));

        SetValue(request.GetEncoding(new LberEncoder()));
    }

    /// <summary>
    ///     Constructs <see cref="AsyncPagedResultsControl" />
    /// </summary>
    /// <param name="oid">The OID of the control, as a dotted string.</param>
    /// <param name="critical">
    ///     True if the Ldap operation should be discarded if the control is not supported. False if the
    ///     operation can be processed without the control.
    /// </param>
    /// <param name="values">The control-specific data.</param>
    /// <exception cref="InvalidOperationException">If decoder constructor fails</exception>
    /// <exception cref="InvalidCastException">If unable to cast ASN1 object to sequence</exception>
    public AsyncPagedResultsControl(string oid, bool critical, byte[] values) : base(oid, critical, values)
    {
        var decoder = new LberDecoder();
        if (decoder == null) throw new InvalidOperationException($"Failed to build {nameof(LberDecoder)}");

        var asn1Object = decoder.Decode(values);
        if (asn1Object is not Asn1Sequence sequence) throw new InvalidCastException(DecodedNotSequence);

        var size = sequence.get_Renamed(0);
        if (size is not Asn1Integer integerSize) throw new InvalidOperationException(DecodedNotInteger);

        Size = integerSize.IntValue();

        var cookie = sequence.get_Renamed(1);
        if (cookie is not Asn1OctetString octetCookie) throw new InvalidOperationException(DecodedNotOctetString);

        Cookie = octetCookie.ByteValue();
    }

    /// <summary>
    ///     <list type="number">
    ///         <item>
    ///             <term>REQUEST</term>
    ///             <description>
    ///                 An LDAP client application that needs to control the rate at which results are returned MAY
    ///                 specify on the searchRequest a pagedResultsControl with size set to the desired page size
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <term>RESPONSE</term>
    ///             <description>
    ///                 Each time the server returns a set of results to the client when processing a search request
    ///                 containing the pagedResultsControl, the server includes the pagedResultsControl control in the
    ///                 searchResultDone message. In the control returned to the client, the size MAY be set to the server’s
    ///                 estimate of the total number of entries in the entire result set. Servers that cannot provide such an
    ///                 estimate MAY set this size to zero (0).
    ///             </description>
    ///         </item>
    ///     </list>
    /// </summary>
    public int Size { get; }

    /// <summary>
    ///     <list type="number">
    ///         <item>
    ///             <term>INITIAL REQUEST</term>
    ///             <description>empty cookie</description>
    ///         </item>
    ///         <item>
    ///             <term>CONSEQUENT REQUEST</term>
    ///             <description>cookie from previous response</description>
    ///         </item>
    ///         <item>
    ///             <term>RESPONSE</term>
    ///             <description>
    ///                 The cookie MUST be set to an empty value if there are no more entries to return (i.e., the
    ///                 page of search results returned was the last), or, if there are more entries to return, to an octet
    ///                 string of the server’s choosing, used to resume the search.
    ///             </description>
    ///         </item>
    ///     </list>
    /// </summary>
    public byte[]? Cookie { get; }

    /// <summary>
    ///     An empty byte array
    /// </summary>
    public static byte[] GetEmptyCookie => Array.Empty<byte>();

    /// <summary>
    ///     Determine if cooke is empty
    /// </summary>
    /// <returns></returns>
    public bool IsEmptyCookie()
    {
        return Cookie == null || Cookie.Length == 0;
    }
}