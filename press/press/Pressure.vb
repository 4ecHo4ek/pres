Public Class Pressure
    Public f2 As Graph
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim file = New WriteAndReadToFile
        Dim cearchPart = New CalculatingPart
        Dim draw = New DrawingPart
        Dim filePath As String = "bmp\"
        Dim fileName = filePath + CStr(calculationCount) + ".bmp"
        Dim pressure As Double

        file.WriteLogs("Начинаю вычисление давления")
        Button1.Text = "Вычисляю"
        f1 = Me
        ProgressSetings()

        ' тут получаем файлы с начальной инфой
        If Not file.WriteBeginigDataToFile(f1) Then
            file.WriteLogs("ошибка формирования начального запроса")
            Exit Sub
        End If

        ProgressBar1.Value = 1
        file.WriteLogs("Входные данные сформированы")
        If calculationCount = 0 Then
            If IO.Directory.Exists("pressures") Then
                IO.Directory.Delete("pressures", True)
            End If
            IO.Directory.CreateDirectory("pressures") ' тут со всеми папками сделать чтоб при певром запуске они обновлялись
            If IO.Directory.Exists("bmp") Then
                IO.Directory.Delete("bmp", True)
            End If
            IO.Directory.CreateDirectory("bmp")



        End If
        file.WriteLogs("Начало вычисления давления")

        Try
            ProgressBar1.Value = 2
            SetRightParams()
            pressure = cearchPart.Calculatings()
        Catch
            file.WriteLogs("ошибка выполнения программы")
            Exit Sub
        End Try

        If calculationCount = 0 Then
            f2 = New Graph
            f2.Show()
            f2.Height = f1.HeightBoxTF.Text
            f2.Width = f1.WidhtBoxTF.Text
            file.WriteLogs("форма графика создна")
        End If

        Try
            draw.Draw()
            f2.Select()
            Using mstream As New System.IO.MemoryStream(IO.File.ReadAllBytes(fileName))
                f2.Graphic.Image = Image.FromStream(mstream)
            End Using
            calculationCount += 1
        Catch
            If calculationCount = 0 Then
                f2.Graphic.Image = Nothing
            Else
                fileName = filePath + CStr(calculationCount - 1) + ".bmp"
                f2.Graphic.Image = Image.FromFile(fileName)
            End If
            file.WriteLogs("ошибка загрузки изображения эксперимента, возврат предыдущего изображения")
            Exit Sub
        End Try
        file.WriteLogs("график построен")
        file.WriteLogs("Предельное давление " & CStr(Format(pressure, "0.###E+0")) & " достигнуто")
        Button1.Text = "Вычислено"
        f1.ProgressBar1.Value = 10

        ReadLogs()
    End Sub

    Function ReadLogs()
        Dim path As String = "log.txt"
        Dim SR = New IO.StreamReader(path)


        While SR.Peek <> -1
            LogTF.Text = LogTF.Text & vbCrLf & SR.ReadLine()
        End While

        SR.Close()
    End Function

    Function ProgressSetings()
        ProgressBar1.Minimum = 0
        ProgressBar1.Maximum = 10
    End Function

    Function SetRightParams()
        Dim time1, time2 As Double

        time1 = f1.BeginGraphTimeTF.Text
        time2 = f1.EndGraphTimeTF.Text

        If time1 > time2 Then
            Dim tmp = time1
            time1 = time2
            time2 = tmp
        ElseIf time1 = time2 Then
            time2 += 10
        End If
        f1.BeginGraphTimeTF.Text = CStr(time1)
        f1.EndGraphTimeTF.Text = CStr(time2)

    End Function

    Private Sub Pressure_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim standart = New CalculatingAndStandartParams
        standart.setStandart(Me)
        ' Button2.Enabled = False
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Dim close = New CancelingOrClosing
        close.CloseAndDelete()
        f2.Close()
        LogTF.Text = ""
        f1.ProgressBar1.Value = 0
    End Sub

    Private Sub WidhtBoxTF_Leave(sender As Object, e As EventArgs) Handles WidhtBoxTF.Leave
        Dim tmp = 800
        If WidhtBoxTF.Text.Equals("") Then
            WidhtBoxTF.Text = tmp
        End If
        WidhtBoxTF.Text = WidhtBoxTF.Text.Replace(".", ",")
        If Not IsNumeric(WidhtBoxTF.Text) Then
            WidhtBoxTF.Text = tmp
        End If
    End Sub

    Private Sub HeightBoxTF_Leave(sender As Object, e As EventArgs) Handles HeightBoxTF.Leave
        Dim tmp = 640
        If HeightBoxTF.Text.Equals("") Then
            HeightBoxTF.Text = tmp
        End If
        HeightBoxTF.Text = HeightBoxTF.Text.Replace(".", ",")
        If Not IsNumeric(HeightBoxTF.Text) Then
            HeightBoxTF.Text = tmp
        End If
    End Sub

    Private Sub BeginGraphTimeTF_Leave(sender As Object, e As EventArgs) Handles BeginGraphTimeTF.Leave
        Dim tmp = 0
        If BeginGraphTimeTF.Text.Equals("") Then
            BeginGraphTimeTF.Text = tmp
        End If
        BeginGraphTimeTF.Text = BeginGraphTimeTF.Text.Replace(".", ",")
        If Not IsNumeric(BeginGraphTimeTF.Text) Then
            BeginGraphTimeTF.Text = tmp
        End If
        If CDbl(BeginGraphTimeTF.Text) > CDbl(EndGraphTimeTF.Text) Then
            BeginGraphTimeTF.Text = tmp
            LogTF.Text = LogTF.Text & vbCrLf & "Время начала построения графика не может начинаться раньше его завершения"
        End If
    End Sub

    Private Sub EndGraphTimeTF_Leave(sender As Object, e As EventArgs) Handles EndGraphTimeTF.Leave
        Dim tmp = tEndTF.Text
        If EndGraphTimeTF.Text.Equals("") Then
            EndGraphTimeTF.Text = tmp
        End If
        EndGraphTimeTF.Text = EndGraphTimeTF.Text.Replace(".", ",")
        If Not IsNumeric(EndGraphTimeTF.Text) Then
            EndGraphTimeTF.Text = tmp
        End If
        If CDbl(EndGraphTimeTF.Text) > tmp Then
            EndGraphTimeTF.Text = tmp
            LogTF.Text = LogTF.Text & vbCrLf & "Время окончания построения графика не может быть больше времени окончания эксперимента"
        End If
    End Sub

    Private Sub BeginPressTF_LocationChanged(sender As Object, e As EventArgs) Handles BeginPressTF.LocationChanged
        Dim tmp = 10000
        If BeginPressTF.Text.Equals("") Then
            BeginPressTF.Text = tmp
        End If
        BeginPressTF.Text = BeginPressTF.Text.Replace(".", ",")
        If Not IsNumeric(BeginPressTF.Text) Then
            BeginPressTF.Text = tmp
        End If
    End Sub

    Private Sub MinFlowTF_Leave(sender As Object, e As EventArgs) Handles MinFlowTF.Leave
        Dim tmp = 10 ^ -13
        If MinFlowTF.Text.Equals("") Then
            MinFlowTF.Text = tmp
        End If
        MinFlowTF.Text = MinFlowTF.Text.Replace(".", ",")
        If Not IsNumeric(MinFlowTF.Text) Then
            MinFlowTF.Text = tmp
        End If
    End Sub

    Private Sub AlphaTF_Leave(sender As Object, e As EventArgs) Handles AlphaTF.Leave
        Dim tmp = -2500
        If AlphaTF.Text.Equals("") Then
            AlphaTF.Text = tmp
        End If
        AlphaTF.Text = AlphaTF.Text.Replace(".", ",")
        If Not IsNumeric(AlphaTF.Text) Then
            AlphaTF.Text = tmp
        End If
    End Sub

    Private Sub HKoefTF_Leave(sender As Object, e As EventArgs) Handles HKoefTF.Leave
        Dim tmp = 0.5
        If HKoefTF.Text.Equals("") Then
            HKoefTF.Text = tmp
        End If
        HKoefTF.Text = HKoefTF.Text.Replace(".", ",")
        If Not IsNumeric(HKoefTF.Text) Then
            HKoefTF.Text = tmp
        End If
    End Sub

    Private Sub TempBeginTF_Leave(sender As Object, e As EventArgs) Handles TempBeginTF.Leave
        Dim tmp = 293
        If TempBeginTF.Text.Equals("") Then
            TempBeginTF.Text = tmp
        End If
        TempBeginTF.Text = TempBeginTF.Text.Replace(".", ",")
        If Not IsNumeric(TempBeginTF.Text) Then
            TempBeginTF.Text = tmp
        End If
    End Sub

    Private Sub TempEndTF_Leave(sender As Object, e As EventArgs) Handles TempEndTF.Leave
        Dim tmp = 893
        If TempEndTF.Text.Equals("") Then
            TempEndTF.Text = tmp
        End If
        TempEndTF.Text = TempEndTF.Text.Replace(".", ",")
        If Not IsNumeric(TempEndTF.Text) Then
            TempEndTF.Text = tmp
        End If
    End Sub

    Private Sub t1TF_Leave(sender As Object, e As EventArgs) Handles t1TF.Leave
        Dim tmp = 2000
        If t1TF.Text.Equals("") Then
            t1TF.Text = tmp
        End If
        t1TF.Text = t1TF.Text.Replace(".", ",")
        If Not IsNumeric(t1TF.Text) Then
            t1TF.Text = tmp
        End If
    End Sub

    Private Sub tStartHeatRisingTF_Leave(sender As Object, e As EventArgs) Handles tStartHeatRisingTF.Leave
        Dim tmp = 25000
        If tStartHeatRisingTF.Text.Equals("") Then
            tStartHeatRisingTF.Text = tmp
        End If
        tStartHeatRisingTF.Text = tStartHeatRisingTF.Text.Replace(".", ",")
        If Not IsNumeric(tStartHeatRisingTF.Text) Then
            tStartHeatRisingTF.Text = tmp
        End If
    End Sub

    Private Sub tEndHeatRisingTF_Leave(sender As Object, e As EventArgs) Handles tEndHeatRisingTF.Leave
        Dim tmp = 28000
        If tEndHeatRisingTF.Text.Equals("") Then
            tEndHeatRisingTF.Text = tmp
        End If
        tEndHeatRisingTF.Text = tEndHeatRisingTF.Text.Replace(".", ",")
        If Not IsNumeric(tEndHeatRisingTF.Text) Then
            tEndHeatRisingTF.Text = tmp
        End If
    End Sub

    Private Sub tKatodActivateTF_Leave(sender As Object, e As EventArgs) Handles tKatodActivateTF.Leave
        Dim tmp = 30000
        If tKatodActivateTF.Text.Equals("") Then
            tKatodActivateTF.Text = tmp
        End If
        tKatodActivateTF.Text = tKatodActivateTF.Text.Replace(".", ",")
        If Not IsNumeric(tKatodActivateTF.Text) Then
            tKatodActivateTF.Text = tmp
        End If
    End Sub

    Private Sub tKatodActiveTF_Leave(sender As Object, e As EventArgs) Handles tKatodActiveTF.Leave
        Dim tmp = 31000
        If tKatodActiveTF.Text.Equals("") Then
            tKatodActiveTF.Text = tmp
        End If
        tKatodActiveTF.Text = tKatodActiveTF.Text.Replace(".", ",")
        If Not IsNumeric(tKatodActiveTF.Text) Then
            tKatodActiveTF.Text = tmp
        End If
    End Sub

    Private Sub tEndHeatingTF_Leave(sender As Object, e As EventArgs) Handles tEndHeatingTF.Leave
        Dim tmp = 35000
        If tEndHeatingTF.Text.Equals("") Then
            tEndHeatingTF.Text = tmp
        End If
        tEndHeatingTF.Text = tEndHeatingTF.Text.Replace(".", ",")
        If Not IsNumeric(tEndHeatingTF.Text) Then
            tEndHeatingTF.Text = tmp
        End If
    End Sub

    Private Sub tKatodDisactivateTF_Leave(sender As Object, e As EventArgs) Handles tKatodDisactivateTF.Leave
        Dim tmp = 38000
        If tKatodDisactivateTF.Text.Equals("") Then
            tKatodDisactivateTF.Text = tmp
        End If
        tKatodDisactivateTF.Text = tKatodDisactivateTF.Text.Replace(".", ",")
        If Not IsNumeric(tKatodDisactivateTF.Text) Then
            tKatodDisactivateTF.Text = tmp
        End If
    End Sub

    Private Sub tEndTF_Leave(sender As Object, e As EventArgs) Handles tEndTF.Leave
        Dim tmp = 40000
        If tEndTF.Text.Equals("") Then
            tEndTF.Text = tmp
        End If
        tEndTF.Text = tEndTF.Text.Replace(".", ",")
        If Not IsNumeric(tEndTF.Text) Then
            tEndTF.Text = tmp
        End If
    End Sub

    Private Sub d1TF_Leave(sender As Object, e As EventArgs) Handles d1TF.TextChanged
        Dim tmp = 20 * 10 ^ -3
        If d1TF.Text.Equals("") Then
            d1TF.Text = tmp
        End If
        d1TF.Text = d1TF.Text.Replace(".", ",")
        If Not IsNumeric(d1TF.Text) Then
            d1TF.Text = tmp
        End If
    End Sub

    Private Sub l1TF_Leave(sender As Object, e As EventArgs) Handles l1TF.Leave
        Dim tmp = 1
        If l1TF.Text.Equals("") Then
            l1TF.Text = tmp
        End If
        l1TF.Text = l1TF.Text.Replace(".", ",")
        If Not IsNumeric(l1TF.Text) Then
            l1TF.Text = tmp
        End If
    End Sub

    Private Sub s1TF_Leave(sender As Object, e As EventArgs) Handles s1TF.Leave
        Dim tmp = 0.055
        If s1TF.Text.Equals("") Then
            s1TF.Text = tmp
        End If
        s1TF.Text = s1TF.Text.Replace(".", ",")
        If Not IsNumeric(s1TF.Text) Then
            s1TF.Text = tmp
        End If
    End Sub

    Private Sub d2TF_Leave(sender As Object, e As EventArgs) Handles d2TF.Leave
        Dim tmp = 25 * 10 ^ -3
        If d2TF.Text.Equals("") Then
            d2TF.Text = tmp
        End If
        d2TF.Text = d2TF.Text.Replace(".", ",")
        If Not IsNumeric(d2TF.Text) Then
            d2TF.Text = tmp
        End If
    End Sub

    Private Sub l2TF_Leave(sender As Object, e As EventArgs) Handles l2TF.Leave
        Dim tmp = 1
        If l2TF.Text.Equals("") Then
            l2TF.Text = tmp
        End If
        l2TF.Text = l2TF.Text.Replace(".", ",")
        If Not IsNumeric(l2TF.Text) Then
            l2TF.Text = tmp
        End If
    End Sub

    Private Sub s2TF_Leave(sender As Object, e As EventArgs) Handles s2TF.Leave
        Dim tmp = 0.33
        If s2TF.Text.Equals("") Then
            s2TF.Text = tmp
        End If
        s2TF.Text = s2TF.Text.Replace(".", ",")
        If Not IsNumeric(s2TF.Text) Then
            s2TF.Text = tmp
        End If
    End Sub

    Private Sub minPreVacTF_Leave(sender As Object, e As EventArgs) Handles minPreVacTF.Leave
        Dim tmp = 10
        If minPreVacTF.Text.Equals("") Then
            minPreVacTF.Text = tmp
        End If
        minPreVacTF.Text = minPreVacTF.Text.Replace(".", ",")
        If Not IsNumeric(minPreVacTF.Text) Then
            minPreVacTF.Text = tmp
        End If
    End Sub

    Private Sub WidhtBoxTF_MouseMove(sender As Object, e As MouseEventArgs) Handles WidhtBoxTF.MouseMove
        If calculationCount > 0 Then
            WidhtBoxTF.ReadOnly = True
        Else
            WidhtBoxTF.ReadOnly = False
        End If
    End Sub
    Private Sub HeightBoxTF_MouseMove(sender As Object, e As MouseEventArgs) Handles HeightBoxTF.MouseMove
        If calculationCount > 0 Then
            HeightBoxTF.ReadOnly = True
        Else
            HeightBoxTF.ReadOnly = False
        End If
    End Sub

    Private Sub BeginGraphTimeTF_MouseMove(sender As Object, e As MouseEventArgs) Handles BeginGraphTimeTF.MouseMove
        If calculationCount > 0 Then
            BeginGraphTimeTF.ReadOnly = True
        Else
            BeginGraphTimeTF.ReadOnly = False
        End If
    End Sub

    Private Sub EndGraphTimeTF_MouseMove(sender As Object, e As MouseEventArgs) Handles EndGraphTimeTF.MouseMove
        If calculationCount > 0 Then
            EndGraphTimeTF.ReadOnly = True
        Else
            EndGraphTimeTF.ReadOnly = False
        End If
    End Sub


    Private Sub Button1_MouseMove(sender As Object, e As MouseEventArgs) Handles Button1.MouseMove
        If Button1.Text = "Вычислено" Then
            Button1.Text = "Рассчитать"
        End If
    End Sub

    Private Sub Pressure_MouseMove(sender As Object, e As MouseEventArgs) Handles Me.MouseMove
        If calculationCount = 0 Then
            Button2.Enabled = False
        Else
            Button2.Enabled = True
        End If
    End Sub
End Class
