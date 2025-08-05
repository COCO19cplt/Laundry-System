Imports MySql.Data.MySqlClient

Public Class Form2
    ' Hard-coded admin credentials
    Private Const ADMIN_USER As String = "owner"
    Private Const ADMIN_PASS As String = "laundry123"

    ' Will hold the CustomerID after registration or login lookup
    Public Shared LoggedInCustomerID As Integer = 0

    Private conn As New MySqlConnection(
        "server=localhost;userid=root;password=;database=laundrydb;")
    Private cmd As MySqlCommand

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim user = TextBox1.Text.Trim().Replace("'", "''")
        Dim pass = TextBox2.Text.Trim().Replace("'", "''")

        ' — Admin login —
        If user = ADMIN_USER AndAlso pass = ADMIN_PASS Then
            MsgBox("Admin login successful.")
            Me.Hide()
            Form4.Show()
            Return
        End If

        ' — Customer login or immediate post-registration —
        Try
            conn.Open()

            ' 1) Validate credentials in Users
            Dim q1 = "SELECT UserID FROM Users " &
                     "WHERE Username='" & user & "' " &
                     "  AND Password='" & pass & "' " &
                     "  AND Role='Customer'"
            cmd = New MySqlCommand(q1, conn)
            Dim userRes = cmd.ExecuteScalar()
            If userRes Is Nothing Then
                MsgBox("Invalid credentials.")
                Return
            End If

            ' 2) If we just registered, use that ID; otherwise lookup by Name
            If LoggedInCustomerID = 0 Then
                ' Lookup the customer profile (match by Username→Name)
                Dim q2 = "SELECT CustomerID FROM Customer " &
                         "WHERE Name='" & user & "'"
                cmd = New MySqlCommand(q2, conn)
                Dim custRes = cmd.ExecuteScalar()
                If custRes Is Nothing Then
                    MsgBox("Customer profile not found. Please register first.")
                    Return
                End If
                LoggedInCustomerID = Convert.ToInt32(custRes)
            End If

            MsgBox("Customer login successful.")
            Me.Hide()
            Form7.UserID = LoggedInCustomerID
            Form7.Show()

        Catch ex As Exception
            MsgBox("Error during login: " & ex.Message)
        Finally
            conn.Close()
        End Try
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        ' Go to registration
        Me.Hide()
        CreateAcc.Show()
    End Sub
End Class


