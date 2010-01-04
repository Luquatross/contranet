Namespace RSS
    Public Class NamedUrl
        Protected m_name As String
        Protected m_url As Uri

        Property Name() As String
            Get
                Return m_name
            End Get
            Set(ByVal value As String)
                m_name = value
            End Set
        End Property

        Sub New()

        End Sub
        Sub New(ByVal name As String, ByVal domain As Uri)
            Me.m_name = name
            Me.m_url = domain
        End Sub

    End Class

End Namespace