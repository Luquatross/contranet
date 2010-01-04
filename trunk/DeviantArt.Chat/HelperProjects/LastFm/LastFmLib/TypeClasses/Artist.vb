
Namespace TypeClasses
    ''' <summary>
    ''' The Artist class contains things related to an artist, such as image urls, play count,MusicBrainz ID,...
    ''' </summary>
    Public Class Artist
        Protected m_Name As String
        Protected m_PlayCount As Integer
        Protected m_mbId As String
        Protected m_Url As Uri
        Protected m_ImageUrl As Uri
        Protected m_ImageSmallUrl As Uri

        ''' <summary>
        ''' Gets or sets the image url
        ''' </summary>
        ''' <value>The image url.</value>
        Property Image() As Uri
            Get
                Return m_ImageUrl
            End Get
            Set(ByVal value As Uri)
                m_ImageUrl = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets url for the small image.
        ''' </summary>
        Property ImageSmall() As Uri
            Get
                Return m_ImageSmallUrl
            End Get
            Set(ByVal value As Uri)
                m_ImageSmallUrl = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the URL of the artist page
        ''' </summary>
        ''' <value>The URL.</value>
        Property Url() As Uri
            Get
                Return m_Url
            End Get
            Set(ByVal value As Uri)
                m_Url = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the play count for this artist
        ''' </summary>
        ''' <value>The play count.</value>
        Property PlayCount() As Integer
            Get
                Return m_PlayCount
            End Get
            Set(ByVal value As Integer)
                m_PlayCount = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the MusicBrainz ID.
        ''' </summary>
        ''' <value>The mb id.</value>
        Property mbId() As String
            Get
                Return m_mbId
            End Get
            Set(ByVal value As String)
                m_mbId = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the name of the artist
        ''' </summary>
        ''' <value>The name.</value>
        Property Name() As String
            Get
                Return m_Name
            End Get
            Set(ByVal value As String)
                m_Name = value
            End Set
        End Property
        ''' <summary>
        ''' Initializes a new instance of the <see cref="Artist" /> class.
        ''' </summary>
        Sub New()

        End Sub
        ''' <summary>
        ''' Initializes a new instance of the <see cref="Artist" /> class.
        ''' </summary>
        ''' <param name="name">The name.</param>
        ''' <param name="url">The URL.</param>
        ''' <param name="ImageUrl">The image URL.</param>
        ''' <param name="ImageSmallUrl">The URL to the small image.</param>
        ''' <param name="playcount">The playcount.</param>
        ''' <param name="mbid">The mbid.</param>
        Sub New(ByVal name As String, Optional ByVal url As Uri = Nothing, Optional ByVal ImageUrl As Uri = Nothing, Optional ByVal ImageSmallUrl As Uri = Nothing, Optional ByVal playcount As Integer = 0, Optional ByVal mbid As String = "")
            m_Name = name
            m_Url = url
            m_ImageUrl = ImageUrl
            m_ImageSmallUrl = ImageSmallUrl
            m_mbId = mbid
            m_PlayCount = playcount
        End Sub
    End Class
End Namespace