Imports System.Xml
Namespace WebRequests
    ''' <summary>
    ''' Requests a list of Artists which are similiar to an artist
    ''' </summary>
    Public Class SimilarArtistsRequest
        Inherits RequestBase
        Private m_Artist As String
        Private m_Artists As New TypeClasses.WeightedArtistList

        ''' <summary>
        ''' Gets the artists which are similar.
        ''' </summary>
        ''' <value>The artists.</value>
        ReadOnly Property SimilarArtists() As TypeClasses.WeightedArtistList
            Get
                Return m_Artists
            End Get
        End Property

        ''' <summary>
        ''' The artist of which the similar artists should be requested
        ''' </summary>
        ''' <value>The artist.</value>
        ReadOnly Property Artist() As String
            Get
                Return m_Artist
            End Get
        End Property

        ''' <summary>
        ''' Initializes a new instance of the <see cref="SimilarArtistsRequest" /> class.
        ''' </summary>
        ''' <param name="artist">The artist of which the similar artists should be requested</param>
        Sub New(ByVal artist As String)
            MyBase.New(WebRequests.RequestType.SimilarArtists, "SimilarArtists")
            m_Artist = artist
        End Sub

        Private Sub New()
            MyBase.new(RequestType.SimilarArtists)
        End Sub
        ''' <summary>
        ''' Initializes a new instance of the <see cref="ArtistMetadataRequest" /> class. For doing so it deserializes and xml element.
        ''' </summary>
        ''' <param name="e">The xml element.</param>
        Private Sub New(ByVal e As XmlElement)
            Me.New()
            NewFromElement(e)
        End Sub
        Private Sub NewFromElement(ByVal e As Xml.XmlElement)
            If Not IsValidForDeserialization(e, Me.Type) Then Me.Finalize()
            GetTimeStampFromElem(e)
            success(RequestData)
        End Sub
        ''' <summary>
        ''' Initializes an instance of this class by deserializing an XmlElement
        ''' </summary>
        ''' <param name="data">The data.</param>
        ''' <returns>
        ''' If successful, a ArtistMetadataRequest is returned, else <c>null</c>
        ''' </returns>
        Public Overloads Shared Function InitFromXmlElement(ByVal data As XmlElement) As SimilarArtistsRequest
            Return CreateRequest(data)
        End Function
        Protected Shared Function CreateRequest(ByVal e As Xml.XmlElement) As SimilarArtistsRequest
            Dim result As SimilarArtistsRequest = Nothing

            If (IsValidForDeserialization(e, RequestType.SimilarArtists)) Then
                result = New SimilarArtistsRequest(e)
            End If
            Return result
        End Function
        Public Overrides Sub Start()
            Me.get("/1.0/get.php?resource=artist&document=similar&format=xml&artist=" + EscapeUriData(m_Artist))
        End Sub

        Protected Overrides Sub success(ByVal data As String)
            Dim xml As New XmlDocument
            Try
                xml.LoadXml(data)
            Catch ex As Xml.XmlException
                MsgBox("Similar Artists Request: Couldn't parse data!")
                Exit Sub
            End Try

            Dim attr As XmlAttributeCollection = xml.SelectNodes("similarartists").Item(0).Attributes
            Dim artist As String = attr.GetNamedItem("artist").Value
            Dim image As String = attr.GetNamedItem("picture").Value
            Dim w As TypeClasses.WeightedArtist = TypeClasses.WeightedArtist.weighted(artist, 100)
            w.SetImageUrlByString(image)
            m_Artists.Add(TypeClasses.WeightedArtist.weighted(artist, 100))
            Dim values As XmlNodeList = xml.GetElementsByTagName("artist")
            For i As Integer = 0 To values.Count - 1
                Dim n As XmlNode = values.Item(i)
                Dim item As XmlNode = n.Item("name")
                Dim match As XmlNode = n.Item("match")
                Dim img_s As XmlNode = n.Item("image_small")
                Dim img As XmlNode = n.Item("image")
                Dim wa As TypeClasses.WeightedArtist = TypeClasses.WeightedArtist.weighted(item.InnerText, CInt(match.InnerText))
                wa.SetImageUrlByString(img.InnerText)
                wa.SetImageSmallUrlByString(img_s.InnerText)
                w.SetImageUrlByString(image)
                m_Artists.Add(wa)
            Next
        End Sub
    End Class
End Namespace

