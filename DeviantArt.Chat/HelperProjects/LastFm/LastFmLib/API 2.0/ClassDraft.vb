Imports System.Xml
Imports LastFmLib.API20.Types
Namespace API20
    Public Class ClassDraft
        Inherits Base.BaseRequest



        Sub New()
            MyBase.New(RequestType.Unknown)
        End Sub

        Public Overloads Overrides Sub Start()

        End Sub

        Protected Overrides Sub Success(ByVal elem As System.Xml.XmlElement)

        End Sub
    End Class
End Namespace
