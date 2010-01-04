Namespace API20.Artist
    ''' <summary>
    ''' Get the metadata for an artist on Last.fm. Includes biography. 
    ''' </summary>
    Public Class ArtistGetInfo
        Inherits Base.BaseArtistRequest


        Dim m_album As String
        Dim m_mbId As Guid
        Dim m_result As Types.ArtistInfo
        Property MbId() As Guid
            Get
                Return m_mbId
            End Get
            Set(ByVal value As Guid)
                m_mbId = value
            End Set
        End Property
        Property Album() As String
            Get
                Return m_album
            End Get
            Set(ByVal value As String)

            End Set
        End Property

        ReadOnly Property Result() As Types.ArtistInfo
            Get
                Return m_result
            End Get
        End Property
        Private Sub New(ByVal artist As String, ByVal album As String)
            Me.New(artist)
            m_album = album
        End Sub
        Sub New(ByVal artist As String)
            MyBase.New(RequestType.ArtistGetInfo, artist)
            m_artist = artist

        End Sub

        Sub New(ByVal mbid As Guid)
            'That's exceptionally as mbid replaces the artist name
            Me.New("")
            m_mbId = mbid
        End Sub

        Public Overrides Sub Start()
            If Not String.IsNullOrEmpty(Artist) Then SetAddParamValue("artist", m_artist)
            If Not IsNothing(m_mbId) Then SetAddParamValue("mbid", m_mbId.ToString)
            MyBase.Start()
        End Sub
        Protected Overrides Sub Success(ByVal elem As System.Xml.XmlElement)
            m_result = Types.ArtistInfo.FromXmlElement(elem)
        End Sub
    End Class
End Namespace