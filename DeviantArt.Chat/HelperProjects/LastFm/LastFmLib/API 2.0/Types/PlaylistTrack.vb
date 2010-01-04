Imports System.Xml
Namespace API20.Types
    Public Class PlaylistTrack

        Private m_artist As String
        Public ReadOnly Property Artist() As String
            Get
                Return m_artist
            End Get

        End Property
        Private m_album As String
        Public ReadOnly Property Album() As String
            Get
                Return m_album
            End Get

        End Property
        Private m_title As String
        Public ReadOnly Property Title() As String
            Get
                Return m_title
            End Get

        End Property
        Private m_identifier As Uri
        Public ReadOnly Property Identifier() As Uri
            Get
                Return m_identifier
            End Get

        End Property
        Private m_duration As Integer
        Public ReadOnly Property Duration() As Integer
            Get
                Return m_duration
            End Get

        End Property
        Private m_info As Uri
        Public ReadOnly Property InfoUrl() As Uri
            Get
                Return m_info
            End Get

        End Property
        Private m_image As Uri
        Public ReadOnly Property Image() As Uri
            Get
                Return m_image
            End Get

        End Property
        Private m_artistpage As Uri
        Public ReadOnly Property ArtistPage() As Uri
            Get
                Return m_artistpage
            End Get

        End Property


        Private m_albumPage As Uri
        Public ReadOnly Property AlbumPage() As Uri
            Get
                Return m_albumPage
            End Get

        End Property

        Private m_trackPage As Uri
        Public ReadOnly Property TrackPage() As Uri
            Get
                Return m_trackPage
            End Get
        End Property

        Sub New(ByVal elem As XmlElement)
            m_artist = Util.GetSubElementValue(elem, "creator")
            m_album = Util.GetSubElementValue(elem, "album")
            m_title = Util.GetSubElementValue(elem, "title")
            m_identifier = Util.GetUrl(Util.GetSubElementValue(elem, "indentifier"))
            Integer.TryParse(Util.GetSubElementValue(elem, "duration"), m_duration)
            m_info = Util.GetUrl(Util.GetSubElementValue(elem, "info"))
            m_image = Util.GetUrl(Util.GetSubElementValue(elem, "image"))
            Dim extension As XmlElement = elem.SelectSingleNode("extension")
            m_artistpage = Util.GetUrl(Util.GetSubElementValue(extension, "artistpage"))
            m_albumPage = Util.GetUrl(Util.GetSubElementValue(extension, "albumpage"))
            m_trackPage = Util.GetUrl(Util.GetSubElementValue(extension, "trackpage"))
        End Sub

        Function ToDebugString()
            Dim result As String = ""
            result &= "Artist: " & m_artist & vbCrLf
            result &= "Album: " & m_album & vbCrLf
            result &= "Title: " & m_title & vbCrLf
            result &= "Duration: " & m_duration & vbCrLf
            result &= "Info-url: " & m_info.AbsoluteUri & vbCrLf
            result &= "Image: " & m_image.AbsoluteUri & vbCrLf
            result &= "Artist page: " & m_artistpage.AbsoluteUri & vbCrLf
            result &= "Album page: " & m_albumPage.AbsoluteUri & vbCrLf
            result &= "Track page: " & m_trackPage.AbsoluteUri & vbCrLf
            Return result
        End Function

    End Class
End Namespace