Imports System.Xml
Namespace API20.Base
    Public MustInherit Class BaseUserGetTopX
        Inherits Base.BaseUserRequest

        Protected m_period As ChartPeriod
        Property Period() As ChartPeriod
            Get
                Return m_period
            End Get
            Set(ByVal value As ChartPeriod)
                m_period = value
            End Set
        End Property

        Protected Sub New(ByVal type As RequestType, Optional ByVal user As String = "", Optional ByVal p As ChartPeriod = ChartPeriod.Overall)
            MyBase.New(type, user)
            m_period = p
        End Sub
        Public Overrides Sub Start()
            SetAddParamValue("artist", m_User)
            SetAddParamValue("period", PeriodToString(m_period))
            MyBase.Start()
        End Sub
        

        Protected Function PeriodToString(ByVal p As ChartPeriod) As String
            Select Case p
                Case ChartPeriod.ThreeMonths
                    Return "3month"
                Case ChartPeriod.SixMonths
                    Return "6month"
                Case ChartPeriod.TwelveMonths
                    Return "12month"
                Case Else
                    Return "overall"
            End Select
        End Function


    End Class
End Namespace