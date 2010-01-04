Imports System.Xml
Imports LastFmLib.API20.Types
Namespace API20.User
    ''' <summary>
    ''' Get the top albums listened to by a user. You can stipulate a time period. Sends the overall chart by default. 
    ''' </summary>
    Public Class UserGetTopAlbums
        Inherits Base.BaseUserGetTopX

        Dim m_result As List(Of AlbumInfo)
        ReadOnly Property Result() As List(Of AlbumInfo)
            Get
                Return m_result
            End Get
        End Property


        ''' <summary>
        ''' Initializes a new instance of the <see cref="UserGetTopAlbums" /> class.
        ''' </summary>
        ''' <param name="uname">The username.</param>
        ''' <param name="period">The time period of the chars.</param>
        Public Sub New(ByVal uname As String, Optional ByVal period As ChartPeriod = ChartPeriod.Overall)
            MyBase.New(RequestType.UserGetTopAlbums, uname, period)
        End Sub

        
        Public Overloads Overrides Sub Start()
            MyBase.Start()
        End Sub

        Protected Overrides Sub Success(ByVal elem As System.Xml.XmlElement)
            If elem.HasAttribute("user") Then m_User = elem.GetAttribute("user")
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
