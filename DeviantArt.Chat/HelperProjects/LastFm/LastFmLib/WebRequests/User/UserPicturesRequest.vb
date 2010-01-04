Namespace WebRequests
    ''' <summary>
    ''' Gets the avatar picture urls for a list of users.
    ''' </summary>
    Public Class UserPicturesRequest
        Inherits RequestBase
        Private m_names As New List(Of String)
        Private m_urls As New Dictionary(Of String, String)
        ''' <summary>
        ''' Gets the urls.
        ''' </summary>
        ''' <value>The urls.</value>
        ReadOnly Property Urls() As Dictionary(Of String, String)
            Get
                Return m_urls
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the names.
        ''' </summary>
        ''' <value>The names.</value>
        Property Names() As List(Of String)
            Get
                Return m_names
            End Get
            Set(ByVal value As List(Of String))
                m_names = value
            End Set
        End Property
        ''' <summary>
        ''' Initializes a new instance of the <see cref="UserPicturesRequest" /> class.
        ''' </summary>
        Sub New()
            MyBase.New(WebRequests.RequestType.UserPictures, "UserPictures")
        End Sub
        Sub New(ByVal userlist As List(Of String))
            Me.New()
            m_names = userlist
        End Sub

        Public Overrides Sub Start()
            Dim rpc As New XMLRPC
            rpc.Method = "getAvatarUrls"
            rpc.addParameter(m_names)
            Request(rpc)
        End Sub
        Protected Overrides Sub success(ByVal data As String)
            Dim r As New XMLRPC
            Dim retVals As New List(Of Object)
            Dim err As String = ""
            Dim parsed As Boolean = XMLRPC.ParseResponse(data, retVals, err)
            If Not parsed Then
                setFailed(WebRequestResultCode.WebRequestResult_Custom, "Couldn't parse")
                Exit Sub
            End If
            If retVals(0).GetType IsNot (New List(Of Object)).GetType Then
                setFailed(WebRequestResultCode.WebRequestResult_Custom, "Result wasn't an <array>.")
                Exit Sub
            End If
            Dim arr As New List(Of Object)
            arr.AddRange(retVals(0))
            For Each val As Object In arr
                Dim map As New Dictionary(Of String, String)
                For Each a As KeyValuePair(Of String, Object) In val
                    map.Add(a.Key, CStr(a.Value))
                Next
                Dim user As String = map.Item("name")
                Dim url As String = map.Item("avatar")
                m_urls.Add(user, url)
            Next

        End Sub
        ''' <summary>
        ''' Fetches for user.
        ''' </summary>
        ''' <param name="username">The username.</param>
        ''' <returns></returns>
        Public Shared Function fetchForUser(ByVal username As String) As UserPicturesRequest
            Dim r As New UserPicturesRequest
            Dim l As New List(Of String)
            l.Add(username)
            r.Names = l
            r.Start()
            Return r
        End Function

    End Class
End Namespace
