Imports System.Xml
Namespace API20.Auth
    ''' <summary>
    ''' Fetch an unathorized request token for an API account. 
    ''' This is step 2 of the authentication process for desktop applications. 
    ''' Web applications do not need to use this service. 
    ''' </summary>
    Public Class AuthGetToken
        Inherits Base.BaseRequest
        Dim m_token As Types.AuthToken

        ReadOnly Property Result() As Types.AuthToken
            Get
                Return m_token
            End Get
        End Property
        Sub New()
            MyBase.New(RequestType.AuthGetToken)
            'm_methodClass = "auth"
            'm_methodName = "gettoken"
        End Sub

        Sub New(ByVal api_key As MD5Hash)
            Me.New()
            m_AuthData.ApiKey = api_key

        End Sub
        Public Overrides Sub Start()

            MyBase.Start()

        End Sub
        Protected Overrides Sub Success(ByVal elem As System.Xml.XmlElement)

            If Not elem.IsEmpty Then
                Dim tokenMD5 As MD5Hash = New MD5Hash(elem.InnerText, True)
                m_token = New Types.AuthToken(tokenMD5)
            End If
        End Sub
    End Class
End Namespace
