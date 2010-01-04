Imports System.Xml
Imports LastFmLib.API20.Types
Namespace API20.Artist
    ''' <summary>
    ''' Get the top fans for an artist on Last.fm, based on listening data. 
    ''' </summary>
    Public Class ArtistGetTopFans
        Inherits Base.BaseArtistRequest
        Dim m_topFans As List(Of TopFan)
        ReadOnly Property Result() As List(Of TopFan)
            Get
                Return m_topFans
            End Get
        End Property
        Sub New(ByVal artist As String)
            MyBase.New(RequestType.ArtistGetTopFans, artist)
        End Sub
        Protected Overrides Sub Success(ByVal elem As System.Xml.XmlElement)
            Dim list As XmlNodeList = elem.SelectNodes("user")
            m_topFans = New List(Of TopFan)
            For Each u As XmlElement In list
                Dim fan As TopFan = TopFan.FromXmlElemnt(u)
                m_topFans.Add(fan)
            Next
        End Sub

        Public Overrides Function ToString() As String
            Dim result As String = "Top fans for: " & m_artist
            For Each f As TopFan In m_topFans
                result &= f.Name & ", "
            Next
            result.Remove(result.IndexOf(","), 2)
            Return result
        End Function
    End Class
End Namespace