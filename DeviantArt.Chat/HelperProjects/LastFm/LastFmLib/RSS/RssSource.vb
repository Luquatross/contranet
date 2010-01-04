Namespace RSS
    Public Class RssSource
        Inherits NamedUrl

        Property Url() As Uri
            Get
                Return m_url
            End Get
            Set(ByVal value As Uri)
                m_url = value
            End Set
        End Property


        Sub New()

        End Sub
        Sub New(ByVal name As String, ByVal url As Uri)
            Me.m_name = name
            Me.m_url = url
        End Sub

        Shared Function FromXmlElement(ByVal e As Xml.XmlElement) As Category
            Dim result As New Category
            If e Is Nothing Then
                Return Nothing
            End If

            If Not e.Name.ToLower = "source" Then
                Return Nothing
            End If

            If e.HasAttribute("url") Then
                Dim uristring As String
                uristring = e.GetAttribute("url")
                If Uri.IsWellFormedUriString(uristring, UriKind.RelativeOrAbsolute) Then
                    result.Domain = New Uri(uristring)
                End If
            End If
            result.Name = e.InnerText
            Return result
        End Function
    End Class
End Namespace

