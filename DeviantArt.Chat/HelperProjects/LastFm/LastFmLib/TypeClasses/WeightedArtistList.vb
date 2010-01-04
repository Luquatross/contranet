Namespace TypeClasses
    Public Class WeightedArtistList
        Inherits System.Collections.Generic.List(Of WeightedArtist)

        Sub New()
            MyBase.New()
        End Sub
        Sub New(ByVal thelist As List(Of WeightedArtist))
            MyBase.New()
            Me.AddRange(thelist)

        End Sub

        Public Function toStringList() As List(Of String)
            Dim strings As New List(Of String)
            For Each t As WeightedArtist In Me
                strings.Add(t.ToString)
            Next
            Return strings
        End Function

        Private Sub qsort(ByVal sort As sortType)

            Select Case sort
                Case sortType.WeightAscending
                    Me.Sort(New Comparison(Of WeightedArtist)(AddressOf weightlessThan))

                Case sortType.WeightDescending
                    Me.Sort(New Comparison(Of WeightedArtist)(AddressOf weightMoreThan))

                Case sortType.NameAscending
                    Me.Sort(New Comparison(Of WeightedArtist)(AddressOf caseInsensitiveLessThan))
                Case sortType.NameAscendingCaseSensitive
                    Me.Sort(New Comparison(Of WeightedArtist)(AddressOf caseSensitiveLessThan))
                Case sortType.NameDescending
                    Me.Sort(New Comparison(Of WeightedArtist)(AddressOf caseInsensitiveMoreThan))
                Case sortType.NameAscendingCaseSensitive
                    Me.Sort(New Comparison(Of WeightedArtist)(AddressOf caseSensitiveMoreThan))
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

        Private Function caseSensitiveMoreThan(ByVal s1 As WeightedArtist, ByVal s2 As WeightedArtist) As Integer
            If s1.Name > s2.Name Then
                Return -1
            ElseIf s1.Name = s2.Name Then
                Return 0
            Else
                Return 1
            End If
        End Function
        'Private Delegate Function caseInsensitiveLessThanDelegate(ByRef s1 As String, ByRef s2 As String) As Boolean
        Function caseSensitiveLessThan(ByVal s1 As WeightedArtist, ByVal s2 As WeightedArtist) As Integer
            If s1.Name < s2.Name Then
                Return -1
            ElseIf s1.Name = s2.Name Then
                Return 0
            Else
                Return 1
            End If
        End Function

        Private Function caseInsensitiveMoreThan(ByVal s1 As WeightedArtist, ByVal s2 As WeightedArtist) As Integer
            If s1.Name.ToLower > s2.Name.ToLower Then
                Return -1
            ElseIf s1.Name.ToLower = s2.Name.ToLower Then
                Return 0
            Else
                Return 1
            End If
        End Function
        'Private Delegate Function caseInsensitiveLessThanDelegate(ByRef s1 As String, ByRef s2 As String) As Boolean
        Function caseInsensitiveLessThan(ByVal s1 As WeightedArtist, ByVal s2 As WeightedArtist) As Integer
            If s1.Name.ToLower < s2.Name.ToLower Then
                Return -1
            ElseIf s1.Name.ToLower = s2.Name.ToLower Then
                Return 0
            Else
                Return 1
            End If
        End Function




        'Private Delegate Function weightlessThanDelegate(byval s1 As WeightedArtist, byval s2 As WeightedArtist) As Boolean
        Private Function weightlessThan(ByVal s1 As WeightedArtist, ByVal s2 As WeightedArtist) As Integer
            If s1.weighting < s2.weighting Then
                Return -1
            ElseIf s1.weighting = s2.weighting Then
                Return 0
            Else
                Return 1
            End If
        End Function

        ''' <summary>
        ''' If the weighting of s1 is more than s2.weighting
        ''' </summary>
        ''' <param name="s1">The s1.</param>
        ''' <param name="s2">The s2.</param>
        ''' <returns>
        ''' If successful, a Int32 is returned, else <c>null</c>
        ''' </returns>
        Private Function weightMoreThan(ByVal s1 As WeightedArtist, ByVal s2 As WeightedArtist) As Integer
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
