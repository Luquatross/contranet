Namespace RSS
    Public Class XCalEventData
        Private m_start As DateTime
        Private m_end As DateTime
        Private m_location As String

        ''' <summary>
        ''' Gets or sets the start date.
        ''' </summary>
        ''' <value>The start date.</value>
        Property StartDate() As DateTime
            Get
                Return m_start
            End Get
            Set(ByVal value As DateTime)
                m_start = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the end date.
        ''' </summary>
        ''' <value>The end date.</value>
        Property EndDate() As DateTime
            Get
                Return m_end
            End Get
            Set(ByVal value As DateTime)
                m_end = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the location.
        ''' </summary>
        ''' <value>The location.</value>
        Property Location() As String
            Get
                Return m_location
            End Get
            Set(ByVal value As String)
                m_location = value
            End Set
        End Property

        ''' <summary>
        ''' Gets the location URL if the Location-property contains a valid Url.
        ''' </summary>
        ''' <value>The location URL.</value>
        ''' <returns>An Uri if succesful, else nothing.</returns>
        ReadOnly Property LocationUrl() As Uri
            Get
                If Uri.IsWellFormedUriString(m_location, UriKind.RelativeOrAbsolute) Then
                    Return New Uri(m_location)
                Else
                    Return Nothing
                End If
            End Get
        End Property

        'Public Shared Function FromParentNode(ByVal parentNode As Xml.XmlElement) As XCalEventData
        '    Dim result As New XCalEventData

        '    Dim ns As New Xml.XmlNamespaceManager(parentNode.OwnerDocument.NameTable)
        '    Dim xcalnodes As Xml.XmlNodeList = parentNode.SelectNodes("/[starts-with(/,'xcal')]")
        '    For Each e As Xml.XmlElement In xcalnodes
        '        result.SetPropertyByXmlElement(e)
        '    Next
        '    Return result
        'End Function
        Public Function SetPropertyByXmlElement(ByVal e As Xml.XmlElement) As Boolean
            Debug.Print(e.LocalName)
            Select Case e.LocalName
                Case "dtstart"
                    m_start = GetDateTimeFromString(e.InnerText)
                Case "dtend"
                    m_end = GetDateTimeFromString(e.InnerText)
                Case "location"
                    m_location = e.InnerText
                Case Else
                    Return False
            End Select
        End Function
        Private Function GetDateTimeFromString(ByVal s As String) As DateTime
            Dim result As DateTime
            If DateTime.TryParse(s, result) Then
                Return result
            Else
                Return Nothing
            End If
            'DateTime.TryParse()
        End Function
    End Class
End Namespace