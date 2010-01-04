Imports System.Xml
Namespace API20.Base
    Public MustInherit Class BaseArtistRequest
        Inherits Base.BaseRequest

        Protected m_artist As String
        Property Artist() As String
            Get
                Return m_artist
            End Get
            Set(ByVal value As String)
                m_artist = value
            End Set
        End Property

        Protected Sub New(ByVal type As RequestType, ByVal artist As String)
            MyBase.New(type)
            m_requiredParams.Add("artist")
            m_artist = artist
        End Sub
        Public Overrides Sub Start()
            SetAddParamValue("artist", m_artist)
            MyBase.Start()
        End Sub
        Protected MustOverride Overrides Sub Success(ByVal elem As System.Xml.XmlElement)

    End Class
End Namespace