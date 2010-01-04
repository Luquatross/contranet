Imports System.Xml
Namespace API20.Types
    ''' <summary>
    ''' Stores music event data, like artists, description, headliner, venue and url
    ''' </summary>
    Public Class MusicEvent
        Dim m_id As Integer
        Dim m_artists As List(Of String)
        Dim m_venue As Types.Venue
        Dim m_title As String
        Dim m_startDate As DateTime
        Dim m_description As String
        Dim m_url As Uri
        Dim m_imageSmall As Uri
        Dim m_imageMedium As Uri
        Dim m_imageLarge As Uri
        Dim m_headliner As String

        Property Headliner() As String
            Get
                Return m_headliner
            End Get
            Set(ByVal value As String)
                m_headliner = value
            End Set
        End Property
        Property ImageLarge() As Uri
            Get
                Return m_imageLarge
            End Get
            Set(ByVal value As Uri)
                m_imageLarge = value
            End Set
        End Property
        Property ImageMedium() As Uri
            Get
                Return m_imageMedium
            End Get
            Set(ByVal value As Uri)
                m_imageMedium = value
            End Set
        End Property
        Property ImageSmall() As Uri
            Get
                Return m_imageSmall
            End Get
            Set(ByVal value As Uri)
                m_imageSmall = value
            End Set
        End Property

        Property Url() As Uri
            Get
                Return m_url
            End Get
            Set(ByVal value As Uri)
                m_url = value
            End Set
        End Property
        Property Description() As String
            Get
                Return m_description
            End Get
            Set(ByVal value As String)
                m_description = value
            End Set
        End Property
        Property StartDate() As DateTime
            Get
                Return m_startDate
            End Get
            Set(ByVal value As DateTime)
                m_startDate = value
            End Set
        End Property
        Property Title() As String
            Get
                Return m_title
            End Get
            Set(ByVal value As String)
                m_title = value
            End Set
        End Property
        Property Venue() As Types.Venue
            Get
                Return m_venue
            End Get
            Set(ByVal value As Types.Venue)
                m_venue = value
            End Set
        End Property
        Property Artists() As List(Of String)
            Get
                Return m_artists
            End Get
            Set(ByVal value As List(Of String))
                m_artists = value
            End Set
        End Property
        Property Id() As Integer
            Get
                Return m_id
            End Get
            Set(ByVal value As Integer)
                m_id = value
            End Set
        End Property
        Private Sub New(ByVal elem As XmlElement)

            Integer.TryParse(Util.GetSubElementValue(elem, "id"), m_id)
            m_title = Util.GetSubElementValue(elem, "title")
            m_headliner = Util.GetSubElementValue(elem, "artists/headliner")
            Dim artistNodes As XmlNodeList = elem.SelectNodes("artists/artist")
            If artistNodes.Count > 0 Then
                m_artists = New List(Of String)
                For Each a As XmlElement In artistNodes
                    m_artists.Add(a.InnerText)
                Next
            End If
            m_url = Util.GetUrl(Util.GetSubElementValue(elem, "url"))
            'venue node
            Dim v As XmlElement = elem.SelectSingleNode("venue")
            If v IsNot Nothing AndAlso Not v.IsEmpty Then m_venue = New Types.Venue(v)
        End Sub
        Shared Function FromXmlElement(ByVal e As XmlElement)
            Dim result As New MusicEvent(e)
            Return result
        End Function
        Public Function ToDebugString() As String
            Dim result As String = ""
            With Me
                result &= "Title: " & .Title & vbCrLf
                result &= "Description: " & .Description & vbCrLf
                result &= "Url: " & Util.GetUrlstrOrNothing(Me.Url) & vbCrLf
                result &= "Headliner: " & .Headliner & vbCrLf
                Dim a As String = ""
                If .Artists IsNot Nothing Then a = String.Join(",", .Artists.ToArray)
                result &= "Artists: " & a & vbCrLf
                result &= .Venue.ToString & vbCrLf

            End With
            Return result
        End Function
        Sub SetImagesByXmlElem(ByVal e As Xml.XmlElement)
            Uri.TryCreate(Util.GetSubElementValue(e, "image[@size=""small""]"), UriKind.Absolute, Me.ImageSmall)
            Uri.TryCreate(Util.GetSubElementValue(e, "image[@size=""medium""]"), UriKind.Absolute, Me.ImageMedium)
            Uri.TryCreate(Util.GetSubElementValue(e, "image[@size=""large""]"), UriKind.Absolute, Me.ImageLarge)
        End Sub
    End Class
End Namespace
