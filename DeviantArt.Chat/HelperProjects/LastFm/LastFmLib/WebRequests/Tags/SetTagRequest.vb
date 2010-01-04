Namespace WebRequests
    ''' <summary>
    ''' Tags an Artist, Album, Track, etc. for a user
    ''' </summary>
    Public Class SetTagRequest
        Inherits RequestBase
        Private m_user As TypeClasses.LastFmUser
        Dim m_track As TypeClasses.TrackInfo
        Private m_tags As New List(Of String)
        Dim m_type As ItemType
        Dim m_mode As TagMode
        Dim m_token As String





        ''' <summary>
        ''' Gets or sets the token.
        ''' </summary>
        ''' <value>The token.</value>
        Property Token() As String
            Get
                Return m_token
            End Get
            Set(ByVal value As String)
                m_token = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the tagging mode(append or overwrite)
        ''' </summary>
        ''' <value>The mode.</value>
        Property Mode() As TagMode
            Get
                Return m_mode
            End Get
            Set(ByVal value As TagMode)
                m_mode = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the tags.
        ''' </summary>
        ''' <value>The tags.</value>
        Public Property Tags() As List(Of String)
            Get
                Return m_tags
            End Get
            Set(ByVal Value As List(Of String))
                m_tags = Value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the track.
        ''' </summary>
        ''' <value>The track.</value>
        Public Property Track() As TypeClasses.Track
            Get
                Return m_track
            End Get
            Set(ByVal value As TypeClasses.Track)
                m_track = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the type of what to tag.
        ''' </summary>
        ''' <value>The type of what to tag.</value>
        ''' <exception cref="NotSupportedException">Thrown if tag type is not artist, album or track.</exception>
        Public Property TagType() As ItemType
            Get
                Return m_type
            End Get
            Set(ByVal value As ItemType)
                If value = ItemType.ItemAlbum Or value = ItemType.ItemArtist Or value = ItemType.ItemTrack Then
                    m_type = value
                Else
                    Throw New NotSupportedException("Tagging is only possible for artist, album and track!")
                End If
            End Set
        End Property
        ''' <summary>
        ''' Gets the title string(e.g. Name - Album or Name - Track).
        ''' </summary>
        ''' <value>The title string.</value>
        Public ReadOnly Property TitleString() As String
            Get
                Dim _title As String = m_track.ArtistName
                If Not String.IsNullOrEmpty(m_track.Album) Then
                    _title = _title & " - " & m_track.Album
                ElseIf Not String.IsNullOrEmpty(m_track.Title) Then
                    _title = _title & " - " & m_track.Title
                End If

                Return _title
            End Get

        End Property
        ''' <summary>
        ''' Initializes a new instance of the <see cref="SetTagRequest" /> class.
        ''' </summary>
        Private Sub New()
            MyBase.New(RequestType.SetTag, "SetTag")
        End Sub

        ''' <summary>
        ''' Initializes a new instance of the <see cref="SetTagRequest" /> class.
        ''' </summary>
        ''' <param name="user">The user.</param>
        ''' <param name="track">The track.</param>
        ''' <param name="it">The tagging type(artist, album or track).</param>
        ''' <param name="tags">The tags.</param>
        Public Sub New(ByVal user As TypeClasses.LastFmUser, ByVal track As TypeClasses.TrackInfo, _
                        ByVal it As ItemType, ByVal tags As List(Of String))
            Me.New()
            m_user = user
            m_track = track
            Me.TagType = it
            m_tags = tags
        End Sub

        ''' <summary>
        ''' Determines the type of what should be tagged(Artist, Album, Track,...).
        ''' </summary>
        ''' <param name="type">The type.</param>
        Sub setType(ByVal type As ItemType)
            m_type = type
        End Sub

        ''' <summary>
        ''' Appends the specified tag to a track. The returned request has to be started manually!
        ''' </summary>
        ''' <param name="track">The track.</param>
        ''' <param name="tags">The tag(s). Should be one tag or a CSV list</param>
        ''' <returns>A request instance. Propertys have to be set correctly afterwards(e.g. username, passwordMD5)</returns>
        Public Shared Function Append(ByVal track As TypeClasses.Track, ByVal tags As List(Of String)) As SetTagRequest
            Dim request As New SetTagRequest
            With request
                .setType(ItemType.ItemTrack)
                .Mode = TagMode.TAG_APPEND
                .Tags = tags
                .Track = track
                Return request
            End With
        End Function

        'Won't be implemented as there is too much to port from QMimeData into .Net(gave it a simple try, maybe you'll make it....)
        'Public Shared Function Append(ByVal mime As TypeClasses.MimeData, ByVal tag As String) As SetTagRequest
        '    Dim request As New SetTagRequest
        '    request.setType(mime.it
        'End Function

        Public Overrides Sub Start()
            If String.IsNullOrEmpty(m_user.Username) Then
                Throw New ArgumentNullException("Username")
                Exit Sub
            End If

            Dim xml_rpc As New XMLRPC
            Dim challenge As String = CStr(UnixTime.GetUnixTime)
            With xml_rpc
                .addParameter(m_user.Username)
                .addParameter(challenge)
                .addParameter(GetRequestAuthCode(m_user.PasswordMD5, challenge))
                .addParameter(m_track.ArtistName)
            End With
            Select Case m_type
                Case ItemType.ItemArtist
                    xml_rpc.Method = "tagArtist"
                Case ItemType.ItemAlbum
                    'CHECK THIS OUT!!!!
                    m_token = m_track.Album

                    xml_rpc.Method = "tagAlbum"
                    xml_rpc.addParameter(m_token)
                Case ItemType.ItemTrack
                    m_token = m_track.Title
                    xml_rpc.Method = "tagTrack"
                    xml_rpc.addParameter(m_token)
            End Select
            xml_rpc.addParameter(m_tags)
            xml_rpc.addParameter(IIf(m_mode = TagMode.TAG_OVERWRITE, "set", "append"))
            Request(xml_rpc)
        End Sub
        Protected Overrides Sub success(ByVal data As String)
            Dim retVals As New List(Of Object)
            Dim [error] As String = ""
            Dim parsed As Boolean = XMLRPC.ParseResponse(data, retVals, [error])

            If Not parsed Then
                setFailed(WebRequestResultCode.WebRequestResult_Custom, "Couldn't parse Xml response")
            Else
                Dim response As String = CStr(retVals(0))
                If response <> "OK" Then
                    setFailed(WebRequestResultCode.WebRequestResult_Custom, "Tag request failed, returned: " & response)
                End If
            End If
        End Sub
    End Class
End Namespace
