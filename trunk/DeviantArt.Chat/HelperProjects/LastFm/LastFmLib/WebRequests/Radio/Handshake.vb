Namespace WebRequests
    ''' <summary>
    ''' This Request is for doing the handshake with Last.fm's server.
    ''' The request returns the basic user-specific information like whether he is a subscriber and a session Id which is needed for most other requests,
    ''' like the the GetXspfPlaylistRequest or all deriving classes of the ActionRequest class.
    ''' </summary>
    Public Class Handshake
        Inherits WebRequests.RequestBase

        Private m_user As New TypeClasses.LastFmUser
        Private m_version As String = Defaults.kClientVersion
        Private m_streamUrl As Uri
        Private m_isSubscriber As Boolean
        Private m_message As String
        Private m_platform As String = Defaults.kPlatform
        Private m_language As String = Defaults.kLanguageCode
        ''' <summary>
        ''' Gets or sets the language string. Default is Defaults.kLanguageCode.
        ''' </summary>
        ''' <value>The language.</value>
        Property Language() As String
            Get
                Return m_language
            End Get
            Set(ByVal value As String)
                m_language = value

            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the plaform string. See audioscrobbler doc for further information. Default is Defaults.kPlatform.
        ''' </summary>
        ''' <value>The plaform.</value>
        Property Plaform() As String
            Get
                Return m_platform
            End Get
            Set(ByVal value As String)
                m_platform = value
            End Set
        End Property
        ''' <summary>
        ''' If there is a message in the server response, you can obtain it here.
        ''' </summary>
        ''' <value>The message.</value>
        ReadOnly Property Message() As String
            Get
                Return m_message
            End Get
        End Property
        ''' <summary>
        ''' Gets a value indicating whether the user is a subscriber.
        ''' </summary>
        ''' <value>
        ''' <c>true</c> if the user is a subscriber; otherwise, <c>false</c>.
        ''' </value>
        ReadOnly Property IsSubscriber() As Boolean
            Get
                Return m_isSubscriber
            End Get
        End Property

        ''' <summary>
        ''' Gets the stream URL.
        ''' </summary>
        ''' <value>The stream URL.</value>
        ReadOnly Property StreamUrl() As Uri
            Get
                Return m_streamUrl
            End Get
        End Property


        ''' <summary>
        ''' Gets or sets the version string. Default is Defaults.kClientVersion.
        ''' </summary>
        ''' <value>The version.</value>
        Property ClientVersion() As String
            Get
                Return m_version
            End Get
            Set(ByVal value As String)
                m_version = value
            End Set
        End Property


        ''' <summary>
        ''' Gets the has message.
        ''' </summary>
        ''' <value>The has message.</value>
        ReadOnly Property hasMessage() As String
            Get
                Return Not String.IsNullOrEmpty(m_message)
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the user.
        ''' </summary>
        ''' <value>The user.</value>
        Property User() As TypeClasses.LastFmUser
            Get
                Return m_user
            End Get
            Set(ByVal value As TypeClasses.LastFmUser)
                m_user = value
            End Set
        End Property

        ''' <summary>
        ''' Initializes a new instance of the <see cref="Handshake" /> class.
        ''' </summary>
        Sub New()
            MyBase.New(WebRequests.RequestType.Handshake, "Handshake")
            m_isSubscriber = False
        End Sub
        ''' <summary>
        ''' Initializes a new instance of the <see cref="Handshake" /> class.
        ''' </summary>
        ''' <param name="u">The instance of the user class</param>
        Sub New(ByVal u As TypeClasses.LastFmUser)
            Me.New()
            m_user = u

        End Sub


        ''' <summary>
        ''' Starts the request and gets the response.
        ''' </summary>
        Public Overrides Sub Start()
            Dim path As String
            path = "/radio/handshake.php?version=" & m_version & "&platform=" & m_platform & "&username=" & EscapeUriData(m_user.Username) & _
                    "&passwordmd5=" & m_user.PasswordMD5 & "&language=" & m_language
            Me.get(path)

        End Sub
        Protected Overrides Sub success(ByVal data As String)
            Dim m_baseHostPriv As String
            
            m_user.SessionID = parameter("session", data)
            m_baseHostPriv = parameter("base_url", data)
            m_basePath = parameter("base_path", data)
            Dim sUrl As String = parameter("stream_url", data)
            m_streamUrl = GetUrl(sUrl)
            m_isSubscriber = parameter("subscriber", data) Is "1"
            m_message = parameter("info_message", data)
            Dim is_banned As Boolean = parameter("banned", data) Is "1"
            If String.IsNullOrEmpty(m_user.SessionID) Or String.IsNullOrEmpty(m_baseHostPriv) Or String.IsNullOrEmpty(m_basePath) _
                                               Or (m_streamUrl Is Nothing) Then
                setFailed(WebRequestResultCode.Handshake_SessionFailed, tr("Could not connect to server"))
            ElseIf is_banned Then
                setFailed(WebRequestResultCode.Handshake_Banned, tr("This client version is obsolete. Please update."))
            ElseIf m_user.SessionID.ToLower = "failed)" Then
                Dim msg As String = parameter("msg", data).ToLower
                If msg = "no such user" Or msg = "padd md5 not 32 len" Then
                    setFailed(WebRequestResultCode.Handshake_WrongUserNameOrPassword, tr("Could not connect to server. Wrong username or password."))
                Else
                    setFailed(WebRequestResultCode.Handshake_SessionFailed, tr("Could not connect to server."))
                End If
            ElseIf m_user.SessionID.Length <> 32 Then
                setFailed(WebRequestResultCode.Handshake_SessionFailed, tr("Radio handshake failed: session length not 32 bytes. Retrying."))
                TryAgain()

            Else
                Me.BaseHost = m_baseHostPriv

            End If
        End Sub

    End Class
End Namespace
