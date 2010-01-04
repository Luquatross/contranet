Imports System.Xml
Imports LastFmLib.API20.Types
Namespace API20.Tag
    ''' <summary>
    ''' Get the top tracks tagged by this tag, ordered by tag count. 
    ''' </summary>
    Public Class TagGetTopTracks
        Inherits Base.BaseTagRequest


        Dim m_result As List(Of Types.Track)
        ReadOnly Property Result() As List(Of Types.Track)
            Get
                Return m_result
            End Get
        End Property


        Private Sub New()
            MyBase.New(RequestType.TagGetTopTracks)
        End Sub

        Public Sub New(ByVal tagName As String)
            Me.New()
            m_Tag = tagName
        End Sub
        Public Overloads Overrides Sub Start()
            MyBase.Start()
        End Sub

        Protected Overrides Sub Success(ByVal elem As System.Xml.XmlElement)
            If elem.HasAttribute("tag") Then m_Tag = elem.GetAttribute("tag")
            Dim trackNodes As XmlNodeList = elem.SelectNodes("track")

            Dim tracks As New List(Of Types.Track)
            For Each trackElem As XmlElement In trackNodes
                Dim track As Types.Track = track.FromXmlElement(trackElem)
                tracks.Add(track)
            Next
            m_result = tracks
        End Sub
    End Class
End Namespace
