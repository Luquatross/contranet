Namespace API20.Types
    ''' <summary>
    ''' Stores session data. Keep the session key!
    ''' </summary>
    Public Class Session
        Protected m_username As String
        Protected m_sessionKey As MD5Hash
        Protected m_isSubscriber As Boolean

        Public Property Username() As String
            Get
                Return m_username
            End Get
            Set(ByVal value As String)
                m_username = value
            End Set
        End Property
        Public Property SessionKey() As MD5Hash
            Get
                Return m_sessionKey
            End Get
            Set(ByVal value As MD5Hash)
                m_sessionKey = value
            End Set
        End Property
        Property IsSubscriber() As Boolean
            Get
                Return m_isSubscriber
            End Get
            Set(ByVal value As Boolean)
                m_isSubscriber = value
            End Set
        End Property
    End Class
End Namespace