Public Class Unit
    Public body As Body
    Public channel As Channel
    Public drain As Drain
    Public gun As Gun
    Public katod As Katod

    Public Function FindAllValume() As Double
        body = New Body

        Return body.volume
    End Function
End Class
