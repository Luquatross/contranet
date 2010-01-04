Imports System.Xml
Namespace WebRequests
    ''' <summary>
    ''' Requests a list of tags which are similiar to a tag.
    ''' </summary>
    Public Class SimilarTagsRequest
        Inherits TagsRequestBase
        Private m_Tag As String
        ''' <summary>
        ''' The tag of which the similar ones should be requested.
        ''' </summary>
        ''' <value>The tag.</value>
        Property Tag() As String
            Get
                Return m_Tag
            End Get
            Set(ByVal value As String)
                m_Tag = value
            End Set
        End Property

        ''' <summary>
        ''' Initializes a new instance of the <see cref="SimilarTagsRequest" /> class.
        ''' </summary>
        ''' <param name="tag">The tag of which the similar ones should be requested.</param>
        Sub New(ByVal tag As String)
            MyBase.New(RequestType.SimilarTags, "SimilarTags")
            m_Tag = tag
        End Sub
        Public Overrides Sub Start()
            Dim rpc As New XMLRPC()
            With rpc
                .Method = "getSimilarTags"
                .addParameter(m_Tag)
            End With
            Request(rpc)

        End Sub
        Protected Overrides Sub success(ByVal data As String)
            Dim retVals As New List(Of Object)
            Dim err As String = ""
            Dim parsed As Boolean = XMLRPC.ParseResponse(data, retVals, err)
            If Not parsed Or retVals.Count = 0 Then Exit Sub
            If Not retVals(0).GetType Is (New Dictionary(Of String, Object)).GetType Then Exit Sub
            Dim topMap As Dictionary(Of String, Object) = retVals(0)
            If topMap.ContainsKey("faultCode") Then
                Dim faultString As String = CStr(topMap.Item("faultCode"))
                Exit Sub
            End If
            Dim wmax As Integer = 0

            For Each v As Object In topMap.Item("tags")
                Dim map As Dictionary(Of String, Object) = v
                Dim w As Integer = CInt(map.Item("weight"))
                If w > wmax Then wmax = w
                m_tags.Add(TypeClasses.WeightedString.weighted(CStr(map.Item("name")).ToLower, w))

            Next
            Dim searchterm As String = CStr(topMap.Item("search"))
            m_tags.Insert(0, TypeClasses.WeightedString.weighted(searchterm, wmax))
        End Sub
    End Class
End Namespace

