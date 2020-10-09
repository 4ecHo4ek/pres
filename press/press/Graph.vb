Imports System.ComponentModel

Public Class Graph

    Private Sub Graph_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Graphic.Height = Me.Height
        Graphic.Width = Me.Width
        '  f1.Button2.Enabled = True
    End Sub

    Private Sub Graph_Closed(sender As Object, e As EventArgs) Handles Me.Closed
        Dim close = New CancelingOrClosing
        close.CloseAndDelete()
    End Sub

End Class