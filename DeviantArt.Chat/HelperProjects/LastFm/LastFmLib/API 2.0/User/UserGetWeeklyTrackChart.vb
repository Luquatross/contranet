Imports LastFmLib.API20.Types
Imports System.Xml
Namespace API20.User
    ''' <summary>
    ''' Get a track chart for a user profile, for a given date range.
    ''' If no date range is supplied, it will return the most recent track chart for this user. 
    ''' </summary>
    Public Class UserGetWeeklyTrackChart
        Inherits Base.BaseUserWeeklyChart


        Private m_Tracks As List(Of Types.Track)
        Public ReadOnly Property Result() As List(Of Types.Track)
            Get
                Return m_Tracks
            End Get
        End Property



        Sub New(ByVal User As String, Optional ByVal cInfo As ChartInfo = Nothing)
            MyBase.New(RequestType.UserGetWeeklyTrackChart, User, cInfo)
            m_User = User
        End Sub
        Public Overrides Sub Start()
            MyBase.Start()
        End Sub
        Protected Overrides Sub Success(ByVal elem As System.Xml.XmlElement)
            If elem.HasAttribute("user") Then m_User = elem.GetAttribute("user")
            m_ChartInfo = ChartInfo.FromXmlElement(elem)
            m_Tracks = New List(Of Types.Track)
            For Each a As XmlElement In elem.SelectNodes("track")
                m_Tracks.Add(Types.Track.FromXmlElement(a))
            Next
        End Sub
    End Class
End Namespace
