Public Class CancelingOrClosing
    Function CloseAndDelete()
        calculationCount = 0

        If IO.Directory.Exists("pressures") Then
            IO.Directory.Delete("pressures", True)
        End If

        If IO.Directory.Exists("allinfo") Then
            IO.Directory.Delete("allinfo", True)
        End If

        If IO.Directory.Exists("bmp") Then
            IO.Directory.Delete("bmp", True)
        End If

        If IO.File.Exists("log.txt") Then
            IO.File.Delete("log.txt")
        End If

        If IO.File.Exists("pressSaving.txt") Then
            IO.File.Delete("pressSaving.txt")
        End If
    End Function
End Class
