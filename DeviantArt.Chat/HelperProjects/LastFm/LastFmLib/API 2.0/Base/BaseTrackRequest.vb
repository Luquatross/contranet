Imports LastFmLib.API20.Types
Namespace API20.Base
    Public MustInherit Class BaseTrackRequest
        Inherits BaseRequest
        Protected m_track As Types.Track


        Public Property Track() As Track
            Get
                Return m_track
            End Get
            Set(ByVal value As Track)
                m_track = value
            End Set
        End Property

        Sub New(ByVal type As RequestType, ByVal track As track)
            MyBase.New(type)
            m_track = track
        End Sub

        Public Overrides Sub Start()
            SetAddParamValue("artist", m_track.ArtistName)
            SetAddParamValue("track", m_track.Title)
            MyBase.Start()
        End Sub

    End Class
End Namespace
