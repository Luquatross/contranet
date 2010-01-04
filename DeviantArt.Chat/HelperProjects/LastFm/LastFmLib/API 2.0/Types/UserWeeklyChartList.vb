Namespace API20.Types
    Public Class UserWeeklyChartList
        Inherits Base.BaseWeeklyChartList
        Property User() As String
            Get
                Return m_attr
            End Get
            Set(ByVal value As String)
                M_attr = value
            End Set
        End Property
        Sub New()

        End Sub

        ''' <summary>
        ''' Initializes a new instance of the <see cref="UserWeeklyChartList" /> class.
        ''' </summary>
        ''' <param name="e">The e.</param>
        Private Sub New(ByVal e As Xml.XmlElement)
            MyBase.New(e, "User")
        End Sub
        ''' <summary>
        ''' Creates an instance from an xml element
        ''' </summary>
        ''' <param name="e">The e.</param>
        ''' <returns>
        ''' If successful, a UserWeeklyChartList is returned, else <c>null</c>
        ''' </returns>
        Shared Function FromXmlElement(ByVal e As Xml.XmlElement) As UserWeeklyChartList
            Return New UserWeeklyChartList(e)
        End Function

        Overrides Function ToDebugString() As String
            Dim result As String = "User: " & User & vbCrLf
            result = result & MyBase.ToDebugString
            Return result
        End Function
    End Class
End Namespace