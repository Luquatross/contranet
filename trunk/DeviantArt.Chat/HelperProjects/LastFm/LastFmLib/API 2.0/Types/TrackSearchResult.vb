Namespace API20.Types
    ''' <summary>
    ''' Result type for track search.
    ''' </summary>
    Public Class TrackSearchResult
        Inherits Base.SearchResultBase

        Private m_Tracks As List(Of Types.Track)
        ''' <summary>
        ''' Gets the tracks.
        ''' </summary>
        ''' <value>The tracks.</value>
        Public ReadOnly Property Tracks() As List(Of Types.Track)
            Get
                Return m_Tracks
            End Get
            'Set(ByVal value As List(Of Types.Track))
            '    m_Tracks = value
            'End Set
        End Property
        ''' <summary>
        ''' Initializes a new instance of the <see cref="TrackSearchResult" /> class.
        ''' </summary>
        ''' <param name="e">The e.</param>
        Sub New(ByVal e As Xml.XmlElement)
            MyBase.New(e)
            m_Tracks = New List(Of Types.Track)
            For Each t As Xml.XmlElement In e.SelectNodes("trackmatches/track")
                m_Tracks.Add(Track.FromXmlElement(t))
            Next
        End Sub

    End Class
End Namespace