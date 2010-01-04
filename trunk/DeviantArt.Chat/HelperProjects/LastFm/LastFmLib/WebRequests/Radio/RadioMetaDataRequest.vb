
Namespace WebRequests
    ''' <summary>
    ''' Gets the information about the track which is currently playing.
    ''' </summary>
    Public Class RadioMetaDataRequest
        Inherits RequestBase
        Private m_stationFeed As String
        Private m_stationName As String
        Private m_MetaData As TypeClasses.MetaData
        Private m_session As String
        ''' <summary>
        ''' Gets or sets the sessionId.
        ''' </summary>
        ''' <value>The session.</value>
        Property SessionId() As String
            Get
                Return m_session
            End Get
            Set(ByVal value As String)
                m_session = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or Return the meta data of the Request
        ''' </summary>
        ''' <value>The meta data.</value>
        Property MetaData() As TypeClasses.MetaData
            Get
                Return m_MetaData
            End Get
            Set(ByVal value As TypeClasses.MetaData)
                m_MetaData = value
            End Set
        End Property
        ''' <summary>
        ''' Gets the name of the station.
        ''' </summary>
        ''' <value>The name of the station.</value>
        ReadOnly Property StationName() As String
            Get
                Return m_stationName
            End Get
        End Property
        ''' <summary>
        ''' Gets the station feed url.
        ''' </summary>
        ''' <value>The station feed url.</value>
        ReadOnly Property StationFeed() As String
            Get
                Return m_stationFeed
            End Get

        End Property

        ''' <summary>
        ''' Initializes a new instance of the <see cref="RadioMetaDataRequest" /> class.
        ''' </summary>
        ''' <param name="sId">The SessionID.</param>
        Sub New(ByVal sId As String)
            MyBase.New(RequestType.RadioMetaData, "RadioMetaData")
            'm_cacheable = False
            m_session = sId
            '//The following two lines aren't implemented yet
            'm_retrytimeout = 0
            'm_retry_timer.setSingleShot( true );
        End Sub
        Public Overrides Sub Start()
            Me.get(m_basePath & "/np.php?session=" & SessionId)
        End Sub
        Protected Overrides Sub success(ByVal data As String)
            Dim track As New TypeClasses.MetaData
            Dim stationName As String
            If data.Length <= 0 Then
                Me.setFailed(WebRequestResultCode.WebRequestResult_Custom, "Get metadata for radio failed")
                Exit Sub
            ElseIf parameter("streaming", data) = "false" Then
                Me.setFailed(WebRequestResultCode.WebRequestResult_Custom, "Get metadata for radio failed, because nothing is streamed!")
                Exit Sub
            Else
                With track
                    .ArtistName = parameter("artist", data)
                    .Album = parameter("album", data)
                    .Title = parameter("track", data)
                    .albumPicUrl = GetUriFromString(parameter("albumcover_medium", data))
                    .artistPageUrl = parameter("artist_url", data)
                    .albumPageUrl = parameter("album_url", data)
                    .trackPageUrl = parameter("track_url", data)
                    .Duration = parameter("trackduration", data)
                    .Source = TypeClasses.TrackInfo.SourceEnum.Radio

                    Dim errorCode As Integer = CInt(parameter("error", data))
                    Dim discovery As Boolean = parameter("discovery", data)
                    m_stationFeed = parameter("stationfeed", data)
                    stationName = parameter("station", data)
                    If errorCode <> 0 Then
                        setFailed(WebRequestResultCode.WebRequestResult_Custom, "Error number " & errorCode & "occured!")
                    End If
                    m_MetaData = track
                End With
            End If
        End Sub
    End Class
End Namespace

