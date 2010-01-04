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

Namespace API20.Base
#Const REQUEST_DEBUG_OUTPUT = 0
    '0 - Disables debug output for requests
    '1 - Enables it

    ''' <summary>
    ''' This is the base class for all requests. To create(means code) a new request, just inherit this class.
    ''' </summary>


    Public MustInherit Class BaseRequest
        Inherits System.Object

        Public Event ResultEvent(ByRef request As BaseRequest)

        Private Const MAX_RETRIES As Integer = 4
        Private tries As Integer
        Private m_cancelled As Boolean = False

        Protected m_RequestType As RequestType
        Protected m_methodClass As String
        Protected m_methodName As String

        Dim m_auto_delete As Boolean
        Dim m_responseHeaderCode As Integer
        Dim m_HttpResult As WebRequestResultCode
        Dim m_errorMessage As String
        Dim m_failureCode As API20.FailureCode

        Protected m_baseHost As Uri = Settings.BaseHost
        Protected m_basePath As String = Settings.BasePath

        Protected m_AuthData As Types.AuthData = Settings.AuthData
        Protected m_requiredParams As New List(Of String)(New String() {"api_key"})
        Protected m_languageCode As String = Settings.DefaultLangCode
        Private m_timeout As Integer = Settings.DefaultRequestTimeout

        Dim m_http As Net.HttpWebRequest
        Protected m_name As String

        Protected m_requestData As Object


        Protected m_cacheable As Boolean = True
        Protected m_timestamp As Integer

        Protected m_parameters As New SortedDictionary(Of String, String)
        Protected m_rawMode As Boolean = False
        Protected m_accessMode As API20.RequestAccessMode = RequestAccessMode.Read
        Protected m_requestMode As API20.RequestMode = RequestMode.Rest
        Protected m_rpc As New WebRequests.XMLRpc
        Property RequestMode() As API20.RequestMode
            Get
                Return m_requestMode
            End Get
            Set(ByVal value As API20.RequestMode)
                m_requestMode = value
            End Set
        End Property
        ReadOnly Property FailureCode() As FailureCode
            Get
                Return m_failureCode
            End Get
        End Property
        Property AuthData() As API20.Types.AuthData
            Get
                Return m_AuthData
            End Get
            Set(ByVal value As API20.Types.AuthData)
                m_AuthData = value
            End Set
        End Property
        Property AccessMode() As RequestAccessMode
            Get
                Return m_accessMode
            End Get
            Set(ByVal value As RequestAccessMode)
                m_accessMode = value
            End Set
        End Property
        Overridable ReadOnly Property RequiresAuth() As Boolean
            Get
                Return False
            End Get
        End Property

        Property RawMode() As Boolean
            Get
                Return m_rawMode
            End Get
            Set(ByVal value As Boolean)
                m_rawMode = value
            End Set
        End Property
        ReadOnly Property MethodClass() As String
            Get
                Return m_methodClass
            End Get
        End Property
        ReadOnly Property MethodName() As String
            Get
                Return m_methodName
            End Get
        End Property

        ReadOnly Property FullMethodName() As String
            Get
                Return m_methodClass & "." & m_methodName
            End Get
        End Property


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
        ''' Gets or sets the base path(default is "/2.0").
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
        Public Property BaseHost() As Uri
            Get
                Return m_baseHost
            End Get
            Set(ByVal value As Uri)
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
            Protected Set(ByVal value As String)
                m_name = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the type of the request.
        ''' </summary>
        ''' <value>The type.</value>
        Property Type() As RequestType
            Get
                Return m_RequestType
            End Get
            Protected Set(ByVal value As RequestType)
                m_RequestType = value
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
                Return m_HttpResult
            End Get
        End Property

        ''' <summary>
        ''' Gets a value indicating whether this request failed.
        ''' </summary>
        ''' <value><c>true</c> if failed; otherwise, <c>false</c>.</value>
        ReadOnly Property failed() As Boolean
            Get
                Return (m_HttpResult <> WebRequestResultCode.Request_Success)
            End Get
        End Property
        ''' <summary>
        ''' Gets a value indicating whether this request was successful.
        ''' </summary>
        ''' <value><c>true</c> if succeeded; otherwise, <c>false</c>.</value>
        ReadOnly Property succeeded() As Boolean
            Get
                If (m_HttpResult = WebRequestResultCode.Request_Success) Then
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
                If (m_HttpResult = WebRequestResultCode.Request_Aborted) Then
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
        ''' Initializes a new instance of the <see cref="BaseRequest" /> class.
        ''' </summary>
        Protected Sub New()
            If m_baseHost Is Nothing Then
                m_baseHost = New Uri(IIf(System.Environment.CommandLine.Contains("--debug"), _
                            "http://wsdev.audioscrobbler.com", _
                            "http://ws.audioscrobbler.com"))
                'm_cacheable = GetCacheableState(Type)
                m_timestamp = UnixTime.GetUnixTime()
            End If
            If RequiresAuth Then m_requiredParams.Add("api_sig")

        End Sub

        ''' <summary>
        ''' Initializes a new instance of the <see cref="BaseRequest" /> class.
        ''' </summary>
        ''' <param name="type">The type of the request.</param>
        ''' <param name="name">The name of the request.</param>
        Protected Sub New(ByVal type As RequestType, Optional ByVal name As String = "")
            Me.New()
            Me.m_RequestType = type
            SetRequestMethodByRequestType(m_RequestType)
            Me.m_name = IIf(String.IsNullOrEmpty(m_name), m_RequestType.ToString(), "") 'm_type.ToString() & "Request", name)
        End Sub

        Protected Sub New(ByVal authData As API20.Types.AuthData, ByVal type As RequestType, Optional ByVal name As String = "")
            Me.New(type, name)
            m_AuthData = authData
        End Sub
        'Public Sub New(ByVal data As String)
        '    Me.New()
        '    success(data)
        'End Sub

        ''' <summary>
        ''' Starts the request and gets the response.
        ''' </summary>
        Public Overridable Overloads Sub Start()
            'params "method","api_key","raw"
            PutDefaultsIntoParams()

            If Me.HasRequiredParams(m_requiredParams.ToArray) Then
                If m_requestMode = modEnums.RequestMode.Rest Then
                    Dim paramStr As String = GetParameterString()
                    Dim urlstr As String = m_baseHost.AbsoluteUri & If(BasePath.ToLower.StartsWith("/"), BasePath.Substring(1), m_basePath) & "?" & paramstr
                    If Uri.IsWellFormedUriString(urlstr, UriKind.Absolute) Then
                        Dim u As New Uri(urlstr)
                        Debug.Print("Started request with url " & urlstr)
                        If m_accessMode = RequestAccessMode.Read Then
                            Me.get(u)
                        Else
                            Post(paramStr)
                        End If
                    End If
                Else
                    m_parameters.Remove("method")
                    m_rpc.Method = Me.FullMethodName
                    m_rpc.addParameter(m_parameters)
                    Post(m_rpc.toString)
                    m_parameters.Add("method", FullMethodName)
                End If
            Else
                Throw New ArgumentException("A api call parameter is missing!")
            End If


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
            [get](New Uri(m_baseHost, m_basePath & path))
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
        Private Sub DoGet(ByVal url As System.Uri)
            Dim response As HttpWebResponse
            m_cancelled = False
            m_http = Net.HttpWebRequest.Create(url)
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
                m_HttpResult = WebRequestResultCode.Request_Success
                response = m_http.GetResponse()
            Catch w As WebException
                'MsgBox("Request " & Me.Name & " fehlgeschlagen: " & w.Message)
                If w.Status = WebExceptionStatus.RequestCanceled Then
                    m_cancelled = True
                    Exit Sub
                End If
                response = w.Response
                Me.setFailed(WebRequestResultCode.Request_BadResponseCode, w.Message)
                Exit Try
            End Try

            HeaderReceived(response)
            Dim s As New IO.StreamReader(response.GetResponseStream, System.Text.Encoding.UTF8)

            Dim res() As Byte = System.Text.Encoding.UTF8.GetBytes(s.ReadToEnd)
            m_requestData = System.Text.Encoding.UTF8.GetString(res)
            SuccessPrivate(m_requestData)

            RaiseEvent ResultEvent(Me)
        End Sub
        ''' <summary>
        ''' Here deriving classes can handle the async callback of the DoGet(True)-Method
        ''' </summary>
        ''' <param name="ar">The ar.</param>
        Overridable Sub DoGetAsyncCallBack(ByVal ar As IAsyncResult)

        End Sub

        Protected Sub Post(ByRef data As String, Optional ByVal doasync As Boolean = False)
            Debug.Print(Me.FullMethodName & " posted data " & data)
            Dim dra As New PostAsyncDelegate(AddressOf PostPrivate)
            Dim posturl As Uri = New Uri(m_baseHost, m_basePath)
            If doasync Then
                dra.BeginInvoke(posturl, data, New AsyncCallback(AddressOf DoRequestAsyncCallBack), New Object)
            Else
                PostPrivate(posturl, data)
            End If
        End Sub
        Private Delegate Sub PostAsyncDelegate(ByVal url As Uri, ByRef data As String)
        ''' <summary>
        ''' Does the request using the "POST"-method.
        ''' </summary>
        ''' <param name="url">The POST url.</param>
        ''' <param name="data">The data which should be posted</param>
        Private Sub PostPrivate(ByVal url As Uri, ByRef data As String)

#If REQUEST_DEBUG_OUTPUT = 1 Then
            Debug.Print("Request " & Me.Name & " posted the following data to /2.0: ")
            Debug.Indent()
            Debug.Print(data)
            Debug.Unindent()
#End If
            ' Dim requestBytes() As Byte = System.Text.Encoding.UTF8.GetBytes(xml)
            ' Create a request using a URL that can receive a post. 
            System.Net.ServicePointManager.Expect100Continue = False
            Dim _http As HttpWebRequest = WebRequest.Create(url)
            System.Net.ServicePointManager.Expect100Continue = False
            _http.ServicePoint.Expect100Continue = False
            _http.Timeout = m_timeout
            '_http.Connection = "Close"
            _http.ProtocolVersion = New Version(1, 0)
            ' Set the Method property of the request to POST.
            _http.Method = "POST"
            If m_requestMode = modEnums.RequestMode.XmlRpc Then
                _http.ContentType = "text/xml"
            Else
                _http.ContentType = "application/x-www-form-urlencoded"
            End If
            ' Create POST data and convert it to a byte array.
            Dim byteArray As Byte() = System.Text.Encoding.UTF8.GetBytes(data)
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

                'ok it's not successful, but SuccessPrivate will be called later on for error handling ;)
                Me.setFailed(WebRequestResultCode.Request_BadResponseCode, w.Message)
                'therefor not 'exit sub'
                response = w.Response
            End Try
            HeaderReceived(response)
            Dim s As New IO.StreamReader(response.GetResponseStream, System.Text.Encoding.UTF8)
            'HIER ENCODING NOCHMAL ÜBERPRÜFEN, ggf ascii statt utf-8
            Dim res() As Byte = System.Text.Encoding.UTF8.GetBytes(s.ReadToEnd)
            m_requestData = System.Text.Encoding.UTF8.GetString(res)
            m_HttpResult = WebRequestResultCode.Request_Success
            SuccessPrivate(m_requestData)
            RaiseEvent ResultEvent(Me)

        End Sub

        ''' <summary>
        ''' Here deriving classes can handle the async callback of the DoRequest(True)-Method
        ''' </summary>
        ''' <param name="ar">The ar.</param>
        Overridable Sub DoRequestAsyncCallBack(ByVal ar As IAsyncResult)

        End Sub

        Sub SuccessPrivate(ByVal s As String)


            If m_requestMode = modEnums.RequestMode.XmlRpc Then
                Dim o As New List(Of Object)
                Dim err As String = ""
                WebRequests.XMLRpc.ParseResponse(s, o, err)
                If o.Count > 0 Then
                    If err <> "" Then
                        Dim d As Dictionary(Of String, Object) = CType(o(0), Dictionary(Of String, Object))
                        m_failureCode = CInt(d("faultCode"))
                        m_errorMessage = d("faultString")
                        Exit Sub
                    Else
                        If TypeOf (o(0)) Is Dictionary(Of String, Object) Then
                            Dim d As Dictionary(Of String, Object) = CType(o(0), Dictionary(Of String, Object))
                            s = CStr(d(0))
                        ElseIf o(0).GetType.Equals(GetType(String)) Then
                            s = CStr(o(0))
                        End If
                        s = s.Replace("\""", """")
                    End If
                End If
            End If
            Dim xdoc As New XmlDocument()
            Try
                xdoc.LoadXml(s)
            Catch ex As Exception
                setFailed(FailureCode.InvalidDataReturned, "Xml parse error. s was " & s)
                Exit Sub
            End Try
            Dim root As XmlElement = xdoc.DocumentElement

            If Not RawMode Then
                If root.HasAttribute("status") AndAlso root.GetAttribute("status") = "ok" Then
                    If root.IsEmpty Then Exit Sub
                    Success(root.FirstChild)
                Else
                    Dim errNode As XmlElement = root.SelectSingleNode("error")
                    If errNode.HasAttribute("code") Then
                        m_failureCode = CInt(errNode.GetAttribute("code"))
                        m_errorMessage = errNode.InnerText

                    End If

                End If
            Else
                If root.IsEmpty Then Exit Sub
                Success(root)
            End If


        End Sub
        Protected MustOverride Sub Success(ByVal elem As XmlElement)
        ''' <summary>
        ''' Sets the reason why the request failed
        ''' </summary>
        ''' <param name="failureCode">FailureCode</param>
        ''' <param name="message">The message.</param>
        Protected Sub setFailed(ByVal failureCode As API20.FailureCode, Optional ByVal message As String = "")
            m_HttpResult = Type
            If Not (String.IsNullOrEmpty(message)) Then
                m_errorMessage = message
                Debug.Print("Request " & Me.m_name & " failed: " & m_errorMessage)
            End If
        End Sub

        '''' <summary>
        '''' Gets the value of a Key-Value-Pair(e.g Abc=bbbb)-string if name can be found in the data string
        '''' </summary>
        '''' <param name="keyname">The keyname.</param>
        '''' <param name="data">The data string.</param>
        '''' <returns></returns>
        'Protected Function parameter(ByVal keyname As String, ByVal data As String) As String
        '    Dim list() As String = data.Split(Chr(10))

        '    For Each s As String In list
        '        If Not String.IsNullOrEmpty(s) Then
        '            If s.Contains("=") Then
        '                If keyname.ToLower = s.Substring(0, s.IndexOf("=")).ToLower Then
        '                    Return s.Substring(s.IndexOf("=") + 1)
        '                End If
        '            End If
        '        End If
        '    Next
        '    Return Nothing
        'End Function

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
            If String.IsNullOrEmpty(data) Then Return Nothing
            'Double encode because of last.fm's "&"-bug
            'result = Uri.EscapeDataString(Uri.EscapeDataString(data))
            result = Uri.EscapeDataString(data)
            'Debug.Print("Escaped " & data & " to " & result)
            Return result
        End Function
        ''' <summary>
        ''' Converts a URI-Escaped string into human-readable data.
        ''' </summary>
        ''' <param name="data">The data.</param>
        ''' <returns></returns>
        Shared Function UnEscapeUriData(ByVal data As String) As String
            'Return Uri.UnescapeDataString(Uri.UnescapeDataString(data))
            Return Uri.UnescapeDataString(data)
        End Function


        Protected Sub SetRequestMethodByRequestType(ByVal rt As RequestType)
            Dim typestr As String = System.Enum.GetName(GetType(RequestType), rt)
            Dim classes As String() = New String() {"album", "artist", "auth", "event", "geo", _
                                                    "group", "playlist", "tag", "tasteometer", "user", "track"}
            With typestr.ToLower
                For Each c As String In classes
                    If .StartsWith(c) Then
                        m_methodClass = c
                        Dim matchindex As Integer = .IndexOf(c)
                        Dim methodN As String = typestr.Substring(matchindex + c.Length)
                        methodN = methodN.Substring(0, 1).ToLower & methodN.Substring(1)
                        m_methodName = methodN
                        Exit Sub
                    End If
                Next
            End With
        End Sub
        Sub PutDefaultsIntoParams()
            With m_parameters
                SetAddDictValue(m_parameters, "method", Me.FullMethodName)
                SetAddDictValue(m_parameters, "api_key", m_AuthData.ApiKey.ToString)
                If m_rawMode Then SetAddDictValue(m_parameters, "raw", "true")

                If Me.RequiresAuth Then
                    If Not m_RequestType = RequestType.AuthGetSession Then
                        SetAddDictValue(m_parameters, "sk", m_AuthData.Session.SessionKey.ToString)
                    End If

                End If
                Dim sig As MD5Hash = GetMethodSignatureHash()
                SetAddDictValue(m_parameters, "api_sig", sig.ToString)


            End With
        End Sub
        Protected Function GetParameterString() As String
            Dim str As String = ""
            For Each kv As KeyValuePair(Of String, String) In m_parameters
                str = str & EscapeUriData(kv.Key) & "=" & EscapeUriData(kv.Value) & "&"
            Next
            'Strip of the last &
            If str.EndsWith("&") Then str = str.Substring(0, str.Length - 1)
            Return str
        End Function

        Private Sub SetAddDictValue(ByRef d As SortedDictionary(Of String, String), ByVal keyname As String, ByVal value As String)
            If d Is Nothing Then d = New SortedDictionary(Of String, String)
            If d.ContainsKey(keyname) Then
                d.Item(keyname) = value
            Else
                m_parameters.Add(keyname, value)
            End If
        End Sub
        Protected Sub SetAddParamValue(ByVal key As String, ByVal value As String)
            SetAddDictValue(m_parameters, key, value)
        End Sub

        Protected Function HasRequiredParams(ByRef paramNames As String()) As Boolean
            For Each s As String In paramNames
                If m_requestMode = modEnums.RequestMode.XmlRpc And s = "method" Then Continue For
                If Not m_parameters.ContainsKey(s.ToLower) Then Return False
            Next
            Return True
        End Function
        ''' <summary>
        ''' Gets the method signature hash required for authentication.
        ''' </summary>
        ''' <param name="autoRefreshToken">Determines if the token should be refreshed automtatically if it was invalidated.</param>
        ''' <returns>An <see cref="MD5Hash">MD5-Hash</see> as method signature. If the token refresh fails Nothing is returned.</returns>
        ''' <remarks></remarks>
        Protected Function GetMethodSignatureHash(Optional ByVal autoRefreshToken As Boolean = False) As MD5Hash

            Dim s As String = ""
            For Each n As Collections.Generic.KeyValuePair(Of String, String) In m_parameters
                If n.Key = "api_sig" Then Continue For
                s = s & LastFmLib.ConvertToUTF8(n.Key) & ConvertToUTF8(n.Value)
            Next
            's = s & ConvertToUTF8("method") & ConvertToUTF8(m_parameters("method"))

            s = s & ConvertToUTF8(m_AuthData.ApiSecret.ToString)

            Return New MD5Hash(s, False)
        End Function

    End Class
End Namespace
