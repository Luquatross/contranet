Namespace WebRequests
    Public Class UserReplyTrackerFeedRequest
        Inherits FeedRequestBase

        Dim m_user As String
        Property Username() As String
            Get
                Return m_user
            End Get
            Set(ByVal value As String)
                m_user = value
            End Set
        End Property
        
        Protected Overrides Function RequestUrl() As String
            Return ("/1.0/user/" & m_user & "/replytracker.rss")
        End Function

    End Class
End Namespace
