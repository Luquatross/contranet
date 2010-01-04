Namespace API20.Artist
    ''' <summary>
    ''' Remove a user's tag from an artist. 
    ''' </summary>
    Public Class ArtistRemoveTag
        Inherits Base.BaseArtistRequest

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


        Sub New(ByVal artist As String, ByVal tag As String)
            MyBase.New(RequestType.ArtistRemoveTag, artist)
            m_accessMode = RequestAccessMode.Write
            m_tag = tag
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
