Imports LastFmLib.API20.Types
Imports System.Xml
Namespace API20.Groups
    ''' <summary>
    ''' Get a track chart for a group, for a given date range. If no date range is supplied, it will return the most recent album chart for this group. 
    ''' </summary>
    Public Class GroupGetWeeklyTrackChart
        Inherits Base.BaseGroupWeeklyChart
       
        Private m_Tracks As List(Of Types.Track)
        Public ReadOnly Property Result() As List(Of Types.Track)
            Get
                Return m_Tracks
            End Get
        End Property

        Sub New(ByVal group As String, Optional ByVal ci As ChartInfo = Nothing)
            MyBase.New(RequestType.GroupGetWeeklyTrackChart, group, ci)

        End Sub
        Public Overrides Sub Start()
            MyBase.Start()
        End Sub
        Protected Overrides Sub Success(ByVal elem As System.Xml.XmlElement)
            If elem.HasAttribute("group") Then Group = elem.GetAttribute("group")
            m_ChartInfo = ChartInfo.FromXmlElement(elem)
            m_Tracks = New List(Of Types.Track)
            For Each a As XmlElement In elem.SelectNodes("track")
                m_Tracks.Add(Types.Track.FromXmlElement(a))
            Next
        End Sub
    End Class
End Namespace
