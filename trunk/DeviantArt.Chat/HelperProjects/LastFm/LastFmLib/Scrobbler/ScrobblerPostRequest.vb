Namespace Scrobbler
    Public Class ScrobblerPostRequest
        Inherits ScrobblerHttp
        Dim m_data As String
        Sub New()
            MyBase.New()
        End Sub
        Public Overrides Sub setUrl(ByVal url As System.Uri)
            If Not url Is Nothing Then
                m_path = url.AbsolutePath
                m_host = url.Host
                setHost(m_host, url.Port)
            End If
        End Sub

        Protected Overridable Sub request()

            MyBase.Post(m_path, m_data)

        End Sub
        Public Overridable Sub request(ByRef data As String)
            m_data = data
            request()
        End Sub
    End Class
End Namespace
