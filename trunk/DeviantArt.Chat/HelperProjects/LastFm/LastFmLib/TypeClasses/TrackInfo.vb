Imports LastFmLib.Constants
Imports LastFmLib.Utils
Namespace TypeClasses
    Public Class TrackInfo
        Inherits TypeClasses.Track
        'DO NOT UNDER ANY CIRCUMSTANCES CHANGE THE ORDER OF THIS ENUM!
        ' you will cause broken settings and b0rked scrobbler cache submissions
        Enum SourceEnum
            Unknown = -1
            Radio
            Player
            MediaDevice
        End Enum

        Enum ScrobblableStatusEnum
            OkToScrobble
            NoTimeStamp
            TooShort
            ArtistNameMissing
            TrackNameMissing
            ExcludedDir
            ArtistInvalid
        End Enum
        Protected m_TrackNumber As Integer

        Protected m_album As String
        Protected m_url As Uri
        Protected m_playCount As Integer
        Protected m_duration As Integer
        Protected m_fileName As String
        Protected m_mbId As String
        Protected m_timeStamp As Long 'UnixTime in Seconds
        Protected m_source As SourceEnum
        Protected m_authCode As String
        Protected m_uniqueID As String
        Protected m_playerId As String

        Protected m_paths As New List(Of String)
        Protected m_nextPath As Integer

        Protected m_username As String
        Protected m_InvalidArtists As List(Of String) = InitInvalid()
        Protected m_ratingCharacter As Char
        Protected m_currentPosition As Integer = -1
        Protected m_scrobbled As Boolean
#Region "Propertys"
        ''' <summary>
        ''' Gets or sets the current position.
        ''' </summary>
        ''' <value>The current position.</value>
        Property currentPosition() As Integer
            Get
                Return m_currentPosition
            End Get
            Set(ByVal value As Integer)
                m_currentPosition = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the rating character.
        ''' </summary>
        ''' <value>The rating character.</value>
        Property RatingCharacter() As Char
            Get
                Return m_ratingCharacter
            End Get
            Set(ByVal value As Char)
                If value = "S" Or value = "L" Or value = "B" Then
                    m_ratingCharacter = value
                Else
                    Throw New InvalidOperationException("Value has to be S(kip),L(ove) or B(an)!")
                End If
            End Set
        End Property

        Property TrackUrl() As Uri
            Get
                Return m_url
            End Get
            Set(ByVal value As Uri)
                m_url = value
            End Set
        End Property
        Property TrackNumber() As Integer
            Get
                Return m_TrackNumber
            End Get
            Set(ByVal value As Integer)
                m_TrackNumber = value
            End Set
        End Property

        Property Album() As String
            Get
                Return m_album
            End Get
            Set(ByVal value As String)
                m_album = value
            End Set
        End Property

        Property PlayCount() As Integer
            Get
                Return m_playCount
            End Get
            Set(ByVal value As Integer)
                m_playCount = value
            End Set
        End Property
        Property Duration() As Integer
            Get
                Return m_duration
            End Get
            Set(ByVal value As Integer)
                m_duration = value
            End Set
        End Property
        Property mbId() As String
            Get
                Return m_mbId
            End Get
            Set(ByVal value As String)
                m_mbId = value
            End Set
        End Property

        ReadOnly Property HasMorePaths() As Boolean
            Get
                Return IIf(m_nextPath < m_paths.Count, True, False)
            End Get
        End Property
        Property TimeStamp() As Long
            Get
                Return m_timeStamp
            End Get
            Set(ByVal value As Long)
                m_timeStamp = value
            End Set
        End Property

        Property FileName() As String
            Get
                Return m_fileName
            End Get
            Set(ByVal value As String)
                m_fileName = value
            End Set
        End Property
        Property UniqueID() As String
            Get
                Return m_uniqueID
            End Get
            Set(ByVal value As String)
                m_uniqueID = value
            End Set
        End Property
        Property Source() As SourceEnum
            Get
                Return m_source
            End Get
            Set(ByVal value As SourceEnum)
                m_source = value
            End Set
        End Property
        Property AuthCode() As String
            Get
                Return m_authCode
            End Get
            Set(ByVal value As String)
                m_authCode = value
            End Set
        End Property
        Property PlayerId() As String
            Get
                Return m_playerId
            End Get
            Set(ByVal value As String)
                m_playerId = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the username. Here you have the possiblity to assign a username to this track, e.g. for scrobbling
        ''' </summary>
        ''' <value>The username.</value>
        Property Username() As String
            Get
                Return m_username
            End Get
            Set(ByVal value As String)
                m_username = value
            End Set
        End Property
        ReadOnly Property IsSkipped() As Boolean
            Get
                Return RatingCharacter <> "S"
            End Get
        End Property
        ReadOnly Property IsLoved() As Boolean
            Get
                Return RatingCharacter <> "L"
            End Get
        End Property
        ReadOnly Property IsBanned() As Boolean
            Get
                Return RatingCharacter <> "B"
            End Get
        End Property
        ReadOnly Property IsSkippedLovedOrBanned() As Boolean
            Get
                Return Not (IsSkipped Or IsLoved Or IsBanned)
            End Get
        End Property
        ReadOnly Property IsScrobbled() As Boolean
            Get
                Return m_scrobbled
            End Get
        End Property
        ReadOnly Property DownloadUris() As List(Of Uri)
            Get
                Return GetDownloadUrisFromPath()
            End Get
        End Property
        Property DownloadUriStrings() As List(Of String)
            Get
                Return m_paths
            End Get
            Set(ByVal value As List(Of String))
                m_paths = value
            End Set
        End Property
#End Region
        Public Overrides Function Clone() As Object
            Dim result As TrackInfo
            result = Me.MemberwiseClone
            result.m_url = cloneurl(m_url)
            result.m_paths = CloneList(Of String)(m_paths)
            result.m_InvalidArtists = InitInvalid()
            Return result
        End Function

        Public Sub New()
            MyBase.new()
        End Sub

        Public Sub New(ByVal artist As String, ByVal album As String, ByVal title As String)
            MyBase.New(artist, title)
            Me.Album = album
        End Sub


        ReadOnly Property scrobblableStatus() As ScrobblableStatusEnum
            Get

                ' Check duration
                If (Duration() < kScrobbleMinLength) Then

                    'LOG(3, "Track length is " << duration() << " s which is too short, will not submit.\n")
                    Return ScrobblableStatusEnum.TooShort
                End If

                ' Radio tracks above preview length always scrobblable
                If (Source() = SourceEnum.Radio) Then

                    Return ScrobblableStatusEnum.OkToScrobble
                End If

                ' Check timestamp
                If (m_timeStamp = 0) Then

                    'LOG(3, "Track has no timestamp, will not submit.\n")
                    Return ScrobblableStatusEnum.NoTimeStamp
                End If

                ' Check if any required fields are empty
                If (String.IsNullOrEmpty(ArtistName())) Then

                    'LOG(3, "Artist was missing, will not submit.\n")
                    Return ScrobblableStatusEnum.ArtistNameMissing
                End If
                If (String.IsNullOrEmpty(Title())) Then

                    'LOG(3, "Artist, track or duration was missing, will not submit.\n")
                    Return ScrobblableStatusEnum.TrackNameMissing
                End If

                '' Check if dir excluded
                'If (dirExcluded(path())) Then

                '    'LOG(3, "Track is in excluded directory '" << path() << "', " << "will not submit.\n")
                '    Return ScrobblableStatusEnum.ExcludedDir
                'End If

                ' Check if artist name is an invalid one like "unknown"
                For Each invalid As String In m_InvalidArtists
                    If (ArtistName().ToLower() = invalid) Then
                        'LOG(3, "Artist '" << artist() << "' is an invalid artist name, will not submit.\n")
                        Return ScrobblableStatusEnum.ArtistInvalid
                    End If
                Next

                ' All tests passed!
                Return ScrobblableStatusEnum.OkToScrobble

            End Get
        End Property
        'These is for Scrobbling. Portage is incomplete as I don't need it...
        Property Path() As String
            Get
                If m_paths.Count = 0 Then
                    Return ""
                Else
                    Return m_paths.Item(0)
                End If
            End Get
            Set(ByVal value As String)
                m_paths.Clear()
                m_paths.Add(Path)
            End Set
        End Property

        ReadOnly Property NextPath() As String
            Get
                If m_nextPath < m_paths.Count Then
                    m_nextPath = m_nextPath + 1
                    Return m_paths.Item(m_nextPath)
                Else
                    Return ""
                End If
            End Get
        End Property


        Property SourceString() As String
            Get
                Select Case m_source
                    Case SourceEnum.Radio
                        Return "L"
                    Case SourceEnum.Player
                        Return ""
                    Case SourceEnum.MediaDevice
                        Return "P"
                    Case Else
                        Return "U"
                End Select
            End Get
            Set(ByVal value As String)
                Select Case value

                    Case "L"
                        m_source = SourceEnum.Radio
                    Case ""
                        m_source = SourceEnum.Player
                    Case "P"
                        m_source = SourceEnum.MediaDevice
                    Case "U"
                        m_source = SourceEnum.Unknown
                End Select

            End Set
        End Property
        'Overloads ReadOnly Property IsEmpty() As Boolean
        '    Get
        '        Return (String.IsNullOrEmpty(m_artist) And String.IsNullOrEmpty(m_title))
        '    End Get
        'End Property
        Public Sub New(ByRef e As Xml.XmlElement)
            MyBase.new()
            Me.ArtistName = Itemtext(e, "artist")
            Me.Album = Itemtext(e, "album")
            Me.Title = Itemtext(e, "track")
            Me.Duration = Itemtext(e, "duration")
            Me.PlayCount = Itemtext(e, "playcount")
            'Me.FileName = Itemtext(e, "filename")
            Me.UniqueID = Itemtext(e, "uniqueID")
            Me.SourceString = Itemtext(e, "source")
            Me.AuthCode = Itemtext(e, "authCode")
            Dim timestamp As String = Itemtext(e, "timestamp")
            If String.IsNullOrEmpty(timestamp) Or timestamp = "0" Then
                timeStampMe()
            Else
                Me.TimeStamp = Itemtext(e, "timestamp")
            End If
        End Sub
        Protected Function Itemtext(ByVal e As Xml.XmlElement, ByVal text As String) As String
            If Not e.IsEmpty Then
                Try
                    If e.Item(text) IsNot Nothing AndAlso Not String.IsNullOrEmpty(e.Item(text).InnerText) Then
                        Return e.Item(text).InnerText
                    End If
                Catch a As Exception
                    Return Nothing
                End Try
            End If
            Return Nothing
        End Function
        Sub timeStampMe()
            SetTimeStamp(UnixTime.GetUnixTime(UnixTime.UnixTimeFormat.Seconds))
        End Sub

        Sub SetTimeStamp(ByVal timestamp As UnixTime)
            m_timeStamp = timestamp.get(UnixTime.UnixTimeFormat.Seconds)
        End Sub

        ''' <summary>
        ''' Sets the Unixtime stamp(in Seconds)
        ''' </summary>
        ''' <param name="timestamp">The timestamp.</param>
        Sub SetTimeStamp(ByVal timestamp As Long)
            m_timeStamp = timestamp
        End Sub
        'DirExcluded is needed for Scrobbling. You are free to implement this.
        'Public Function DirExcluded(ByVal path As String) As Boolean
        '    If String.IsNullOrEmpty(path) Then Return False
        '    Dim dirToTest As IO.Path
        '    IO.Path.InvalidPathChars()
        '    io.Directory.
        'End Function
        Shared Function InitInvalid() As List(Of String)
            Dim l As New List(Of String)
            l.Add("unknown artist")
            l.Add("unknown")
            l.Add("[unknown artist]")
            l.Add("[unknown]")

            Return l
        End Function
        Public Overrides Function toString() As String

            If String.IsNullOrEmpty(m_artist) Then
                Return m_title
            ElseIf String.IsNullOrEmpty(m_title) Then
                Return m_artist
            Else
                Return m_artist & " " + Microsoft.VisualBasic.ChrW(8211) & " " & m_title
            End If

        End Function
        Public Function SameAs(ByRef that As TrackInfo)
            If Not (Me.ArtistName() = that.ArtistName()) Then Return False
            If Not Me.Title = that.Title Then Return False
            If Not (Title() = that.Title()) Then Return False

            If Not (Album() = that.Album()) Then Return False

            Return True
        End Function

        'TrackInfo::scrobbleTime()
        '{
        '    // If we don't have a length or it's less than the minimum, return the
        '    // threshold
        '    if (duration() <= 0 || duration() < Constants::kScrobbleMinLength)
        '        return Constants::kScrobbleTimeMax;

        '    float scrobPoint = qBound(
        '        Constants::kScrobblePointMin, 
        '        The::settings().currentUser().scrobblePoint(),
        '        Constants::kScrobblePointMax );

        '    scrobPoint /= 100.0f;

        '    return qMin( Constants::kScrobbleTimeMax, int(duration() * scrobPoint) );
        '}
        Public Function ScrobbleTime(ByVal u As LastFmUser) As Integer

            '// If we don't have a length or it's less than the minimum, return the
            '// threshold
            If (Duration() <= 0 Or Duration() < kScrobbleMinLength) Then Return kScrobbleTimeMax

            Dim scrobPoint As Single = qBound(kScrobblePointMin, _
                u.scrobblePoint, _
                kScrobblePointMax)

            scrobPoint /= 100.0F

            Return qMin(kScrobbleTimeMax, CInt(Duration() * scrobPoint))

        End Function
        ''' <summary>
        ''' Returns a List(Of TrackInfo)-instance containing this track instance only.
        ''' </summary>
        ''' <returns>
        ''' If successful, a List(Of TrackInfo) is returned, else <c>null</c>
        ''' </returns>
        Public Function ToTrackInfoList() As List(Of TrackInfo)
            Dim dummy As New List(Of TrackInfo)
            dummy.Add(Me)
            Return dummy
        End Function
        ''' <summary>
        ''' Returns a XML representation of this instance, e.g. serializes it.
        ''' </summary>
        ''' <param name="document">The xml document.</param>
        ''' <returns>
        ''' If successful, a XmlElement is returned, else <c>null</c>
        ''' </returns>
        Public Function ToXmlElement(ByRef document As Xml.XmlDocument) As Xml.XmlElement
            Dim item As Xml.XmlElement = document.CreateElement("item")

            Dim artist As Xml.XmlElement = document.CreateElement("artist")
            Dim artistText As Xml.XmlText = document.CreateTextNode(m_artist)
            artist.AppendChild(artistText)
            item.AppendChild(artist)


            Dim album As Xml.XmlElement = document.CreateElement("album")
            Dim albumText As Xml.XmlText = document.CreateTextNode(m_artist)
            album.AppendChild(albumText)
            item.AppendChild(album)


            Dim title As Xml.XmlElement = document.CreateElement("track")
            Dim titleText As Xml.XmlText = document.CreateTextNode(m_title)
            title.AppendChild(titleText)
            item.AppendChild(title)


            Dim length As Xml.XmlElement = document.CreateElement("duration")
            Dim lengthText As Xml.XmlText = document.CreateTextNode(m_duration)
            length.AppendChild(lengthText)
            item.AppendChild(length)

            Dim playtime As Xml.XmlElement = document.CreateElement("timestamp")
            Dim playtimeText As Xml.XmlText = document.CreateTextNode(m_timeStamp)
            playtime.AppendChild(playtimeText)
            item.AppendChild(playtime)

            Dim playcount As Xml.XmlElement = document.CreateElement("playcount")
            Dim playcountText As Xml.XmlText = document.CreateTextNode(m_playCount)
            playcount.AppendChild(playcountText)
            item.AppendChild(playcount)

            'Dim filename As Xml.XmlElement = document.CreateElement("filename")
            'Dim filenameText As Xml.XmlText = document.CreateTextNode(m_fileName)
            'filename.AppendChild(filenameText)
            'item.AppendChild(filename)

            Dim uniqueID As Xml.XmlElement = document.CreateElement("uniqueID")
            Dim uniqueIDText As Xml.XmlText = document.CreateTextNode(m_uniqueID)
            uniqueID.AppendChild(uniqueIDText)
            item.AppendChild(uniqueID)

            Dim source As Xml.XmlElement = document.CreateElement("source")
            Dim sourceText As Xml.XmlText = document.CreateTextNode(m_source)
            source.AppendChild(sourceText)
            item.AppendChild(source)

            Dim authKey As Xml.XmlElement = document.CreateElement("authCode")
            Dim authKeyText As Xml.XmlText = document.CreateTextNode(m_authCode)
            authKey.AppendChild(authKeyText)
            item.AppendChild(authKey)
            Return item
        End Function


        ''' <summary>
        ''' Sets the paths.
        ''' </summary>
        ''' <param name="paths">The paths.</param>
        Sub SetPaths(ByVal paths As List(Of String))
            If Not paths.Count = 0 Then
                m_paths = paths
            Else
                m_paths = New List(Of String)
            End If
        End Sub


        ''' <summary>
        ''' Get a value inside a specific bound
        ''' </summary>
        ''' <param name="min">The min.</param>
        ''' <param name="value">The value.</param>
        ''' <param name="max">The max.</param>
        ''' <returns>If value is outside the bounds, min/max is returned, else value</returns>

        Private Function qBound(ByVal min As Integer, ByVal value As Integer, ByVal max As Integer) As Integer
            If value < min Then Return min
            If value > max Then Return max
            Return value
        End Function

        Private Function qMin(ByVal min As Integer, ByVal value As Integer) As Integer
            If value < min Then Return min
            Return value
        End Function

        Private Function GetDownloadUrisFromPath() As List(Of Uri)
            Dim result As New List(Of Uri)
            For Each s As String In m_paths
                Try
                    If Not String.IsNullOrEmpty(s) Then
                        If Uri.IsWellFormedUriString(s, UriKind.RelativeOrAbsolute) Then result.Add(New Uri(s))
                    End If
                Catch wrongFormat As UriFormatException
                    'Do nothing
                End Try
            Next
            Return result
        End Function
        Public Function toMetadata() As MetaData
            Dim result As MetaData = TrackInfo.toMetadata(Me)

            Return result
        End Function
        Shared Function toMetadata(ByVal track As TrackInfo) As MetaData
            Dim result As New MetaData
            'just to be sure...
            With result

                .Album = track.Album
                .ArtistName = track.ArtistName
                .AuthCode = track.AuthCode
                .Duration = track.Duration
                .FileName = track.FileName
                .mbId = track.mbId
                .PlayCount = track.PlayCount
                .PlayerId = track.PlayerId
                .Source = track.Source
                .TimeStamp = track.TimeStamp
                .Title = track.Title
                .UniqueID = track.UniqueID
                .Username = track.Username
                .SetDownloadUris(track.DownloadUriStrings)
            End With
            Return result
        End Function
        Public Function MergeWith(ByVal t As TrackInfo) As TrackInfo
            If t Is Nothing Then Return Me
            Dim result As New TrackInfo
            result = Me
            With result
                If String.IsNullOrEmpty(.Album) Then .Album = t.Album

                If String.IsNullOrEmpty(.ArtistName) Then .ArtistName = t.ArtistName

                If String.IsNullOrEmpty(.AuthCode) Then .AuthCode = t.AuthCode

                If .DownloadUris.Count = 0 Then
                    .SetPaths(t.DownloadUriStrings)
                End If

                If .Duration = 0 Then .Duration = t.Duration
                If String.IsNullOrEmpty(.FileName) Then .FileName = t.FileName

                If String.IsNullOrEmpty(.mbId) Then .mbId = t.mbId

                If String.IsNullOrEmpty(.Path) Then .Path = t.Path
                If .PlayCount = 0 Then .PlayCount = t.PlayCount
                If String.IsNullOrEmpty(.PlayerId) Then .PlayerId = t.PlayerId

                If .Source = Nothing Then .Source = t.Source
                If .TimeStamp = Nothing Then .TimeStamp = t.TimeStamp
                If String.IsNullOrEmpty(.Title) Then .Title = t.Title
            End With
            Return result
        End Function
        Protected Sub SetDownloadUris(ByVal u As List(Of String))
            m_paths = u
        End Sub
        Public Overrides Function ToTrackInfo() As TrackInfo
            Return Me
        End Function
    End Class
End Namespace