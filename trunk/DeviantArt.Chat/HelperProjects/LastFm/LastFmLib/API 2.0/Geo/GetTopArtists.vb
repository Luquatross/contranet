Imports System.Xml
Imports LastFmLib.API20.Types
Namespace API20.Geo
    ''' <summary>
    ''' Get the most popular artists on Last.fm by country 
    ''' </summary>
    Public Class GetTopArtists
        Inherits Base.BaseRequest
        Dim m_country As String
        Dim m_artists As List(Of ArtistInfo)
        ReadOnly Property Result() As List(Of ArtistInfo)
            Get
                Return m_artists
            End Get
        End Property
        Property Country() As String
            Get
                Return m_country
            End Get
            Set(ByVal value As String)
                m_country = value
            End Set
        End Property

        Sub New(ByVal country As String)
            MyBase.New(RequestType.GeoGetTopArtists)
            m_country = country
        End Sub

        Public Overloads Overrides Sub Start()
            SetAddParamValue("country", m_country)
            MyBase.Start()
        End Sub

        Protected Overrides Sub Success(ByVal elem As System.Xml.XmlElement)
            If elem.HasAttribute("country") Then m_country = elem.GetAttribute("country")
            m_artists = New List(Of ArtistInfo)
            Dim list As XmlNodeList = elem.SelectNodes("artist")
            For Each a As XmlElement In list
                Dim artist As ArtistInfo = ArtistInfo.FromXmlElement(a)
                m_artists.Add(artist)
            Next

        End Sub
    End Class
End Namespace
