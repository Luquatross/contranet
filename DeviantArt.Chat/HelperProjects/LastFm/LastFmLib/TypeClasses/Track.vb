Namespace TypeClasses
    ''' <summary>
    ''' A track(without album,...)
    ''' </summary>
    Public Class Track
        Implements ICloneable

        '//TODO we have issues with naming and many different classes for this kind of data
        '   // main naming issue is class name and the track member data string
        Protected m_artist As String
        Protected m_title As String

        ''' <summary>
        ''' Gets or sets the artist.
        ''' </summary>
        ''' <value>The artist.</value>
        Property ArtistName() As String
            Get
                Return m_artist
            End Get
            Set(ByVal value As String)
                m_artist = value
            End Set
        End Property
        ''' <summary>
        ''' Initializes a new instance of the <see cref="Track" /> class.
        ''' </summary>
        Sub New()

        End Sub
        ''' <summary>
        ''' Initializes a new instance of the <see cref="Track" /> class.
        ''' </summary>
        ''' <param name="artist">The artist.</param>
        ''' <param name="title">The title.</param>
        Sub New(ByVal artist As String, ByVal title As String)
            m_artist = artist
            m_title = title
        End Sub

        ''' <summary>
        ''' Gets or sets the title.
        ''' </summary>
        ''' <value>The title.</value>
        Property Title() As String
            Get
                Return m_title
            End Get
            Set(ByVal value As String)
                m_title = value
            End Set
        End Property
        ''' <summary>
        ''' Implements the operator =.
        ''' </summary>
        ''' <param name="trackone">The trackone.</param>
        ''' <param name="other">The other.</param>
        ''' <returns>The result of the operator.</returns>
        Public Shared Operator =(ByVal trackone As Track, ByVal other As Track) As Boolean
            Return other.m_artist = trackone.m_artist And trackone.m_title = other.m_title
        End Operator
        ''' <summary>
        ''' Implements the operator &lt;&gt;.
        ''' </summary>
        ''' <param name="trackone">The trackone.</param>
        ''' <param name="other">The other.</param>
        ''' <returns>The result of the operator.</returns>
        Public Shared Operator <>(ByVal trackone As Track, ByVal other As Track) As Boolean

            Return Not (trackone = other)
        End Operator
        Public Shared Widening Operator CType(ByVal t As Track) As String
            Return t.DisplayText
        End Operator

        ''' <summary>
        ''' Gets the display text(= "[artist] - [title]")
        ''' </summary>
        ''' <value>The display text.</value>
        ReadOnly Property DisplayText() As String
            Get
                '   //FIXME duplicated in TrackInfo.cpp
                If String.IsNullOrEmpty(m_artist) Then
                    Return m_title ' //NOTE could be empty too
                ElseIf String.IsNullOrEmpty(m_title) Then
                    Return m_artist
                Else
                    Return m_artist & " - " & m_title
                End If
            End Get
        End Property
        ''' <summary>
        ''' Gets a value indicating whether this instance is empty.
        ''' </summary>
        ''' <value><c>true</c> if this instance is empty; otherwise, <c>false</c>.</value>
        ReadOnly Property IsEmpty() As Boolean
            Get
                Return String.IsNullOrEmpty(m_title) And String.IsNullOrEmpty(m_artist)
            End Get
        End Property
        ''' <summary>
        ''' Same as displayText method. 
        ''' </summary>
        Public Overrides Function ToString() As String
            Return Me.DisplayText
        End Function

        Public Overridable Function ToTrackInfo() As TrackInfo
            Return New TrackInfo(Me.ArtistName, "", Me.Title)
        End Function
        'The following is not ported yet(is there a need for it?)
        '    static Track fromMimeData( const class QMimeData* );

        Public Overridable Function Clone() As Object Implements System.ICloneable.Clone
            Return New Track(Me.ArtistName, Me.Title)
        End Function
  

    End Class
End Namespace
