Namespace Scrobbler
    ''' <summary>
    ''' Class needed for initializing a Scrobbler(Manager/-Cache).
    ''' </summary>
    Public Class ScrobblerInit
        Dim m_user As TypeClasses.LastFmUser
        Dim m_client_version As String = Defaults.kClientVersion
        ''' <summary>
        ''' Initializes a new instance of the <see cref="ScrobblerInit" /> class.
        ''' </summary>
        Sub New()

        End Sub
        ''' <summary>
        ''' Initializes a new instance of the <see cref="ScrobblerInit" /> class.
        ''' </summary>
        ''' <param name="theuser">The LastFmUser-instance.</param>
        ''' <param name="versionString">The version string. If unset, this will be set to My.Application.Info.Version.ToString</param>
        Sub New(ByVal theuser As TypeClasses.LastFmUser, Optional ByVal versionString As String = Nothing)
            Me.New()
            m_user = theuser
            If String.IsNullOrEmpty(m_client_version) Then m_client_version = My.Application.Info.Version.ToString
        End Sub

        Property User() As TypeClasses.LastFmUser
            Get
                Return m_user
            End Get
            Set(ByVal value As TypeClasses.LastFmUser)
                m_user = value
            End Set
        End Property
        Property Version() As String
            Get
                Return m_client_version
            End Get
            Set(ByVal value As String)
                m_client_version = value
            End Set
        End Property
    End Class
    Public Module Structs

        ''' <summary>
        ''' The scrobbler Status
        ''' </summary>
        Public Enum ScrobblerStatus
            ''' <summary>
            ''' Connecting.
            ''' </summary>
            Connecting
            ''' <summary>
            ''' Handshake was made.
            ''' </summary>
            Handshaken
            ''' <summary>
            ''' Scrobbling
            ''' </summary>
            Scrobbling
            ''' <summary>
            ''' Tracks were scrobbled.
            ''' </summary>
            TracksScrobbled
            ''' <summary>
            ''' Tracks weren't scrobbled
            ''' </summary>
            TracksNotScrobbled
            ''' <summary>
            ''' Dummy var for ScrobblerError enum
            ''' </summary>
            StatusMax
        End Enum

        ''' <summary>
        ''' Enum for types of scrobbler errors. Min value is ScrobblerStatus.StatusMax.
        ''' </summary>
        Public Enum ScrobblerError

            ''' <summary>
            ''' Bad session ID
            ''' </summary>
            BadSession = ScrobblerStatus.StatusMax
            ''' <summary>
            ''' Client is banned.
            ''' </summary>
            BannedClient
            ''' <summary>
            ''' Bad Authorisation.
            ''' </summary>
            BadAuthorisation
            ''' <summary>
            ''' Timstamp wrong.
            ''' </summary>
            BadTime

            ''' <summary>
            ''' Scrobbler is not initialized
            ''' </summary>
            NotInitialized
            ''' <summary>
            ''' No error.
            ''' </summary>
            NoError
        End Enum
    End Module
End Namespace
