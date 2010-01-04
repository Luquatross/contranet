Imports System.Xml
Namespace API20.Base
    Public MustInherit Class BaseUserRequest
        Inherits Base.BaseRequest

        Protected m_User As String
        Property User() As String
            Get
                Return m_User
            End Get
            Set(ByVal value As String)
                m_User = value
            End Set
        End Property

        Protected Sub New(ByVal type As RequestType, ByVal User As String)
            MyBase.New(type)
            m_requiredParams.Add("user")
            m_User = User
        End Sub
        Public Overrides Sub Start()
            SetAddParamValue("user", m_User)
            MyBase.Start()
        End Sub
        Protected MustOverride Overrides Sub Success(ByVal elem As System.Xml.XmlElement)

    End Class
End Namespace