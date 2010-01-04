Imports System.Xml
Imports LastFmLib.API20.Types
Namespace API20.Artist

    ''' <summary>
    ''' Get the top tags for an artist on Last.fm, ordered by popularity. 
    ''' </summary>
    Public Class ArtistGetTopTags
        Inherits Base.BaseArtistRequest

        Dim m_result As Dictionary(Of String, Uri)
        ReadOnly Property Result() As Dictionary(Of String, Uri)
            Get
                Return m_result
            End Get
        End Property

        Public Sub New(ByVal aName As String)
            MyBase.New(RequestType.ArtistGetTopTags, aName)

        End Sub
        Public Overloads Overrides Sub Start()
            MyBase.Start()
        End Sub

        Protected Overrides Sub Success(ByVal elem As System.Xml.XmlElement)
            If elem.HasAttribute("artist") Then m_artist = elem.GetAttribute("artist")
            Dim tagNodes As XmlNodeList = elem.SelectNodes("tag")

            Dim tags As New Dictionary(Of String, Uri)
            For Each tagElem As XmlElement In tagNodes
                Dim name As String = Util.GetSubElementValue(tagElem, "name")
                Dim u As Uri = Nothing
                Uri.TryCreate(Util.GetSubElementValue(tagElem, "url"), UriKind.RelativeOrAbsolute, u)
                tags.Add(name, u)
            Next
            m_result = tags
        End Sub
    End Class
End Namespace
