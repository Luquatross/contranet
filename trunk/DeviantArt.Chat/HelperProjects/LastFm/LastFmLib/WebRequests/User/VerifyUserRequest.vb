Namespace WebRequests
    ''' <summary>
    ''' Verifies a user
    ''' </summary>
    Public Class VerifyUserRequest
        Inherits RequestBase
        Private m_username As String
        Private m_passwordMD5 As String
        Private m_passwordMD5Lower As String
        Private m_bootStrapCode As BootStrapCodeEnum
        Private m_userAuthCode As UserAuthCodeEnum

        Enum BootStrapCodeEnum
            BOOTSTRAP_DENIED = 0
            BOOTSTRAP_ALLOWED
        End Enum
        Enum UserAuthCodeEnum
            AUTH_OK = 0
            AUTH_OK_LOWER
            AUTH_BADUSER
            AUTH_BADPASS
            AUTH_ERROR
        End Enum
        ''' <summary>
        ''' Gets the boot strap code.
        ''' </summary>
        ''' <value>The boot strap code.</value>
        ReadOnly Property BootStrapCode() As BootStrapCodeEnum
            Get
                Return m_bootStrapCode
            End Get
        End Property
        ''' <summary>
        ''' Gets the user auth code.
        ''' </summary>
        ''' <value>The user auth code.</value>
        ReadOnly Property UserAuthCode() As UserAuthCodeEnum
            Get
                Return m_userAuthCode
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the password Md5 lower.
        ''' </summary>
        ''' <value>The password M d5 lower.</value>
        Property PasswordMD5Lower() As String
            Get
                Return m_passwordMD5Lower
            End Get
            Set(ByVal value As String)
                m_passwordMD5Lower = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the password Md5.
        ''' </summary>
        ''' <value>The password M d5.</value>
        Property PasswordMD5() As String
            Get
                Return m_passwordMD5
            End Get
            Set(ByVal value As String)
                m_passwordMD5 = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the username.
        ''' </summary>
        ''' <value>The username.</value>
        Property Username() As String
            Get
                Return m_username
            End Get
            Set(ByVal value As String)
                m_username = value
            End Set
        End Property

        ''' <summary>
        ''' Initializes a new instance of the <see cref="VerifyUserRequest" /> class.
        ''' </summary>
        Sub New()
            MyBase.New(WebRequests.RequestType.VerifyUser, "VerifyUser")
        End Sub

        Public Overrides Sub Start(ByVal doasync As Boolean)
            Dim time As Long = UnixTime.GetUnixTime
            Dim auth As String = m_passwordMD5 & time
            Dim authLower = m_passwordMD5Lower & time

            Dim authdMD5 As String = ConvertStringEncoding(auth, System.Text.Encoding.ASCII, System.Text.Encoding.UTF8)
            Dim authdMD5Lower As String = ConvertStringEncoding(authLower, System.Text.Encoding.ASCII, System.Text.Encoding.UTF8)

            Dim path As String = "/ass/pwcheck.php?" & _
                                 "&time=" & EscapeUriData(time) & _
                                 "&username=" & EscapeUriData(m_username) & _
                                 "&auth=" & EscapeUriData(authdMD5) & _
                                 "&auth2=" & EscapeUriData(authdMD5Lower) & _
                                 "&defaultplayer="
            Me.get(path)
        End Sub
        Protected Overrides Sub success(ByVal response As String)
            response = response.Trim
            If response.Contains("BOOTSTRAP") Then
                m_bootStrapCode = BootStrapCodeEnum.BOOTSTRAP_ALLOWED
            Else
                m_bootStrapCode = BootStrapCodeEnum.BOOTSTRAP_DENIED
            End If
            If (response.Contains("OK2")) Then
                m_userAuthCode = UserAuthCodeEnum.AUTH_OK_LOWER
            ElseIf (response.Contains("OK")) Then
                m_userAuthCode = UserAuthCodeEnum.AUTH_OK
            ElseIf (response.Contains("INVALIDUSER")) Then
                m_userAuthCode = UserAuthCodeEnum.AUTH_BADUSER
            ElseIf (response.Contains("BADPASSWORD")) Then
                m_userAuthCode = UserAuthCodeEnum.AUTH_BADPASS
            Else
                m_userAuthCode = UserAuthCodeEnum.AUTH_ERROR
            End If
        End Sub
    End Class
End Namespace
