Imports System.Xml
Imports LastFmLib.API20.Types
Namespace API20.Tracks


    ''' <summary>
    ''' Get the top fans for this track on Last.fm, based on listening data.
    ''' Supply either track title &amp; artist or mbid.
    ''' </summary>
    Public Class TrackGetTopFans
        Inherits Base.BaseRequest

        Dim m_topFans As List(Of TopFan)

        Private m_Track As Track
        Public Property Track() As Track
            Get
                Return m_Track
            End Get
            Set(ByVal value As Track)
                m_Track = value
            End Set
        End Property

        ReadOnly Property Result() As List(Of TopFan)
            Get
                Return m_topFans
            End Get
        End Property
        Sub New(ByVal track As Track)
            MyBase.New(RequestType.TrackGetTopFans)
            m_Track = track
        End Sub
        Sub New(ByVal artist As String, ByVal title As String)
            Me.New(New Types.Track(artist, title))
        End Sub
        Public Overrides Sub Start()
            With m_Track
                If Not String.IsNullOrEmpty(.ArtistName) Then SetAddParamValue("artist", .ArtistName)
                If Not String.IsNullOrEmpty(.ArtistName) Then SetAddParamValue("track", .Title)
                If .Mbid <> Guid.Empty Then SetAddParamValue("mbid", .Mbid.ToString)
            End With
            MyBase.Start()
        End Sub
        Protected Overrides Sub Success(ByVal elem As System.Xml.XmlElement)
            Dim list As XmlNodeList = elem.SelectNodes("user")
            m_topFans = New List(Of TopFan)
            For Each u As XmlElement In list
                Dim fan As TopFan = TopFan.FromXmlElemnt(u)
                m_topFans.Add(fan)
            Next
        End Sub

        Public Overrides Function ToString() As String
            Dim result As String = "Top fans for: " & m_Track.ArtistName & " - " & m_Track.Title
            For Each f As TopFan In m_topFans
                result &= f.Name & ", "
            Next
            result.Remove(result.IndexOf(","), 2)
            Return result
        End Function
    End Class
End Namespace