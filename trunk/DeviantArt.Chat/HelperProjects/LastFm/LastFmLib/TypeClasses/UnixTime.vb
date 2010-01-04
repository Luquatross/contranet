Public Class UnixTime

    Dim time As TimeSpan
    ''' <summary>
    ''' Bestimmt das Ein-/Ausgabeformat der Unixzeit
    ''' </summary>
    Public Enum UnixTimeFormat
        ''' <summary>
        ''' Millisekunden
        ''' </summary>
        Milliseconds = 0
        ''' <summary>
        ''' Sekunden(Standard)
        ''' </summary>
        Seconds
        ''' <summary>
        ''' Minuten
        ''' </summary>
        Minutes
        ''' <summary>
        ''' Stunden
        ''' </summary>
        Hours
        ''' <summary>
        ''' Tage
        ''' </summary>
        Days
    End Enum

    Property TimeSpan() As TimeSpan
        Get
            Return time
        End Get
        Set(ByVal value As TimeSpan)
            time = value
        End Set
    End Property
    ''' <summary>
    ''' Erstellt eine neue Instanz der <see cref="UnixTime" />-Klasse.
    ''' </summary>
    Public Sub New()
        Me.set(GetUnixTime(UnixTimeFormat.Milliseconds), UnixTimeFormat.Milliseconds)
    End Sub

    Public Sub New(ByVal t As Long, ByVal format As UnixTimeFormat)
        Me.set(t, format)
    End Sub

    ''' <summary>
    ''' Gibt die Unixzeit im Format <paramref name="format">format</paramref> zurück.
    ''' </summary>
    ''' <param name="format">Das Zeitformat</param>
    ''' <returns></returns>
    Public Function [get](Optional ByVal format As UnixTimeFormat = UnixTimeFormat.Seconds) As Long
        Return convertTimeSpanToUnixTime(time, format)
    End Function
    ''' <summary>
    ''' Setzt die Unixzeit innerhalb dieser Klasse.
    ''' </summary>
    ''' <param name="value">Ein Long-Wert, der eine Zahl passend zu <paramref name="format">format</paramref> enthält.</param>
    ''' <param name="format">Das Zeitformat des Parameters <paramref name="value">value</paramref>.</param>
    Public Sub [set](ByVal value As Long, Optional ByVal format As UnixTimeFormat = UnixTimeFormat.Seconds)
        time = ConvertValueToTimeSpan(value, format)
    End Sub

    ''' <summary>
    ''' Gibt die Unixzeit zurück, die bis zu einem bestimmten Datum vergangen ist.
    ''' </summary>
    ''' <param name="value">Der Zeitwert</param>
    ''' <param name="format">Das Zeitformat des Parameters <paramref name="value">value</paramref>.</param>
    ''' <returns></returns>
    Public Shared Function ConvertValueToTimeSpan(ByVal value As Long, Optional ByVal format As UnixTimeFormat = UnixTimeFormat.Seconds) As TimeSpan
        Dim d As New TimeSpan(0, 0, 0)
        Select Case format
            Case UnixTimeFormat.Milliseconds
                d = TimeSpan.FromMilliseconds(value)
            Case UnixTimeFormat.Seconds
                d = TimeSpan.FromSeconds(value)
            Case UnixTimeFormat.Minutes
                d = TimeSpan.FromMinutes(value)
            Case UnixTimeFormat.Hours
                d = TimeSpan.FromHours(value)
            Case UnixTimeFormat.Days
                d = TimeSpan.FromDays(value)
            Case Else
                Return Nothing
                Exit Function
        End Select
        Return d
    End Function
    ''' <summary>
    ''' Gibt die Unixzeit im Zeitformat <paramref name="format">format</paramref> zurück und weist diesen Zeitwert der Klasse zu.
    ''' </summary>
    ''' <returns>Die Zeitformatierte Unixzeit.</returns>
    Public Shared Function GetUnixTime(Optional ByVal format As UnixTimeFormat = UnixTimeFormat.Seconds) As Long
        Dim t As TimeSpan
        t = GetUnixTimeOfDate(Date.UtcNow)
        Return convertTimeSpanToUnixTime(t, format)
    End Function
    ''' <summary>
    ''' Gibt die Unixzeit zurück.
    ''' </summary>
    ''' <param name="d">The date of which the unix time is calculated.</param>
    ''' <returns>Eine TimeSpan, die die Unixzeit(die Zeit seit dem 1.1.1970 vergangen ist</returns>
    Public Shared Function GetUnixTimeOfDate(ByVal d As Date) As TimeSpan
        Return (d - New DateTime(1970, 1, 1))

    End Function

    Public Shared Function GetDateOfUnixTime(ByVal seconds As Integer) As DateTime
        Dim t As TimeSpan = TimeSpan.FromSeconds(seconds)
        Return New DateTime(1970, 1, 1).Add(t)
    End Function
    ''' <summary>
    ''' Konvertiert eine Zeitspanne in eine Zeitwert im Zeitformat <paramref name="format">format</paramref>.
    ''' </summary>
    ''' <param name="ts">Die zeitspanne</param>
    ''' <param name="format">Das Zeitformat</param>
    ''' <returns></returns>
    Private Shared Function convertTimeSpanToUnixTime(ByVal ts As TimeSpan, ByVal format As UnixTimeFormat) As Long

        Select Case format
            Case UnixTimeFormat.Milliseconds
                Return ts.TotalMilliseconds
            Case UnixTimeFormat.Seconds
                Return ts.TotalSeconds
            Case UnixTimeFormat.Minutes
                Return ts.TotalMinutes
            Case UnixTimeFormat.Hours
                Return ts.TotalHours
            Case UnixTimeFormat.Days
                Return ts.TotalDays
            Case Else
                Return Nothing
        End Select
    End Function
    Shared Widening Operator CType(ByVal u As UnixTime) As String
        Return u.get
    End Operator
    Shared Widening Operator CType(ByVal u As UnixTime) As Integer
        Return u.get
    End Operator
End Class
