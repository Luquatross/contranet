Imports System.Xml
Imports LastFmLib.API20.Types
Namespace API20.User
    ''' <summary>
    ''' Get the top artists listened to by a user.
    ''' You can stipulate a time period. Sends the overall chart by default. 
    ''' </summary>
    Public Class UserGetTopArtists
        Inherits Base.BaseUserGetTopX

        Dim m_artists As List(Of ArtistInfo)
        ''' <summary>
        ''' Gets the request result.
        ''' </summary>
        ''' <value>The result.</value>
        ReadOnly Property Result() As List(Of ArtistInfo)
            Get
                Return m_artists
            End Get
        End Property
        ''' <summary>
        ''' Initializes a new instance of the <see cref="UserGetTopArtists" /> class.
        ''' </summary>
        ''' <param name="user">The user name</param>
        Sub New(ByVal user As String, Optional ByVal period As ChartPeriod = ChartPeriod.Overall)
            MyBase.New(RequestType.UserGetTopArtists, user, period)
        End Sub
        Protected Overrides Sub Success(ByVal elem As System.Xml.XmlElement)
            Dim list As XmlNodeList = elem.SelectNodes("artist")
            m_artists = New List(Of ArtistInfo)
            For Each u As XmlElement In list
                m_artists.Add(ArtistInfo.FromXmlElement(u))
            Next
        End Sub

        Public Function ToDebugString() As String
            Dim result As String = "Top fans for: " & m_User
            For Each f As ArtistInfo In m_artists
                result &= f.Name & ", "
            Next
            result.Remove(result.IndexOf(","), 2)
            Return result
        End Function
    End Class
End Namespace