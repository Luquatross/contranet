Imports LastFmLib.API20.Types
Imports System.Xml
Namespace API20.Groups
    ''' <summary>
    ''' Get an album chart for a group, for a given date range. If no date range is supplied, it will return the most recent album chart for this group. 
    ''' </summary>
    Public Class GroupGetWeeklyAlbumChart
        Inherits Base.BaseGroupWeeklyChart

        Private m_albums As List(Of AlbumInfo)
        Public ReadOnly Property Result() As List(Of AlbumInfo)
            Get
                Return m_albums
            End Get
        End Property


        Sub New(ByVal group As String, Optional ByVal ci As ChartInfo = Nothing)
            MyBase.New(RequestType.GroupGetWeeklyAlbumChart, group, ci)
        End Sub

        Public Overrides Sub Start()
            MyBase.Start()
        End Sub
        Protected Overrides Sub Success(ByVal elem As System.Xml.XmlElement)
            If elem.HasAttribute("group") Then Group = elem.GetAttribute("group")
            m_ChartInfo = ChartInfo.FromXmlElement(elem)
            m_albums = New List(Of AlbumInfo)
            For Each a As XmlElement In elem.SelectNodes("album")
                m_albums.Add(AlbumInfo.FromXmlElement(a))
            Next
        End Sub
    End Class
End Namespace
