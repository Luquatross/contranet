Namespace API20.Tracks
    ''' <summary>
    ''' Search for a track by track name. Returns track matches sorted by relevance. 
    ''' </summary>
    Public Class TrackSearch
        Inherits Base.BaseTrackRequest
        Dim m_result As Types.TrackSearchResult

        Private m_limit As Integer
        Public Property Limit() As Integer
            Get
                Return m_limit
            End Get
            Set(ByVal value As Integer)
                m_limit = value
            End Set
        End Property

        ReadOnly Property Result() As Types.TrackSearchResult
            Get
                Return m_result
            End Get
        End Property

        Sub New(ByVal title As String, Optional ByVal artist As String = "", Optional ByVal limit As Integer = 0)
            Me.New(New Types.Track(title, artist), limit)
        End Sub
        Sub New(ByVal track As Types.Track, Optional ByVal limit As Integer = 0)
            MyBase.New(RequestType.TrackSearch, track)
            m_limit = limit
        End Sub
        Public Overrides Sub Start()
            If m_limit > 0 Then SetAddParamValue("limit", CStr(m_limit))
            MyBase.Start()
        End Sub
        Protected Overrides Sub Success(ByVal elem As System.Xml.XmlElement)
            m_result = New Types.TrackSearchResult(elem)
        End Sub
    End Class
End Namespace
