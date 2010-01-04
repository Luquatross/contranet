Imports LastFmLib.TypeClasses
Namespace Scrobbler
    Public Class ScrobblerManager
        'Implements ICloneable
        Public Event ScrobbleEvent(ByRef t As TrackInfo)
        Public Event NowPlayingEvent(ByRef t As TrackInfo)
        Public Event CacheScrobbleEvent(ByRef c As ScrobblerCache)
        Public Event StatusChangedEvent(ByRef sender As Scrobbler, ByVal s As ScrobblerStatus)


        Private WithEvents m_scrobblers As New List(Of Scrobbler)
        ReadOnly Property CanScrobble(ByVal username As String)
            Get
                Return ScrobblerForUser(username).CanSubmit
            End Get
        End Property
        ReadOnly Property Scrobblers() As List(Of Scrobbler)
            Get
                Return m_scrobblers
            End Get
        End Property
        ReadOnly Property LastError(ByVal user As String) As ScrobblerError
            Get
                Dim s As Scrobbler = ScrobblerForUser(user)
                Return IIf(s IsNot Nothing, s.lasterror, ScrobblerError.NotInitialized)
            End Get
        End Property
        Sub New()

        End Sub


        Public Function Handshake(ByRef init As ScrobblerInit) As Boolean
            Dim scrobbler As Scrobbler = ScrobblerForUser(init.user.Username)
            If scrobbler Is Nothing Then
                scrobbler = New Scrobbler(init)
                AddHandler scrobbler.Handshaken, AddressOf onHandshaken
                AddHandler scrobbler.Scrobbled, AddressOf onScrobbled
                AddHandler scrobbler.Invalidated, AddressOf onInvalidated
                scrobbler.Handshake()
                m_scrobblers.Add(scrobbler)
                RaiseEvent StatusChangedEvent(scrobbler, ScrobblerStatus.Connecting)
            End If
            Return scrobbler.CanSubmit
        End Function

        'Public Function Clone() As Object Implements System.ICloneable.Clone
        '    Dim result As New ScrobblerManager()
        '    result.Scrobblers = byval m_scrobblers
        '    Return result
        'End Function

        Private Function ScrobblerForUser(ByVal user As String) As Scrobbler
            For Each s As Scrobbler In m_scrobblers
                If s.Username = user Then Return s
            Next
            Return Nothing
        End Function
        ''' <summary>
        ''' Scrobbles the specified track. Please set track.Username before calling!
        ''' </summary>
        ''' <param name="track">The track.</param>
        ''' <param name="scrobbleSLBTracks">if set to <c>true</c> tracks with a skipped, loved or banned rating character are scrobbled, too.</param>
        Overloads Sub Scrobble(ByVal track As TrackInfo, Optional ByVal scrobbleSLBTracks As Boolean = False)
            If Not scrobbleSLBTracks And track.IsSkippedLovedOrBanned Then Exit Sub
            Dim cache As New ScrobblerCache(track.Username)
            cache.Append(track, cache.AutoSave)
            Scrobble(cache)
        End Sub
        Overloads Sub Scrobble(ByRef cache As ScrobblerCache)
            If cache.Tracks.Count = 0 Then Exit Sub
            Dim scrobbler As Scrobbler = ScrobblerForUser(cache.Username())
            RaiseEvent StatusChangedEvent(scrobbler, ScrobblerStatus.Scrobbling)
            Dim N As Integer = scrobbler.Submit(cache)
            If N > 0 Then
                RaiseEvent StatusChangedEvent(scrobbler, ScrobblerStatus.TracksScrobbled)
                RaiseEvent CacheScrobbleEvent(cache)
            Else
                RaiseEvent StatusChangedEvent(scrobbler, ScrobblerStatus.TracksNotScrobbled)
            End If
        End Sub
        Sub NowPlayingNotification(ByRef track As TrackInfo)
            Dim scrobbler As Scrobbler = ScrobblerForUser(Track.username)
            If scrobbler IsNot Nothing AndAlso scrobbler.CanAnnounce Then
                scrobbler.announce(track)
            Else
                'Do nothing
            End If
        End Sub
        Private Sub onHandshaken(ByRef sender As Scrobbler)
            Dim username As String = sender.Username
            Dim cache As New ScrobblerCache(username)
            Scrobble(cache)
            RaiseEvent StatusChangedEvent(sender, ScrobblerStatus.Handshaken)
        End Sub
        Private Sub onScrobbled(ByRef sender As Scrobbler, ByRef tracks As List(Of TrackInfo))
            Dim cache As New ScrobblerCache(sender.Username)
            '########################
            'feel free to uncomment this
            'If tracks.Count > 2 Then cache.backup()
            '########################
            Dim remaining As Integer = cache.Remove(tracks)
            If remaining > 0 Then
                Scrobble(cache)
            Else
                If sender.NumScrobbled() > 0 Then RaiseEvent StatusChangedEvent(sender, ScrobblerStatus.TracksScrobbled)
                sender.ResetScrobbleCount()
            End If
        End Sub
        Private Sub onInvalidated(ByRef sender As Scrobbler, ByVal code As ScrobblerError)

            Select Case code
                Case ScrobblerError.BannedClient
                Case ScrobblerError.BadAuthorisation
                Case ScrobblerError.BadTime
                Case ScrobblerError.BadSession
                Case Else
                    Dim username As String = sender.Username
                    Handshake(sender.Init)
                    Exit Select
            End Select
            RaiseEvent StatusChangedEvent(sender, code)
            Dim n As Integer
            For Each s As Scrobbler In m_scrobblers
                If s.Equals(sender) Then
                    If m_scrobblers.Remove(s) Then n += 1
                End If
            Next
        End Sub


    End Class
End Namespace