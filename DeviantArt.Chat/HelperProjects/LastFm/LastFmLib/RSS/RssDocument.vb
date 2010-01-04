Namespace RSS
    Public Class RssDocument
        Dim m_header As ChannelHeader
        Dim m_items As New List(Of RssItem)
        Property Header() As ChannelHeader
            Get
                Return m_header
            End Get
            Set(ByVal value As ChannelHeader)
                m_header = value
            End Set
        End Property
        Property Items() As List(Of RssItem)
            Get
                Return m_items
            End Get
            Set(ByVal value As List(Of RssItem))
                m_items = value
            End Set
        End Property
        Shared Function FromRSSString(ByVal s As String) As RssDocument
            Dim p As New RssParser(s)
            Dim doc As RssDocument = p.Parse()
            Return doc
        End Function
    End Class
End Namespace
