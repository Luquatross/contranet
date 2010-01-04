Imports LastFmLib.API20.Types
Imports System.Xml
Namespace API20.Base
    ''' <summary>
    ''' Get a track chart for a user profile, for a given date range.
    ''' If no date range is supplied, it will return the most recent track chart for this user. 
    ''' </summary>
    Public MustInherit Class BaseUserWeeklyChart
        Inherits Base.BaseUserRequest

        Protected m_ChartInfo As ChartInfo

        Public Property ChartInfo() As ChartInfo
            Get
                Return m_ChartInfo
            End Get
            Set(ByVal value As ChartInfo)
                m_ChartInfo = value
            End Set
        End Property
        Sub New(ByVal type As RequestType, ByVal sUser As String, Optional ByVal cInfo As ChartInfo = Nothing)
            MyBase.New(type, sUser)
            m_ChartInfo = cInfo
        End Sub
        Public Overrides Sub Start()
            'user set by base class
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
