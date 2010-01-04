Namespace API20.Tracks
    ''' <summary>
    ''' Remove a user's tag from a track. 
    ''' </summary>
    Public Class TrackRemoveTag
        Inherits Base.BaseTrackRequest

        Public Overrides ReadOnly Property RequiresAuth() As Boolean
            Get
                Return True
            End Get
        End Property
        Private m_tag As String
        Public Property Tag() As String
            Get
                Return m_tag
            End Get
            Set(ByVal value As String)
                m_tag = value
            End Set
        End Property

        Sub New(ByVal track As Types.Track, ByVal tag As String)
            MyBase.New(RequestType.ArtistRemoveTag, track)
            m_accessMode = RequestAccessMode.Write
            m_tag = Tag
        End Sub
        Sub New(ByVal artist As String, ByVal title As String, ByVal tag As String)
            Me.New(New Types.Track(artist, title), tag)
        End Sub
        Public Overrides Sub Start()
            SetAddParamValue("tag", m_tag)
            MyBase.Start()
        End Sub
        Protected Overrides Sub Success(ByVal elem As System.Xml.XmlElement)
            'Won't ever get here
        End Sub
    End Class
End Namespace
