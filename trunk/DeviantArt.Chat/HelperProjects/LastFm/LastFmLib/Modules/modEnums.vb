Namespace WebRequests
    Public Module modEnums
        Public Enum RequestType
            Handshake
            ChangeStation
            GetXspfPlaylist
            SetTag
            WebService
            Skip
            ArtistMetaData
            AlbumMetaData
            TrackMetaData
            VerifyUser
            EnableScrobbling
            EnableDiscoveryMode
            UnListen
            LoveTrack
            UnLoveTrack
            BanTrack
            'Public
            UnBanTrack
            AddTrackToMyPlaylist
            Friends
            UserPictures
            RecentTracks
            RecentlyBannedTracks
            RecentlyLovedTracks
            Neighbours
            SimilarArtists
            DeleteFriend
            Recommend
            UserPicturesRequest
            ReportRebuffering
            RadioMetaData
            UserTags
            UserArtistTags
            UserAlbumTags
            UserTrackTags
            SimilarTags
            SearchTag
            ArtistTags
            TopTags
            TopArtists
            TrackToId
        End Enum

        Public Enum TagMode
            TAG_OVERWRITE = 0
            TAG_APPEND
        End Enum
        Enum ItemType
            ItemArtist = 1
            ItemTrack
            ItemAlbum
            ItemTag
            ItemUser
            ItemStation
            ItemUnknown
        End Enum

        Enum TopXTimeSpan
            None
            ThreeMonths
            SixMonths
            TwelveMonths
        End Enum
    End Module
End Namespace
Namespace TypeClasses
    Public Module Enums
        Public Enum sortType
            WeightAscending
            WeightDescending
            NameAscending
            NameAscendingCaseSensitive
            NameDescending
            NameDescendingCaseSensitive
        End Enum
    End Module
End Namespace
