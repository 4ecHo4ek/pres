Public Class CancelingOrClosing
    Function Close()
        calculationCount = 0

        If IO.Directory.Exists("pressures") Then
            IO.Directory.Delete("pressures", True)
        End If

        If IO.Directory.Exists("partsfordrawing") Then
            IO.Directory.Delete("partsfordrawing", True)
        End If

        If IO.Directory.Exists("allinfo") Then
            IO.Directory.Delete("allinfo", True)
        End If

        If IO.Directory.Exists("bmp") Then
            IO.Directory.Delete("bmp", True)
        End If
    End Function
End Class
