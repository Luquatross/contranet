Namespace API20.Tracks
    ''' <summary>
    ''' Ban a track for a given user profile. 
    ''' This needs to be supplemented with a scrobbling submission containing the 'ban' rating (see the audioscrobbler API). 
    ''' </summary>
    Public Class TrackBan
        Inherits Base.BaseTrackRequest

        Public Overrides ReadOnly Property RequiresAuth() As Boolean
            Get
                Return True
            End Get
        End Property
        Sub New(ByVal artist As String, ByVal title As String)
            Me.New(New Types.Track(artist, title))
        End Sub
        Sub New(ByVal track As Types.Track)
            MyBase.New(RequestType.TrackBan, track)
            m_accessMode = RequestAccessMode.Write
        End Sub
        Protected Overrides Sub Success(ByVal elem As System.Xml.XmlElement)
            'Won't ever get here
        End Sub
    End Class
End Namespace
