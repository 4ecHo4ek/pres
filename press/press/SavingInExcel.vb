Public Class SavingInExcel
    Function SaveInExcel(ByVal number As Integer, ByVal path As String)
        If calculationCount = 0 Then
            Dim write = New WriteAndReadToFile
            write.WriteLogs("Нечего сохранять")
            Exit Function
        End If

        Dim read = New WriteAndReadToFile
        Dim settings = New ExcelSaving
        Dim exl As Object
        Dim timeName = "время"
        Dim pressure = New PressureParams

        exl = CreateObject("excel.application")
        exl.workbooks.add()
        settings = read.ReadExcelSettings(settings)
        Dim koefForTime As Integer = settings.koef
        Dim nameFile As String = "pressures\pressure" + CStr(koefForTime) + ".data"
        pressure = read.ReadPressure(pressure, nameFile)

        Select Case koefForTime
            Case 0
                timeName = "сек."
            Case 1
                timeName = "мин."
            Case 2
                timeName = "час."
        End Select

        If exl.visible = False Then exl.visible = True
        exl.range("A1").value = "этап откачки"
        exl.range("B1").value = "время, " + timeName
        exl.range("C1").value = "давление, Па."
        WriteInExcel(exl, pressure, settings)

    End Function

    Function WriteInExcel(ByVal exl As Object, ByVal pressure As PressureParams, ByVal settings As ExcelSaving)
        Dim timeKoef, k As Integer
        Dim time As Double
        Dim tB, tE As Double
        Dim numParts As Integer = 1
        Dim koefForTime As Integer = settings.koef
        Dim pres = pressure.pressure
        Dim flag As Boolean = True

        time = settings.tBeginSaving
        k = 1
        While time < settings.tEndSaving
            If time < pressure.t1 And flag Then
                exl.range("A" & k).value = "форврак. откачка"
                tB = 0
                tE = pressure.t1
                numParts = settings.worvakKoef
                flag = Not flag
            ElseIf time < pressure.tStartHeatRising And Not flag Then
                exl.range("A" & k).value = "вакуумн. откачка"
                tB = pressure.t1
                tE = pressure.tStartHeatRising
                numParts = settings.vakPreHeatKoef
                flag = Not flag
            ElseIf time <= pressure.tEndHeatRising And flag Then
                exl.range("A" & k).value = "вакуумн. откачка при прогреве камеры"
                tB = pressure.tStartHeatRising
                tE = pressure.tEndHeatRising
                numParts = settings.heatingKoef
                flag = Not flag
            ElseIf time < pressure.tKatodActivate And Not flag Then
                exl.range("A" & k).value = "откачка до прокала катода"
                tB = pressure.tEndHeatRising
                tE = pressure.tKatodActivate
                numParts = settings.hightTempTimeKoef
                flag = Not flag
            ElseIf time <= pressure.tKatodActive And flag Then
                exl.range("A" & k).value = "откачка при прокале катода"
                tB = pressure.tKatodActivate
                tE = pressure.tKatodActive
                numParts = settings.katodRisingKoef
                flag = Not flag
            ElseIf time < pressure.tEndHeating And Not flag Then
                exl.range("A" & k).value = "откачка до отключения прогрева"
                tB = pressure.tKatodActive
                tE = pressure.tEndHeating
                numParts = settings.heatingConstKoef
                flag = Not flag
            ElseIf time < pressure.tKatodDisactivate And flag Then
                exl.range("A" & k).value = "откачка до отключения тока катода"
                tB = pressure.tEndHeating
                tE = pressure.tKatodDisactivate
                numParts = settings.endKatodKoef
                flag = Not flag
            ElseIf time <= pressure.tEnd And Not flag Then
                exl.range("A" & k).value = "откачка до окончания процесса"
                tB = pressure.tKatodDisactivate
                tE = pressure.tEnd
                numParts = settings.endKoef
                flag = Not flag
            End If

            If numParts < 1 Then
                numParts = 1
            End If

            If (tE - tB) Mod numParts <> 0 Then
                timeKoef = (tE - tB) Mod numParts
            End If

            For i As Integer = 0 To numParts - 1
                time = tB + (tE - tB - timeKoef) * i / (numParts - 1)
                exl.range("B" & k).value = Format(time / 60 ^ koefForTime, "0.##")
                exl.range("c" & k).value = Format(pres(time), "Scientific")
                k += 1
            Next
            If tE = pressure.tEnd Then
                Exit While
            End If

        End While
        exl.range("B" & k).value = Format(pressure.tEnd / 60 ^ koefForTime, "0.##")
        exl.range("c" & k).value = Format(pres(pressure.tEnd), "Scientific")

    End Function

    Function MakeData(ByVal data As ExcelSaving, ByVal f1 As Pressure) As ExcelSaving
        data.tBeginSaving = f1.tBeginSavingTF.Text
        data.tEndSaving = f1.tEndSavingTF.Text
        data.worvakKoef = f1.worvakKoefTF.Text
        data.vakPreHeatKoef = f1.vakPreHeatKoefTF.Text
        data.heatingKoef = f1.heatingKoefTF.Text
        data.hightTempTimeKoef = f1.hightTempTimeKoefTF.Text
        data.katodRisingKoef = f1.katodRisingKoefTF.Text
        data.heatingConstKoef = f1.heatingConstKoefTF.Text
        data.endKatodKoef = f1.endKatodKoefTF.Text
        data.endKoef = f1.endKoefTF.Text
        Return data
    End Function
End Class

<System.Serializable()> Public Structure ExcelSaving
    Public koef As Integer ' коэф перевода времени
    Public tBeginSaving As Double
    Public tEndSaving As Double
    Public worvakKoef As Integer ' число сохранений для форвака
    Public vakPreHeatKoef As Integer
    Public heatingKoef As Integer
    Public hightTempTimeKoef As Integer
    Public katodRisingKoef As Integer
    Public heatingConstKoef As Integer
    Public endKatodKoef As Integer
    Public endKoef As Integer

End Structure
