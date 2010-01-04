Imports System.Xml
Imports LastFmLib.API20.Types
Namespace API20.Groups
    ''' <summary>
    ''' Get a list of available charts for this group, expressed as date ranges which can be sent to the chart services. 
    ''' </summary>
    Public Class GroupGetWeeklyChartList
        Inherits Base.BaseRequest


        Private m_group As String
        Private m_result As GroupWeeklyChartList

        Public ReadOnly Property Result() As GroupWeeklyChartList
            Get
                Return m_result
            End Get
        End Property

        Public Property Group() As String
            Get
                Return m_group
            End Get
            Set(ByVal value As String)
                m_group = value
            End Set
        End Property



        Sub New(ByVal group As String)
            MyBase.New(RequestType.GroupGetWeeklyChartList)
            m_group = group
        End Sub

        Public Overloads Overrides Sub Start()
            SetAddParamValue("group", m_group)
            MyBase.Start()
        End Sub

        Protected Overrides Sub Success(ByVal elem As System.Xml.XmlElement)
            m_result = GroupWeeklyChartList.FromXmlElement(elem)
        End Sub
    End Class
End Namespace
