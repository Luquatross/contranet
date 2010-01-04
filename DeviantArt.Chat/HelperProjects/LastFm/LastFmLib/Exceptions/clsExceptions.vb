Namespace Exceptions
    Public Class LastFmException
        Inherits System.Exception
        'ReadOnly Property what() As String
        '    Get
        '        Return m_what
        '    End Get
        'End Property
        'ReadOnly Property tr_what() As String
        '    Get
        '        Return m_what
        '    End Get
        'End Property
        Sub New()

        End Sub
        Sub New(ByRef msg As String)
            MyBase.New(msg)
        End Sub
        Sub New(ByVal msg As String, ByVal innerEx As Exception)
            MyBase.New(msg, innerEx)
        End Sub
    End Class
    Public Class ConnectionException
        Inherits LastFmException
        Sub New(ByVal msg As String)
            MyBase.New(msg)
        End Sub
    End Class
    Public Class NetworkException
        Inherits LastFmException
        Sub New(ByVal msg As String)
            MyBase.New(msg)
        End Sub
    End Class
    Public Class BadClientException
        Inherits LastFmException
        Sub New(ByVal msg As String)
            MyBase.New(msg)
        End Sub
    End Class
    Class ParseException
        Inherits LastFmException
        Sub New(ByVal msg As String)
            MyBase.New(msg)
        End Sub
        Sub New(ByVal msg As String, ByVal innerexception As Exception)
            MyBase.New(msg, innerexception)
        End Sub
    End Class
    Public Class BadCommandException
        Inherits LastFmException
        Sub New(ByVal msg As String)
            MyBase.New(msg)
        End Sub
    End Class
    Class RadioException
        Inherits LastFmException
        Sub New(ByVal msg As String)
            MyBase.New(msg)
        End Sub
    End Class
    Public Class PlaylistException
        Inherits LastFmException
        Sub New()
            MyBase.New()
        End Sub
        Sub New(ByVal message As String)
            MyBase.New(message)
        End Sub
        Sub New(ByVal message As String, ByVal innerException As Exception)
            MyBase.New(message, innerException)
        End Sub
    End Class


End Namespace
