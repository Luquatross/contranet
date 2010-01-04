Namespace API20.Album
    ''' <summary>
    ''' Get the metadata for an album on Last.fm using the album name or a musicbrainz id. 
    ''' See playlist.fetch on how to get the album playlist. 
    ''' </summary>
    ''' <remarks></remarks>
    Public Class AlbumGetInfo
        Inherits Base.BaseRequest

        Dim m_artist As String
        Dim m_album As String
        Dim m_mbId As Guid
        Dim m_result As Types.AlbumInfo
        Property MbId() As Guid
            Get
                Return m_mbId
            End Get
            Set(ByVal value As Guid)
                m_mbId = value
            End Set
        End Property
        Property Album() As String
            Get
                Return m_album
            End Get
            Set(ByVal value As String)

            End Set
        End Property
        Property Artist() As String
            Get
                Return m_artist
            End Get
            Set(ByVal value As String)
                m_artist = value
            End Set
        End Property
        ReadOnly Property Result() As Types.AlbumInfo
            Get
                Return m_result
            End Get
        End Property
        Private Sub New()
            MyBase.New(RequestType.AlbumGetInfo)
        End Sub
        Sub New(ByVal artist As String, ByVal album As String)
            Me.New()
            m_artist = artist
            m_album = album
        End Sub
        Sub New(ByVal mbid As Guid)
            Me.New()
            m_mbId = mbid
        End Sub
        Sub New(ByVal mbid As String)
            Me.New(New Guid(mbid))
        End Sub
        Public Overrides Sub Start()
            If Not String.IsNullOrEmpty(Artist) Then SetAddParamValue("artist", m_artist)
            If Not String.IsNullOrEmpty(Album) Then SetAddParamValue("album", m_album)
            If MbId <> Guid.Empty Then SetAddParamValue("mbid", MbId.ToString)

            MyBase.Start()
        End Sub
        Protected Overrides Sub Success(ByVal elem As System.Xml.XmlElement)
            m_result = Types.AlbumInfo.FromXmlElement(elem)
        End Sub
    End Class
End Namespace