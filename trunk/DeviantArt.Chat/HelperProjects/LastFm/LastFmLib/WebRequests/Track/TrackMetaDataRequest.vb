Namespace WebRequests
    ''' <summary>
    ''' Requests the meta data of track
    ''' </summary>
    Public Class TrackMetaDataRequest
        Inherits RequestBase
        Dim m_track As New TypeClasses.MetaData
        ''' <summary>
        ''' Gets the MetaData(only senseful after request^^) or sets them(should contain artist,album,title)
        ''' </summary>
        ''' <value>The MetaData.</value>
        Property Track() As TypeClasses.MetaData
            Get
                Return m_track
            End Get
            Set(ByVal value As TypeClasses.MetaData)
                m_track = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the lanugage code.
        ''' </summary>
        ''' <value>The lanugage code.</value>
        Property LanugageCode()
            Get
                Return m_languageCode
            End Get
            Set(ByVal value)
                'for m_languageCode definition, see RequestBase class
                m_languageCode = value
            End Set
        End Property
        ''' <summary>
        ''' Same as Track property, only for better finding the result.
        ''' </summary>
        ''' <value>The meta data.</value>
        ReadOnly Property MetaData() As TypeClasses.MetaData
            Get
                Return Me.Track
            End Get
        End Property
        ''' <summary>
        ''' Initializes a new instance of the <see cref="TrackMetaDataRequest" /> class.
        ''' </summary>
        ''' <param name="md">The metadata which conatins at least artist, album and title.</param>
        Sub New(ByVal md As TypeClasses.MetaData)
            MyBase.New(RequestType.TrackMetaData, "TrackMetaData")
            m_track = md
        End Sub
        ''' <summary>
        ''' Initializes a new instance of the <see cref="TrackMetaDataRequest" /> class.
        ''' </summary>
        ''' <param name="t">The TrackInfo instance containing artist, album and title.</param>
        Sub New(ByVal t As TypeClasses.TrackInfo)
            Me.New(TypeClasses.TrackInfo.toMetadata(t))
        End Sub
        Public Overrides Sub Start()

            Dim xmlrpc As New XMLRPC
            xmlrpc.addParameter(xmlrpc.Escape(m_track.ArtistName))
            xmlrpc.addParameter(xmlrpc.Escape(m_track.Title))
            xmlrpc.addParameter(xmlrpc.Escape(m_track.Album))
            xmlrpc.addParameter(m_languageCode) 'Language code
            Dim params As String = ""
            For Each p As String In xmlrpc.Parameters
                params = params & p & ", "
            Next
            Debug.Print("trackMetaData params are " & params)
            xmlrpc.Method = "trackMetadata"
            'xmlrpc.UseCache = True 'Think I didn't implemented this yet....
            Request(xmlrpc)
        End Sub
        Protected Overrides Sub success(ByVal data As String)
            Dim retVals As New List(Of Object)
            Dim [error] As String = ""
            If Not XMLRPC.ParseResponse(data, retVals, [error]) Then
                setFailed(WebRequestResultCode.WebRequestResult_Custom, [error])
                Exit Sub
            End If
            If Not TypeOf (retVals(0)) Is Dictionary(Of String, Object) Then
                setFailed(WebRequestResultCode.WebRequestResult_Custom, "Result wasn't a <struct>, track not found?")
                Exit Sub
            End If

            Dim map As Dictionary(Of String, Object) = retVals(0)
            If map.ContainsKey("faultCode") Then
                Dim faultstring As String = map.Item("faultString")
                setFailed(WebRequestResultCode.WebRequestResult_Custom, faultstring)
                Exit Sub
            End If
            m_track = New TypeClasses.MetaData

            With m_track
                .ArtistName = CStr(GetValueFromDictonary(map, "artistName"))
                .albumPicUrl = GetUriFromString(CStr(GetValueFromDictonary(map, "albumCover")))
                .label = CStr(GetValueFromDictonary(map, "albumLabel"))
                .Album = CStr(GetValueFromDictonary(map, "albumName"))
                .numTracks = CInt(IIf(String.IsNullOrEmpty(GetValueFromDictonary(map, "albumNumTracks")), 0, GetValueFromDictonary(map, "albumNumTracks")))
                If Not String.IsNullOrEmpty(CStr(GetValueFromDictonary(map, "albumReleaseDate"))) Then
                    Dim rd As Date = Date.Parse(CStr(GetValueFromDictonary(map, "albumReleaseDate")))
                    .releaseDate = rd
                End If
                .albumPageUrl = CStr(GetValueFromDictonary(map, "albumUrl"))
                .buyTrackUrl = CStr(GetValueFromDictonary(map, "trackBuyURL"))
                .buyTrackString = CStr(GetValueFromDictonary(map, "buyTrackString"))
                .buyAlbumUrl = CStr(GetValueFromDictonary(map, "buyAlbumUrl"))
                .buyAlbumString = CStr(GetValueFromDictonary(map, "buyAlbumString"))
                Dim tags As New List(Of String)
                For Each v As String In GetValueFromDictonary(map, "trackTags")
                    tags.Add(v)
                Next
                .trackTags = tags
                .Title = CStr(GetValueFromDictonary(map, "trackTitle"))
                .trackPageUrl = CStr(GetValueFromDictonary(map, "trackUrl"))
            End With
        End Sub
        Private Function GetUriFromString(ByVal s As String) As Uri
            If Uri.IsWellFormedUriString(s, UriKind.Absolute) Then
                Return IIf(String.IsNullOrEmpty(s), Nothing, New Uri(s))
            Else
                Return Nothing
            End If

        End Function
        Private Function GetValueFromDictonary(ByRef d As Dictionary(Of String, Object), ByVal keyname As String) As Object
            Try
                If Not d.ContainsKey(keyname) Then Return Nothing
                Return d(keyname)
            Catch knf As KeyNotFoundException
                'do nothing
                Return Nothing
            End Try
            Return Nothing
        End Function
    End Class

End Namespace
