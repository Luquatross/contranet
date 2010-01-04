Imports System.Xml
Imports LastFmLib.API20.Types
Namespace API20.User
    ''' <summary>
    ''' Get the top tags used by this user. 
    ''' </summary>
    Public Class UserGetTopTags
        Inherits Base.BaseUserGetTopX

        Dim m_result As List(Of TagInfo)
        ReadOnly Property Result() As List(Of TagInfo)
            Get
                Return m_result
            End Get
        End Property

        Private m_limit As Integer
        Public Property Limit() As Integer
            Get
                Return m_limit
            End Get
            Set(ByVal value As Integer)
                m_limit = value
            End Set
        End Property


        Public Sub New(ByVal uName As String, Optional ByVal limit As Integer = 0)
            MyBase.New(RequestType.UserGetTopTags, uName)
            m_limit = limit
        End Sub
        Public Overloads Overrides Sub Start()
            If m_limit > 0 Then SetAddParamValue("limit", m_limit)
            MyBase.Start()
        End Sub

        Protected Overrides Sub Success(ByVal elem As System.Xml.XmlElement)
            If elem.HasAttribute("user") Then m_User = elem.GetAttribute("user")
            Dim tagNodes As XmlNodeList = elem.SelectNodes("tag")

            Dim tags As New List(Of TagInfo)
            For Each tagElem As XmlElement In tagNodes
                Dim t As TagInfo = TagInfo.FromXmlElement(tagElem)
                tags.Add(t)
            Next
            m_result = tags
        End Sub
    End Class
End Namespace
