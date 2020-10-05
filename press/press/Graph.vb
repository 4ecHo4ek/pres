Imports System.ComponentModel

Public Class Graph

    Private Sub Graph_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Graphic.Height = Me.Height
        Graphic.Width = Me.Width
    End Sub

    Private Sub Graph_Closed(sender As Object, e As EventArgs) Handles Me.Closed
        Dim close = New CancelingOrClosing
        close.Close()
    End Sub
End Class