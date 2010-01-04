Namespace TypeClasses
    ''' <summary>
    ''' A class for holding meta data of an user(Name, image, recent tracks list and his last activity)
    ''' </summary>
    ''' <remarks>Needed Proeprtys</remarks>
    Public Class UserMetaData
        Private m_recentTracks As New List(Of String)
        Private m_name As String
        Private m_lastActivity As String
        Private m_image As Uri

        ''' <summary>
        ''' Gets or sets the name.
        ''' </summary>
        ''' <value>The name.</value>
        Property Name() As String
            Get
                Return m_name
            End Get
            Set(ByVal value As String)
                m_name = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the image.
        ''' </summary>
        ''' <value>The image.</value>
        Property Image() As Uri
            Get
                Return m_image
            End Get
            Set(ByVal value As Uri)
                m_image = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the recent tracks list.
        ''' </summary>
        ''' <value>The recent tracks.</value>
        Property recentTracks() As List(Of String)
            Get
                Return m_recentTracks
            End Get
            Set(ByVal value As List(Of String))
                m_recentTracks = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets the last activity date string.
        ''' </summary>
        ''' <value>The last activity.</value>
        Property LastActivity() As String
            Get
                Return m_lastActivity
            End Get
            Set(ByVal value As String)
                m_lastActivity = value
            End Set
        End Property
    End Class
End Namespace
