Imports LastFmLib.API20.Types
Imports System.Xml
Namespace API20.Groups
    ''' <summary>
    ''' Get an artist chart for a group, for a given date range. If no date range is supplied, it will return the most recent album chart for this group. 
    ''' </summary>
    Public Class GroupGetWeeklyArtistChart
        Inherits Base.BaseGroupWeeklyChart
      
        Private m_artists As List(Of ArtistInfo)
        Public ReadOnly Property Result() As List(Of ArtistInfo)
            Get
                Return m_artists
            End Get
        
        End Property

        Sub New(ByVal group As String, Optional ByVal ci As ChartInfo = Nothing)
            MyBase.New(RequestType.GroupGetWeeklyArtistChart, group, ci)

        End Sub
        Public Overrides Sub Start()
            MyBase.Start()
        End Sub
        Protected Overrides Sub Success(ByVal elem As System.Xml.XmlElement)
            If elem.HasAttribute("group") Then Group = elem.GetAttribute("group")
            m_ChartInfo = ChartInfo.FromXmlElement(elem)
            m_artists = New List(Of ArtistInfo)
            For Each a As XmlElement In elem.SelectNodes("artist")
                m_artists.Add(ArtistInfo.FromXmlElement(a))
            Next
        End Sub
    End Class
End Namespace
