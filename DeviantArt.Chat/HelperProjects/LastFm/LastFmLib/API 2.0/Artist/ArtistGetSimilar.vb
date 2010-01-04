Namespace API20.Artist
    ''' <summary>
    ''' Get all the artists similar to this artist 
    ''' </summary>
    Public Class ArtistGetSimilar
        Inherits Base.BaseArtistRequest

        Dim m_limit As Integer
        Dim m_result As List(Of Types.ArtistInfo)

        Private m_Streamable As Boolean
        Public Property IsStreamable() As Boolean
            Get
                Return m_Streamable
            End Get
            Set(ByVal value As Boolean)
                m_Streamable = value
            End Set
        End Property

        Public ReadOnly Property Result() As List(Of Types.ArtistInfo)
            Get
                Return m_result
            End Get
        End Property
        Property Limit() As Integer
            Get
                Return m_limit
            End Get
            Set(ByVal value As Integer)
                m_limit = value
            End Set
        End Property



        Sub New(ByVal artist As String, Optional ByVal limit As Integer = 0)
            MyBase.New(RequestType.ArtistGetSimilar, artist)
            m_artist = artist
            m_limit = limit


        End Sub
        Public Overrides Sub Start()

            If Limit > 0 Then SetAddParamValue("limit", m_limit)
            MyBase.Start()
        End Sub
        Protected Overrides Sub Success(ByVal elem As System.Xml.XmlElement)
            'We will get a minimal artist, but the object will contain all his/her similar ones, too
            m_artist = Util.GetAttrValue(elem, "name")
            m_result = New List(Of Types.ArtistInfo)
            For Each a As Xml.XmlElement In elem.SelectNodes("artist")
                m_result.Add(Types.ArtistInfo.FromXmlElement(a))
            Next

        End Sub
    End Class
End Namespace