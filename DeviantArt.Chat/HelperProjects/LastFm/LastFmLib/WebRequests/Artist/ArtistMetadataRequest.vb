Imports System.Xml
Imports LastFmLib.WebRequests
Imports LastFmLib.TypeClasses
'Example use
'===========
'Dim amr as new LastFmLib.WebRequests.ArtistMetaDataRequest("Sting")
'amr.start()
'Do sth., e.g. return the wikiText
'if amr.succeeded then return amr.MetaData.wikiText'put your code here
'
Namespace WebRequests

    ''' <summary>
    ''' Requests Metadata of an artist
    ''' </summary>
    Public Class ArtistMetadataRequest
        Inherits RequestBase

        Dim m_artist As String
        Dim rpc As New XMLRpc()
        Dim m_meta_data As New MetaData()
        ''' <summary>
        ''' Gets the metadata of the Artist, if request succeeded.
        ''' </summary>
        ''' <value>The metadata.</value>
        ReadOnly Property Metadata() As MetaData
            Get
                Return m_meta_data
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the artist name.
        ''' </summary>
        ''' <value>The artist.</value>
        Property Artist() As String
            Get
                Return m_artist
            End Get
            Set(ByVal value As String)
                m_artist = value
            End Set

        End Property

        ''' <summary>
        ''' Initializes a new instance of the <see cref="ArtistMetadataRequest" /> class.
        ''' </summary>
        ''' <param name="artist">The artist.</param>
        Public Sub New(ByVal artist As String)
            MyBase.New(RequestType.ArtistMetaData, "ArtistMetaData")
            m_artist = artist

        End Sub



        Overrides Sub start()

            rpc.addParameter(XMLRpc.Escape(m_artist))
            rpc.addParameter("en")
            rpc.Method = "artistMetadata"
            Request(rpc)
        End Sub
        Protected Overrides Sub success(ByVal data As String)
            'MyBase.success(data)
            Dim retVals As New List(Of Object)
            Dim [error] As String = ""
            Dim parsed As Boolean = XMLRpc.ParseResponse(data, retVals, [error])
            Dim map As New Collections.Generic.Dictionary(Of String, Object)
            If Not parsed Then
                MsgBox([error])
                Exit Sub
            End If
            map = retVals(0)

            If Not retVals.Item(0).GetType.Equals((New Collections.Generic.Dictionary(Of String, Object)).GetType) Then
                setFailed(RequestBase.WebRequestResultCode.WebRequestResult_Custom, "Result wasn't a <struct>, artist not found?")
                Exit Sub
            End If
            If map.ContainsKey("faultCode") Then
                Dim faultString As String = CStr(map.Item("faultString"))
                'LOGL( 2, faultString );
                setFailed(RequestBase.WebRequestResultCode.WebRequestResult_Custom, faultString)
                Exit Sub
            End If

            With m_meta_data
                'Manchmal TargetInvocationException hier!!!!
                Me.Artist = CStr(map.Item("artistName"))
                .ArtistName = Me.Artist.Clone
                .artistPageUrl = CStr(map.Item("artistPageUrl"))


                Dim tags As New List(Of String)
                'For Each val As Object In map.Values
                '    tags.Add(CStr(val))
                'Next
                For Each a As String In map.Item("artistTags")
                    tags.Add(a)
                Next
                .ArtistTags = tags
                .numListeners = CInt(map.Item("numListeners"))
                .numPlays = CInt(map.Item("numPlays"))
                .artistPicUrl = New Uri(CStr(map.Item("picture")))

                Dim similar As New List(Of String)
                For Each a As String In map.Item("similar")
                    similar.Add(a)
                Next
                .similarArtists = similar

                Dim fans As New List(Of String)
                For Each a As String In map.Item("topFans")
                    fans.Add(a)
                Next
                .topFans = fans

                .wikiPageUrl = CStr(map.Item("wikiPageUrl"))
                Dim wiki As String = CStr(map.Item("wikiText"))
                '
                wiki = Utils.StripBBCode(wiki)
                '.wiki = wiki.Replace(Chr(13), vbLf)
                .Wiki = wiki.Replace(Chr(10), vbCrLf)
                .Wiki = wiki.Replace("&amp;", "&")
            End With

        End Sub

    End Class


End Namespace
