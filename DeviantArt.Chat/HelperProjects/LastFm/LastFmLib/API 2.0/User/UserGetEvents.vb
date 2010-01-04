Imports LastFmLib.API20.Types
Namespace API20.User
    ''' <summary>
    ''' Get a list of events that this user is attending. Easily integratable into calendars, using the ical standard (see 'more formats' section below). 
    ''' </summary>
    Public Class UserGetEvents
        Inherits Base.BaseUserRequest

        Private m_TotalEvents As Integer

        Private m_Events As List(Of MusicEvent)
        Public ReadOnly Property Result() As List(Of MusicEvent)
            Get
                Return m_Events
            End Get
        End Property

        Public Property TotalEvents() As Integer
            Get
                Return m_TotalEvents
            End Get
            Set(ByVal value As Integer)
                m_TotalEvents = value
            End Set
        End Property


        Sub New(ByVal uname As String)
            MyBase.New(RequestType.UserGetEvents, uname)
        End Sub

        Protected Overrides Sub Success(ByVal elem As System.Xml.XmlElement)
            m_User = Util.GetAttrValue(elem, "user")
            m_TotalEvents = CInt(Util.GetAttrValue(elem, "total"))
            m_Events = New List(Of MusicEvent)
            For Each ev As Xml.XmlElement In elem
                m_Events.Add(MusicEvent.FromXmlElement(ev))
            Next
        End Sub
    End Class
End Namespace
