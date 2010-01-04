Namespace API20.Types
    ''' <summary>
    ''' Stores the auth token. Can determine if it was invalidated.
    ''' </summary>
    Public Class AuthToken
        Inherits MD5Hash
        Dim m_timestamp As Integer
        Dim m_autoRefresh As Boolean = True

        ''' <summary>
        ''' Determines if then token should be automatically refreshed
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property AutoRefresh() As Boolean
            Get
                Return m_autoRefresh
            End Get
            Set(ByVal value As Boolean)
                m_autoRefresh = value
            End Set
        End Property
        ''' <summary>
        ''' The token age in seconds
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property Age() As Integer
            Get
                Return UnixTime.GetUnixTime - m_timestamp
            End Get
        End Property

        ''' <summary>
        ''' Gets a value indicating whether the is valid yet.
        ''' </summary>
        ''' <value><c>true</c> if this instance is valid; otherwise, <c>false</c>.</value>
        ReadOnly Property IsValid() As Boolean
            Get
                Return Age <= 3600
            End Get
        End Property
        ''' <summary>
        ''' Gets a value indicating whether the token is outdated.
        ''' </summary>
        ''' <value>
        ''' <c>true</c> if this instance is outdated; otherwise, <c>false</c>.
        ''' </value>
        ReadOnly Property IsOutdated() As Boolean
            Get
                Return Not IsValid
            End Get
        End Property
        ''' <summary>
        ''' Initializes a new instance of the <see cref="AuthToken" /> class.
        ''' </summary>
        ''' <param name="token">The token.</param>
        Sub New(ByVal token As MD5Hash)
            MyBase.New(token)
            m_timestamp = UnixTime.GetUnixTime()
        End Sub
        Sub New()

        End Sub
        ''' <summary>
        ''' Requests a new token
        ''' </summary>
        ''' <param name="api_key">The api_key.</param>
        ''' <returns>
        ''' <c>true</c> if successful, else <c>false</c>
        ''' </returns>
        Function RequestToken(ByVal api_key As MD5Hash) As Boolean
            Dim t As New API20.Auth.AuthGetToken(api_key)
            UpdateTimestamp()
            t.Start()
            If t.succeeded Then
                m_hash = t.Result.ToString
                Return True
            End If
            Return False
        End Function
        ''' <summary>
        ''' Updates the timestamp.
        ''' </summary>
        Sub UpdateTimestamp()
            m_timestamp = UnixTime.GetUnixTime
        End Sub
    End Class
End Namespace