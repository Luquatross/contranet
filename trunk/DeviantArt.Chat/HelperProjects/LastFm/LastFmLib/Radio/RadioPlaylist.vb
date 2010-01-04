Imports LastFmLib.TypeClasses
Imports LastFmLib
Imports System.Runtime.InteropServices
Namespace Radio
    ''' <summary>
    ''' A class for obtaining a radio playlist from Last.fm.
    ''' For download paths use the path property of the currentTrack property.
    ''' Use setSession to initially download a new playlist chunk.
    ''' </summary>
    Public Class RadioPlaylist

        ''' <summary>
        ''' Occurs when the playlist is loaded.
        ''' </summary>
        Public Event playlistLoaded(ByRef stationName As String, ByVal skipsleft As Integer)
        ''' <summary>
        ''' Occurs when an error occured.
        ''' </summary>
        Public Event [error](ByVal [err] As RadioError, ByVal message As String)
        Const k_minQueueSize As Integer = 2
        Private m_resolver As New XspfResolver()
        Private m_trackQueue As New Queue(Of TypeClasses.TrackInfo)
        Private m_session As String
        Private m_xspf As String
        Private m_basepath As String
        Private m_allXspfRetrieved As Boolean
        Private m_requestingPlaylist As Boolean
        Private WithEvents m_currentTrack As TrackInfo
        Private m_currentRequest As WebRequests.GetXspfPlaylistRequest = Nothing

        ''' <summary>
        ''' The RadioPlaylist's request Type
        ''' </summary>
        Public Enum TypeEnum
            ''' <summary>
            ''' Station
            ''' </summary>
            Type_Station
            ''' <summary>
            ''' Playlist
            ''' </summary>
            Type_Playlist
        End Enum
        ''' <summary>
        ''' An enum for radio errors
        ''' </summary>
        Enum RadioError

            ''' <summary>
            ''' Playlist Invalid
            ''' </summary>
            Radio_BadPlaylist = WebRequests.RequestBase.WebRequestResultCode.WebRequestResult_Custom + 1
            ''' <summary>
            ''' Url invalid
            ''' </summary>
            Radio_InvalidUrl
            ''' <summary>
            ''' Authentication invalid
            ''' </summary>
            Radio_InvalidAuth
            ''' <summary>
            ''' There were too many retries
            ''' </summary>
            Radio_TooManyRetries
            ''' <summary>
            ''' The track was not found
            ''' </summary>
            Radio_TrackNotFound
            ''' <summary>
            ''' The skip limit was exceeded
            ''' </summary>
            Radio_SkipLimitExceeded
            ''' <summary>
            ''' Tried to resume illegally
            ''' </summary>
            Radio_IllegalResume
            ''' <summary>
            ''' No more playlists available
            ''' </summary>
            Radio_OutOfPlaylist
            ''' <summary>
            ''' Failed to load a plugin
            ''' </summary>
            Radio_PluginLoadFailed
            ''' <summary>
            ''' No Soundcard available
            ''' </summary>
            Radio_NoSoundcard
            ''' <summary>
            ''' Error during playback
            ''' </summary>
            Radio_PlaybackError
            ''' <summary>
            ''' Connection was refused
            ''' </summary>
            Radio_ConnectionRefused
            ''' <summary>
            ''' An unknown errror occured
            ''' </summary>
            Radio_UnknownError
        End Enum
        ''' <summary>
        ''' Gets the sessionID
        ''' </summary>
        ''' <value>The session.</value>
        ReadOnly Property SessionID() As String
            Get
                Return m_session
            End Get
        End Property
        ''' <summary>
        ''' Gets the current Request Type.
        ''' </summary>
        ReadOnly Property Type() As TypeEnum
            Get
                Return IIf(String.IsNullOrEmpty(m_session), TypeEnum.Type_Playlist, TypeEnum.Type_Station)
            End Get
        End Property
        ''' <summary>
        ''' Gets a value indicating whether this instance has more items in .
        ''' </summary>
        ''' <value><c>true</c> if this instance has more; otherwise, <c>false</c>.</value>
        ReadOnly Property hasMore() As Boolean
            Get
                Return Not (m_trackQueue.Count = 0)
            End Get
        End Property
        ''' <summary>
        ''' Gets the amount of tracks in the plalist
        ''' </summary>

        ReadOnly Property size() As Integer
            Get
                Return m_trackQueue.Count
            End Get
        End Property
        ''' <summary>
        ''' Gets a value indicating whether this instance is out of content.
        ''' </summary>
        ''' <value>
        ''' <c>true</c> if this instance is out of content; otherwise, <c>false</c>.
        ''' </value>
        ReadOnly Property isOutOfContent() As Boolean
            Get
                Return m_allXspfRetrieved
            End Get
        End Property
        ''' <summary>
        ''' Gets the current track.
        ''' </summary>
        ''' <value>The current track.</value>
        ReadOnly Property currentTrack() As TrackInfo
            Get
                Return m_currentTrack
            End Get
        End Property

        ReadOnly Property Tracks() As List(Of TrackInfo)
            Get
                Return New List(Of TrackInfo)(m_trackQueue)
            End Get
        End Property
        ReadOnly Property Item(ByVal index As Integer) As TrackInfo
            Get
                Return Tracks(index)
            End Get
        End Property
        ''' <summary>
        ''' Initializes a new instance of the <see cref="RadioPlaylist" /> class.
        ''' </summary>
        Public Sub New()

            m_currentRequest = Nothing
            m_allXspfRetrieved = False
            m_requestingPlaylist = False
        End Sub
        ''' <summary>
        ''' Sets the session and then requests the playlist.
        ''' </summary>
        ''' <param name="session">The sessionID.</param>
        Sub setSession(ByVal session As String)
            clear()
            m_session = session
            m_allXspfRetrieved = False

            If (m_trackQueue.Count < k_minQueueSize) And Not m_requestingPlaylist Then
                requestPlaylistChunk()
            End If
        End Sub
        ''' <summary>
        ''' Sets the base path.
        ''' </summary>
        ''' <param name="path">The path.</param>
        Sub setBasePath(ByVal path As String)
            m_basepath = path
        End Sub
        ''' <summary>
        ''' Sets the XSPF and parses it
        ''' </summary>
        ''' <param name="xspf">The XSPF containing string.</param>
        Sub setXspf(ByVal xspf As String)
            clear()
            m_xspf = xspf
            parseXspf(m_xspf)
            m_allXspfRetrieved = True
        End Sub
        ''' <summary>
        ''' Clears the playlist
        ''' </summary>
        Sub clear()
            m_trackQueue.Clear()
            m_currentTrack = New TrackInfo()
            abort()
            m_allXspfRetrieved = True
            m_session = ""
            m_xspf = ""
        End Sub
        ''' <summary>
        ''' Aborts the current Request
        ''' </summary>
        Sub abort()
            If Not m_currentRequest Is Nothing Then m_currentRequest.Abort()
        End Sub
        ''' <summary>
        ''' Discards the remaining tracks and requests a new playlist.
        ''' </summary>
        Sub discardRemaining()
            m_trackQueue.Clear()
            'Empty track
            m_currentTrack = New TrackInfo()
            abort()
            If Type = TypeEnum.Type_Station Then requestPlaylistChunk()

        End Sub
        ''' <summary>
        ''' moves to the next Track
        ''' </summary>
        ''' <returns></returns>
        Public Function nextTrack() As TypeClasses.TrackInfo
            'If Me.isOutOfContent Then
            '    Throw New PlaylistException("Not enough content!")
            '    Return Nothing
            'End If

            m_currentTrack = m_trackQueue.Dequeue()
            If (Not m_allXspfRetrieved And (m_trackQueue.Count < k_minQueueSize)) And Not m_requestingPlaylist Then
                requestPlaylistChunk()
            End If

            'Debug.Print("Next track(" & m_currentTrack.ToString & " dequeued. Queue length is now " & m_trackQueue.Count)
            Return m_currentTrack
        End Function
        Delegate Sub requestPlaylistChunkDelegate(ByVal invoked As Boolean)
        ''' <summary>
        ''' Requests the playlist chunk.
        ''' </summary>
        Private Sub requestPlaylistChunk(Optional ByVal invoked As Boolean = False)
            If Not invoked Then
                Dim d As New requestPlaylistChunkDelegate(AddressOf requestPlaylistChunk)
                d.Invoke(True)
                Exit Sub
            Else
                'Dim t As Threading.Thread
                If Not String.IsNullOrEmpty(m_session) Then
                    'LOGL( 4, "Requesting playlist chunk..." )
                    m_currentRequest = New WebRequests.GetXspfPlaylistRequest(m_session, m_basepath)
                    AddHandler m_currentRequest.Result, AddressOf xspfPlaylistRequestReturn
                    't = New Threading.Thread(AddressOf m_currentRequest.Start)
                    't.Start()
                    m_requestingPlaylist = True
                    m_currentRequest.Start(True)

                End If
            End If
        End Sub

        ''' <summary>
        ''' Handles the result of the playlist Request
        ''' </summary>
        ''' <param name="request">The request.</param>
        Private Sub xspfPlaylistRequestReturn(ByRef request As WebRequests.RequestBase)
            RemoveHandler m_currentRequest.Result, AddressOf xspfPlaylistRequestReturn
            m_requestingPlaylist = False
            m_currentRequest = Nothing
            If request.aborted Then Exit Sub
            If request.failed Then
                RaiseEvent [error](CInt(request.resultCode), request.errorMessage)
                Exit Sub
            End If
            Dim xspf As String = request.RequestData
            parseXspf(xspf)

        End Sub
        ''' <summary>
        ''' Parses the XSPF.
        ''' </summary>
        ''' <param name="xspf">The XSPF.</param>
        Private Sub parseXspf(ByVal xspf As String)

            'LOGL( 4, "XSPF to parse:\n" << xspf.constData()  )
            Try
                Dim sizeBefore As Integer = m_trackQueue.Count
                Dim tracks As List(Of TrackInfo) = m_resolver.resolveTracks(xspf)

                For Each a As TrackInfo In tracks
                    Debug.Write(a.toString & ", ")
                    m_trackQueue.Enqueue(a)
                Next
                Debug.Write(vbCrLf)
                If (m_trackQueue.Count = sizeBefore) Then m_allXspfRetrieved = True
                RaiseEvent playlistLoaded(m_resolver.Station, m_resolver.SkipLimit)
            Catch e As Exceptions.ParseException
                '// Seems we won't get any more xspf. Setting this will make station
                '// finish properly on reaching the end.
                m_allXspfRetrieved = True
                RaiseEvent error(RadioError.Radio_BadPlaylist, "The playlist could not be read. Error:" & vbCrLf & vbCrLf & e.Message)
            End Try
        End Sub
        Function GetTrackList() As List(Of TrackInfo)
            Return New List(Of TrackInfo)(m_trackQueue)
        End Function
    End Class

End Namespace
