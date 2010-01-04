Imports LastFmLib.API20
Imports LastFmLib.API20.Types

Public Class frmMain

    'Change this to your own
    Shared API_KEY As New LastFmLib.MD5Hash(My.Resources.api_ressource.api_key)
    Shared API_SECRET As New LastFmLib.MD5Hash(My.Resources.api_ressource.api_secret)
    Shared SESSION_KEY As New LastFmLib.MD5Hash(My.Resources.api_ressource.session_key)
    Dim vars As New Dictionary(Of String, String)
    Private Sub lbVars_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lbVars.SelectedIndexChanged
        txtValue.Text = vars(lbVars.Text)
    End Sub


    Private Sub frmMain_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        Settings.AuthData = New AuthData(API_KEY, API_SECRET)
        Settings.AuthData.Session = New Session
        Settings.AuthData.Session.SessionKey = SESSION_KEY
        'Also Preset secret(uncomment)
        SetVarItems()
        For Each s As String In lbVars.Items
            vars.Add(s, "")
        Next
        PresetVars()
    End Sub
    Sub SetVarItems()
        With lbVars.Items
            Dim var As String() = New String() {"TOM_method", "user2", "session_key", "limit", "chart_from", "chart_to", "playlist_url", "country", "event_location", "event_distance", "user", "artist", "album", "track", "tag", "taglist", "eventid", "group", "api_key", "token", "secret", "artist_mbid", "track_mbid"}
            .AddRange(var)
        End With
    End Sub
    Sub PresetVars()
        ddRequestMethod.SelectedIndex = 0
        '50 km
        vars("event_distance") = 50
        vars("playlist_url") = "lastfm://playlist/artist/Cher"
        vars("event_location") = "London"
        vars("country") = "germany" 'Cause I come from there ;)
        vars("api_key") = LastFmLib.API20.Settings.AuthData.ApiKey.ToString
        vars("secret") = Settings.AuthData.ApiSecret
        vars("user") = "tburny"
        vars("tag") = "rock"
        vars("group") = "lastfmlib"
        vars("taglist") = "music i like,favourite"
        vars("artist") = "Cher"
        vars("album") = "Believe"
        vars("track") = "All or Nothing"
        vars("eventid") = "328799"
        vars("artist_mbid") = "bfcc6d75-a6a5-4bc6-8282-47aec8531818"
        'result limit for various requests
        vars("limit") = "5"
        vars("session_key") = Settings.AuthData.Session.SessionKey.ToString
        vars("user2") = "oern" 'Thanks for the nice hp :D
        vars("TOM_method") = "user"
    End Sub

    Private Sub cmdSet_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmdSet.Click
        If lbVars.Text = "api_key" Then Settings.AuthData.ApiKey = New LastFmLib.MD5Hash(txtValue.Text)
        If lbVars.Text = "api_secret" Then Settings.AuthData.ApiSecret = New LastFmLib.MD5Hash(txtValue.Text)
        If lbVars.Text = "session_key" Then
            Settings.AuthData.Session = New Session
            Settings.AuthData.Session.SessionKey = New LastFmLib.MD5Hash(txtValue.Text)
        End If

        vars(lbVars.Text) = txtValue.Text

    End Sub

    Private Sub cmdRun_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdRun.Click
        Debug.Print("=====================" & lbMethod.Text & "=====================")
        'Dim ResultStr As String = ""
        Dim rm As RequestMode = ddRequestMethod.SelectedIndex
        Dim rqName As String = lbMethod.Text
        Dim artist As String = vars("artist")
        Dim album As String = vars("album")
        Dim track As String = vars("track")
        Dim tag As String = vars("tag")
        Dim artistMbId As Guid = If(String.IsNullOrEmpty(vars("artist_mbid")), Nothing, New Guid(vars("artist_mbid")))
        Dim user As String = vars("user")
        Dim taglist As New List(Of String)(vars("taglist").Split(","))
        Dim eventid As Integer = CInt(vars("eventid"))
        Dim location As String = vars("event_location")
        Dim distance As String = vars("event_distance")
        Dim country As String = vars("country")
        Dim group As String = vars("group")
        Dim limit As Integer = vars("limit")
        Dim plUrl As Uri = Nothing
        Dim cInfo As New ChartInfo()
        Dim user2 As String = vars("user2")
        If Not String.IsNullOrEmpty(vars("chart_from")) Then
            cInfo.From = New LastFmLib.UnixTime(CLng(vars("chart_from")), LastFmLib.UnixTime.UnixTimeFormat.Seconds)
        End If
        If Not String.IsNullOrEmpty(vars("chart_to")) Then
            cInfo.From = New LastFmLib.UnixTime(CLng(vars("chart_to")), LastFmLib.UnixTime.UnixTimeFormat.Seconds)
        End If
        Uri.TryCreate(vars("playlist_url"), UriKind.RelativeOrAbsolute, plUrl)
        'Select Case lbMethod.Text.ToLower

        '    Case "auth.gettoken"
        '        'you can hand over an api_key, but request also can get it from Settings class
        '        Dim r As New Auth.AuthGetToken()
        '        r.RequestMode = rm
        '        r.Start()
        '        If r.succeeded Then
        '            Settings.AuthData.Token = r.AuthToken
        '            Debug.Print(r.AuthToken.ToString)
        '        Else
        '            Debug.Print(lbMethod.Text & " failed(" & r.FailureCode & "):" & r.errorMessage)
        '        End If
        '    Case "auth.getsession"

        '        Dim s As New Auth.AuthGetSession(Settings.AuthData.Token)
        '        's.AuthData = r.AuthData
        '        s.Start()
        '        If s.succeeded Then
        '            Debug.Print("Session key: " & s.Session.SessionKey.ToString)
        '        Else
        '            Debug.Print(lbMethod.Text & " failed(" & s.FailureCode & "):" & s.errorMessage)

        '            If s.FailureCode = 14 Then
        '                MsgBox("Plese request user auth!")
        '            End If

        '        End If
        '    Case "playlist.fetch"
        'End Select
        Select Case rqName.ToLower
            Case "album.addtags"
                Dim rq As New Album.AlbumAddTags(artist, album, taglist)
                rq.RequestMode = rm
                rq.Start()
                If rq.succeeded Then
                    Debug.Print("Tags " & String.Join(",", taglist.ToArray) & " added.")
                Else
                    Debug.Print("album.addTags failed(" & rq.FailureCode & "):" & rq.errorMessage)
                End If
            Case "album.getinfo"
                Dim rq As New Album.AlbumGetInfo(artist, album)
                rq.RequestMode = rm
                rq.Start()
                If rq.succeeded Then
                    Debug.Print(rq.Result.ToDebugString)
                Else
                    Debug.Print("album.getInfo failed(" & rq.FailureCode & "):" & rq.errorMessage)
                End If
            Case "album.removetag"
                Dim rq As New Album.AlbumRemoveTag(artist, album, tag)

                rq.RequestMode = rm
                rq.Start()
                If rq.succeeded Then
                    Debug.Print("Tag " & tag & " removed.")
                Else
                    Debug.Print("album.removeTag failed(" & rq.FailureCode & "):" & rq.errorMessage)
                End If
            Case "artist.addtags"
                Dim rq As New Artist.ArtistAddTags(artist, taglist)
                rq.RequestMode = rm
                rq.Start()
                If rq.succeeded Then
                    Debug.Print("Tags " & String.Join(",", taglist.ToArray) & " added.")
                Else
                    Debug.Print("artist.addTags failed(" & rq.FailureCode & "):" & rq.errorMessage)
                End If
            Case "artist.getevents"
                Dim rq As New Artist.ArtistGetEvents(artist)
                rq.RequestMode = rm
                rq.Start()
                If rq.succeeded Then
                    For Each m As MusicEvent In rq.Result
                        Debug.Print(m.ToDebugString)
                    Next
                Else
                    Debug.Print("artist.getEvents failed(" & rq.FailureCode & "):" & rq.errorMessage)
                End If
            Case "artist.getinfo"
                Dim rq As Artist.ArtistGetInfo = IIf(IsNothing(artistMbId), New Artist.ArtistGetInfo(artist), New Artist.ArtistGetInfo(artistMbId))

                rq.RequestMode = rm
                rq.Start()
                If rq.succeeded Then
                    Debug.Print(rq.Result.ToDebugString)
                Else
                    Debug.Print("artist.getInfo failed(" & rq.FailureCode & "):" & rq.errorMessage)
                End If
            Case "artist.getsimilar"
                Dim rq As New Artist.ArtistGetSimilar(artist)
                rq.RequestMode = rm
                rq.Start()
                If rq.succeeded Then
                    For Each a As ArtistInfo In rq.Result
                        Debug.Print(a.ToDebugString)
                    Next
                Else
                    Debug.Print("artist.getSimilar failed(" & rq.FailureCode & "):" & rq.errorMessage)
                End If
            Case "artist.gettopalbums"
                Dim rq As New Artist.ArtistGetTopAlbums(artist)
                rq.RequestMode = rm
                rq.Start()
                If rq.succeeded Then
                    For Each i As AlbumInfo In rq.Result
                        Debug.Print(i.ToDebugString)
                    Next
                Else
                    Debug.Print("artist.getTopAlbums failed(" & rq.FailureCode & "):" & rq.errorMessage)
                End If
            Case "artist.gettopfans"
                Dim rq As New Artist.ArtistGetTopFans(artist)
                rq.RequestMode = rm
                rq.Start()
                If rq.succeeded Then
                    For Each f As TopFan In rq.Result
                        Debug.Print(f.ToDebugString)
                    Next
                Else
                    Debug.Print("artist.getTopFans failed(" & rq.FailureCode & "):" & rq.errorMessage)
                End If
            Case "artist.gettoptags"
                Dim rq As New Artist.ArtistGetTopTags(artist)
                rq.RequestMode = rm
                rq.Start()
                If rq.succeeded Then
                    For Each t As KeyValuePair(Of String, Uri) In rq.Result
                        Debug.Print("Tag name: " & t.Key)
                        Debug.Print("Url: " & t.Value.AbsoluteUri)
                    Next
                Else
                    Debug.Print("artist.getTopTags failed(" & rq.FailureCode & "):" & rq.errorMessage)
                End If
            Case "artist.gettoptracks"
                Dim rq As New Artist.ArtistGetTopTracks(artist)
                rq.RequestMode = rm
                rq.Start()
                If rq.succeeded Then
                    For Each t As Track In rq.Result
                        Debug.Print(t.ToDebugString)
                    Next
                Else
                    Debug.Print("artist.getTopTracks failed(" & rq.FailureCode & "):" & rq.errorMessage)
                End If
            Case "artist.removetag"
                Dim rq As New Artist.ArtistRemoveTag(artist, tag)
                rq.RequestMode = rm
                rq.Start()
                If rq.succeeded Then
                    Debug.Print("Tag " & tag & " removed.")
                Else
                    Debug.Print("artist.removeTag failed(" & rq.FailureCode & "):" & rq.errorMessage)
                End If
            Case "artist.search"
                Dim rq As New Artist.ArtistSearch(artist)
                rq.RequestMode = rm
                rq.Start()
                If rq.succeeded Then
                    Debug.Print(rq.Result.ToDebugString)
                Else
                    Debug.Print("artist.search failed(" & rq.FailureCode & "):" & rq.errorMessage)
                End If
            Case "artist.share"
                Dim rq As New Artist.ArtistShare(artist)
                'recommend to me/youself so you don't bug other users ;)
                rq.Recipients.Add(user)
                rq.RequestMode = rm
                rq.Start()
                If rq.succeeded Then
                    Debug.Print("Shared(recommended) artist " & artist & " to users " & String.Join(",", rq.Recipients.ToArray))
                Else
                    Debug.Print("artist.share failed(" & rq.FailureCode & "):" & rq.errorMessage)
                End If
                'Case "auth.getmobilesession"
                '    Dim rq As New Auth.AuthgetMobileSession()
                '    rq.RequestMode = rm
                '    rq.Start()
                '    If rq.succeeded Then
                '        Debug.Print(rq.Result.ToDebugString)
                '    Else
                '        Debug.Print("auth.getMobileSession failed(" & rq.FailureCode & "):" & rq.errorMessage)
                '    End If
            Case "auth.getsession"
                Dim rq As New Auth.AuthGetSession(Settings.AuthData.Token)
                rq.RequestMode = rm
                rq.Start()
                If rq.succeeded Then
                    Debug.Print("Got Session " & rq.Result.SessionKey.ToString)
                    Settings.AuthData.Session = rq.Result
                    vars("session_key") = rq.Result.SessionKey
                Else
                    Debug.Print("auth.getSession failed(" & rq.FailureCode & "):" & rq.errorMessage)
                End If
            Case "auth.gettoken"
                Dim rq As New Auth.AuthGetToken(Settings.AuthData.Token)
                rq.RequestMode = rm
                rq.Start()
                If rq.succeeded Then
                    Debug.Print(rq.Result.ToString)
                Else
                    Debug.Print("auth.getToken failed(" & rq.FailureCode & "):" & rq.errorMessage)
                End If
            Case "event.attend"
                'maybe change this, assign a combo box or so
                Dim rq As New Events.EventAttend(eventid, EventAttendanceStatus.MaybeAttending)
                rq.RequestMode = rm
                rq.Start()
                If rq.succeeded Then
                    Debug.Print("Set attendance status of event with id " & eventid)
                Else
                    Debug.Print("event.attend failed(" & rq.FailureCode & "):" & rq.errorMessage)
                End If
            Case "event.getinfo"
                Dim rq As New Events.EventGetInfo(eventid)
                rq.RequestMode = rm
                rq.Start()
                If rq.succeeded Then
                    Debug.Print(rq.Result.ToDebugString)
                Else
                    Debug.Print("event.getInfo failed(" & rq.FailureCode & "):" & rq.errorMessage)
                End If
            Case "geo.getevents"
                Dim rq As New Geo.GeoGetEvents(location, distance)
                rq.RequestMode = rm
                rq.Start()
                If rq.succeeded Then
                    For Each m As MusicEvent In rq.Result
                        Debug.Print(m.ToDebugString)
                    Next

                Else
                    Debug.Print("geo.getEvents failed(" & rq.FailureCode & "):" & rq.errorMessage)
                End If
            Case "geo.gettopartists"
                Dim rq As New Geo.GetTopArtists("germany")
                rq.RequestMode = rm
                rq.Start()
                If rq.succeeded Then
                    For Each a As ArtistInfo In rq.Result
                        Debug.Print(a.ToDebugString)
                    Next
                Else
                    Debug.Print("geo.getTopArtists failed(" & rq.FailureCode & "):" & rq.errorMessage)
                End If
            Case "geo.gettoptracks"
                Dim rq As New Geo.GetToptracks(country)
                rq.RequestMode = rm
                rq.Start()
                If rq.succeeded Then
                    For Each t As Track In rq.Result
                        Debug.Print(t.ToDebugString)
                    Next
                Else
                    Debug.Print("geo.getTopTracks failed(" & rq.FailureCode & "):" & rq.errorMessage)
                End If
            Case "group.getweeklyalbumchart"
                Dim rq As New Groups.GroupGetWeeklyAlbumChart(group)
                rq.RequestMode = rm
                rq.Start()
                If rq.succeeded Then
                    For Each a As AlbumInfo In rq.Result
                        Debug.Print(a.ToDebugString)
                    Next

                Else
                    Debug.Print("group.getWeeklyAlbumChart failed(" & rq.FailureCode & "):" & rq.errorMessage)
                End If
            Case "group.getweeklyartistchart"
                Dim rq As New Groups.GroupGetWeeklyArtistChart(group)
                rq.RequestMode = rm
                rq.Start()
                If rq.succeeded Then
                    For Each a As ArtistInfo In rq.Result
                        Debug.Print(a.ToDebugString)
                    Next

                Else
                    Debug.Print("group.getWeeklyArtistChart failed(" & rq.FailureCode & "):" & rq.errorMessage)
                End If
            Case "group.getweeklychartlist"
                Dim rq As New Groups.GroupGetWeeklyChartList(group)
                rq.RequestMode = rm
                rq.Start()
                If rq.succeeded Then
                    Debug.Print(rq.Result.ToDebugString)
                Else
                    Debug.Print("group.getWeeklyChartList failed(" & rq.FailureCode & "):" & rq.errorMessage)
                End If
            Case "group.getweeklytrackchart"
                Dim rq As New Groups.GroupGetWeeklyTrackChart(group)
                rq.RequestMode = rm
                rq.Start()
                If rq.succeeded Then
                    For Each t As Track In rq.Result
                        Debug.Print(t.ToDebugString)
                    Next
                Else
                    Debug.Print("group.getWeeklyTrackChart failed(" & rq.FailureCode & "):" & rq.errorMessage)
                End If
            Case "playlist.fetch"
                Dim rq As New Playlists.PlaylistFetch(plUrl)
                rq.RequestMode = rm
                rq.Start()
                If rq.succeeded Then
                    Debug.Print(rq.Result.ToDebugString)
                Else
                    Debug.Print("playlist.fetch failed(" & rq.FailureCode & "):" & rq.errorMessage)
                End If
            Case "tag.getsimilar"
                Dim rq As New Tag.tagGetSimilar(tag, limit)
                rq.RequestMode = rm
                rq.Start()
                If rq.succeeded Then
                    For Each t As TagInfo In rq.Result
                        Debug.Print(t.ToDebugString)
                    Next
                Else
                    Debug.Print("tag.getSimilar failed(" & rq.FailureCode & "):" & rq.errorMessage)
                End If
            Case "tag.gettopalbums"
                Dim rq As New Tag.TagGetTopAlbums(tag)
                rq.RequestMode = rm
                rq.Start()
                If rq.succeeded Then
                    For Each a As AlbumInfo In rq.Result
                        Debug.Print(a.ToDebugString)
                    Next

                Else
                    Debug.Print("tag.getTopAlbums failed(" & rq.FailureCode & "):" & rq.errorMessage)
                End If
            Case "tag.gettopartists"
                Dim rq As New Tag.TagGetTopArtists(tag)
                rq.RequestMode = rm
                rq.Start()
                If rq.succeeded Then
                    For Each a As ArtistInfo In rq.Result
                        Debug.Print(a.ToDebugString)
                    Next
                Else
                    Debug.Print("tag.getTopArtists failed(" & rq.FailureCode & "):" & rq.errorMessage)
                End If
            Case "tag.gettoptags"
                Dim rq As New Tag.TagGetTopTags()
                rq.RequestMode = rm
                rq.Start()
                If rq.succeeded Then
                    For Each a As KeyValuePair(Of String, Uri) In rq.Result
                        Debug.Print("Tag name: " & a.Key)
                        Debug.Print("Url: " & If(a.Value IsNot Nothing, a.Value.AbsoluteUri, ""))
                    Next
                Else
                    Debug.Print("tag.getTopTags failed(" & rq.FailureCode & "):" & rq.errorMessage)
                End If
            Case "tag.gettoptracks"
                Dim rq As New Tag.TagGetTopTracks(tag)
                rq.RequestMode = rm
                rq.Start()
                If rq.succeeded Then
                    For Each t As Track In rq.Result
                        Debug.Print(t.ToDebugString)
                    Next

                Else
                    Debug.Print("tag.getTopTracks failed(" & rq.FailureCode & "):" & rq.errorMessage)
                End If
            Case "tag.search"
                Dim rq As New Tag.tagSearch(tag)
                rq.RequestMode = rm
                rq.Start()
                If rq.succeeded Then
                    Debug.Print(rq.Result.ToDebugString)
                Else
                    Debug.Print("tag.search failed(" & rq.FailureCode & "):" & rq.errorMessage)
                End If
            Case "tasteometer.compare"
                Dim t1 As New TasteOMeterData
                t1.Type = TasteOMeterType.User
                t1.Value = user

                Dim t2 As New TasteOMeterData
                t2.Type = If(vars("TOM_method") = user, TasteOMeterType.User, TasteOMeterType.Artists)
                t2.Value = If(vars("TOM_method") = user, user2, artist)
                Dim rq As New TasteOMeter.TasteOMeterCompare(t1, t2)
                rq.RequestMode = rm
                rq.Start()
                If rq.succeeded Then
                    For Each a As ArtistInfo In rq.Result
                        Debug.Print(a.ToDebugString)
                    Next
                Else
                    Debug.Print("tasteometer.compare failed(" & rq.FailureCode & "):" & rq.errorMessage)
                End If
            Case "track.addtags"
                Dim rq As New Tracks.TrackAddTags(artist, track, taglist)
                rq.RequestMode = rm
                rq.Start()
                If rq.succeeded Then
                    Debug.Print("Tagged track " & artist & " - " & track)
                Else
                    Debug.Print("track.addTags failed(" & rq.FailureCode & "):" & rq.errorMessage)
                End If
            Case "track.ban"
                Dim rq As New Tracks.TrackBan(artist, track)
                rq.RequestMode = rm
                rq.Start()
                If rq.succeeded Then
                    Debug.Print("Banned track " & artist & " - " & track)
                Else
                    Debug.Print("track.ban failed(" & rq.FailureCode & "):" & rq.errorMessage)
                End If
            Case "track.getsimilar"
                Dim rq As New Tracks.TrackGetSimilar(New Track(artist, track))
                rq.RequestMode = rm
                rq.Start()
                If rq.succeeded Then
                    For Each t As Track In rq.Result
                        Debug.Print(t.ToDebugString)
                    Next
                Else
                    Debug.Print("track.getSimilar failed(" & rq.FailureCode & "):" & rq.errorMessage)
                End If
            Case "track.gettopfans"

                Dim rq As New Tracks.TrackGetTopFans(New Track(artist, track))
                rq.RequestMode = rm
                rq.Start()
                If rq.succeeded Then
                    For Each a As TopFan In rq.Result
                        Debug.Print(a.ToDebugString)
                    Next
                Else
                    Debug.Print("track.getTopFans failed(" & rq.FailureCode & "):" & rq.errorMessage)
                End If
            Case "track.gettoptags"
                Dim rq As New Tracks.TrackGetTopTags(New Track(artist, track))
                rq.RequestMode = rm
                rq.Start()
                If rq.succeeded Then
                    For Each a As TagInfo In rq.Result
                        Debug.Print(a.ToDebugString)
                    Next
                Else
                    Debug.Print("track.getTopTags failed(" & rq.FailureCode & "):" & rq.errorMessage)
                End If
            Case "track.love"
                Dim rq As New Tracks.TrackLove(artist, track)
                rq.RequestMode = rm
                rq.Start()
                If rq.succeeded Then
                    Debug.Print("loved track " & artist & " - " & track)
                Else
                    Debug.Print("track.love failed(" & rq.FailureCode & "):" & rq.errorMessage)
                End If
            Case "track.removetag"
                Dim rq As New Tracks.TrackRemoveTag(artist, track, tag)
                rq.RequestMode = rm
                rq.Start()
                If rq.succeeded Then
                    Debug.Print("Tag " & tag & " remvoed")
                Else
                    Debug.Print("track.removeTag failed(" & rq.FailureCode & "):" & rq.errorMessage)
                End If
            Case "track.search"
                Dim rq As New Tracks.TrackSearch(track, artist, limit)

                rq.RequestMode = rm
                rq.Start()
                If rq.succeeded Then
                    Debug.Print(rq.Result.ToDebugString)
                Else
                    Debug.Print("track.search failed(" & rq.FailureCode & "):" & rq.errorMessage)
                End If
            Case "track.share"
                Debug.Print("Note: you can also prove a recommandation message by setting msg in the New() constructor")
                Dim r As New List(Of String)
                r.Add(user)
                Dim rq As New Tracks.TrackShare(artist, track, r)

                rq.RequestMode = rm
                rq.Start()
                If rq.succeeded Then
                    Debug.Print("shared track " & rq.Track.ToString)
                Else
                    Debug.Print("track.share failed(" & rq.FailureCode & "):" & rq.errorMessage)
                End If
            Case "user.getevents"
                Dim rq As New User.UserGetEvents(user)

                rq.RequestMode = rm
                rq.Start()
                If rq.succeeded Then
                    For Each n As MusicEvent In rq.Result
                        Debug.Print(n.ToDebugString)
                    Next
                Else
                    Debug.Print("user.getEvents failed(" & rq.FailureCode & "):" & rq.errorMessage)
                End If
            Case "user.getfriends"
                Dim rq As New User.UserGetFriends(user, True)
                rq.RequestMode = rm
                rq.Start()
                If rq.succeeded Then
                    For Each n As FriendUser In rq.Result
                        Debug.Print(n.ToDebugString)
                    Next
                Else
                    Debug.Print("user.getFriends failed(" & rq.FailureCode & "):" & rq.errorMessage)
                End If
            Case "user.getneighbours"
                Dim rq As New User.UserGetNeighbours(user)
                rq.RequestMode = rm
                rq.Start()
                If rq.succeeded Then
                    For Each n As UserInfo In rq.Result
                        Debug.Print(n.ToDebugString)
                    Next

                Else
                    Debug.Print("user.getNeighbours failed(" & rq.FailureCode & "):" & rq.errorMessage)
                End If
            Case "user.getplaylists"
                Dim rq As New User.UserGetPlaylists(user)
                rq.RequestMode = rm
                rq.Start()
                If rq.succeeded Then
                    'Debug.Print(rq.Result.ToDebugString)
                Else
                    Debug.Print("user.getPlaylists failed(" & rq.FailureCode & "):" & rq.errorMessage)
                End If
            Case "user.getrecenttracks"
                Dim rq As New User.UserGetRecentTracks(user)
                rq.RequestMode = rm
                rq.Start()
                If rq.succeeded Then
                    For Each t As RecentTrack In rq.Result
                        Debug.Print(t.ToDebugString)
                    Next

                Else
                    Debug.Print("user.getRecentTracks failed(" & rq.FailureCode & "):" & rq.errorMessage)
                End If
            Case "user.gettopalbums"
                Dim rq As New User.UserGetTopAlbums(user)
                rq.RequestMode = rm
                rq.Start()
                If rq.succeeded Then
                    For Each i As AlbumInfo In rq.Result
                        Debug.Print(i.ToDebugString)
                    Next
                Else
                    Debug.Print("user.getTopAlbums failed(" & rq.FailureCode & "):" & rq.errorMessage)
                End If
            Case "user.gettopartists"
                Dim rq As New User.UserGetTopArtists(user)
                rq.RequestMode = rm
                rq.Start()
                If rq.succeeded Then
                    For Each i As ArtistInfo In rq.Result
                        Debug.Print(i.ToDebugString)
                    Next
                Else
                    Debug.Print("user.getTopArtists failed(" & rq.FailureCode & "):" & rq.errorMessage)
                End If
            Case "user.gettoptags"
                Dim rq As New User.UserGetTopTags(user)
                rq.RequestMode = rm
                rq.Start()
                If rq.succeeded Then
                    For Each i As TagInfo In rq.Result
                        Debug.Print(i.ToDebugString)
                    Next
                Else
                    Debug.Print("user.getTopTags failed(" & rq.FailureCode & "):" & rq.errorMessage)
                End If
            Case "user.gettoptracks"
                Dim rq As New User.UserGetTopTracks(user)
                rq.RequestMode = rm
                rq.Start()
                If rq.succeeded Then
                    For Each i As Track In rq.Result
                        Debug.Print(i.ToDebugString)
                    Next
                Else
                    Debug.Print("user.getTopTracks failed(" & rq.FailureCode & "):" & rq.errorMessage)
                End If
            Case "user.getweeklyalbumchart"
                Dim rq As New User.UserGetWeeklyalbumChart(user)
                rq.RequestMode = rm
                rq.Start()
                If rq.succeeded Then
                    For Each i As Types.AlbumInfo In rq.Result
                        Debug.Print(i.ToDebugString)
                    Next
                Else
                    Debug.Print("user.getWeeklyAlbumChart failed(" & rq.FailureCode & "):" & rq.errorMessage)
                End If
            Case "user.getweeklyartistchart"
                Dim rq As New User.UserGetWeeklyArtistChart(user)
                rq.RequestMode = rm
                rq.Start()
                If rq.succeeded Then
                    For Each i As ArtistInfo In rq.Result
                        Debug.Print(i.ToDebugString)
                    Next
                Else
                    Debug.Print("user.getWeeklyArtistChart failed(" & rq.FailureCode & "):" & rq.errorMessage)
                End If
            Case "user.getweeklychartlist"
                Dim rq As New User.UserGetWeeklyChartList(user)
                rq.RequestMode = rm
                rq.Start()
                If rq.succeeded Then
                    Debug.Print(rq.Result.ToDebugString)
                Else
                    Debug.Print("user.getWeeklyChartList failed(" & rq.FailureCode & "):" & rq.errorMessage)
                End If
            Case "user.getweeklytrackchart"
                Dim rq As New User.UserGetWeeklyTrackChart(user)
                rq.RequestMode = rm
                rq.Start()
                If rq.succeeded Then
                    For Each i As Track In rq.Result
                        Debug.Print(i.ToDebugString)
                    Next
                Else
                    Debug.Print("user.getWeeklyTrackChart failed(" & rq.FailureCode & "):" & rq.errorMessage)
                End If

        End Select

        Debug.Print("===================== END OF " & lbMethod.Text & "=====================")
    End Sub

    Private Sub cmdHashTest_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)
        Dim res As MsgBoxResult = MsgBox("Request token?", vbYesNo)
        Dim r As New Auth.AuthGetToken()
        r.RequestMode = RequestMode.Rest
        If res = MsgBoxResult.Yes Then r.Start()
        If r.succeeded Or res = MsgBoxResult.No Then
            Settings.AuthData.Token = r.Result
            Debug.Print(Settings.AuthData.Token.ToString)
            Dim s As New Auth.AuthGetSession(Settings.AuthData.Token)
            's.AuthData = r.AuthData
            s.Start()
            If s.succeeded Then
                Debug.Print("Session key: " & s.Result.SessionKey.ToString)
            Else
                Debug.Print(lbMethod.Text & " failed(" & s.FailureCode & "):" & s.errorMessage)
            End If
        Else
            Debug.Print(lbMethod.Text & " failed(" & r.FailureCode & "):" & r.errorMessage)
        End If
    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        Settings.AuthData.AskUserToGrantPermissions()
    End Sub

    Private Sub lbMethod_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles lbMethod.SelectedIndexChanged

    End Sub

    Private Sub ddRequestMethod_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ddRequestMethod.SelectedIndexChanged

    End Sub
End Class
