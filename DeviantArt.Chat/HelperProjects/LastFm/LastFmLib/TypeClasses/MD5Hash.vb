Imports System.Text
Imports System.Security.Cryptography

Public Class MD5Hash
    Protected m_hash As String
    
    Property HexString() As String
        Get
            Return m_hash
        End Get
        Set(ByVal value As String)
            If m_hash.Length <> 32 Then
                Throw New InvalidOperationException("'value' must be 32 chars long!")
            Else
                m_hash = value
            End If

        End Set
    End Property
    Sub New()

    End Sub
    Sub New(ByVal hash As MD5Hash)
        m_hash = hash.ToString
    End Sub
    Sub New(ByVal s As String, Optional ByVal isEncrypted As Boolean = True)
        If Not isEncrypted Then
            s = GetHash(s)
        End If
        m_hash = s
    End Sub
    Public Shared Function FromString(ByVal s As String) As MD5Hash
        Return New MD5Hash(s)
    End Function

    Public Shared Function GetHash(ByVal SourceText As String) As String
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

    Private Shared Function ToHexString(ByVal bytes() As Byte) As String
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

        Return hexStr.ToLower
    End Function
    Public Overrides Function ToString() As String
        Return m_hash
    End Function
    Public Shared Narrowing Operator CType(ByVal hash As MD5Hash) As String
        Return hash.ToString
    End Operator

End Class
