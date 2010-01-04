Imports System.Net
Imports LastFmLib.Utils
Namespace TypeClasses
    ''' <summary>
    ''' A type for metadata of a track, such as artist,album,track name, url, MB-ID, url for buying, image, ... Just take a look at the tons of propertys.
    ''' </summary>
    Public Class MetaData
        Inherits TypeClasses.TrackInfo
        'private:

        ' Track data
        Private m_trackTags As New List(Of String)
        Private m_trackUrl As String
        Private m_buyTrackString As String
        Private m_buyTrackUrl As String

        ' Album data
        Private m_albumPicUrl As Uri
        Private m_albumUrl As String
        Private m_label As String
        Private m_releaseDate As Date
        Private m_numTracks As Integer
        Private m_buyAlbumString As String
        Private m_buyAlbumUrl As String

        ' Artist data
        Private m_artistTags As New List(Of String)
        Private m_similarArtists As New List(Of String)
        Private m_wiki As String
        Private m_artistPicUrl As Uri
        Private m_artistUrl As String
        Private m_wikiUrl As String
        Private m_topFans As New List(Of String)
        Private m_numListeners As Integer
        Private m_numPlays As Integer
#Region "New"
        ''' <summary>
        ''' Initializes a new instance of the <see cref="MetaData" /> class.
        ''' </summary>
        Public Sub New()
            MyBase.New()
        End Sub
        ''' <summary>
        ''' Initializes a new instance of the <see cref="MetaData" /> class.
        ''' </summary>
        ''' <param name="e">The XML element to use.</param>


        Public Sub New(ByRef e As Xml.XmlElement)
            MyBase.New(e)

            m_numListeners = Itemtext(e, "listeners")
        End Sub
#End Region
#Region "Propertys"
        ''' <summary>
        ''' Returns if the track is buyable.
        ''' </summary>

        ReadOnly Property IsTrackBuyable()
            Get
                Return (Not (String.IsNullOrEmpty(buyTrackUrl)))
            End Get
        End Property
        ''' <summary>
        ''' Returns if the track is buyable.
        ''' </summary>

        ReadOnly Property IsAlbumBuyable()
            Get
                Return (Not (String.IsNullOrEmpty(buyAlbumUrl)))
            End Get
        End Property

        ''' <summary>
        ''' Gets or sets the tags list for the current track
        ''' </summary>
        ''' <value>The track tags.</value>
        Property trackTags() As List(Of String)
            Get
                Return m_trackTags
            End Get
            Set(ByVal value As List(Of String))
                m_trackTags = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the track page URL.
        ''' </summary>
        ''' <value>The track page URL.</value>
        Property trackPageUrl() As String
            Get
                Return m_trackUrl
            End Get
            Set(ByVal value As String)
                m_trackUrl = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the buy track string(often things like "Buy on Amazon.com").
        ''' </summary>
        ''' <value>The buy track string.</value>
        Property buyTrackString() As String
            Get
                Return m_buyTrackString
            End Get
            Set(ByVal value As String)
                m_buyTrackString = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the url where you can buy the track
        ''' </summary>
        ''' <value>The buy track URL.</value>
        Property buyTrackUrl() As String
            Get
                Return m_buyTrackUrl
            End Get
            Set(ByVal value As String)
                m_buyTrackUrl = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the album picture URL.
        ''' </summary>
        ''' <value>The album pic URL.</value>
        Property albumPicUrl() As Uri
            Get
                Return m_albumPicUrl
            End Get
            Set(ByVal value As Uri)
                m_albumPicUrl = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the album page URL.
        ''' </summary>
        ''' <value>The album page URL.</value>
        Property albumPageUrl() As String
            Get
                Return m_albumUrl
            End Get
            Set(ByVal value As String)
                m_albumUrl = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the label.
        ''' </summary>
        ''' <value>The label.</value>
        Property Label() As String
            Get
                Return m_label
            End Get
            Set(ByVal value As String)
                m_label = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the release date.
        ''' </summary>
        ''' <value>The release date.</value>
        Property releaseDate() As Date
            Get
                Return m_releaseDate
            End Get
            Set(ByVal value As Date)
                m_releaseDate = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the number of track in the current album.
        ''' </summary>
        ''' <value>The num tracks.</value>
        Property numTracks() As Integer
            Get
                Return m_numTracks
            End Get
            Set(ByVal value As Integer)
                m_numTracks = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the "buy album" string.
        ''' </summary>
        ''' <value>The buy album string.</value>
        Property buyAlbumString() As String
            Get
                Return m_buyAlbumString
            End Get
            Set(ByVal value As String)
                m_buyAlbumString = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the "buy album" URL.
        ''' </summary>
        ''' <value>The buy album URL.</value>
        Property buyAlbumUrl() As String
            Get
                Return m_buyAlbumUrl
            End Get
            Set(ByVal value As String)
                m_buyAlbumUrl = value
            End Set
        End Property

        ' Artist methods
        Property ArtistTags() As List(Of String)
            Get
                Return m_artistTags
            End Get
            Set(ByVal value As List(Of String))
                m_artistTags = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the similar artist list.
        ''' </summary>
        ''' <value>The similar artists list.</value>
        Property similarArtists() As List(Of String)
            Get
                Return m_similarArtists
            End Get
            Set(ByVal value As List(Of String))
                m_similarArtists = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the artist picture URL.
        ''' </summary>
        ''' <value>The artist pic URL.</value>
        Property artistPicUrl() As Uri
            Get
                Return m_artistPicUrl
            End Get
            Set(ByVal value As Uri)
                m_artistPicUrl = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the wiki text
        ''' </summary>
        ''' <value>The wiki text.</value>
        Property Wiki() As String
            Get
                Return m_wiki
            End Get
            Set(ByVal value As String)
                m_wiki = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the wiki page URL.
        ''' </summary>
        ''' <value>The wiki page URL.</value>
        Property wikiPageUrl() As String
            Get
                Return m_wikiUrl
            End Get
            Set(ByVal value As String)
                m_wikiUrl = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the artist page URL.
        ''' </summary>
        ''' <value>The artist page URL.</value>
        Property artistPageUrl() As String
            Get
                Return m_artistUrl
            End Get
            Set(ByVal value As String)
                m_artistUrl = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the top fans list.
        ''' </summary>
        ''' <value>The top fans list.</value>
        Property topFans() As List(Of String)
            Get
                Return m_topFans
            End Get
            Set(ByVal value As List(Of String))
                m_topFans = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the number of listeners.
        ''' </summary>
        ''' <value>The num listeners.</value>
        Property numListeners() As Integer
            Get
                Return m_numListeners
            End Get
            Set(ByVal value As Integer)
                m_numListeners = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the number of plays.
        ''' </summary>
        ''' <value>The num plays.</value>
        Property numPlays() As Integer
            Get
                Return m_numPlays
            End Get
            Set(ByVal value As Integer)
                m_numPlays = value
            End Set
        End Property
#End Region
        ''' <summary>
        ''' Combines this instance with another instatance of the MetaData class
        ''' </summary>
        ''' <param name="md">The meta data</param>
        ''' <returns>All propertys of this instance which are empty are filled with the ones of the parameter's instance.</returns>
        Public Overloads Function MergeWith(ByVal md As MetaData) As MetaData
            If md Is Nothing Then Return Me.Clone
            md = md.Clone
            Dim result As MetaData = Me.Clone
            '###############################################################
            'From TrackInfo, it's double code but I dunno how to do better #
            '###############################################################
            With result
                If String.IsNullOrEmpty(.Album) Then .Album = md.Album

                If String.IsNullOrEmpty(.ArtistName) Then .ArtistName = md.ArtistName

                If String.IsNullOrEmpty(.AuthCode) Then .AuthCode = md.AuthCode

                If .DownloadUris.Count = 0 Then
                    .SetPaths(md.DownloadUriStrings)
                End If

                If .Duration = 0 Then .Duration = md.Duration
                If String.IsNullOrEmpty(.FileName) Then .FileName = md.FileName

                If String.IsNullOrEmpty(.mbId) Then .mbId = md.mbId

                If String.IsNullOrEmpty(.Path) Then .Path = md.Path
                If .PlayCount = 0 Then .PlayCount = md.PlayCount
                If String.IsNullOrEmpty(.PlayerId) Then .PlayerId = md.PlayerId

                If .Source = Nothing Then .Source = md.Source
                If .TimeStamp = Nothing Then .TimeStamp = md.TimeStamp
                If String.IsNullOrEmpty(.Title) Then .Title = md.Title
            End With

            '#######################
            'End of TrackInfo part #
            '#######################

            With result

                If .albumPageUrl Is Nothing Then .albumPageUrl = md.albumPageUrl
                If .albumPicUrl Is Nothing Then .albumPicUrl = md.albumPicUrl

                If .artistPageUrl Is Nothing Then .artistPageUrl = md.artistPageUrl
                If .artistPicUrl Is Nothing Then .artistPicUrl = md.artistPicUrl
                If .ArtistTags.Count = 0 Then .ArtistTags = md.ArtistTags
                If String.IsNullOrEmpty(.AuthCode) Then .AuthCode = md.AuthCode
                If String.IsNullOrEmpty(.buyAlbumString) Then .buyAlbumString = md.buyAlbumString
                If .buyAlbumUrl Is Nothing Then .buyAlbumUrl = md.buyAlbumUrl
                If String.IsNullOrEmpty(.buyTrackString) Then .buyTrackString = md.buyTrackString
                If .buyTrackUrl Is Nothing Then .buyTrackUrl = md.buyTrackUrl

                If String.IsNullOrEmpty(.Label) Then .Label = md.Label

                If .numListeners = 0 Then .numListeners = md.numListeners
                If .numPlays = 0 Then .numPlays = md.numPlays
                If .numTracks = 0 Then .numTracks = md.numTracks

                If .releaseDate = Nothing Then .releaseDate = md.releaseDate
                If .similarArtists.Count = 0 Then .similarArtists = md.similarArtists

                If .topFans.Count = 0 Then .topFans = md.topFans
                If .trackPageUrl Is Nothing Then .trackPageUrl = md.trackPageUrl
                If .trackTags.Count = 0 Then .trackTags = md.trackTags
                If String.IsNullOrEmpty(.UniqueID) Then .trackTags = md.trackTags
                If String.IsNullOrEmpty(.Username) Then .Username = md.Username
                If String.IsNullOrEmpty(.Wiki) Then .Wiki = md.Wiki
                If .wikiPageUrl Is Nothing Then .wikiPageUrl = md.wikiPageUrl
                If .Wiki Is Nothing Then .Wiki = md.Wiki
            End With
            Return result
        End Function
        ''' <summary>
        ''' Converts this instance to an HTML string. If viewing it in the browser, the main information about the current album are given.
        ''' EXPERIMENTAL!
        ''' </summary>
        ''' <returns>A string containing the HTML</returns>
        Public Function ToHTMLString() As String
            Dim result As String
            With Me
                result = "<html><head><title>Info of Album " & .Album & " by " & .ArtistName & "</title></head>"
                'result &= "<body>" & IIf(.albumPicUrl IsNot Nothing, "<img src='" & .albumPicUrl.AbsoluteUri & "' alt='No Albumcover'/>", "") & "<br>"
                result &= "Album: " & IIf(String.IsNullOrEmpty(.Album), "(none)", "<a href='" & .albumPageUrl & "'>" & .Album) & "</a>"
                result &= "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<a href='" & .buyAlbumUrl & "'>(buy)</a><br>"

                result &= "Artist: " & IIf(String.IsNullOrEmpty(.ArtistName), "(none)", "<a href='" & .artistPageUrl & "'>" & .ArtistName) & "</a><br>"
                result &= "Tracks Total: " & .numTracks & "<br>"
                result &= "<u><a href='" & .wikiPageUrl & "'>Artist Biography</a></u><br>" & .Wiki
                result &= "</body></html>"
            End With
            Return result
        End Function
        Public Overrides Function ToTrackInfo() As TrackInfo
            Return Me
        End Function
        Public Overrides Function Clone() As Object
            Dim m As MetaData = Me.MemberwiseClone
            m.m_url = CloneUrl(m_url)
            m.m_paths = CloneList(Of String)(m_paths)
            m.m_InvalidArtists = InitInvalid()
            m.m_trackTags = CloneList(Of String)(m_trackTags)

            m.m_albumPicUrl = CloneUrl(m_albumPicUrl)
            m.m_artistTags = CloneList(Of String)(m_artistTags)

            m.m_similarArtists = CloneList(Of String)(m_similarArtists)


            m.m_artistPicUrl = CloneUrl(m_artistPicUrl)
            m.m_topFans = CloneList(Of String)(m_topFans)
            Return m
        End Function
    End Class
End Namespace