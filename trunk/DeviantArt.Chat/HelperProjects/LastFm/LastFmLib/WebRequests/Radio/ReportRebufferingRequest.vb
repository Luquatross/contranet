Namespace WebRequests
    ''' <summary>
    ''' Reports the rebuffering of the client -.-
    ''' </summary>
    Public Class ReportRebufferingRequest
        Inherits RequestBase
        Private m_streamerHost As String
        Private m_username As String
        ''' <summary>
        ''' Gets or sets the username.
        ''' </summary>
        ''' <value>The username.</value>
        Public Property Username() As String
            Get
                Return m_username
            End Get
            Set(ByVal Value As String)
                m_username = Value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the streamer host.
        ''' </summary>
        ''' <value>The streamer host.</value>
        Public Property StreamerHost() As String
            Get
                Return m_streamerHost
            End Get
            Set(ByVal Value As String)
                m_streamerHost = Value
            End Set
        End Property

        ''' <summary>
        ''' Initializes a new instance of the <see cref="ReportRebufferingRequest" /> class.
        ''' </summary>
        Sub New()
            MyBase.New(RequestType.ReportRebuffering, "ReportRebuffering")
        End Sub
        ''' <summary>
        ''' Initializes a new instance of the <see cref="ReportRebufferingRequest" /> class.
        ''' </summary>
        ''' <param name="username">The username.</param>
        ''' <param name="streamerHost_">The streamer host_.</param>
        Sub New(ByVal username As String, ByVal streamerHost_ As String)
            Me.New()
            m_username = username
            m_streamerHost = streamerHost_
        End Sub
        ''' <summary>
        ''' Starts the request and gets the response.
        ''' </summary>
        Public Overrides Sub Start()
            Me.BaseHost = "www.last.fm"
            Dim path As String = "/log/client/radio/buffer_underrun" & _
                                 "?userid=" & m_username & _
                                 "&hostname=" & StreamerHost

        End Sub
        Protected Overrides Sub success(ByVal data As String)
            '// Code will never get here as the service returns a 404.
            '// Which it should. Oddly enough.
        End Sub
    End Class
End Namespace