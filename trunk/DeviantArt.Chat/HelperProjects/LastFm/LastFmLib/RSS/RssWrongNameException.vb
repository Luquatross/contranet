Public Class RssWrongNameException
    Inherits Exception
    Dim m_param As String, m_rightValue As String


    Sub New(ByVal paramName As String, Optional ByVal RightValue As String = "")
        MyBase.New("The name of " & paramName & " MUST BE '" & RightValue & "'!")
        m_param = paramName
        m_rightValue = RightValue
    End Sub


End Class
