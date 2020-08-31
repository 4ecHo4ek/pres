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

    Function setData() As Data
        Dim data = New Data
        data.alpha = -2000
        data.d1 = 0.5
        data.d2 = 0.5
        data.l1 = 0.5
        data.l2 = 0.5
        data.pressureBegin = 10000
        data.qFlows = 10 ^ -10
        data.s1 = 25 * 10 ^ -3
        data.s2 = 33 * 10 ^ -1
        data.t1 = 1500
        data.temperatureBeforeHeat = 293
        data.temperatureAfterHeat = 793
        data.tStartHeatRising = 3000
        data.tEndHeatRising = 5000
        data.tKatodActivate = 7000
        data.tKatodActive = 8000
        data.tEndHeating = 10000
        data.tKatodDisactivate = 11000
        data.tEnd = 13000
        Return data
    End Function

    Function setUnit() As Unit
        Dim unit = New Unit
        Dim katod = New Katod
        Dim gun = New Gun
        Dim body = New Body
        Dim channel = New Channel
        Dim drain = New Drain
        unit.body = body
        unit.channel = channel
        unit.drain = drain
        unit.gun = gun
        unit.katod = katod
        unit.body.qBegin = 10 ^ -5
        unit.body.qEnd = 10 ^ -4
        unit.body.qSumAfterHeat = 10 ^ -13
        unit.body.qSumBeforeHeat = 10 ^ -10
        unit.body.square = 1
        unit.body.volume = 1
        unit.channel.qBegin = 10 ^ -5
        unit.channel.square = 10 ^ -4
        unit.channel.volume = 1
        unit.channel.square = 1
        unit.drain.qBegin = 10 ^ -5
        unit.drain.square = 10 ^ -4
        unit.drain.volume = 1
        unit.drain.square = 1
        unit.gun.qBegin = 10 ^ -5
        unit.gun.square = 10 ^ -4
        unit.gun.volume = 1
        unit.gun.square = 1
        unit.katod.qBegin = 10 ^ -5
        unit.katod.square = 10 ^ -4
        unit.katod.volume = 1
        unit.katod.square = 1
        unit.katod.temperatureMax = 1400
        unit.katod.tRise = 600
        unit.gun.temperatureMax = 1400
        unit.gun.tRise = 600
        Return unit
    End Function

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim tmp = New TmpParam
        Dim t As Double
        Dim unit As Unit
        unit = setUnit()
        Dim data = setData()

        Dim pressure(data.tEnd + 1) As Decimal ' вероятно убирать
        Dim time(data.tEnd + 1) As Double ' вероятно убирать

        tmp.p = data.pressureBegin
        '  pressure(0) = p может и не понадобится
        '  time(0) = 0  может и не понадобится

        For t = 1 To data.tEnd
            tmp.t = t

            If tmp.t < data.t1 Then ' перепроверить тут
                tmp.d = data.d1
                tmp.l = data.l1
                tmp.s = data.s1
                tmp.temperature = data.temperatureBeforeHeat
            ElseIf tmp.t >= data.t1 And tmp.t < data.tEndHeatRising Then
                tmp.d = data.d2
                tmp.l = data.l2
                tmp.s = data.s2
            ElseIf tmp.t >= data.tEndHeatRising And tmp.t < data.tEndHeating Then
                tmp.temperature = data.temperatureAfterHeat
            Else
                Dim tTmp = data.temperatureAfterHeat
                data.temperatureAfterHeat = data.temperatureBeforeHeat
                data.temperatureBeforeHeat = tTmp
            End If

            ' тут надо что то сделать с h для рунгекутта'
            tmp.p += StartCalculatePressureWithRungeKutt(tmp, data, unit)
            pressure(t) = tmp.p
            time(t) = tmp.t

            'удалить
            If tmp.t Mod 1000 = 0 Then
                '     Console.WriteLine(tmp.p)
            End If

        Next t

        MsgBox(pressure(t))
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
        Dim flowParam As Decimal ' пропускание щелями
        sEf = SEffectiveWithOtherParts(tmp)

        Select Case tmp.t
            Case tmp.t < data.tEndHeating
                changingParams.qSumBefore = unit.body.qSumBeforeHeat
                changingParams.qSumAfter = unit.body.qSumAfterHeat
            Case tmp.t >= data.tEndHeating
                changingParams.qSumBefore = unit.body.qSumAfterHeat
                changingParams.qSumAfter = unit.body.qSumBeforeHeat
        End Select

        Select Case tmp.t
            Case tmp.t < data.tStartHeatRising ' значения до нагрева
                changingParams = SetStructForElementConst(changingParams, data, unit.body)
                q += FlowConstant(tmp, data, changingParams)
                changingParams = SetStructForElementConst(changingParams, data, unit.channel)
                q += FlowConstant(tmp, data, changingParams)
                changingParams = SetStructForElementConst(changingParams, data, unit.drain)
                q += FlowConstant(tmp, data, changingParams)
                changingParams = SetStructForElementConst(changingParams, data, unit.gun)
                q += FlowConstant(tmp, data, changingParams)
                changingParams = SetStructForElementConst(changingParams, data, unit.katod)
                q += FlowConstant(tmp, data, changingParams)
                flowParam = FlowParameterWhenConst(tmp, data, changingParams)
            Case (tmp.t >= data.tStartHeatRising) And (tmp.t <= data.tEndHeatRising) ' время нагрева камеры
                changingParams = SetStructForElementDinam(changingParams, data, unit.body)
                q += FlowConstant(tmp, data, changingParams)
                changingParams = SetStructForElementDinam(changingParams, data, unit.channel)
                q += FlowConstant(tmp, data, changingParams)
                changingParams = SetStructForElementDinam(changingParams, data, unit.drain)
                q += FlowConstant(tmp, data, changingParams)
                changingParams = SetStructForElementDinam(changingParams, data, unit.gun)
                q += FlowConstant(tmp, data, changingParams)
                changingParams = SetStructForElementDinam(changingParams, data, unit.katod)
                q += FlowConstant(tmp, data, changingParams)
                flowParam = FlowParameterWhenDimanic(tmp, data, changingParams)
            Case tmp.t < data.tKatodActivate ' время до включения катода (до его доп нагрева)
                changingParams = SetStructForElementConst(changingParams, data, unit.body)
                q += FlowConstant(tmp, data, changingParams)
                changingParams = SetStructForElementConst(changingParams, data, unit.channel)
                q += FlowConstant(tmp, data, changingParams)
                changingParams = SetStructForElementConst(changingParams, data, unit.drain)
                q += FlowConstant(tmp, data, changingParams)
                changingParams = SetStructForElementConst(changingParams, data, unit.gun)
                q += FlowConstant(tmp, data, changingParams)
                changingParams = SetStructForElementConst(changingParams, data, unit.katod)
                q += FlowConstant(tmp, data, changingParams)
                flowParam = FlowParameterWhenConst(tmp, data, changingParams)
            Case tmp.t >= data.tKatodActivate And tmp.t < data.tEndHeating ' промежуток времени до полной остановки подогрева камеры
                changingParams = SetStructForElementConst(changingParams, data, unit.body)
                q += FlowConstant(tmp, data, changingParams)
                changingParams = SetStructForElementConst(changingParams, data, unit.channel)
                q += FlowConstant(tmp, data, changingParams)
                changingParams = SetStructForElementConst(changingParams, data, unit.drain)
                q += FlowConstant(tmp, data, changingParams)
                flowParam = FlowParameterWhenConst(tmp, data, changingParams)
            Case tmp.t < data.tEnd ' оставшееся время остывания
                changingParams = SetStructForElementDinam(changingParams, data, unit.body)
                q += FlowConstant(tmp, data, changingParams)
                changingParams = SetStructForElementDinam(changingParams, data, unit.channel)
                q += FlowConstant(tmp, data, changingParams)
                changingParams = SetStructForElementDinam(changingParams, data, unit.drain)
                q += FlowConstant(tmp, data, changingParams)
                flowParam = FlowParameterWhenDimanic(tmp, data, changingParams)
        End Select
        'начиная с прогрева катода, он и пушка уходят в отдельную ветку
        Select Case tmp.t
            Case tmp.t >= data.tKatodActivate And tmp.t <= data.tKatodActive ' разогрев катода до макс
                changingParams = SetStructForElementDinam(changingParams, data, unit.gun)
                changingParams.temperatureBegin = data.temperatureAfterHeat * 0.9
                changingParams.temperatureAfter = unit.katod.temperatureMax
                q += FlowConstant(tmp, data, changingParams)
                changingParams = SetStructForElementDinam(changingParams, data, unit.katod)
                changingParams.temperatureBegin = data.temperatureAfterHeat
                changingParams.temperatureAfter = unit.katod.temperatureMax
                q += FlowConstant(tmp, data, changingParams)
            Case tmp.t > data.tKatodActive And tmp.t <= data.tKatodDisactivate
                changingParams = SetStructForElementConst(changingParams, data, unit.gun)
                changingParams.temperatureBegin = unit.katod.temperatureMax
                q += FlowConstant(tmp, data, changingParams)
                changingParams = SetStructForElementConst(changingParams, data, unit.katod)
                changingParams.temperatureBegin = unit.katod.temperatureMax
                q += FlowConstant(tmp, data, changingParams)
            Case tmp.t > data.tKatodDisactivate
                changingParams = SetStructForElementDinam(changingParams, data, unit.gun)
                changingParams.temperatureBegin = unit.katod.temperatureMax * 0.9
                changingParams.temperatureAfter = data.temperatureAfterHeat
                q += FlowConstant(tmp, data, changingParams)
                changingParams = SetStructForElementDinam(changingParams, data, unit.katod)
                changingParams.temperatureBegin = unit.katod.temperatureMax * 0.9
                changingParams.temperatureAfter = data.temperatureAfterHeat
                q += FlowConstant(tmp, data, changingParams)
        End Select
        'здесь пропускающую способность течей указываем только для корпуса'
        Return (sEf + q + flowParam) / unit.body.volume
    End Function

    Function SetStructForBody(ByVal tmp As ChangingParams, ByVal data As Data, ByVal unit As Unit, ByVal flag As Boolean) As ChangingParams ' если ниже работает - удалить

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

    Function SetStructForElementConst(ByVal tmp As ChangingParams, ByVal data As Data, ByVal unit As Element) As ChangingParams ' вносим в структуру данные для последующего расчета для каждого компанента
        ' если это константа, то заполнябтся параметры с Before'
        tmp.volume = unit.volume
        tmp.square = unit.square
        tmp.kBeforeHeat = unit.qBegin
        tmp.temperatureBegin = data.temperatureBeforeHeat
        Return tmp
    End Function

    Function SetStructForElementDinam(ByVal tmp As ChangingParams, ByVal data As Data, ByVal unit As Element) As ChangingParams
        tmp.volume = unit.volume
        tmp.square = unit.square
        tmp.kBeforeHeat = unit.qBegin
        tmp.kAfterHeat = unit.qEnd
        Return tmp
    End Function

    Function FlowParameterWhenConst(ByVal tmp As TmpParam, ByVal data As Data, ByVal unit As ChangingParams) As Decimal
        Dim flowParam = unit.qSumBefore * (data.pressureBegin - tmp.p)
        Return flowParam
    End Function

    Function FlowParameterWhenDimanic(ByVal tmp As TmpParam, ByVal data As Data, ByVal unit As ChangingParams) As Decimal
        Dim qSumKoef = ((unit.qSumAfter - unit.kBeforeHeat) * (tmp.t - unit.timeEnd) / (unit.timeEnd - unit.timeBegin)) * (data.pressureBegin - tmp.p)
        Return qSumKoef
    End Function

    Function FlowConstant(ByVal tmp As TmpParam, ByVal data As Data, ByVal unit As ChangingParams) As Decimal
        Dim constParam As Decimal
        Dim answer As Decimal
        Dim param1 = unit.square * unit.q * unit.kBeforeHeat

        constParam = Math.E ^ (data.alpha * ((1 / tmp.temperature) - 1 / 293))
        answer = param1 * constParam * Math.E ^ (-unit.kBeforeHeat * tmp.t * constParam) * unit.volume
        Return answer
    End Function

    Function FlowDinam(ByVal tmp As TmpParam, ByVal data As Data, ByVal unit As ChangingParams) As Decimal
        Dim answer As Decimal
        Dim tempParam = Math.E ^ (data.alpha * (1 / (unit.temperatureBegin + (unit.temperatureAfter - unit.temperatureBegin) / (unit.timeEnd - unit.timeBegin) *
                        (tmp.t - unit.timeBegin)) ^ 2 - 1 / 293))
        Dim koefParam = unit.kBeforeHeat + (unit.kAfterHeat - unit.kBeforeHeat) * (tmp.t - unit.timeBegin) / (unit.timeEnd - unit.timeBegin)
        Dim dinamParam = unit.square * unit.q * koefParam * tempParam

        answer = dinamParam * Math.E ^ (-koefParam * tmp.t * tempParam) 'тут быть внимательней со стеменью в tempParam, тут была -2'
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
        'удалить
        If tmp.t Mod 1000 = 0 Then
            Console.WriteLine(f)
        End If
        Return f
    End Function
End Class
