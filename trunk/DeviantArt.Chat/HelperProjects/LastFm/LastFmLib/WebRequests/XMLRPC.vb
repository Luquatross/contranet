Imports System.Xml
Namespace WebRequests
    ''' <summary>
    ''' A class for parsing XML remote procedure calls.
    ''' </summary>
    Public Class XMLRpc


#Region "Variables"
        Private m_method As String
        Private m_parameters As New List(Of Object)
        Private m_useCache As Boolean
#End Region
#Region "Propertys"
        ''' <summary>
        ''' Gets the parameter list.
        ''' </summary>
        ''' <value>The parameters.</value>
        Public ReadOnly Property Parameters() As List(Of Object)
            Get
                Return m_parameters
            End Get
        End Property
        Public Property Method() As String
            Get
                Return m_method
            End Get
            Set(ByVal value As String)
                m_method = value
            End Set
        End Property
        ''' <summary>
        ''' Gets or sets a value indicating whether to use a cache or not.
        ''' </summary>
        ''' <value>Caching isn't implemented yet, so this will throw a NotSupportedException.</value>
        Public Property UseCache() As Boolean
            Get
                Return m_useCache
            End Get
            Set(ByVal value As Boolean)
                'not implemented => throw exception
                Throw New NotSupportedException("Not implemented yet.")
                'm_useCache = value
            End Set
        End Property

#End Region
#Region "Enums"
        ''' <summary>
        ''' An enum for the "types" inside the rpc.
        ''' </summary>
        Enum enType
            ''' <summary>
            ''' Integer
            ''' </summary>
            [Integer]
            ''' <summary>
            ''' Structure
            ''' </summary>
            Struct
            ''' <summary>
            ''' Array
            ''' </summary>
            [Array]
            ''' <summary>
            ''' Boolean
            ''' </summary>
            [Boolean]
            ''' <summary>
            ''' String
            ''' </summary>
            [String]
            [Double]
            ''' <summary>
            ''' Unknown type
            ''' </summary>
            [Unhandled]
        End Enum
#End Region

#Region "Functions"

#Region "addParameter"
        ''' <summary>
        ''' Adds the parameter.
        ''' </summary>
        ''' <param name="v">The new parameter(-object)</param>
        Public Sub addParameter(ByVal v As Object)
            m_parameters.Add(v)
        End Sub
#End Region

#Region "ToString"
        ''' <summary>
        ''' Returns the xml request data, including parameters, as an XML-string.
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function toString() As String

            If String.IsNullOrEmpty(m_method) Then Return Nothing

            Dim xdoc As New XmlDocument()
            xdoc.PreserveWhitespace = True
            Dim newLine As XmlWhitespace = xdoc.CreateWhitespace(vbCrLf)
            xdoc.AppendChild(xdoc.CreateXmlDeclaration("1.0", "", ""))
            xdoc.AppendChild(newLine)
            Dim methodCall As XmlElement = xdoc.CreateElement("methodCall")
            methodCall.AppendChild(newLine)
            Dim elem As XmlElement = xdoc.CreateElement("methodName")
            elem.InnerText = m_method
            methodCall.AppendChild(elem)
            methodCall.AppendChild(newLine)

            elem = xdoc.CreateElement("params")
            elem.AppendChild(newLine)
            For Each p As Object In m_parameters
                Dim param As XmlElement = ParseObjectToXmlElem(xdoc, p, False)
                elem.AppendChild(param)
            Next
            elem.AppendChild(newLine)
            methodCall.AppendChild(elem)
            xdoc.AppendChild(methodCall)
            Return xdoc.InnerXml
        End Function


#End Region
#Region "Parse"
        ''' <summary>
        ''' Parses the specified XML response.
        ''' </summary>
        ''' <param name="xmlResponse">The XML response.</param>
        ''' <param name="returnValues">The return values.</param>
        ''' <param name="error">The error.</param>
        ''' <returns></returns>
        Public Shared Function ParseResponse(ByVal xmlResponse As String, ByRef returnValues As List(Of Object), ByRef [error] As String) As Boolean
            Dim xml As New Xml.XmlDocument

            Dim node As Xml.XmlNode
            Dim param As Xml.XmlElement
            Try
                xml.LoadXml(xmlResponse)
            Catch x As XmlException
                [error] = "Couldn't parse XML response: " + xmlResponse
                Return False
            End Try

            Dim fault As Xml.XmlNodeList = xml.DocumentElement.SelectNodes("fault/value/struct")

            'Wenn es einen Knoten mit fehler gibt
            If (fault.Count > 0) Then
                Dim faultElem As XmlElement = fault(0)

                Dim o As Object = ParseValueStringToObject(faultElem)
                returnValues = New List(Of Object)
                returnValues.Add(o)
                [error] = "Fault present in XML response: " + xmlResponse
                Return False
            End If
            'console.writeLine("<?xml version=""1.0"" encoding=""UTF-8""?>")
            'console.writeLine("<methodResponse>")

            Dim params As Xml.XmlNodeList = xml.DocumentElement.SelectNodes("params/param")
            If (params.Count = 0) Then
                [error] = "No params present in XML response: " + xmlResponse
                Return False
            End If
            'console.writeLine("<params>")
            For i As Integer = 0 To params.Count() - 1

                node = params.Item(i)
                '######DEBUG########
                'console.writeLine("<param>")
                'console.write("<value>")
                'Skip past the pointless "<value>" tag
                param = node.FirstChild.FirstChild

                If (param.IsEmpty) Then

                    [error] = "Malformed XML: " + xmlResponse
                    Return False
                Else
                    Dim o As Object
                    o = ParseValueStringToObject(param)
                    returnValues.Add(o)
                End If
                'console.write("</value>" & vbCrLf & "</param>")
            Next
            'console.write("</params>")
            Return True
        End Function
#End Region
#Region "ParseObjectToXmlElem"
        Private Function ParseObjectToXmlElem(ByRef xdoc As Xml.XmlDocument, ByRef o As Object, Optional ByVal verify As Boolean = False, Optional ByVal noParam As Boolean = False) As XmlElement
            If o Is Nothing Then Return xdoc.CreateElement("nil")
            Dim ws As XmlWhitespace = xdoc.CreateWhitespace(vbCrLf)
            Dim result As XmlElement = Nothing
            Dim typeName As String = o.GetType.Name.ToString.ToLower
            If typeName.Contains("`") Then typeName = typeName.Remove(typeName.IndexOf("`"))
            Dim param As XmlElement = xdoc.CreateElement("param")
            param.AppendChild(ws)
            Dim valElem As XmlElement = xdoc.CreateElement("value")
            Dim typeElem As XmlElement = Nothing
            Dim typeContent As String = ""
            If typeName = "list" Then typeName = "array"
            If TypeOf (o) Is Array Then typeName = "array"
            If typeName = "dictionary" Or typeName = "sorteddictionary" Then typeName = "struct"
            Dim elemType As enType = TypeFromString(typeName)
            Select Case elemType
                Case enType.String
                    typeElem = xdoc.CreateElement("string")
                    typeContent = CStr(o)
                Case enType.Integer
                    typeElem = xdoc.CreateElement("i4")
                    typeContent = CStr(o)
                Case enType.Boolean
                    typeElem = xdoc.CreateElement("boolean")
                    typeContent = CStr(o)
                Case enType.Double
                    typeElem = xdoc.CreateElement("double")
                    typeContent = CStr(o)
                Case enType.Array
                    typeElem = xdoc.CreateElement("array")
                    typeElem.AppendChild(ws)
                    Dim dataelem As XmlElement = xdoc.CreateElement("data")
                    dataelem.AppendChild(ws)
                    For Each ob As Object In o
                        Dim subelem As XmlElement = ParseObjectToXmlElem(xdoc, ob, verify)
                        dataelem.AppendChild(subelem)
                    Next
                    dataelem.AppendChild(ws)
                    typeElem.AppendChild(dataelem)
                Case enType.Struct
                    typeElem = xdoc.CreateElement("struct")
                    typeElem.AppendChild(ws)
                    For Each kv As Object In o
                        Dim member As XmlElement = xdoc.CreateElement("member")
                        member.AppendChild(ws)
                        Dim name As XmlElement = xdoc.CreateElement("name")
                        name.InnerText = kv.Key
                        member.AppendChild(name)
                        Dim valNode As XmlElement = ParseObjectToXmlElem(xdoc, kv.Value, verify, True)
                        member.AppendChild(valNode)
                        typeElem.AppendChild(member)
                    Next
            End Select
            If Not String.IsNullOrEmpty(typeContent) Then typeElem.InnerText = typeContent
            If Not typeElem Is Nothing Then
                valElem.AppendChild(typeElem)
                param.AppendChild(valElem)
                If noParam Then Return valElem
                result = param
            End If

            Return result
        End Function
#End Region
#Region "ParseValueStringToObject"
        Private Shared Function ParseValueStringToObject(ByRef e As Xml.XmlElement) As Object
            'Console.WriteLine()
            'Console.WriteLine("=============================================================")
            'Console.WriteLine(e.Name & " - " & e.InnerXml)
            Dim tag As String = e.Name
            Select Case TypeFromString(tag)
                Case enType.String
                    'console.write("<string>{0}</string>", e.InnerXml)
                    Return UnEscape(e.InnerXml)

                Case enType.Integer
                    'console.write("<int>{0}</int>", e.InnerXml)

                    Return CInt(e.InnerText)

                Case enType.Boolean

                    Return CBool(e.InnerText)

                Case enType.Struct
                    'console.write("<struct>" & vbCrLf)
                    '!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                    'HIER ggf. listentyp ÄNDERN!
                    '!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                    Dim map As New Dictionary(Of String, Object)
                    Dim nodes As Xml.XmlNodeList
                    'Dim node As Xml.XmlNode
                    'Dim name As Xml.XmlNode
                    'Dim value As Xml.XmlNode
                    'Dim v As Object

                    nodes = e.SelectNodes("member")

                    For Each node As XmlElement In nodes
                        Dim name As Xml.XmlElement = node.SelectSingleNode("name")
                        Dim value As Xml.XmlElement = node.SelectSingleNode("value").FirstChild

                        'console.write("<member>")
                        'console.writeLine("<name>{0}</name>", name.InnerText)

                        'console.write("<value>")
                        Dim v As Object = ParseValueStringToObject(value)
                        'console.writeLine("</value>")
                        If map.ContainsKey(name.InnerText) Then
                            map.Item(name.InnerText) = v
                        Else
                            map.Add(name.InnerText, v)
                        End If


                        'console.writeLine("</member>")
                    Next
                    'console.writeLine("</struct></value>")
                    Return map

                Case enType.Array
                    'console.writeLine("<array>")
                    'console.writeLine("<data>")
                    Dim array As New List(Of Object)
                    Dim nodes As Xml.XmlNodeList
                    Dim node As Xml.XmlNode

                    nodes = e.FirstChild().ChildNodes()
                    'console.write("<value>")
                    For j As Integer = 0 To nodes.Count() - 1
                        node = nodes.Item(j)

                        If (node.NodeType = Xml.XmlNodeType.Element) Then
                            If node.Name = "value" Then
                                Dim o As Object = ParseValueStringToObject(node.FirstChild)
                                array.Add(o)
                            End If
                        End If
                    Next
                    'console.write("</value>")
                    'console.writeLine("</data></array>")
                    Return array


                Case Else
                    ' Support for this type not yet implemented
                    Return Nothing
            End Select
        End Function
#End Region

#Region "(Un-)escape"
        ''' <summary>
        ''' Escapes some chars in the XML string.
        ''' </summary>
        ''' <param name="xml">The XML.</param>
        ''' <returns></returns>
        Public Shared Function Escape(ByVal xml As String) As String
            If String.IsNullOrEmpty(xml) Then Return xml
            xml = xml.Replace("&", "&#38;")
            xml = xml.Replace("<", "&lt;")
            xml = xml.Replace(">", "&gt;")
            'xml = xml.Replace(" ", "%20")

            'Test
            'xml = clsRequest.EscapeUriData(xml)
            Return xml
        End Function

        ''' <summary>
        ''' Unscapes some chars in the XML string.
        ''' </summary>
        ''' <param name="xml">The XML.</param>
        ''' <returns></returns>
        Public Shared Function UnEscape(ByVal xml As String) As String
            If String.IsNullOrEmpty(xml) Then Return xml
            xml = xml.Replace("&amp", "&;")
            xml = xml.Replace("&lt;", "<")
            xml = xml.Replace("&gt;", ">")
            'xml = xml.Replace("%20", " ")
            'Auch test
            'xml = clsRequest.UnEscapeUriData(xml)
            Return xml
        End Function
#End Region
        ''' <summary>
        ''' conversion from a type string like "string" or "i4" into a TypeEnum
        ''' </summary>
        ''' <param name="s">The s.</param>
        ''' <returns></returns>
        Private Shared Function TypeFromString(ByVal s As String) As enType
            If (s = "int32") Then Return enType.Integer
            If (s = "i4") Then Return enType.Integer
            If (s = "int") Then Return enType.Integer
            If (s = "boolean") Then Return enType.Boolean
            If (s = "struct") Then Return enType.Struct
            If (s = "array") Then Return enType.Array
            If (s = "string") Then Return enType.String

            Return enType.Unhandled

        End Function
#End Region

    End Class


End Namespace
