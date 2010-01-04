Imports System.Drawing
''' <summary>
''' Thought I ported Trolltechs QMimeData Class. Get/SetData Methods aren't implemented yet.
''' </summary>
Public Class QMimeData
    
    Protected m_mimeparameters As New List(Of String)
    Protected m_contents As New Dictionary(Of String, Object)
    'String for storing the key for
    Protected m_imageString As String
    ReadOnly Property hasColor() As Boolean
        Get
            If m_contents.ContainsKey("application/x-color") Then
                Return TypeOf (m_contents("application/x-color")) Is Color
            Else
                Return False
            End If
        End Get
    End Property
    Property ColorData() As Color
        Get
            If hasColor Then
                Return m_contents("application/x-color")
            Else
                Return Nothing
            End If
        End Get
        Set(ByVal value As Color)
            If m_contents.ContainsKey("application/x-color") Then
                m_contents("application/x-color") = value
            Else
                m_contents.Add("application/x-color", value)
            End If

        End Set
    End Property
    ReadOnly Property hasHTML() As Boolean
        Get
            If m_contents.ContainsKey("text/html") Then
                Return IIf(TypeOf (m_contents("text/html")) Is String, True, False)
            Else
                Return False
            End If
        End Get
    End Property
    Property HTML() As String
        Get
            If hasHTML Then
                Return m_contents("text/html")
            End If
            Return Nothing
        End Get
        Set(ByVal value As String)
            If m_contents.ContainsKey("text/html") Then
                m_contents("text/html") = value
            Else
                m_contents.Add("text/html", value)
            End If

        End Set
    End Property
    ReadOnly Property hasUris() As Boolean
        Get
            If m_contents.ContainsKey("text/uri-list") Then
                Return IIf(TypeOf (m_contents("text/uri-list")) Is String, True, False)
            Else
                Return False
            End If
        End Get
        
    End Property
    Property URIs() As List(Of Uri)
        Get
            If hasUris Then
                Return m_contents("text/uri-list")
            End If
            Return Nothing
        End Get
        Set(ByVal value As List(Of Uri))
            If hasUris Then
                m_contents.Add("text/uri-list", value)
            Else
                m_contents("text/uri-list") = value
            End If
        End Set
    End Property
    ReadOnly Property hasText() As Boolean
        Get
            If m_contents.ContainsKey("text/plain") Then
                Return IIf(TypeOf (m_contents("text/plain")) Is String, True, False)
            Else
                Return False
            End If
        End Get
    End Property
    Property Text() As String
        Get
            If hasText Then Return m_contents("text/plain")
            Return Nothing
        End Get
        Set(ByVal value As String)
            If hasText Then
                m_contents.Add("text/plain", value)
            Else
                m_contents("text/plain") = value
            End If
        End Set
    End Property
    ReadOnly Property hasImage() As Boolean
        Get
            'this isn't done very well according to mime specs, but as the class stores only one image it satisfies our needs^^
            'Maybe have to rework it later on^^
            If m_contents.ContainsKey("image/") Then
                Return IIf(TypeOf (m_contents("image/")) Is Image, True, False)
            Else
                Return False
            End If
        End Get
    End Property
    Property ImageData() As Image
        Get
            If hasImage Then
                Return m_contents("image")
            Else
                Return Nothing
            End If
        End Get
        Set(ByVal value As Image)
            If hasImage Then
                m_contents.Add("image/", value)
            Else
                m_contents("image/") = value
            End If
        End Set
    End Property
    
    ReadOnly Property ParameterString() As String
        Get
            Dim mt As String = ""
            If m_mimeparameters.Count > 0 Then
                For Each s As String In m_mimeparameters
                    If Not mt.EndsWith(";") Then mt = mt & ";"
                    mt = mt & " " & s & ";"
                Next
            End If
            Return mt
        End Get
    End Property

    'Property Data() As Byte()
    '    Get
    '        If TypeOf (m_data) Is Array Then
    '            Return m_data
    '        Else
    '            Return Nothing
    '        End If
    '    End Get
    '    Set(ByVal value As Byte())
    '        m_data = value
    '    End Set
    'End Property
    Sub clear()
        m_contents = New Dictionary(Of String, Object)
    End Sub


    Public Function hasFormat(ByVal format As String) As Boolean
        Return m_contents.ContainsKey(format)
    End Function
End Class
