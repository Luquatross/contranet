Imports System.Xml
Namespace WebRequests
    ''' <summary>
    ''' Gets the personal tags of a user
    ''' </summary>
    Public Class UserTagsRequest
        Inherits TagsRequestBase
        Protected m_username As String
        ''' <summary>
        ''' Gets the path(relative to the user's url)
        ''' </summary>
        ''' <value>The path.</value>
        Overridable ReadOnly Property path() As String
            Get
                Return "/tags.xml"
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the username.
        ''' </summary>
        ''' <value>The username.</value>
        Property Username() As String
            Get
                Return m_username
            End Get
            Set(ByVal value As String)
                m_username = value
            End Set
        End Property
        ''' <summary>
        ''' Initializes a new instance of the <see cref="UserTagsRequest" /> class.
        ''' </summary>
        ''' <param name="requesttype">The requesttype.</param>
        ''' <param name="name">The name.</param>
        Protected Sub New(ByVal requesttype As WebRequests.RequestType, ByRef name As String)
            MyBase.New(requesttype, name)
        End Sub
        ''' <summary>
        ''' Initializes a new instance of the <see cref="UserTagsRequest" /> class.
        ''' </summary>
        Sub New()
            MyBase.New(WebRequests.RequestType.UserTags, "UserTags")
        End Sub
        ''' <summary>
        ''' Initializes a new instance of the <see cref="UserTagsRequest" /> class.
        ''' </summary>
        ''' <param name="user">The username.</param>
        Sub New(ByVal user As String)
            Me.New()
            m_username = user
        End Sub
        Public Overrides Sub Start()
            [get]("/1.0/user/" + EscapeUriData(m_username) + path)
        End Sub
        Protected Overrides Sub success(ByVal data As String)
            Dim xml As New XmlDocument
            xml.LoadXml(data)

            Dim values As XmlNodeList = xml.DocumentElement.SelectNodes("tag")

            For i As Integer = 0 To values.Count - 1
                Dim item As XmlNode = values.Item(i)

                Dim Name As String = item.Item("name").InnerText()
                Dim count As Integer = CInt(item.Item("count").InnerText())

                m_tags.Add(TypeClasses.WeightedString.counted(Name, count))
            Next
        End Sub

    End Class
End Namespace
