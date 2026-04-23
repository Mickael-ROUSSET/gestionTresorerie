Public Class DataAccessException
    Inherits Exception

    Public Sub New(message As String, innerException As Exception)
        MyBase.New(message, innerException)
    End Sub
End Class