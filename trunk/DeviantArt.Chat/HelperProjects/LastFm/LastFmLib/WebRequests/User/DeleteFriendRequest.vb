Namespace WebRequests
    ''' <summary>
    ''' Request for deleting a Friend of a user in Last.fm
    ''' </summary>
    Public Class DeleteFriendRequest
        Inherits RequestBase
        Private m_user As TypeClasses.LastFmUser
        Private m_friend_username As String
        ''' <summary>
        ''' Gets the deleted username.
        ''' </summary>
        ReadOnly Property DeletedUsername() As String
            Get
                Return m_friend_username
            End Get
        End Property
        Property User() As TypeClasses.LastFmUser
            Get
                Return m_user
            End Get
            Set(ByVal value As TypeClasses.LastFmUser)
                m_user = value
            End Set
        End Property
        ''' <summary>
        ''' Initializes a new instance of the <see cref="DeleteFriendRequest" /> class.
        ''' </summary>
        ''' <param name="target_user">The user which should be deleted.</param>
        Sub New(ByVal user As TypeClasses.LastFmUser, ByVal target_user As String)
            MyBase.New(WebRequests.RequestType.DeleteFriend, "DeleteFriend")
            m_user = user
            m_friend_username = target_user
        End Sub
        Public Overrides Sub Start()
            Dim rpc As New XMLRPC
            rpc.Method = "removeFriend"
            Dim challenge As String = GetChallenge()
            With rpc
                .addParameter(EscapeUriData(m_user.Username))
                .addParameter(challenge)
                .addParameter(GetRequestAuthCode(User.PasswordMD5, challenge))
                .addParameter(EscapeUriData(m_friend_username))
            End With
        End Sub
    End Class
End Namespace