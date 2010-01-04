Imports System.Xml
Imports LastFmLib.API20.Types
Namespace API20.Artist
    ''' <summary>
    ''' Get the top tracks by an artist on Last.fm, ordered by popularity 
    ''' </summary>
    Public Class ArtistGetTopTracks
        Inherits Base.BaseArtistRequest

        Dim m_result As List(Of Types.Track)
        ReadOnly Property Result() As List(Of Types.Track)
            Get
                Return m_result
            End Get
        End Property



        Public Sub New(ByVal aName As String)
            MyBase.New(RequestType.ArtistGetTopTracks, aName)
        End Sub
        Public Overloads Overrides Sub Start()
            MyBase.Start()

        End Sub

        Protected Overrides Sub Success(ByVal elem As System.Xml.XmlElement)
            If elem.HasAttribute("artist") Then m_artist = elem.GetAttribute("artist")
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
