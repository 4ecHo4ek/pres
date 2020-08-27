Public Class Form1

    Structure TmpParam
        Dim l As Double ' длина соед канала
        Dim d As Double ' димаетр соед канала
        Dim t As Double ' время
        Dim p As Decimal ' давление в системе
        Dim s As Double ' скорость откачки в данный момент
    End Structure

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim tmp = New TmpParam
        Dim t As Double
        Dim unit = MakeUnitFully()
        Dim data = CreateData()
        Dim p = data.pressureBegin
        Dim pressure(data.tEnd) As Decimal ' вероятно убирать
        Dim time(data.tEnd) As Double ' вероятно убирать

        '  pressure(0) = p может и не понадобится
        '  time(0) = 0  может и не понадобится
        For t = 1 To data.tEnd
            Select Case t
                Case 0
                    tmp.d = data.d1
                    tmp.l = data.l1
                    tmp.s = data.s1
                Case t = data.t1
                    tmp.d = data.d2
                    tmp.l = data.l2
                    tmp.s = data.s2
            End Select
            tmp.t = t
            ' тут надо что то сделать с h для рунгекутта'
            tmp.p += StartCalculatePressureWithRungeKutt(tmp, data, unit)
            pressure(t) = tmp.p
            time(t) = t
        Next t



    End Sub

    Function CreateData() As Data
        'здесь вносим инфу про дату'
    End Function

    Function MakeUnitFully() As Unit
        ' здесь необходимо для каждого параметра задать свои значения ' 
    End Function


    Dim h As Double = 1 ' изменить!!!!! '

    Function StartCalculatePressureWithRungeKutt(ByVal tmp As TmpParam, ByVal data As Data, ByVal unit As Unit) As Decimal
        Dim pressure As Decimal
        Dim k1, k2, k3, k4 As Decimal

        k1 = CalculatePressureWithRungeKutt(tmp, data, unit)
        tmp.t += h / 2
        tmp.p += (h * k1) / 2
        k2 = CalculatePressureWithRungeKutt(tmp, data, unit)
        tmp.t += h / 2
        tmp.p += (h * k2) / 2
        k3 = CalculatePressureWithRungeKutt(tmp, data, unit)
        tmp.t += h
        tmp.p += h * k3
        k4 = CalculatePressureWithRungeKutt(tmp, data, unit)
        pressure = h * (k1 + 2 * k2 + 2 * k3 + k4) / 6

        Return pressure
    End Function

    Function CalculatePressureWithRungeKutt(ByVal tmp As TmpParam, ByVal data As Data, ByVal unit As Unit) As Decimal
        Dim sEf As Decimal ' часть с эффективной скоростью откачки
        Dim q As Decimal ' часть с газовыделениями

        sEf = SEffectiveWithOtherParts(tmp) / unit.FindAllValume


        Select Case tmp.t
            Case tmp.t < data.tStartHeatRising
                'все константы'
            Case (tmp.t >= data.tStartHeatRising) And (tmp.t <= data.tEndHeatRising)
                'первый набор темпиратуры'
            Case tmp.t < data.tKatodActivate
                'разогрев катода'
            Case tmp.t >= data.tEndHeating
                'отключаем нагрев, следим за катодом и его областью'
            Case tmp.t <= data.tKatodDisactivate
                'полное охлаждение системы'
        End Select
        ' тут ищем Q от всех элементов и складываем


        Return (sEf + q)
    End Function

    Function SEffectiveWithOtherParts(ByVal tmp As TmpParam) As Decimal ' высчитываем компоненту с эффективной скоростью откачки
        Dim koef As Decimal
        Dim sEf As Decimal
        Dim k1 As Double = 1.36 * 10 ^ 3 ' просто разгружаем решение
        Dim k2 As Double = tmp.d ^ 3 ' аналогично
        Dim f As Decimal

        koef = (1 + 1.9 * (10 ^ 4) * tmp.d * tmp.p) / (1 + 2.35 * (10 ^ 4) * tmp.d * tmp.p)
        sEf = (tmp.s * ((k1 * tmp.p * (tmp.d * k2) / tmp.l) + (121 * k2 / tmp.l)) * koef) /
            (tmp.s + ((k1 * tmp.p * (tmp.d * k2) / tmp.l) + (121 * k2 / tmp.l) * koef))
        f = (-1) * tmp.p * sEf

        Return f
    End Function
End Class
