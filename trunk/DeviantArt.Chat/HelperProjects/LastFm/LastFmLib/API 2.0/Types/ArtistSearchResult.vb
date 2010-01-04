Namespace API20.Types
    ''' <summary>
    ''' Keeps artist search results
    ''' </summary>
    Public Class ArtistSearchResult
        Inherits Base.SearchResultBase
        Dim m_artists As List(Of ArtistInfo)

        Property Artists() As List(Of ArtistInfo)
            Get
                Return m_artists
            End Get
            Set(ByVal value As List(Of ArtistInfo))
                m_artists = value
            End Set
        End Property
        ''' <summary>
        ''' Creates a new instance of ArtistSearchResult using an xml-element
        ''' </summary>
        ''' <param name="e"></param>
        ''' <remarks></remarks>
        Sub New(ByVal e As Xml.XmlElement)
            MyBase.New(e)
        End Sub
        Public Overrides Function ToDebugString() As String
            MyBase.ToDebugString()
            Dim result As String = ""
            With Me
                result = MyBase.ToString
                Dim artists As String = ""
                For Each sArtist As ArtistInfo In .Artists
                    artists &= sArtist.Name & ", "
                Next
                artists = artists.Substring(0, artists.Length - 2)
                result &= "Matched artists: " & artists
                Return result
            End With
        End Function
        ''' <summary>
        ''' Creates an instance from an xml element containing the search results
        ''' </summary>
        ''' <param name="elem"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function FromXmlElement(ByVal elem As Xml.XmlElement) As ArtistSearchResult
            Dim m_result As New ArtistSearchResult(elem)

            Dim artistNodes As Xml.XmlNodeList = elem.SelectNodes("./artistmatches/artist")
            m_result.Artists = New List(Of ArtistInfo)
            For Each e As Xml.XmlElement In artistNodes
                Dim a As ArtistInfo = ArtistInfo.FromXmlElement(e)
                'some inconsistency in the api
                'Dim streamableVal As Integer
                'Integer.TryParse(Util.GetSubElementValue(elem, "streamable"), streamableVal)
                'a.Streamable = CBool(streamableVal)
                'Integer.TryParse(Util.GetSubElementValue(elem, "listeners"), a.NumListeners)
                m_result.Artists.Add(a)
            Next
            Return m_result
        End Function

    End Class
End Namespace
