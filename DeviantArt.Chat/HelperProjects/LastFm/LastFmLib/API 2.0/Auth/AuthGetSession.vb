Namespace API20.Auth
    ''' <summary>
    ''' Fetch a session key for a user. 
    ''' The third step in the authentication process. See the authentication how-to for more information. 
    ''' </summary>
    Public Class AuthGetSession
        Inherits Base.BaseRequest

        ReadOnly Property Result() As Types.Session
            Get
                Return m_AuthData.Session
            End Get
        End Property

        Public Overrides ReadOnly Property RequiresAuth() As Boolean
            Get
                Return True
            End Get
        End Property

        Sub New(ByVal token As MD5Hash)
            MyBase.New(RequestType.AuthGetSession)
            m_AuthData.Token = token
        End Sub
        Public Overrides Sub Start()
            SetAddParamValue("token", m_AuthData.Token.ToString)
            If m_AuthData.Token.AutoRefresh Then
                'Checks if token timed out
                If Not m_AuthData.CheckToken() = True Then
                    ' Return Nothing
                End If
            End If
            'SetAddParamValue("api_sig", GetMethodSignatureHash(m_AuthData.Token.AutoRefresh))
            MyBase.Start()
        End Sub
        Protected Overrides Sub Success(ByVal elem As System.Xml.XmlElement)
            Dim s As New Types.Session
            Dim sessionNode As Xml.XmlElement = elem.SelectSingleNode("session")
            Dim username As String = elem.SelectSingleNode("name").InnerText
            Dim key As String = elem.SelectSingleNode("key").InnerText
            Dim subscriber As Integer = elem.SelectSingleNode("subscriber").InnerText
            s.Username = username
            s.SessionKey = New MD5Hash(key, True)
            s.IsSubscriber = If(CInt(subscriber) <> 0, True, False)
            m_AuthData.Session = s
        End Sub
    End Class
End Namespace
