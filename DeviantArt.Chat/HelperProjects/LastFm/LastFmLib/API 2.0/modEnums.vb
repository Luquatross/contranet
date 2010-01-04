Namespace API20
    Public Module modEnums
        Public Structure TasteOMeterData
            ''' <summary>
            ''' 
            ''' </summary>
            Private m_type As TasteOMeterType

            Private m_value As String
            ''' <summary>
            ''' Gets or sets the TastOmeter data value. Can be a Last.fm username, a MySpace profile url or a comma-seperated string list of artists
            ''' </summary>
            ''' <value>The value. Can be a Last.fm username, a MySpace profile url or a comma-seperated string list of artists</value>
            Public Property Value() As String
                Get
                    Return m_value
                End Get
                Set(ByVal value As String)
                    m_value = value
                End Set
            End Property
            
            ''' <summary>
            ''' Gets or sets the type of Taste-O-Meter data
            ''' </summary>
            ''' <value>The type.</value>
            Property Type() As TasteOMeterType
                Get
                    Return m_type
                End Get
                Set(ByVal value As TasteOMeterType)
                    m_type = value
                End Set
            End Property


        End Structure
        ''' <summary>
        ''' Type of Taste-O-Meter data
        ''' </summary>
        Public Enum TasteOMeterType
            ''' <summary>
            ''' 
            ''' </summary>
            User
            ''' <summary>
            ''' 
            ''' </summary>
            Artists
            ''' <summary>
            ''' 
            ''' </summary>
            MySpace
        End Enum

        Public Enum ChartPeriod
            Overall
            ThreeMonths
            SixMonths
            TwelveMonths
        End Enum
        Public Enum RequestMode
            Rest
            XmlRpc
        End Enum
        Public Enum RequestAccessMode
            Read
            Write
        End Enum
        Public Enum RequestType
            Unknown = 0
            'Album
            AlbumAddTags
            AlbumGetInfo
            AlbumRemoveTag

            'Artist
            ArtistAddTags
            ArtistGetEvents
            ArtistGetInfo
            ArtistGetSimilar
            ArtistGetTopAlbums
            ArtistGetTopFans
            ArtistGetTopTags
            ArtistGetTopTracks
            ArtistRemoveTag
            ArtistSearch
            ArtistShare

            'Auth
            AuthGetMobileSession
            AuthGetSession
            AuthGetToken

            'Event
            EventAttend
            EventGetInfo

            'Geo
            GeoGetEvents
            GeoGetTopArtists
            GeoGetTopTracks

            'Group
            GroupGetWeeklyAlbumChart
            GroupGetWeeklyArtistChart
            GroupGetWeeklyChartList
            GroupGetWeeklyTrackChart

            'Playlist
            PlaylistFetch

            'Tag
            TagGetSimilar
            TagGetTopAlbums
            TagGetTopArtists
            TagGetTopTags
            TagGetTopTracks
            TagSearch

            'Tasteometer
            TasteOMeterCompare

            'Track
            TrackAddTags
            TrackBan
            TrackGetSimilar
            TrackGetTopFans
            TrackGetTopTags
            TrackLove
            TrackRemoveTag
            TrackSearch
            TrackShare

            'User
            UserGetEvents
            UserGetFriends
            UserGetNeighbours
            UserGetPlaylists
            UserGetRecentTracks
            UserGetTopAlbums
            UserGetTopArtists
            UserGetTopTags
            UserGetTopTracks
            UserGetWeeklyAlbumChart
            UserGetWeeklyArtistChart
            UserGetWeeklyChartList
            UserGetWeeklyTrackChart


        End Enum
        Public Enum FailureCode
            NoError
            UnknownError = 1
            ''' <summary>
            ''' Invalid service -This service does not exist
            ''' </summary>
            ''' <remarks></remarks>
            InvalidService = 2
            ''' <summary>
            ''' Invalid Method - No method with that name in this package
            ''' </summary>
            ''' <remarks></remarks>
            InvalidMethod = 3
            ''' <summary>
            ''' Authentication Failed - You do not have permissions to access the service
            ''' </summary>
            ''' <remarks></remarks>
            AuthenticationFailed = 4
            ''' <summary>
            ''' Invalid format - This service doesn't exist in that format
            ''' </summary>
            ''' <remarks></remarks>
            InvalidFormat = 5
            ''' <summary>
            ''' Invalid parameters - A parameter is missing or invalid.
            ''' </summary>
            ''' <remarks></remarks>
            InvalidParameters = 6
            ''' <summary>
            ''' Invalid resource specified
            ''' </summary>
            ''' <remarks></remarks>
            InvalidResource = 7

            ''' <summary>
            ''' The request result was empty, e.g. because of a non-existing artist/album/track/tag.
            ''' </summary>
            ''' <remarks></remarks>
            EmptyResult = 8
            ''' <summary>
            ''' Invalid session key - Please re-authenticate
            ''' </summary>
            ''' <remarks></remarks>
            InvalidSessionKey = 9
            ''' <summary>
            ''' Invalid API key - You must be granted a valid key by last.fm
            ''' </summary>
            ''' <remarks></remarks>
            InvalidApiKey = 10
            ''' <summary>
            ''' Service Offline - This service is temporarily offline. Try again later.
            ''' </summary>
            ''' <remarks></remarks>
            ServiceOffline = 11
            ''' <summary>
            ''' This service is only available to paid last.fm subscribers
            ''' </summary>
            ''' <remarks></remarks>
            SubscribersOnly = 12
            ''' <summary>
            ''' Invalid method signature supplied
            ''' </summary>
            ''' <remarks></remarks>
            InvalidMethodSignature
            ''' <summary>
            ''' Invalid authentication token. Please check username/password supplied
            ''' </summary>
            ''' <remarks></remarks>
            InvalidAuthenticationToken = 14
            ''' <summary>
            ''' The token has expired.
            ''' </summary>
            ''' <remarks></remarks>
            TokenExpired = 15
            ''' <summary>
            ''' The data returned from the server were invalid.
            ''' </summary>
            ''' <remarks>Custom error.</remarks>
            InvalidDataReturned
        End Enum
        Enum EventAttendanceStatus
            Attending = 0
            MaybeAttending = 1
            NotAttending = 2
        End Enum
    End Module
End Namespace
