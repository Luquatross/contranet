Namespace WebRequests
    Public MustInherit Class FeedRequestBase
        Inherits RequestBase
        Protected m_doc As New RSS.RssDocument()

        Protected MustOverride Function RequestUrl() As String


        ReadOnly Property RssDocument() As RSS.RssDocument
            Get
                Return m_doc
            End Get
        End Property
        Public Overrides Sub Start()
            Me.get(RequestUrl)
        End Sub
        Protected Overrides Sub success(ByVal data As String)
            Dim p As New RSS.RssParser(data)
            m_doc = p.Parse()
        End Sub
    End Class


End Namespace
