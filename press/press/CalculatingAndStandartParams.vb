Public Class CalculatingAndStandartParams

    Function setStandart(ByVal f1 As Pressure)
        f1.WidhtBoxTF.Text = 800
        f1.HeightBoxTF.Text = 640
        f1.BeginGraphTimeTF.Text = 0
        f1.EndGraphTimeTF.Text = 60000
        f1.BeginPressTF.Text = 10000
        f1.MinFlowTF.Text = 10 ^ -13
        f1.AlphaTF.Text = -2000
        f1.HKoefTF.Text = 0.5
        f1.TempBeginTF.Text = 293
        f1.TempEndTF.Text = 893
        f1.t1TF.Text = 2000
        f1.tStartHeatRisingTF.Text = 25000
        f1.tEndHeatRisingTF.Text = 40000
        f1.tKatodActivateTF.Text = 45000
        f1.tKatodActiveTF.Text = 45500
        f1.tEndHeatingTF.Text = 50000
        f1.tKatodDisactivateTF.Text = 55000
        f1.tEndTF.Text = 60000
        f1.d1TF.Text = 20 * 10 ^ -3
        f1.l1TF.Text = 1
        f1.s1TF.Text = 0.055
        f1.d2TF.Text = 25 * 10 ^ -3
        f1.l2TF.Text = 1
        f1.s2TF.Text = 0.33
        f1.minPreVacTF.Text = 10
        ' корпус
        f1.bodykUdelTF.Text = 10 ^ -5
        f1.bodykAfterHeatTF.Text = 10 ^ -8
        f1.bodyqUdelTF.Text = 10 ^ -6
        f1.bodyqAfterHeatTF.Text = 10 ^ -8
        f1.bodysquareTF.Text = 300
        f1.bodyvolumeTF.Text = 0.6
        ' коллектор
        f1.drainkUdelTF.Text = 10 ^ -5
        f1.drainkAfterHeatTF.Text = 10 ^ -8
        f1.drainqUdelTF.Text = 10 ^ -6
        f1.drainqAfterHeatTF.Text = 10 ^ -8
        f1.drainsquareTF.Text = 100
        f1.drainvolumeTF.Text = 0.22
        ' катод
        f1.katodkUdelTF.Text = 10 ^ -5
        f1.katodkAfterHeatTF.Text = 10 ^ -8
        f1.katodqUdelTF.Text = 10 ^ -6
        f1.katodqAfterHeat.Text = 10 ^ -8
        f1.katodsquareTF.Text = 30
        f1.katodtemperatureMaxTF.Text = 1400
        f1.katodkMinTF.Text = 10 ^ -10
        f1.katodqMinTF.Text = 10 ^ -10
        ' пролетный канал
        f1.channelkUdelTF.Text = 10 ^ -5
        f1.channelkAfterHeatTF.Text = 10 ^ -8
        f1.channelqUdelTF.Text = 10 ^ -6
        f1.channelqAfterHeatTF.Text = 10 ^ -8
        f1.channelsquareTF.Text = 200
        ' пушка
        f1.gunkUdelTF.Text = 10 ^ -5
        f1.gunkAfterHeatTF.Text = 10 ^ -8
        f1.gunqUdelTF.Text = 10 ^ -6
        f1.gunqAfterHeatTF.Text = 10 ^ -8
        f1.gunsquareTF.Text = 200
        f1.guntemperatureMaxTF.Text = 1000
        f1.gunkMinTF.Text = 10 ^ -10
        f1.gunqMinTF.Text = 10 ^ -10
        f1.gunvolumeTF.Text = 0.42
        ' сохранение
        f1.tBeginSavingTF.Text = 0
        f1.tEndSavingTF.Text = f1.tEndTF.Text
        f1.worvakKoefTF.Text = 3
        f1.vakPreHeatKoefTF.Text = 10
        f1.heatingKoefTF.Text = 50
        f1.hightTempTimeKoefTF.Text = 20
        f1.katodRisingKoefTF.Text = 20
        f1.heatingConstKoefTF.Text = 20
        f1.endKatodKoefTF.Text = 20
        f1.endKoefTF.Text = 20
    End Function

    Function SetDataParams(ByVal f1 As Pressure) As Boolean
        Dim f1leData As String = "allInfo\data.data"
        Dim koef = 60 ^ (f1.ComboBox3.SelectedIndex)

        Dim data = New Data With {
            .pressureBegin = f1.BeginPressTF.Text,
            .qFlows = f1.MinFlowTF.Text,
            .alpha = f1.AlphaTF.Text,
            .h = f1.HKoefTF.Text,
            .temperatureBeforeHeat = f1.TempBeginTF.Text,
            .temperatureAfterHeat = f1.TempEndTF.Text,
            .t1 = f1.t1TF.Text * koef,
            .tStartHeatRising = f1.tStartHeatRisingTF.Text * koef,
            .tEndHeatRising = f1.tEndHeatRisingTF.Text * koef,
            .tKatodActivate = f1.tKatodActivateTF.Text * koef,
            .tKatodActive = f1.tKatodActiveTF.Text * koef,
            .tEndHeating = f1.tEndHeatingTF.Text * koef,
            .tKatodDisactivate = f1.tKatodDisactivateTF.Text * koef,
            .tEnd = f1.tEndTF.Text * koef,
            .d1 = f1.d1TF.Text,
            .l1 = f1.l1TF.Text,
            .s1 = f1.s1TF.Text,
            .d2 = f1.d2TF.Text,
            .l2 = f1.l2TF.Text,
            .s2 = f1.s2TF.Text,
            .minPreVac = f1.minPreVacTF.Text,
            .tStartGraph = f1.BeginGraphTimeTF.Text,
            .tEndEndGraph = f1.EndGraphTimeTF.Text
        }
        Try
            If IO.File.Exists(f1leData) Then
                IO.File.Delete(f1leData)
            End If

            Dim formatter As New Runtime.Serialization.Formatters.Binary.BinaryFormatter()
            Using stream As IO.Stream = IO.File.Create(f1leData)
                formatter.Serialize(stream, data)
            End Using
            Return True
        Catch ex As Exception
            f1.LogTF.Text = f1.LogTF.Text & vbCrLf & "ошибка формирования начальных данных системы"
            Return False
        End Try
    End Function

    Function SetUnitParams(ByVal f1 As Pressure) As Boolean
        Dim unit = New Unit
        Dim katod = New Katod
        Dim gun = New Gun
        Dim body = New Body
        Dim channel = New Channel
        Dim drain = New Drain
        Dim fileUnit As String = "allInfo\units.data"

        'заполняем данные корпуса

        body.kUdel = f1.bodykUdelTF.Text
        body.kAfterHeat = f1.bodykAfterHeatTF.Text
        body.qUdel = f1.bodyqUdelTF.Text
        body.qAfterHeat = f1.bodyqAfterHeatTF.Text
        body.square = f1.bodysquareTF.Text
        body.volume = f1.bodyvolumeTF.Text
        unit.body = body
        'заполняем данные канала

        channel.kUdel = f1.channelkUdelTF.Text
        channel.kAfterHeat = f1.channelkAfterHeatTF.Text
        channel.qUdel = f1.channelqUdelTF.Text
        channel.qAfterHeat = f1.channelqAfterHeatTF.Text
        channel.square = f1.channelsquareTF.Text
        unit.channel = channel
        'заполняем данные коллектора

        drain.kUdel = f1.drainkUdelTF.Text
        drain.kAfterHeat = f1.drainkAfterHeatTF.Text
        drain.qUdel = f1.drainqUdelTF.Text
        drain.qAfterHeat = f1.drainqAfterHeatTF.Text
        drain.square = f1.drainsquareTF.Text
        drain.volume = f1.drainvolumeTF.Text
        unit.drain = drain
        'заполняем данные пушки

        gun.kUdel = f1.gunkUdelTF.Text
        gun.kAfterHeat = f1.gunkAfterHeatTF.Text
        gun.qUdel = f1.gunqUdelTF.Text
        gun.qAfterHeat = f1.gunqAfterHeatTF.Text
        gun.volume = f1.gunvolumeTF.Text
        gun.square = f1.gunsquareTF.Text
        gun.kMin = f1.gunkMinTF.Text
        gun.qMin = f1.gunqMinTF.Text
        gun.temperatureMax = f1.guntemperatureMaxTF.Text
        unit.gun = gun
        'заполняем данные катода

        katod.kUdel = f1.katodkUdelTF.Text
        katod.kAfterHeat = f1.katodkAfterHeatTF.Text
        katod.qUdel = f1.katodqUdelTF.Text
        katod.qAfterHeat = f1.katodqAfterHeat.Text
        katod.square = f1.katodsquareTF.Text
        katod.temperatureMax = f1.katodtemperatureMaxTF.Text
        katod.kMin = f1.katodkMinTF.Text
        katod.qMin = f1.katodqMinTF.Text
        unit.katod = katod
        Try
            If IO.File.Exists(fileUnit) Then
                IO.File.Delete(fileUnit)
            End If
            Dim formatter As New Runtime.Serialization.Formatters.Binary.BinaryFormatter()
            Using stream As IO.Stream = IO.File.Create(fileUnit)
                formatter.Serialize(stream, unit)
            End Using
            Return True
        Catch ex As Exception
            f1.LogTF.Text = f1.LogTF.Text & vbCrLf & "ошибка формирования начальных данных прибора"
            Return False
        End Try
    End Function

    Function SetDrawingParams(ByVal f1 As Pressure) As Boolean
        Dim fileDraw As String = "allInfo\draw.data"
        Dim koef = 60 ^ (f1.ComboBox3.SelectedIndex)
        Dim draw = New DrawingData With {
            .tStart = f1.BeginGraphTimeTF.Text * koef,
            .tEnd = f1.EndGraphTimeTF.Text * koef,
            .height = f1.HeightBoxTF.Text,
            .width = f1.WidhtBoxTF.Text
        }
        Try
            If IO.File.Exists(fileDraw) Then
                IO.File.Delete(fileDraw)
            End If
            Dim formatter As New Runtime.Serialization.Formatters.Binary.BinaryFormatter()
            Using stream As IO.Stream = IO.File.Create(fileDraw)
                formatter.Serialize(stream, draw)
            End Using
            Return True
        Catch ex As Exception
            f1.LogTF.Text = f1.LogTF.Text & vbCrLf & "ошибка формирования начальных данных графика"
            Return False
        End Try
    End Function
End Class
