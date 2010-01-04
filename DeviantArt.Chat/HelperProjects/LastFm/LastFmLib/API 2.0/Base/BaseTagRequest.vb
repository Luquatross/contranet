Imports System.Xml
Namespace API20.Base
    Public Class BaseTagRequest
        Inherits Base.BaseRequest

        Protected m_Tag As String
        Property Tag() As String
            Get
                Return m_Tag
            End Get
            Set(ByVal value As String)
                m_Tag = value
            End Set
        End Property

        Protected Sub New(ByVal type As RequestType, Optional ByVal Tag As String = "")
            MyBase.New(type)
            m_requiredParams.Add("tag")
            m_Tag = Tag
        End Sub
        Public Overrides Sub Start()
            SetAddParamValue("tag", m_Tag)
            MyBase.Start()
        End Sub
        Protected Overrides Sub Success(ByVal elem As System.Xml.XmlElement)

        End Sub
    End Class
End Namespace