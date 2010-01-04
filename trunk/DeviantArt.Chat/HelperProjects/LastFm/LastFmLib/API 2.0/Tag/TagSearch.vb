Namespace API20.Tag
    ''' <summary>
    ''' Search for a tag by name. Returns matches sorted by relevance. 
    ''' </summary>
    Public Class tagSearch
        Inherits Base.BaseRequest
        Dim m_result As Types.tagSearchResult
        Dim m_queryString As String

        Property QueryString() As String
            Get
                Return m_queryString
            End Get
            Set(ByVal value As String)
                m_queryString = value
            End Set
        End Property
        ReadOnly Property Result() As Types.tagSearchResult
            Get
                Return m_result
            End Get
        End Property
        Private Sub New()
            MyBase.New(RequestType.TagSearch)
            m_requiredParams.Add("tag")
        End Sub
        Sub New(ByVal queryString As String)
            Me.New()
            m_queryString = queryString
        End Sub
        Public Overrides Sub Start()
            SetAddParamValue("tag", m_queryString)
            MyBase.Start()
        End Sub
        Protected Overrides Sub Success(ByVal elem As System.Xml.XmlElement)
            m_result = Types.tagSearchResult.FromXmlElement(elem)
        End Sub
    End Class
End Namespace