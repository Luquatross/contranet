Imports LastFmLib.TypeClasses
Imports LastFmLib.WebRequests
Namespace WebRequests
    ''' <summary>
    ''' Requests a friend list for a user.
    ''' </summary>
    Public Class FriendsRequest
        Inherits RequestBase
        Private m_username As String
        Private m_usernames As New List(Of String)
        Private m_metaDatas As New List(Of UserMetaData)

        ReadOnly Property metaDatas() As List(Of UserMetaData)
            Get
                Return m_metaDatas
            End Get
        End Property
        ReadOnly Property FriendUsernames() As List(Of String)
            Get
                Return m_usernames
            End Get
        End Property
        Property username() As String
            Get
                Return m_username
            End Get
            Set(ByVal value As String)
                m_username = value
            End Set
        End Property
        
        Public Sub New(ByVal username As String)
            MyBase.New(RequestType.Friends, "Friends")
            Me.username = username
        End Sub
        Public Overrides Sub Start()
            'I won't implement this, for portablity reasons, so just throw an exception
            '    If (m_username.isEmpty()) Then
            'm_username = The::webService()->currentUsername();
            If String.IsNullOrEmpty(m_username) Then
                Throw New ArgumentNullException("Friendsrequest.Username")
            End If
            [get]("/1.0/user/" + EscapeUriData(m_username) + "/friends.xml?showtracks=1")
        End Sub
        Protected Overrides Sub success(ByVal data As String)
            'MyBase.success(data)
            Dim document As New Xml.XmlDocument
            Try
                document.LoadXml(data)
            Catch x As Xml.XmlException
                Throw New Exceptions.ParseException("Failed to load XML in FriendsRequest.Success().", x)
            End Try
            Dim nl As Xml.XmlNodeList
            nl = document.SelectNodes("friends")
            If nl.Count = 0 Then Exit Sub
            Dim user As String = nl.Item(0).Attributes.GetNamedItem("user").Value
            Dim values As Xml.XmlNodeList = document.DocumentElement.SelectNodes("user")

            For i As Integer = 0 To values.Count - 1
                Dim umd As New UserMetaData()
                Dim image As Xml.XmlNode = values.Item(i).Item("image")
                umd.Name = values.Item(i).Attributes.GetNamedItem("username").Value
                If image IsNot Nothing Then umd.Image = New Uri(image.InnerText)

                'Recent Track
                Dim lasttrack As Xml.XmlNode = values.Item(i).Item("lasttrack")
                If lasttrack IsNot Nothing Then
                    Dim recentArtist As Xml.XmlNode = lasttrack.Item("artist")
                    Dim recentTrack As Xml.XmlNode = lasttrack.Item("name")
                    Dim recentDate As Xml.XmlNode = lasttrack.Item("date")

                    If recentArtist IsNot Nothing And recentTrack IsNot Nothing Then 'And recentDate IsNot Nothing Then
                        If recentDate IsNot Nothing Then umd.LastActivity = recentDate.InnerText
                        Dim l As New List(Of String)
                        'set the recent tracks to only the last one...more information aren't provieded
                        l.Add(recentArtist.InnerText & " - " & recentTrack.InnerText)
                        umd.recentTracks = l
                    End If
                End If
                m_metaDatas.Add(umd)
                m_usernames.Add(umd.Name)
            Next
            'Check here if this is done correctly
            m_usernames.Sort()
        End Sub
        Public Overrides Sub TryAgain()

        End Sub
    End Class
End Namespace