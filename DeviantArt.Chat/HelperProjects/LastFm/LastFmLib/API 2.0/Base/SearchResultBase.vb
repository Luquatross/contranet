Namespace API20.Base
    Public MustInherit Class SearchResultBase
        Dim m_searchTerms As String
        Dim m_startIndex As Integer
        Dim m_totalResults As Integer
        Dim m_startPage As Integer
        Dim m_itemsPerPage As Integer
        Dim m_role As String

        Property Role() As String
            Get
                Return m_role
            End Get
            Set(ByVal value As String)
                m_role = value
            End Set
        End Property
        Property ItemsPerPage() As Integer
            Get
                Return m_itemsPerPage
            End Get
            Set(ByVal value As Integer)
                m_itemsPerPage = value
            End Set
        End Property
        Property StartPage() As Integer
            Get
                Return m_startPage
            End Get
            Set(ByVal value As Integer)
                m_startPage = value
            End Set
        End Property
        Property StartIndex() As Integer
            Get
                Return m_startIndex
            End Get
            Set(ByVal value As Integer)
                m_startIndex = value
            End Set
        End Property
        Property SearchTerms() As String
            Get
                Return m_searchTerms
            End Get
            Set(ByVal value As String)
                m_searchTerms = value
            End Set
        End Property

        Property TotalResults() As Integer
            Get
                Return m_totalResults
            End Get
            Set(ByVal value As Integer)
                m_totalResults = value
            End Set
        End Property

        Protected Sub New(ByVal elem As Xml.XmlElement)
            Dim nsm As New Xml.XmlNamespaceManager(elem.OwnerDocument.NameTable)
            nsm.AddNamespace("opensearch", "http://a9.com/-/spec/opensearch/1.1/")
            Dim query As Xml.XmlElement = elem.SelectSingleNode("opensearch:Query", nsm)
            If query.HasAttribute("role") Then m_role = query.GetAttribute("role")
            If query.HasAttribute("searchTerms") Then m_role = query.GetAttribute("searchTerms")
            If query.HasAttribute("startPage") Then Integer.TryParse(query.GetAttribute("startPage"), m_startPage)

            Integer.TryParse(Util.GetSubElementValue(elem, "opensearch:totalResults", nsm), m_totalResults)
            Integer.TryParse(Util.GetSubElementValue(elem, "opensearch:startIndex", nsm), m_startIndex)
            Integer.TryParse(Util.GetSubElementValue(elem, "opensearch:itemsPerPage", nsm), m_itemsPerPage)

        End Sub

        Public Overridable Function ToDebugString() As String
            Dim result As String = ""
            With Me
                result &= "Search terms:" & .SearchTerms & vbCrLf
                result &= "Role: " & .Role & vbCrLf
                result &= "Start index :" & .StartIndex & vbCrLf
                result &= "Start page" & .StartPage & vbCrLf
                result &= "Number of total results" & .TotalResults
                result &= "Items per page" & .ItemsPerPage & vbCrLf
                Return result
            End With
        End Function

    End Class
End Namespace
