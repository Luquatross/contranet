Imports System.Xml
Namespace WebRequests
    ''' <summary>
    ''' Gets the overall top tags.
    ''' </summary>
    Public Class OverallTopTagsRequest
        Inherits TagsRequestBase
        ''' <summary>
        ''' Initializes a new instance of the <see cref="OverallTopTagsRequest" /> class.
        ''' </summary>
        Sub New()
            MyBase.New(RequestType.TopTags, "TopTags")
        End Sub
        Public Overrides Sub Start()
            Me.get("/1.0/tag/toptags.xml")
        End Sub
        Protected Overrides Sub success(ByVal data As String)
            Dim xml As New XmlDocument
            xml.LoadXml(data)

            Dim values As XmlNodeList = xml.SelectNodes("toptags/tag")

            For Each item As XmlNode In values
                Dim Name As String = item.Attributes("name").InnerText
                Dim count As Integer = CInt(item.Attributes("count").InnerText())

                m_tags.Add(TypeClasses.WeightedString.counted(Name, count))
            Next
        End Sub
    End Class
End Namespace