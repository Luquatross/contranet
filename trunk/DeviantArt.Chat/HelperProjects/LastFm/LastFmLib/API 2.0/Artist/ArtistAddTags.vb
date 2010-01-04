Namespace API20.Artist
    ''' <summary>
    ''' Tag an artist with one or more user supplied tags. 
    ''' </summary>
    ''' <remarks></remarks>
    Public Class ArtistAddTags
        Inherits Base.BaseArtistRequest

        Dim m_tags As New List(Of String)
        Property Tags() As List(Of String)
            Get
                Return m_tags
            End Get
            Set(ByVal value As List(Of String))
                m_tags = value
            End Set
        End Property
        Public Overrides ReadOnly Property RequiresAuth() As Boolean
            Get
                Return True
            End Get
        End Property
        Sub New(ByVal artist As String)
            MyBase.New(RequestType.ArtistAddTags, artist)
            m_accessMode = RequestAccessMode.Write
        End Sub

        Sub New(ByVal artist As String, ByVal tags As List(Of String))
            Me.New(artist)
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
