Namespace WebRequests
    ''' <summary>
    ''' Requests the ID of a track
    ''' </summary>
    Public Class TrackToIDRequest
        Inherits RequestBase

        Private m_id As Integer
        Private m_isStreamable As Boolean
        Private m_track As TypeClasses.Track

        ''' <summary>
        ''' Gets or sets the track.
        ''' </summary>
        ''' <value>The track.</value>
        Property Track() As TypeClasses.Track
            Get
                Return m_track
            End Get
            Set(ByVal value As TypeClasses.Track)
                m_track = value
            End Set
        End Property
        ''' <summary>
        ''' Gets a value indicating whether the track is streamable.
        ''' </summary>
        ''' <value>
        ''' <c>true</c> if this instance is streamable; otherwise, <c>false</c>.
        ''' </value>
        ReadOnly Property isStreamable() As Boolean
            Get
                Return m_isStreamable
            End Get
        End Property
        ''' <summary>
        ''' Gets the track id.
        ''' </summary>
        ''' <value>The id.</value>
        ReadOnly Property TrackId() As Integer
            Get
                Return m_id
            End Get
        End Property

        ''' <summary>
        ''' Initializes a new instance of the <see cref="TrackToIDRequest" /> class.
        ''' </summary>
        ''' <param name="track">The track.</param>
        Public Sub New(ByVal track As TypeClasses.Track)
            MyBase.New(RequestType.TrackToId, "trackToId")
            m_track = track
        End Sub
        Public Overrides Sub Start()
            Dim rpc As New XMLRPC()
            rpc.addParameter(m_track.ArtistName)
            rpc.addParameter(m_track.Title)

            rpc.Method = "trackToId"
            Request(rpc)
        End Sub

        Protected Overrides Sub success(ByVal data As String)
            Dim values As New List(Of Object)
            Dim err As String = ""

            If Not XMLRPC.ParseResponse(data, values, err) Then
                setFailed(WebRequestResultCode.WebRequestResult_Custom, err)
                Exit Sub
            End If

            Dim map As Dictionary(Of String, Object) = values(0)
            m_id = CInt(map("trackID"))
            m_isStreamable = CBool(map("isLastfm"))
        End Sub
    End Class
End Namespace