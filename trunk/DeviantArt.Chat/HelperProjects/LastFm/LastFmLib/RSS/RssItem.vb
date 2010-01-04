Namespace RSS
    Public Class RssItem
        Private m_author As String
        Private m_title As String
        Private m_link As Uri
        Private m_description As String
        Private m_authorMail As String
        Private m_commentsUrl As Uri
        Private m_guid As String
        Private m_pubDate As DateTime
        Private m_source As RssSource
        Private m_categories As New List(Of Category)
        Private m_enclosure As Enclosure
        Private m_eventData As XCalEventData
        Property EventData() As XCalEventData
            Get
                Return m_eventData
            End Get
            Set(ByVal value As XCalEventData)
                m_eventData = value
            End Set
        End Property
        Property Enclosure() As Enclosure
            Get
                Return m_enclosure
            End Get
            Set(ByVal value As Enclosure)
                m_enclosure = value
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
        Property Source() As NamedUrl
            Get
                Return m_source
            End Get
            Set(ByVal value As NamedUrl)
                m_source = value
            End Set
        End Property
        Property PubDate() As DateTime
            Get
                Return m_pubDate
            End Get
            Set(ByVal value As Date)
                m_pubDate = value
            End Set
        End Property
        Property Guid() As String
            Get
                Return m_guid
            End Get
            Set(ByVal value As String)
                m_guid = value
            End Set
        End Property
        Property CommentsUrl() As Uri
            Get
                Return m_commentsUrl
            End Get
            Set(ByVal value As Uri)
                m_commentsUrl = value
            End Set
        End Property
        Property AuthorMail() As String
            Get
                Return m_authorMail
            End Get
            Set(ByVal value As String)
                m_authorMail = value
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
        Property Author() As String
            Get
                Return m_author
            End Get
            Set(ByVal value As String)
                m_author = value
            End Set
        End Property

        Shared Function FromXMLElement(ByVal e As Xml.XmlElement) As RssItem
            Dim result As New RssItem
            If e Is Nothing Then
                Throw New ArgumentNullException("e")
                Return Nothing
            End If
            If e.Name <> "item" Then
                Throw New Xml.XmlException("The name of 'e' MUST BE 'item'!")
                Return Nothing
            End If
            result.EventData = New XCalEventData()
            With result
                For Each n As Xml.XmlElement In e.ChildNodes
                    'Debug.Print(n.Name)
                    Select Case n.Name.ToLower
                        Case "title"
                            .Title = n.InnerText
                        Case "link"
                            If Uri.IsWellFormedUriString(n.InnerText, UriKind.RelativeOrAbsolute) Then
                                .Link = New Uri(n.InnerText)
                            End If
                        Case "description"
                            .Description = n.InnerText
                        Case "author"
                            .Author = n.InnerText
                        Case "category"
                            Dim cat As Category = Category.FromXmlElement(n)
                            .Categories.Add(cat)
                        Case "comments"
                            If Uri.IsWellFormedUriString(n.InnerText, UriKind.RelativeOrAbsolute) Then
                                .CommentsUrl = New Uri(n.InnerText)
                            End If
                        Case "enclosure"
                            Dim enc As Enclosure = Enclosure.FromXmlElement(n)
                            .Enclosure = enc
                        Case "guid"
                            'maybe create a class and add an "isPermalink"
                            .Guid = e.InnerText
                        Case "pubdate"
                            Dim d As DateTime
                            DateTime.TryParse(n.InnerText, d)
                            .PubDate = d
                        Case "source"
                            .Source = RssSource.FromXmlElement(n)
                        Case Else
                            If n.Name.StartsWith("xcal:") Then
                                .EventData.SetPropertyByXmlElement(n)
                            End If
                            'Case "xcal:dtend"
                            '    .EventData.SetPropertyByXmlElement(e)
                            'Case "xcal:location"
                            '    .EventData.SetPropertyByXmlElement(e)
                    End Select
                Next

                Return result
            End With
        End Function
    End Class
End Namespace