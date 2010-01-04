Namespace API20
    Public MustInherit Class Settings
        Private Shared m_baseHost As Uri = New Uri("http://ws.audioscrobbler.com")
        Private Shared m_basePath As String = "/2.0"
        Private Shared m_langCode As String = Defaults.kLanguageCode
        'Maybe change this later, this api key is for LastFmLib....
        Private Shared m_apiKey As MD5Hash = Nothing
        Private Shared m_secret As MD5Hash = Nothing
        Private Shared m_timeout As Integer = Defaults.kRequestTimeoutInMs
        Private Shared m_authData As New Types.AuthData(m_apiKey, m_secret)

        Shared Property AuthData() As API20.Types.AuthData
            Get
                Return m_authData
            End Get
            Set(ByVal value As API20.Types.AuthData)
                m_authData = value
            End Set
        End Property
        Shared Property DefaultRequestTimeout() As Integer
            Get
                Return m_timeout
            End Get
            Set(ByVal value As Integer)
                m_timeout = value
            End Set
        End Property
        Shared Property BaseHost() As Uri
            Get
                Return m_baseHost
            End Get
            Set(ByVal value As Uri)
                m_baseHost = value
            End Set
        End Property

        Shared Property BasePath() As String
            Get
                Return m_basePath
            End Get
            Set(ByVal value As String)
                m_basePath = value
            End Set
        End Property

        Shared ReadOnly Property BaseUrl() As Uri
            Get
                Return New Uri(m_baseHost, m_basePath)
            End Get
        End Property


        Shared Property DefaultLangCode() As String
            Get
                Return m_langCode
            End Get
            Set(ByVal value As String)
                m_langCode = value
            End Set
        End Property
    End Class
End Namespace