Namespace API20.Types
    ''' <summary>
    ''' Class for storing user information, like name, url as image urls
    ''' </summary>
    Public Class UserInfo
        Inherits Base.BaseImageData
        Private m_Name As String
        Private m_url As Uri

        Public Property Url() As Uri
            Get
                Return m_url
            End Get
            Set(ByVal value As Uri)
                m_url = value
            End Set
        End Property

        Public Property Name() As String
            Get
                Return m_Name
            End Get
            Set(ByVal value As String)
                m_Name = value
            End Set
        End Property

        Sub New(ByVal uname As String, ByVal url As Uri)
            m_url = url
            m_Name = uname
        End Sub
        Sub New(ByVal e As Xml.XmlElement)
            m_Name = Util.GetSubElementValue(e, "name")
            m_url = Util.GetUrl(Util.GetSubElementValue(e, "url"))
            SetImagesByXmlElem(e)
        End Sub

        Overridable Function ToDebugString() As String
            Dim result As String = ""
            result &= "Username: " & m_Name & vbCrLf
            result &= "Url: " & If(m_url IsNot Nothing, m_url.AbsoluteUri, "") & vbCrLf
            Return result
        End Function

    End Class
End Namespace
