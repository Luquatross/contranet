Namespace API20.Types
    ''' <summary>
    ''' Stores information of an album.
    ''' </summary>
    Public Class AlbumInfo
        Inherits Base.BaseImageData
        Dim m_albumName As String
        Dim m_artist As String
        Dim m_id As Integer
        Dim m_mbId As Guid
        Dim m_url As Uri
        Dim m_releaseDate As DateTime

        Dim m_numListeners As Integer
        Dim m_playCount As Integer
        Dim m_tags As Dictionary(Of String, Uri)

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
        ''' Gets or sets the release date.
        ''' </summary>
        ''' <value>The release date.</value>
        Property ReleaseDate() As DateTime
            Get
                Return m_releaseDate
            End Get
            Set(ByVal value As DateTime)
                m_releaseDate = value
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
        ''' Gets or sets the album id.
        ''' </summary>
        ''' <value>The id.</value>
        Property Id() As Integer
            Get
                Return m_id
            End Get
            Set(ByVal value As Integer)
                m_id = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the artist.
        ''' </summary>
        ''' <value>The artist.</value>
        Property Artist() As String
            Get
                Return m_artist
            End Get
            Set(ByVal value As String)
                m_artist = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the name of the album.
        ''' </summary>
        ''' <value>The name of the album.</value>
        Property AlbumName() As String
            Get
                Return m_albumName
            End Get
            Set(ByVal value As String)
                m_albumName = value
            End Set
        End Property
        ''' <summary>
        ''' Toes the debug string.
        ''' </summary>
        ''' <returns>
        ''' If successful, a String is returned, else <c>null</c>
        ''' </returns>
        Public Function ToDebugString() As String
            Dim result As String = ""
            result &= "Artist: " & Me.Artist & vbCrLf
            result &= "Album: " & Me.AlbumName & vbCrLf
            result &= "Release date: " & Me.ReleaseDate.ToString & vbCrLf
            result &= "Url: " & Util.GetUrlstrOrNothing(Me.Url) & vbCrLf
            result &= "Image url: " & Util.GetUrlstrOrNothing(Me.ImageMedium) & vbCrLf
            If Tags IsNot Nothing Then
                Dim tagstr As String = ""
                For Each kv As KeyValuePair(Of String, Uri) In Tags
                    tagstr &= kv.Key & ","
                Next
                tagstr &= tagstr.Substring(0, tagstr.Length - 1)
                result &= "Tags: " & tagstr & vbCrLf
            End If
            result &= "MB-id: " & If(Not IsNothing(MbId), MbId.ToString, "")
            Return result
        End Function
        ''' <summary>
        ''' Initializes a new instance of the <see cref="AlbumInfo" /> class.
        ''' </summary>
        Sub New()

        End Sub

        ''' <summary>
        ''' Initializes a new instance of the <see cref="AlbumInfo" /> class.
        ''' </summary>
        ''' <param name="elem">The elem.</param>
        Sub New(ByVal elem As Xml.XmlElement)
            Dim elemvalue As String = ""
            With Me
                .AlbumName = Util.GetSubElementValue(elem, "name")
                .Artist = Util.GetSubElementValue(elem, "artist")
                elemvalue = Util.GetSubElementValue(elem, "id")
                .Id = If(IsNothing(elemvalue), -1, CInt(elemvalue))
                .MbId = If(String.IsNullOrEmpty(Util.GetSubElementValue(elem, "mbid")), Nothing, New Guid(Util.GetSubElementValue(elem, "mbid")))
                elemvalue = Util.GetSubElementValue(elem, "releasedate")
                DateTime.TryParse(elemvalue, .ReleaseDate)
                Uri.TryCreate(Util.GetSubElementValue(elem, "url"), UriKind.RelativeOrAbsolute, .Url)
                'luckily we've got xpath^^

                .SetImagesByXmlElem(elem)
                .NumListeners = If(Util.GetSubElementValue(elem, "listeners") Is Nothing, -1, CInt(Util.GetSubElementValue(elem, "listeners")))
                .PlayCount = If(Util.GetSubElementValue(elem, "playcount") Is Nothing, -1, CInt(Util.GetSubElementValue(elem, "playcount")))
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
            End With
        End Sub
        ''' <summary>:
        ''' Froms the XML element.
        ''' </summary>
        ''' <param name="elem">The elem.</param>
        ''' <returns>
        ''' If successful, a AlbumInfo is returned, else <c>null</c>
        ''' </returns>
        Shared Function FromXmlElement(ByVal elem As Xml.XmlElement) As AlbumInfo
            Return New AlbumInfo(elem)
        End Function

    End Class
End Namespace