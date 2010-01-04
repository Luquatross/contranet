'These are only pseudo-like classes, for better naming and code readability
Imports LastFmLib.TypeClasses
Public Class Tag
    Inherits WeightedString
    Sub New(ByVal name As String, Optional ByVal count As Integer = -1)
        MyBase.New(name, count)
    End Sub
End Class
Public Class TagList
    Inherits WeightedStringList
End Class
