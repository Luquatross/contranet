Namespace WebRequests
    ''' <summary>
    ''' Gets the top listened artists of a user
    ''' </summary>
    Public Class UserTopListenedArtistsRequest
        Inherits RequestBase
        Dim m_user As String
        Dim m_type As String
        Dim m_Artists As New TypeClasses.WeightedArtistList
        Dim m_ts As TopXTimeSpan = TopXTimeSpan.None

        ''' <summary>
        ''' Gets or sets the time span of which the chart should be generated
        ''' </summary>
        ''' <value>The chart time span.</value>
        Property ChartTimeSpan() As TopXTimeSpan
            Get
                Return m_ts
            End Get
            Set(ByVal value As TopXTimeSpan)
                m_ts = value
            End Set
        End Property
        ''' <summary>
        ''' Gets the artists.
        ''' </summary>
        ''' <value>The artists.</value>
        ReadOnly Property Artists() As TypeClasses.WeightedArtistList
            Get
                Return m_Artists
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the username
        ''' </summary>
        ''' <value>The user.</value>
        Property User() As String
            Get
                Return m_user
            End Get
            Set(ByVal value As String)
                m_user = value
            End Set
        End Property
        Public Overrides Sub Start()
            MyBase.get("/1.0/user/" & User & "/topartists.xml" & IIf(m_ts <> TopXTimeSpan.None, "?type=" & TopXTimeSpanToURiString(m_ts), ""))
        End Sub
        ''' <summary>
        ''' Initializes a new instance of the <see cref="UserTopListenedArtistsRequest" /> class.
        ''' </summary>
        ''' <param name="username">The username.</param>
        ''' <param name="ts">The time span of which the chart should be generated.</param>
        Sub New(ByVal username As String, Optional ByVal ts As TopXTimeSpan = TopXTimeSpan.None)
            MyBase.New(WebRequests.RequestType.TopArtists, "TopArtists")
            m_user = username
            m_ts = ts
        End Sub
        Protected Overrides Sub success(ByVal data As String)
            m_Artists.Clear()
            Dim doc As New Xml.XmlDocument
            Try
                doc.LoadXml(data)
            Catch x As Xml.XmlException
                Throw New Exceptions.ParseException("UserTopArtistsRequest(" & m_user & "): Couldn't parse XML.", x)
            End Try
            Dim topartists As Xml.XmlNode = doc.SelectSingleNode("topartists")
            m_user = topartists.Attributes("user").Value
            m_type = topartists.Attributes("type").Value

            Dim artists As Xml.XmlNodeList = topartists.SelectNodes("artist")
            For Each n As Xml.XmlNode In artists
                Dim name As String = n.SelectSingleNode("name").InnerText
                Dim mbid As String = n.SelectSingleNode("mbid").InnerText
                Dim playcount As Integer = n.SelectSingleNode("playcount").InnerText
                Dim url As Uri = GetUriFromString(n.SelectSingleNode("url").InnerText)
                Dim imagesmall As Uri = GetUriFromString(n.SelectSingleNode("thumbnail").InnerText)
                Dim image As Uri = GetUriFromString(n.SelectSingleNode("image").InnerText)
                Dim rank As Integer = CInt(n.SelectSingleNode("rank").InnerText)
                Dim w As New TypeClasses.WeightedArtist(name, rank)
                With w
                    .mbId = mbid
                    .PlayCount = playcount
                    .Url = url
                    .ImageSmall = imagesmall
                    .Image = image
                End With
                m_Artists.Add(w)
            Next

        End Sub
    End Class
End Namespace