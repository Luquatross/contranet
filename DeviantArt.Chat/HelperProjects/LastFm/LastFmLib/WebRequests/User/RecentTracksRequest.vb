
Namespace WebRequests
    ''' <summary>
    ''' Request for getting the recently played tracks of a user
    ''' </summary>
    Public Class RecentTracksRequest
        Inherits RequestBase
        Protected m_tracks As List(Of TypeClasses.Track)
        Protected m_username As String
        Protected m_key As String

        ''' <summary>
        ''' Gets the track list.
        ''' </summary>
        ''' <value>The track list.</value>
        ReadOnly Property TrackList() As List(Of TypeClasses.Track)
            Get
                Return m_tracks
            End Get
        End Property
        Property Username() As String
            Get
                Return m_username
            End Get
            Set(ByVal value As String)
                m_username = value
            End Set
        End Property
        Protected Sub New(ByVal type As WebRequests.RequestType, ByVal key As String)
            MyBase.New(type, key)
            m_key = key
        End Sub

        Public Sub New(ByVal username As String)
            MyBase.New(WebRequests.RequestType.RecentTracks, "RecentTracksRequest")
            m_key = "recenttracks"
            m_username = username
        End Sub
        Public Overrides Sub Start()
            Me.get("/1.0/user/" & EscapeUriData(m_username) & "/" & m_key & ".xml")
        End Sub
        Protected Overrides Sub success(ByVal data As String)
            Dim document As New Xml.XmlDocument
            document.LoadXml(data)
            Dim values As Xml.XmlNodeList = document.SelectNodes("track")
            m_tracks = New List(Of TypeClasses.Track)
            For i As Integer = 0 To values.Count - 1
                Dim track As New TypeClasses.Track
                track.ArtistName = values(i).Item("artist").InnerText
                track.Title = values(i).Item("name").InnerText
                m_tracks.Add(track)
            Next
        End Sub
    End Class




    Class RecentlyBannedTracksRequest
        Inherits RecentTracksRequest
        Sub New(ByVal sUsername As String)
            MyBase.New(RequestType.RecentlyBannedTracks, "RecentlyBannedTracks")
            m_key = "recentbannedtracks"
            m_username = sUsername
        End Sub
    End Class




    Class RecentlyLovedTracksRequest
        Inherits RecentTracksRequest
        Sub New(ByVal sUsername As String)
            MyBase.New(RequestType.RecentlyLovedTracks, "RecentlyLovedTracks")
            m_key = "recentlovedtracks"
            m_username = sUsername
        End Sub
    End Class
End Namespace
