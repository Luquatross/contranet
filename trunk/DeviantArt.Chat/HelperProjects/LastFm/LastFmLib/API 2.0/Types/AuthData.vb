Namespace API20.Types
    ''' <summary>
    ''' Class for storing auth data, like api key, token, secret and session
    ''' </summary>
    Public Class AuthData
        Dim m_key As New MD5Hash()
        Dim m_token As New AuthToken
        Dim m_secret As New MD5Hash
        Dim m_session As Session
        ReadOnly Property HasSession() As Boolean
            Get
                Return Session IsNot Nothing
            End Get
        End Property
        ReadOnly Property HasApiKey() As Boolean
            Get
                Return ApiKey IsNot Nothing
            End Get
        End Property
        ReadOnly Property HasToken() As Boolean
            Get
                Return Token IsNot Nothing
            End Get
        End Property
        ReadOnly Property HasSecret() As Boolean
            Get
                Return ApiSecret IsNot Nothing
            End Get
        End Property
        Property Session() As Session
            Get
                Return m_session
            End Get
            Set(ByVal value As Session)
                m_session = value
            End Set
        End Property
        Property ApiKey() As MD5Hash
            Get
                Return m_key
            End Get
            Set(ByVal value As MD5Hash)

            End Set
        End Property
        Property Token() As AuthToken
            Get
                Return m_token
            End Get
            Set(ByVal value As AuthToken)
                m_token = value
            End Set
        End Property
        Property ApiSecret() As MD5Hash
            Get
                Return m_secret
            End Get
            Set(ByVal value As MD5Hash)
                m_secret = value
            End Set
        End Property
        Sub New()

        End Sub
        Sub New(ByVal api_key As MD5Hash, Optional ByVal secret As MD5Hash = Nothing, Optional ByVal token As MD5Hash = Nothing)
            SetByHashes(api_key, token, secret)
        End Sub

        Private Sub SetByHashes(ByVal api_key As MD5Hash, ByVal token As MD5Hash, ByVal secret As MD5Hash)
            m_key = api_key
            m_token = token
            m_secret = secret
        End Sub

        ''' <summary>
        ''' Checks the token and updates it if it's outdated.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function CheckToken() As Boolean
            If m_token Is Nothing Then
                m_token = New AuthToken
                m_token.RequestToken(m_key)
            End If
            If m_token.IsOutdated Then
                If Not m_token.RequestToken(m_key) Then
                    Return False
                End If
            End If
            Return True
        End Function
        Public Sub AskUserToGrantPermissions()
            CheckToken()
            System.Diagnostics.Process.Start("http://last.fm/api/auth?api_key=" & m_key.ToString & "&token=" & m_token.ToString)
        End Sub
    End Class
End Namespace
