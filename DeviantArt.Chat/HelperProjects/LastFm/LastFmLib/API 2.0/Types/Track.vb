Namespace API20.Types
    ''' <summary>
    ''' Stores track meta data
    ''' </summary>
    Public Class Track
        Inherits Base.BaseImageData
        Dim m_title As String
        Dim m_mbid As Guid
        Dim m_playCount As Integer
        Dim m_numListeners As Integer
        Dim m_url As Uri
        Dim m_artist As New ArtistInfo()
        Dim m_isStreamable As Boolean
        Dim m_isFullTrack As Boolean

        Property IsFullTrack() As Boolean
            Get
                Return m_isFullTrack
            End Get
            Set(ByVal value As Boolean)
                m_isFullTrack = value
            End Set
        End Property
        Property IsStreamable() As Boolean
            Get
                Return m_isStreamable
            End Get
            Set(ByVal value As Boolean)
                m_isStreamable = value
            End Set
        End Property
        Property Artist() As ArtistInfo
            Get
                Return m_artist
            End Get
            Set(ByVal value As ArtistInfo)
                m_artist = value
            End Set
        End Property
        ReadOnly Property ArtistName() As String
            Get
                Return Artist.Name
            End Get
        End Property
        Property Url() As Uri
            Get
                Return m_url
            End Get
            Set(ByVal value As Uri)
                m_url = value
            End Set
        End Property
        Property NumListeners() As Integer
            Get
                Return m_numListeners
            End Get
            Set(ByVal value As Integer)
                m_numListeners = value
            End Set
        End Property
        Property PlayCount() As Integer
            Get
                Return m_playCount
            End Get
            Set(ByVal value As Integer)
                m_playCount = value
            End Set
        End Property
        Property Mbid() As Guid
            Get
                Return m_mbid
            End Get
            Set(ByVal value As Guid)
                m_mbid = value
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
        Sub New()

        End Sub

        Sub New(ByVal artist As String, ByVal title As String)
            m_artist = New ArtistInfo(artist)
            m_title = title
        End Sub

        Sub New(ByVal artist As ArtistInfo, ByVal title As String)
            m_artist = artist
            m_title = title
        End Sub
        Sub New(ByVal mbid As Guid)
            m_mbid = mbid
        End Sub
        Function ToDebugString() As String
            Dim result As String = ""
            result &= "Artist: " & Artist.Name
            result &= "Title: " & Title
            result &= "Play count: " & PlayCount
            result &= "MB-Id: " & Mbid.ToString
            result &= "Streamable" & IsStreamable.ToString
            result &= "Full track: " & IsFullTrack
            Return result
        End Function
        Protected Sub New(ByVal e As Xml.XmlElement)
            With Me
                .Title = Util.GetSubElementValue(e, "name")
                Integer.TryParse(Util.GetSubElementValue(e, "playcount"), .PlayCount)
                .Mbid = Util.GetGuid(Util.GetSubElementValue(e, "mbid"))
                Uri.TryCreate(Util.GetSubElementValue(e, "url"), UriKind.RelativeOrAbsolute, .Url)
                Dim stream As Xml.XmlElement = e.SelectSingleNode("streamable")
                If stream IsNot Nothing Then
                    m_isStreamable = CBool(stream.InnerText)
                    If stream.HasAttribute("fulltrack") Then m_isFullTrack = stream.GetAttribute("fulltrack")
                End If

                Dim artistElem As Xml.XmlElement = e.SelectSingleNode("artist")
                If artistElem IsNot Nothing Then .Artist = ArtistInfo.FromXmlElement(artistElem)
                SetImagesByXmlElem(e)
            End With
        End Sub
        Shared Function FromXmlElement(ByVal e As Xml.XmlElement) As Track
            Dim result As New Track(e)
            Return result
        End Function
        Public Overrides Function ToString() As String
            Return If(Artist IsNot Nothing, Artist.Name & " - ", "") & Title
        End Function
    End Class
End Namespace