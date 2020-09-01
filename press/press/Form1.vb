Public Class Form1

    Structure TmpParam
        Dim l As Double ' длина соед канала
        Dim d As Double ' димаетр соед канала
        Dim t As Double ' время
        Dim p As Decimal ' давление в системе
        Dim s As Double ' скорость откачки в данный момент
        Dim temperature As Double ' переменная для темпиратуры
        Dim alpha As Double ' задаем альфу
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
        Dim unit As Unit
        unit = MakeUnitFully()
        Dim data = CreateData()
        Dim pressure(data.tEnd + 1) As Decimal ' вероятно убирать
        Dim time(data.tEnd + 1) As Double ' вероятно убирать

        tmp.p = data.pressureBegin
        pressure(0) = tmp.p
        time(0) = 0
        tmp.alpha = data.alpha
        For t = 1 To data.tEnd
            tmp.t = t
            If tmp.t = 1 Then
                tmp.d = data.d1
                tmp.l = data.l1
                tmp.s = data.s1
            ElseIf tmp.t = data.t1 Then
                tmp.d = data.d2
                tmp.l = data.l2
                tmp.s = data.s2
            End If

            ' тут надо что то сделать с h для рунгекутта'
            tmp.p += StartCalculatePressureWithRungeKutt(tmp, data, unit)
            pressure(t) = tmp.p
            time(t) = t

            'удалить
            If tmp.t > data.tStartHeatRising - 1 And tmp.t < data.tStartHeatRising + 10 Then
                Console.WriteLine(tmp.p)
            End If

        Next t


        'просто для наглядности'
        Dim g As Graphics
        g = Me.CreateGraphics
        Dim pe = 400 / (Math.Log(data.pressureBegin) - Math.Log(pressure(data.tEnd)))
        Dim te = 600 / data.tEnd
        For i = 1 To data.tEnd - 1
            If pressure(i) > 0 Then
                g.DrawLine(Pens.Red, CSng(20 + te * time(i)), CSng(20 + pe * (Math.Log(data.pressureBegin) - Math.Log(pressure(i)))),
                 CSng(20 + te * time(i + 1)), CSng(20 + pe * (Math.Log(data.pressureBegin) - Math.Log(pressure(i + 1)))))
            Else
                g.DrawLine(Pens.Green, CSng(20 + te * time(i)), CSng(20 + pe * (Math.Log(data.pressureBegin) - Math.Log(-pressure(i)))),
                 CSng(20 + te * time(i + 1)), CSng(20 + pe * (Math.Log(data.pressureBegin) - Math.Log(-pressure(i + 1)))))
            End If
        Next i
        ' g.DrawLine(Pens.Black, CSng(20 + te * data.t1), CSng(0), CSng(20 + te * data.t1), 800)
        ' g.DrawLine(Pens.Brown, CSng(20 + te * data.tStartHeatRising), CSng(0), CSng(20 + te * data.tStartHeatRising), 800)
        '  g.DrawLine(Pens.Gray, CSng(20 + te * data.tEndHeatRising), CSng(0), CSng(20 + te * data.tEndHeatRising), 800)
        '  g.DrawLine(Pens.Yellow, CSng(20 + te * data.tKatodActivate), CSng(0), CSng(20 + te * data.tKatodActivate), 800)
        ' g.DrawLine(Pens.Pink, CSng(20 + te * data.tKatodActive), CSng(0), CSng(20 + te * data.tKatodActive), 800)
        ' g.DrawLine(Pens.Gold, CSng(20 + te * data.tEndHeating), CSng(0), CSng(20 + te * data.tEndHeating), 800)
        ' g.DrawLine(Pens.Indigo, CSng(20 + te * data.tKatodDisactivate), CSng(0), CSng(20 + te * data.tKatodDisactivate), 800)
        '  g.DrawLine(Pens.Aqua, CSng(20 + te * data.tEnd), CSng(0), CSng(20 + te * data.tEnd), 800)
        '   MsgBox(pressure(t))
    End Sub

    Function CreateData() As Data
        Dim data = New Data
        data.alpha = -2000
        data.d1 = 0.5
        data.d2 = 0.5
        data.l1 = 0.5
        data.l2 = 0.5
        data.pressureBegin = 10000
        data.qFlows = 10 ^ -10
        data.s1 = 25 * 10 ^ -3
        data.s2 = 33 * 10 ^ -2
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

    Function MakeUnitFully() As Unit
        Dim unit = New Unit
        Dim katod = New Katod
        Dim gun = New Gun
        Dim body = New Body
        Dim channel = New Channel
        Dim drain = New Drain
        'заполняем данные корпуса
        unit.body = body
        unit.body.kBeforeHeat = 10 ^ -5
        unit.body.kAfterHeat = 10 ^ -4
        unit.body.qSumAfterHeat = 2 * 10 ^ -11
        unit.body.qSumBeforeHeat = 10 ^ -11
        unit.body.square = 1
        unit.body.volume = 1
        unit.body.qUdel = 10 ^ -7
        'заполняем данные канала
        unit.channel = channel
        unit.channel.kBeforeHeat = 10 ^ -5
        unit.channel.kAfterHeat = 10 ^ -4
        unit.channel.volume = 1
        unit.channel.square = 1
        unit.channel.qUdel = 10 ^ -7
        'заполняем данные коллектора
        unit.drain = drain
        unit.drain.kBeforeHeat = 10 ^ -5
        unit.drain.kAfterHeat = 10 ^ -4
        unit.drain.volume = 1
        unit.drain.square = 1
        unit.drain.qUdel = 10 ^ -7
        'заполняем данные пушки
        unit.gun = gun
        unit.gun.kBeforeHeat = 10 ^ -5
        unit.gun.kAfterHeat = 10 ^ -4
        unit.gun.volume = 1
        unit.gun.square = 1
        unit.gun.qUdel = 10 ^ -7
        'заполняем данные катода
        unit.katod = katod
        unit.katod.kBeforeHeat = 10 ^ -5
        unit.katod.kAfterHeat = 10 ^ -4
        unit.katod.volume = 1
        unit.katod.square = 1
        unit.katod.qUdel = 10 ^ -7
        unit.katod.temperatureMax = 1400
        unit.katod.tRise = 600
        Return unit
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
        If pressure > 1 Then
            Console.WriteLine(pressure)
        End If
        Return pressure
    End Function

    Function CalculatePressureWithRungeKutt(ByVal tmp As TmpParam, ByVal data As Data, ByVal unit As Unit) As Decimal
        Dim sEf As Decimal ' часть с эффективной скоростью откачки
        Dim q As Decimal ' часть с газовыделениями
        Dim flowParam As Decimal ' пропускание щелями
        Dim changingParams = New ChangingParams

        q = 0
        sEf = SEffectiveWithOtherParts(tmp)
        ' пропускающая способность щелей корпуса
        If tmp.t < data.tEndHeating Then
            changingParams.qSumBefore = unit.body.qSumBeforeHeat
            changingParams.qSumAfter = unit.body.qSumAfterHeat
        ElseIf tmp.t >= data.tEndHeating Then
            changingParams.qSumBefore = unit.body.qSumAfterHeat
            changingParams.qSumAfter = unit.body.qSumBeforeHeat
        End If
        ' парциальные потоки от элементов системы
        If tmp.t < data.tStartHeatRising Then ' значения до нагрева
            changingParams.temperatureBegin = data.temperatureBeforeHeat
            q += FlowConstant(tmp, unit.body, changingParams, data)
            q += FlowConstant(tmp, unit.channel, changingParams, data)
            q += FlowConstant(tmp, unit.drain, changingParams, data)
            q += FlowConstant(tmp, unit.gun, changingParams, data)
            q += FlowConstant(tmp, unit.katod, changingParams, data)
            flowParam = FlowParameterWhenConst(tmp, data, changingParams, unit.body)
            'время нагрева камеры
        ElseIf (tmp.t >= data.tStartHeatRising) And (tmp.t <= data.tEndHeatRising) Then
            changingParams.temperatureBegin = data.temperatureBeforeHeat
            changingParams.temperatureAfter = data.temperatureAfterHeat
            changingParams.timeBegin = data.tStartHeatRising
            changingParams.timeEnd = data.tEndHeatRising
            q += FlowDinam(tmp, unit.body, changingParams, data)
            q += FlowDinam(tmp, unit.channel, changingParams, data)
            q += FlowDinam(tmp, unit.drain, changingParams, data)
            q += FlowDinam(tmp, unit.gun, changingParams, data)
            q += FlowDinam(tmp, unit.katod, changingParams, data)
            flowParam = FlowParameterWhenDimanic(tmp, data, changingParams, unit.body)
            ' время до включения катода (до его доп нагрева)
        ElseIf (tmp.t > data.tEndHeatRising) And (tmp.t < data.tKatodActivate) Then
            changingParams.temperatureBegin = data.temperatureAfterHeat
            q += FlowConstant(tmp, unit.body, changingParams, data)
            q += FlowConstant(tmp, unit.channel, changingParams, data)
            q += FlowConstant(tmp, unit.drain, changingParams, data)
            q += FlowConstant(tmp, unit.gun, changingParams, data)
            q += FlowConstant(tmp, unit.katod, changingParams, data)
            flowParam = FlowParameterWhenConst(tmp, data, changingParams, unit.body)
            ' промежуток времени до полной остановки подогрева камеры
        ElseIf tmp.t >= data.tKatodActivate And tmp.t < data.tEndHeating Then
            changingParams.temperatureBegin = data.temperatureAfterHeat
            q += FlowConstant(tmp, unit.body, changingParams, data)
            q += FlowConstant(tmp, unit.channel, changingParams, data)
            q += FlowConstant(tmp, unit.drain, changingParams, data)
            flowParam = FlowParameterWhenConst(tmp, data, changingParams, unit.body)
            ' оставшееся время остывания
        ElseIf tmp.t > data.tEndHeating And tmp.t <= data.tEnd Then
            changingParams.temperatureBegin = data.temperatureAfterHeat
            changingParams.temperatureAfter = data.temperatureBeforeHeat
            changingParams.timeBegin = data.tEndHeating
            changingParams.timeEnd = data.tEnd
            q += FlowDinam(tmp, unit.body, changingParams, data)
            q += FlowDinam(tmp, unit.channel, changingParams, data)
            q += FlowDinam(tmp, unit.drain, changingParams, data)
            q += FlowDinam(tmp, unit.gun, changingParams, data)
            q += FlowDinam(tmp, unit.katod, changingParams, data)
            flowParam = FlowParameterWhenDimanic(tmp, data, changingParams, unit.body)
        End If
        'начиная с прогрева катода, он и пушка уходят в отдельную ветку
        If tmp.t >= data.tKatodActivate And tmp.t <= data.tKatodActive Then ' разогрев катода до макс
            changingParams.timeBegin = data.tKatodActivate
            changingParams.timeEnd = data.tKatodActive
            changingParams.temperatureBegin = data.temperatureAfterHeat
            changingParams.temperatureAfter = unit.katod.temperatureMax
            q += FlowDinam(tmp, unit.katod, changingParams, data)
            changingParams.temperatureAfter = unit.katod.temperatureMax * 0.8
            q += FlowDinam(tmp, unit.gun, changingParams, data)
        ElseIf tmp.t > data.tKatodActive And tmp.t <= data.tKatodDisactivate Then
            changingParams.temperatureBegin = unit.katod.temperatureMax * 0.8
            q += FlowConstant(tmp, unit.gun, changingParams, data)
            changingParams.temperatureBegin = unit.katod.temperatureMax
            q += FlowConstant(tmp, unit.katod, changingParams, data)
        ElseIf tmp.t > data.tKatodDisactivate Then
            changingParams.timeBegin = data.tKatodDisactivate
            changingParams.timeEnd = data.tEnd
            changingParams.temperatureBegin = unit.katod.temperatureMax
            changingParams.temperatureAfter = data.temperatureBeforeHeat
            q += FlowDinam(tmp, unit.katod, changingParams, data)
            changingParams.temperatureBegin = unit.katod.temperatureMax * 0.8
            q += FlowDinam(tmp, unit.gun, changingParams, data)
        End If
        Return (sEf + q + flowParam) / unit.body.volume
    End Function

    'рассчет потока натекания через стенки при постоянной темпиратуре
    Function FlowParameterWhenConst(ByVal tmp As TmpParam, ByVal data As Data, ByVal unit As ChangingParams, ByVal body As Body) As Decimal
        Dim flowParam As Decimal

        If tmp.t < data.tEndHeating Then
            unit.qSumBefore = body.qSumBeforeHeat
        Else
            unit.qSumBefore = body.qSumAfterHeat
        End If
        flowParam = unit.qSumBefore * (data.pressureBegin - tmp.p)
        Return flowParam
    End Function

    'рассчет потока натекания через стенки при изменяемой темпиратуре
    Function FlowParameterWhenDimanic(ByVal tmp As TmpParam, ByVal data As Data, ByVal unit As ChangingParams, ByVal body As Body) As Decimal
        Dim qSumKoef As Decimal  ' тут есть ошибка '

        If tmp.t < data.tEndHeating Then
            unit.qSumBefore = body.qSumBeforeHeat
            unit.qSumAfter = body.qSumAfterHeat
        Else
            unit.qSumBefore = body.qSumAfterHeat
            unit.qSumAfter = body.qSumBeforeHeat
        End If
        qSumKoef = ((unit.qSumAfter - unit.qSumBefore) * (tmp.t - unit.timeEnd) / (unit.timeEnd - unit.timeBegin)) * (data.pressureBegin - tmp.p)
        Return qSumKoef
    End Function

    Function FlowConstant(ByVal tmp As TmpParam, ByVal element As Element, ByVal unit As ChangingParams, ByVal data As Data) As Decimal
        Dim constParam As Decimal
        Dim answer As Decimal
        Dim param1 As Decimal

        If tmp.t < data.tEndHeating Then
            unit.kBeforeHeat = element.kBeforeHeat
        Else
            unit.kBeforeHeat = element.kAfterHeat
        End If
        param1 = element.square * element.qUdel * element.kBeforeHeat ' тут с к от времени менять
        constParam = Math.E ^ (tmp.alpha * ((1 / unit.temperatureBegin) - 1 / 293))
        answer = param1 * constParam * Math.E ^ (-element.kBeforeHeat * tmp.t * constParam)
        Return answer
    End Function

    Function FlowDinam(ByVal tmp As TmpParam, ByVal element As Element, ByVal unit As ChangingParams, ByVal data As Data) As Decimal
        Dim answer As Decimal
        Dim tempParam As Decimal
        Dim koefParam As Decimal
        Dim dinamParam As Decimal

        If tmp.t < data.tEndHeating Then
            unit.kBeforeHeat = element.kBeforeHeat
            unit.kAfterHeat = element.kAfterHeat
        Else
            unit.kBeforeHeat = element.kAfterHeat
            unit.kAfterHeat = element.kBeforeHeat
        End If
        tempParam = Math.E ^ (tmp.alpha * (1 / (unit.temperatureBegin + (unit.temperatureAfter - unit.temperatureBegin) * (tmp.t - unit.timeBegin) / (unit.timeEnd - unit.timeBegin))) - 1 / 293) ' тут нужна степень по идее у первого множителя по времени
        koefParam = unit.kBeforeHeat + (unit.kAfterHeat - unit.kBeforeHeat) * (tmp.t - unit.timeBegin) / (unit.timeEnd - unit.timeBegin)
        dinamParam = element.square * element.qUdel * koefParam * tempParam
        answer = dinamParam * Math.E ^ (-koefParam * tmp.t * tempParam) 'тут быть внимательней со стеменью в tempParam, тут была -2'
        Return answer
    End Function

    'эффективная скорость откачки
    Function SEffectiveWithOtherParts(ByVal tmp As TmpParam) As Decimal ' высчитываем компоненту с эффективной скоростью откачки
        Dim koef As Decimal
        Dim sEf As Decimal
        Dim k1 As Double ' просто разгружаем решение
        Dim k2 As Double ' аналогично

        k1 = 1.36 * 10 ^ 3
        k2 = tmp.d ^ 3
        koef = (1 + 1.9 * (10 ^ 4) * tmp.d * tmp.p) / (1 + 2.35 * (10 ^ 4) * tmp.d * tmp.p)
        sEf = (tmp.s * ((k1 * tmp.p * (tmp.d * k2) / tmp.l) + (121 * k2 / tmp.l)) * koef) /
            (tmp.s + ((k1 * tmp.p * (tmp.d * k2) / tmp.l) + (121 * k2 / tmp.l) * koef))
        Return (-1) * tmp.p * sEf
    End Function

End Class
