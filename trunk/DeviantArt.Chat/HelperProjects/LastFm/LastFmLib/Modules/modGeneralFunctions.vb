Imports System.Text
Imports System.Security.Cryptography
Imports System.ComponentModel
Public Module GeneralFunctions
    Function getversionstring() As String
        Return My.Application.Info.Version.ToString
    End Function
    ''' <summary>
    ''' This Function is desgned to return the translation of "s" in the user's language.
    ''' It is NOT Implemented yet, so it only returns s
    ''' </summary>
    ''' <param name="s">The string to translate</param>
    ''' <returns>Function is NOT Implemented yet, so it only returns s</returns>
    Function tr(ByRef s As String) As String
        Return s
    End Function
    Public Function CovertStringEncoding(ByVal str As String, ByVal src As System.Text.Encoding, ByVal dest As System.Text.Encoding) As String
        Dim b() As Byte
        b = src.GetBytes(str)
        b = System.Text.Encoding.Convert(src, dest, b)
        Return dest.GetString(b)
    End Function
    Public Function ConvertToUTF8(ByVal str As String) As String
        Return ConvertStringEncoding(str, System.Text.Encoding.ASCII, System.Text.Encoding.UTF8)
    End Function
    Public Function ConvertStringEncoding(ByVal s As String, ByVal ConvertFrom As System.Text.Encoding, ByVal ConvertTo As System.Text.Encoding)
        If String.IsNullOrEmpty(s) Then Return ""
        Dim FromBytes() As Byte = ConvertFrom.GetBytes(s)
        Dim ToBytes() As Byte = Encoding.Convert(ConvertFrom, ConvertTo, FromBytes)
        Return New String(ConvertTo.GetChars(ToBytes))
    End Function
    Function GetRequestAuthCode(ByVal passwordMD5 As String, ByVal challenge As String) As String
        Return ConvertToUTF8(MD5Digest(passwordMD5 & challenge))
    End Function

    Public Function GetChallenge() As String
        Return UnixTime.GetUnixTime
    End Function
    Public Function MD5Digest(ByVal SourceText As String) As String
        'Create an encoding object to ensure the encoding standard for the source text
        Dim Ue As New ASCIIEncoding()
        'Retrieve a byte array based on the source text
        Dim ByteSourceText() As Byte = Ue.GetBytes(SourceText)
        'Instantiate an MD5 Provider object
        Dim Md5 As New MD5CryptoServiceProvider()
        'Compute the hash value from the source
        Dim ByteHash() As Byte = Md5.ComputeHash(ByteSourceText)
        'And convert it to String format for return
        Return ToHexString(ByteHash)

    End Function

    Private Function ToHexString(ByVal bytes() As Byte) As String
        'Documentation states timestamp must begin with 0x.
        Dim hexStr As String = ""
        Dim i As Integer

        For i = 0 To bytes.Length - 1
            'If it's a single digit, append a zero in front of it.
            If Hex(bytes(i)).Length = 1 Then
                hexStr &= 0 & Hex(bytes(i))
            Else
                hexStr = hexStr + Hex(bytes(i))
            End If
        Next i

        Return hexStr

    End Function
    Public Function GetUriFromString(ByVal s As String) As Uri
        If String.IsNullOrEmpty(s) Then Return Nothing
        If Uri.IsWellFormedUriString(s, UriKind.RelativeOrAbsolute) Then Return New Uri(s)
        Return Nothing
    End Function

    Public Function TopXTimeSpanToURiString(ByVal ts As WebRequests.TopXTimeSpan) As String
        Select Case ts
            Case WebRequests.TopXTimeSpan.None
                Return ""
            Case WebRequests.TopXTimeSpan.ThreeMonths
                Return "3months"
            Case WebRequests.TopXTimeSpan.SixMonths
                Return "6months"
            Case WebRequests.TopXTimeSpan.TwelveMonths
                Return "12months"
            Case Else
                Return ""
        End Select
    End Function


End Module

