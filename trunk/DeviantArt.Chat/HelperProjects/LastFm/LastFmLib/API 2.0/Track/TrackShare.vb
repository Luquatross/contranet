Namespace API20.Tracks
    ''' <summary>
    ''' Share a track twith one or more Last.fm users or other friends. 
    ''' </summary>
    Public Class TrackShare
        Inherits Base.BaseShare
        Public Overrides ReadOnly Property RequiresAuth() As Boolean
            Get
                Return True
            End Get
        End Property
        Private m_track As Types.Track
        Public Property Track() As Types.Track
            Get
                Return m_track
            End Get
            Set(ByVal value As Types.Track)
                m_track = value
            End Set
        End Property


        Sub New(ByVal artist As String, ByVal track As String, Optional ByVal recipients As List(Of String) = Nothing, Optional ByVal msg As String = "")
            Me.New(New Types.Track(artist, track), recipients, msg)
        End Sub
        Sub New(ByVal track As Types.Track, Optional ByVal recipients As List(Of String) = Nothing, Optional ByVal msg As String = "")
            MyBase.New(RequestType.TrackShare, recipients, msg)
            m_track = track
        End Sub
        Public Overrides Sub Start()
            SetAddParamValue("artist", m_track.ArtistName)
            SetAddParamValue("track", m_track.Title)
            MyBase.Start()
        End Sub
    End Class
End Namespace
