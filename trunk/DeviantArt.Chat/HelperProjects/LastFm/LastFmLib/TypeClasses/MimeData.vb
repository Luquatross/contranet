Namespace TypeClasses
    ''' <summary>
    ''' This class inhertits the QMimeData class(a port from QT to .net) used to store various things.
    ''' </summary>
    Public Class MimeData
        Inherits QMimeData
        ''' <summary>
        ''' Gets a value indicating whether this instance has a track.
        ''' </summary>
        ''' <value><c>true</c> if this instance has a track; otherwise, <c>false</c>.</value>
        ReadOnly Property hasTrack() As Boolean
            Get
                If m_contents.ContainsKey("item/track") Then
                    Return TypeOf m_contents("item/track") Is Track
                Else
                    Return False
                End If
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the track.
        ''' </summary>
        ''' <value>The track.</value>
        Property Track() As Track
            Get
                If hasTrack() Then
                    Return m_contents("item/track")
                Else
                    Return Nothing
                End If
            End Get
            Set(ByVal value As Track)
                If m_contents.ContainsKey("item/track") Then
                    m_contents("item/track") = value
                Else
                    m_contents.Add("item/track", value)
                End If
            End Set
        End Property
        ''' <summary>
        ''' Gets a value indicating whether this instance has a user.
        ''' </summary>
        ''' <value><c>true</c> if this instance has a user; otherwise, <c>false</c>.</value>
        ReadOnly Property hasUser() As Boolean
            Get
                If m_contents.ContainsKey("item/user") Then
                    Return TypeOf m_contents("item/user") Is String
                Else
                    Return False
                End If
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the user.
        ''' </summary>
        ''' <value>The user.</value>
        Property User() As String
            Get
                If hasUser() Then
                    Return m_contents("item/user")
                Else
                    Return Nothing
                End If
            End Get
            Set(ByVal value As String)
                If m_contents.ContainsKey("item/user") Then
                    m_contents("item/user") = value
                Else
                    m_contents.Add("item/user", value)
                End If
            End Set
        End Property

        ''' <summary>
        ''' Gets a value indicating whether this instance has a tag.
        ''' </summary>
        ''' <value><c>true</c> if this instance has a tag; otherwise, <c>false</c>.</value>
        ReadOnly Property hasTag() As Boolean
            Get
                If m_contents.ContainsKey("item/tag") Then
                    Return TypeOf m_contents("item/tag") Is Track
                Else
                    Return False
                End If
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the tag.
        ''' </summary>
        ''' <value>The tag.</value>
        Property Tag() As Tag
            Get
                If hasTag() Then
                    Return m_contents("item/tag")
                Else
                    Return Nothing
                End If
            End Get
            Set(ByVal value As Tag)
                If m_contents.ContainsKey("item/tag") Then
                    m_contents("item/tag") = value
                Else
                    m_contents.Add("item/tag", value)
                End If
            End Set
        End Property
        ''' <summary>
        ''' Gets a value indicating whether this instance has a station.
        ''' </summary>
        ''' <value>
        ''' <c>true</c> if this instance has a station; otherwise, <c>false</c>.
        ''' </value>
        ReadOnly Property hasStation() As Boolean
            Get
                If m_contents.ContainsKey("item/station") Then
                    Return TypeOf m_contents("item/station") Is Track
                Else
                    Return False
                End If
            End Get
        End Property
        ''' <summary>
        ''' Gets or sets the station.
        ''' </summary>
        ''' <value>The station.</value>
        Property Station() As Station
            Get
                If hasStation() Then
                    Return m_contents("item/station")
                Else
                    Return Nothing
                End If
            End Get
            Set(ByVal value As Station)
                If m_contents.ContainsKey("item/station") Then
                    m_contents("item/station") = value
                Else
                    m_contents.Add("item/station", value)
                End If
            End Set
        End Property

    End Class
End Namespace
