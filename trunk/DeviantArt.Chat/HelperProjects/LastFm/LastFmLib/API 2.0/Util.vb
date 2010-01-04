Imports System.Xml
Public MustInherit Class Util
    ''' <summary>
    ''' Gets the value of en sub-element of an xml-element
    ''' </summary>
    ''' <param name="e">The parent element</param>
    ''' <param name="name">the child element's name</param>
    ''' <returns>The inner text of the child element</returns>
    ''' <remarks></remarks>
    Public Shared Function GetSubElementValue(ByVal e As XmlElement, ByVal name As String, Optional ByVal nsm As XmlNamespaceManager = Nothing) As String
        If e Is Nothing Then Return Nothing
        If e.IsEmpty Then Return Nothing
        Dim q As XmlElement = Nothing
        If nsm Is Nothing Then
            q = e.SelectSingleNode(name)
        Else
            q = e.SelectSingleNode(name, nsm)
        End If

        If q IsNot Nothing AndAlso Not q.IsEmpty Then Return q.InnerText
        Return Nothing
    End Function
    ''' <summary>
    ''' Helper function to get an url from a (maybe empty) string
    ''' </summary>
    ''' <param name="s"></param>
    ''' <returns>an Uri</returns>
    ''' <remarks></remarks>
    Public Shared Function GetUrl(ByVal s As String) As Uri
        If String.IsNullOrEmpty(s) Then Return Nothing
        Dim result As Uri = Nothing
        If s.StartsWith("www.") Then s = "http://" & s
        Uri.TryCreate(s, UriKind.RelativeOrAbsolute, result)
        Return result
    End Function
    Public Shared Function GetAttrValue(ByVal e As XmlElement, ByVal name As String) As String
        Return If(e.HasAttribute(name), e.GetAttribute(name), Nothing)
    End Function
    Public Shared Function GetGuid(ByVal s As String) As Guid
        If String.IsNullOrEmpty(s) Then Return Nothing
        If String.IsNullOrEmpty(s.Trim) Then Return Nothing
        Try
            Return New Guid
        Catch ex As Exception
            Return Nothing
        End Try
    End Function
    Shared Function GetUrlstrOrNothing(ByVal url As Uri) As String
        If url Is Nothing Then Return ""
        If url.IsAbsoluteUri = False Then Return url.OriginalString
        Return url.AbsoluteUri
    End Function
End Class
