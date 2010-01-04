Namespace RSS
    Public Class Enclosure
        Private m_url As Uri
        Private m_length As Integer
        Private m_mimeType As String
        Property MimeType() As String
            Get
                Return m_mimeType
            End Get
            Set(ByVal value As String)
                m_mimeType = value
            End Set
        End Property
        Property Length() As Integer
            Get
                Return m_length
            End Get
            Set(ByVal value As Integer)
                m_length = value
            End Set
        End Property
        Property Url() As Uri
            Get
                Return m_url
            End Get
            Set(ByVal value As Uri)
                m_url = value
            End Set
        End Property

        Shared Function FromXmlElement(ByVal e As Xml.XmlElement) As Enclosure

            If e.Name.ToLower <> "enclosure" Then
                Throw New RssWrongNameException("e", "enclosure")
                Return Nothing
            End If

            Dim result As New Enclosure()

            If e.HasAttribute("url") Then
                Dim urlstr As String = e.GetAttribute("url")
                If Uri.IsWellFormedUriString(urlstr, UriKind.RelativeOrAbsolute) Then
                    result.Url = New Uri(urlstr)
                End If
            End If

            If e.HasAttribute("length") Then
                result.Length = CInt(e.GetAttribute("length"))
            End If

            If e.HasAttribute("type") Then
                result.MimeType = e.GetAttribute("type")
            End If
            Return result
        End Function
    End Class
End Namespace