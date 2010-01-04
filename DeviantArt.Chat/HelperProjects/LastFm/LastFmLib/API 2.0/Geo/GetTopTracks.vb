Imports System.Xml
Imports LastFmLib.API20.Types
Namespace API20.Geo
    ''' <summary>
    ''' Get the most popular tracks on Last.fm by country 
    ''' </summary>
    Public Class GetToptracks
        Inherits Base.BaseRequest
        Dim m_country As String
        Dim m_tracks As List(Of Types.Track)
        ReadOnly Property Result() As List(Of Types.Track)
            Get
                Return m_tracks
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
            MyBase.New(RequestType.GeoGetTopTracks)
            m_country = country
        End Sub

        Public Overloads Overrides Sub Start()
            SetAddParamValue("country", m_country)
            MyBase.Start()
        End Sub

        Protected Overrides Sub Success(ByVal elem As System.Xml.XmlElement)
            If elem.HasAttribute("country") Then m_country = elem.GetAttribute("country")
            m_tracks = New List(Of Types.Track)
            Dim list As XmlNodeList = elem.SelectNodes("track")
            For Each a As XmlElement In list
                Dim track As Types.Track = track.FromXmlElement(a)
                m_tracks.Add(track)
            Next

        End Sub
    End Class
End Namespace
