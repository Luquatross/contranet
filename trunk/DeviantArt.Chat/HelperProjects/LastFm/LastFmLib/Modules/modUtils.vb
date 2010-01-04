Namespace Utils
    Public Module Utils
        Public Function StripBBCode(ByVal str As String) As String
            Dim nIdxNext As Integer = 0
            Dim nIdxStart As Integer
            If Not str.Contains("[") Then Return str
            While (nIdxNext < str.Length)
                nIdxStart = str.IndexOf("[", nIdxNext)

                If (nIdxStart = -1) Then
                    Return str
                End If

                nIdxStart += 1

                If nIdxStart >= str.Length Then
                    Return Nothing
                End If

                Dim nIdxStop As Integer = str.IndexOf("]", nIdxStart)
                If nIdxStop = -1 Then
                    Return Nothing
                End If
                Dim numRemove As Integer = nIdxStop - nIdxStart + 2
                str = str.Remove(nIdxStart - 1, numRemove)
                nIdxNext = nIdxStop + 1 - numRemove
            End While
            Return str
        End Function
        Public Function UrlEncodeItem(ByVal item As String) As String
            'item = UrlEncodeSpecialChars(item)
            item = Uri.EscapeDataString(Uri.EscapeDataString(item))
            Return item
        End Function

        Private Function UrlEncodeSpecialChars(ByVal str As String) As String
            str = str.Replace("&", "%26")
            str = str.Replace("/", "%2F")
            str = str.Replace(";", "%3B")
            str = str.Replace("+", "%2B")
            str = str.Replace("#", "%23")

            Return str
        End Function

        Public Function CloneUrl(ByVal url As Uri) As Uri
            If url Is Nothing Then Return Nothing
            Return New Uri(url.AbsoluteUri)
        End Function
        Public Function CloneList(Of T)(ByVal l As List(Of T)) As List(Of T)
            If l Is Nothing Then Return New List(Of T)
            If l.Count = 0 Then Return New List(Of T)
            Dim arr() As T = l.ToArray
            Return New List(Of T)(arr)
        End Function

    End Module
End Namespace
