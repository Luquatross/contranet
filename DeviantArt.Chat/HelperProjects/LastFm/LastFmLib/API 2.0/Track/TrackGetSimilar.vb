Imports System.Xml
Imports LastFmLib.API20.Types
Namespace API20.Tracks
    Public Class TrackGetSimilar
        Inherits Base.BaseTrackRequest


        Private m_tracks As List(Of Track)
        Public ReadOnly Property Result() As List(Of Track)
            Get
                Return m_tracks
            End Get
        End Property


        Sub New(ByVal track As Track)
            MyBase.New(RequestType.TrackGetSimilar, track)
        End Sub
        Sub New(ByVal mbid As Guid)
            Me.New(New Track(mbid))
        End Sub
        Sub New(ByVal artist As String, ByVal title As String)
            Me.New(New Track(artist, title))
        End Sub

        Protected Overrides Sub Success(ByVal elem As System.Xml.XmlElement)
            m_tracks = New List(Of Track)
            Dim artist As String = Util.GetAttrValue(elem, "artist")
            Dim track As String = Util.GetAttrValue(elem, "track")
            If Not String.IsNullOrEmpty(artist) And Not String.IsNullOrEmpty(track) Then
                m_track = New Track(artist, track)
            End If
            For Each st As XmlElement In elem.SelectNodes("track")
                m_tracks.Add(Types.Track.FromXmlElement(st))
            Next
        End Sub
    End Class
End Namespace

