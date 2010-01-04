Namespace RSS
    Public Class Cloud
        Private m_domain As Uri
        Private m_port As Integer
        Private m_path As Uri
        Private m_registerProcedure As String
        Private m_protocol As String


        Property Protocol() As String
            Get
                Return m_protocol
            End Get
            Set(ByVal value As String)
                m_protocol = value
            End Set
        End Property

        Property RegisterProcedure() As String
            Get
                Return m_registerProcedure
            End Get
            Set(ByVal value As String)
                m_registerProcedure = value
            End Set
        End Property

        Property Path() As Uri
            Get
                Return m_path
            End Get
            Set(ByVal value As Uri)
                m_path = value
            End Set
        End Property

        Property Port() As Integer
            Get
                Return m_port
            End Get
            Set(ByVal value As Integer)
                m_port = value
            End Set
        End Property

        Property Domain() As Uri
            Get
                Return m_domain
            End Get
            Set(ByVal value As Uri)
                m_domain = value
            End Set
        End Property
        Sub New()

        End Sub

        Shared Function fromXmlElement(ByVal e As Xml.XmlElement) As Cloud
            Dim result As New Cloud
            If Not e.Name.ToLower = "cloud" Then
                Return Nothing
            End If

            If e.HasAttribute("domain") Then
                Dim uristring As String
                uristring = e.GetAttribute("domain")
                If Uri.IsWellFormedUriString(uristring, UriKind.RelativeOrAbsolute) Then
                    result.Domain = New Uri(uristring)
                End If
            End If
            If e.HasAttribute("port") Then
                result.Port = CInt(e.GetAttribute("port"))
            End If

            If e.HasAttribute("path") Then
                Dim uristring As String
                uristring = e.GetAttribute("path")
                If Uri.IsWellFormedUriString(uristring, UriKind.RelativeOrAbsolute) Then
                    result.Path = New Uri(uristring)
                End If
            End If
            If e.HasAttribute("protocol") Then
                result.Protocol = e.GetAttribute("protocol")
            End If
            If e.HasAttribute("registerProcedure") Then
                result.RegisterProcedure = e.GetAttribute("registerProcedure")
            End If
            Return result
        End Function
    End Class
End Namespace