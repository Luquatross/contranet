Imports LastFmLib.API20.Types
Namespace API20.User
    ''' <summary>
    ''' Get a list of the user's friends on Last.fm. 
    ''' </summary>
    Public Class UserGetFriends
        Inherits Base.BaseUserRequest

        Private m_limit As Integer

        Private m_recenttracks As Boolean
        Private m_users As List(Of FriendUser)
        ReadOnly Property Result() As List(Of FriendUser)
            Get
                Return m_users
            End Get
        End Property
        Public Property IncludeRecentTracks() As Boolean
            Get
                Return m_recenttracks
            End Get
            Set(ByVal value As Boolean)
                m_recenttracks = value
            End Set
        End Property

        Public Property Limit() As Integer
            Get
                Return m_limit
            End Get
            Set(ByVal value As Integer)
                m_limit = value
            End Set
        End Property

        Sub New(ByVal uname As String, Optional ByVal incRecentTracks As Boolean = False)
            MyBase.New(RequestType.UserGetFriends, uname)
            m_recenttracks = True
        End Sub
        Public Overrides Sub Start()
            If m_limit > 0 Then SetAddParamValue("limit", CStr(m_limit))
            If m_recenttracks Then SetAddParamValue("recenttracks", CStr(m_recenttracks))
            MyBase.Start()
        End Sub
        Protected Overrides Sub Success(ByVal elem As System.Xml.XmlElement)
            m_User = Util.GetAttrValue(elem, "user")
            m_users = New List(Of FriendUser)
            For Each a As Xml.XmlElement In elem.SelectNodes("user")
                m_users.Add(New FriendUser(a))
            Next
        End Sub
    End Class
End Namespace
