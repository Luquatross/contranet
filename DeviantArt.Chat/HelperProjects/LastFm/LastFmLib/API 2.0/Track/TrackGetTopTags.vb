Imports System.Xml
Imports LastFmLib.API20.Types
Namespace API20.Tracks
    ''' <summary>
    ''' Get the top tags for this track on Last.fm, ordered by tag count. Supply either track &amp; artist name or mbid. 
    ''' </summary>
    Public Class TrackGetTopTags
        Inherits Base.BaseRequest

        Private m_Track As Track

        Private m_tags As List(Of TagInfo)
        Public ReadOnly Property Result() As List(Of TagInfo)
            Get
                Return m_tags
            End Get

        End Property

        Public Property Track() As Track
            Get
                Return m_Track
            End Get
            Set(ByVal value As Track)
                m_Track = value
            End Set
        End Property

        Sub New(ByVal track As Track)
            MyBase.New(RequestType.TrackGetTopTags)
            m_Track = track
        End Sub


        Public Overloads Overrides Sub Start()
            With m_Track
                If Not String.IsNullOrEmpty(.ArtistName) Then SetAddParamValue("artist", .ArtistName)
                If Not String.IsNullOrEmpty(.ArtistName) Then SetAddParamValue("track", .Title)
                If Not IsNothing(.Mbid) Then SetAddParamValue("mbid", .Mbid.ToString)
            End With
            MyBase.Start()
        End Sub

        Protected Overrides Sub Success(ByVal elem As System.Xml.XmlElement)

            Dim artist As String = Util.GetAttrValue(elem, "artist")
            Dim title As String = Util.GetAttrValue(elem, "track")
            If m_Track Is Nothing Then
                m_Track = New Track()
            End If
            m_Track.Artist.Name = artist
            m_Track.Title = title
            m_tags = New List(Of TagInfo)
            For Each t As XmlElement In elem.SelectNodes("tag")
                m_tags.Add(TagInfo.FromXmlElement(t))
            Next
        End Sub
    End Class
End Namespace
