Imports System.Xml
Namespace API20.Events
    ''' <summary>
    ''' Get the metadata for an event on Last.fm. Includes attendance and lineup information. 
    ''' </summary>
    Public Class EventGetInfo
        Inherits Base.BaseRequest
        Dim m_id As Integer
        Dim m_event As Types.MusicEvent
        Property Id() As Integer
            Get
                Return m_id
            End Get
            Set(ByVal value As Integer)
                m_id = value
            End Set
        End Property
        ReadOnly Property Result() As Types.MusicEvent
            Get
                Return m_event
            End Get
        End Property

        Sub New(ByVal eventId As Integer)
            MyBase.New(RequestType.EventGetInfo)
            m_id = eventId
        End Sub

        Public Overloads Overrides Sub Start()
            SetAddParamValue("event", CStr(m_id))
            MyBase.Start()
        End Sub

        Protected Overrides Sub Success(ByVal elem As System.Xml.XmlElement)
            m_event = Types.MusicEvent.FromXmlElement(elem)
        End Sub
    End Class
End Namespace
