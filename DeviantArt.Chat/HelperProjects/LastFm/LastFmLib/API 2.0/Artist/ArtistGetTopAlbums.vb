Imports System.Xml
Imports LastFmLib.API20.Types
Namespace API20.Artist
    ''' <summary>
    ''' Get the top albums for an artist on Last.fm, ordered by popularity. 
    ''' </summary>
    Public Class ArtistGetTopAlbums
        Inherits Base.BaseArtistRequest

        Dim m_result As List(Of AlbumInfo)
        ReadOnly Property Result() As List(Of AlbumInfo)
            Get
                Return m_result
            End Get
        End Property

        Public Sub New(ByVal aName As String)
            MyBase.New(RequestType.ArtistGetTopAlbums, aName)
        End Sub
        Public Overloads Overrides Sub Start()
            SetAddParamValue("artist", m_artist)
            MyBase.Start()
        End Sub


        Protected Overrides Sub Success(ByVal elem As System.Xml.XmlElement)
            If elem.HasAttribute("artist") Then m_artist = elem.GetAttribute("artist")
            Dim albumNodes As XmlNodeList = elem.SelectNodes("album")

            Dim albums As New List(Of AlbumInfo)
            For Each albumElem As XmlElement In albumNodes
                Dim album As AlbumInfo = AlbumInfo.FromXmlElement(albumElem)
                albums.Add(album)
            Next
            m_result = albums
        End Sub
    End Class
End Namespace
