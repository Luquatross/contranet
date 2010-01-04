Namespace WebRequests
    Public MustInherit Class UserFeedRequestBase
        Inherits FeedRequestBase

        Protected m_user As String
        Sub New()

        End Sub
        Public Sub New(ByVal username As String)
            m_user = username
        End Sub
        Property Username() As String
            Get
                Return m_user
            End Get
            Set(ByVal value As String)
                m_user = value
            End Set
        End Property
    End Class
End Namespace