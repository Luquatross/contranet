Imports System.Xml
Imports LastFmLib.API20.Types
Namespace API20.TasteOMeter
    ''' <summary>
    ''' Get a Tasteometer score from two inputs, along with a list of shared artists.
    ''' If the input is a User or a Myspace URL, some additional information is returned. 
    ''' </summary>
    Public Class TasteOMeterCompare
        Inherits Base.BaseRequest

        Private m_data1 As TasteOMeterData
        Private m_limit As Integer
        Private m_score As Single
        Private m_artists As List(Of ArtistInfo)
        Public ReadOnly Property Result() As List(Of ArtistInfo)
            Get
                Return m_artists
            End Get
        End Property

        Public ReadOnly Property Score() As Single
            Get
                Return m_score
            End Get
            'Set(ByVal value As Single)
            '    m_score = value
            'End Set
        End Property

        Public Property Limit() As Integer
            Get
                Return m_limit
            End Get
            Set(ByVal value As Integer)
                m_limit = value
            End Set
        End Property


        ''' <summary>
        ''' The first user/artist or myspace account
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Data1() As TasteOMeterData
            Get
                Return m_data1
            End Get
            Set(ByVal value As TasteOMeterData)
                m_data1 = value
            End Set
        End Property
        Private m_data2 As TasteOMeterData
        ''' <summary>
        ''' The first user/artist or myspace account
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Data2() As TasteOMeterData
            Get
                Return m_data2
            End Get
            Set(ByVal value As TasteOMeterData)
                m_data1 = value
            End Set
        End Property

        Sub New(ByVal t1 As TasteOMeterData, ByVal t2 As TasteOMeterData)
            MyBase.New(RequestType.TasteOMeterCompare)
            m_data1 = t1
            m_data2 = t2
        End Sub

        Public Overloads Overrides Sub Start()
            SetAddParamValue("type1", GetTypeStr(Data1.Type))
            SetAddParamValue("value1", Data1.Value)
            SetAddParamValue("type2", GetTypeStr(Data2.Type))
            SetAddParamValue("value2", Data2.Value)
            If m_limit > 0 Then SetAddParamValue("limit", m_limit)
            MyBase.Start()
        End Sub

        Protected Overrides Sub Success(ByVal elem As System.Xml.XmlElement)
            Dim result As XmlElement = elem.SelectSingleNode("result")
            m_score = CSng(result.SelectSingleNode("score").InnerText)
            m_artists = New List(Of ArtistInfo)
            For Each a As XmlElement In result.SelectNodes("artists/artist")
                m_artists.Add(ArtistInfo.FromXmlElement(a))
            Next

            'maybe parse the input node later on, but doubt that's really necessary
            Dim Input As XmlElement = elem.SelectSingleNode("input")
        End Sub

        Private Function GetTypeStr(ByVal t As TasteOMeterType) As String
            Select Case t
                Case TasteOMeterType.Artists
                    Return "artists"
                Case TasteOMeterType.MySpace
                    Return "myspace"
                Case TasteOMeterType.User
                    Return "user"
                Case Else
                    Return ""
            End Select
        End Function
    End Class
End Namespace
