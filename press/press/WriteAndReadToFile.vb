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

    Function WritePressuer(ByVal pressure() As Double, ByVal name As String, ByVal tStart As Double, ByVal tEnd As Double)
        Dim drawing = New DrawingPart.PressureParams
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
        Dim formatter As New Runtime.Serialization.Formatters.Binary.BinaryFormatter()
        Using stream As IO.Stream = IO.File.Create(name)
            formatter.Serialize(stream, drawing)
        End Using
    End Function

    Function ReadPressure(ByVal pressParam As DrawingPart.PressureParams, ByVal name As String) As DrawingPart.PressureParams

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
