Namespace Constants
    Public Module Constants
        ' SCROBBLING CONSTANTS

        ' The plugin ID used by HttpInput when submitting radio tracks to the player listener
        Public Const kRadioPluginId As String = "radio"

        ' Limits for user-configurable scrobble point (%)
        Public Const kScrobblePointMin As Integer = 50
        Public Const kScrobblePointMax As Integer = 100

        ' Shortest track length allowed to scrobble (s)
        Public Const kScrobbleMinLength As Integer = 31

        ' Upper limit for scrobble time (s)
        Public Const kScrobbleTimeMax As Integer = 240

        ' Min size of buffer holding streamed http data, i.e the size the http
        ' buffer needs to get to before we start streaming.
        Public Const kHttpBufferMinSize As Integer = 16 * 1024

        ' Max
        Public Const kHttpBufferMaxSize As Integer = 256 * 1024
    End Module
End Namespace
Namespace Defaults
    Public Module Defaults
        ' Percentage of track length at which to scrobble
        Public Const kScrobblePoint As Integer = 50
        Public Const kLanguageCode As String = kLanguageCodeEnglish
        Public Const kAudioscrobblerFoundingTimeStamp As Integer = 1009843200

        'Some language codes. For more, see ISO 639-2: http://www.loc.gov/standards/iso639-2/php/code_list.php
        Public Const kLanguageCodeEnglish As String = "en"
        Public Const kLanguageCodeGerman As String = "de"
        Public Const kLanguageCodeFrench As String = "fr"
        Public Const kLanguageCodeItalian As String = "it"
        Public Const kLanguageCodeSpanish As String = "es"
        Public Const kLanguageCodeJapanese As String = "ja"
        Public Const kLanguageCodePolish As String = "pl"
        Public Const kLanguageCodePortuguese As String = "pt"
        Public Const kLanguageCodeRussian As String = "ru"
        Public Const kLanguageCodeTurkish As String = "tr"

        'Default platform
        Public Const kPlatform As String = "win32"



        Public Const kRequestTimeoutInMs As Integer = 20000
        'For the lines below, please see http://www.audioscrobbler.net/development/protocol/
        Public Const kClientVersion As String = "1.0"
        Public Const kClientID As String = kClientIDTest
        Public Const kClientIDTest As String = "tst"
        Public Const kClientIDAmigaAMP As String = "ami"
        Public Const kClientIDAudacious As String = "aud"
        Public Const kClientIDApplescriptable_MacOS_X_Application As String = "osx"
        Public Const kClientIDBMPx As String = "mpx"
        Public Const kClientIDFooBar As String = "foo"
        Public Const kClientIDHerrie As String = "her"
        Public Const kClientID_iTunes As String = kClientIDApplescriptable_MacOS_X_Application
        Public Const kClientIDMusicMatch_Jukebox As String = "mmj"
        Public Const kClientIDQCD As String = "qcd"
        Public Const kClientIDSliMP3 As String = "slm"
        Public Const kClientIDWinamp2 As String = "wa2"
        Public Const kClientIDWinamp3 As String = "wa3"
        Public Const kClientIDWindows_Media_Player As String = "wmp"
        Public Const kClientIDXMMS As String = "xms"
        Public Const kClientIDXMMS2 As String = "xm2"

        ''' <summary>
        ''' What should be appended as a cache file name for submission. Default is [username]_submissions.xml
        ''' </summary>
        ''' <remarks></remarks>
        Public Const kSubmissionCacheFilenamePostfix = "_submissions.xml"
        Public Const kSubmissionCacheDir As String = "cache/"
        Public Const kSubmissionMediaDevicePostfix = "_mediadevice.xml"
    End Module

End Namespace