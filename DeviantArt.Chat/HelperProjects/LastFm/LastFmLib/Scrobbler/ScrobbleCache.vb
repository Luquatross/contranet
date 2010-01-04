Imports LastFmLib.TypeClasses
Namespace Scrobbler
    Public Class ScrobblerCache
        Protected m_path As String
        Protected m_username As String
        Protected m_xmldoc As New Xml.XmlDocument
        Protected m_tracks As New List(Of TrackInfo)
        Protected m_autoSave As Boolean = False
        'Maybe revert the slash...dunno if this lib will be used by Mono/Linux 
        'and M$ Windows automatically reverts the slash^^
        Dim m_saveFolder As String = Defaults.kSubmissionCacheDir
        Property SaveFolder() As String
            Get
                Return m_saveFolder
            End Get
            Set(ByVal value As String)
                m_saveFolder = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets a value indicating whether ScrobblerManager should auto-save the cache.
        ''' </summary>
        ''' <value><c>true</c> if ScrobblerManager should auto-save the cache; otherwise, <c>false</c>.</value>
        Property AutoSave() As Boolean
            Get
                Return m_autoSave
            End Get
            Set(ByVal value As Boolean)
                m_autoSave = value
            End Set
        End Property
        Property XmlDoc() As Xml.XmlDocument
            Get
                Return m_xmldoc
            End Get
            Set(ByVal value As Xml.XmlDocument)
                m_xmldoc = value
            End Set
        End Property
        Public ReadOnly Property Tracks() As List(Of TrackInfo)
            Get
                Return m_tracks
            End Get
        End Property
        Public ReadOnly Property Path() As String
            Get
                Return m_path
            End Get
        End Property
        Public ReadOnly Property Username() As String
            Get
                Return m_username
            End Get
        End Property
        Sub New()

        End Sub
        Sub New(ByVal username As String)
            m_username = Username
            'savepath = savefolder & username & "_submissions.xml"
            m_path = m_saveFolder & m_username & Defaults.kSubmissionCacheFilenamePostfix
            read()
        End Sub
        Overloads Shared Function fromFile(ByVal username As String, ByVal path As String) As ScrobblerCache
            Dim cache As New ScrobblerCache(username)
            cache.m_path = path
            cache.read()
            Return cache
        End Function

        'Overloads Shared Function fromFile(ByVal path As String) As ScrobbleCache
        '    Dim cache As New ScrobbleCache()
        '    cache.m_path = path
        '    cache.read()
        '    Return cache
        'End Function
        Public Shared Function MediaDeviceCache(ByRef username As String) As ScrobblerCache
            Return ScrobblerCache.fromFile(username, username & Defaults.kSubmissionMediaDevicePostfix)
        End Function
        ''' <summary>
        ''' Appends the specified tracks and writes them to disk.
        ''' </summary>
        ''' <param name="tracks">The tracks.</param>
        Overloads Sub Append(ByVal tracks As List(Of TrackInfo), Optional ByVal autoWrite As Boolean = True)
            For Each track As TrackInfo In tracks
                merge(track)
            Next
            If autoWrite Then write()
        End Sub
        Overloads Sub Append(ByVal track As TrackInfo, Optional ByVal autoWrite As Boolean = True)
            Dim dummy As New List(Of TrackInfo)
            dummy.Add(track)
            Append(dummy, autoWrite)
        End Sub
        ''' <summary>
        ''' Removes the specified elements from cache.
        ''' </summary>
        ''' <param name="toremove">The toremove.</param>
        ''' <returns>How many elements were removed</returns>
        Overloads Function Remove(ByVal toremove As List(Of TrackInfo), Optional ByVal autoWrite As Boolean = True) As Integer
            Dim trackcount As Integer = m_tracks.Count
            For Each t As TrackInfo In m_tracks
                For Each removeItem As TrackInfo In toremove
                    If removeItem.TimeStamp = t.TimeStamp And removeItem.SameAs(t) Then
                        m_tracks.Remove(t)
                    End If
                Next
            Next
            If autoWrite Or Me.AutoSave Then write()
            Return trackcount - m_tracks.Count
        End Function
        Overloads Function Remove(ByVal track As TrackInfo, Optional ByVal autoWrite As Boolean = True) As Integer
            Remove(track.ToTrackInfoList, autoWrite)
        End Function

        Protected Sub merge(ByRef track As TrackInfo)
            If track Is Nothing Then
                Throw New ArgumentNullException("track")
            End If

            'if the track is empty, just exit(???)
            If track.isEmpty Then Exit Sub

            Dim u As New UnixTime(track.TimeStamp, UnixTime.UnixTimeFormat.Seconds)
            Dim asCreation As TimeSpan = UnixTime.GetUnixTimeOfDate(New Date(2003, 1, 1))

            If u.TimeSpan.TotalSeconds < asCreation.TotalSeconds Then
                Debug.Print("Won't scrobble track from before the date Audioscrobbler project was founded!")
                Exit Sub
            End If

            'Loop through existing tracks and complete track info if avaliable
            For Each cachedtrack As TrackInfo In m_tracks
                If track.SameAs(cachedtrack) And track.TimeStamp = cachedtrack.TimeStamp Then
                    track = cachedtrack.MergeWith(track)
                End If
            Next
            m_tracks.Add(track)
        End Sub
        Public Function read(Optional ByVal loadFromFile As Boolean = False) As Boolean
            Dim xmldoc As New Xml.XmlDocument
            If loadFromFile Then
                If IO.File.Exists(m_path) Then
                    Try
                        xmldoc.Load(m_path)
                    Catch x As Xml.XmlException
                        'Throw New Exceptions.ParseException("ScrobbleCache.read: Could not parse XML.", x)
                        Return False
                        Exit Function
                    End Try
                Else
                    Return False
                End If
            Else
                If m_xmldoc Is Nothing Or m_xmldoc.DocumentElement Is Nothing Then Return False
                xmldoc = m_xmldoc
            End If
            If m_tracks Is Nothing Then
                m_tracks = New List(Of TrackInfo)
            End If
            m_tracks.Clear()

            'read out the username, if possible
            With xmldoc.DocumentElement
                If .HasAttribute("user") Then
                    m_username = .Attributes("user").InnerText
                End If
            End With

            For Each n As Xml.XmlElement In xmldoc.DocumentElement

                If n.Name = "item" Then
                    Dim t As New TypeClasses.TrackInfo(n)
                    m_tracks.Add(t)
                End If

            Next
            Return True

        End Function

        Public Sub write(Optional ByVal writeToDisk As Boolean = False)
            If m_tracks.Count = 0 And (writeToDisk Or AutoSave) Then
                Try
                    IO.File.Delete(m_path)
                Catch e As Exception
                    'do nothing
                End Try
            Else
                Dim xmldoc As New Xml.XmlDocument()

                xmldoc.CreateXmlDeclaration("1.0", "UTF-8", "")
                Dim e As Xml.XmlElement = xmldoc.CreateElement("submissions")
                xmldoc.AppendChild(e)
                'Don't know if the following two lines are really needed, feel free to delete them...
                Dim a As Xml.XmlAttribute = xmldoc.CreateAttribute("product")
                a.Value = "Audioscrobbler"
                e.Attributes.Append(a)

                a = xmldoc.CreateAttribute("version")
                a.Value = "1.2"
                e.Attributes.Append(a)

                'add a username for better refernce, but keep the xml compatible to the original client
                a = xmldoc.CreateAttribute("user")
                a.Value = m_username
                e.Attributes.Append(a)

                For Each t As TrackInfo In m_tracks
                    e.AppendChild(t.ToXmlElement(xmldoc))
                Next
                xmldoc.AppendChild(e)
                m_xmldoc = xmldoc
                If writeToDisk Then
                    Dim fulldirname As String = IO.Path.GetDirectoryName(m_path)
                    If Not IO.Directory.Exists(fulldirname) Then IO.Directory.CreateDirectory(fulldirname)
                    Dim w As Xml.XmlWriter = Xml.XmlWriter.Create(m_path)
                    xmldoc.WriteTo(w)
                    'w.Flush()
                    w.Close()
                End If
            End If
        End Sub
        ''' <summary>
        ''' Creates a timestamped backup of current cache.
        ''' </summary>
        Sub backup()
            Dim timestamp As String = Date.Now.ToString("yyyyMMddhhmm")
            Dim filename As String = IO.Path.GetFileNameWithoutExtension(m_path) & "." & timestamp & "backup.xml"
            Dim backup As ScrobblerCache = ScrobblerCache.fromFile(Me.Username, filename)

            '// append in case we made a backup this minute already
            'should make sense^^
            'Append auto-writes tracks to a file, just to mention=>as it's a backup, force it!
            backup.Append(Me.Tracks, True)

        End Sub
        Public Shared Operator =(ByVal s1 As ScrobblerCache, ByVal s2 As ScrobblerCache) As Boolean
            Return s1.Tracks.Equals(s2.Tracks) And s1.Username = s2.Username
        End Operator
        Public Shared Operator <>(ByVal s1 As ScrobblerCache, ByVal s2 As ScrobblerCache) As Boolean
            Return Not (s1 = s2)
        End Operator
    End Class
End Namespace