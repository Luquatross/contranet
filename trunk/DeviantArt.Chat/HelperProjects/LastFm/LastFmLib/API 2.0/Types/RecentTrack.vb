Namespace API20.Types
    ''' <summary>
    ''' Information about a recent track
    ''' </summary>
    Public Class RecentTrack
        Inherits Track
        Private m_nowPlaying As Boolean
        Private m_artistMbId As Guid
        Private m_album As String
        Private m_albumMbId As Guid
        ReadOnly Property AlbumMbId() As Guid
            Get
                Return m_albumMbId
            End Get
        End Property
        Public ReadOnly Property Album() As String
            Get
                Return m_album
            End Get
            'Set(ByVal value As String)
            '    m_album = value
            'End Set
        End Property

        ReadOnly Property ArtistMbId() As Guid
            Get
                Return m_artistMbId
            End Get
            'Set(ByVal value As Guid)

            'End Set
        End Property

        Public ReadOnly Property NowPlaying() As Boolean
            Get
                Return m_nowPlaying
            End Get
            'Set(ByVal value As Boolean)
            '    m_nowPlaying = value
            'End Set
        End Property


        Sub New()
            MyBase.New()
        End Sub
        Sub New(ByVal e As Xml.XmlElement)
            MyBase.New(e)
            If e.HasAttribute("nowplayling") Then m_nowPlaying = CBool(e.GetAttribute("nowplaying"))

            Dim aNode As Xml.XmlElement = e.SelectSingleNode("artist")
            If aNode IsNot Nothing Then
                Dim guidVal As String = Util.GetAttrValue(aNode, "mbid")
                If Not String.IsNullOrEmpty(guidVal) Then
                    m_artistMbId = New Guid(guidVal)
                End If
            End If
            aNode = e.SelectSingleNode("album")
            If aNode IsNot Nothing Then
                Dim guidVal As String = Util.GetAttrValue(aNode, "mbid")
                m_album = aNode.InnerText
                If Not String.IsNullOrEmpty(guidVal) Then
                    m_albumMbId = New Guid(guidVal)
                End If
            End If
        End Sub
        Shared Shadows Function FromXmlElement(ByVal e As Xml.XmlElement) As RecentTrack
            Return New RecentTrack(e)
        End Function
    End Class
End Namespace