Namespace API20.Types
    ''' <summary>
    ''' Result class of tag search
    ''' </summary>
    Public Class TagSearchResult
        Inherits Base.SearchResultBase
        Dim m_tags As List(Of TagInfo)

        Property tags() As List(Of TagInfo)
            Get
                Return m_tags
            End Get
            Set(ByVal value As List(Of TagInfo))
                m_tags = value
            End Set
        End Property
        ''' <summary>
        ''' Creates a new instance of tagSearchResult using an xml-element
        ''' </summary>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Sub New(ByVal e As Xml.XmlElement)
            MyBase.New(e)
        End Sub
        Public Overrides Function ToDebugString() As String
            Dim result As String = MyBase.ToDebugString
            With Me
                result = MyBase.ToString
                Dim tags As String = ""
                For Each stag As TagInfo In .tags
                    tags &= stag.Name & ", "
                Next
                tags = tags.Substring(0, tags.Length - 2)
                result &= "Matched tags: " & tags
                Return result
            End With
        End Function
        ''' <summary>
        ''' Creates an instance from an xml element containing the search results
        ''' </summary>
        ''' <param name="elem"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function FromXmlElement(ByVal elem As Xml.XmlElement) As tagSearchResult
            Dim m_result As New tagSearchResult(elem)

            Dim tagNodes As Xml.XmlNodeList = elem.SelectNodes("./tagmatches/tag")
            m_result.tags = New List(Of TagInfo)
            For Each e As Xml.XmlElement In tagNodes
                m_result.tags.Add(TagInfo.FromXmlElement(e))
            Next
            Return m_result
        End Function

    End Class
End Namespace
