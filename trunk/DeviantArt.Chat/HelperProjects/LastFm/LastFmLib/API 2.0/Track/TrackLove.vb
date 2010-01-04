Namespace API20.Tracks
    ''' <summary>
    ''' Love a track for a user profile. 
    ''' This needs to be supplemented with a scrobbling submission containing the 'love' rating (see the audioscrobbler API). 
    ''' </summary>
    Public Class TrackLove
        Inherits Base.BaseTrackRequest
        Public Overrides ReadOnly Property RequiresAuth() As Boolean
            Get
                Return True
            End Get
        End Property
        Sub New(ByVal track As Types.Track)
            MyBase.New(RequestType.TrackLove, track)
            m_accessMode = RequestAccessMode.Write
        End Sub
        Sub New(ByVal artist As String, ByVal title As String)
            Me.New(New Types.Track(artist, title))

        End Sub

        Protected Overrides Sub Success(ByVal elem As System.Xml.XmlElement)
            'Won't ever get here
        End Sub
    End Class
End Namespace
