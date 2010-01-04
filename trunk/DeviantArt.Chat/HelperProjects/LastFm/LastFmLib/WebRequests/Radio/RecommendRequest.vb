Namespace WebRequests
    ''' <summary>
    ''' Recommends something to a user, e.g a track or a station
    ''' </summary>
    Public Class RecommendRequest
        Inherits RequestBase
        Dim m_target_username As String
        Dim m_User As TypeClasses.LastFmUser
        Dim m_message As String
        Dim m_artist As String
        Dim m_album As String
        Dim m_track As String
        Dim m_token As String
        Dim m_type As ItemType = ItemType.ItemTrack
        Dim m_language As String = Defaults.kLanguageCode
        Enum ItemType
            ItemArtist = 1
            ItemTrack
            ItemAlbum
            'ItemTag
            'ItemUser
            'ItemStation
            'ItemUnknown
        End Enum
        Property Message() As String
            Get
                Return m_message
            End Get
            Set(ByVal value As String)
                m_message = value
            End Set
        End Property
        Property RecommendationType() As ItemType
            Get
                Return m_type
            End Get
            Set(ByVal value As ItemType)
                m_type = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the language code. Default is Defaults.kLanguageCode
        ''' </summary>
        ''' <value>The language code.</value>
        Property LanguageCode() As String
            Get
                Return m_language
            End Get
            Set(ByVal value As String)
                m_language = value
            End Set
        End Property
        Property User() As TypeClasses.LastFmUser
            Get
                Return m_user
            End Get
            Set(ByVal value As TypeClasses.LastFmUser)
                m_user = value
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
        ''' Gets or sets the album.
        ''' </summary>
        ''' <value>The album.</value>
        Property Album() As String
            Get
                Return m_album
            End Get
            Set(ByVal value As String)
                m_album = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the track.
        ''' </summary>
        ''' <value>The track.</value>
        Property Track() As String
            Get
                Return m_track
            End Get
            Set(ByVal value As String)
                m_track = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the target username.
        ''' </summary>
        ''' <value>The target username.</value>
        Property TargetUsername() As String
            Get
                Return m_target_username
            End Get
            Set(ByVal value As String)
                m_target_username = value
            End Set
        End Property
        ''' <summary>
        ''' Gets the token.
        ''' </summary>
        ''' <value>The token.</value>
        ReadOnly Property Token() As String
            Get
                Return m_token
            End Get
        End Property
        ''' <summary>
        ''' Initializes a new instance of the <see cref="RecommendRequest" /> class.
        ''' </summary>
        Sub New()
            MyBase.New(WebRequests.RequestType.Recommend, "Recommend")
        End Sub
        ''' <summary>
        ''' Initializes a new instance of the <see cref="RecommendRequest" /> class.
        ''' </summary>
        '''<param name="u">The user.</param>
        ''' <param name="sTargetUser">The target user.</param>
        ''' <param name="ti">The track information.</param>
        ''' <param name="msg">The message for the recommendation</param>
        ''' <param name="RecommendType">Optional. Type of the recommendation(Artist-,Album-,Track-). Default is track.</param>
        ''' <param name="lang">Optional. The langauge string. Leave empty for setting this to Defaults.kLanguageCode </param>
        Sub New(ByVal u As TypeClasses.LastFmUser, ByVal sTargetUser As String, ByVal ti As TypeClasses.TrackInfo, Optional ByVal RecommendType As ItemType = ItemType.ItemTrack, Optional ByVal msg As String = "", Optional ByVal lang As String = "")
            Me.New()

            If Not String.IsNullOrEmpty(lang) Then m_language = lang
            m_User = u
            m_target_username = sTargetUser

            m_type = RecommendType

            m_artist = ti.ArtistName
            m_album = ti.Album

            Select Case m_type
                Case ItemType.ItemArtist
                    m_token = ""
                Case ItemType.ItemAlbum
                    m_token = m_album
                Case ItemType.ItemTrack
                    m_token = m_track
            End Select

            m_message = msg
        End Sub

        Public Overrides Sub Start()
            Dim it As ItemType = m_type
            Dim xml_rpc As New XMLRPC()
            Dim challenge As String = (New UnixTime).get

            With xml_rpc
                .addParameter(m_User.Username)
                .addParameter(challenge)
                .addParameter(GetRequestAuthCode(m_User.PasswordMD5, challenge))
                .addParameter(m_artist)
                .Method = "recommendItem"
            End With
            Select Case it
                Case ItemType.ItemArtist
                    xml_rpc.addParameter("")
                    xml_rpc.addParameter("artist")
                Case ItemType.ItemAlbum
                    xml_rpc.addParameter(m_token)
                    xml_rpc.addParameter("album")
                Case ItemType.ItemTrack
                    m_track = m_token
                    xml_rpc.addParameter(m_token)
                    xml_rpc.addParameter("track")
            End Select
            With xml_rpc
                .addParameter(m_target_username)
                .addParameter(m_message)
                .addParameter(m_language)
            End With
            Request(xml_rpc)
        End Sub
    End Class
End Namespace
