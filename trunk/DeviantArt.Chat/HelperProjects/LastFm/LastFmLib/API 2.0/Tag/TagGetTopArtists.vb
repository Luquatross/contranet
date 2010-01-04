Imports System.Xml
Imports LastFmLib.API20.Types
Namespace API20.Tag
    Public Class TagGetTopArtists
        Inherits Base.BaseTagRequest
        Dim m_topartists As List(Of ArtistInfo)
        ReadOnly Property Result() As List(Of ArtistInfo)
            Get
                Return m_topartists
            End Get
        End Property
        Sub New(ByVal tag As String)
            MyBase.New(RequestType.TagGetTopArtists, tag)
        End Sub
        Protected Overrides Sub Success(ByVal elem As System.Xml.XmlElement)
            Dim list As XmlNodeList = elem.SelectNodes("artist")
            m_topartists = New List(Of ArtistInfo)
            For Each u As XmlElement In list
                m_topartists.Add(ArtistInfo.FromXmlElement(u))
            Next
        End Sub


        Public Overrides Function ToString() As String
            Dim result As String = "Top artists for: " & m_Tag
            For Each f As ArtistInfo In m_topartists
                result &= f.Name & ", "
            Next
            result.Remove(result.IndexOf(","), 2)
            Return result
        End Function
    End Class
End Namespace