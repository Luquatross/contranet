Imports System.Xml
Namespace RSS
    Public Class RssParser
        Dim m_rss As String
        Dim m_doc As RssDocument
        ReadOnly Property Document() As RssDocument
            Get
                Return m_doc
            End Get
        End Property
        Property RSSText() As String
            Get
                Return m_rss
            End Get
            Set(ByVal value As String)
                m_rss = value
            End Set
        End Property
        Sub New()

        End Sub
        Sub New(ByVal rss As String)
            m_rss = rss
        End Sub
        Sub New(ByVal url As Uri)
            If url Is Nothing Then Exit Sub

            Dim wc As New Net.WebClient
            Try
                m_rss = wc.DownloadString(url)
            Catch we As Net.WebException
                Debug.Print("Failed to get RSS from " & url.AbsoluteUri)
            End Try
        End Sub

        Public Function Parse() As RssDocument
            Dim result As New RssDocument
            Dim xdoc As New XmlDocument
            Try
                xdoc.LoadXml(m_rss)
            Catch x As XmlException
                Return Nothing
            End Try
            Dim root As XmlElement = xdoc.DocumentElement

            'Dim nsm As New Xml.XmlNamespaceManager(xdoc.NameTable)
            'For Each s As String In nsm.HasN
            '    Debug.Print(s)
            'Next
            If root.Name <> "rss" Then
                Throw New RssWrongNameException("e", "rss")
                Return Nothing
            End If
            root = root.SelectSingleNode("channel")
            If root Is Nothing Then Return Nothing

            result.Header = ChannelHeader.FromXmlElement(root)
            For Each e As XmlElement In root.SelectNodes("item")
                Dim i As RssItem = RssItem.FromXMLElement(e)
                If i IsNot Nothing Then result.Items.Add(i)
            Next
            m_doc = result
            Return result
        End Function

        'Shared Function GetTestRSS() As String
        '    Return IO.File.ReadAllText("C:\test.rss")
        'End Function
    End Class
End Namespace