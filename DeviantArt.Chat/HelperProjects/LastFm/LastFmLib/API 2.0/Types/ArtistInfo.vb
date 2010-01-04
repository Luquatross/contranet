Namespace API20.Types
    ''' <summary>
    ''' Stores artist information
    ''' </summary>
    Public Class ArtistInfo
        Inherits Base.BaseImageData
        Dim m_artist As String
        Dim m_mbId As Guid
        Dim m_url As Uri

        Dim m_imageOrignial As Uri
        Dim m_imageHuge As Uri
        Dim m_numListeners As Integer
        Dim m_playCount As Integer
        Dim m_streamable As Boolean
        Dim m_tags As Dictionary(Of String, Uri)
        Dim m_bioPushlishDate As DateTime
        Dim m_bioSummary As String
        Dim m_bioText As String
        Dim m_similarArtists As Dictionary(Of String, ArtistInfo)

        ''' <summary>
        ''' Gets or sets the similar artists.
        ''' </summary>
        ''' <value>The similar artists.</value>
        Property SimilarArtists() As Dictionary(Of String, ArtistInfo)
            Get
                Return m_similarArtists
            End Get
            Set(ByVal value As Dictionary(Of String, ArtistInfo))
                m_similarArtists = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the tags.
        ''' </summary>
        ''' <value>The tags.</value>
        Property Tags() As Dictionary(Of String, Uri)
            Get
                Return m_tags
            End Get
            Set(ByVal value As Dictionary(Of String, Uri))
                m_tags = value
            End Set
        End Property
        ReadOnly Property HasMbId() As Boolean
            Get
                Return Not IsNothing(m_mbId)
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the biography text.
        ''' </summary>
        ''' <value>The bio text.</value>
        Property BioText() As String
            Get
                Return m_bioText
            End Get
            Set(ByVal value As String)
                m_bioText = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the biography summary.
        ''' </summary>
        ''' <value>The bio summary.</value>
        Property BioSummary() As String
            Get
                Return m_bioSummary
            End Get
            Set(ByVal value As String)
                m_bioSummary = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the bio pushlish date.
        ''' </summary>
        ''' <value>The bio pushlish date.</value>
        Property BioPushlishDate() As DateTime
            Get
                Return m_bioPushlishDate
            End Get
            Set(ByVal value As DateTime)
                m_bioPushlishDate = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets a value indicating whether this artist is streamable.
        ''' </summary>
        ''' <value>
        ''' <c>true</c> if this instance is streamable; otherwise, <c>false</c>.
        ''' </value>
        Property IsStreamable() As Boolean
            Get
                Return m_streamable
            End Get
            Set(ByVal value As Boolean)
                m_streamable = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the play count.
        ''' </summary>
        ''' <value>The play count.</value>
        Property PlayCount() As Integer
            Get
                Return m_playCount
            End Get
            Set(ByVal value As Integer)
                m_playCount = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the number of listeners.
        ''' </summary>
        ''' <value>The num listeners.</value>
        Property NumListeners() As Integer
            Get
                Return m_numListeners
            End Get
            Set(ByVal value As Integer)
                m_numListeners = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the original image's url.
        ''' </summary>
        ''' <value>The url.</value>
        Property ImageOriginal() As Uri
            Get
                Return m_imageOrignial
            End Get
            Set(ByVal value As Uri)
                m_imageOrignial = value
            End Set
        End Property


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
        ''' Gets or sets the MB-Id.
        ''' </summary>
        ''' <value>The mb id.</value>
        Property MbId() As Guid
            Get
                Return m_mbId
            End Get
            Set(ByVal value As Guid)
                m_mbId = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the artist name.
        ''' </summary>
        ''' <value>The name.</value>
        Property Name() As String
            Get
                Return m_artist
            End Get
            Set(ByVal value As String)
                m_artist = value
            End Set
        End Property
        ''' <summary>
        ''' Initializes a new instance of the <see cref="ArtistInfo" /> class.
        ''' </summary>
        Sub New()
            MyBase.new()
        End Sub
        Sub New(ByVal name As String)
            Me.New()
            m_artist = name
        End Sub
        Private Sub New(ByVal elem As Xml.XmlElement)
            With Me
                Dim elemvalue As String = ""

                .Name = Util.GetSubElementValue(elem, "name")
                'little hack
                If elem.HasAttribute("artist") Then .Name = elem.GetAttribute("artist")
                Util.GetGuid(elem.GetAttribute("mbid"))
                If Not Me.HasMbId Then m_mbId = Util.GetGuid(Util.GetSubElementValue(elem, "mbid"))
                If elem.HasAttribute("mbid") And Not Me.HasMbId Then
                    m_mbId = Util.GetGuid(Util.GetAttrValue(elem, "mbid"))
                End If

                .Url = Util.GetUrl(Util.GetSubElementValue(elem, "url"))

                'luckily we've got xpath^^

                .SetImagesByXmlElem(elem)
                .IsStreamable = If(Util.GetSubElementValue(elem, "streamable") = "1", True, False)
                'little hack
                If elem.HasAttribute("streamable") Then .IsStreamable = elem.GetAttribute("streamable")
                Dim similarartists As Xml.XmlNodeList = elem.SelectNodes("similar/artist")
                If similarartists.Count = 0 Then
                    'for artist.getSimilar
                    similarartists = elem.SelectNodes("./artist")
                End If
                If similarartists.Count > 0 Then
                    .SimilarArtists = New Dictionary(Of String, ArtistInfo)
                    For Each a As Xml.XmlElement In similarartists
                        Dim info As ArtistInfo = ArtistInfo.FromXmlElement(a)
                        .SimilarArtists.Add(info.Name, info)
                    Next
                End If

                .NumListeners = If(Util.GetSubElementValue(elem, "./stats/listeners") Is Nothing, -1, CInt(Util.GetSubElementValue(elem, "./stats/listeners")))
                .PlayCount = If(Util.GetSubElementValue(elem, "./stats/plays") Is Nothing, -1, CInt(Util.GetSubElementValue(elem, "./stats/plays")))
                Dim tags As Xml.XmlNodeList = elem.SelectNodes("./toptags/tag")
                If tags.Count > 0 Then
                    Dim tagRes As New Dictionary(Of String, Uri)
                    For Each t As Xml.XmlElement In tags
                        Dim name As String = Util.GetSubElementValue(t, "name")
                        Dim urlstr As String = Util.GetSubElementValue(t, "url")
                        Dim u As Uri = Nothing
                        If Uri.IsWellFormedUriString(urlstr, UriKind.Absolute) Then
                            u = New Uri(urlstr)
                        End If
                        tagRes.Add(name, u)
                    Next
                    .Tags = tagRes
                End If
                Dim bioNode As Xml.XmlElement = elem.SelectSingleNode("bio")
                DateTime.TryParse(Util.GetSubElementValue(bioNode, "published"), .BioPushlishDate)
                .BioSummary = Util.GetSubElementValue(bioNode, "summary")
                If Not String.IsNullOrEmpty(.BioSummary) Then .BioSummary = Uri.UnescapeDataString(.BioSummary)
                .BioText = Util.GetSubElementValue(bioNode, "content")
                If Not String.IsNullOrEmpty(.BioText) Then .BioText = Uri.UnescapeDataString(.BioText)
            End With
        End Sub
        ''' <summary>
        ''' Creates a string for debugging.
        ''' </summary>
        ''' <returns>
        ''' If successful, a String is returned, else <c>null</c>
        ''' </returns>
        Public Function ToDebugString() As String

            Dim result As String = ""
            result &= "Artist: " & m_artist & vbCrLf

            result &= "Url: " & Util.GetUrlstrOrNothing(Me.Url) & vbCrLf
            result &= "Image url: " & Util.GetUrlstrOrNothing(Me.ImageMedium) & vbCrLf
            Dim tagstr As String = ""
            If Tags IsNot Nothing Then
                For Each kv As KeyValuePair(Of String, Uri) In Tags
                    tagstr &= kv.Key & ","
                Next
                tagstr &= tagstr.Substring(0, tagstr.Length - 1)
                result &= "Tags: " & tagstr & vbCrLf
            End If
            result &= "MB-id: " & If(Not IsNothing(MbId), MbId.ToString, "") & vbCrLf
            If SimilarArtists IsNot Nothing Then
                Dim similar As String = ""
                For Each kv As KeyValuePair(Of String, ArtistInfo) In SimilarArtists
                    similar &= kv.Key & ","
                Next
                similar = similar.Substring(0, similar.Length - 1)
                result &= "Similar artists: " & similar & vbCrLf
            End If
            Return result

        End Function
        ''' <summary>
        ''' Creates an instance of ArtistInfo from an XmlElement's content
        ''' </summary>
        ''' <param name="elem">The elem.</param>
        ''' <returns>
        ''' If successful, a ArtistInfo is returned, else <c>null</c>
        ''' </returns>
        Shared Function FromXmlElement(ByVal elem As Xml.XmlElement) As ArtistInfo
            Dim m_result As New ArtistInfo(elem)

            Return m_result
        End Function


    End Class
End Namespace