Namespace API20.Tag
    ''' <summary>
    ''' Search for tags similar to this one. Returns tags ranked by similarity, based on listening data. 
    ''' </summary>
    Public Class tagGetSimilar
        Inherits Base.BaseTagRequest

        Dim m_limit As Integer
        Dim m_result As List(Of Types.TagInfo)

        Property Result() As List(Of Types.TagInfo)
            Get
                Return m_result
            End Get
            Set(ByVal value As List(Of Types.TagInfo))
                m_result = value
            End Set
        End Property
        Property Limit() As Integer
            Get
                Return m_limit
            End Get
            Set(ByVal value As Integer)
                m_limit = value
            End Set
        End Property



        Sub New(ByVal tag As String, Optional ByVal limit As Integer = 0)
            MyBase.New(RequestType.TagGetSimilar)
            m_Tag = tag
            m_limit = limit


        End Sub
        Public Overrides Sub Start()

            If Limit > 0 Then SetAddParamValue("limit", m_limit)
            MyBase.Start()
        End Sub
        Protected Overrides Sub Success(ByVal elem As System.Xml.XmlElement)
            'We will get a minimal tag, but the object will contain all his/her similar ones, too
            m_Tag = If(elem.HasAttribute("tag"), elem.GetAttribute("tag"), Nothing)
            m_result = New List(Of Types.TagInfo)
            For Each tn As Xml.XmlElement In elem.SelectNodes("tag")
                m_result.Add(Types.TagInfo.FromXmlElement(tn))
            Next
        End Sub
    End Class
End Namespace