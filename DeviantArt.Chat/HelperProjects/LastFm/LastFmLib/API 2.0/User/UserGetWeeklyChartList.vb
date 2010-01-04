Imports System.Xml
Imports LastFmLib.API20.Types
Namespace API20.User
    ''' <summary>
    ''' Get a list of available charts for this user, expressed as date ranges which can be sent to the chart services. 
    ''' </summary>
    Public Class UserGetWeeklyChartList
        Inherits Base.BaseUserRequest



        Private m_result As UserWeeklyChartList

        Public Property Result() As UserWeeklyChartList
            Get
                Return m_result
            End Get
            Set(ByVal value As UserWeeklyChartList)
                m_result = value
            End Set
        End Property




        Sub New(ByVal user As String)
            MyBase.New(RequestType.UserGetWeeklyChartList, user)
            m_User = user
        End Sub

        Public Overloads Overrides Sub Start()
            MyBase.Start()
        End Sub

        Protected Overrides Sub Success(ByVal elem As System.Xml.XmlElement)
            m_result = UserWeeklyChartList.FromXmlElement(elem)
        End Sub
    End Class
End Namespace
