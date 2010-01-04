Imports System.Xml
Imports LastFmLib.API20.Types
Namespace API20.User
    ''' <summary>
    ''' Get a list of the recent tracks listened to by this user. Indicates now playing track if the user is currently listening. 
    ''' </summary>
    Public Class UserGetRecentTracks
        Inherits Base.BaseUserRequest

        Private m_limit As Integer

        Private m_tracks As List(Of RecentTrack)
        Public Property Result() As List(Of RecentTrack)
            Get
                Return m_tracks
            End Get
            Set(ByVal value As List(Of RecentTrack))
                m_tracks = value
            End Set
        End Property

        Public Property Limit() As Integer
            Get
                Return m_limit
            End Get
            Set(ByVal value As Integer)
                m_limit = value
            End Set
        End Property

        Sub New(ByVal uName As String, Optional ByVal iLimit As Integer = 0)
            MyBase.New(RequestType.UserGetRecentTracks, uName)
            m_limit = iLimit
        End Sub

        Public Overloads Overrides Sub Start()
            If m_limit > 0 Then
                SetAddParamValue("limit", CStr(m_limit))
            End If
            MyBase.Start()
        End Sub

        Protected Overrides Sub Success(ByVal elem As System.Xml.XmlElement)
            m_User = Util.GetAttrValue(elem, "user")
            m_tracks = New List(Of RecentTrack)
            For Each t As Xml.XmlElement In elem.SelectNodes("track")
                m_tracks.Add(New RecentTrack(t))
            Next

        End Sub
    End Class
End Namespace
