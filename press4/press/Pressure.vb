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
        ComboBox1.Items.Clear()

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

        Try
            If calculationCount = 0 Then
                f2 = New Graph
                f2.Show()
                f2.Height = f1.HeightBoxTF.Text
                f2.Width = f1.WidhtBoxTF.Text
                file.WriteLogs("форма графика создна")
            End If
        Catch
            file.WriteLogs("не удалось открыть график")
        End Try

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

        ComboBox1.Items.AddRange(IO.File.ReadAllLines("pressSaving.txt"))
        ReadLogs()
    End Sub


    Private Sub Pressure_MouseMove(sender As Object, e As MouseEventArgs) Handles Me.MouseMove
        If calculationCount = 0 Then
            Button2.Enabled = False
            ComboBox3.Enabled = True
            Button3.Enabled = False
        Else
            Button2.Enabled = True
            ComboBox3.Enabled = False
            Button3.Enabled = True
        End If
        If EndGraphTimeTF.Text > tEndTF.Text Then
            EndGraphTimeTF.Text = tEndTF.Text
        End If
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        Dim excel = New SavingInExcel
        Dim save As New SaveFileDialog
        Dim write = New WriteAndReadToFile
        Dim saving = New ExcelSaving

        saving = excel.MakeData(saving, f1)
        write.WriteForSaving(saving)
        save.Filter = "Excel files *.xlsx|*.xlsx"
        If save.ShowDialog = DialogResult.Cancel Then
            Exit Sub
        End If
        save.ShowDialog()
        excel.SaveInExcel(ComboBox1.SelectedIndex, save.FileName)
        save.Dispose()
    End Sub

    Function ReadLogs()
        Dim path As String = "log.txt"
        Dim SR = New IO.StreamReader(path)


        While SR.Peek <> -1
            LogTF.Text = LogTF.Text & vbCrLf & SR.ReadLine()
        End While

        SR.Close()
    End Function

    Private Sub Pressure_Closed(sender As Object, e As EventArgs) Handles Me.Closed
        Dim clean = New CancelingOrClosing

        clean.CloseAndDelete()
    End Sub

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
        Dim clean = New CancelingOrClosing

        standart.setStandart(Me)
        ComboBox1.Items.Clear()
        ComboBox1.Items.Add("Нечего сохранять")
        ComboBox1.SelectedIndex = 0
        ComboBox3.SelectedIndex = 0
        clean.CloseAndDelete()
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Dim close = New CancelingOrClosing
        ComboBox1.Items.Clear()
        close.CloseAndDelete()
        f2.Close()
        LogTF.Text = ""
        f1.ProgressBar1.Value = 0
        ComboBox1.Items.Add("Нечего сохранять")
    End Sub

    Private Sub ComboBox3_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox3.SelectedIndexChanged
        If ComboBox3.SelectedIndex = 0 Then
            Label25.Text = "сек."
            Label27.Text = "сек."
        ElseIf ComboBox3.SelectedIndex = 1 Then
            Label25.Text = "мин."
            Label27.Text = "мин."
        ElseIf ComboBox3.SelectedIndex = 2 Then
            Label25.Text = "час."
            Label27.Text = "час."
        End If
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
    Private Sub tBeginSavingTF_Leave(sender As Object, e As EventArgs) Handles tBeginSavingTF.Leave
        Dim tmp = 0
        If tBeginSavingTF.Text.Equals("") Then
            tBeginSavingTF.Text = tmp
        End If
        tBeginSavingTF.Text = tBeginSavingTF.Text.Replace(".", ",")
        If Not IsNumeric(tBeginSavingTF.Text) Then
            tBeginSavingTF.Text = tmp
        End If
    End Sub
    Private Sub tEndSavingTF_Leave(sender As Object, e As EventArgs) Handles tEndSavingTF.Leave
        Dim tmp = tEndTF.Text
        If tEndSavingTF.Text.Equals("") Then
            tEndSavingTF.Text = tmp
        End If
        tEndSavingTF.Text = tEndSavingTF.Text.Replace(".", ",")
        If Not IsNumeric(tEndSavingTF.Text) Then
            tEndSavingTF.Text = tmp
        End If
    End Sub
    Private Sub worvakKoefTF_Leave(sender As Object, e As EventArgs) Handles worvakKoefTF.Leave
        Dim tmp = 3
        If tEndSavingTF.Text.Equals("") Then
            tEndSavingTF.Text = tmp
        End If
        tEndSavingTF.Text = tEndSavingTF.Text.Replace(".", ",")
        If Not IsNumeric(tEndSavingTF.Text) Then
            tEndSavingTF.Text = tmp
        End If
    End Sub
    Private Sub vakPreHeatKoefTF_Leave(sender As Object, e As EventArgs) Handles vakPreHeatKoefTF.Leave
        Dim tmp = 10
        If vakPreHeatKoefTF.Text.Equals("") Then
            vakPreHeatKoefTF.Text = tmp
        End If
        vakPreHeatKoefTF.Text = vakPreHeatKoefTF.Text.Replace(".", ",")
        If Not IsNumeric(vakPreHeatKoefTF.Text) Then
            vakPreHeatKoefTF.Text = tmp
        End If
    End Sub
    Private Sub heatingKoefTF_Leave(sender As Object, e As EventArgs) Handles heatingKoefTF.Leave
        Dim tmp = 50
        If heatingKoefTF.Text.Equals("") Then
            heatingKoefTF.Text = tmp
        End If
        heatingKoefTF.Text = heatingKoefTF.Text.Replace(".", ",")
        If Not IsNumeric(heatingKoefTF.Text) Then
            heatingKoefTF.Text = tmp
        End If
    End Sub
    Private Sub hightTempTimeKoefTF_Leave(sender As Object, e As EventArgs) Handles hightTempTimeKoefTF.Leave
        Dim tmp = 20
        If hightTempTimeKoefTF.Text.Equals("") Then
            hightTempTimeKoefTF.Text = tmp
        End If
        hightTempTimeKoefTF.Text = hightTempTimeKoefTF.Text.Replace(".", ",")
        If Not IsNumeric(hightTempTimeKoefTF.Text) Then
            hightTempTimeKoefTF.Text = tmp
        End If
    End Sub
    Private Sub katodRisingKoefTF_Leave(sender As Object, e As EventArgs) Handles katodRisingKoefTF.Leave
        Dim tmp = 20
        If katodRisingKoefTF.Text.Equals("") Then
            katodRisingKoefTF.Text = tmp
        End If
        katodRisingKoefTF.Text = katodRisingKoefTF.Text.Replace(".", ",")
        If Not IsNumeric(katodRisingKoefTF.Text) Then
            katodRisingKoefTF.Text = tmp
        End If
    End Sub
    Private Sub heatingConstKoefTF_Leave(sender As Object, e As EventArgs) Handles heatingConstKoefTF.Leave
        Dim tmp = 20
        If heatingConstKoefTF.Text.Equals("") Then
            heatingConstKoefTF.Text = tmp
        End If
        heatingConstKoefTF.Text = heatingConstKoefTF.Text.Replace(".", ",")
        If Not IsNumeric(heatingConstKoefTF.Text) Then
            heatingConstKoefTF.Text = tmp
        End If
    End Sub
    Private Sub endKatodKoefTF_Leave(sender As Object, e As EventArgs) Handles endKatodKoefTF.Leave
        Dim tmp = 20
        If endKatodKoefTF.Text.Equals("") Then
            endKatodKoefTF.Text = tmp
        End If
        endKatodKoefTF.Text = endKatodKoefTF.Text.Replace(".", ",")
        If Not IsNumeric(endKatodKoefTF.Text) Then
            endKatodKoefTF.Text = tmp
        End If
    End Sub
    Private Sub endKoefTF_Leave(sender As Object, e As EventArgs) Handles endKoefTF.Leave
        Dim tmp = 20
        If endKoefTF.Text.Equals("") Then
            endKoefTF.Text = tmp
        End If
        endKoefTF.Text = endKoefTF.Text.Replace(".", ",")
        If Not IsNumeric(endKoefTF.Text) Then
            endKoefTF.Text = tmp
        End If
    End Sub
    Private Sub autotemp_CheckedChanged(sender As Object, e As EventArgs) Handles autotemp.CheckedChanged
        If autotemp.Checked = True Then
            guntemperatureMaxTF.Text = katodtemperatureMaxTF.Text * 0.8
        End If
    End Sub
    Private Sub drainkUdelTF_Leave(sender As Object, e As EventArgs) Handles drainkUdelTF.Leave
        Dim tmp = 10 ^ -5
        If drainkUdelTF.Text.Equals("") Then
            drainkUdelTF.Text = tmp
        End If
        drainkUdelTF.Text = drainkUdelTF.Text.Replace(".", ",")
        If Not IsNumeric(drainkUdelTF.Text) Then
            drainkUdelTF.Text = tmp
        End If
    End Sub
    Private Sub drainkAfterHeatTF_Leave(sender As Object, e As EventArgs) Handles drainkAfterHeatTF.Leave
        Dim tmp = 10 ^ -8
        If drainkAfterHeatTF.Text.Equals("") Then
            drainkAfterHeatTF.Text = tmp
        End If
        drainkAfterHeatTF.Text = drainkAfterHeatTF.Text.Replace(".", ",")
        If Not IsNumeric(drainkAfterHeatTF.Text) Then
            drainkAfterHeatTF.Text = tmp
        End If
    End Sub
    Private Sub drainqUdelTF_Leave(sender As Object, e As EventArgs) Handles drainqUdelTF.Leave
        Dim tmp = 10 ^ -8
        If drainqUdelTF.Text.Equals("") Then
            drainqUdelTF.Text = tmp
        End If
        drainqUdelTF.Text = drainqUdelTF.Text.Replace(".", ",")
        If Not IsNumeric(drainqUdelTF.Text) Then
            drainqUdelTF.Text = tmp
        End If
    End Sub
    Private Sub drainqAfterHeatTF_Leave(sender As Object, e As EventArgs) Handles drainqAfterHeatTF.Leave
        Dim tmp = 10 ^ -5
        If drainqAfterHeatTF.Text.Equals("") Then
            drainqAfterHeatTF.Text = tmp
        End If
        drainqAfterHeatTF.Text = drainqAfterHeatTF.Text.Replace(".", ",")
        If Not IsNumeric(drainqAfterHeatTF.Text) Then
            drainqAfterHeatTF.Text = tmp
        End If
    End Sub
    Private Sub drainsquareTF_Leave(sender As Object, e As EventArgs) Handles drainsquareTF.Leave
        Dim tmp = 100
        If drainsquareTF.Text.Equals("") Then
            drainsquareTF.Text = tmp
        End If
        drainsquareTF.Text = drainsquareTF.Text.Replace(".", ",")
        If Not IsNumeric(drainsquareTF.Text) Then
            drainsquareTF.Text = tmp
        End If
    End Sub
    Private Sub drainvolumeTF_Leave(sender As Object, e As EventArgs) Handles drainvolumeTF.Leave
        Dim tmp = 0.22
        If drainvolumeTF.Text.Equals("") Then
            drainvolumeTF.Text = tmp
        End If
        drainvolumeTF.Text = drainvolumeTF.Text.Replace(".", ",")
        If Not IsNumeric(drainvolumeTF.Text) Then
            drainvolumeTF.Text = tmp
        End If
    End Sub
    Private Sub bodykUdelTF_Leave(sender As Object, e As EventArgs) Handles bodykUdelTF.Leave
        Dim tmp = 10 ^ -5
        If bodykUdelTF.Text.Equals("") Then
            bodykUdelTF.Text = tmp
        End If
        bodykUdelTF.Text = bodykUdelTF.Text.Replace(".", ",")
        If Not IsNumeric(bodykUdelTF.Text) Then
            bodykUdelTF.Text = tmp
        End If
    End Sub
    Private Sub bodykAfterHeatTF_Leave(sender As Object, e As EventArgs) Handles bodykAfterHeatTF.Leave
        Dim tmp = 10 ^ -8
        If bodykAfterHeatTF.Text.Equals("") Then
            bodykAfterHeatTF.Text = tmp
        End If
        bodykAfterHeatTF.Text = bodykAfterHeatTF.Text.Replace(".", ",")
        If Not IsNumeric(bodykAfterHeatTF.Text) Then
            bodykAfterHeatTF.Text = tmp
        End If
    End Sub
    Private Sub bodyqUdelTF_Leave(sender As Object, e As EventArgs) Handles bodyqUdelTF.Leave
        Dim tmp = 10 ^ -5
        If bodyqUdelTF.Text.Equals("") Then
            bodyqUdelTF.Text = tmp
        End If
        bodyqUdelTF.Text = bodyqUdelTF.Text.Replace(".", ",")
        If Not IsNumeric(bodyqUdelTF.Text) Then
            bodyqUdelTF.Text = tmp
        End If
    End Sub
    Private Sub bodyqAfterHeatTF_Leave(sender As Object, e As EventArgs) Handles bodyqAfterHeatTF.Leave
        Dim tmp = 10 ^ -8
        If bodyqAfterHeatTF.Text.Equals("") Then
            bodyqAfterHeatTF.Text = tmp
        End If
        bodyqAfterHeatTF.Text = bodyqAfterHeatTF.Text.Replace(".", ",")
        If Not IsNumeric(bodyqAfterHeatTF.Text) Then
            bodyqAfterHeatTF.Text = tmp
        End If
    End Sub
    Private Sub bodysquareTF_Leave(sender As Object, e As EventArgs) Handles bodysquareTF.Leave
        Dim tmp = 300
        If bodysquareTF.Text.Equals("") Then
            bodysquareTF.Text = tmp
        End If
        bodysquareTF.Text = bodysquareTF.Text.Replace(".", ",")
        If Not IsNumeric(bodysquareTF.Text) Then
            bodysquareTF.Text = tmp
        End If
    End Sub
    Private Sub bodyvolumeTF_Leave(sender As Object, e As EventArgs) Handles bodyvolumeTF.Leave
        Dim tmp = 0.6
        If bodyvolumeTF.Text.Equals("") Then
            bodyvolumeTF.Text = tmp
        End If
        bodyvolumeTF.Text = bodyvolumeTF.Text.Replace(".", ",")
        If Not IsNumeric(bodyvolumeTF.Text) Then
            bodyvolumeTF.Text = tmp
        End If
    End Sub
    Private Sub katodkUdelTF_Leave(sender As Object, e As EventArgs) Handles katodkUdelTF.Leave
        Dim tmp = 10 ^ -5
        If katodkUdelTF.Text.Equals("") Then
            katodkUdelTF.Text = tmp
        End If
        katodkUdelTF.Text = katodkUdelTF.Text.Replace(".", ",")
        If Not IsNumeric(katodkUdelTF.Text) Then
            katodkUdelTF.Text = tmp
        End If
    End Sub
    Private Sub katodkAfterHeatTF_Leave(sender As Object, e As EventArgs) Handles katodkAfterHeatTF.Leave
        Dim tmp = 10 ^ -8
        If katodkAfterHeatTF.Text.Equals("") Then
            katodkAfterHeatTF.Text = tmp
        End If
        katodkAfterHeatTF.Text = katodkAfterHeatTF.Text.Replace(".", ",")
        If Not IsNumeric(katodkAfterHeatTF.Text) Then
            katodkAfterHeatTF.Text = tmp
        End If
    End Sub
    Private Sub katodqUdelTF_Leave(sender As Object, e As EventArgs) Handles katodqUdelTF.Leave
        Dim tmp = 10 ^ -5
        If katodqUdelTF.Text.Equals("") Then
            katodqUdelTF.Text = tmp
        End If
        katodqUdelTF.Text = katodqUdelTF.Text.Replace(".", ",")
        If Not IsNumeric(katodqUdelTF.Text) Then
            katodqUdelTF.Text = tmp
        End If
    End Sub
    Private Sub katodqAfterHeat_Leave(sender As Object, e As EventArgs) Handles katodqAfterHeat.Leave
        Dim tmp = 10 ^ -8
        If katodqAfterHeat.Text.Equals("") Then
            katodqAfterHeat.Text = tmp
        End If
        katodqAfterHeat.Text = katodqAfterHeat.Text.Replace(".", ",")
        If Not IsNumeric(katodqAfterHeat.Text) Then
            katodqAfterHeat.Text = tmp
        End If
    End Sub
    Private Sub katodsquareTF_Leave(sender As Object, e As EventArgs) Handles katodsquareTF.Leave
        Dim tmp = 30
        If katodsquareTF.Text.Equals("") Then
            katodsquareTF.Text = tmp
        End If
        katodsquareTF.Text = katodsquareTF.Text.Replace(".", ",")
        If Not IsNumeric(katodsquareTF.Text) Then
            katodsquareTF.Text = tmp
        End If
    End Sub
    Private Sub katodtemperatureMaxTF_Leave(sender As Object, e As EventArgs) Handles katodtemperatureMaxTF.Leave
        Dim tmp = 1400
        If katodtemperatureMaxTF.Text.Equals("") Then
            katodtemperatureMaxTF.Text = tmp
        End If
        katodtemperatureMaxTF.Text = katodtemperatureMaxTF.Text.Replace(".", ",")
        If Not IsNumeric(katodtemperatureMaxTF.Text) Then
            katodtemperatureMaxTF.Text = tmp
        End If
    End Sub
    Private Sub katodkMinTF_Leave(sender As Object, e As EventArgs) Handles katodkMinTF.Leave
        Dim tmp = 10 ^ -10
        If katodkMinTF.Text.Equals("") Then
            katodkMinTF.Text = tmp
        End If
        katodkMinTF.Text = katodkMinTF.Text.Replace(".", ",")
        If Not IsNumeric(katodkMinTF.Text) Then
            katodkMinTF.Text = tmp
        End If
    End Sub
    Private Sub katodqMinTF_Leave(sender As Object, e As EventArgs) Handles katodqMinTF.Leave
        Dim tmp = 10 ^ -10
        If katodqMinTF.Text.Equals("") Then
            katodqMinTF.Text = tmp
        End If
        katodqMinTF.Text = katodqMinTF.Text.Replace(".", ",")
        If Not IsNumeric(katodqMinTF.Text) Then
            katodqMinTF.Text = tmp
        End If
    End Sub
    Private Sub channelkUdelTF_Leave(sender As Object, e As EventArgs) Handles channelkUdelTF.Leave
        Dim tmp = 10 ^ -5
        If channelkUdelTF.Text.Equals("") Then
            channelkUdelTF.Text = tmp
        End If
        channelkUdelTF.Text = channelkUdelTF.Text.Replace(".", ",")
        If Not IsNumeric(channelkUdelTF.Text) Then
            channelkUdelTF.Text = tmp
        End If
    End Sub
    Private Sub channelkAfterHeatTF_Leave(sender As Object, e As EventArgs) Handles channelkAfterHeatTF.Leave
        Dim tmp = 10 ^ -8
        If channelkAfterHeatTF.Text.Equals("") Then
            channelkAfterHeatTF.Text = tmp
        End If
        channelkAfterHeatTF.Text = channelkAfterHeatTF.Text.Replace(".", ",")
        If Not IsNumeric(channelkAfterHeatTF.Text) Then
            channelkAfterHeatTF.Text = tmp
        End If
    End Sub
    Private Sub channelqUdelTF_Leave(sender As Object, e As EventArgs) Handles channelqUdelTF.Leave
        Dim tmp = 10 ^ -5
        If channelqUdelTF.Text.Equals("") Then
            channelqUdelTF.Text = tmp
        End If
        channelqUdelTF.Text = channelqUdelTF.Text.Replace(".", ",")
        If Not IsNumeric(channelqUdelTF.Text) Then
            channelqUdelTF.Text = tmp
        End If
    End Sub
    Private Sub channelqAfterHeatTF_Leave(sender As Object, e As EventArgs) Handles channelqAfterHeatTF.Leave
        Dim tmp = 10 ^ -8
        If channelqAfterHeatTF.Text.Equals("") Then
            channelqAfterHeatTF.Text = tmp
        End If
        channelqAfterHeatTF.Text = channelqAfterHeatTF.Text.Replace(".", ",")
        If Not IsNumeric(channelqAfterHeatTF.Text) Then
            channelqAfterHeatTF.Text = tmp
        End If
    End Sub
    Private Sub channelsquareTF_Leave(sender As Object, e As EventArgs) Handles channelsquareTF.Leave
        Dim tmp = 200
        If channelsquareTF.Text.Equals("") Then
            channelsquareTF.Text = tmp
        End If
        channelsquareTF.Text = channelsquareTF.Text.Replace(".", ",")
        If Not IsNumeric(channelsquareTF.Text) Then
            channelsquareTF.Text = tmp
        End If
    End Sub
    Private Sub gunkUdelTF_Leave(sender As Object, e As EventArgs) Handles gunkUdelTF.Leave
        Dim tmp = 10 ^ -5
        If gunkUdelTF.Text.Equals("") Then
            gunkUdelTF.Text = tmp
        End If
        gunkUdelTF.Text = gunkUdelTF.Text.Replace(".", ",")
        If Not IsNumeric(gunkUdelTF.Text) Then
            gunkUdelTF.Text = tmp
        End If
    End Sub
    Private Sub gunkAfterHeatTF_Leave(sender As Object, e As EventArgs) Handles gunkAfterHeatTF.Leave
        Dim tmp = 10 ^ -8
        If gunkAfterHeatTF.Text.Equals("") Then
            gunkAfterHeatTF.Text = tmp
        End If
        gunkAfterHeatTF.Text = gunkAfterHeatTF.Text.Replace(".", ",")
        If Not IsNumeric(gunkAfterHeatTF.Text) Then
            gunkAfterHeatTF.Text = tmp
        End If
    End Sub
    Private Sub gunqUdelTF_Leave(sender As Object, e As EventArgs) Handles gunqUdelTF.Leave
        Dim tmp = 10 ^ -5
        If gunqUdelTF.Text.Equals("") Then
            gunqUdelTF.Text = tmp
        End If
        gunqUdelTF.Text = gunqUdelTF.Text.Replace(".", ",")
        If Not IsNumeric(gunqUdelTF.Text) Then
            gunqUdelTF.Text = tmp
        End If
    End Sub
    Private Sub gunqAfterHeatTF_Leave(sender As Object, e As EventArgs) Handles gunqAfterHeatTF.Leave
        Dim tmp = 10 ^ -8
        If gunqAfterHeatTF.Text.Equals("") Then
            gunqAfterHeatTF.Text = tmp
        End If
        gunqAfterHeatTF.Text = gunqAfterHeatTF.Text.Replace(".", ",")
        If Not IsNumeric(gunqAfterHeatTF.Text) Then
            gunqAfterHeatTF.Text = tmp
        End If
    End Sub
    Private Sub gunsquareTF_Leave(sender As Object, e As EventArgs) Handles gunsquareTF.Leave
        Dim tmp = 200
        If gunsquareTF.Text.Equals("") Then
            gunsquareTF.Text = tmp
        End If
        gunsquareTF.Text = gunsquareTF.Text.Replace(".", ",")
        If Not IsNumeric(gunsquareTF.Text) Then
            gunsquareTF.Text = tmp
        End If
    End Sub
    Private Sub gunvolumeTF_Leave(sender As Object, e As EventArgs) Handles gunvolumeTF.Leave
        Dim tmp = 0.42
        If gunvolumeTF.Text.Equals("") Then
            gunvolumeTF.Text = tmp
        End If
        gunvolumeTF.Text = gunvolumeTF.Text.Replace(".", ",")
        If Not IsNumeric(gunvolumeTF.Text) Then
            gunvolumeTF.Text = tmp
        End If
    End Sub
    Private Sub guntemperatureMaxTF_Leave(sender As Object, e As EventArgs) Handles guntemperatureMaxTF.Leave
        Dim tmp = 1000
        If guntemperatureMaxTF.Text.Equals("") Then
            guntemperatureMaxTF.Text = tmp
        End If
        guntemperatureMaxTF.Text = guntemperatureMaxTF.Text.Replace(".", ",")
        If Not IsNumeric(guntemperatureMaxTF.Text) Then
            guntemperatureMaxTF.Text = tmp
        End If
        If guntemperatureMaxTF.Text <> katodtemperatureMaxTF.Text Then
            autotemp.Checked = False
        End If
    End Sub
    Private Sub gunkMinTF_Leave(sender As Object, e As EventArgs) Handles gunkMinTF.Leave
        Dim tmp = 10 ^ -10
        If gunkMinTF.Text.Equals("") Then
            gunkMinTF.Text = tmp
        End If
        gunkMinTF.Text = gunkMinTF.Text.Replace(".", ",")
        If Not IsNumeric(gunkMinTF.Text) Then
            gunkMinTF.Text = tmp
        End If
    End Sub
    Private Sub gunqMinTF_Leave(sender As Object, e As EventArgs) Handles gunqMinTF.Leave
        Dim tmp = 10 ^ -10
        If gunqMinTF.Text.Equals("") Then
            gunqMinTF.Text = tmp
        End If
        gunqMinTF.Text = gunqMinTF.Text.Replace(".", ",")
        If Not IsNumeric(gunqMinTF.Text) Then
            gunqMinTF.Text = tmp
        End If
    End Sub
End Class
