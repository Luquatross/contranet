Imports System.Xml
Namespace API20.Types
    ''' <summary>
    ''' Class for storing value data
    ''' </summary>
    Public Class Venue
        Dim m_name As String
        Dim m_city As String
        Dim m_country As String
        Dim m_street As String
        Dim m_postalCode As Integer
        Dim m_geoPoint As Drawing.PointF
        Dim m_timzone As String
        Dim m_url As Uri
        Dim m_id As Integer
        ''' <summary>
        ''' Gets or sets the URL.
        ''' </summary>
        ''' <value>The URL.</value>
        Property Url() As Uri
            Get
                Return m_url
            End Get
            Set(ByVal value As Uri)
                m_url = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the timzone.
        ''' </summary>
        ''' <value>The timzone.</value>
        Property Timzone() As String
            Get
                Return m_timzone
            End Get
            Set(ByVal value As String)
                m_timzone = value
            End Set
        End Property
        ''' <summary>
        ''' The geogrpahic venue point, where X is the latitude and y the longitude
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property GeoPoint() As Drawing.PointF
            Get

                Return m_geoPoint
            End Get
            Set(ByVal value As Drawing.PointF)
                m_geoPoint = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the postal code.
        ''' </summary>
        ''' <value>The postal code.</value>
        Property PostalCode() As Integer
            Get
                Return m_postalCode
            End Get
            Set(ByVal value As Integer)
                m_postalCode = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the street.
        ''' </summary>
        ''' <value>The street.</value>
        Property Street() As String
            Get
                Return m_street
            End Get
            Set(ByVal value As String)
                m_street = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the country.
        ''' </summary>
        ''' <value>The country.</value>
        Property Country() As String
            Get
                Return m_country
            End Get
            Set(ByVal value As String)
                m_country = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the city.
        ''' </summary>
        ''' <value>The city.</value>
        Property City() As String
            Get
                Return m_city
            End Get
            Set(ByVal value As String)
                m_city = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the name.
        ''' </summary>
        ''' <value>The name.</value>
        Property Name() As String
            Get
                Return m_name
            End Get
            Set(ByVal value As String)
                m_name = value
            End Set
        End Property
        Sub New(ByVal elem As Xml.XmlElement)
            Dim nsm As New XmlNamespaceManager(elem.OwnerDocument.NameTable)
            nsm.AddNamespace("geo", "http://www.w3.org/2003/01/geo/wgs84_pos#")
            m_name = Util.GetSubElementValue(elem, "name")

            Dim loc As XmlElement = elem.SelectSingleNode("location")
            m_city = Util.GetSubElementValue(loc, "city")
            m_street = Util.GetSubElementValue(loc, "street")
            Integer.TryParse(Util.GetSubElementValue(loc, "postalcode"), m_postalCode)

            Dim latitude As Single
            Dim latstr As String = Util.GetSubElementValue(loc, "geo:point/geo:lat", nsm)
            'see if this is country dependant!
            'If Not String.IsNullOrEmpty(latstr) Then latstr = latstr.Replace(".", ",")
            If Not String.IsNullOrEmpty(latstr) Then latitude = Single.Parse(latstr, System.Globalization.NumberFormatInfo.InvariantInfo)

            Dim longitude As Single
            Dim longstr As String = Util.GetSubElementValue(loc, "geo:point/geo:long", nsm)
            'see if this is country dependant!
            'If Not String.IsNullOrEmpty(longstr) Then longstr = longstr.Replace(".", ",")
            If Not String.IsNullOrEmpty(longstr) Then longitude = Single.Parse(longstr, System.Globalization.NumberFormatInfo.InvariantInfo)
            Dim p As New Drawing.PointF(latitude, longitude)
            m_geoPoint = p
            m_timzone = Util.GetSubElementValue(loc, "timezone")

            Uri.TryCreate(Util.GetSubElementValue(elem, "url"), UriKind.RelativeOrAbsolute, m_url)
        End Sub

        Public Function ToDebugString() As String
            Dim result As String = ""
            With Me
                result &= "Venue name:" & .Name & vbCrLf
                result &= "City: " & .City & vbCrLf
                result &= "Street: " & .Street & vbCrLf
                result &= "Postal code: " & vbCrLf
                result &= "Geo point: " & .GeoPoint.ToString & vbCrLf
                result &= "Time zone: " & .Timzone & vbCrLf
                result &= "Venue url: " & Util.GetUrlstrOrNothing(Me.Url)
            End With
            Return result
        End Function
    End Class
End Namespace