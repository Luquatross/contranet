Imports System.Xml
Imports LastFmLib.API20.Types
Namespace API20.User
    Public Class UserGetPlaylists
        Inherits Base.BaseUserRequest



        Sub New(ByVal uname As String)
            MyBase.New(RequestType.UserGetPlaylists, uname)
        End Sub

        Public Overloads Overrides Sub Start()
            Throw New NotImplementedException()
        End Sub

        Protected Overrides Sub Success(ByVal elem As System.Xml.XmlElement)

        End Sub
    End Class
End Namespace

