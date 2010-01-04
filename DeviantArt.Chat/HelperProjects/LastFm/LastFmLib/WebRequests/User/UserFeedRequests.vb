Namespace WebRequests
    Public Class UserManualRecommendationsFeedRequest
        Inherits UserFeedRequestBase

        Protected Overrides Function RequestUrl() As String
            Return "/1.0/user/" & m_user & "/manualrecs.rss"
        End Function
    End Class

    Public Class UserJournalFeedRequest
        Inherits UserFeedRequestBase

        Protected Overrides Function RequestUrl() As String
            Return "/1.0/user/" & m_user & "/replytracker.rss"
        End Function
    End Class

    Public Class UserCurrentEventsFeedRequest
        Inherits UserFeedRequestBase

        Protected Overrides Function RequestUrl() As String
            Return "/1.0/user/" & m_user & "/events.rss"
        End Function
    End Class
    Public Class UserFriendsEventsFeedRequest
        Inherits UserFeedRequestBase

        Protected Overrides Function RequestUrl() As String
            Return "/1.0/user/" & m_user & "/friendsevents.rss"
        End Function
    End Class
    Public Class UserSystemRecommendationsFeedRequest
        Inherits UserFeedRequestBase

        Protected Overrides Function RequestUrl() As String
            Return "/1.0/user/" & m_user & "/eventsysrecs.rss"
        End Function
    End Class
End Namespace
