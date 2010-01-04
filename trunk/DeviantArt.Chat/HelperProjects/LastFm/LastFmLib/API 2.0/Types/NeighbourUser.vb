Namespace API20.User
    ''' <summary>
    ''' A user from user.GetNeighbours
    ''' </summary>
    Public Class NeighbourUser
        Inherits Types.UserInfo

        Private m_Match As Single
        Public Property Match() As Single
            Get
                Return m_Match
            End Get
            Set(ByVal value As Single)
                m_Match = value
            End Set
        End Property

        Sub New(ByVal name As String, ByVal Url As Uri)
            MyBase.New(name, Url)
        End Sub
        Sub New(ByVal e As Xml.XmlElement)
            MyBase.New(e)
            m_Match = CSng(Util.GetSubElementValue(e, "match"))
        End Sub
    End Class
End Namespace
