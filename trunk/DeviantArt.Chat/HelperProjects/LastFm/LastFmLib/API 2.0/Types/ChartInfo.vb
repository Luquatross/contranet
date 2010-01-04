Namespace API20.Types
    ''' <summary>
    ''' Class for storing weekly chart data.
    ''' </summary>
    Public Class ChartInfo


        Protected m_from As UnixTime
        Protected m_to As UnixTime

        Property From() As UnixTime
            Get
                Return m_from
            End Get
            Set(ByVal value As UnixTime)
                m_from = value
            End Set
        End Property

        Property [To]() As UnixTime
            Get
                Return m_to
            End Get
            Set(ByVal value As UnixTime)
                m_to = value
            End Set
        End Property

        Sub New()

        End Sub
        Protected Sub New(ByVal e As Xml.XmlElement)
            Me.New()
            Dim f As Integer
            Integer.TryParse(e.GetAttribute("from"), f)
            If e.HasAttribute("from") Then
                m_from = New UnixTime(f, UnixTime.UnixTimeFormat.Seconds)
            End If
            Integer.TryParse(e.GetAttribute("to"), f)
            If e.HasAttribute("to") Then
                m_to = New UnixTime(f, UnixTime.UnixTimeFormat.Seconds)
            End If
        End Sub

        Public Shared Function FromXmlElement(ByVal elem As Xml.XmlElement) As ChartInfo
            Return New ChartInfo(elem)
        End Function

    End Class

End Namespace