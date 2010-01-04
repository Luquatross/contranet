Imports LastFmLib.TypeClasses
Imports LastFmLib.WebRequests


Public Class LastFmClient
    Dim m_user As TypeClasses.LastFmUser
    Dim m_tunedIn As Boolean
    Property User() As TypeClasses.LastFmUser
        Get
            Return m_user
        End Get
        Set(ByVal value As TypeClasses.LastFmUser)
            m_user = value
        End Set
    End Property
    ReadOnly Property IsLoggedIn() As Boolean
        Get
            Return Not String.IsNullOrEmpty(m_user.SessionID) AndAlso m_user.SessionID.Length = 32
        End Get
    End Property
    Sub New()

    End Sub
    Sub New(ByVal user As TypeClasses.LastFmUser)
        m_user = user
    End Sub

    ''' <summary>
    ''' Logins this user and sets the SessionID property if successful.
    ''' </summary>
    ''' <returns>
    ''' The TypeClasses.Handshake instance which was used.
    ''' </returns>
    ''' <remarks>It is recommended to keep the result in a variable for reuse. 
    ''' You may need it e.g. for the base url and the base path of the requests(if they were changed^^)
    ''' and to view the error message if the handshake failed.</remarks>
    Public Overridable Function Login() As WebRequests.Handshake
        Dim hs As New WebRequests.Handshake(m_user)
        hs.Start()
        If hs.succeeded Then
            m_user.SessionID = hs.User.SessionID
        End If
        Return hs
    End Function
    Public Overridable Function GetScrobblerManager() As Scrobbler.ScrobblerManager
        Dim m As New Scrobbler.ScrobblerManager()
        Dim init As New Scrobbler.ScrobblerInit
        init.User = m_user

        'you may change this, of course^^
        'For clients who have no official client Id this MUST be left as is!
        'init.Version = My.Application.Info.Version.ToString(2)

        m.Handshake(init)
        Return m
    End Function

    ''' <summary>
    ''' Gets the radio playlist for this user.
    ''' </summary>
    ''' <param name="BasePath">The base path.</param>
    ''' <returns></returns>
    ''' <exception cref="InvalidOperationException">Thrown if the session ID is not set.</exception>
    Public Overridable Function GetRadioPlaylist(Optional ByVal BasePath As String = "/radio") As Radio.RadioPlaylist
        'maybe you want to remove the "and m_tunedIn" if you tune in on your own
        If m_user.hasSessionID And m_tunedIn Then
            Dim pl As New Radio.RadioPlaylist
            pl.setBasePath(BasePath)
            'retreive playlist
            pl.setSession(m_user.SessionID)
            pl.nextTrack()
            Return pl
        Else
            Throw New InvalidOperationException("You have to set the session ID first. You may do this by calling Login() first.")
            Return Nothing
        End If
    End Function


    Public Overridable Function ChangeStation(ByVal station As StationUrl) As Station
        Dim csr As New WebRequests.ChangeStationRequest(m_user.SessionID, station)
        csr.Start()
        If csr.succeeded Then
            m_tunedIn = True
            Return csr.Station
        End If
        Return Nothing
    End Function



    ''' <summary>
    ''' Gets the meta data of the currently playing track. Track needs to be streamed(means downloaded) to get info!
    ''' </summary>
    ''' <returns>
    ''' If successful, a MetaData is returned, else <c>null</c>
    ''' </returns>
    Public Function GetCurrentlyPlayingTrackMetaData() As MetaData
        Dim rmdr As New RadioMetaDataRequest(m_user.SessionID)
        rmdr.Start()
        If rmdr.succeeded Then Return rmdr.MetaData
        Return Nothing
    End Function

    Public Function GetArtistMetadata(ByVal artistName As String) As MetaData
        Dim amd As New ArtistMetadataRequest(artistName)
        amd.start()
        If amd.succeeded Then Return amd.Metadata
        Return Nothing
    End Function
    Public Function GetAlbumMetaData(ByVal artist As String, ByVal album As String) As AlbumMetaDataRequest
        Dim amd As New AlbumMetaDataRequest(artist, album)
        amd.Start()
        Return amd
    End Function

    Public Function GetTrackMetaData(ByVal t As TrackInfo) As MetaData
        Dim tmd As New TrackMetaDataRequest(t)
        tmd.Start()
        If tmd.succeeded Then Return tmd.MetaData
        Return Nothing
    End Function

    ''' <summary>
    ''' Gets the overall top tags.
    ''' </summary>
    ''' <returns>
    ''' If successful, a WeightedStringList is returned, else <c>null</c>
    ''' </returns>
    Public Function GetOverallTopTags() As WeightedStringList
        Dim oat As New OverallTopTagsRequest()
        oat.Start()
        Return IIf(oat.succeeded, oat.tags, Nothing)
    End Function

    Public Function GetSimilarArtists(ByVal artist As String) As WeightedArtistList
        Dim sa As New SimilarArtistsRequest(artist)
        sa.Start()
        Return IIf(sa.succeeded, sa.SimilarArtists, Nothing)
    End Function

    Public Function GetSimilarTags(ByVal tag As String) As WeightedStringList
        Dim st As New SimilarArtistsRequest(tag)
        st.Start()
        Return IIf(st.succeeded, st.SimilarArtists, Nothing)
    End Function

    Public Function GetTrackId(ByVal track As Track) As Integer
        Dim ti As New TrackToIDRequest(track)
        ti.Start()
        Return ti.TrackId
    End Function

    Public Function GetTrackStreamableStatus(ByVal track As Track) As Boolean
        Dim ti As New TrackToIDRequest(track)
        ti.Start()
        Return ti.isStreamable
    End Function
End Class
