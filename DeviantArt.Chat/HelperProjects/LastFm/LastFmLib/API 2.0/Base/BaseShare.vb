Namespace API20.Base
    ''' <summary>
    ''' Base class for [class].share requests
    ''' </summary>
    ''' <remarks></remarks>
    Public MustInherit Class BaseShare
        Inherits BaseRequest

        Protected m_recipients As New List(Of String)
        Protected m_message As String
        ''' <summary>
        ''' An optional message to send with the recommendation. If not supplied a default message will be used.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property Message() As String
            Get
                Return m_message
            End Get
            Set(ByVal value As String)
                m_message = value
            End Set
        End Property
        ''' <summary>
        ''' Email Address | Last.fm Username - A cstring list of email addresses or Last.fm usernames. Maximum is 10.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Property Recipients() As List(Of String)
            Get
                Return m_recipients
            End Get
            Set(ByVal value As List(Of String))
                m_recipients = value
            End Set
        End Property

        Protected Sub New(ByVal type As RequestType, ByVal recipients As List(Of String), ByVal msg As String)
            MyBase.New(type)
            m_accessMode = RequestAccessMode.Write
            m_recipients = recipients
            m_message = msg
        End Sub
        Public Overrides Sub Start()
            If m_recipients.Count = 0 Or m_recipients.Count > 10 Then
                setFailed(modEnums.FailureCode.InvalidParameters, "Number of recipients is 0 or greater than 10!")
                Exit Sub
            Else
                Dim rString As String = String.Join(",", m_recipients.ToArray)
                SetAddParamValue("recipient", rString)
            End If
            MyBase.Start()
        End Sub
        Protected NotOverridable Overrides Sub Success(ByVal elem As System.Xml.XmlElement)
            'Won't ever get here
        End Sub
    End Class
End Namespace
