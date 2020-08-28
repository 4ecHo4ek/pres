Public Class Form1

    Structure TmpParam
        Dim l As Double ' длина соед канала
        Dim d As Double ' димаетр соед канала
        Dim t As Double ' время
        Dim p As Decimal ' давление в системе
        Dim s As Double ' скорость откачки в данный момент
        Dim temperature As Double ' переменная для темпиратуры
    End Structure

    Structure ChangingParams
        Dim temperatureBegin As Double ' темпиратура до начала изменения
        Dim temperatureAfter As Double ' темпиратура после изменения
        Dim kBeforeHeat As Decimal ' коэф. скорости газовыделения до изменения
        Dim kAfterHeat As Decimal ' коэф. скорости газовыд. после изменения
        Dim timeBegin As Double ' время начало изменения
        Dim timeEnd As Double ' время конец изменения
        Dim qSumBefore As Decimal ' суммарная пропускающая способность течей до
        Dim qSumAfter As Decimal ' суммарная пропускающая способность течей после
        Dim square As Double ' площадь юнита
        Dim volume As Double ' объем юнита
        Dim q As Decimal ' удельное газовыделение стенок
        'меняется ли оно?'
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
                    tmp.temperature = data.temperatureBeforeHeat
                Case t = data.t1
                    tmp.d = data.d2
                    tmp.l = data.l2
                    tmp.s = data.s2
                Case t = data.tEndHeatRising
                    tmp.temperature = data.temperatureAfterHeat
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
        Dim q As Decimal = 0 ' часть с газовыделениями
        Dim changingParams = New ChangingParams
        Dim constFlag As Boolean ' если флаг, значит константа, иначе переменная
        sEf = SEffectiveWithOtherParts(tmp) / unit.FindAllValume

        'для всех элементов кроме корпуса пропускающая способность течей = 0'
        'и скорей всего и объем будет только тела указан '

        'заполнение структур данных для каждого объекта???'


        Select Case tmp.t
            Case tmp.t < data.tStartHeatRising ' значения до нагрева
                constFlag = True
                ' так для каждого из блоков для каждого времени '
                ' не забыть про особое внимание катоду'
                changingParams = SetStructForBody(changingParams, data, unit, constFlag)
                q += FlowConstant(tmp, data, changingParams)
            Case (tmp.t >= data.tStartHeatRising) And (tmp.t <= data.tEndHeatRising) ' время нагрева камеры
                'первый набор темпиратуры'
            Case tmp.t < data.tKatodActivate ' время до включения катода (до его доп нагрева)
                'разогрев катода'
            Case tmp.t >= data.tEndHeating ' время до полного отключения тепла (остывание)
                'отключаем нагрев, следим за катодом и его областью'
            Case tmp.t <= data.tKatodDisactivate ' отклчюение катода
                'полное охлаждение системы'
        End Select
        ' тут ищем Q от всех элементов и складываем


        Return (sEf + q)
    End Function

    Function SetStructForBody(ByVal tmp As ChangingParams, ByVal data As Data, ByVal unit As Unit, ByVal flag As Boolean) As ChangingParams ' вносим в структуру данные для последующего расчета для каждого компанента

        ' если это константа, то заполнябтся параметры с Before'
        tmp.volume = unit.body.volume
        tmp.square = unit.body.square
        If flag Then
            tmp.kBeforeHeat = unit.body.qBegin
            tmp.qSumBefore = unit.body.qSumBeforeHeat
        Else
            tmp.kBeforeHeat = unit.body.qBegin
            tmp.kAfterHeat = unit.body.qEnd
            tmp.qSumBefore = unit.body.qSumBeforeHeat
            tmp.qSumAfter = unit.body.qSumAfterHeat
            tmp.temperatureBegin = data.temperatureBeforeHeat
            tmp.temperatureAfter = data.temperatureAfterHeat
        End If

        Return tmp
    End Function

    Function SetStructForChannel(ByVal tmp As ChangingParams, ByVal data As Data, ByVal unit As Unit, ByVal flag As Boolean) As ChangingParams ' вносим в структуру данные для последующего расчета для каждого компанента

        ' если это константа, то заполнябтся параметры с Before'
        tmp.volume = unit.channel.volume
        tmp.square = unit.channel.square
        If flag Then
            tmp.kBeforeHeat = unit.channel.qBegin
            tmp.qSumBefore = unit.channel.qSumBeforeHeat
        Else
            tmp.kBeforeHeat = unit.channel.qBegin
            tmp.kAfterHeat = unit.channel.qEnd
            tmp.qSumBefore = unit.channel.qSumBeforeHeat
            tmp.qSumAfter = unit.channel.qSumAfterHeat
            tmp.temperatureBegin = data.temperatureBeforeHeat
            tmp.temperatureAfter = data.temperatureAfterHeat
        End If

        Return tmp
    End Function

    Function SetStructForDrain(ByVal tmp As ChangingParams, ByVal data As Data, ByVal unit As Unit, ByVal flag As Boolean) As ChangingParams ' вносим в структуру данные для последующего расчета для каждого компанента

        ' если это константа, то заполнябтся параметры с Before'
        tmp.volume = unit.drain.volume
        tmp.square = unit.drain.square
        If flag Then
            tmp.kBeforeHeat = unit.drain.qBegin
            tmp.qSumBefore = unit.drain.qSumBeforeHeat
        Else
            tmp.kBeforeHeat = unit.drain.qBegin
            tmp.kAfterHeat = unit.drain.qEnd
            tmp.qSumBefore = unit.drain.qSumBeforeHeat
            tmp.qSumAfter = unit.drain.qSumAfterHeat
            tmp.temperatureBegin = data.temperatureBeforeHeat
            tmp.temperatureAfter = data.temperatureAfterHeat
        End If

        Return tmp
    End Function

    Function SetStructForGun(ByVal tmp As ChangingParams, ByVal data As Data, ByVal unit As Unit, ByVal flag As Boolean) As ChangingParams ' вносим в структуру данные для последующего расчета для каждого компанента

        ' если это константа, то заполнябтся параметры с Before'
        tmp.volume = unit.gun.volume
        tmp.square = unit.gun.square
        If flag Then
            tmp.kBeforeHeat = unit.gun.qBegin
            tmp.qSumBefore = unit.gun.qSumBeforeHeat
        Else
            tmp.kBeforeHeat = unit.gun.qBegin
            tmp.kAfterHeat = unit.gun.qEnd
            tmp.qSumBefore = unit.gun.qSumBeforeHeat
            tmp.qSumAfter = unit.gun.qSumAfterHeat
            tmp.temperatureBegin = data.temperatureBeforeHeat
            tmp.temperatureAfter = data.temperatureAfterHeat
        End If

        Return tmp
    End Function

    Function SetStructForKatod(ByVal tmp As ChangingParams, ByVal data As Data, ByVal unit As Unit, ByVal flag As Boolean) As ChangingParams ' вносим в структуру данные для последующего расчета для каждого компанента

        ' если это константа, то заполнябтся параметры с Before'
        tmp.volume = unit.katod.volume
        tmp.square = unit.katod.square
        If flag Then
            tmp.kBeforeHeat = unit.katod.qBegin
            tmp.qSumBefore = unit.katod.qSumBeforeHeat
        Else
            tmp.kBeforeHeat = unit.katod.qBegin
            tmp.kAfterHeat = unit.katod.qEnd
            tmp.qSumBefore = unit.katod.qSumBeforeHeat
            tmp.qSumAfter = unit.katod.qSumAfterHeat
            tmp.temperatureBegin = data.temperatureBeforeHeat
            tmp.temperatureAfter = data.temperatureAfterHeat
        End If

        Return tmp
    End Function

    Function FlowConstant(ByVal tmp As TmpParam, ByVal data As Data, ByVal unit As ChangingParams) As Decimal
        Dim constParam As Decimal
        Dim answer As Decimal
        Dim param1 = unit.square * unit.q * unit.kBeforeHeat
        Dim flowParam = unit.qSumBefore * (data.pressureBegin - tmp.p)

        constParam = Math.E ^ (data.alpha * ((1 / tmp.temperature) - 1 / 293))
        answer = (param1 * constParam * Math.E ^ (-unit.kBeforeHeat * tmp.t * constParam) + flowParam) * unit.volume

        Return answer
    End Function

    Function FlowDinam(ByVal tmp As TmpParam, ByVal data As Data, ByVal unit As ChangingParams) As Decimal
        Dim answer As Decimal
        Dim tempParam = Math.E ^ (data.alpha * (1 / (unit.temperatureBegin + (unit.temperatureAfter - unit.temperatureBegin) / (unit.timeEnd - unit.timeBegin) *
                        (tmp.t - unit.timeBegin)) ^ 2 - 1 / 293))
        Dim koefParam = unit.kBeforeHeat + (unit.kAfterHeat - unit.kBeforeHeat) * (tmp.t - unit.timeBegin) / (unit.timeEnd - unit.timeBegin)
        Dim qSumKoef = ((unit.qSumAfter - unit.kBeforeHeat) * (tmp.t - unit.timeEnd) / (unit.timeEnd - unit.timeBegin)) * (data.pressureBegin - tmp.p)
        Dim dinamParam = unit.square * unit.q * koefParam * tempParam

        answer = dinamParam * Math.E ^ (-qSumKoef * tmp.t * tempParam) + qSumKoef 'тут быть внимательней со стеменью в tempParam, тут была -2'

        Return answer
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
