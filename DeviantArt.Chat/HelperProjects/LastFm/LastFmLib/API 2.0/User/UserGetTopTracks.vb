Imports System.Xml
Imports LastFmLib.API20.Types
Namespace API20.User
    ''' <summary>
    ''' Get the top tracks listened to by a user. 
    ''' You can stipulate a time period. Sends the overall chart by default. 
    ''' </summary>
    Public Class UserGetTopTracks
        Inherits Base.BaseUserGetTopX


        Dim m_result As List(Of Types.Track)
        ReadOnly Property Result() As List(Of Types.Track)
            Get
                Return m_result
            End Get
        End Property



        Public Sub New(ByVal uName As String, Optional ByVal period As ChartPeriod = ChartPeriod.Overall)
            MyBase.New(RequestType.UserGetTopTracks, uName, period)

        End Sub
        Public Overloads Overrides Sub Start()
            MyBase.Start()
        End Sub

        Protected Overrides Sub Success(ByVal elem As System.Xml.XmlElement)
            If elem.HasAttribute("user") Then m_User = elem.GetAttribute("user")
            Dim trackNodes As XmlNodeList = elem.SelectNodes("track")

            Dim tracks As New List(Of Types.Track)
            For Each trackElem As XmlElement In trackNodes
                Dim track As Types.Track = track.FromXmlElement(trackElem)
                tracks.Add(track)
            Next
            m_result = tracks
        End Sub
    End Class
End Namespace
