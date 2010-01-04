Imports LastFmLib
Imports LastFmLib.TypeClasses
Imports System.Xml
Namespace WebRequests
    ''' <summary>
    ''' Base class for all tags requests
    ''' </summary>
    Public MustInherit Class TagsRequestBase
        Inherits RequestBase
        Protected m_tags As New WeightedStringList()
        ''' <summary>
        ''' Gets the resulting tag list(after a successful request).
        ''' </summary>
        ''' <value>The tags.</value>
        ReadOnly Property tags() As WeightedStringList
            Get
                Return m_tags
            End Get
        End Property

        ''' <summary>
        ''' Initializes a new instance of the <see cref="TagsRequestBase" /> class.
        ''' </summary>
        ''' <param name="type">The type.</param>
        ''' <param name="name">The name.</param>
        Protected Sub New(ByVal [type] As RequestType, ByVal name As String)
            MyBase.New([type], name)
        End Sub

    End Class

End Namespace
