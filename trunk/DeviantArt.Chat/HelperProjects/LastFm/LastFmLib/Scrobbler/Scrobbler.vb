Namespace Scrobbler
    ''' <summary>
    ''' The Scrobbler itself.
    ''' </summary>
    Public Class Scrobbler
#Region "Events"
        Public Event Handshaken(ByRef sender As Scrobbler)
        Public Event Scrobbled(ByRef sender As Scrobbler, ByRef tracks As List(Of TypeClasses.TrackInfo))
        Public Event Invalidated(ByRef sender As Scrobbler, ByVal e As ScrobblerError)
#End Region

#Region "Variables"
        Private m_init As New ScrobblerInit
        Private m_scrobbled As Integer
        Private m_lastError As ScrobblerError
        Private WithEvents m_timer As New Timers.Timer()
        Private m_submitted_tracks As New List(Of TypeClasses.TrackInfo)
        Private WithEvents m_handshake As New ScrobblerHandshakeRequest()
        Private WithEvents m_now_playing As New ScrobblerNowPlayingRequest
        Private WithEvents m_submission As New ScrobblerPostRequest
        Private m_ScrobblerSessionID As String
        Private m_hard_failures As Integer
#End Region

#Region "Propertys"
        ReadOnly Property ScrobblerSessionID()
            Get
                Return m_ScrobblerSessionID
            End Get
        End Property
        ReadOnly Property CanSubmit() As Boolean
            Get
                Return m_init.User.hasSessionID And (m_submitted_tracks.Count = 0)
            End Get
        End Property

        ReadOnly Property CanAnnounce() As Boolean
            Get
                Return m_init.User.hasSessionID
            End Get
        End Property

        ReadOnly Property Username() As String
            Get
                Return m_init.User.Username
            End Get
        End Property
        ReadOnly Property Init() As ScrobblerInit
            Get
                Return m_init
            End Get
        End Property
        ReadOnly Property LastError() As ScrobblerError
            Get
                Return m_lastError
            End Get
        End Property

        ReadOnly Property NumScrobbled() As Integer
            Get
                Return m_scrobbled
            End Get
        End Property
        Sub New(ByRef init As ScrobblerInit)
            m_init = init
        End Sub
        Public Function Handshake() As Boolean
            Dim shr As New ScrobblerHandshakeRequest
            shr.request(m_init)

            m_ScrobblerSessionID = shr.ScrobblerSessionId

            m_submission.setUrl(shr.SubmissionUrl)
            m_now_playing.setUrl(shr.NowPlayingUrl)
            Return shr.ErrorCode = ScrobblerError.NoError
        End Function
#End Region



#Region "GetSubmissionString"
        ''' <summary>
        ''' Gets the submission string(converted to UTF-8) of a ScrobblerCache.
        ''' </summary>
        ''' <returns>
        ''' If successful, a String is returned, else <c>null</c>
        ''' </returns>
        Public Overloads Function GetSubmissionString(ByRef cache As ScrobblerCache) As String
            Return GetSubmissionString(cache.Tracks)
        End Function

        ''' <summary>
        ''' Gets the submission string(converted to UTF-8) of a track list.
        ''' </summary>
        ''' <param name="tracks">The tracks.</param>
        ''' <returns>
        ''' If successful, a String is returned, else <c>null</c>
        ''' </returns>
        Public Overloads Function GetSubmissionString(ByRef tracks As List(Of TypeClasses.TrackInfo)) As String
            'For more information see http://www.audioscrobbler.net/development/protocol/

            Dim data As String = ""
            Dim portable As Boolean = False
            'Scrobbler session Id
            data = "s=" & m_ScrobblerSessionID
            For n As Integer = 0 To tracks.Count - 1
                Dim i As TypeClasses.TrackInfo = tracks(n)
                Dim rating As String = ""
                Dim source As String = ""
                If i.RatingCharacter = "S" Or i.RatingCharacter = "L" Or i.RatingCharacter = "B" Then
                    rating = i.RatingCharacter
                End If
                source = i.SourceString
                'If source is the software, append the auth code(see scrobbler specs)
                If source.ToLower = "l" Then source = source & i.AuthCode
                'Artist name
                data += "&a[" & n & "]=" & UriEncodeData(i.ArtistName)
                'Title
                data += "&t[" & n & "]=" & UriEncodeData(i.Title)
                'TimeStamp
                data += "&i[" & n & "]=" & UriEncodeData(i.TimeStamp)
                'The source string(P,R,E,L+auth code,U)
                data += "&o[" & n & "]=" & UriEncodeData(source)
                'Rating character: (S)kip, (L)ove, (B)an or empty
                data += "&r[" & n & "]=" & UriEncodeData(rating)
                'track length
                data += "&l[" & n & "]=" & UriEncodeData(i.Duration)
                'album name
                data += "&b[" & n & "]=" & UriEncodeData(i.Album)
                'current position
                data += "&n[" & n & "]=" & IIf(i.currentPosition > 0, UriEncodeData(i.currentPosition), "")
                'MusicBrainz Id
                data += "&m[" & n & "]=" & UriEncodeData(i.mbId)
                If i.Source = TypeClasses.TrackInfo.SourceEnum.MediaDevice Then
                    portable = True
                    data += "&portable=1"
                End If
            Next
            data = GeneralFunctions.ConvertToUTF8(data)
            Return data
        End Function
#End Region
#Region "Submit"
        ''' <summary>
        ''' Submits the specified tracks.
        ''' </summary>
        ''' <param name="tracks">The tracks.</param>
        ''' <returns>The amount of submitted tracks</returns>
        ''' <remarks>Only a max. of 50 tracks are submitted at once.</remarks>
        Public Overloads Function Submit(ByRef tracks As List(Of TypeClasses.TrackInfo)) As Integer
            'maybe change the ByRef to ByVal....

            Dim dummy As New List(Of TypeClasses.TrackInfo)
            'Sort tracks chronological
            tracks.Sort(New Comparison(Of TypeClasses.TrackInfo)(AddressOf TrackCompare))
            'If there are more than 50 tracks, only submit 50 tracks, else all ones
            dummy.AddRange(tracks.GetRange(0, IIf(tracks.Count > 50, 50, tracks.Count)))
            'create a dummy copy
            m_submitted_tracks = New List(Of TypeClasses.TrackInfo)(dummy)
            'Get the submission string
            Dim data As String = GetSubmissionString(dummy)
            m_submission.request(data)


            Return dummy.Count
        End Function
        ''' <summary>Submits the specified tracks of a ScrobblerCache.
        ''' </summary>
        ''' <param name="cache">The cache.</param>
        ''' <returns>The amount of submitted tracks</returns>
        ''' <remarks>Only a max. of 50 tracks are submitted at once.</remarks>
        Public Overloads Function Submit(ByRef cache As ScrobblerCache) As Integer
            If Not CanSubmit Then Return Nothing
            'The new is 'cause I want to make sure ther's no pointer to the original tracks
            Dim tracks As New List(Of TypeClasses.TrackInfo)
            tracks.AddRange(cache.Tracks)

            Return Submit(tracks)
        End Function
#End Region
#Region "Announce"
        Public Sub Announce(ByRef track As TypeClasses.TrackInfo)
            If track.IsEmpty Then Exit Sub

            Dim data As String = ""
            'Add params, encode chars to %nn if param not empty
            data += "s=" & UriEncodeData(m_ScrobblerSessionID)
            data += "&a=" & UriEncodeData(track.ArtistName)
            data += "&t=" & UriEncodeData(track.Title)
            data += "&b=" & UriEncodeData(track.Album)
            data += "&l=" & UriEncodeData(CStr(track.Duration))
            data += "&n=" & IIf(track.TrackNumber = 0, "", UriEncodeData(CStr(track.TrackNumber)))
            data += "&m=" & UriEncodeData(track.mbId)
            m_now_playing.request(GeneralFunctions.ConvertToUTF8(data))
        End Sub
#End Region

        Public Sub ResetScrobbleCount()
            m_scrobbled = 0
        End Sub
        'Should be t1<t2, check this out....
        Private Function TrackCompare(ByVal t1 As TypeClasses.TrackInfo, ByVal t2 As TypeClasses.TrackInfo) As Integer

            Return t1.TimeStamp.CompareTo(t2.TimeStamp)
        End Function
#Region "Event handler"
        Sub onHandshakeReturn(ByRef result As String) Handles m_handshake.done
            'Check out the CrLf!
            Dim results() As String = result.Split(vbCrLf)
            Dim code As String = results(0)

            If code = "OK" And results.Length >= 4 Then
                Debug.Print("Track(s) scrobbled!")
                m_lastError = ScrobblerError.NoError
                m_ScrobblerSessionID = results(1)
                m_now_playing.setUrl(results(2))
                m_submission.setUrl(results(3))
                m_hard_failures = 0
                m_handshake.resetRetryTimer()
                RaiseEvent Handshaken(Me)
                m_handshake = Nothing
            ElseIf code = "BANNED" Then
                Debug.Print("scrobbling failed!")
                m_lastError = ScrobblerError.BannedClient
                Invalidate(m_lastError)
            ElseIf code = "BADAUTH" Then
                Debug.Print("scrobbling failed!")
                m_lastError = ScrobblerError.BadAuthorisation
                Invalidate(m_lastError)
            ElseIf code = "BADTIME" Then
                Debug.Print("scrobbling failed!")
                m_lastError = ScrobblerError.BadTime
                Invalidate(m_lastError)
            Else
                hardFailure(Me)
            End If
        End Sub
        Sub onNowPlayingReturn(ByRef result As String) Handles m_now_playing.done
            Dim code As String = result.Split(vbLf)(0)
            If code = "OK" Then
                m_lastError = ScrobblerError.NoError
                m_hard_failures = 0
                m_submission.resetRetryTimer()

            ElseIf code = "BADSESSION" Then
                m_lastError = ScrobblerError.BadSession
                Invalidate(m_lastError)

            Else
                hardFailure(Me)
            End If
        End Sub
        Sub onSubmissionReturn(ByRef result As String) Handles m_submission.done
            Dim code As String = result.Split(vbLf)(0)
            If code = "OK" Then
                m_lastError = ScrobblerError.NoError

                For Each track As TypeClasses.TrackInfo In m_submitted_tracks
                    If track.IsLoved Or track.IsScrobbled Then m_scrobbled += 1
                Next

                m_hard_failures = 0
                m_submission.resetRetryTimer()
                '// we must clear so that if more submissions are required they can proceed
                '// as canSubmit() returns false if there are submitted tracks in the queue
                Dim cp As New List(Of TypeClasses.TrackInfo)
                cp.AddRange(m_submitted_tracks)
                m_submitted_tracks.Clear()
                RaiseEvent Scrobbled(Me, cp)
            ElseIf code = "BADSESSION" Then
                m_lastError = ScrobblerError.BadSession
                Invalidate(m_lastError)
            Else
                hardFailure(Me)
            End If
        End Sub
        Private Sub OnHandshakeResponseHeaderReceived(ByVal h As Net.HttpStatusCode) Handles m_handshake.ResponseHeaderReceived
            If Not h = Net.HttpStatusCode.OK Then hardFailure(m_handshake)
        End Sub
#End Region

#Region "Error handling"

        Overridable Overloads Sub hardFailure(ByRef sender As Object)

            m_lastError = ScrobblerError.NotInitialized
            If sender IsNot m_handshake And m_hard_failures > 3 Then
                Invalidate(m_lastError)
            Else
                Try
                    sender.Abort()
                Catch ex As Exception

                End Try
                sender.retry()
            End If
        End Sub

        ''' <summary>
        ''' Invalidates this scrobbler instance and raises an event
        ''' </summary>
        ''' <param name="e">The error.</param>
        Private Sub Invalidate(ByVal e As ScrobblerError)
            m_handshake.Abort()
            'm_now_playing.Abort()
            m_submission.Abort()
            RaiseEvent Invalidated(Me, e)
        End Sub

        Shared Function ErrorDescription(ByVal e As ScrobblerError) As String
            Select Case e
                Case ScrobblerError.BadSession
                    Return tr("Bad session")
                Case ScrobblerError.BannedClient
                    Return tr("Client too old")
                Case ScrobblerError.BadAuthorisation
                    Return tr("Wrong username / password")
                Case ScrobblerError.BadTime
                    Return (tr("Wrong timezone"))
                Case ScrobblerError.NotInitialized
                    Return tr("Could not read server")
                Case Else
                    Return "OK"
            End Select
        End Function
#End Region
        Function UriEncodeData(ByVal s As String) As String
            If String.IsNullOrEmpty(s) Then Return ""
            Return (Uri.EscapeDataString(s))
        End Function
    End Class


End Namespace
