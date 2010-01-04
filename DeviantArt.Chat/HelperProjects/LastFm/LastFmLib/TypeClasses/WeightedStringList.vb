Namespace TypeClasses
    
    ''' <summary>
    ''' A list of WeightedStrings, with the possibilty to sort them.
    ''' </summary>
    Public Class WeightedStringList
        Inherits System.Collections.Generic.List(Of WeightedString)



        'ReadOnly Property List() As List(Of WeightedString)
        '    Get
        '        Return me
        '    End Get
        'End Property
        Sub New()
            MyBase.New()
        End Sub
        Sub New(ByVal thelist As List(Of WeightedString))
            MyBase.New()
            Me.AddRange(thelist)

        End Sub

        Public Function toStringList() As List(Of String)
            Dim strings As New List(Of String)
            For Each t As WeightedString In Me
                strings.Add(t.ToString)
            Next
            Return strings
        End Function

        Private Sub qsort(ByVal sort As sortType)

            Select Case sort
                Case sortType.WeightAscending
                    Me.Sort(New Comparison(Of WeightedString)(AddressOf weightlessThan))
                    'Me.Sort(begin, [end] - begin + 1, New Comparison(Of WeightedString)(AddressOf weightlessThan))

                Case sortType.WeightDescending
                    Me.Sort(New Comparison(Of WeightedString)(AddressOf weightMoreThan))
                    'Me.Sort(begin, [end] - begin + 1, New Comparison(Of WeightedString)(AddressOf weightMoreThan))

                Case sortType.NameAscending
                    Me.Sort(New Comparison(Of WeightedString)(AddressOf caseInsensitiveLessThan))
                Case sortType.NameAscendingCaseSensitive
                    Me.Sort(New Comparison(Of WeightedString)(AddressOf caseSensitiveLessThan))
                Case sortType.NameDescending
                    Me.Sort(New Comparison(Of WeightedString)(AddressOf caseInsensitiveMoreThan))
                Case sortType.NameDescendingCaseSensitive
                    Me.Sort(New Comparison(Of WeightedString)(AddressOf caseSensitiveMoreThan))
            End Select

        End Sub



        Public Sub sortWeightingAscending()
            qsort(sortType.WeightAscending)
        End Sub

        Public Sub sortWeightingDescending()
            qsort(sortType.WeightDescending)
        End Sub

        Public Sub sortNamesAscending(Optional ByVal caseSensitive As Boolean = False)
            If Not caseSensitive Then
                qsort(sortType.NameAscending)
            Else
                qsort(sortType.NameAscendingCaseSensitive)
            End If
        End Sub

        Public Sub sortNamesDescending(Optional ByVal caseSensitive As Boolean = False)
            If Not caseSensitive Then
                qsort(sortType.NameDescending)
            Else
                qsort(sortType.NameDescendingCaseSensitive)
            End If
        End Sub

        Private Function caseSensitiveMoreThan(ByVal s1 As WeightedString, ByVal s2 As WeightedString) As Integer
            If s1.Name > s2.Name Then
                Return -1
            ElseIf s1.Name = s2.Name Then
                Return 0
            Else
                Return 1
            End If
        End Function
        'Private Delegate Function caseInsensitiveLessThanDelegate(ByRef s1 As String, ByRef s2 As String) As Boolean
        Function caseSensitiveLessThan(ByVal s1 As WeightedString, ByVal s2 As WeightedString) As Integer
            If s1.Name < s2.Name Then
                Return -1
            ElseIf s1.Name = s2.Name Then
                Return 0
            Else
                Return 1
            End If
        End Function

        Private Function caseInsensitiveMoreThan(ByVal s1 As WeightedString, ByVal s2 As WeightedString) As Integer
            If s1.Name.ToLower > s2.Name.ToLower Then
                Return -1
            ElseIf s1.Name.ToLower = s2.Name.ToLower Then
                Return 0
            Else
                Return 1
            End If
        End Function
        'Private Delegate Function caseInsensitiveLessThanDelegate(ByRef s1 As String, ByRef s2 As String) As Boolean
        Function caseInsensitiveLessThan(ByVal s1 As WeightedString, ByVal s2 As WeightedString) As Integer
            If s1.Name.ToLower < s2.Name.ToLower Then
                Return -1
            ElseIf s1.Name.ToLower = s2.Name.ToLower Then
                Return 0
            Else
                Return 1
            End If
        End Function

        'Private Delegate Function weightlessThanDelegate(ByRef s1 As WeightedString, ByRef s2 As WeightedString) As Boolean
        Private Function weightlessThan(ByVal s1 As WeightedString, ByVal s2 As WeightedString) As Integer
            If s1.weighting < s2.weighting Then
                Return -1
            ElseIf s1.weighting = s2.weighting Then
                Return 0
            Else
                Return 1
            End If
        End Function

        'Private Delegate Function weightMoreThanDelegate(ByRef s1 As WeightedString, ByRef s2 As WeightedString) As Boolean
        Private Function weightMoreThan(ByVal s1 As WeightedString, ByVal s2 As WeightedString) As Integer
            If s1.weighting > s2.weighting Then
                Return -1
            ElseIf s1.weighting = s2.weighting Then
                Return 0
            Else
                Return 1
            End If
        End Function
    End Class
End Namespace

