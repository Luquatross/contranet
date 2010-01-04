Namespace API20.Artist
    ''' <summary>
    ''' Search for an artist by name. Returns artist matches sorted by relevance. 
    ''' </summary>
    Public Class ArtistSearch
        Inherits Base.BaseRequest
        Dim m_result As Types.ArtistSearchResult
        Dim m_queryString As String

        Property QueryString() As String
            Get
                Return m_queryString
            End Get
            Set(ByVal value As String)
                m_queryString = value
            End Set
        End Property
        ReadOnly Property Result() As Types.ArtistSearchResult
            Get
                Return m_result
            End Get
        End Property
        Private Sub New()
            MyBase.New(RequestType.ArtistSearch)
            m_requiredParams.Add("artist")
        End Sub
        Sub New(ByVal queryString As String)
            Me.New()
            m_queryString = queryString
        End Sub
        Public Overrides Sub Start()
            SetAddParamValue("artist", m_queryString)
            MyBase.Start()
        End Sub
        Protected Overrides Sub Success(ByVal elem As System.Xml.XmlElement)
            m_result = Types.ArtistSearchResult.FromXmlElement(elem)
        End Sub
    End Class
End Namespace