Namespace API20.Album
    ''' <summary>
    ''' Remove a user's tag from an album. 
    ''' </summary>
    ''' <remarks></remarks>
    Public Class AlbumRemoveTag
        Inherits Base.BaseArtistRequest
        Public Overrides ReadOnly Property RequiresAuth() As Boolean
            Get
                Return True
            End Get
        End Property
        Private m_tag As String
        Private m_album As String
        Public Property Album() As String
            Get
                Return m_album
            End Get
            Set(ByVal value As String)
                m_album = value
            End Set
        End Property

        Public Property Tag() As String
            Get
                Return m_tag
            End Get
            Set(ByVal value As String)
                m_tag = value
            End Set
        End Property


        Sub New(ByVal artist As String, ByVal album As String, ByVal tag As String)
            MyBase.New(RequestType.AlbumRemoveTag, artist)
            m_accessMode = RequestAccessMode.Write
            m_album = album
            m_tag = tag
        End Sub

        Public Overrides Sub Start()
            SetAddParamValue("album", m_album)
            SetAddParamValue("tag", m_tag)
            MyBase.Start()
        End Sub
        Protected Overrides Sub Success(ByVal elem As System.Xml.XmlElement)
            'Won't ever get here
        End Sub
    End Class
End Namespace
