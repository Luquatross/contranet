Namespace API20.Types
    ''' <summary>
    ''' Weekly chart list for a group
    ''' </summary>
    Public Class GroupWeeklyChartList
        Inherits Base.BaseWeeklyChartList
        Property Group() As String
            Get
                Return m_attr
            End Get
            Set(ByVal value As String)
                m_attr = value
            End Set
        End Property
        Sub New()

        End Sub
        Overrides Function ToDebugString() As String
            Dim result As String = ""
            result &= "Group: " & Group & vbCrLf
            result &= MyBase.ToDebugString
            Return result
        End Function
        Private Sub New(ByVal e As Xml.XmlElement)
            MyBase.New(e, "group")
        End Sub
        Shared Function FromXmlElement(ByVal e As Xml.XmlElement) As GroupWeeklyChartList
            Return New GroupWeeklyChartList(e)
        End Function
    End Class
End Namespace