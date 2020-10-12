<System.Serializable()> Public Structure DrawingData
    Public tStart As Double ' начальный отрезок для отрисовки
    Public tEnd As Double ' конечный отрезок для отрисовки
    Public countDraw As Integer ' счетчик для смены цвета
    Public height As Integer ' габариты экрана (забираем с главной формы)
    Public width As Integer
End Structure
