Imports System.Net
Imports System.Xml
'How to create a class
'=====================
'1. Inherit RequestBase
'2. Override Start() because it does nothing in the BaseClass
'3. Override success(String) or any other success method(be sure to comment out the mybase.success(...) thing!)
'4. Put your result source code in there
'5. Give own, additional Propertys to it
'6. To start the request, just call [requestInstance].Start()
'7. Check with [requestInstance].succeeded if it was successful

Namespace WebRequests
#Const REQUEST_DEBUG_OUTPUT = 0
    '0 - Disables debug output for requests
    '1 - Enables it

    ''' <summary>
    ''' This is the base class for all requests. To create(means code) a new request, just inherit this class.
    ''' </summary>


    Public MustInherit Class RequestBase
        Inherits System.Object
        'Implements IFromData
        Public Event Result(ByRef request As RequestBase)

        Private Const MAX_RETRIES As Integer = 4
        Private tries As Integer
        Dim m_type As RequestType
        Dim m_responseHeaderCode As Integer
        Dim m_result As WebRequestResultCode
        Dim m_auto_delete As Boolean
        Dim m_errorMessage As String
        Protected m_baseHost As String = "ws.audioscrobbler.com"
        Protected m_basePath As String = "/radio"
        Protected m_languageCode As String = Defaults.kLanguageCode
        Dim m_http As Net.HttpWebRequest
        Protected m_name As String
        Protected m_requestData As Object
        Private m_timeout As Integer = Defaults.kRequestTimeoutInMs
        Private m_cancelled As Boolean = False
        Protected m_cacheable As Boolean = True
        Protected m_timestamp As Integer

        Property Timestamp() As Integer
            Get
                Return m_timestamp
            End Get
            Set(ByVal value As Integer)
                m_timestamp = value
            End Set
        End Property
        ReadOnly Property TimeStampDate() As DateTime
            Get
                Return UnixTime.GetDateOfUnixTime(m_timestamp)
            End Get
        End Property
        ReadOnly Property IsCachable() As Boolean
            Get
                Return m_cacheable
            End Get
        End Property
        Enum WebRequestResultCode

            ' classRequest codes
            ' ------------------------------------------------------------------------
            Request_Undefined
            Request_Success

            '/ We aborted it so the user prolly doesn't care
            Request_Aborted

            '/ DNS failed
            Request_HostNotFound

            '/ HTTP response code
            Request_BadResponseCode

            '/ We've timed out waiting for an HTTP response several times
            Request_NoResponse

            Request_SerializationError

            ' class Handshake codes
            ' ------------------------------------------------------------------------
            Handshake_WrongUserNameOrPassword
            Handshake_Banned
            Handshake_SessionFailed

            ' class ChangeStationRequest codes
            ' ------------------------------------------------------------------------
            ChangeStation_NotEnoughContent     ' there is not enough content to play this station.
            ChangeStation_TooFewGroupMembers   ' this group does not have enough members for radio.
            ChangeStation_TooFewFans           ' this artist does not have enough fans for radio.
            ChangeStation_TooFewNeighbours     ' there are not enough neighbours for this radio.
            ChangeStation_Unavailable          ' this item is not available for streaming.
            ChangeStation_SubscribersOnly      ' this feature is only available to subscribers.
            ChangeStation_StreamerOffline      ' the streaming system is offline for maintenance
            ChangeStation_InvalidSession       ' session has timed out please re-handshake
            ChangeStation_UnknownError         ' no idea

            ' class GetXspfPlaylistRequest codes
            ' ------------------------------------------------------------------------
            Playlist_InvalidSession            ' 401 session timed out need to re-handshake
            Playlist_RecSysDown                ' 503 recommendation systems down treat as connection error

            ' class <insert new class name here> codes
            ' ------------------------------------------------------------------------


            '/ Custom undefined error
            WebRequestResult_Custom = 1000
        End Enum
        Enum RadioErrorCode

            RADIO_ERROR_NOT_ENOUGH_CONTENT = 1 ' There is not enough content to play this station.
            RADIO_ERROR_FEWGROUPMEMBERS        ' This group does not have enough members for radio.
            RADIO_ERROR_FEWFANS                ' This artist does not have enough fans for radio.
            RADIO_ERROR_UNAVAILABLE            ' This item is not available for streaming.
            RADIO_ERROR_SUBSCRIBE              ' This feature is only available to subscribers.
            RADIO_ERROR_FEWNEIGHBOURS          ' There are not enough neighbours for this radio.
            RADIO_ERROR_OFFLINE                 ' The streaming system is offline for maintenance please try again later
        End Enum

        ''' <summary>
        ''' Gets or sets the request timeout(in milliseconds).
        ''' </summary>
        ''' <value>The request timeout. Minimum is 1000.</value>
        ''' <exception cref="ArgumentOutOfRangeException">Thrown if the timeout is set to less than 1000 milliseconds.</exception>
        Property RequestTimeout() As Integer
            Get
                Return m_timeout
            End Get
            Set(ByVal value As Integer)
                If value >= 1000 Then
                    m_timeout = value
                Else
                    Throw New ArgumentOutOfRangeException("value", "The timeout has to be more than 1000 (ms)!")
                End If
            End Set
        End Property
        ReadOnly Property Cancelled() As Boolean
            Get
                Return m_cancelled
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the base path(default is "/radio").
        ''' </summary>
        ''' <value>The base path.</value>
        Public Property BasePath() As String
            Get
                Return m_basePath
            End Get
            Set(ByVal value As String)
                m_basePath = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the base host(default is "ws.audioscrobbler.com").
        ''' </summary>
        ''' <value>The base host .</value>
        Public Property BaseHost() As String
            Get
                Return m_baseHost
            End Get
            Set(ByVal value As String)
                m_baseHost = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the request data.
        ''' </summary>
        ''' <value>The data.</value>
        Public Property RequestData() As Object
            Get
                Return m_requestData
            End Get
            Set(ByVal value As Object)
                m_requestData = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the name of the request.
        ''' </summary>
        ''' <value>The name.</value>
        Property Name() As String
            Get
                Return m_name
            End Get
            Set(ByVal value As String)
                m_name = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the type of the request.
        ''' </summary>
        ''' <value>The type.</value>
        Property Type() As RequestType
            Get
                Return m_type
            End Get
            Set(ByVal value As RequestType)
                m_type = value
            End Set
        End Property
        ''' <summary>
        ''' Gets the response header code.
        ''' </summary>

        ReadOnly Property ResponseHeaderCode() As Integer
            Get
                Return m_responseHeaderCode
            End Get
        End Property

        ''' <summary>
        ''' Gets the result code(if it was successful or type of error).
        ''' </summary>

        ReadOnly Property resultCode() As WebRequestResultCode
            Get
                Return m_result
            End Get
        End Property

        ''' <summary>
        ''' Gets a value indicating whether this request failed.
        ''' </summary>
        ''' <value><c>true</c> if failed; otherwise, <c>false</c>.</value>
        ReadOnly Property failed() As Boolean
            Get
                Return (m_result <> WebRequestResultCode.Request_Success)
            End Get
        End Property
        ''' <summary>
        ''' Gets a value indicating whether this request was successful.
        ''' </summary>
        ''' <value><c>true</c> if succeeded; otherwise, <c>false</c>.</value>
        ReadOnly Property succeeded() As Boolean
            Get
                If (m_result = WebRequestResultCode.Request_Success) Then
                    Return True
                Else
                    Return False
                End If
            End Get
        End Property

        ''' <summary>
        ''' Gets a value indicating whether this request was aborted.
        ''' </summary>
        ''' <value><c>true</c> if aborted; otherwise, <c>false</c>.</value>
        ReadOnly Property aborted() As Boolean
            Get
                If (m_result = WebRequestResultCode.Request_Aborted) Then
                    Return True
                Else
                    Return False
                End If
            End Get
        End Property

        ''' <summary>
        ''' Gets the error message, if there is one.
        ''' </summary>
        ''' <value>The error message.</value>
        ReadOnly Property errorMessage() As String
            Get
                Return m_errorMessage
            End Get
        End Property

        ''' <summary>
        ''' Unknown functionality. Probably for request cache, dunno.
        ''' </summary>
        ''' <value><c>true</c> if [auto delete]; otherwise, <c>false</c>.</value>
        Property autoDelete() As Boolean
            Get
                Return m_auto_delete
            End Get
            Set(ByVal value As Boolean)
                m_auto_delete = value
            End Set
        End Property

        ''' <summary>
        ''' Initializes a new instance of the <see cref="RequestBase" /> class.
        ''' </summary>
        Protected Sub New()
            If (String.IsNullOrEmpty(m_baseHost)) Then
                m_baseHost = IIf(System.Environment.CommandLine.Contains("--debug"), _
                            "wsdev.audioscrobbler.com", _
                            "ws.audioscrobbler.com")
                m_cacheable = GetCacheableState(Type)
                m_timestamp = UnixTime.GetUnixTime()
            End If

        End Sub

        ''' <summary>
        ''' Initializes a new instance of the <see cref="RequestBase" /> class.
        ''' </summary>
        ''' <param name="type">The type of the request.</param>
        ''' <param name="name">The name of the request.</param>
        Public Sub New(ByVal type As RequestType, Optional ByVal name As String = "")
            Me.New()
            Me.m_type = type
            Me.m_name = IIf(String.IsNullOrEmpty(m_name), m_type.ToString(), "") 'm_type.ToString() & "Request", name)
        End Sub

        Public Sub New(ByVal data As String)
            Me.New()
            success(data)
        End Sub
        ''' <summary>
        ''' Starts the request and gets the response. Should be overrridden.
        ''' </summary>
        Public Overridable Overloads Sub Start()
            Throw New NotSupportedException("The start method has to be overridden to do anything.")
        End Sub
        ''' <summary>
        ''' Starts the Request.
        ''' </summary>
        ''' <param name="doasync">If set to <c>true</c> then the request is made asynchronously.</param>
        Public Overridable Overloads Sub Start(ByVal doasync As Boolean)
            Throw New NotSupportedException("The current class did not overload Start([Boolean]), so asynchonous operations are unsupported!")
        End Sub
        ''' <summary>
        ''' Aborts the Request.
        ''' </summary>

        Public Overridable Sub Abort()
            m_http.Abort()
        End Sub
        ''' <summary>
        ''' Does a retry until MAX_RETRIES by launching the Start() method
        ''' </summary>

        Public Overridable Sub TryAgain()

            Start()
            tries += 1
            If tries = MAX_RETRIES Then
                setFailed(WebRequestResultCode.Request_NoResponse, tr("No response from server!"))
                tries = 0
            End If

        End Sub


        ''' <summary>
        ''' Retrieves the Request asyncronous using the GET Method. If successful, success() is called.
        ''' </summary>
        ''' <param name="host">The host.</param>
        ''' <param name="path">The path.</param>
        Public Sub [get](ByVal host As String, ByVal path As String, Optional ByVal doasync As Boolean = False)
            [get](host & IIf(path.Substring(0, 1) = "/", path, "/" & path))
        End Sub
        ''' <summary>
        ''' Retrieves the Request asyncronous using the GET Method. If successful, success() is called.
        ''' </summary>
        ''' <param name="path">The RELATIVE path(baseHost is added before "path").</param>
        Public Sub [get](ByVal path As String, Optional ByVal doasync As Boolean = False)
            'Me.get(New Uri("http://" & _baseHost & IIf(path.Substring(0, 1) = "/", path, "/" & path)))
            [get](New Uri("http://" & m_baseHost & path))
        End Sub
        ''' <summary>
        ''' Retrieves the Request asyncronous using the GET Method. If successful, success() is called.
        ''' </summary>
        ''' <param name="path">The absolute System.Uri from where it is tried to get the request/response</param>
        Public Sub [get](ByVal path As System.Uri, Optional ByVal doasync As Boolean = False)
            Dim dga As DoGetAsyncDelegate
            If doasync Then
                dga = New DoGetAsyncDelegate(AddressOf DoGet)
                'Don't know if this is right or not for Async operations, maybe some code is missing...
                Dim ac As New AsyncCallback(AddressOf DoGetAsyncCallBack)
                dga.BeginInvoke(path, ac, New Object)

            Else
                DoGet(path)
            End If
        End Sub
        Private Delegate Sub DoGetAsyncDelegate(ByVal path As System.Uri)
        Private Sub DoGet(ByVal path As System.Uri)
            Dim response As HttpWebResponse
            m_cancelled = False
            m_http = Net.HttpWebRequest.Create(path)
            m_http.Method = "GET"
            '_http.Headers.Add(Net.HttpRequestHeader.Host, path.Host)
            m_http.Headers.Add(Net.HttpRequestHeader.AcceptLanguage, "en")
            '            m_http.Connection = "Close"
            m_http.ProtocolVersion = New Version(1, 0)
            m_http.Timeout = m_timeout
            Try

#If REQUEST_DEBUG_OUTPUT = 1 Then
                Debug.Print("Request " & Me.Name & " gets " & path.AbsoluteUri)
#End If

                response = m_http.GetResponse()
            Catch w As WebException
                'MsgBox("Request " & Me.Name & " fehlgeschlagen: " & w.Message)
                If w.Status = WebExceptionStatus.RequestCanceled Then
                    m_cancelled = True
                    Exit Sub
                End If
                Me.setFailed(WebRequestResultCode.Request_BadResponseCode, w.Message)
                Exit Sub
            End Try
            HeaderReceived(response)
            m_result = WebRequestResultCode.Request_Success
            success(response)

            RaiseEvent Result(Me)
        End Sub
        ''' <summary>
        ''' Here deriving classes can handle the async callback of the DoGet(True)-Method
        ''' </summary>
        ''' <param name="ar">The ar.</param>
        Overridable Sub DoGetAsyncCallBack(ByVal ar As IAsyncResult)

        End Sub

        Protected Sub Request(ByRef rpc As XMLRpc, Optional ByVal doasync As Boolean = False)
            Dim dra As New DoRequestAsyncDelegate(AddressOf DoRequest)
            If doasync Then
                dra.BeginInvoke(rpc, New AsyncCallback(AddressOf DoRequestAsyncCallBack), New Object)
            Else
                DoRequest(rpc)
            End If
        End Sub
        Private Delegate Sub DoRequestAsyncDelegate(ByRef rpc As XMLRpc)
        ''' <summary>
        ''' Does the request using the "POST"-method.
        ''' </summary>
        ''' <param name="rpc">The RPC.</param>
        Private Sub DoRequest(ByRef rpc As XMLRpc)
            Dim xml As String = rpc.toString
#If REQUEST_DEBUG_OUTPUT = 1 Then
            Debug.Print("Request " & Me.Name & " posted the following data to /1.0/rw/xmlrpc.php: ")
            Debug.Indent()
            Debug.Print(xml)
            Debug.Unindent()
#End If
            ' Dim requestBytes() As Byte = System.Text.Encoding.UTF8.GetBytes(xml)
            ' Create a request using a URL that can receive a post. 
            System.Net.ServicePointManager.Expect100Continue = False
            Dim _http As HttpWebRequest = WebRequest.Create(New Uri("http://" & m_baseHost & "/1.0/rw/xmlrpc.php"))
            System.Net.ServicePointManager.Expect100Continue = False
            _http.ServicePoint.Expect100Continue = False
            _http.Timeout = 20 * 1000
            '_http.Connection = "Close"
            _http.ProtocolVersion = New Version(1, 0)
            ' Set the Method property of the request to POST.
            _http.Method = "POST"
            ' Create POST data and convert it to a byte array.
            Dim byteArray As Byte() = System.Text.Encoding.UTF8.GetBytes(xml)
            ' Set the ContentLength property of the WebRequest.
            _http.ContentLength = byteArray.Length

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
                'MsgBox("Request " & Me.Name & " fehlgeschlagen: " & w.Message)
                Me.setFailed(WebRequestResultCode.Request_BadResponseCode, w.Message)
                Exit Sub
            End Try
            HeaderReceived(response)
            success(response)
            m_result = WebRequestResultCode.Request_Success

            RaiseEvent Result(Me)

        End Sub
        ''' <summary>
        ''' Here deriving classes can handle the async callback of the DoRequest(True)-Method
        ''' </summary>
        ''' <param name="ar">The ar.</param>
        Overridable Sub DoRequestAsyncCallBack(ByVal ar As IAsyncResult)

        End Sub
        ''' <summary>
        ''' Is called if the request was successful. 
        ''' </summary>
        ''' <param name="data">The data.</param>
        Protected Overridable Sub success(ByVal data As String)
            Dim a As Boolean = False
        End Sub
        ''' <summary>
        ''' Is called if the request was successful. Converts Bytes to UTF-8 and calls success(String).
        ''' </summary>
        ''' <param name="data">The data.</param>
        Protected Overridable Sub success(ByVal data() As Byte)
            success(System.Text.Encoding.UTF8.GetString(data))
        End Sub
        ''' <summary>
        ''' Is called if the request was successful. Calls success(byte()).
        ''' </summary>
        ''' <param name="response">The response.</param>
        Protected Sub success(ByVal response As Net.WebResponse)
            Dim s As New IO.StreamReader(response.GetResponseStream, System.Text.Encoding.UTF8)
            'HIER ENCODING NOCHMAL ÜBERPRÜFEN, ggf ascii statt utf-8
            Dim res() As Byte = System.Text.Encoding.UTF8.GetBytes(s.ReadToEnd)
            m_requestData = System.Text.Encoding.UTF8.GetString(res)
            success(res)
        End Sub
        ''' <summary>
        ''' Sets the reason why the request failed
        ''' </summary>
        ''' <param name="type">The type.</param>
        ''' <param name="message">The message.</param>
        Protected Sub setFailed(ByVal type As WebRequestResultCode, Optional ByVal message As String = "")
            m_result = type
            If Not (String.IsNullOrEmpty(message)) Then
                m_errorMessage = message
                Debug.Print("Request " & Me.m_name & " failed: " & m_errorMessage)
            End If
        End Sub

        ''' <summary>
        ''' Gets the value of a Key-Value-Pair(e.g Abc=bbbb)-string if name can be found in the data string
        ''' </summary>
        ''' <param name="keyname">The keyname.</param>
        ''' <param name="data">The data string.</param>
        ''' <returns></returns>
        Protected Function parameter(ByVal keyname As String, ByVal data As String) As String
            Dim list() As String = data.Split(Chr(10))

            For Each s As String In list
                If Not String.IsNullOrEmpty(s) Then
                    If s.Contains("=") Then
                        If keyname.ToLower = s.Substring(0, s.IndexOf("=")).ToLower Then
                            Return s.Substring(s.IndexOf("=") + 1)
                        End If
                    End If
                End If
            Next
            Return Nothing
        End Function

        '''' <summary>
        '''' Initializes an instance from an valid XmlElement containing a request response.
        '''' </summary>
        '''' <param name="data">The data.</param>
        '''' <returns>
        '''' An instance of a RequestBase derivate which supports this method, throws an exception else.
        '''' </returns>
        '''' <exception cref="NotSupportedException">Throws a <c>NotSupportedException</c> if the class does not support this way of initializaltion</exception>
        'Protected Shared Function InitFromXmlElement(ByVal data As Xml.XmlElement) As RequestBase
        '    Throw New NotSupportedException("The method 'InitFromData' was not overridden by the deriving class(unsuppported) or you tried to call this method from RequestBase!")
        'End Function
        '''' <summary>
        '''' Initializes an instance from a string containing a request response.
        '''' </summary>
        '''' <param name="data">The data.</param>
        '''' <returns>
        '''' An instance of a RequestBase derivate which supports this method, throws an exception else.
        '''' </returns>
        '''' <exception cref="NotSupportedException">Throws a <c>NotSupportedException</c> if the class does not support this way of initializaltion</exception>
        'Private Shared Function InitFromString(ByVal data As String) As RequestBase
        '    Throw New NotSupportedException("The method 'InitFromData' was not overridden by the deriving class(unsuppported) or you tried to call this method from RequestBase!")
        'End Function
        ''' <summary>
        ''' Called if the Response header was received. Returns if the status code is "200 OK" or not
        ''' </summary>
        ''' <param name="h">The HTTPWebResponse.</param>
        ''' <returns></returns>
        Overridable Function HeaderReceived(ByRef h As HttpWebResponse) As Boolean
            Return IIf(h.StatusCode = 200, True, False)
        End Function
        ''' <summary>
        ''' Escapes a string to and uri conform string.
        ''' </summary>
        ''' <param name="data">The data.</param>
        ''' <returns></returns>
        Shared Function EscapeUriData(ByVal data As String) As String
            Dim result As String
            'Double encode because of last.fm's "&"-bug
            result = Uri.EscapeDataString(Uri.EscapeDataString(data))
            'Debug.Print("Escaped " & data & " to " & result)
            Return result
        End Function
        ''' <summary>
        ''' Converts a URI-Escaped string into human-readable data.
        ''' </summary>
        ''' <param name="data">The data.</param>
        ''' <returns></returns>
        Shared Function UnEscapeUriData(ByVal data As String) As String
            Return Uri.UnescapeDataString(Uri.UnescapeDataString(data))
        End Function
        Protected Function GetUrl(ByVal s As String) As Uri
            If String.IsNullOrEmpty(s) Then Return Nothing
            If Not Uri.IsWellFormedUriString(s, UriKind.RelativeOrAbsolute) Then
                Return Nothing
            End If

            Return New Uri(s)
        End Function

        Shared Function GetCacheableState(ByVal type As RequestType) As Boolean
            Select Case type
                Case RequestType.AddTrackToMyPlaylist
                    Return False
                Case RequestType.BanTrack
                    Return False
                Case RequestType.ChangeStation
                    Return False
                Case RequestType.DeleteFriend
                    Return False
                Case RequestType.EnableDiscoveryMode
                    Return False
                Case RequestType.EnableScrobbling
                    Return False
                Case RequestType.GetXspfPlaylist
                    Return False
                Case RequestType.Handshake
                    Return False
                Case RequestType.LoveTrack
                    Return False
                Case RequestType.RadioMetaData
                    Return False
                Case RequestType.Recommend
                    Return False
                Case RequestType.ReportRebuffering
                    Return False
                Case RequestType.SetTag
                    Return False
                Case RequestType.Skip
                    Return False
                Case RequestType.UnBanTrack
                    Return False
                Case RequestType.UnListen
                    Return False
                Case RequestType.UnLoveTrack
                    Return False
                Case RequestType.VerifyUser
                    Return False
                Case RequestType.WebService
                    Return False
                Case Else
                    Return True
            End Select
        End Function

        Protected Shared Function IsValidForDeserialization(ByVal data As Xml.XmlElement, ByVal type As RequestType) As Boolean
            If data.Name = "request" Then
                If data.HasAttribute("type") Then
                    Dim val As Integer = CInt(data.GetAttribute("type"))
                    If val > 0 And RequestType.IsDefined((New RequestType).GetType, val) Then
                        If type Then
                        Else
                            Throw New Exceptions.ParseException("The attribute 'type' has to contain an Integer > 0! Moreover, this value has to be caontained in RequestType!")
                            Return False
                        End If

                    Else
                        Throw New Exceptions.ParseException("'data' has no attribute determining the request type!")
                        Return False
                    End If
                End If
                If data.HasAttribute("timestamp") Then
                    Dim val As Integer = CInt(data.GetAttribute("timestamp"))
                    'Value is a unix time stamp: 2008-01-01, 00:00
                    'If val < Defaults.kAudioscrobblerFoundingTimeStamp Then
                    '    'm_timestamp = Defaults.kAudioscrobblerFoundingTimeStamp
                    'Else
                    '    Throw New Exceptions.ParseException("'data' has no attribute determining the request type!")
                    '    Return False
                    'End If
                End If
            End If
            Return True
        End Function

        ''' <summary>
        ''' Serialzes this instance into an - for RequestCache - valid XmlElement
        ''' </summary>
        ''' <param name="doc">The XmlDocument</param>
        ''' <returns>
        ''' If successful, a XmlElement is returned, else <c>null</c>
        ''' </returns>
        Public Function ToXmlElement(ByRef doc As Xml.XmlDocument) As Xml.XmlElement
            If GetCacheableState(m_type) = True Then
                Dim e As Xml.XmlElement = doc.CreateElement("request")
                Dim attr As Xml.XmlAttribute
                'for custom names
                If m_type.ToString <> m_name Then
                    attr = doc.CreateAttribute("name")
                    attr.Value = m_name
                End If
                attr = doc.CreateAttribute("type")
                attr.Value = CInt(m_type)
                e.Attributes.Append(attr)
                attr = doc.CreateAttribute("timestamp")
                attr.Value = CStr(m_timestamp)

                Dim cdata As Xml.XmlCDataSection = doc.CreateCDataSection(RequestData)
                e.AppendChild(cdata)
                Return e

            Else
                Return Nothing
            End If
        End Function
        Protected Sub GetTimeStampFromElem(ByRef e As Xml.XmlElement)
            If e.HasAttribute("timestamp") Then
                m_timestamp = CInt(e.GetAttribute("timestamp"))
            Else
                m_timestamp = Defaults.kAudioscrobblerFoundingTimeStamp
            End If
        End Sub
        Protected Function GetXmlElemRequestData(ByVal e As Xml.XmlElement) As String
            Return e.InnerText
        End Function

        'Sub NewFromElement(ByVal e As Xml.XmlElement)
        '    If Not IsValidForDeserialization(e, Me.Type) Then Me.Finalize()
        '    GetTimeStampFromElem(e)
        '    success(data)
        'End Sub
        'Protected Shared Function CreateRequest(ByVal e As Xml.XmlElement) As T
        '    Dim result As T

        '    If (IsValidForDeserialization(e, RequestType.ArtistMetaData)) Then
        '        result = New T(e)
        '    End If
        '    Return result
        'End Function
    End Class

    Partial Class ArtistMetadataRequest
        Private Sub New()
            MyBase.New(RequestType.ArtistMetaData)
        End Sub
        ''' <summary>
        ''' Initializes a new instance of the <see cref="ArtistMetadataRequest" /> class. For doing so it deserializes and xml element.
        ''' </summary>
        ''' <param name="e">The xml element.</param>
        Private Sub New(ByVal e As XmlElement)
            Me.New()
            If Not IsValidForDeserialization(e, Me.Type) Then setFailed(WebRequestResultCode.Request_SerializationError, "Xml element content was invalid!")
            If e.HasAttribute("name") Then m_name = e.GetAttribute("name")

            GetTimeStampFromElem(e)

            Dim data As String = GetXmlElemRequestData(e)
            If Not String.IsNullOrEmpty(data) Then
                m_requestData = data
                Me.success(data)
            Else
                setFailed(WebRequestResultCode.Request_Undefined, "No data for deserialization found!")
            End If

        End Sub
        ''' <summary>
        ''' Initializes an instance of this class by deserializing an XmlElement
        ''' </summary>
        ''' <param name="e">The data.</param>
        ''' <returns>
        ''' If successful, a ArtistMetadataRequest is returned, else <c>null</c>
        ''' </returns>
        Public Shared Function InitFromXmlElement(ByVal e As XmlElement) As ArtistMetadataRequest
            Dim result As ArtistMetadataRequest = Nothing

            If (IsValidForDeserialization(e, RequestType.ArtistMetaData)) Then
                result = New ArtistMetadataRequest(e)
            End If
            Return result
        End Function
    End Class
End Namespace
