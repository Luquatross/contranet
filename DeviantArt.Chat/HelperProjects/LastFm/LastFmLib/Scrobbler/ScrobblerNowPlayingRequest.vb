Namespace Scrobbler
    Public Class ScrobblerNowPlayingRequest
        Inherits ScrobblerPostRequest
        Sub New()
            MyBase.new()
            m_retry_timer.Interval = 5000

        End Sub
        Protected Overrides Sub request()
            MyBase.request()

        End Sub
    End Class
End Namespace
