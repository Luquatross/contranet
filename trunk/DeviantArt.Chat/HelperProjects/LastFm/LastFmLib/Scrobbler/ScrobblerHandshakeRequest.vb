Namespace Scrobbler

    ''' <summary>
    ''' This is for handshaking with the Audioscrobbler submission system-
    ''' </summary>
    Public Class ScrobblerHandshakeRequest
        Inherits ScrobblerHttp
        Dim m_init As New ScrobblerInit
        Dim m_error As ScrobblerError = ScrobblerError.NoError
        Dim m_responseString As String
        Dim m_ScrobblerSessionId As String
        Dim m_nowPlayingUrl As Uri
        Dim m_submissionUrl As Uri

        Dim m_clientID As String = Defaults.kClientID
        ''' <summary>
        ''' Gets the response string.
        ''' </summary>
        ''' <value>The response string.</value>
        ReadOnly Property ResponseString() As String
            Get
                Return m_responseString
            End Get
        End Property
        ''' <summary>
        ''' Gets the scrobbler session id.
        ''' </summary>
        ''' <value>The scrobbler session id.</value>
        ReadOnly Property ScrobblerSessionId() As String
            Get
                Return m_ScrobblerSessionId
            End Get
        End Property
        ''' <summary>
        ''' Gets the now playing URL.
        ''' </summary>
        ''' <value>The now playing URL.</value>
        ReadOnly Property NowPlayingUrl() As Uri
            Get
                Return m_nowPlayingUrl
            End Get
        End Property
        ''' <summary>
        ''' Gets the submission URL.
        ''' </summary>
        ''' <value>The submission URL.</value>
        ReadOnly Property SubmissionUrl() As Uri
            Get
                Return m_submissionUrl
            End Get
        End Property
        ''' <summary>
        ''' Gets the error code.
        ''' </summary>
        ''' <value>The error code.</value>
        ReadOnly Property ErrorCode() As ScrobblerError
            Get
                Return m_error
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the client id. Default is Defaults.kClientID
        ''' </summary>
        ''' <value>The client id.</value>
        Property ClientId() As String
            Get
                Return m_clientID
            End Get
            Set(ByVal value As String)
                m_clientID = value
            End Set
        End Property
        Sub New()

        End Sub
        ''' <summary>
        ''' Requests this instance.
        ''' </summary>
        Private Sub request()
            'MyBase.request()
            Dim timestamp As String = CStr((New UnixTime).get)
            Dim authtoken As String = CovertStringEncoding(MD5Digest(m_init.user.PasswordMD5 & timestamp), System.Text.Encoding.ASCII, System.Text.Encoding.UTF8)
            Dim query_String As String = "?hs=true" & "&p=1.2" & "&c=" & m_clientID & "&v=" & m_init.Version & _
                                        "&u=" & Uri.EscapeDataString(Uri.EscapeDataString((m_init.User.Username))) & "&t=" & timestamp & "&a=" & authtoken.ToLower
            Me.get(New Uri("http://" & Me.host & "/" & query_String))
        End Sub
        ''' <summary>
        ''' Does the Handshake
        ''' </summary>
        ''' <param name="init">The initial structure.</param>
        Public Overridable Sub request(ByRef init As ScrobblerInit)
            m_init = init
            request()
        End Sub
        ''' <summary>
        ''' Is called if the request was successful.
        ''' </summary>
        ''' <param name="data">The data.</param>
        Protected Overrides Sub success(ByVal data As String)
            'MyBase.success(data)
            Dim lines() As String = data.Split(vbLf)
            Dim code As String = lines(0)
            m_responseString = lines(0).Trim
            If code = "OK" And lines.Length >= 4 Then
                m_error = ScrobblerError.NoError
                m_ScrobblerSessionId = lines(1)
                m_nowPlayingUrl = New Uri(lines(2))
                m_submissionUrl = New Uri(lines(3))
                resetRetryTimer()
            ElseIf code = "BANNED" Then
                m_error = ScrobblerError.BannedClient
            ElseIf code = "BADAUTH" Then
                m_error = ScrobblerError.BadAuthorisation
            ElseIf code = "BADTIME" Then
                m_error = ScrobblerError.BadTime

            End If
        End Sub
    End Class
End Namespace
