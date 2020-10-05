Public Class DrawingPart

    <System.Serializable()> Structure PartsForDrawing
        Dim height As Integer ' габариты экрана (const)
        Dim width As Integer ' габариты экрана (const)
        Dim tStart As Double ' начальный отрезок для отрисовки (const)
        Dim tEnd As Double ' конечный отрезок для отрисовки (const)

        Dim xDelta As Double ' начальное смещение для времени 
        Dim hParam As Double ' коэф для нормирования 
        Dim wParam As Double ' коэф для нормирования 
        Dim pMax As Double ' максимальное давление
        Dim pMin As Double ' минимальное давление
        Dim countDraw As Integer ' счетчик для смены цвета
    End Structure

    <System.Serializable()> Structure PressureParams
        Dim pMax As Double
        Dim pMin As Double
        Dim pressure() As Double
    End Structure


    Function Draw()

        Dim btMap As Bitmap
        Dim drawingData = New DrawingData
        Dim parts = New PartsForDrawing
        Dim write = New WriteToFile
        Dim pressParam = New PressureParams
        Dim pressure(calculationCount + 1)() As Double
        Dim filePath As String = "bmp\"
        Dim fileName = filePath + CStr(calculationCount) + ".bmp"


        Try
            drawingData = write.ReadDrawingData(drawingData)
            parts = SetCorrect(parts) ' должен быть в pressure ?
            pressure = PreparePressures(pressure, write)
            parts = PreparePressuresParams(parts, write)
            parts = SetConstParams(parts, drawingData)
        Catch ex As Exception
            f1.LogTF.Text = f1.LogTF.Text & vbCrLf & "ошибка сбора данных построения графика"
            Exit Function
        End Try

        Try
            If calculationCount = 0 Then
                btMap = New Bitmap(parts.width, parts.height) ' 1
                g = Graphics.FromImage(btMap) ' 1
            Else
                Dim fileName1 = filePath + CStr(calculationCount - 1) + ".bmp"
                btMap = New Bitmap(fileName1)
                g = Graphics.FromImage(btMap)
            End If
        Catch
            f1.LogTF.Text = f1.LogTF.Text & vbCrLf & "ошибка подготовки поля рисурка"
            Exit Function
        End Try
        f1.ProgressBar1.Value = 8
        Try
            parts = SetKoefForDraw(parts)
            setAxis(g, parts)
            DrawGraph(g, pressure, parts)
        Catch
            f1.LogTF.Text = f1.LogTF.Text & vbCrLf & "ошибка формирования изображения графика"
            Exit Function
        End Try

        Dim newFileName = filePath + CStr(calculationCount) + ".bmp"
        Dim filestream As System.IO.FileStream

        Try
            filestream = New System.IO.FileStream(newFileName, IO.FileMode.OpenOrCreate)
            btMap.Save(filestream, Imaging.ImageFormat.Bmp)
            filestream.Close()
            g.Dispose()
            btMap.Dispose()
        Catch
            f1.LogTF.Text = f1.LogTF.Text & vbCrLf & "ошибка сохранения изображения графика"
            Exit Function
        End Try
        f1.ProgressBar1.Value = 9
    End Function

    Function PreparePressuresParams(ByVal parts As PartsForDrawing, ByVal write As WriteToFile) As PartsForDrawing
        Dim pMax(calculationCount + 1) As Double
        Dim pMin(calculationCount + 1) As Double
        Dim pressParam = New PressureParams
        Dim min, max As Double

        For i As Integer = 0 To calculationCount
            Dim name = "pressures\pressure" + CStr(i) + ".data"
            pressParam = write.ReadPressure(pressParam, name)
            pMax(i) = pressParam.pMax
            pMin(i) = pressParam.pMin
        Next
        min = pMax(0)
        max = pMin(0)
        For i As Integer = 0 To calculationCount
            If max < pMax(i) Then
                max = pMax(i)
            End If
            If min > pMin(i) Then
                min = pMin(i)
            End If
        Next i
        parts.pMax = max
        parts.pMin = min
        Return parts
    End Function

    Function PreparePressures(ByVal pressure()() As Double, ByVal write As WriteToFile) As Double()()
        Dim pressParam = New PressureParams

        For i As Integer = 0 To calculationCount
            Dim name = "pressures\pressure" + CStr(i) + ".data"
            pressParam = write.ReadPressure(pressParam, name)
            pressure(i) = pressParam.pressure
        Next
        Return pressure
    End Function


    Function SetConstParams(ByVal parts As PartsForDrawing, ByVal drawingData As DrawingData) As PartsForDrawing
        parts.height = drawingData.height
        parts.width = drawingData.width
        parts.tStart = drawingData.tStart
        parts.tEnd = drawingData.tEnd
        Return parts
    End Function


    'проверяем правильность установления параметров времени
    Function SetCorrect(ByVal parts As PartsForDrawing) As PartsForDrawing ' это вообще делать перед отправкой
        Dim tmp As Integer
        ' вводить не меньше 10 сек разницы (логи)
        parts.tStart = Math.Abs(parts.tStart)
        parts.tEnd = Math.Abs(parts.tEnd)
        If parts.tStart > parts.tEnd Then
            tmp = parts.tStart
            parts.tStart = parts.tEnd
            parts.tEnd = tmp
        ElseIf parts.tEnd - parts.tStart < 20 Then
            parts.tStart += 10
        End If
        Return parts
    End Function


    ' прописать чтоб строились 1 раз до нажатия на кнопку стерания
    Function setAxis(ByVal g As Graphics, ByVal part As PartsForDrawing) ' переработать
        Dim valueP As Double
        Dim valueT As Double
        Dim coordP As Integer
        Dim coordT As Integer
        Dim pMin As Double
        Dim pMax As Double
        Dim font As New Font("arial", 8, FontStyle.Regular)
        Dim fontForNames As New Font("arial", 10, FontStyle.Italic)
        Dim deltaT As Integer
        Dim name As String
        Dim time As Double
        Dim pres As Double

        g.FillRectangle(Brushes.White, 0, 0, part.width, part.height)
        name = ""
        deltaT = part.tEnd - part.tStart
        g.DrawLine(Pens.Black, 40, 0, 40, part.height - 80)
        g.DrawLine(Pens.Black, 40, part.height - 80, part.width - 20, part.height - 80)

        pMax = 20
        pMin = 20 + part.hParam * (Math.Log10(part.pMax) - Math.Log10(part.pMin))
        For i As Integer = 0 To 9
            valueP = part.pMax + (part.pMin - part.pMax) * i / 9
            valueT = part.tStart + (part.tEnd - part.tStart) * i / 9
            coordP = pMax - (pMax - pMin) * i / 9
            coordT = part.wParam * part.tStart + part.xDelta + (part.wParam * part.tEnd - part.wParam * part.tStart) * i / 9

            pres = Math.Log10(part.pMax) - (Math.Log10(part.pMax) - Math.Log10(part.pMin)) * i / 9
            g.DrawLine(Pens.Black, coordT, part.height - 75, coordT, part.height - 85) ' t
            g.DrawLine(Pens.Black, 35, coordP, 45, coordP) ' p
            If deltaT < 600 Then
                time = part.tStart + (part.tEnd - part.tStart) * i / 9
                name = "t, сек."
            ElseIf deltaT > 18000 Then
                time = (part.tStart + (part.tEnd - part.tStart) * i / 9) / 3600
                name = "t, час."
            Else
                time = (part.tStart + (part.tEnd - part.tStart) * i / 9) / 60
                name = "t, мин."
            End If
            g.DrawString(Format(time, "0.#"), font, Brushes.Black, coordT, part.height - 70)
            g.DrawString(Format(pres, "0.#"), font, Brushes.Black, 5, coordP)
        Next
        ' проработать механизм прикрепления лейблов к краям
        g.DrawString(name, fontForNames, Brushes.Black, part.width - 60, part.height - 100)
        g.DrawString("Lg(p)", fontForNames, Brushes.Black, 5, 0)


    End Function

    Function SetKoefForDraw(ByVal part As PartsForDrawing) As PartsForDrawing ' 1
        part.hParam = (part.height - 130) / (Math.Log10(part.pMax) - Math.Log10(part.pMin))
        part.wParam = (part.width - 130) / (part.tEnd - part.tStart)
        part.xDelta = 50 - part.wParam * part.tStart
        Return part
    End Function

    Function DrawGraph(ByVal g As Graphics, ByVal pressure()() As Double, ByVal part As PartsForDrawing)
        Dim hParam As Double
        Dim wParam As Double
        Dim colors As Pen() = {Pens.Red, Pens.Green, Pens.Gray, Pens.Gold, Pens.Aqua, Pens.Blue, Pens.Brown, Pens.Coral, Pens.Crimson, Pens.DarkRed, Pens.Honeydew, Pens.Lime}

        hParam = part.hParam
        wParam = part.wParam
        For i As Integer = 0 To calculationCount
            part.countDraw = i

            If part.countDraw > 12 Then
                part.countDraw = part.countDraw Mod 12
            End If

            For j As Integer = part.tStart To part.tEnd - 1
                g.DrawLine(colors(part.countDraw), CSng(part.xDelta + wParam * j), CSng(20 + hParam * (Math.Log10(part.pMax) - Math.Log10(pressure(i)(j)))),
                CSng(part.xDelta + wParam * (j + 1)), CSng(20 + hParam * (Math.Log10(part.pMax) - Math.Log10(pressure(i)(j + 1)))))
            Next j
        Next i

    End Function
End Class
