Namespace TypeClasses
    Public Class Station
        Private m_name As String
        Private m_stationUrl As StationUrl

        Public Property Name() As String
            Get
                Return IIf(String.IsNullOrEmpty(m_name), m_stationUrl, m_name)
            End Get
            Set(ByVal Value As String)
                m_name = Value
            End Set
        End Property

        Public Property Url() As StationUrl
            Get
                Return m_stationUrl
            End Get
            Set(ByVal Value As StationUrl)
                m_stationUrl = Value
            End Set
        End Property

        Sub New()

        End Sub
        Sub New(ByVal url As StationUrl, ByVal name As String)
            m_name = name
            m_stationUrl = url
        End Sub

        Function ToXmlElement(ByRef doc As Xml.XmlDocument) As Xml.XmlElement
            Dim e As Xml.XmlElement = doc.CreateElement("station")
            e.SetAttribute("name", m_name)
            e.SetAttribute("url", m_stationUrl.ToString())
            Return e
        End Function
    End Class
End Namespace