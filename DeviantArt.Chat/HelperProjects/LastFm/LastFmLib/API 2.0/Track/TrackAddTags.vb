Namespace API20.Tracks
    ''' <summary>
    ''' Tag an album using a list of user supplied tags. 
    ''' </summary>
    Public Class TrackAddTags
        Inherits Base.BaseTrackRequest
        Public Overrides ReadOnly Property RequiresAuth() As Boolean
            Get
                Return True
            End Get
        End Property
        Dim m_tags As New List(Of String)
        Property Tags() As List(Of String)
            Get
                Return m_tags
            End Get
            Set(ByVal value As List(Of String))
                m_tags = value
            End Set
        End Property

        Sub New(ByVal artist As String, ByVal title As String)
            Me.New(New Types.Track(artist, title))
        End Sub
        Sub New(ByVal track As Types.Track)
            MyBase.New(RequestType.TrackAddTags, track)
            m_accessMode = RequestAccessMode.Write
        End Sub

        Sub New(ByVal artist As String, ByVal album As String, ByVal tags As List(Of String))
            Me.New(artist, album)
            m_tags = tags
        End Sub

        Public Overrides Sub Start()
            If Tags.Count > 0 And Tags.Count <= 10 Then
                Dim tagstr As String = String.Join(",", m_tags.ToArray)
                SetAddParamValue("tags", tagstr)
            Else
                setFailed(modEnums.FailureCode.InvalidParameters, "Tag count zero or greater than 10!")
                Exit Sub
            End If
            MyBase.Start()
        End Sub
        Protected Overrides Sub Success(ByVal elem As System.Xml.XmlElement)
            'Won't ever get here
        End Sub
    End Class
End Namespace
