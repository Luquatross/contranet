Namespace Radio
    Class Enums
        Enum RadioState

            State_Uninitialised = 0
            State_Handshaking
            State_Handshaken
            State_ChangingStation
            State_FetchingPlaylist
            State_FetchingStream  ''* it's like requesting the start of the streaming */
            State_StreamFetched   ''* server responded ok  start buffering */
            State_Buffering
            State_Streaming
            State_Skipping
            State_Stopping
            State_Stopped
        End Enum

        ' This enum extends the WebRequestResult enum so that we can reuse
        ' the error codes in our error signal.
        Enum RadioError

            Radio_BadPlaylist = 83479 'WebRequestResult_Custom + 1
            Radio_InvalidUrl
            Radio_InvalidAuth
            Radio_TooManyRetries
            Radio_TrackNotFound
            Radio_SkipLimitExceeded
            Radio_IllegalResume
            Radio_OutOfPlaylist
            Radio_PluginLoadFailed
            Radio_NoSoundcard
            Radio_PlaybackError
            Radio_ConnectionRefused
            Radio_UnknownError
        End Enum
    End Class
End Namespace