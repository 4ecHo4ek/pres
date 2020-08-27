Public Class data
    Dim l1 As Double ' длина входа маломощного насоса
    Dim l2 As Double ' длина входа мощного насоса
    Dim d1 As Double ' диаметр малого насоса
    Dim d2 As Double ' диаметр большого насоса
    Dim temperatureBeforeHeat As Double ' темпиратура до нагрева
    Dim temperatureAfterHeat As Double ' макс темпиратура нагрева
    Dim s1 As Double ' скорость откачки малого насоса
    Dim s2 As Double ' скорость откачки большого насоса
    Dim pressureBegin As Double ' начальное давление
    Dim qFlows As Double ' пропускная способность течей
    Dim t1 As Double ' время отключения маломощного насоса
    Dim tStartHeatRising As Double ' время включения печи
    Dim tEndHeatRising As Double ' конец набора темпиратуры
    Dim tKatodActivate As Double ' время вкл катода
    Dim tEndHeating As Double ' отключение темпиратуры
    Dim tKatodDisactivate As Double ' отключение катода
    Dim tEnd As Double ' окончание откачки
End Class
