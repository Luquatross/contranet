Imports System.Xml
Imports LastFmLib.API20.Types
Namespace API20.Geo
    ''' <summary>
    ''' Get all events in a specific location by country or city name. 
    ''' </summary>
    Public Class GeoGetEvents
        Inherits Base.BaseRequest

        Dim m_location As String
        Dim m_distance As Integer = 0
        Dim m_page As Integer? = Nothing
        Dim m_totalPages As Integer
        Dim m_totalResults As Integer
        Dim m_events As List(Of MusicEvent)

        ReadOnly Property Result() As List(Of MusicEvent)
            Get
                Return m_events
            End Get
        End Property

        ReadOnly Property TotalResults() As Integer
            Get
                Return m_totalResults
            End Get
        End Property
        ReadOnly Property TotalPages() As Integer
            Get
                Return m_totalPages
            End Get

        End Property
        Property Page() As Integer
            Get
                Return m_page
            End Get
            Set(ByVal value As Integer)
                m_page = value
            End Set
        End Property
        Property Distance() As Integer
            Get
                Return m_distance
            End Get
            Set(ByVal value As Integer)
                m_distance = value
            End Set
        End Property
        Property Location() As String
            Get
                Return m_location
            End Get
            Set(ByVal value As String)
                m_location = value
            End Set
        End Property

        Sub New(ByVal location As String, Optional ByVal distance As Integer = 0)
            MyBase.New(RequestType.GeoGetEvents)
            m_location = location
            m_distance = distance
        End Sub

        Public Overloads Overrides Sub Start()
            If Not String.IsNullOrEmpty(m_location) Then
                SetAddParamValue("location", m_location)
            End If
            If m_distance > 0 Then
                SetAddParamValue("distance", m_distance)
            End If
            If m_page > 0 Then
                SetAddParamValue("page", m_page)
            End If
            MyBase.Start()
        End Sub

        Protected Overrides Sub Success(ByVal elem As System.Xml.XmlElement)
            If elem.HasAttribute("location") Then m_location = elem.GetAttribute("location")
            If elem.HasAttribute("page") Then m_page = CInt(elem.GetAttribute("page"))
            If elem.HasAttribute("totalpages") Then m_totalPages = elem.GetAttribute("totalpages")
            If elem.HasAttribute("total") Then m_totalResults = elem.GetAttribute("total")

            Dim eventNodes As XmlNodeList = elem.SelectNodes("event")
            m_events = New List(Of MusicEvent)
            For Each e As XmlNode In eventNodes
                Dim m As MusicEvent = MusicEvent.FromXmlElement(e)
                m_events.Add(m)
            Next
        End Sub
    End Class
End Namespace
