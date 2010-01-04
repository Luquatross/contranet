Imports LastFmLib.API20.Types
Imports System.Xml
Namespace API20.User
    ''' <summary>
    ''' Get an artist chart for a user profile, for a given date range.
    ''' If no date range is supplied, it will return the most recent artist chart for this user. 
    ''' </summary>
    Public Class UserGetWeeklyArtistChart
        Inherits Base.BaseUserWeeklyChart




        Private m_artists As List(Of ArtistInfo)
        Public ReadOnly Property Result() As List(Of ArtistInfo)
            Get
                Return m_artists
            End Get
        End Property



        Sub New(ByVal sUser As String, Optional ByVal cInfo As ChartInfo = Nothing)
            MyBase.New(RequestType.UserGetWeeklyArtistChart, sUser, cInfo)
        End Sub
        Public Overrides Sub Start()
            MyBase.Start()
        End Sub
        Protected Overrides Sub Success(ByVal elem As System.Xml.XmlElement)
            If elem.HasAttribute("user") Then m_User = elem.GetAttribute("user")
            m_ChartInfo = ChartInfo.FromXmlElement(elem)
            m_artists = New List(Of ArtistInfo)
            For Each a As XmlElement In elem.SelectNodes("artist")
                m_artists.Add(ArtistInfo.FromXmlElement(a))
            Next
        End Sub
    End Class
End Namespace
