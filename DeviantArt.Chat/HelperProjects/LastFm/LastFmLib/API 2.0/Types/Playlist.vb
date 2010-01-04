Imports System.Xml
Namespace API20.Types
    ''' <summary>
    ''' Class for holding playlists. Not implemented yet
    ''' </summary>
    Public Class Playlist

        Private m_title As String
        Public ReadOnly Property Title() As String
            Get
                Return m_title
            End Get
        End Property


        Private m_annotation As String
        Public ReadOnly Property Annotation() As String
            Get
                Return m_annotation
            End Get
        End Property

        Private m_creator As Uri
        Public ReadOnly Property Creator() As Uri
            Get
                Return m_creator
            End Get
        End Property

        Private m_date As DateTime
        Public ReadOnly Property [Date]() As DateTime
            Get
                Return m_date
            End Get
        End Property


        Private m_tracks As List(Of PlaylistTrack)
        Public ReadOnly Property Tracks() As List(Of PlaylistTrack)
            Get
                Return m_tracks
            End Get
        End Property

        Private m_version As String
        Public ReadOnly Property Version() As String
            Get
                Return m_version
            End Get
        End Property

        Sub New(ByVal elem As XmlElement)
            Dim nsm As New XmlNamespaceManager(elem.OwnerDocument.NameTable)
            nsm.AddNamespace(String.Empty, "http://xspf.org/ns/0/")

            m_version = Util.GetAttrValue(elem, "version")
            m_title = Util.GetSubElementValue(elem, "title[@xmlns='http://xspf.org/ns/0/']")
            m_annotation = Util.GetSubElementValue(elem, "annotation")
            DateTime.TryParse(Util.GetSubElementValue(elem, "date"), m_date)
            m_tracks = New List(Of PlaylistTrack)
            For Each tNode As XmlElement In elem.SelectNodes("tracklist/track")
                Dim t As New PlaylistTrack(tNode)
                m_tracks.Add(t)
            Next
        End Sub
        Function ToDebugString() As String
            Dim result As String = ""
            result &= "Playlist title: " & m_title & vbCrLf
            result &= "Annotation: " & m_annotation & vbCrLf
            result &= "Creator: " & m_creator.AbsoluteUri & vbCrLf

            For Each t As PlaylistTrack In m_tracks
                result &= t.todebugString() & vbCrLf
            Next
            Return result
        End Function
    End Class
End Namespace