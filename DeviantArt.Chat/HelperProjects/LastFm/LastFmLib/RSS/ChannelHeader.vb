Namespace RSS
    Public Class ChannelHeader
        Private m_title As String
        Private m_link As Uri
        Private m_description As String
        Private m_language As String
        Private m_copyright As String
        Private m_managingEditorMail As String
        Private m_webMasterMail As String
        Private m_pubDate As Date
        Private m_lastBuildDate As Date
        Private m_categories As New List(Of Category)
        Private m_generator As String
        Private m_DocumentationUrl As Uri
        Private m_cloud As Cloud
        Private m_ttl As Integer
        Private m_image As RssImage
        Private m_picsRating As String
        Private m_skipHours As New List(Of Integer)
        Private m_skipDays As New List(Of String)
        Property SkipHours() As List(Of Integer)
            Get
                Return m_skipHours
            End Get
            Set(ByVal value As List(Of Integer))
                m_skipHours = value
            End Set
        End Property

        Property PicsRating() As String
            Get
                Return m_picsRating
            End Get
            Set(ByVal value As String)
                m_picsRating = value
            End Set
        End Property
        Property Image() As RssImage
            Get
                Return m_image
            End Get
            Set(ByVal value As RssImage)
                m_image = value
            End Set
        End Property
        Property TTL() As Integer
            Get
                Return m_ttl
            End Get
            Set(ByVal value As Integer)
                m_ttl = value
            End Set
        End Property
        Property Cloud() As Cloud
            Get
                Return m_cloud
            End Get
            Set(ByVal value As Cloud)
                m_cloud = value
            End Set
        End Property
        Property DocumentationUrl() As Uri
            Get
                Return m_DocumentationUrl
            End Get
            Set(ByVal value As Uri)
                m_DocumentationUrl = value
            End Set
        End Property
        Property Generator() As String
            Get
                Return m_generator
            End Get
            Set(ByVal value As String)
                m_generator = value
            End Set
        End Property
        Property Categories() As List(Of Category)
            Get
                Return m_categories
            End Get
            Set(ByVal value As List(Of Category))
                m_categories = value
            End Set
        End Property
        Property LastBuildDate() As Date
            Get
                Return m_lastBuildDate
            End Get
            Set(ByVal value As Date)
                m_lastBuildDate = value
            End Set
        End Property
        Property PubDate() As Date
            Get
                Return m_pubDate
            End Get
            Set(ByVal value As Date)
                m_pubDate = value
            End Set
        End Property
        Property WebMasterMail() As String
            Get
                Return m_webMasterMail
            End Get
            Set(ByVal value As String)
                m_webMasterMail = value
            End Set
        End Property
        Property ManagingEditorMail() As String
            Get
                Return m_managingEditorMail
            End Get
            Set(ByVal value As String)
                m_managingEditorMail = value
            End Set
        End Property
        Property Copyright() As String
            Get
                Return m_copyright
            End Get
            Set(ByVal value As String)
                m_copyright = value
            End Set
        End Property
        Property Language() As String
            Get
                Return m_language
            End Get
            Set(ByVal value As String)
                m_language = value
            End Set
        End Property
        Property Description() As String
            Get
                Return m_description
            End Get
            Set(ByVal value As String)
                m_description = value
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

        Shared Function FromXmlElement(ByVal e As Xml.XmlElement) As ChannelHeader
            Dim result As New ChannelHeader
            If e.Name.ToLower <> "channel" Then
                Throw New RssWrongNameException("e", "channel")
                Return Nothing
            End If
            With e
                For Each n As Xml.XmlElement In e.ChildNodes
                    Debug.Print(n.Name)
                    If n.NodeType = Xml.XmlNodeType.Element Then
                        Select Case n.Name.ToLower
                            Case "title"
                                result.Title = n.InnerText
                            Case "link"
                                If Uri.IsWellFormedUriString(n.InnerText, UriKind.RelativeOrAbsolute) Then
                                    result.Link = New Uri(n.InnerText)
                                End If
                            Case "description"
                                result.Description = n.InnerText
                            Case "language"
                                result.Language = n.InnerText
                            Case "copyright"
                                result.Copyright = n.InnerText
                            Case "managingeditor"
                                result.ManagingEditorMail = n.InnerText
                            Case "webmaster"
                                result.WebMasterMail = n.InnerText
                            Case "pubdate"
                                result.PubDate = n.InnerText
                            Case "lastbuilddate"
                                Dim d As Date
                                If Date.TryParse(n.InnerText, d) Then result.LastBuildDate = d
                            Case "category"
                                Dim c As Category = Category.FromXmlElement(n)
                                result.Categories.Add(c)
                            Case "generator"
                                result.Generator = n.InnerText
                            Case "docs"
                                If Uri.IsWellFormedUriString(n.InnerText, UriKind.RelativeOrAbsolute) Then
                                    result.DocumentationUrl = New Uri(n.InnerText)
                                End If
                            Case "cloud"
                                result.Cloud = Cloud.fromXmlElement(n)
                            Case "ttl"
                                result.TTL = CInt(n.InnerText)
                            Case "image"
                                result.Image = RssImage.FromXmlElement(n)
                            Case "rating"
                                result.PicsRating = n.InnerText
                            Case "textinput"
                                'ignore it
                            Case "skiphours"
                                For Each x As Xml.XmlElement In n.SelectNodes("hour")
                                    If Not result.SkipHours.Contains(CInt(x.InnerText)) Then
                                        result.SkipHours.Add(CInt(x.InnerText))
                                    End If
                                Next
                            Case "skipdays"
                                For Each x As Xml.XmlElement In n.SelectNodes("day")
                                    If Not result.SkipHours.Contains(CInt(x.InnerText)) Then
                                        result.SkipHours.Add(CInt(x.InnerText))
                                    End If
                                Next
                        End Select
                    End If
                Next
            End With
            Return result
        End Function
    End Class
End Namespace