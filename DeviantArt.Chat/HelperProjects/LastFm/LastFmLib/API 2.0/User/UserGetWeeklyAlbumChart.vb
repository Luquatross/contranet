Imports LastFmLib.API20.Types
Imports System.Xml
Namespace API20.User
    ''' <summary>
    ''' Get an album chart for a user profile, for a given date range.
    ''' If no date range is supplied, it will return the most recent album chart for this user. 
    ''' </summary>
    Public Class UserGetWeeklyalbumChart
        Inherits Base.BaseUserWeeklyChart

        Private m_albums As List(Of AlbumInfo)
        Public ReadOnly Property Result() As List(Of AlbumInfo)
            Get
                Return m_albums
            End Get
        End Property


        Sub New(ByVal username As String, Optional ByVal chartInfo As ChartInfo = Nothing)
            MyBase.New(RequestType.UserGetWeeklyAlbumChart, username, chartInfo)
            m_User = username
            m_ChartInfo = chartInfo
        End Sub
        Public Overrides Sub Start()
            MyBase.Start()
        End Sub
        Protected Overrides Sub Success(ByVal elem As System.Xml.XmlElement)
            If elem.HasAttribute("user") Then m_User = elem.GetAttribute("user")
            m_ChartInfo = ChartInfo.FromXmlElement(elem)
            m_albums = New List(Of AlbumInfo)
            For Each a As XmlElement In elem.SelectNodes("album")
                m_albums.Add(AlbumInfo.FromXmlElement(a))
            Next
        End Sub
    End Class
End Namespace
