Namespace TypeClasses
    ''' <summary>
    ''' Type for station urls("lastfm://.....")
    ''' </summary>
    Public Class StationUrl
        Dim value As String
        ''' <summary>
        ''' Returns whether the station url is a playlist or not.
        ''' </summary>
        ''' <value>The is playlist.</value>
        ReadOnly Property isPlaylist() As Boolean
            Get
                Return value.StartsWith("lastfm://play/") Or _
                        value.StartsWith("lastfm://preview/") Or _
                        value.StartsWith("lastfm://track/") Or _
                        value.StartsWith("lastfm://playlist/")
            End Get
        End Property

        Sub New()

        End Sub
        ''' <summary>
        ''' Initializes a new instance of the <see cref="StationUrl" /> class.
        ''' </summary>
        ''' <param name="url">The station url.</param>
        Sub New(ByRef url As String)
            value = url
        End Sub
        ''' <summary>
        ''' Initializes a new instance of the <see cref="StationUrl" /> class.
        ''' </summary>
        ''' <param name="url">The station URL.</param>
        Sub New(ByRef url As Uri)
            value = url.AbsoluteUri
        End Sub

        ''' <summary>
        ''' Implements the operator CType for converting an url to a station url.
        ''' </summary>
        ''' <param name="url">The URL.</param>
        ''' <returns>The result of the operator.</returns>
        Shared Widening Operator CType(ByVal url As String) As StationUrl
            Return New StationUrl(url)
        End Operator
        ''' <summary>
        ''' Implements the operator CType for converting a StationUrl
        ''' </summary>
        ''' <param name="su">The su.</param>
        ''' <returns>The result of the operator.</returns>
        Shared Widening Operator CType(ByVal su As StationUrl) As String
            Return su.value
        End Operator
        ''' <summary>
        ''' Converts a station url to an url-string.
        ''' </summary>
        ''' <param name="su">The station url instance.</param>
        ''' <returns></returns>
        Public Overloads Shared Function ToString(ByVal su As StationUrl) As String
            Return su.value
        End Function
        ''' <summary>
        ''' Converts the current instance into a station url string
        ''' </summary>
        ''' <returns>A string containing the station url.</returns>
        ''' <remarks></remarks>
        Public Overrides Function ToString() As String
            Return value
        End Function
    End Class
End Namespace
