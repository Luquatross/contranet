Imports System.Xml
Imports LastFmLib.API20.Types
Namespace API20.Playlists
    ''' <summary>
    ''' Fetch XSPF playlists using a lastfm playlist url. 
    ''' </summary>
    Public Class PlaylistFetch
        Inherits Base.BaseRequest

        Private m_url As Uri

        Private m_playlist As Types.Playlist

        Private m_fod As Boolean = False
        Private m_alpha As Boolean = False


        Private m_desktop As Boolean = False
        Private m_streaming As Boolean = False

        Private m_plTimestamp As UnixTime
        Public Property PlaylistTimestamp() As UnixTime
            Get
                Return m_plTimestamp
            End Get
            Set(ByVal value As UnixTime)
                m_plTimestamp = value
            End Set
        End Property

        Public Property Desktop() As Boolean
            Get
                Return m_desktop
            End Get
            Set(ByVal value As Boolean)
                m_desktop = value
            End Set
        End Property

        Public Overrides ReadOnly Property RequiresAuth() As Boolean
            Get
                Return True
            End Get
        End Property

        Property Alpha() As Boolean
            Get
                Return m_alpha
            End Get
            Set(ByVal value As Boolean)
                m_alpha = value
            End Set
        End Property
        Public Property Fod() As Boolean
            Get
                Return m_fod
            End Get
            Set(ByVal value As Boolean)
                m_fod = value
            End Set
        End Property

        Public ReadOnly Property Result() As Types.Playlist
            Get
                Return m_playlist
            End Get

        End Property

        Public Property Url() As Uri
            Get
                Return m_url
            End Get
            Set(ByVal value As Uri)
                m_url = value
            End Set
        End Property


        Private Sub New()
            MyBase.New(RequestType.PlaylistFetch)
        End Sub

        Sub New(ByVal pUrl As Uri)
            Me.New()
            m_url = pUrl
        End Sub

        Public Overloads Overrides Sub Start()
            If m_fod Then SetAddParamValue("fod", m_fod.ToString.ToLower)
            If m_alpha Then SetAddParamValue("alpha", m_alpha.ToString.ToLower)
            If m_streaming Then SetAddParamValue("streaming", m_alpha.ToString.ToLower)
            If m_desktop Then SetAddParamValue("desktop", m_desktop.ToString.ToLower)
            SetAddParamValue("playlistURL", m_url.AbsoluteUri)
            'If m_desktop Then SetAddParamValue("desktop", m_alpha.ToString.ToLower)
            MyBase.Start()
        End Sub

        Protected Overrides Sub Success(ByVal elem As System.Xml.XmlElement)
            m_playlist = New Playlist(elem)
        End Sub
    End Class
End Namespace
