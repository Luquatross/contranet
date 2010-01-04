Imports System.Xml
Imports LastFmLib.API20.Types
Namespace API20.Tag
    ''' <summary>
    ''' Fetches the top global tags on Last.fm, sorted by popularity (number of times used) 
    ''' </summary>
    Public Class TagGetTopTags
        Inherits Base.BaseRequest


        Dim m_result As Dictionary(Of String, Uri)
        ReadOnly Property Result() As Dictionary(Of String, Uri)
            Get
                Return m_result
            End Get
        End Property

        'No args, overall top tags ;)
        Public Sub New()
            MyBase.New(RequestType.TagGetTopTags)
        End Sub

        Public Overloads Overrides Sub Start()
            MyBase.Start()
        End Sub

        Protected Overrides Sub Success(ByVal elem As System.Xml.XmlElement)

            Dim tagNodes As XmlNodeList = elem.SelectNodes("tag")

            Dim tags As New Dictionary(Of String, Uri)
            For Each tagElem As XmlElement In tagNodes
                Dim name As String = Util.GetSubElementValue(tagElem, "name")
                Dim u As Uri = Nothing
                u = Util.GetUrl(Util.GetSubElementValue(tagElem, "url"))
                tags.Add(name, u)
            Next
            m_result = tags
        End Sub
    End Class
End Namespace
