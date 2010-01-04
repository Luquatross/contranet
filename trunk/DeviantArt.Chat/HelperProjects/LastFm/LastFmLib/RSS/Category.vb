Namespace RSS
    Public Class Category
        Inherits NamedUrl
        Public Property Domain() As Uri
            Get
                Return m_url
            End Get
            Set(ByVal value As Uri)
                m_url = value
            End Set
        End Property


        Shared Function FromXmlElement(ByVal e As Xml.XmlElement) As Category
            Dim result As New Category
            If e Is Nothing Then
                Return Nothing
            End If

            If Not e.Name.ToLower = "category" Then
                Return Nothing
            End If

            If e.HasAttribute("domain") Then
                Dim uristring As String
                uristring = e.GetAttribute("domain")
                If Uri.IsWellFormedUriString(uristring, UriKind.RelativeOrAbsolute) Then
                    result.Domain = New Uri(uristring)
                End If
            End If
            result.Name = e.InnerText
            Return result
        End Function
    End Class
End Namespace