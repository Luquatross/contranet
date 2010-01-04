Namespace RSS
    Public Class RssImage
        Private m_url As Uri
        Private m_title As String
        Private m_link As Uri
        Private m_width As Integer
        Private m_Height As Integer
        Private m_description As String
        ReadOnly Property IsValid() As Boolean
            Get
                Return m_url IsNot Nothing And Not String.IsNullOrEmpty(m_title) And Not m_link Is Nothing
            End Get
        End Property
        ReadOnly Property HasWidth() As Boolean
            Get
                Return m_width > 0
            End Get
        End Property
        ReadOnly Property HasHeight() As Boolean
            Get
                Return m_Height > 0
            End Get
        End Property
        ReadOnly Property HasDescription() As Boolean
            Get
                Return Not String.IsNullOrEmpty(m_description)
            End Get
        End Property
        Property Description() As String
            Get
                Return m_description
            End Get
            Set(ByVal value As String)
                m_description = value
            End Set
        End Property

        Property Height() As Integer
            Get
                Return m_Height
            End Get
            Set(ByVal value As Integer)
                m_Height = value
            End Set
        End Property
        Property Width() As Integer
            Get
                Return m_width
            End Get
            Set(ByVal value As Integer)
                m_width = value
            End Set
        End Property
        Property Link() As Uri
            Get
                Return m_link
            End Get
            Set(ByVal value As Uri)
                m_link = value
            End Set
        End Property
        Property Title() As String
            Get
                Return m_title
            End Get
            Set(ByVal value As String)
                m_title = value
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

        Sub New()

        End Sub

        Shared Function FromXmlElement(ByVal e As Xml.XmlElement) As RssImage
            Dim result As New RssImage
            If e.Name IsNot "image" Then Return Nothing

            If e.Item("url") IsNot Nothing Then
                Dim urlstr As String = e.Item("url").InnerText
                If Uri.IsWellFormedUriString(urlstr, UriKind.RelativeOrAbsolute) Then
                    result.Url = New Uri(urlstr)
                End If
            End If

            If e.Item("title") IsNot Nothing Then result.Title = e.Item("title").InnerText

            If e.Item("link") IsNot Nothing Then
                Dim linkstr As String = e.Item("link").InnerText
                If Uri.IsWellFormedUriString(linkstr, UriKind.RelativeOrAbsolute) Then
                    result.Link = New Uri(linkstr)
                End If
            End If

            If e.Item("width") IsNot Nothing Then
                result.Width = CInt(e.Item("title").InnerText)
            Else
                'Default value
                result.Width = 88
            End If
            If e.Item("height") IsNot Nothing Then
                result.Height = CInt(e.Item("title").InnerText)
            Else
                'Default value
                result.Height = 31
            End If

            If e.Item("description") IsNot Nothing Then result.Title = e.Item("description").InnerText

            Return result
        End Function
    End Class
End Namespace
