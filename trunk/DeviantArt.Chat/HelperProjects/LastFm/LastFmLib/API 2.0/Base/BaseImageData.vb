Namespace API20.Base
    Public MustInherit Class BaseImageData
        Protected m_imageSmall As Uri
        Protected m_imageMedium As Uri
        Protected m_imageLarge As Uri
        Property ImageLarge() As Uri
            Get
                Return m_imageLarge
            End Get
            Set(ByVal value As Uri)
                m_imageLarge = value
            End Set
        End Property
        Property ImageMedium() As Uri
            Get
                Return m_imageMedium
            End Get
            Set(ByVal value As Uri)
                m_imageMedium = value
            End Set
        End Property
        Property ImageSmall() As Uri
            Get
                Return m_imageSmall
            End Get
            Set(ByVal value As Uri)
                m_imageSmall = value
            End Set
        End Property
        Sub New()

        End Sub
        Protected Sub SetImagesByXmlElem(ByVal e As Xml.XmlElement)

            Uri.TryCreate(Util.GetSubElementValue(e, "image[@size=""small""]"), UriKind.Absolute, Me.ImageSmall)
            Uri.TryCreate(Util.GetSubElementValue(e, "image[@size=""medium""]"), UriKind.Absolute, Me.ImageMedium)
            Uri.TryCreate(Util.GetSubElementValue(e, "image[@size=""large""]"), UriKind.Absolute, Me.ImageLarge)
            If e.SelectNodes("image").Count = 1 Then
                Uri.TryCreate(Util.GetSubElementValue(e, "image"), UriKind.Absolute, Me.ImageMedium)
            End If
            'Uri.TryCreate(Util.GetSubElementValue(e, "image[@size=""original""]"), UriKind.Absolute, Me.ImageOriginal)
        End Sub

    End Class
End Namespace
