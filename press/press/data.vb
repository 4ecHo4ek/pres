Public Class Data
    Public l1 As Double ' длина входа маломощного насоса
    Public l2 As Double ' длина входа мощного насоса
    Public d1 As Double ' диаметр малого насоса
    Public d2 As Double ' диаметр большого насоса
    Public temperatureBeforeHeat As Double ' темпиратура до нагрева
    Public temperatureAfterHeat As Double ' макс темпиратура нагрева
    Public alpha As Double ' параметр альфа (порядка -2000)
    Public s1 As Double ' скорость откачки малого насоса
    Public s2 As Double ' скорость откачки большого насоса
    Public pressureBegin As Double ' начальное давление
    Public qFlows As Double ' пропускная способность течей
    Public t1 As Double ' время отключения маломощного насоса
    Public tStartHeatRising As Double ' время включения печи
    Public tEndHeatRising As Double ' конец набора темпиратуры
    Public tKatodActivate As Double ' время вкл катода
    Public tEndHeating As Double ' отключение темпиратуры
    Public tKatodDisactivate As Double ' отключение катода
    Public tEnd As Double ' окончание откачки
End Class
