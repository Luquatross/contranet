Imports System.Xml
Imports System.Xml.XPath
Namespace Radio
    ''' <summary>
    ''' Resolves the XSPF playlist received from Last.fm
    ''' </summary>
    Public Class XspfResolver
        Private m_station As String
        Private m_skipLimit As Integer
        ''' <summary>
        ''' Gets the skip limit(if there is).
        ''' </summary>
        ''' <value>The skip limit.</value>
        ReadOnly Property SkipLimit() As Integer
            Get
                Return m_skipLimit
            End Get
        End Property
        ''' <summary>
        ''' Gets the station url
        ''' </summary>
        ''' <value>The station url</value>
        ReadOnly Property Station() As String
            Get
                Return m_station
            End Get
        End Property


        ''' <summary>
        ''' Resolves the tracks from an XSPF-containing string.
        ''' </summary>
        ''' <param name="xspf">The string containing the XSPF.</param>
        ''' <returns></returns>
        Public Function resolveTracks(ByVal xspf As String) As List(Of TypeClasses.TrackInfo)
            Dim tracks As New List(Of TypeClasses.TrackInfo)

            Dim doc As New XmlDocument

            Try
                doc.LoadXml(xspf)
            Catch x As XmlException
                Dim [error] As String = "Couldn't parse XSPF." & vbCrLf & "Error: " & x.Message & vbCrLf & "Line: " & x.LineNumber
                Throw New Exceptions.ParseException("Xspfresover.ResolveTracks: Couldn't parse XSPF.", x)
            End Try
            'Get Station name
            Dim docElem As XmlElement = doc.DocumentElement
            Dim title As XmlNode = docElem.Item("title")
            If Not title Is Nothing Then
                Dim e As XmlElement = title
                If Not title Is Nothing Then
                    m_station = Uri.UnescapeDataString(e.InnerText)
                    m_station = m_station.Replace("+", " ")
                    m_station = m_station.Trim
                    If Not String.IsNullOrEmpty(Station) Then
                        m_station = m_station.Substring(0, 1).ToUpper & m_station.Substring(1)
                    End If
                End If
            End If
            ' Get skip count
            m_skipLimit = -1

            Dim link As XmlElement = docElem.Item("link")
            While (Not elementIsNullOrEmpty(link))

                If link.HasAttribute("rel") And link.GetAttribute("rel") = "http://www.last.fm/skipsLeft" Then

                    Dim conversionOk = True

                    Try
                        m_skipLimit = CInt(link.InnerText())
                    Catch ex As Exception
                        Debug.Print("Fehler in XspfResolver.resolveTracks(): " & ex.Message)
                        conversionOk = False
                    Finally
                        If (Not conversionOk) Then

                            'LOGL( 2, "Failed to read skip limit" );
                            m_skipLimit = -1
                        End If
                    End Try
                    Exit While

                Else

                    'link = link.nextSiblingElement( "link" )
                    link = GetFirstChild(link.ChildNodes, "link")
                    If link.IsEmpty Then link = link.NextSibling
                End If
            End While

            ' God I hate writing XML parsing code, it is truly the most tedious thing on earth.

            ' Now, look for tracklist
            Dim trackList As XmlNode = docElem.Item("trackList")

            If (trackList Is Nothing) Then

                Dim [error] As String = "Required XSPF node trackList missing."

                'LOGL( 1, error );
                Throw New Exceptions.ParseException("Xspfresover.ResolveTracks: " & [error])
            End If

            'Dim trackNode As XmlNode = trackList.FirstChild()
            For Each trackNode As XmlNode In trackList.SelectNodes("track")
                'While (Not trackNode Is Nothing)

                If Not (trackNode.Name() = "track") Then

                    Continue For
                End If

                Dim track As New TypeClasses.TrackInfo
                With track
                    .Source = TypeClasses.TrackInfo.SourceEnum.Radio

                    .AuthCode = childText(trackNode, "lastfm:trackauth")
                    .Title = childText(trackNode, "title")
                    .ArtistName = childText(trackNode, "creator")
                    .Album = childText(trackNode, "album")
                    .Duration = CInt(CInt(childText(trackNode, "duration")) / 1000) ' comes in ms

                    ' Hacky workaround for tracks wrongly labelled as having duration 0 on the site
                    If (.Duration() = 0) Then

                        ' Make em a minute
                        .Duration = 60
                    End If
                End With

                ' There can be more than one location
                Dim paths As New List(Of String)
                Dim locations As XmlNodeList = trackNode.SelectNodes("location")

                For Each l As XmlNode In locations
                    paths.Add(l.InnerText)
                Next
                track.SetPaths(paths)

                ' If parsing of any of these vital fields failed, just don't add
                ' the track to the playlist.
                If (Not String.IsNullOrEmpty(track.ArtistName()) And Not String.IsNullOrEmpty(track.Title) And _
                track.Duration() <> 0 And Not _
                     String.IsNullOrEmpty(track.Path())) Then

                    tracks.Add(track)
                End If

                'trackNode = trackNode.NextSibling()
                'End While
            Next
            Return tracks
        End Function

        ''' <summary>
        ''' Returns the text that a child of a parent node contaions
        ''' </summary>
        ''' <param name="parent">The parent node.</param>
        ''' <param name="tagName">Name of the child tag.</param>
        ''' <returns></returns>
        Private Function childText(ByVal parent As XmlNode, ByVal tagName As String)
            Return parent.Item(tagName).InnerText
        End Function
        ''' <summary>
        ''' Gets the first child.
        ''' </summary>
        ''' <param name="x">The node list.</param>
        ''' <param name="s">The child name.</param>
        ''' <returns></returns>
        Private Function GetFirstChild(ByVal x As XmlNodeList, ByVal s As String) As XmlElement
            For Each a As XmlNode In x
                If a.Name = s Then Return a
            Next
            Return Nothing
        End Function
        ''' <summary>
        ''' returns wheter the xmlElement is empty/nothing or not.
        ''' </summary>
        ''' <param name="location">The location.</param>
        ''' <returns></returns>
        Private Shared Function elementIsNullOrEmpty(ByVal location As XmlElement) As Boolean
            If location Is Nothing Then Return True
            If location.IsEmpty Then Return True
        End Function
    End Class
End Namespace