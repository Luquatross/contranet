Namespace WebRequests
    ''' <summary>
    ''' Searches for a Ta
    ''' </summary>
    Public Class SearchTagRequest
        Inherits TagsRequestBase
        Private m_tag As String
        Public Property Tag() As String
            Get
                Return m_tag
            End Get
            Set(ByVal Value As String)
                m_tag = Value
            End Set
        End Property

        Sub New()
            MyBase.New(RequestType.SearchTag, "SearchTags")
        End Sub
        Overrides Sub start()
            Me.get("/1.0/tag/" & EscapeUriData(m_tag) & "/search.xml?showtop10=1")
        End Sub

        Protected Overrides Sub success(ByVal data As String)
            Dim xml As New Xml.XmlDocument
            xml.LoadXml(data)

            Dim values As Xml.XmlNodeList = xml.GetElementsByTagName("tag")
            For Each node As Xml.XmlNode In values
                Dim item As Xml.XmlNode = node.Item("name")
                Dim match As Xml.XmlNode = node.Item("match")
                Dim match_pc As Integer = CInt(CSng(match.InnerText) * 100)
                m_tags.Add(TypeClasses.WeightedString.weighted(item.InnerText, match_pc))
            Next
        End Sub
    End Class
End Namespace