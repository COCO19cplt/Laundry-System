Imports MySql.Data.MySqlClient

Public Class CreateAcc
    Private conn As New MySqlConnection(
        "server=localhost;userid=root;password=;database=laundrydb;")
    Private cmd As MySqlCommand

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        ' Read & escape inputs
        Dim name = TextBox1.Text.Trim().Replace("'", "''")
        Dim user = TextBox2.Text.Trim().Replace("'", "''")
        Dim pass = TextBox3.Text.Trim().Replace("'", "''")
        Dim confirm = TextBox4.Text.Trim().Replace("'", "''")
        Dim phone = TextBox5.Text.Trim().Replace("'", "''")
        Dim email = TextBox6.Text.Trim().Replace("'", "''")
        Dim addr = TextBox7.Text.Trim().Replace("'", "''")

        ' Validate all fields
        If name = "" OrElse user = "" OrElse pass = "" OrElse confirm = "" _
           OrElse phone = "" OrElse email = "" OrElse addr = "" Then
            MsgBox("Please fill in every field.")
            Return
        End If
        If pass <> confirm Then
            MsgBox("Passwords do not match.")
            Return
        End If

        Try
            conn.Open()

            ' Ensure unique username
            cmd = New MySqlCommand(
                "SELECT COUNT(*) FROM Users WHERE Username='" & user & "'",
                conn)
            If Convert.ToInt32(cmd.ExecuteScalar()) > 0 Then
                MsgBox("Username already exists.")
                Return
            End If

            ' Insert into Users
            Dim qU = "INSERT INTO Users (Username, Password, Email, Role) " &
                     "VALUES ('" & user & "','" & pass & "','" & email & "','Customer')"
            cmd = New MySqlCommand(qU, conn)
            cmd.ExecuteNonQuery()

            ' Insert into Customer
            Dim qC = "INSERT INTO Customer (Name, Phone, Email, Address) " &
                     "VALUES ('" & name & "','" & phone & "','" & email & "','" & addr & "')"
            cmd = New MySqlCommand(qC, conn)
            cmd.ExecuteNonQuery()

            ' Grab the new CustomerID for immediate use
            cmd = New MySqlCommand("SELECT LAST_INSERT_ID()", conn)
            Form2.LoggedInCustomerID = Convert.ToInt32(cmd.ExecuteScalar())

            MsgBox("Registration successful! You can now log in.")
            Me.Hide()
            Form2.Show()

        Catch ex As Exception
            MsgBox("Error during registration: " & ex.Message)
        Finally
            conn.Close()
        End Try
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Me.Hide()
        Form2.Show()
    End Sub





    Private Sub TextBox2_TextChanged(sender As Object, e As EventArgs) Handles TextBox2.TextChanged

    End Sub

    Private Sub MaskedTextBox1_MaskInputRejected(sender As Object, e As MaskInputRejectedEventArgs)

    End Sub

    Private Sub TextBox3_TextChanged(sender As Object, e As EventArgs) Handles TextBox3.TextChanged

    End Sub

    Private Sub TextBox1_TextChanged(sender As Object, e As EventArgs) Handles TextBox1.TextChanged

    End Sub

    Private Sub TextBox4_TextChanged(sender As Object, e As EventArgs) Handles TextBox4.TextChanged

    End Sub
End Class
