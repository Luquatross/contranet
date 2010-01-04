Namespace WebRequests
    ''' <summary>
    ''' Class for Action requests. Also used by deriving classes e.g. to love/ban a track
    ''' </summary>
    ''' <remarks>Propertys you have to set: Username, PasswordMD5</remarks>
    Public MustInherit Class ActionRequest
        Inherits RequestBase

        Private m_track As TypeClasses.Track
        Private m_user As TypeClasses.LastFmUser
        Private m_method As String

        Property User() As TypeClasses.LastFmUser
            Get
                Return m_user
            End Get
            Set(ByVal value As TypeClasses.LastFmUser)
                m_user = value
            End Set
        End Property
        Property Track() As TypeClasses.Track
            Get
                Return m_track
            End Get
            Set(ByVal value As TypeClasses.Track)
                m_track = value
            End Set
        End Property


        ''' <summary>
        ''' Initializes a new instance of the <see cref="ActionRequest" /> class.
        ''' </summary>
        ''' <param name="method">The method string.</param>
        ''' <param name="t">The type of Request</param>
        Protected Sub New(ByRef method As String, ByVal t As WebRequests.RequestType)
            MyBase.New(t, method)
            m_method = method
        End Sub
        ''' <summary>
        ''' Initializes a new instance of the <see cref="ActionRequest" /> class.
        ''' </summary>
        ''' <param name="method">The method.</param>
        ''' <param name="rqt">The request type.</param>
        ''' <param name="u">The instance of the user class</param>
        Protected Sub New(ByRef method As String, ByVal rqt As WebRequests.RequestType, ByVal u As TypeClasses.LastFmUser)
            Me.New(method, rqt)
            m_user = u
        End Sub

        ''' <summary>
        ''' Initializes a new instance of the <see cref="ActionRequest" /> class.
        ''' </summary>
        ''' <param name="u">The instance of the user class</param>
        ''' <param name="track">The track for which the ActionRequest should be done</param>
        Protected Sub New(ByRef method As String, ByVal rqt As WebRequests.RequestType, ByVal u As TypeClasses.LastFmUser, ByVal track As TypeClasses.Track)
            Me.New(method, rqt, u)
            m_track = track
        End Sub
        ''' <summary>
        ''' Starts the Request. Please set the username and password property before starting, else the request will fail!
        ''' </summary>
        Public Overrides Sub Start()
            'MyBase.Start()
            Dim xmlrpc As New XMLRPC
            xmlrpc.Method = m_method
            Dim challenge As String = CInt(UnixTime.GetUnixTime)
            'Not like in the Original, the Username must be set as a Property for success
            With xmlrpc
                .addParameter(xmlrpc.Escape(m_user.Username))
                .addParameter(challenge)
                .addParameter(System.Text.Encoding.UTF8.GetString(System.Text.Encoding.ASCII.GetBytes(GenerateHash(m_user.PasswordMD5 + challenge))).ToLower)
                .addParameter(xmlrpc.Escape(m_track.ArtistName))
                .addParameter(xmlrpc.Escape(m_track.Title))
            End With
            Request(xmlrpc)
        End Sub
        Public Overrides Sub TryAgain()

        End Sub
    End Class
    ''' <summary>
    ''' Removes a track from the "Recently listened tracks"-list
    ''' </summary>
    Public Class UnListenRequest

        Inherits ActionRequest
        Public Sub New(ByVal track As TypeClasses.Track, Optional ByVal u As TypeClasses.LastFmUser = Nothing)
            MyBase.new("removeRecentlyListenedTrack", WebRequests.RequestType.UnListen, u)
            Me.Track = track
        End Sub
        Public Sub New(ByVal track As TypeClasses.TrackInfo, Optional ByVal u As TypeClasses.LastFmUser = Nothing)
            MyBase.new("removeRecentlyListenedTrack", WebRequests.RequestType.UnListen, u)
            Me.Track = track
        End Sub
    End Class
    ''' <summary>
    ''' Loves a track
    ''' </summary>
    Public Class LoveRequest

        Inherits ActionRequest
        Public Sub New(ByVal track As TypeClasses.Track, Optional ByVal u As TypeClasses.LastFmUser = Nothing)
            MyBase.new("loveTrack", WebRequests.RequestType.LoveTrack, u)
            Me.Track = track
        End Sub
        Public Sub New(ByVal track As TypeClasses.TrackInfo, Optional ByVal u As TypeClasses.LastFmUser = Nothing)
            MyBase.new("loveTrack", WebRequests.RequestType.LoveTrack, u)
            Me.Track = track
        End Sub
    End Class
    ''' <summary>
    ''' Removes the "love" of a track.
    ''' </summary>
    Public Class UnLoveRequest

        Inherits ActionRequest
        Public Sub New(ByVal track As TypeClasses.Track, Optional ByVal u As TypeClasses.LastFmUser = Nothing)
            MyBase.new("unLoveTrack", WebRequests.RequestType.UnLoveTrack, u)
            Me.Track = track
        End Sub
        Public Sub New(ByVal track As TypeClasses.TrackInfo, Optional ByVal u As TypeClasses.LastFmUser = Nothing)
            MyBase.new("unLoveTrack", WebRequests.RequestType.UnLoveTrack, u)
            Me.Track = track
        End Sub
    End Class

    ''' <summary>
    ''' Bans a track
    ''' </summary>
    Public Class BanRequest
        Inherits ActionRequest
        Public Sub New(ByVal track As TypeClasses.Track, Optional ByVal u As TypeClasses.LastFmUser = Nothing)
            MyBase.new("banTrack", WebRequests.RequestType.BanTrack, u)
            Me.Track = track
        End Sub
        Public Sub New(ByVal track As TypeClasses.TrackInfo, Optional ByVal u As TypeClasses.LastFmUser = Nothing)
            MyBase.new("banTrack", WebRequests.RequestType.BanTrack, u)
            Me.Track = track
        End Sub
    End Class
    ''' <summary>
    ''' Removes a previously banned track from the "banned tracks"-list
    ''' </summary>
    Public Class unBanRequest

        Inherits ActionRequest
        ''' <summary>
        ''' Initializes a new instance of the <see cref="unBanRequest" /> class.
        ''' </summary>
        ''' <param name="track">The track.</param>
        ''' <param name="u">The u.</param>
        Public Sub New(ByVal track As TypeClasses.Track, Optional ByVal u As TypeClasses.LastFmUser = Nothing)
            MyBase.new("unBanTrack", WebRequests.RequestType.UnBanTrack, u)
            Me.Track = track
        End Sub
        ''' <summary>
        ''' Initializes a new instance of the <see cref="unBanRequest" /> class.
        ''' </summary>
        ''' <param name="track">The track.</param>
        ''' <param name="u">The u.</param>
        Public Sub New(ByVal track As TypeClasses.TrackInfo, Optional ByVal u As TypeClasses.LastFmUser = Nothing)
            MyBase.new("unBanTrack", WebRequests.RequestType.UnBanTrack, u)
            Me.Track = track
        End Sub
    End Class
    ''' <summary>
    ''' Adds a track to the user's personal playlist.
    ''' </summary>
    ''' <remarks>A "removeTrackFromUserPlaylist" would be really nice to have.</remarks>
    Public Class addTrackToUserPlaylistRequest
        Inherits ActionRequest
        Public Sub New(ByVal track As TypeClasses.Track, Optional ByVal u As TypeClasses.LastFmUser = Nothing)
            MyBase.new("addTrackToUserPlaylist", RequestType.AddTrackToMyPlaylist, u)
            Me.Track = track
        End Sub
        Public Sub New(ByVal track As TypeClasses.TrackInfo, Optional ByVal u As TypeClasses.LastFmUser = Nothing)
            MyBase.new("addTrackToUserPlaylist", RequestType.AddTrackToMyPlaylist, u)
            Me.Track = track

        End Sub
    End Class
End Namespace
