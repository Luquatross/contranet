Namespace WebRequests

    ''' <summary>
    ''' Requestst the user's tags for an artist
    ''' </summary>
    Class UserArtistTagsRequest
        Inherits UserTagsRequest
        Dim m_Artist As String
        ''' <summary>
        ''' Gets or sets the artist.
        ''' </summary>
        ''' <value>The artist.</value>
        Property Artist()
            Get
                Return m_Artist
            End Get
            Set(ByVal value)
                m_Artist = value
            End Set
        End Property
        ''' <summary>
        ''' Gets the request path.
        ''' </summary>
        ''' <value>The path.</value>
        Public Overrides ReadOnly Property path() As String
            Get
                Return "/artisttags.xml?artist=" & EscapeUriData(Artist)
            End Get
        End Property
        ''' <summary>
        ''' Initializes a new instance of the <see cref="UserArtistTagsRequest" /> class.
        ''' </summary>
        Sub New()
            MyBase.New(RequestType.UserArtistTags, "UserArtistTags")
        End Sub
        ''' <summary>
        ''' Initializes a new instance of the <see cref="UserArtistTagsRequest" /> class.
        ''' </summary>
        ''' <param name="sUsername">The username.</param>
        ''' <param name="sArtist">The artist.</param>
        Sub New(ByVal sUsername As String, ByVal sArtist As String)
            Me.New()
            m_username = sUsername
            m_Artist = sArtist
        End Sub
        'for UserTrackTags request
        Protected Sub New(ByVal type As WebRequests.RequestType, ByRef name As String)
            MyBase.New(RequestType.ArtistMetaData, name)
        End Sub
    End Class
End Namespace
