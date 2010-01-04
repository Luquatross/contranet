Imports System.Security.Cryptography
Imports System.Text
Public Module modMD5Hash
    Public Function GenerateHash(ByVal SourceText As String) As String
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
End Module
