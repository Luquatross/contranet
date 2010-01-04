Namespace TypeClasses
    ''' <summary>
    ''' This class is used for all SimilarXXX things, like SimilarTagsRequest. Each item has a name and a weighting by which it can be sorted.
    ''' </summary>
    Public Class WeightedString
        Private Structure unionstruct
            Dim weighting As Integer
            Dim count As Integer
        End Structure
        Private u As unionstruct
        Private val As String

        Property Name() As String
            Get
                Return val
            End Get
            Set(ByVal value As String)
                val = value
            End Set
        End Property
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
            val = name
            u.weighting = w
        End Sub

        Public Shared Function weighted(ByVal name As String, ByVal w As Integer) As WeightedString
            Dim t As New WeightedString(name)
            't.u.count = c
            t.weighting = w
            Return t

        End Function
        Public Shared Function counted(ByVal name As String, ByVal c As Integer) As WeightedString
            Dim t As New WeightedString(name)
            't.u.count = c
            t.count = c
            Return t
        End Function

        Shared Operator >(ByVal a As WeightedString, ByVal b As WeightedString) As Boolean
            Return a.val > b.val
        End Operator
        Shared Operator <(ByVal a As WeightedString, ByVal b As WeightedString) As Boolean
            Return a.val < b.val
        End Operator
        Public Shared Widening Operator CType(ByVal obj As WeightedString) As String
            Return obj.val
        End Operator


    End Class

End Namespace