Namespace API20.Types
    ''' <summary>
    ''' Class for storing data from xxxTopFans requests
    ''' </summary>
    Public Class TopFan
        Inherits Base.BaseImageData
        Dim m_name As String
        Dim m_url As Uri
        Dim m_weighting As Integer

        Property Weighting() As Integer
            Get
                Return m_weighting
            End Get
            Set(ByVal value As Integer)
                m_weighting = value
            End Set
        End Property
        Property Url() As Uri
            Get
                Return m_url
            End Get
            Set(ByVal value As Uri)
                m_url = value
            End Set
        End Property
        Property Name() As String
            Get
                Return m_name
            End Get
            Set(ByVal value As String)
                m_name = value
            End Set
        End Property
        Sub New()
            MyBase.New()
        End Sub
        Private Sub New(ByVal e As Xml.XmlElement)
            Me.New()
            Me.SetImagesByXmlElem(e)
            Name = Util.GetSubElementValue(e, "name")
            Uri.TryCreate(Util.GetSubElementValue(e, "url"), UriKind.RelativeOrAbsolute, Url)
            Integer.TryParse(Util.GetSubElementValue(e, "weight"), m_weighting)
        End Sub
        Public Function ToDebugString() As String
            Dim result As String = ""
            result &= "Name: " & m_name & vbCrLf
            result &= "Url: " & m_url.AbsoluteUri & vbCrLf
            result &= "Weighting: " & m_weighting & vbCrLf
            result &= "Image url: " & m_imageMedium.AbsoluteUri & vbCrLf
            Return result
        End Function
        Shared Function FromXmlElemnt(ByVal e As Xml.XmlElement) As TopFan
            Dim result As New TopFan(e)
            Return result
        End Function
    End Class
End Namespace
