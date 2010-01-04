Imports System.Xml
Namespace WebRequests

    ''' <summary>
    ''' Gets the information of an album, such as MusicBrainz-Id,Covers, Track Nubers and Titles, Release Date, ....
    ''' </summary>
    ''' <remarks>Just look at the variables below to see what you'll get^^</remarks>
    <Serializable()> _
    Public Class AlbumMetaDataRequest
        Inherits WebRequests.RequestBase
        '        Implements Xml.Serialization.IXmlSerializable

        Dim m_album As String
        Dim m_artist As String
        Dim m_PlayCount As Integer
        Dim m_releaseDate As Date
        Dim m_AlbumUrl As Uri
        Private m_CoverSmall As Uri
        Private m_CoverMedium As Uri
        Private m_CoverLarge As Uri
        Private m_mbId As String
        Private m_tracks As New List(Of TypeClasses.TrackInfo)

#Region "Propertys"
        ''' <summary>
        ''' The MusicBrainz-Id of the current Album.
        ''' </summary>
        ReadOnly Property mbId() As String
            Get
                Return m_mbId
            End Get
        End Property
        ''' <summary>
        ''' Gets the Url to the large cover.
        ''' </summary>
        ReadOnly Property CoverLarge() As Uri
            Get
                Return m_CoverLarge
            End Get
        End Property
        ''' <summary>
        ''' Gets the Url to the medium cover.
        ''' </summary>
        ReadOnly Property CoverMedium() As Uri
            Get
                Return m_CoverMedium
            End Get

        End Property
        ''' <summary>
        ''' Gets the Url to the small cover.
        ''' </summary>
        ReadOnly Property CoverSmall() As Uri
            Get
                Return m_CoverSmall
            End Get

        End Property
        ''' <summary>
        ''' Gets the Url to Last.fm's album page for this album.
        ''' </summary>
        ReadOnly Property Url() As Uri
            Get
                Return m_AlbumUrl
            End Get
        End Property
        ''' <summary>
        ''' Gets the album's release date.
        ''' </summary>
        ''' <value>The release date.</value>
        ReadOnly Property releaseDate() As Date
            Get
                Return m_releaseDate
            End Get
        End Property
        ''' <summary>
        ''' Gets the number of plays for this album.
        ''' </summary>
        ReadOnly Property PlayCount() As Integer
            Get
                Return m_PlayCount
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the artist name.
        ''' </summary>
        ''' <value>The artist name</value>
        Property Artist() As String
            Get
                Return m_artist
            End Get
            Set(ByVal value As String)
                m_artist = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the album name
        ''' </summary>
        ''' <value>The album name</value>
        Property Album() As String
            Get
                Return m_album
            End Get
            Set(ByVal value As String)
                m_album = value
            End Set
        End Property
        ''' <summary>
        ''' Gets the tracks contained in the album
        ''' </summary>
        ReadOnly Property Tracks() As List(Of TypeClasses.TrackInfo)
            Get
                Return m_tracks
            End Get
        End Property
#End Region


        Private Sub New()
            'Name is set automatically by type
            MyBase.new(RequestType.AlbumMetaData)
        End Sub
        ''' <summary>
        ''' Initializes a new instance of the <see cref="AlbumMetaDataRequest" /> class.
        ''' </summary>
        ''' <param name="sArtist">The artist name.</param>
        ''' <param name="sAlbum">The album name.</param>
        Public Sub New(ByVal sArtist As String, ByVal sAlbum As String)
            Me.New()
            m_artist = sArtist
            m_album = sAlbum
        End Sub

        ''' <summary>
        ''' Initializes a new instance of the <see cref="ArtistMetadataRequest" /> class. For doing so it deserializes and xml element.
        ''' </summary>
        ''' <param name="e">The xml element.</param>
        Public Sub New(ByVal e As XmlElement)
            Me.New()
            If Not IsValidForDeserialization(e, Me.Type) Then setFailed(WebRequestResultCode.Request_SerializationError, "Xml element content was invalid!")
            If e.HasAttribute("name") Then m_name = e.GetAttribute("name")

            GetTimeStampFromElem(e)

            Dim data As String = GetXmlElemRequestData(e)
            If Not String.IsNullOrEmpty(data) Then
                Me.success(data)
            Else
                setFailed(WebRequestResultCode.Request_Undefined, "No data for deserialization found!")
            End If
        End Sub
        ''' <summary>
        ''' Initializes an instance of this class by deserializing an XmlElement
        ''' </summary>
        ''' <param name="e">The data.</param>
        ''' <returns>
        ''' If successful, a ArtistMetadataRequest is returned, else <c>null</c>
        ''' </returns>
        Public Shared Function InitFromXmlElement(ByVal e As XmlElement) As AlbumMetaDataRequest
            Dim result As AlbumMetaDataRequest = Nothing

            If (IsValidForDeserialization(e, RequestType.AlbumMetaData)) Then
                result = New AlbumMetaDataRequest(e)
            End If
            Return result
        End Function
        ''' <summary>
        ''' Starts the request and gets the response.
        ''' </summary>
        Public Overrides Sub Start()
            Me.get("/1.0/album/" & EscapeUriData(m_artist) & "/" & EscapeUriData(m_album) & "/info.xml")
        End Sub

        ''' <summary>
        ''' Is called if the request was successful.
        ''' </summary>
        ''' <param name="data">The data.</param>
        Protected Overrides Sub success(ByVal data As String)
            Dim doc As New XmlDocument
            doc.LoadXml(data)
            Dim docelem As XmlElement = doc.DocumentElement
            m_artist = IIf(docelem.HasAttribute("artist"), docelem.Attributes("artist").Value, m_artist)


            m_album = IIf(docelem.HasAttribute("title"), docelem.Attributes("title").Value, m_album)
            With docelem
                m_PlayCount = CInt(.Item("reach").InnerText)
                m_AlbumUrl = IIf(String.IsNullOrEmpty(.Item("url").InnerText), Nothing, New Uri(.Item("url").InnerText))
                Dim relDateStr As String = .Item("releasedate").InnerText
                If String.IsNullOrEmpty(relDateStr) Then
                    m_releaseDate = Nothing
                Else
                    relDateStr = relDateStr.Replace(" 0 ", "1.").Trim
                    relDateStr.Replace(" ", "")
                    If relDateStr.Substring(0, relDateStr.IndexOf(",")).Length = 7 Then relDateStr = "1." & relDateStr
                    m_releaseDate = Date.Parse(relDateStr)
                End If

                Dim ca As XmlNode = .Item("coverart")
                With ca
                    m_CoverSmall = IIf(String.IsNullOrEmpty(.Item("small").InnerText), Nothing, New Uri(.Item("small").InnerText))
                    m_CoverMedium = IIf(String.IsNullOrEmpty(.Item("medium").InnerText), Nothing, New Uri(.Item("medium").InnerText))
                    m_CoverLarge = IIf(String.IsNullOrEmpty(.Item("large").InnerText), Nothing, New Uri(.Item("large").InnerText))
                End With
                m_mbId = .Item("mbid").InnerText

            End With
            Dim tracks As XmlNodeList = docelem.SelectNodes("tracks/track")
            For i As Integer = 0 To tracks.Count - 1
                With tracks(i)
                    Dim t As New TypeClasses.TrackInfo()
                    t.ArtistName = m_artist
                    t.Album = m_album
                    t.Title = tracks(i).Attributes("title").Value
                    t.PlayCount = CInt(tracks(i).Item("reach").InnerText)
                    t.TrackUrl = IIf(String.IsNullOrEmpty(tracks(i).Item("url").InnerText), Nothing, New Uri(tracks(i).Item("url").InnerText))
                    t.TrackNumber = i + 1
                    m_tracks.Add(t)
                End With
            Next
        End Sub



    End Class
End Namespace