Imports LastFmLib.Utils
Imports LastFmLib.TypeClasses
Imports LastFmLib.WebRequests
Namespace WebRequests
    ''' <summary>
    ''' Requests the tags given to an artist
    ''' </summary>
    Public Class ArtistTagsRequest
        Inherits TagsRequestBase

        Private m_artist As String
        ''' <summary>
        ''' Gets or sets the artist.
        ''' </summary>
        ''' <value>The artist.</value>
        Property Artist() As String
            Get
                Return m_artist
            End Get
            Set(ByVal value As String)
                m_artist = value
            End Set
        End Property

        ''' <summary>
        ''' Initializes a new instance of the <see cref="ArtistTagsRequest" /> class.
        ''' </summary>
        ''' <param name="artist">The artist.</param>
        Public Sub New(ByVal artist As String)
            MyBase.New(WebRequests.RequestType.ArtistTags, "ArtistTags")
            Me.m_artist = artist
        End Sub


        Public Overrides Sub Start()
            Me.get("/1.0/artist/" & EscapeUriData(m_artist) & "/toptags.xml")
        End Sub


        Protected Overrides Sub success(ByVal data As String)
            Dim [xml] As New Xml.XmlDocument
            [xml].LoadXml(data)
            Dim values As Xml.XmlNodeList
            values = [xml].DocumentElement.SelectNodes("tag")
            For i As Integer = 0 To values.Count - 1
                m_tags.Add(New WeightedString(values.Item(i).Item("name").InnerText))
            Next
        End Sub
    End Class
End Namespace
