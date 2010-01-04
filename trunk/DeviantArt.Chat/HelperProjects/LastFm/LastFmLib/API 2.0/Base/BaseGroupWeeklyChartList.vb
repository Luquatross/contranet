Imports LastFmLib.API20.Types
Imports System.Xml
Namespace API20.Base
    ''' <summary>
    '''Base class for weekly group charts
    ''' </summary>
    Public MustInherit Class BaseGroupWeeklyChart
        Inherits Base.BaseRequest

        Private m_group As String
        Public Property Group() As String
            Get
                Return m_group
            End Get
            Set(ByVal value As String)
                m_group = value
            End Set
        End Property

        Protected m_ChartInfo As ChartInfo

        Public Property ChartInfo() As ChartInfo
            Get
                Return m_ChartInfo
            End Get
            Set(ByVal value As ChartInfo)
                m_ChartInfo = value
            End Set
        End Property
        Sub New(ByVal type As RequestType, ByVal group As String, Optional ByVal cInfo As ChartInfo = Nothing)
            MyBase.New(type, group)
            m_group = group
            m_ChartInfo = cInfo
        End Sub
        Public Overrides Sub Start()
            SetAddParamValue("group", m_group)
            If m_ChartInfo IsNot Nothing Then
                If CInt(m_ChartInfo.From) > 0 Then
                    SetAddParamValue("from", m_ChartInfo.From.get)
                End If
                If CInt(m_ChartInfo.To) > 0 Then
                    SetAddParamValue("to", m_ChartInfo.To.get)
                End If
            End If
            MyBase.Start()
        End Sub
        Protected MustOverride Overrides Sub Success(ByVal elem As System.Xml.XmlElement)

    End Class
End Namespace
