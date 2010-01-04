Namespace WebRequests
    Public Class NeighboursRequest
        Inherits RequestBase
        Private m_username As String
        Private m_Usernames As New TypeClasses.WeightedStringList()
        ReadOnly Property NeighbourUsernames() As TypeClasses.WeightedStringList
            Get
                Return m_Usernames
            End Get
        End Property
        Property Username() As String
            Get
                Return m_username
            End Get
            Set(ByVal value As String)
                m_username = value
            End Set
        End Property
        Sub New(ByVal username As String)
            MyBase.New(RequestType.Neighbours, "Neighbours")
            m_username = username
        End Sub
        Public Overrides Sub Start()
            Me.get("/1.0/user/" + EscapeUriData(m_username) + "/neighbours.xml")
        End Sub
        Protected Overrides Sub success(ByVal data As String)
            Dim xml As New Xml.XmlDocument
            Try
                xml.LoadXml(data)
            Catch ex As Xml.XmlException
                MsgBox("Neighbours Request: Couldn't parse data!")
                Exit Sub
            End Try
            If xml.DocumentElement.HasAttribute("user") Then
                m_username = xml.DocumentElement.GetAttribute("user")
            End If
            Dim values As Xml.XmlNodeList = xml.DocumentElement.SelectNodes("user")
            For i As Integer = 0 To values.Count - 1
                Dim name As String = values.Item(i).Attributes.GetNamedItem("username").Value
                Dim matchStr As String = values.Item(i).Item("match").InnerText()
                Dim match As Integer = 0
                If Not String.IsNullOrEmpty(matchStr) Then
                    matchStr = matchStr.Replace(".", ",")
                    match = CInt(Single.Parse(matchStr))
                End If
                m_Usernames.Add(TypeClasses.WeightedString.weighted(name, match))
            Next
        End Sub
    End Class
End Namespace
