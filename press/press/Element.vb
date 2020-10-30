<System.Serializable()> Public MustInherit Class Element
    Public square As Double ' площадь
    Public volume As Double ' объем 
    Public kUdel As Decimal ' удельная скорость газовыделения стенок
    Public qUdel As Decimal ' удельный поток газовыделение стенок
    Public qAfterHeat As Decimal ' удельный поток газовыделения после прогрева
    Public kAfterHeat As Decimal ' макс уменьшение к
End Class
