Namespace API20.Types
    ''' <summary>
    ''' User from friends request
    ''' </summary>
    Public Class FriendUser
        Inherits UserInfo
        Dim m_track As RecentTrack
        ReadOnly Property RecentTrack() As RecentTrack
            Get
                Return m_track
            End Get
            'Set(ByVal value As RecentTrack)

            'End Set
        End Property
        Public Overrides Function ToDebugString() As String
            Return MyBase.ToDebugString() & vbCrLf & If(m_track IsNot Nothing, m_track.ToDebugString, "") & vbCrLf
        End Function
        Sub New(ByVal uname As String, ByVal url As Uri)
            MyBase.New(uname, url)
        End Sub
        Sub New(ByVal e As Xml.XmlElement)
            MyBase.New(e)
            Dim x As Xml.XmlElement = e.SelectSingleNode("recenttrack")
            If x IsNot Nothing Then m_track = RecentTrack.FromXmlElement(x)
        End Sub

    End Class
End Namespace