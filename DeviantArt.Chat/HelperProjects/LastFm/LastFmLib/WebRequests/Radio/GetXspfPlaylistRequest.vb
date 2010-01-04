Imports LastFmLib.WebRequests
Namespace WebRequests
    Public Class GetXspfPlaylistRequest
        Inherits RequestBase

        Private m_session As String

        ''' <summary>
        ''' Initializes a new instance of the <see cref="GetXspfPlaylistRequest" /> class.
        ''' </summary>
        ''' <param name="session">The sessionId.</param>
        ''' <param name="basePath">The base path.</param>
        Public Sub New(ByRef session As String, ByRef basePath As String)
            Me.New(session)
            m_basePath = basePath
        End Sub
        ''' <summary>
        ''' Initializes a new instance of the <see cref="GetXspfPlaylistRequest" /> class.
        ''' </summary>
        ''' <param name="session">The sessionId.</param>
        Public Sub New(ByRef session As String)
            MyBase.New(WebRequests.RequestType.GetXspfPlaylist, "GetXspfPlaylist")
            m_session = session

        End Sub
        Public Overrides Sub Abort()

        End Sub

        ''' <summary>
        ''' Starts the Request.
        ''' </summary>
        ''' <param name="doasync">If set to <c>true</c> then the request is made asynchronously.</param>
        Public Overrides Sub Start(ByVal doasync As Boolean)
            'int discovery = static_cast<int>( The::settings().currentUser().isDiscovery() );
            'Scrobbling/Discovery not implemented => discovery = 0
            Dim discovery As Integer = 0
            'QString path = m_basePath + "/xspf.php?" +
            '"sk=" + m_session + 
            '"&discovery=" + QString::number( discovery )  +
            '"&desktop=" + The::settings().version();
            Dim path As String = m_basePath & "/xspf.php?" & _
                                "sk=" & m_session & _
                                "&discovery=" & CStr(discovery) & _
                                "&desktop=" & "1.0.0.0"
            Debug.Print("Requesting new playlist....")
            [get](path, doasync)

        End Sub
        Public Overrides Function HeaderReceived(ByRef h As System.Net.HttpWebResponse) As Boolean
            'MyBase.HeaderReceived(h)
            If h.StatusCode = 401 Then
                setFailed(WebRequestResultCode.Playlist_InvalidSession, "Invalid session. Please re-handshake.")
            ElseIf h.StatusCode = 503 Then
                setFailed(WebRequestResultCode.Playlist_RecSysDown, "Sorry, the playlist service is not responding." & vbCrLf & _
                          "Please try again later.")
            End If
            Return True
        End Function

        'Zum Überladen
        Protected Overrides Sub success(ByVal data As String)
            'MyBase.success(data)
        End Sub
    End Class
End Namespace