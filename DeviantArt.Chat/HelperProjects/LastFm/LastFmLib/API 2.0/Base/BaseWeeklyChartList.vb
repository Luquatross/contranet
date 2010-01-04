Imports System.Xml
Imports LastFmLib.API20.Types
Namespace API20.Base
    Public MustInherit Class BaseWeeklyChartList
        Protected m_attr As String
        Protected m_charts As List(Of ChartInfo)

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Charts() As List(Of ChartInfo)
            Get
                Return m_charts
            End Get
            Set(ByVal value As List(Of ChartInfo))
                m_charts = value
            End Set
        End Property


        Protected Sub New()

        End Sub
        Protected Sub New(ByVal e As XmlElement, Optional ByVal attrname As String = "")
            If Not String.IsNullOrEmpty(attrname) And e.HasAttribute(attrname) Then m_attr = e.GetAttribute(attrname)
            Dim l As XmlNodeList = e.SelectNodes("chart")
            m_charts = New List(Of ChartInfo)
            For Each n As XmlElement In l
                m_charts.Add(ChartInfo.FromXmlElement(n))
            Next
        End Sub
        Overridable Function ToDebugString() As String
            Dim result As String = ""
            For Each c As ChartInfo In m_charts
                result &= "From " & CStr(c.From) & " to " & CStr(c.To) & vbCrLf
            Next
            Return result
        End Function
    End Class
End Namespace