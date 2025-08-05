Imports MySql.Data.MySqlClient

Public Class Form4
    Private connectionString As String = "server=localhost;user id=root;password=;database=laundrydb"
    Private conn As New MySqlConnection(connectionString)
    Private cmd As MySqlCommand
    Private da As MySqlDataAdapter
    Private ds As DataSet
    Private query As String

    ' ────────────────────────────────
    ' Load & Refresh Services Grid
    ' ────────────────────────────────
    Private Sub RefreshServices()
        Try
            conn.Open()
            query = "SELECT ServiceID, ServiceName, UnitPrice FROM Service"
            cmd = New MySqlCommand(query, conn)
            da = New MySqlDataAdapter(cmd)
            ds = New DataSet()
            da.Fill(ds, "Service")
            DataGridView1.DataSource = ds.Tables("Service")
        Catch ex As Exception
            MsgBox("Error loading services: " & ex.Message)
        Finally
            conn.Close()
        End Try
    End Sub

    ' ────────────────────────────────
    ' Populate TextBoxes on Service Row Click
    ' ────────────────────────────────
    Private Sub DataGridView1_CellClick(sender As Object, e As DataGridViewCellEventArgs) Handles DataGridView1.CellClick
        If e.RowIndex >= 0 Then
            Dim row = DataGridView1.Rows(e.RowIndex)
            TextBox1.Text = row.Cells("ServiceID").Value.ToString()
            TextBox2.Text = row.Cells("ServiceName").Value.ToString()
            TextBox3.Text = row.Cells("UnitPrice").Value.ToString()
        End If
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Try
            conn.Open()
            Dim name As String = TextBox2.Text
            Dim price As String = TextBox3.Text
            query = $"INSERT INTO Service (ServiceName, UnitPrice) VALUES ('{name}', '{price}')"
            cmd = New MySqlCommand(query, conn)
            cmd.ExecuteNonQuery()
            MsgBox("Service inserted.")
        Catch ex As Exception
            MsgBox("Error inserting service: " & ex.Message)
        Finally
            conn.Close()
            RefreshServices()
        End Try
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Try
            conn.Open()
            Dim id As String = TextBox1.Text
            Dim name As String = TextBox2.Text
            Dim price As String = TextBox3.Text
            query = $"UPDATE Service SET ServiceName = '{name}', UnitPrice = '{price}' WHERE ServiceID = {id}"
            cmd = New MySqlCommand(query, conn)
            cmd.ExecuteNonQuery()
            MsgBox("Service updated.")
        Catch ex As Exception
            MsgBox("Error updating service: " & ex.Message)
        Finally
            conn.Close()
            RefreshServices()
        End Try
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        Try
            Dim id As String = TextBox1.Text
            If MsgBox("Delete this service?", MsgBoxStyle.YesNo) <> MsgBoxResult.Yes Then Return
            conn.Open()
            query = $"DELETE FROM Service WHERE ServiceID = {id}"
            cmd = New MySqlCommand(query, conn)
            cmd.ExecuteNonQuery()
            MsgBox("Service deleted.")
        Catch ex As Exception
            MsgBox("Error deleting service: " & ex.Message)
        Finally
            conn.Close()
            RefreshServices()
        End Try
    End Sub

    ' ────────────────────────────────
    ' CUSTOMER CRUD
    ' ────────────────────────────────
    Private Sub RefreshCustomers()
        Try
            conn.Open()
            query = "SELECT CustomerID, Name, Phone, Email, Address FROM Customer"
            cmd = New MySqlCommand(query, conn)
            da = New MySqlDataAdapter(cmd)
            ds = New DataSet()
            da.Fill(ds, "Customer")
            DataGridView2.DataSource = ds.Tables("Customer")
        Catch ex As Exception
            MsgBox("Error loading customers: " & ex.Message)
        Finally
            conn.Close()
        End Try
    End Sub

    Private Sub DataGridView2_CellClick(sender As Object, e As DataGridViewCellEventArgs) Handles DataGridView2.CellClick
        If e.RowIndex >= 0 Then
            Dim row = DataGridView2.Rows(e.RowIndex)
            TextBox4.Text = row.Cells("CustomerID").Value.ToString()
            TextBox5.Text = row.Cells("Name").Value.ToString()
            TextBox6.Text = row.Cells("Phone").Value.ToString()
            TextBox7.Text = row.Cells("Email").Value.ToString()
            TextBox8.Text = row.Cells("Address").Value.ToString()
        End If
    End Sub

    Private Sub Button5_Click(sender As Object, e As EventArgs) Handles Button5.Click
        Try
            conn.Open()
            Dim name = TextBox5.Text
            Dim phone = TextBox6.Text
            Dim email = TextBox7.Text
            Dim address = TextBox8.Text
            query = $"INSERT INTO Customer (Name, Phone, Email, Address) VALUES ('{name}', '{phone}', '{email}', '{address}')"
            cmd = New MySqlCommand(query, conn)
            cmd.ExecuteNonQuery()
            MsgBox("Customer inserted.")
        Catch ex As Exception
            MsgBox("Error inserting customer: " & ex.Message)
        Finally
            conn.Close()
            RefreshCustomers()
        End Try
    End Sub

    Private Sub Button6_Click(sender As Object, e As EventArgs) Handles Button6.Click
        Try
            conn.Open()
            Dim id = TextBox4.Text
            Dim name = TextBox5.Text
            Dim phone = TextBox6.Text
            Dim email = TextBox7.Text
            Dim address = TextBox8.Text
            query = $"UPDATE Customer SET Name = '{name}', Phone = '{phone}', Email = '{email}', Address = '{address}' WHERE CustomerID = {id}"
            cmd = New MySqlCommand(query, conn)
            cmd.ExecuteNonQuery()
            MsgBox("Customer updated.")
        Catch ex As Exception
            MsgBox("Error updating customer: " & ex.Message)
        Finally
            conn.Close()
            RefreshCustomers()
        End Try
    End Sub

    Private Sub Button7_Click(sender As Object, e As EventArgs) Handles Button7.Click
        Try
            Dim id = TextBox4.Text
            If MsgBox("Delete this customer?", MsgBoxStyle.YesNo) <> MsgBoxResult.Yes Then Return

            conn.Open()
            Dim chkCmd As New MySqlCommand($"SELECT COUNT(*) FROM LaundryOrder WHERE CustomerID = {id}", conn)
            Dim cnt = Convert.ToInt32(chkCmd.ExecuteScalar())
            If cnt > 0 Then
                MsgBox("Cannot delete: customer has existing orders.")
                Return
            End If

            query = $"DELETE FROM Customer WHERE CustomerID = {id}"
            cmd = New MySqlCommand(query, conn)
            cmd.ExecuteNonQuery()
            MsgBox("Customer deleted.")
        Catch ex As Exception
            MsgBox("Error deleting customer: " & ex.Message)
        Finally
            conn.Close()
            RefreshCustomers()
        End Try
    End Sub

    Private Sub Button8_Click(sender As Object, e As EventArgs) Handles Button8.Click
        RefreshCustomers()
    End Sub

    ' ────────────────────────────────
    ' EXPENSE CRUD
    ' ────────────────────────────────
    Private Sub RefreshExpenses()
        Try
            conn.Open()
            query = "SELECT ExpenseID, ExpenseDate, Description, Amount FROM Expense"
            cmd = New MySqlCommand(query, conn)
            da = New MySqlDataAdapter(cmd)
            ds = New DataSet()
            da.Fill(ds, "Expense")
            DataGridView3.DataSource = ds.Tables("Expense")
        Catch ex As Exception
            MsgBox("Error loading expenses: " & ex.Message)
        Finally
            conn.Close()
        End Try
    End Sub

    Private Sub DataGridView3_CellClick(sender As Object, e As DataGridViewCellEventArgs) Handles DataGridView3.CellClick
        If e.RowIndex >= 0 Then
            Dim row = DataGridView3.Rows(e.RowIndex)
            TextBox9.Text = row.Cells("ExpenseID").Value.ToString()
            TextBox10.Text = CType(row.Cells("ExpenseDate").Value, Date).ToString("yyyy-MM-dd")
            TextBox11.Text = row.Cells("Description").Value.ToString()
            TextBox12.Text = row.Cells("Amount").Value.ToString()
        End If
    End Sub

    Private Sub Button9_Click(sender As Object, e As EventArgs) Handles Button9.Click
        Try
            conn.Open()
            Dim dt = TextBox10.Text
            Dim desc = TextBox11.Text
            Dim amt = TextBox12.Text
            query = $"INSERT INTO Expense (ExpenseDate, Description, Amount) VALUES ('{dt}', '{desc}', '{amt}')"
            cmd = New MySqlCommand(query, conn)
            cmd.ExecuteNonQuery()
            MsgBox("Expense inserted.")
        Catch ex As Exception
            MsgBox("Error inserting expense: " & ex.Message)
        Finally
            conn.Close()
            RefreshExpenses()
        End Try
    End Sub

    Private Sub Button10_Click(sender As Object, e As EventArgs) Handles Button10.Click
        Try
            conn.Open()
            Dim id = TextBox9.Text
            Dim dt = TextBox10.Text
            Dim desc = TextBox11.Text
            Dim amt = TextBox12.Text
            query = $"UPDATE Expense SET ExpenseDate = '{dt}', Description = '{desc}', Amount = '{amt}' WHERE ExpenseID = {id}"
            cmd = New MySqlCommand(query, conn)
            cmd.ExecuteNonQuery()
            MsgBox("Expense updated.")
        Catch ex As Exception
            MsgBox("Error updating expense: " & ex.Message)
        Finally
            conn.Close()
            RefreshExpenses()
        End Try
    End Sub

    Private Sub Button11_Click(sender As Object, e As EventArgs) Handles Button11.Click
        Try
            Dim id = TextBox9.Text
            If MsgBox("Delete this expense?", MsgBoxStyle.YesNo) <> MsgBoxResult.Yes Then Return

            conn.Open()
            query = $"DELETE FROM Expense WHERE ExpenseID = {id}"
            cmd = New MySqlCommand(query, conn)
            cmd.ExecuteNonQuery()
            MsgBox("Expense deleted.")
        Catch ex As Exception
            MsgBox("Error deleting expense: " & ex.Message)
        Finally
            conn.Close()
            RefreshExpenses()
        End Try
    End Sub

    Private Sub Button12_Click(sender As Object, e As EventArgs) Handles Button12.Click
        RefreshExpenses()
    End Sub

    ' ───────────────────────────────────────────
    ' Single Tab‐Change Handler to Refresh Grids
    ' ───────────────────────────────────────────
    Private Sub TabControl1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles TabControl1.SelectedIndexChanged
        Select Case TabControl1.SelectedIndex
            Case 0 : RefreshServices()
            Case 1 : RefreshCustomers()
            Case 2 : RefreshExpenses()
                ' You can extend this if more tabs are added later.
        End Select
    End Sub

    Private Sub Form4_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        RefreshServices() ' Load default tab on startup
    End Sub
End Class

