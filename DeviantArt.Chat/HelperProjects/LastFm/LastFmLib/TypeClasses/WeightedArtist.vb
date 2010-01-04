Namespace TypeClasses
    ''' <summary>
    '''An artist with a weighting by which it can be sordted
    ''' </summary>
    Public Class WeightedArtist
        Inherits Artist
        Private Structure unionstruct
            Dim weighting As Integer
            Dim count As Integer
        End Structure
        Private u As unionstruct


        Property count() As Integer
            Get
                Return u.count
            End Get
            Set(ByVal value As Integer)
                u.count = value
            End Set
        End Property
        Property weighting() As Integer
            Get
                Return u.weighting
            End Get
            Set(ByVal value As Integer)
                u.weighting = value
            End Set
        End Property
        Public Sub New(ByVal name As String, Optional ByVal w As Integer = -1)
            m_Name = name
            u.weighting = w
        End Sub

        Public Shared Function weighted(ByVal name As String, ByVal w As Integer) As WeightedArtist
            Dim t As New WeightedArtist(name)
            't.u.count = c
            t.weighting = w
            Return t

        End Function
        Public Shared Function counted(ByVal name As String, ByVal c As Integer) As WeightedArtist
            Dim t As New WeightedArtist(name)
            't.u.count = c
            t.count = c
            Return t
        End Function

        Shared Operator >(ByVal a As WeightedArtist, ByVal b As WeightedArtist) As Boolean
            Return a.m_Name > b.m_Name
        End Operator
        Shared Operator <(ByVal a As WeightedArtist, ByVal b As WeightedArtist) As Boolean
            Return a.m_Name < b.m_Name
        End Operator
        Public Shared Widening Operator CType(ByVal obj As WeightedArtist) As String
            Return obj.m_Name
        End Operator
        Public Sub SetImageUrlByString(ByVal s As String)
            m_ImageUrl = IIf(String.IsNullOrEmpty(s), Nothing, New Uri(s))
        End Sub
        Public Sub SetImageSmallUrlByString(ByVal s As String)
            m_ImageSmallUrl = IIf(String.IsNullOrEmpty(s), Nothing, New Uri(s))
        End Sub
        Public Function GetBiggestImageUrl() As Uri
            If m_ImageUrl IsNot Nothing Then Return m_ImageUrl
            Return m_ImageSmallUrl
        End Function
    End Class

End Namespace
