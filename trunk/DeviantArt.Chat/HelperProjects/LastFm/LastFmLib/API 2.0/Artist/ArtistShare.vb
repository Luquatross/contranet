Namespace API20.Artist
    ''' <summary>
    ''' Share an artist with Last.fm users or other friends. 
    ''' </summary>
    Public Class ArtistShare
        Inherits Base.BaseShare
        Public Overrides ReadOnly Property RequiresAuth() As Boolean
            Get
                Return True
            End Get
        End Property
        Private m_artist As String
        Public Property Artist() As String
            Get
                Return m_artist
            End Get
            Set(ByVal value As String)
                m_artist = value
            End Set
        End Property

        Sub New(ByVal artist As String, Optional ByVal recipients As List(Of String) = Nothing, Optional ByVal msg As String = "")
            MyBase.New(RequestType.ArtistShare, recipients, msg)
            m_artist = artist
        End Sub
        Public Overrides Sub Start()
            SetAddParamValue("artist", m_artist)
            MyBase.Start()
        End Sub
    End Class
End Namespace
