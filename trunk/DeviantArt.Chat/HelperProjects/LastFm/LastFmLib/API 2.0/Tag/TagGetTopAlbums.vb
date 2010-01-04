Imports System.Xml
Imports LastFmLib.API20.Types
Namespace API20.Tag
    ''' <summary>
    ''' Get the top albums tagged by this tag, ordered by tag count. 
    ''' </summary>
    Public Class TagGetTopAlbums
        Inherits Base.BaseTagRequest
        Dim m_result As List(Of AlbumInfo)
        ReadOnly Property Result() As List(Of AlbumInfo)
            Get
                Return m_result
            End Get
        End Property



        Private Sub New()
            MyBase.New(RequestType.TagGetTopAlbums)
        End Sub

        Public Sub New(ByVal tag As String)
            Me.New()
            m_Tag = tag
        End Sub
        Public Overloads Overrides Sub Start()
            MyBase.Start()
        End Sub

        Protected Overrides Sub Success(ByVal elem As System.Xml.XmlElement)
            If elem.HasAttribute("tag") Then m_Tag = elem.GetAttribute("tag")
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
