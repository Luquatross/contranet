Namespace API20.Artist
    ''' <summary>
    ''' Get a list of events for this artist. Easily integratable into calendars, using the ical standard (see feeds section below). 
    ''' </summary>
    Public Class ArtistGetEvents
        Inherits Base.BaseRequest
        Dim m_result As List(Of Types.MusicEvent)
        Dim m_artist As String

        Property artist() As String
            Get
                Return m_artist
            End Get
            Set(ByVal value As String)
                m_artist = value
            End Set
        End Property

        ReadOnly Property Result() As List(Of Types.MusicEvent)
            Get
                Return m_result
            End Get
        End Property
        Private Sub New()
            MyBase.New(RequestType.ArtistGetEvents)
            m_requiredParams.Add("artist")
        End Sub
        Sub New(ByVal artist As String)
            Me.New()
            m_artist = artist
        End Sub
        Public Overrides Sub Start()
            SetAddParamValue("artist", m_artist)
            MyBase.Start()
        End Sub
        Protected Overrides Sub Success(ByVal elem As System.Xml.XmlElement)
            If elem.HasAttribute("artist") Then m_artist = elem.GetAttribute("artist")

            m_result = New List(Of Types.MusicEvent)
            Dim eventnodes As Xml.XmlNodeList = elem.SelectNodes("event")
            For Each e As Xml.XmlElement In eventnodes
                Dim ev As Types.MusicEvent = Types.MusicEvent.FromXmlElement(e)
                m_result.Add(ev)
            Next

        End Sub
    End Class
End Namespace