Namespace WebRequests
    ''' <summary>
    ''' Requests the user's album tags
    ''' </summary>
    Class UserAlbumTagsRequest
        Inherits UserArtistTagsRequest
        Dim m_Album As String

        Property Album() As String
            Get
                Return m_Album
            End Get
            Set(ByVal value As String)
                m_Album = value
            End Set
        End Property
        Public Overrides ReadOnly Property path() As String
            Get
                Return "/albumtags.xml?artist=" & EscapeUriData(Artist()) & _
                        "&album=" & EscapeUriData(m_Album)
            End Get
        End Property
        ''' <summary>
        ''' Initializes a new instance of the <see cref="UserAlbumTagsRequest" /> class.
        ''' </summary>
        Sub New()
            MyBase.New(RequestType.UserAlbumTags, "UserAlbumTags")
        End Sub
        ''' <summary>
        ''' Initializes a new instance of the <see cref="UserAlbumTagsRequest" /> class.
        ''' </summary>
        ''' <param name="username">The username.</param>
        ''' <param name="album">The album name.</param>
        Sub New(ByVal username As String, ByVal artist As String, ByVal album As String)
            Me.New()

            Me.Username = username
            Me.Album = album
        End Sub
    End Class
End Namespace