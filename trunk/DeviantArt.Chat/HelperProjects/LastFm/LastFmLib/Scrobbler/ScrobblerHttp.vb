Imports System.Net
Namespace Scrobbler
    ''' <summary>
    ''' The Scrobbler HTTP. Internally needed for scrobbling
    ''' </summary>
    Public Class ScrobblerHttp
        Public Event done(ByRef data As String)
        Public Event requestFinished(ByVal i As Integer, ByVal b As Boolean)
        Public Event ResponseHeaderReceived(ByVal h As HttpStatusCode)
        Protected m_id As Integer
        Protected WithEvents m_retry_timer As New System.Timers.Timer
        Protected WithEvents _http As HttpWebRequest
        Protected m_host As String = "post.audioscrobbler.com"
        Protected m_path As String
        Protected m_port As Integer = 80
        Protected _data As String
        Protected m_cancelled As Boolean = False
        ''' <summary>
        ''' Gets the host.
        ''' </summary>
        ''' <value>The host.</value>
        ReadOnly Property host() As String
            Get

                Return m_host
            End Get

        End Property
        ReadOnly Property Cancelled() As Boolean
            Get
                Return m_cancelled
            End Get
        End Property
        Sub New()
            m_retry_timer = New Timers.Timer()
            m_retry_timer.AutoReset = False
            AddHandler m_retry_timer.Elapsed, New Timers.ElapsedEventHandler(AddressOf m_retry_timer_Tick)
            resetRetryTimer()
            m_id = 0
        End Sub
        Overridable Overloads Sub setUrl(ByVal url As Uri)
            m_path = url.PathAndQuery
            m_host = url.Host
            m_port = url.Port
        End Sub
        Overridable Overloads Sub setUrl(ByVal url As String)
            setUrl(New Uri(url))
        End Sub
        Sub setHost(ByVal host As String, Optional ByVal port As Integer = 80)
            m_host = host
            m_port = port
        End Sub
        ''' <summary>
        ''' Retrieves the Request asyncronous using the GET Method. If successful, success() is called.
        ''' </summary>
        ''' <param name="host">The host.</param>
        ''' <param name="path">The path.</param>
        Protected Sub [get](ByVal host As String, ByVal path As String, Optional ByVal doasync As Boolean = False)
            [get](host & IIf(path.Substring(0, 1) = "/", path, "/" & path))
        End Sub


        ''' <summary>
        ''' Retrieves the Request asyncronous using the GET Method. If successful, success() is called.
        ''' </summary>
        ''' <param name="path">The RELATIVE path(baseHost is added before "path").</param>
        Protected Sub [get](ByVal path As String, Optional ByVal doasync As Boolean = False)
            'Me.get(New Uri("http://" & _baseHost & IIf(path.Substring(0, 1) = "/", path, "/" & path)))
            [get](New Uri("http://" & m_host & path))
        End Sub




        ''' <summary>
        ''' Retrieves the Request asyncronous using the GET Method. If successful, success() is called.
        ''' </summary>
        ''' <param name="path">The absolute System.Uri from where it is tried to get the request/response</param>
        Public Sub [get](ByVal path As System.Uri, Optional ByVal doasync As Boolean = False)
            Dim dga As DoGetAsyncDelegate
            If doasync Then
                dga = New DoGetAsyncDelegate(AddressOf DoGetAsync)
                dga.BeginInvoke(path, New AsyncCallback(AddressOf DoGetAsyncCallBack), New Object)
            Else
                DoGetAsync(path)
            End If
        End Sub
        Private Delegate Sub DoGetAsyncDelegate(ByVal path As System.Uri)
        Private Sub DoGetAsync(ByVal path As System.Uri)
            Dim response As WebResponse

            _http = Net.HttpWebRequest.Create(path)
            _http.Timeout = Defaults.kRequestTimeoutInMs
            'use cache
            _http.CachePolicy = New Net.Cache.HttpRequestCachePolicy(Net.Cache.HttpRequestCacheLevel.CacheIfAvailable)
            _http.Method = "GET"
            '_http.Headers.Add(Net.HttpRequestHeader.Host, path.Host)
            _http.Headers.Add(Net.HttpRequestHeader.AcceptLanguage, "en")
            Try
                response = _http.GetResponse()
            Catch w As WebException
                Debug.Print("ScrobblerHTTP: Handshake fehlgeschlagen: " & w.Message)
                retry()
                Exit Sub
            Catch t As TimeoutException
                Debug.Print("ScrobblerHTTP: Handshake fehlgeschlagen: " & t.Message)
                retry()
                Exit Sub
            End Try
            Debug.Print("ScrobblerHTTP: Handshake successful")
            success(response)
        End Sub
        Overridable Sub DoGetAsyncCallBack(ByVal ar As IAsyncResult)

        End Sub
        'Public Sub Post(ByVal data As String)
        '    Post(m_path, data)
        'End Sub
        Protected Sub Post(ByVal path As String, ByVal data As String, Optional ByVal doasync As Boolean = False)
            Dim dra As New DoRequestAsyncDelegate(AddressOf DoRequestAsync)
            If doasync Then
                dra.BeginInvoke(path, data, doasync, New AsyncCallback(AddressOf DoRequestAsyncCallBack), New Object)
            Else
                DoRequestAsync(path, data, doasync)
            End If
        End Sub
        Private Delegate Sub DoRequestAsyncDelegate(ByVal path As String, ByVal data As String, ByVal doasync As Boolean)
        Private Sub DoRequestAsync(ByVal path As String, ByVal data As String, Optional ByVal doasync As Boolean = False)
            Dim requestBytes() As Byte = System.Text.Encoding.UTF8.GetBytes(data)
            m_cancelled = False
            ' Create a request using a URL that can receive a post. 
            _http = HttpWebRequest.Create(New Uri("http://" & m_host & path))
            _http.Timeout = Defaults.kRequestTimeoutInMs
            ' Set the Method property of the request to POST.
            _http.Method = "POST"
            _http.Accept = "*/*"
            '_http.Headers.Add(HttpRequestHeader.Host, _http.RequestUri.Host)

            ' Create POST data and convert it to a byte array.
            Dim byteArray As Byte() = System.Text.Encoding.UTF8.GetBytes(data)
            ' Set the ContentLength property of the WebRequest.
            _http.ContentType = "application/x-www-form-urlencoded"
            _http.ContentLength = byteArray.Length
            _http.KeepAlive = False
            ' Get the request stream.
            Dim dataStream As IO.Stream = _http.GetRequestStream()
            ' Write the data to the request stream.
            dataStream.Write(byteArray, 0, byteArray.Length)
            ' Close the Stream object.
            dataStream.Close()
            Dim response As HttpWebResponse = Nothing
            Try
                ' Get the response.
                response = _http.GetResponse()

            Catch w As WebException
                If w.Status = WebExceptionStatus.RequestCanceled Then
                    m_cancelled = True
                    Debug.Print("ScrobblerHTTP: Posting to " & _http.RequestUri.AbsoluteUri & " cancelled.")
                    Exit Sub
                End If
                RaiseEvent ResponseHeaderReceived(HttpStatusCode.SeeOther)
                Debug.Print("ScrobblerHTTP: Posting data failed")
                Exit Sub
            Catch t As TimeoutException
                Debug.Print("ScrobblerHTTP: POST-ing data timed out")
                retry()
                Exit Sub
            End Try
            RaiseEvent ResponseHeaderReceived(response.StatusCode)
            Debug.Print("ScrobblerHTTP: Successfully posted data (status code: " & response.StatusCode & ")")
            success(response)


        End Sub
        Sub cb()

        End Sub
        Overridable Sub DoRequestAsyncCallBack(ByVal ar As IAsyncResult)

        End Sub
        Private Sub m_retry_timer_Tick(ByVal sender As Object, ByVal e As System.Timers.ElapsedEventArgs) Handles m_retry_timer.Elapsed
            m_retry_timer.Stop()
            Debug.Print("ScrobblerHTTP: Method is " & IIf(_http Is Nothing, "", _http.Method))
            Select Case _http.Method.ToLower
                Case "get"
                    Me.get(m_host, m_path)
                Case "post"
                    Me.Post(m_path, _data)
                Case Else
                    'Do nothing
            End Select

        End Sub
        Public Function retry() As Integer
            With m_retry_timer
                If .Interval < 120 * 60 * 1000 Then .Interval = .Interval * 2
                Debug.Print("ScrobblerHTTP: retrying in " & .Interval & " ms")
                m_retry_timer.Start()
                Return .Interval
            End With
        End Function
        Sub resetRetryTimer()
            m_retry_timer.Interval = 15 * 1000
        End Sub
        ''' <summary>
        ''' Is called if the request was successful. Overriden by many deriving classes.
        ''' </summary>
        ''' <param name="data">The data.</param>
        Protected Overridable Sub success(ByVal data As String)
            RaiseEvent done(data)
        End Sub
        ''' <summary>
        ''' Is called if the request was successful. Overriden by many deriving classes.
        ''' </summary>
        ''' <param name="data">The data.</param>
        Overridable Sub success(ByVal data() As Byte)
            success(System.Text.Encoding.UTF8.GetString(data))
        End Sub
        ''' <summary>
        ''' Is called if the request was successful. Overriden by many deriving classes.
        ''' </summary>
        ''' <param name="response">The response.</param>
        Protected Sub success(ByVal response As Net.WebResponse)
            Dim s As New IO.StreamReader(response.GetResponseStream, System.Text.Encoding.UTF8)
            'HIER ENCODING NOCHMAL ÜBERPRÜFEN, ggf ascii statt utf-8
            Dim res() As Byte = System.Text.Encoding.UTF8.GetBytes(s.ReadToEnd)
            _data = System.Text.Encoding.UTF8.GetString(res)
            success(res)
        End Sub

        Sub Abort()
            If _http IsNot Nothing Then _http.Abort()
            Debug.Print("ScrobblerHttp: Request(Method=" & _http.Method & ") of URI" & _http.RequestUri.AbsoluteUri & " aborted.")
        End Sub
    End Class

End Namespace
