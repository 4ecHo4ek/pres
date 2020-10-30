Public Class CalculatingPart

    Structure TmpParam
        Dim l As Double ' длина соед канала
        Dim d As Double ' димаетр соед канала
        Dim t As Double ' время
        Dim p As Decimal ' давление в системе
        Dim s As Double ' скорость откачки в данный момент
        Dim temperature As Double ' переменная для темпиратуры
        Dim alpha As Double ' задаем альфу
        Dim q As Decimal ' поток
        Dim k As Decimal ' скорость газовыделения
    End Structure

    Structure ChangingParams
        Dim temperatureBegin As Double ' темпиратура до начала изменения
        Dim temperatureAfter As Double ' темпиратура после изменения
        Dim timeBegin As Double ' время начало изменения
        Dim timeEnd As Double ' время конец изменения
        Dim square As Double ' площадь юнита
        Dim volume As Double ' объем юнита
    End Structure


    Function Calculatings()
        Dim data = New Data
        Dim pressure() As Double
        Dim t As Double
        Dim changingParam = New ChangingParams
        Dim unit = New Unit
        Dim tmp = New TmpParam
        Dim write = New WriteAndReadToFile
        Dim text As String

        data = write.SetStructForCalcPressAsData(data)
        unit = write.SetStructForCalcPressAsUnit(unit)
        tmp.p = data.pressureBegin
        tmp.alpha = data.alpha

        ReDim pressure(data.tEnd)
        pressure(0) = data.pressureBegin
        t = 0
        Try
            While t < data.tEnd
                t += data.h * 2
                tmp.t = t
                If t < data.t1 Then
                    tmp.d = data.d1
                    tmp.l = data.l1
                    If tmp.p > data.minPreVac Then
                        tmp.s = data.s1
                    Else
                        tmp.s = 0
                    End If
                Else
                    If t > data.t1 And tmp.p > data.minPreVac Then
                        write.WriteLogs("Минимальное давление для включения вакуумного насоса не достигнуто")
                        WritePressure(data, pressure, tmp.t, write)
                        Exit Function
                    End If
                    tmp.d = data.d2
                    tmp.l = data.l2
                    tmp.s = data.s2
                End If
                tmp.p += StartCalculatePressureWithRungeKutt(changingParam, tmp, data, unit)
                pressure(t) = tmp.p
                f1.ProgressBar1.Value = 7 * t / data.tEnd
            End While
            WritePressure(data, pressure, tmp.t, write)
        Catch ex As Exception
            text = "ошибка вычисления давления на " & CStr(t) & " cек."
            write.WriteLogs(text)
        End Try
        write.WriteLogs("Вычисления успешно завершены")
        write.WriteToSavingPressureFile()
        Return pressure(data.tEnd)
    End Function

    Function WritePressure(ByVal data As Data, ByVal pressure() As Double, ByVal time As Double, ByVal write As WriteAndReadToFile)
        Dim name As String


        If time < data.tStartGraph Then
            Exit Function
        End If
        name = "pressures\pressure" + CStr(calculationCount) + ".data"
        If time = data.tEndEndGraph Then
            write.WritePressuer(pressure, name, data.tStartGraph, data.tEndEndGraph, data)
        Else
            write.WritePressuer(pressure, name, data.tStartGraph, time, data)
        End If

    End Function

    Function StartCalculatePressureWithRungeKutt(ByVal changingParam As ChangingParams, ByVal tmp As TmpParam, ByVal data As Data, ByVal unit As Unit) As Decimal
        Dim pressure As Decimal
        Dim k1, k2, k3, k4 As Decimal
        Dim t, p As Double


        t = tmp.t
        p = tmp.p

        k1 = CalculatePressureWithRungeKutt(changingParam, tmp, data, unit, t, p)
        k2 = CalculatePressureWithRungeKutt(changingParam, tmp, data, unit, t + data.h / 2, p + k1 * data.h / 2)
        k3 = CalculatePressureWithRungeKutt(changingParam, tmp, data, unit, t + data.h / 2, p + k2 * data.h / 2)
        k4 = CalculatePressureWithRungeKutt(changingParam, tmp, data, unit, t + data.h, p + k3 * data.h)
        pressure = data.h * (k1 + 2 * k2 + 2 * k3 + k4) / 6


        Return pressure
    End Function




    Function CalculatePressureWithRungeKutt(ByVal changingParam As ChangingParams, ByVal tmp As TmpParam, ByVal data As Data, ByVal unit As Unit, ByVal t As Double, ByVal p As Double) As Decimal
        Dim sEf As Decimal ' часть с эффективной скоростью откачки
        Dim q As Decimal ' часть с газовыделениями


        q = 0
        sEf = SEffectiveWithOtherParts(tmp, p)

        If tmp.t < data.tStartHeatRising Then
            q = EvacuationPrHeat(tmp, unit, changingParam, data, t, p)
        ElseIf tmp.t >= data.tStartHeatRising And tmp.t <= data.tEndHeatRising Then
            changingParam.timeBegin = data.tStartHeatRising
            changingParam.timeEnd = data.tEndHeatRising
            q = EvacuationWhileHeatRise(tmp, unit, changingParam, data, t, p)
        ElseIf tmp.t < data.tKatodActivate Then
            changingParam.timeBegin = data.tEndHeatRising
            changingParam.timeEnd = data.tKatodActivate
            q = EvacuationAntilActivateKatod(tmp, unit, changingParam, data, t, p)
        ElseIf tmp.t >= data.tKatodActivate And tmp.t <= data.tKatodActive Then
            changingParam.timeBegin = data.tKatodActivate
            changingParam.timeEnd = data.tKatodActive
            q = EvacuationWhileKatodTemperatureRise(tmp, unit, changingParam, data, t, p)
        ElseIf tmp.t < data.tEndHeating Then
            changingParam.timeBegin = data.tKatodActive
            changingParam.timeEnd = data.tEndHeating
            q = EvacuationBeforeHeatingOff(tmp, unit, changingParam, data, t, p)
        ElseIf tmp.t < data.tKatodDisactivate Then
            changingParam.timeBegin = data.tEndHeating
            changingParam.timeEnd = data.tKatodDisactivate
            q = EvacuationBeforeKatodOff(tmp, unit, changingParam, data, t, p)
        ElseIf tmp.t <= data.tEnd Then
            changingParam.timeBegin = data.tKatodDisactivate
            changingParam.timeEnd = data.tEnd
            q = EvacuationBeforeEnd(tmp, unit, changingParam, data, t, p)
        End If

        Return (sEf + q) / (unit.body.volume + unit.drain.volume + unit.gun.volume)
    End Function

    Function FlowParameterWhenConst(ByVal data As Data, ByVal unit As ChangingParams, ByVal p As Double) As Decimal
        Dim flowParam As Decimal

        flowParam = data.qFlows * (data.pressureBegin - p)
        Return flowParam
    End Function

    Function EvacuationPrHeat(ByVal tmp As TmpParam, ByVal unit As Unit, ByVal changingParam As ChangingParams, ByVal data As Data, ByVal t As Double, ByVal p As Double) As Decimal
        Dim q As Decimal

        q = 0
        changingParam.temperatureBegin = data.temperatureBeforeHeat
        tmp = SetTemperature(tmp, data.temperatureBeforeHeat)
        tmp = SetKStatic(tmp, unit.body.kUdel)
        tmp = SetQStatic(tmp, unit.body.qUdel)
        q += FlowFromPart(tmp, unit.body, t)
        tmp = SetKStatic(tmp, unit.channel.kUdel)
        tmp = SetQStatic(tmp, unit.channel.qUdel)
        q += FlowFromPart(tmp, unit.channel, t)
        tmp = SetKStatic(tmp, unit.drain.kUdel)
        tmp = SetQStatic(tmp, unit.drain.qUdel)
        q += FlowFromPart(tmp, unit.drain, t)
        tmp = SetKStatic(tmp, unit.gun.kUdel)
        tmp = SetQStatic(tmp, unit.gun.qUdel)
        q += FlowFromPart(tmp, unit.gun, t)
        tmp = SetKStatic(tmp, unit.katod.kUdel)
        tmp = SetQStatic(tmp, unit.katod.qUdel)
        q += FlowFromPart(tmp, unit.katod, t)
        q += FlowParameterWhenConst(data, changingParam, p)
        Return q
    End Function

    Function EvacuationWhileHeatRise(ByVal tmp As TmpParam, ByVal unit As Unit, ByVal changingParam As ChangingParams, ByVal data As Data, ByVal t As Double, ByVal p As Double) As Decimal
        Dim q As Decimal

        q = 0
        tmp = ChanchingTemperature(tmp, data.temperatureBeforeHeat, data.temperatureAfterHeat, changingParam, t)
        tmp = SetQDinamic(tmp, changingParam, unit.body.qUdel, unit.body.qAfterHeat, t)
        tmp = SetKDinamic(tmp, changingParam, unit.body.kUdel, unit.body.kAfterHeat, t)
        q += FlowFromPart(tmp, unit.body, t)
        tmp = SetQDinamic(tmp, changingParam, unit.channel.qUdel, unit.channel.qAfterHeat, t)
        tmp = SetKDinamic(tmp, changingParam, unit.channel.kUdel, unit.channel.kAfterHeat, t)
        q += FlowFromPart(tmp, unit.channel, t)
        tmp = SetQDinamic(tmp, changingParam, unit.drain.qUdel, unit.drain.qAfterHeat, t)
        tmp = SetKDinamic(tmp, changingParam, unit.drain.kUdel, unit.drain.kAfterHeat, t)
        q += FlowFromPart(tmp, unit.drain, t)
        tmp = SetQDinamic(tmp, changingParam, unit.gun.qUdel, unit.gun.qAfterHeat, t)
        tmp = SetKDinamic(tmp, changingParam, unit.gun.kUdel, unit.gun.kAfterHeat, t)
        q += FlowFromPart(tmp, unit.gun, t)
        tmp = SetQDinamic(tmp, changingParam, unit.katod.qUdel, unit.katod.qAfterHeat, t)
        tmp = SetKDinamic(tmp, changingParam, unit.katod.kUdel, unit.katod.kAfterHeat, t)
        q += FlowFromPart(tmp, unit.katod, t)
        q += FlowParameterWhenConst(data, changingParam, p)
        Return q
    End Function

    Function EvacuationAntilActivateKatod(ByVal tmp As TmpParam, ByVal unit As Unit, ByVal changingParam As ChangingParams, ByVal data As Data, ByVal t As Double, ByVal p As Double) As Decimal
        Dim q As Decimal

        q = 0
        tmp = SetTemperature(tmp, data.temperatureAfterHeat)
        tmp = SetKStatic(tmp, unit.body.kAfterHeat)
        tmp = SetQStatic(tmp, unit.body.qAfterHeat)
        q += FlowFromPart(tmp, unit.body, t)
        tmp = SetKStatic(tmp, unit.channel.kAfterHeat)
        tmp = SetQStatic(tmp, unit.channel.qAfterHeat)
        q += FlowFromPart(tmp, unit.channel, t)
        tmp = SetKStatic(tmp, unit.drain.kAfterHeat)
        tmp = SetQStatic(tmp, unit.drain.qAfterHeat)
        q += FlowFromPart(tmp, unit.drain, t)
        tmp = SetKStatic(tmp, unit.gun.kAfterHeat)
        tmp = SetQStatic(tmp, unit.gun.qAfterHeat)
        q += FlowFromPart(tmp, unit.gun, t)
        tmp = SetKStatic(tmp, unit.katod.kAfterHeat)
        tmp = SetQStatic(tmp, unit.katod.qAfterHeat)
        q += FlowFromPart(tmp, unit.katod, t)
        q += FlowParameterWhenConst(data, changingParam, p)
        Return q
    End Function

    Function EvacuationWhileKatodTemperatureRise(ByVal tmp As TmpParam, ByVal unit As Unit, ByVal changingParam As ChangingParams, ByVal data As Data, ByVal t As Double, ByVal p As Double) As Decimal
        Dim q As Decimal

        q = 0
        tmp = SetTemperature(tmp, data.temperatureAfterHeat)
        tmp = SetKStatic(tmp, unit.body.kAfterHeat)
        tmp = SetQStatic(tmp, unit.body.qAfterHeat)
        q += FlowFromPart(tmp, unit.body, t)
        tmp = SetKStatic(tmp, unit.channel.kAfterHeat)
        tmp = SetQStatic(tmp, unit.channel.qAfterHeat)
        q += FlowFromPart(tmp, unit.channel, t)
        tmp = SetKStatic(tmp, unit.drain.kAfterHeat)
        tmp = SetQStatic(tmp, unit.drain.qAfterHeat)
        q += FlowFromPart(tmp, unit.drain, t)
        tmp = ChanchingTemperature(tmp, data.temperatureAfterHeat, unit.katod.temperatureMax * 0.7, changingParam, t)
        tmp = SetQDinamic(tmp, changingParam, unit.gun.qAfterHeat, unit.gun.qMin, t)
        tmp = SetKDinamic(tmp, changingParam, unit.gun.kAfterHeat, unit.gun.kMin, t)
        q += FlowFromPart(tmp, unit.gun, t)
        tmp = ChanchingTemperature(tmp, data.temperatureAfterHeat, unit.katod.temperatureMax, changingParam, t)
        tmp = SetQDinamic(tmp, changingParam, unit.katod.qAfterHeat, unit.katod.qMin, t)
        tmp = SetKDinamic(tmp, changingParam, unit.katod.kAfterHeat, unit.katod.kMin, t)
        q += FlowFromPart(tmp, unit.katod, t)
        q += FlowParameterWhenConst(data, changingParam, p)
        Return q
    End Function

    Function EvacuationBeforeHeatingOff(ByVal tmp As TmpParam, ByVal unit As Unit, ByVal changingParam As ChangingParams, ByVal data As Data, ByVal t As Double, ByVal p As Double) As Decimal
        Dim q As Decimal

        q = 0
        tmp = SetTemperature(tmp, data.temperatureAfterHeat)
        tmp = SetKStatic(tmp, unit.body.kAfterHeat)
        tmp = SetQStatic(tmp, unit.body.qAfterHeat)
        q += FlowFromPart(tmp, unit.body, t)
        tmp = SetKStatic(tmp, unit.channel.kAfterHeat)
        tmp = SetQStatic(tmp, unit.channel.qAfterHeat)
        q += FlowFromPart(tmp, unit.channel, t)
        tmp = SetKStatic(tmp, unit.drain.kAfterHeat)
        tmp = SetQStatic(tmp, unit.drain.qAfterHeat)
        q += FlowFromPart(tmp, unit.drain, t)
        tmp = SetTemperature(tmp, unit.katod.temperatureMax * 0.7)
        tmp = SetKStatic(tmp, unit.gun.kMin)
        tmp = SetQStatic(tmp, unit.gun.qMin)
        q += FlowFromPart(tmp, unit.gun, t)
        tmp = SetTemperature(tmp, unit.katod.temperatureMax)
        tmp = SetKStatic(tmp, unit.katod.kMin)
        tmp = SetQStatic(tmp, unit.katod.qMin)
        q += FlowFromPart(tmp, unit.katod, t)
        q += FlowParameterWhenConst(data, changingParam, p)
        Return q
    End Function

    Function EvacuationBeforeKatodOff(ByVal tmp As TmpParam, ByVal unit As Unit, ByVal changingParam As ChangingParams, ByVal data As Data, ByVal t As Double, ByVal p As Double) As Decimal
        Dim q As Decimal

        q = 0
        tmp = SetTemperature(tmp, unit.katod.temperatureMax * 0.7)
        tmp = SetKStatic(tmp, unit.gun.kMin)
        tmp = SetQStatic(tmp, unit.gun.qMin)
        q += FlowFromPart(tmp, unit.gun, t)
        tmp = SetTemperature(tmp, unit.katod.temperatureMax)
        tmp = SetKStatic(tmp, unit.katod.kMin)
        tmp = SetQStatic(tmp, unit.katod.qMin)
        q += FlowFromPart(tmp, unit.katod, t)
        changingParam.timeBegin = data.tEndHeating
        changingParam.timeEnd = data.tEnd
        tmp = ChanchingTemperature(tmp, data.temperatureAfterHeat, data.temperatureBeforeHeat, changingParam, t)
        tmp = SetKStatic(tmp, unit.body.kAfterHeat)
        tmp = SetQStatic(tmp, unit.body.qAfterHeat)
        q += FlowFromPart(tmp, unit.body, t)
        tmp = SetKStatic(tmp, unit.channel.kAfterHeat)
        tmp = SetQStatic(tmp, unit.channel.qAfterHeat)
        q += FlowFromPart(tmp, unit.channel, t)
        tmp = SetKStatic(tmp, unit.drain.kAfterHeat)
        tmp = SetQStatic(tmp, unit.drain.qAfterHeat)
        q += FlowFromPart(tmp, unit.drain, t)
        q += FlowParameterWhenConst(data, changingParam, p)
        Return q
    End Function

    Function EvacuationBeforeEnd(ByVal tmp As TmpParam, ByVal unit As Unit, ByVal changingParam As ChangingParams, ByVal data As Data, ByVal t As Double, ByVal p As Double) As Decimal
        Dim q As Decimal

        q = 0
        tmp = ChanchingTemperature(tmp, unit.katod.temperatureMax * 0.7, data.temperatureBeforeHeat, changingParam, t)
        tmp = SetKStatic(tmp, unit.gun.kMin)
        tmp = SetQStatic(tmp, unit.gun.qMin)
        q += FlowFromPart(tmp, unit.gun, t)
        tmp = ChanchingTemperature(tmp, unit.katod.temperatureMax, data.temperatureBeforeHeat, changingParam, t)
        tmp = SetKStatic(tmp, unit.katod.kMin)
        tmp = SetQStatic(tmp, unit.katod.qMin)
        q += FlowFromPart(tmp, unit.katod, t)
        changingParam.timeBegin = data.tEndHeating
        changingParam.timeEnd = data.tEnd
        tmp = ChanchingTemperature(tmp, data.temperatureAfterHeat, data.temperatureBeforeHeat, changingParam, t)
        tmp = SetKStatic(tmp, unit.body.kAfterHeat)
        tmp = SetQStatic(tmp, unit.body.qAfterHeat)
        q += FlowFromPart(tmp, unit.body, t)
        tmp = SetKStatic(tmp, unit.channel.kAfterHeat)
        tmp = SetQStatic(tmp, unit.channel.qAfterHeat)
        q += FlowFromPart(tmp, unit.channel, t)
        tmp = SetKStatic(tmp, unit.drain.kAfterHeat)
        tmp = SetQStatic(tmp, unit.drain.qAfterHeat)
        q += FlowFromPart(tmp, unit.drain, t)
        q += FlowParameterWhenConst(data, changingParam, p)
        Return q
    End Function

    Function ChanchingTemperature(ByVal tmp As TmpParam, ByVal temperatureBegin As Double, ByVal temperatureAfter As Double, ByVal unit As ChangingParams, ByVal t As Double) As TmpParam
        tmp.temperature = temperatureBegin + (temperatureAfter - temperatureBegin) * (t - unit.timeBegin) / (unit.timeEnd - unit.timeBegin)
        Return tmp
    End Function

    Function SetTemperature(ByVal tmp As TmpParam, ByVal temperature As Decimal) As TmpParam
        tmp.temperature = temperature
        Return tmp
    End Function

    Function FlowFromPart(ByVal tmp As TmpParam, ByVal element As Element, ByVal t As Double) As Decimal
        Dim kParam As Decimal
        Dim answer As Decimal
        Dim qParam As Decimal

        qParam = element.square * tmp.q
        kParam = tmp.k * Math.E ^ (tmp.alpha * (1 / tmp.temperature - 1 / 293))
        answer = qParam * kParam * Math.E ^ (-kParam * t)
        Return answer
    End Function

    Function SetQDinamic(ByVal tmp As TmpParam, ByVal unit As ChangingParams, ByVal qBefore As Decimal, ByVal qAfter As Decimal, ByVal t As Double) As TmpParam
        tmp.q = qBefore + (qAfter - qBefore) * (t - unit.timeBegin) / (unit.timeEnd - unit.timeBegin)
        Return tmp
    End Function

    Function SetQStatic(ByVal tmp As TmpParam, ByVal k As Decimal) As TmpParam
        tmp.k = k
        Return tmp
    End Function

    Function SetKDinamic(ByVal tmp As TmpParam, ByVal unit As ChangingParams, ByVal kBefore As Decimal, ByVal kAfter As Decimal, ByVal t As Double) As TmpParam
        tmp.k = kBefore + (kAfter - kBefore) * (t - unit.timeBegin) / (unit.timeEnd - unit.timeBegin)
        Return tmp
    End Function

    Function SetKStatic(ByVal tmp As TmpParam, ByVal k As Decimal) As TmpParam
        tmp.k = k
        Return tmp
    End Function

    'эффективная скорость откачки
    Function SEffectiveWithOtherParts(ByVal tmp As TmpParam, ByVal p As Double) As Decimal ' высчитываем компоненту с эффективной скоростью откачки
        Dim koef As Decimal
        Dim sEf As Decimal
        Dim k1 As Double ' просто разгружаем решение

        k1 = 1.36 * 10 ^ 3
        koef = (1 + 1.9 * (10 ^ 4) * tmp.d * p) / (1 + 2.35 * (10 ^ 4) * tmp.d * p)
        sEf = (-1) * p * tmp.s * ((k1 * p * (tmp.d ^ 4) / tmp.l) + (121 * (tmp.d ^ 3) / tmp.l)) * koef /
            (tmp.s + ((k1 * p * (tmp.d ^ 4) / tmp.l) + (121 * (tmp.d ^ 3) / tmp.l * koef)))
        Return sEf
    End Function

End Class


