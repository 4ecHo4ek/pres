Public Class WriteAndReadToFile

    Function WriteBeginigDataToFile(ByVal f1 As Pressure) As Boolean
        Dim folder As String = "allInfo"
        Dim save = New CalculatingAndStandartParams

        If IO.Directory.Exists(folder) Then
            IO.Directory.Delete(folder, True)
        End If
        IO.Directory.CreateDirectory(folder)

        If Not save.SetDataParams(f1) Then
            WriteLogs("Ошибка подготовки исходных файлов")
            Return False
        End If
        If Not save.SetUnitParams(f1) Then
            WriteLogs("Ошибка подготовки исходных файлов")
            Return False

        End If
        If Not save.SetDrawingParams(f1) Then
            WriteLogs("Ошибка подготовки исходных файлов")
            Return False
        End If

        Return True
    End Function

    Function SetStructForCalcPressAsData(ByVal data As Data) As Data ' читаем

        Dim formatter As New Runtime.Serialization.Formatters.Binary.BinaryFormatter()
        Using stream As IO.Stream = IO.File.Open("allInfo\data.data", IO.FileMode.Open)
            data = formatter.Deserialize(stream)
        End Using
        Return data
    End Function

    Function SetStructForCalcPressAsUnit(ByVal unit As Unit) As Unit

        Dim formatter As New Runtime.Serialization.Formatters.Binary.BinaryFormatter()
        Using stream As IO.Stream = IO.File.Open("allInfo\units.data", IO.FileMode.Open)
            unit = formatter.Deserialize(stream) 'Читаем из файла
        End Using
        Return unit
    End Function

    Function WritePressuer(ByVal pressure() As Double, ByVal name As String, ByVal tStart As Double, ByVal tEnd As Double, ByVal data As Data)
        Dim drawing = New PressureParams
        Dim min, max As Double

        min = pressure(tStart)
        max = pressure(tStart)
        For i As Integer = tStart To tEnd - 1
            If max < pressure(i) Then
                max = pressure(i)
            End If
            If min > pressure(i) Then
                min = pressure(i)
            End If
        Next i
        drawing.pressure = pressure
        drawing.pMax = max
        drawing.pMin = min
        drawing.t1 = data.t1
        drawing.tStartHeatRising = data.tStartHeatRising
        drawing.tEndHeatRising = data.tEndHeatRising
        drawing.tKatodActivate = data.tKatodActivate
        drawing.tKatodActive = data.tKatodActive
        drawing.tEndHeating = data.tEndHeating
        drawing.tKatodDisactivate = data.tKatodDisactivate
        drawing.tEnd = data.tEnd
        Dim formatter As New Runtime.Serialization.Formatters.Binary.BinaryFormatter()
        Using stream As IO.Stream = IO.File.Create(name)
            formatter.Serialize(stream, drawing)
        End Using
    End Function

    Function ReadPressure(ByVal pressParam As PressureParams, ByVal name As String) As PressureParams

        Dim formatter As New Runtime.Serialization.Formatters.Binary.BinaryFormatter()
        Using stream As IO.Stream = IO.File.Open(name, IO.FileMode.Open)
            pressParam = formatter.Deserialize(stream) 'Читаем из файла
        End Using
        Return pressParam
    End Function

    Function ReadDrawingData(ByVal drawingData As DrawingData) As DrawingData

        Dim formatter As New Runtime.Serialization.Formatters.Binary.BinaryFormatter()
        Using stream As IO.Stream = IO.File.Open("allInfo\draw.data", IO.FileMode.Open)
            drawingData = formatter.Deserialize(stream) 'Читаем из файла
        End Using
        Return drawingData
    End Function

    Function WriteToSavingPressureFile()
        Dim path As String = "pressSaving.txt"
        Dim message As String = " вычисление"
        Dim wr = New IO.StreamWriter(path, append:=True)
        Dim num As Integer = calculationCount + 1

        message = CStr(num) + message
        If Not IO.File.Exists(path) Then
            IO.File.Create(path)
        End If
        wr.WriteLine(Message)
        wr.Close()
    End Function

    Function ReadExcelSettings(ByVal settings As ExcelSaving) As ExcelSaving

        Dim formatter As New Runtime.Serialization.Formatters.Binary.BinaryFormatter()
        Using stream As IO.Stream = IO.File.Open("excel.data", IO.FileMode.Open)
            settings = formatter.Deserialize(stream) 'Читаем из файла
        End Using

        Return settings
    End Function

    Function WriteForSaving(ByVal settings As ExcelSaving)
        Try
            If IO.File.Exists("excel.data") Then
                IO.File.Delete("excel.data")
            End If
            Dim formatter As New Runtime.Serialization.Formatters.Binary.BinaryFormatter()
            Using stream As IO.Stream = IO.File.Create("excel.data")
                formatter.Serialize(stream, settings)
            End Using
            Return True
        Catch ex As Exception
            f1.LogTF.Text = f1.LogTF.Text & vbCrLf & "ошибка формирования начальных данных прибора"
            Return False
        End Try
    End Function

    Function WriteLogs(ByVal message As String)
        Dim path As String = "log.txt"
        Dim wr = New IO.StreamWriter(path, append:=True)
        If Not IO.File.Exists(path) Then
            IO.File.Create(path)
        End If

        wr.WriteLine(message)
        wr.Close()
    End Function
End Class
