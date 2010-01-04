Imports System.Xml
Imports LastFmLib.API20.Types
Namespace API20.User
    ''' <summary>
    ''' Get a list of a user's neighbours on Last.fm. 
    ''' </summary>
    Public Class UserGetNeighbours
        Inherits Base.BaseUserRequest


        Private m_limit As Integer

        Private m_users As List(Of NeighbourUser)
        Public ReadOnly Property Result() As List(Of NeighbourUser)
            Get
                Return m_users
            End Get

        End Property

        Public Property Limit() As Integer
            Get
                Return m_limit
            End Get
            Set(ByVal value As Integer)
                m_limit = value
            End Set
        End Property


        Sub New(ByVal uname As String)
            MyBase.New(RequestType.UserGetNeighbours, uname)
        End Sub

        Public Overloads Overrides Sub Start()
            If m_limit > 0 Then SetAddParamValue("limit", CStr(m_limit))
            MyBase.Start()
        End Sub

        Protected Overrides Sub Success(ByVal elem As System.Xml.XmlElement)
            m_User = Util.GetAttrValue(elem, "from")
            m_users = New List(Of NeighbourUser)
            For Each t As XmlElement In elem.SelectNodes("user")
                m_users.Add(New NeighbourUser(t))
            Next
        End Sub
    End Class
End Namespace
