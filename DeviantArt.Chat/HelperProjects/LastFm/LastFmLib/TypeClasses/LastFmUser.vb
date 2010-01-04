Namespace TypeClasses
    ''' <summary>
    ''' Holds the Informations/Settings of a user.
    ''' </summary>
    Public Class LastFmUser
        Dim m_Username As String
        Dim m_PasswordMD5 As String
        Dim m_scobblePoint As Integer
        Dim m_sessionID As String
        Dim m_scrobblerSession As String
        ''' <summary>
        ''' Initializes a new instance of the <see cref="LastFmUser" /> class.
        ''' </summary>
        Public Sub New()
            m_Username = ""
            m_PasswordMD5 = ""
            m_scobblePoint = 50
        End Sub
        Public Sub New(ByVal username As String, ByVal passwordMD5 As String, Optional ByVal scrobblePoint As Integer = 50)
            m_Username = username
            m_PasswordMD5 = passwordMD5.ToLower
            m_scobblePoint = scrobblePoint
        End Sub
        Public Property Username() As String
            Get
                Return m_Username
            End Get
            Set(ByVal value As String)
                m_Username = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the password. If setting it, the PasswordMD5 property will be set automatically, too.
        ''' </summary>
        ''' <value>The password.</value>
        Public WriteOnly Property Password() As String
            'Get
            '    Return m_Password
            'End Get
            Set(ByVal value As String)
                If Not String.IsNullOrEmpty(value) Then m_PasswordMD5 = GenerateHash(value).ToLower
            End Set
        End Property
        ReadOnly Property hasPassword()
            Get
                Return Not String.IsNullOrEmpty(m_PasswordMD5)
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the scrobble point.
        ''' </summary>
        ''' <value>The scrobble point.</value>
        Property scrobblePoint() As Integer
            Get
                Return m_scobblePoint
            End Get
            Set(ByVal value As Integer)
                m_scobblePoint = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the password Md5.
        ''' </summary>
        ''' <value>The password Md5.</value>
        ''' <exception cref="ArgumentOutOfRangeException">When setting the session ID, this exception is thrown of the session id has not 32 chars length.</exception>
        Public Property PasswordMD5() As String
            Get
                Return m_PasswordMD5
            End Get
            Set(ByVal value As String)
                Dim m_tmp As String = m_PasswordMD5
                m_PasswordMD5 = value.ToLower
                If Not ValidatePasswordMD5() Then
                    m_PasswordMD5 = m_tmp
                    Throw New ArgumentOutOfRangeException("value", "The length of the password MD5 MUST be 32!")
                End If
            End Set
        End Property
        Public ReadOnly Property hasSessionID() As Boolean
            Get
                Return Not String.IsNullOrEmpty(m_sessionID)
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the session ID.
        ''' </summary>
        ''' <value>The session ID.</value>
        ''' <exception cref="ArgumentOutOfRangeException">When setting the session ID, this exception is thrown of the session id has not 32 chars length.</exception>
        Public Property SessionID() As String
            Get
                Return m_sessionID
            End Get
            Set(ByVal value As String)
                If value.Length = 32 Then
                    m_sessionID = value
                Else
                    If Not value = "FAILED" Then
                        'Invalid session id length
                        Throw New ArgumentOutOfRangeException("value", "The length of the session ID MUST be 32!")
                    End If
                    
                End If
            End Set
        End Property

        ''' <summary>
        ''' Validates the username and the password MD5
        ''' </summary>
        ''' <returns></returns>
        Public Overridable Function ValidateUsernameAndPasswordMD5() As Boolean
            Dim result As Boolean = True
            result = ValidateUser()
            result = result And ValidatePasswordMD5()
            Return result
        End Function

        ''' <summary>
        ''' Prüft ob der Benutzername gültig ist. Wenn ja, wird True zurückgegeben.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ValidateUser() As Boolean
            If String.IsNullOrEmpty(m_Username) Then Return False
            Return True
        End Function


        ''' <summary>
        ''' Prüft ob der Passwort-MD5 gültig ist. Wenn ja, wird True zurückgegeben.
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function ValidatePasswordMD5() As Boolean
            Return Not (String.IsNullOrEmpty(m_PasswordMD5.Trim) Or PasswordMD5.Length <> 32)
        End Function

        Public Function ToXmlElement(ByRef doc As Xml.XmlDocument) As Xml.XmlElement
            Dim e As Xml.XmlElement = doc.CreateElement("user")
            Dim a As Xml.XmlAttribute = doc.CreateAttribute("name")
            a.Value = m_Username
            e.Attributes.Append(a)
            a = doc.CreateAttribute("passhash")
            a.Value = m_PasswordMD5
            e.Attributes.Append(a)
            a = doc.CreateAttribute("scrobPoint")
            a.Value = m_scobblePoint
            e.Attributes.Append(a)

            Return e
        End Function
        Public Function DeleteFriend(ByVal username As String) As Boolean
            Dim dfr As New WebRequests.DeleteFriendRequest(Me, username)
            dfr.Start()
            Return dfr.succeeded
        End Function
        ''' <summary>
        ''' Gets the friends list - including ther meta data - of this user.
        ''' </summary>
        ''' <returns>If successful, a list of TypeClasses.UserMetaData is returned, else Nothing</returns>
        Public Function GetFriends() As List(Of UserMetaData)
            Dim fr As New WebRequests.FriendsRequest(Me.Username)
            fr.Start()
            If fr.succeeded Then Return fr.metaDatas
            'Comment above, uncomment below and change the list type to string to get only a name list
            'If fr.succeeded Then Return fr.FriendUsernames
            Return Nothing
        End Function

        ''' <summary>
        ''' Gets the neighbours as a TypeClasses.WeightedStringLisr
        ''' </summary>
        ''' <returns>If successful, a WeightedStringList is returned, otherwise Nothing</returns>
        Public Function GetNeighbours() As WeightedStringList
            Dim nr As New WebRequests.NeighboursRequest(Me.Username)
            nr.Start()
            If nr.succeeded Then Return nr.NeighbourUsernames
            Return Nothing
        End Function
        ''' <summary>
        ''' Gets the neighbours as a TypeClasses.WeightedStringLisr
        ''' </summary>
        ''' <returns>If successful, a List(Of Track) is retured, otherwise Nothing</returns>
        Public Function GetRecentlyListenedTracks() As List(Of Track)
            Dim rt As New WebRequests.RecentTracksRequest(Me.Username)
            rt.Start()
            If rt.succeeded Then Return rt.TrackList
            Return Nothing
        End Function


        ''' <summary>
        ''' Gets the recently loved tracks.
        ''' </summary>
        ''' <returns>
        ''' If succesfull, a List(of Track) is returned, otherwise Nothing
        ''' </returns>
        Public Function GetRecentlyLovedTracks() As List(Of Track)
            Dim rlt As New WebRequests.RecentlyLovedTracksRequest(Me.Username)
            rlt.Start()
            If rlt.succeeded Then Return rlt.TrackList
            Return Nothing
        End Function

        ''' <summary>
        ''' Gets the recently banned tracks.
        ''' </summary>
        ''' <returns>
        ''' If succesfull, a List(of Track) is returned, otherwise Nothing
        ''' </returns>
        Public Function GetRecentlyBannedTracks() As List(Of Track)
            Dim rlt As New WebRequests.RecentlyBannedTracksRequest(Me.Username)
            rlt.Start()
            If rlt.succeeded Then Return rlt.TrackList
            Return Nothing
        End Function

        ''' <summary>
        ''' Recommends an artist to a user
        ''' </summary>
        ''' <param name="recommendToUsername">The recommend to username.</param>
        ''' <param name="artist">The artist name.</param>
        ''' <param name="message">The message.</param>
        ''' <returns>
        ''' <c>true</c> if successful, else <c>false</c>
        ''' </returns>
        Public Function RecommendArtist(ByVal recommendToUsername As String, ByVal artist As String, ByVal message As String) As Boolean
            Dim rt As New WebRequests.RecommendRequest(Me, recommendToUsername, New TrackInfo(artist, "", ""), WebRequests.RecommendRequest.ItemType.ItemTrack, message)
            rt.Start()
            Return rt.succeeded
        End Function
        ''' <summary>
        ''' Recommends a track to a user
        ''' </summary>
        ''' <param name="recommendToUsername">The recommend to username.</param>
        ''' <param name="track">The track.</param>
        ''' <param name="message">The message.</param>
        ''' <returns>
        ''' <c>true</c> if successful, else <c>false</c>
        ''' </returns>
        Public Function RecommendAlbum(ByVal recommendToUsername As String, ByVal track As TrackInfo, ByVal message As String) As Boolean
            Dim rt As New WebRequests.RecommendRequest(Me, recommendToUsername, track, WebRequests.RecommendRequest.ItemType.ItemAlbum, message)
            rt.Start()
            Return rt.succeeded
        End Function

        ''' <summary>
        ''' Recommends a track to a user
        ''' </summary>
        ''' <param name="recommendToUsername">The recommend to username.</param>
        ''' <param name="track">The TrackInfo instance containing artist and track</param>
        ''' <param name="message">The message.</param>
        ''' <returns>
        ''' <c>true</c> if successful, else <c>false</c>
        ''' </returns>
        Public Overloads Function RecommendTrack(ByVal recommendToUsername As String, ByVal track As TrackInfo, ByVal message As String) As Boolean
            Dim rt As New WebRequests.RecommendRequest(Me, recommendToUsername, track, WebRequests.RecommendRequest.ItemType.ItemTrack, message)
            rt.Start()
            Return rt.succeeded
        End Function
        ''' <summary>
        ''' Recommends a track to a user
        ''' </summary>
        ''' <param name="recommendToUsername">The recommend to username.</param>
        ''' <param name="track">The track.</param>
        ''' <param name="message">The message.</param>
        ''' <returns>
        ''' <c>true</c> if successful, else <c>false</c>
        ''' </returns>
        Public Overloads Function RecommendTrack(ByVal recommendToUsername As String, ByVal track As Track, ByVal message As String) As Boolean
            Return RecommendTrack(recommendToUsername, track.ToTrackInfo, message)
        End Function
        ''' <summary>
        ''' Tags an artist with a list of tags.
        ''' </summary>
        ''' <param name="artist">The artist.</param>
        ''' <param name="tags">The tags.</param>
        ''' <param name="mode">The tagging mode(append tags or overwrite them).</param>
        ''' <returns>
        ''' <c>true</c> if successful, else <c>false</c>
        ''' </returns>
        Public Overloads Function TagArtist(ByVal artist As String, ByRef tags As List(Of String), _
                                            Optional ByVal mode As WebRequests.TagMode = WebRequests.TagMode.TAG_APPEND) As Boolean
            Dim track As New TrackInfo(artist, "", "")
            Dim str As New WebRequests.SetTagRequest(Me, track, WebRequests.ItemType.ItemArtist, tags)
            str.Start()
            Return str.succeeded
        End Function
        ''' <summary>
        ''' Tags an artist with a list of tags.
        ''' </summary>
        ''' <param name="track">The TrackInfo containg the artist name.</param>
        ''' <param name="tags">The tags.</param>
        ''' <param name="mode">The tagging mode(append tags or overwrite them).</param>
        ''' <returns>
        ''' <c>true</c> if successful, else <c>false</c>
        ''' </returns>
        Public Overloads Function TagArtist(ByVal track As TrackInfo, ByRef tags As List(Of String), Optional ByVal mode As WebRequests.TagMode = WebRequests.TagMode.TAG_APPEND) As Boolean
            Dim t As New WebRequests.SetTagRequest(Me, track, WebRequests.ItemType.ItemArtist, tags)
            t.Start()
        End Function
        ''' <summary>
        ''' Tags an album with a list of tags.
        ''' </summary>
        ''' <param name="track">The TrackInfo containg the artist and the album name.</param>
        ''' <param name="tags">The tags.</param>
        ''' <param name="mode">The tagging mode(append tags or overwrite them).</param>
        ''' <returns>
        ''' <c>true</c> if successful, else <c>false</c>
        ''' </returns>
        Public Function TagAlbum(ByVal track As TrackInfo, ByRef tags As List(Of String), _
                                 Optional ByVal mode As WebRequests.TagMode = WebRequests.TagMode.TAG_APPEND) As Boolean
            Dim str As New WebRequests.SetTagRequest(Me, track, WebRequests.ItemType.ItemAlbum, tags)
            str.Start()
            Return str.succeeded
        End Function

        ''' <summary>
        ''' Tags a track with a list of tags.
        ''' </summary>
        ''' <param name="track">The TrackInfo containg the artist and the album name.</param>
        ''' <param name="tags">The tags.</param>
        ''' <param name="mode">The tagging mode(append tags or overwrite them).</param>
        ''' <returns>
        ''' <c>true</c> if successful, else <c>false</c>
        ''' </returns>
        Public Overloads Function TagTrack(ByVal track As TrackInfo, ByRef tags As List(Of String), _
                                 Optional ByVal mode As WebRequests.TagMode = WebRequests.TagMode.TAG_APPEND) As Boolean
            Dim str As New WebRequests.SetTagRequest(Me, track, WebRequests.ItemType.ItemTrack, tags)
            str.Start()
            Return str.succeeded
        End Function

        ''' <summary>
        ''' Tags an album with a list of tags.
        ''' </summary>
        ''' <param name="track">The Track instance containg the artist and the album name.</param>
        ''' <param name="tags">The tags.</param>
        ''' <param name="mode">The tagging mode(append tags or overwrite them).</param>
        ''' <returns>
        ''' <c>true</c> if successful, else <c>false</c>
        ''' </returns>
        Public Overloads Function TagTrack(ByVal track As Track, ByRef tags As List(Of String), _
                                 Optional ByVal mode As WebRequests.TagMode = WebRequests.TagMode.TAG_APPEND) As Boolean
            'Check out if track has to be explicitly converted into TrackInfo!
            Return Me.TagTrack(track.ToTrackInfo, tags, mode)
        End Function

        ''' <summary>
        ''' Gets the top listened artists.
        ''' </summary>
        ''' <param name="timeSpan">The time span(3, 6, 12 months or overall).</param>
        ''' <returns>
        ''' If successful, a WeightedArtistList is returned, else <c>null</c>
        ''' </returns>
        Public Function GetTopListenedArtists(Optional ByVal timeSpan As WebRequests.TopXTimeSpan = WebRequests.TopXTimeSpan.None) As WeightedArtistList
            Dim tar As New WebRequests.UserTopListenedArtistsRequest(Me.Username, timeSpan)
            tar.Start()
            If tar.succeeded Then Return tar.Artists
            Return Nothing
        End Function
        ''' <summary>
        ''' Gets the user's (personal) tags.
        ''' </summary>
        ''' <returns>
        ''' If successful, a WeightedStringList is returned, else <c>null</c>
        ''' </returns>
        Public Function GetUserTags() As WeightedStringList
            Dim ut As New WebRequests.UserTagsRequest(Me.Username)
            ut.Start()
            If ut.succeeded Then Return ut.tags
            Return Nothing
        End Function

        ''' <summary>
        ''' Gets the user's album tags.
        ''' </summary>
        ''' <param name="artistname">The artistname.</param>
        ''' <param name="album">The album.</param>
        ''' <returns>
        ''' If successful, a WeightedStringList is returned, else <c>null</c>
        ''' </returns>
        Public Function GetUserAlbumTags(ByVal artistname As String, ByVal album As String) As WeightedStringList
            Dim uat As New WebRequests.UserAlbumTagsRequest(Me.Username, artistname, album)
            uat.Start()
            If uat.succeeded Then Return uat.tags
            Return Nothing
        End Function
        ''' <summary>
        ''' Gets the user's artist tags.
        ''' </summary>
        ''' <param name="artistName">Name of the artist.</param>
        ''' <returns>
        ''' If successful, a WeightedStringList is returned, else <c>null</c>
        ''' </returns>
        Public Function GetUserArtistTags(ByVal artistName As String) As WeightedStringList
            Dim uat As New WebRequests.UserArtistTagsRequest(Me.Username, artistName)
            uat.Start()
            If uat.succeeded Then Return uat.tags
            Return Nothing
        End Function

        ''' <summary>
        ''' Gets the user's tags for a track.
        ''' </summary>
        ''' <param name="track">The track.</param>
        ''' <returns>
        ''' If successful, a WeightedStringList is returned, else <c>null</c>
        ''' </returns>
        Public Overloads Function GetUserTrackTags(ByVal track As Track) As WeightedStringList
            Dim utt As New WebRequests.UserTrackTagsRequest(Me.Username, track.ArtistName, track.Title)
            utt.Start()
            If utt.succeeded Then Return utt.tags
            Return Nothing
        End Function

        ''' <summary>
        ''' Gets the user's tags for a track.
        ''' </summary>
        ''' <param name="trackinf">The TrackInfo instance containing artist and track title.</param>
        ''' <returns>
        ''' If successful, a WeightedStringList is returned, else <c>null</c>
        ''' </returns>
        Public Overloads Function GetUserTrackTags(ByVal trackinf As TrackInfo) As WeightedStringList
            Dim utt As New WebRequests.UserTrackTagsRequest(Me.Username, trackinf.ArtistName, trackinf.Title)
            utt.Start()
            If utt.succeeded Then Return utt.tags
            Return Nothing
        End Function

        Public Function LoveTrack(ByVal t As Track) As Boolean
            Dim lt As New WebRequests.LoveRequest(t, Me)
            lt.Start()
            Return lt.succeeded
        End Function
        Public Function UnLoveTrack(ByVal t As Track) As Boolean
            Dim lt As New WebRequests.UnLoveRequest(t, Me)
            lt.Start()
            Return lt.succeeded
        End Function

        Public Function BanTrack(ByVal t As Track) As Boolean
            Dim lt As New WebRequests.BanRequest(t, Me)
            lt.Start()
            Return lt.succeeded
        End Function
        Public Function UnBanTrack(ByVal t As Track) As Boolean
            Dim lt As New WebRequests.unBanRequest(t, Me)
            lt.Start()
            Return lt.succeeded
        End Function

        Public Function AddTrackToPersonalPlaylist(ByVal t As Track) As Boolean
            Dim atr As New WebRequests.addTrackToUserPlaylistRequest(t, Me)
            atr.Start()
            Return atr.succeeded
        End Function
        'Don't know how to handle the VerifyUserRequest *ooops*, will come soon^^
        'Public Function VerifyUser() As WebRequests.VerifyUserRequest
        '    Dim vf As New v
        'End Function
    End Class
End Namespace