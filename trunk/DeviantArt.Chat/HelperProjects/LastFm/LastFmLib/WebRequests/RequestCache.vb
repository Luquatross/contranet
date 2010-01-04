Imports System.Xml
Namespace WebRequests
    Public Class RequestCache
        Private m_cacheDirectory As String
        Private m_requests As List(Of RequestBase)
        Property CacheDirectory() As String
            Get
                Return m_cacheDirectory
            End Get
            Set(ByVal value As String)
                m_cacheDirectory = value
            End Set
        End Property

        Sub Add(ByVal r As RequestBase)
            m_requests.Add(r)
        End Sub

        ''' <summary>
        ''' Converts an XmlElement into a request.
        ''' </summary>
        ''' <param name="data">The data XmlElement. It must have "request" as name and "type" as Attribute</param>
        ''' <returns>
        ''' If successful, a RequestBase is returned, else <c>null</c>
        ''' </returns>
        Private Function XmlElementToRequest(ByVal data As XmlElement) As RequestBase
            Dim result As RequestBase = Nothing
            If data.Name = "request" Then
                Dim type As RequestType
                If data.HasAttribute("type") Then
                    Dim val As Integer = CInt(data.GetAttribute("type"))
                    If val > 0 And RequestType.IsDefined((New RequestType).GetType, val) Then
                        type = val
                    Else
                        Throw New Exceptions.ParseException("The attribute 'type' has to contain an Integer > 0! Moreover, this value has to be caontained in RequestType!")
                    End If

                Else
                    Throw New Exceptions.ParseException("'data' has no attribute determining the request type!")
                End If
                If RequestBase.GetCacheableState(type) = False Then Return Nothing
                Select Case type
                    Case RequestType.AlbumMetaData
                        result = AlbumMetaDataRequest.InitFromXmlElement(data)
                    Case RequestType.ArtistMetaData
                        result = AlbumMetaDataRequest.InitFromXmlElement(data)
                        'Case RequestType.ArtistTags
                        '    result = ArtistTagsRequest.InitFromXmlElement(data)
                        'Case RequestType.Friends
                        '    result = FriendsRequest.InitFromXmlElement(data)
                        'Case RequestType.Neighbours
                        '    result = NeighboursRequest.InitFromXmlElement(data)
                        'Case RequestType.RecentlyBannedTracks
                        '    result = RecentlyBannedTracksRequest.InitFromXmlElement(data)
                        'Case RequestType.RecentlyLovedTracks
                        '    result = RecentlyLovedTracksRequest.InitFromXmlElement(data)
                        'Case RequestType.RecentTracks
                        '    result = RecentTracksRequest.InitFromXmlElement(data)
                        'Case RequestType.SearchTag
                        '    result = SearchTagRequest.InitFromXmlElement(data)
                        'Case RequestType.SimilarArtists
                        '    result = SearchTagRequest.InitFromXmlElement(data)

                End Select
            Else
                Return Nothing
            End If
            Return result
        End Function
    End Class
End Namespace
