Namespace WebRequests
    ''' <summary>
    ''' Gets the tags for a track of a user
    ''' </summary>
    Class UserTrackTagsRequest
        Inherits UserArtistTagsRequest
        Dim m_track As String
        ''' <summary>
        ''' Gets or sets the track name.
        ''' </summary>
        ''' <value>The track name.</value>
        Property TrackName()
            Get
                Return m_track
            End Get
            Set(ByVal value)
                m_track = value
            End Set

        End Property
        ''' <summary>
        ''' Gets the request path(relative to the user's url).
        ''' </summary>
        ''' <value>The path.</value>
        Public Overrides ReadOnly Property path() As String
            Get
                Return "/tracktags.xml?artist=" & RequestBase.EscapeUriData(Artist()) & _
                        "&track=" & RequestBase.EscapeUriData(m_track)
            End Get
        End Property
        ''' <summary>
        ''' Initializes a new instance of the <see cref="UserTrackTagsRequest" /> class.
        ''' </summary>
        Sub New()
            MyBase.New(RequestType.UserTrackTags, "UserTrackTags")
        End Sub
        ''' <summary>
        ''' Initializes a new instance of the <see cref="UserTrackTagsRequest" /> class.
        ''' </summary>
        ''' <param name="sUsername">The s username.</param>
        ''' <param name="sArtist">The s artist.</param>
        ''' <param name="sTrackName">Name of the s track.</param>
        Sub New(ByVal sUsername As String, ByVal sArtist As String, ByVal sTrackName As String)
            Me.New()
            m_username = sUsername
            Me.Artist = sArtist
            m_track = sTrackName
        End Sub
    End Class
End Namespace

