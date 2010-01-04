Imports System.Xml

Namespace API20.Types
    ''' <summary>
    ''' Information on Tags, like name, count and url
    ''' </summary>
    Public Class TagInfo

        Private m_name As String
        Private m_count As Integer
        Private m_url As Uri

        Private m_streamable As Boolean
        Public Property IsStreamable() As Boolean
            Get
                Return m_streamable
            End Get
            Set(ByVal value As Boolean)
                m_streamable = value
            End Set
        End Property

        Public Property Url() As Uri
            Get
                Return m_url
            End Get
            Set(ByVal value As Uri)
                m_url = value
            End Set
        End Property

        Public Property Count() As Integer
            Get
                Return m_count
            End Get
            Set(ByVal value As Integer)
                m_count = value
            End Set
        End Property

        Public Property Name() As String
            Get
                Return m_name
            End Get
            Set(ByVal value As String)
                m_name = value
            End Set
        End Property
        Sub New()

        End Sub
        Function ToDebugString() As String
            Dim result As String = ""
            result &= "Tag: " & Name & vbCrLf
            result &= "Url: " & If(Url IsNot Nothing, Url.AbsoluteUri, "") & vbCrLf
            result &= "Count: " & Count & vbCrLf
            result &= "Streamable" & IsStreamable.ToString & vbCrLf
            Return result
        End Function
        Private Sub New(ByVal e As Xml.XmlElement)
            m_name = Util.GetSubElementValue(e, "name")
            Integer.TryParse(Util.GetSubElementValue(e, "tagcount"), m_count)

            Util.GetUrl(Util.GetSubElementValue(e, "tagcount"))
            Dim s As String = Util.GetSubElementValue(e, "Streamable")
            m_streamable = CBool(If(String.IsNullOrEmpty(s), "0", s))
        End Sub
        Shared Function FromXmlElement(ByVal e As XmlElement) As TagInfo
            Return New TagInfo(e)
        End Function
    End Class
End Namespace
