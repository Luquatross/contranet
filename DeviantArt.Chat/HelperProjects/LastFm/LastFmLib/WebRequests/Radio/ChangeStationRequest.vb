Imports LastFmLib.WebRequests
Imports LastFmLib.TypeClasses
'Example for the use of this class
'=================================
'Public Function TuneIn(ByVal LastFmUrl As String) As Boolean
'    'Create a class instance
'    Dim csr As New ChangeStationRequest(m_Handshake.Session)
'    'set baseHost and -Path by the ones of the handshake(as they might hav changed^^)
'    csr.BaseHost = m_Handshake.BaseHost
'    csr.BasePath = m_Handshake.BasePath
'    'If you want to set the language to other than english, e.g. german:
'    'csr.languageCode = "de"

'    'Set station url
'    csr.stationUrl = New StationUrl(LastFmUrl)
'    'Start the request
'    csr.Start()
'    'Return the result
'    Return csr.succeeded
'End Function
'
Namespace WebRequests
    ''' <summary>
    ''' This class is for changing the or tuning into a station.
    ''' </summary>
    Public Class ChangeStationRequest
        Inherits RequestBase
        Private m_session As String
        Private m_station As New Station()
        Private m_hasXspf As Boolean
        Private m_xspf As String

        Private m_langCode As String = Defaults.kLanguageCode
        ''' <summary>
        ''' Gets the XSPF data.
        ''' </summary>
        ''' <value>The XSPF.</value>
        ReadOnly Property xspf() As String
            Get
                Return m_xspf
            End Get
        End Property
        ''' <summary>
        ''' Gets a value indicating whether this instance has XSPF.
        ''' </summary>
        ''' <value><c>true</c> if this instance has XSPF; otherwise, <c>false</c>.</value>
        ReadOnly Property hasXspf() As Boolean
            Get
                Return m_hasXspf
            End Get
        End Property



        ''' <summary>
        ''' Gets or sets the session id.
        ''' </summary>
        ''' <value>The session id.</value>
        Property SessionId() As String
            Get
                Return m_session
            End Get
            Set(ByVal value As String)
                m_session = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the station URL.
        ''' </summary>
        ''' <value>The station URL.</value>
        Property Station() As Station
            Get
                Return m_station
            End Get
            Set(ByVal value As Station)
                m_station = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the language code. Default is Defaults.kLanguageCode.
        ''' </summary>
        ''' <value>The language code.</value>
        Property languageCode() As String
            Get
                Return m_langCode
            End Get
            Set(ByVal value As String)
                m_langCode = value
            End Set
        End Property
        ''' <summary>
        ''' Initializes a new instance of the <see cref="ChangeStationRequest" /> class.
        ''' </summary>
        ''' <param name="sessionId_">The sessionId(Optional).</param>
        ''' <param name="stationUrl_">The station url url, e.g. lastfm://user/tburny/playlist.</param>
        Public Sub New(ByVal sessionId_ As String, ByVal stationUrl_ As TypeClasses.StationUrl)
            MyBase.New(RequestType.ChangeStation, "ChangeStation")
            m_session = sessionId_
            m_station.Url = stationUrl_
        End Sub
        ''' <summary>
        ''' If there is a track Id(e.g. for previewing a track), call this method to set the right lastfm:// url.
        ''' </summary>
        ''' <param name="id">The id.</param>
        Public Sub setId(ByVal id As Integer)
            m_station.Url = New StationUrl("lastfm://play/tracks/" + CStr(id))
        End Sub


        Public Overrides Sub Start()
            Dim su As String = m_station.Url
            Dim urlwithoutprotocol As String = su
            If su.StartsWith("lastfm://") Then urlwithoutprotocol = su.Substring("lastfm://".Length)
            Dim url As String = IIf(urlwithoutprotocol.Contains("%"), urlwithoutprotocol, Uri.EscapeDataString(urlwithoutprotocol))
            Dim path As String
            If m_station.Url.isPlaylist Then
                path = "/1.0/webclient/getresourceplaylist.php?sk=" & m_session & _
                       "&url=lastfm://" & url & _
                       "&desktop=1"
            Else
                path = m_basePath & "/adjust.php?session=" & m_session & "&url=lastfm://" & url & "&lang=" & m_langCode
            End If
            [get](path)
        End Sub
        Protected Overrides Sub success(ByVal data As String)
            If hasXspf Then
                m_xspf = data
            Else
                If (parameter("response", data) <> "OK") Then
                    Dim errorcode As Integer = CInt(parameter("error", data))
                    Select Case errorcode
                        Case 1
                            setFailed(WebRequestResultCode.ChangeStation_NotEnoughContent, _
                                    tr("Sorry, there is not enough content to play this station. Please choose a different one."))
                        Case 2
                            setFailed(WebRequestResultCode.ChangeStation_TooFewGroupMembers, _
                                tr("This group does not have enough members for radio."))


                        Case 3
                            setFailed(WebRequestResultCode.ChangeStation_TooFewFans, _
                        tr("This artist does not have enough fans for radio."))


                        Case 4
                            setFailed(WebRequestResultCode.ChangeStation_Unavailable, _
                        tr("This item is not available for streaming."))


                        Case 5
                            setFailed(WebRequestResultCode.ChangeStation_SubscribersOnly, _
                                       tr("This station is available to subscribers only." & _
                                           "<p>" & "You can subscribe here: <a href='http://www.last.fm/subscribe/'>http://www.last.fm/subscribe/</a>"))


                        Case 6
                            setFailed(WebRequestResultCode.ChangeStation_TooFewNeighbours, _
                        tr("There are not enough neighbours for this radio mode."))


                        Case 7
                            setFailed(WebRequestResultCode.ChangeStation_StreamerOffline, _
                        tr("The streaming system is offline for maintenance, please try again later."))


                        Case 8
                            setFailed(WebRequestResultCode.ChangeStation_InvalidSession, _
                        tr("The streaming system is offline for maintenance, please try again later."))


                        Case Else
                            setFailed(WebRequestResultCode.ChangeStation_UnknownError, _
                        tr("Starting radio failed. Unknown error."))
                    End Select
                    'LOGL(1,"ChangeStationFailed" & errorMessage)
                Else
                    Dim url As String = parameter("url", data)
                    m_station.Url = url
                    m_station.Name = parameter("stationname", data)
                    If Not String.IsNullOrEmpty(m_station.Name) Then
                        m_station.Name = m_station.Name.Trim
                        'Make first letter upper case
                        m_station.Name = m_station.Name.Substring(0, 1).ToUpper & m_station.Name.Substring(1)
                    End If
                End If
            End If
        End Sub

    End Class

End Namespace