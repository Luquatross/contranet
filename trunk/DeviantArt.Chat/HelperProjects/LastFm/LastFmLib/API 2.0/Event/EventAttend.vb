Imports System.Xml
Imports LastFmLib.API20.Types
Namespace API20.Events
    ''' <summary>
    ''' Set a user's attendance status for an event. 
    ''' </summary>
    Public Class EventAttend
        Inherits Base.BaseRequest

        Public Overrides ReadOnly Property RequiresAuth() As Boolean
            Get
                Return True
            End Get
        End Property
        Private m_event As Integer

        Private m_status As EventAttendanceStatus
        Public Property AttendaceStatus() As EventAttendanceStatus
            Get
                Return m_status
            End Get
            Set(ByVal value As EventAttendanceStatus)
                m_status = value
            End Set
        End Property

        Public Property EventId() As Integer
            Get
                Return m_event
            End Get
            Set(ByVal value As Integer)
                m_event = value
            End Set
        End Property

        Sub New(ByVal eventId As Integer, ByVal status As EventAttendanceStatus)
            MyBase.New(RequestType.EventAttend)
            m_accessMode = RequestAccessMode.Write
            m_event = eventId
            m_status = status
            m_requiredParams.Add("event")
            m_requiredParams.Add("status")
        End Sub

        Public Overloads Overrides Sub Start()
            SetAddParamValue("event", CStr(EventId))
            SetAddParamValue("status", CStr(CInt(m_status)))
            MyBase.Start()
        End Sub


        Protected Overrides Sub Success(ByVal elem As System.Xml.XmlElement)
            'Won't ever get here
        End Sub
    End Class
End Namespace
