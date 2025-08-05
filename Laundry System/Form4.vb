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
        If String.IsNullOrWhiteSpace(TextBox4.Text) Then
            MsgBox("Select a customer to delete.")
            Return
        End If

        Dim customerId = TextBox4.Text
        If MsgBox($"Delete customer {customerId} and ALL their orders/payments?", MsgBoxStyle.YesNo) <> MsgBoxResult.Yes Then
            Return
        End If

        Dim tx As MySqlTransaction = Nothing
        Try
            conn.Open()
            tx = conn.BeginTransaction()

            ' 1) Find all orders for this customer
            Dim getOrders As New MySqlCommand(
            $"SELECT OrderID FROM LaundryOrder WHERE CustomerID = {customerId}",
            conn, tx)
            Dim orders As New List(Of Integer)
            Using rdr = getOrders.ExecuteReader()
                While rdr.Read()
                    orders.Add(rdr.GetInt32(0))
                End While
            End Using

            ' 2) For each order, delete payments and order details
            For Each oid In orders
                Dim delPay = New MySqlCommand(
                $"DELETE FROM Payment WHERE OrderID = {oid}",
                conn, tx)
                delPay.ExecuteNonQuery()

                Dim delDetail = New MySqlCommand(
                $"DELETE FROM OrderDetail WHERE OrderID = {oid}",
                conn, tx)
                delDetail.ExecuteNonQuery()
            Next

            ' 3) Delete the orders themselves
            Dim delOrders = New MySqlCommand(
            $"DELETE FROM LaundryOrder WHERE CustomerID = {customerId}",
            conn, tx)
            delOrders.ExecuteNonQuery()

            ' 4) Finally delete the customer
            Dim delCust = New MySqlCommand(
            $"DELETE FROM Customer WHERE CustomerID = {customerId}",
            conn, tx)
            delCust.ExecuteNonQuery()

            ' Commit everything
            tx.Commit()
            MsgBox("Customer and all related data deleted.")
        Catch ex As Exception
            ' Roll back on any error
            If tx IsNot Nothing Then
                Try
                    tx.Rollback()
                Catch rbEx As Exception
                    MsgBox("Rollback failed: " & rbEx.Message)
                End Try
            End If
            MsgBox("Error deleting customer: " & ex.Message)
        Finally
            conn.Close()
            RefreshCustomers()   ' reload the grid
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

    ' ───────────────────────────────────────────
    ' Insert Expense (Button9_Click)
    ' ───────────────────────────────────────────
    Private Sub Button9_Click(sender As Object, e As EventArgs) Handles Button9.Click
        Try
            conn.Open()
            Dim dt = DateTimePicker3.Value.ToString("yyyy-MM-dd")
            Dim desc = TextBox11.Text.Trim()
            Dim amt = TextBox12.Text.Trim()
            query = $"INSERT INTO Expense (ExpenseDate, Description, Amount) " &
                $"VALUES ('{dt}', '{desc}', '{amt}')"
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

    ' ───────────────────────────────────────────
    ' Update Expense (Button10_Click)
    ' ───────────────────────────────────────────
    Private Sub Button10_Click(sender As Object, e As EventArgs) Handles Button10.Click
        Try
            conn.Open()
            Dim id = TextBox9.Text
            Dim dt = DateTimePicker3.Value.ToString("yyyy-MM-dd")
            Dim desc = TextBox11.Text.Trim()
            Dim amt = TextBox12.Text.Trim()
            query = $"UPDATE Expense " &
                $"SET ExpenseDate = '{dt}', Description = '{desc}', Amount = '{amt}' " &
                $"WHERE ExpenseID = {id}"
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

    ' ───────────────────────────────────────────
    ' Populate picker when grid row clicked
    ' ───────────────────────────────────────────
    Private Sub DataGridView3_CellClick(sender As Object, e As DataGridViewCellEventArgs) Handles DataGridView3.CellClick
        If e.RowIndex >= 0 Then
            Dim row = DataGridView3.Rows(e.RowIndex)
            TextBox9.Text = row.Cells("ExpenseID").Value.ToString()
            DateTimePicker3.Value = Convert.ToDateTime(row.Cells("ExpenseDate").Value)
            TextBox11.Text = row.Cells("Description").Value.ToString()
            TextBox12.Text = row.Cells("Amount").Value.ToString()
        End If
    End Sub

    ' ───────────────────────────────────────────
    ' RefreshExpenses (no change needed here)
    ' ───────────────────────────────────────────


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
            Case 0
                RefreshServices()
            Case 1
                RefreshCustomers()
            Case 2
                RefreshExpenses()
            Case 3
                ' Transactions tab: load all orders
                Button13.PerformClick()
            Case 4
                ' Sales Report tab: reset inputs & clear results
                DateTimePicker1.Value = DateTime.Today
                DateTimePicker2.Value = DateTime.Today
                DataGridView5.DataSource = Nothing
                Label1.Text = "Total Sales: ₱0.00"
            Case 5  ' TabPage6: Payments
                RefreshPayments()
                PopulateOrdersCombo()
                ' Clear fields
                TextBox10.Clear()
                ComboBox1.Text = ""
                DateTimePicker4.Value = DateTime.Today
                TextBox13.Clear()
                ComboBox2.Items.Clear()
                ComboBox2.Items.AddRange(New String() {"Cash", "Card", "GCash", "Other"})
                ComboBox2.SelectedIndex = 0
        End Select

    End Sub


    Private Sub Form4_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        RefreshServices() ' Load default tab on startup
    End Sub

    ' Refresh the orders grid when Button13 is clicked
    Private Sub Button13_Click(sender As Object, e As EventArgs) Handles Button13.Click
        Try
            conn.Open()
            query = "SELECT o.OrderID, c.Name AS Customer, o.OrderDate, o.Status " &
                "FROM LaundryOrder o " &
                "JOIN Customer c ON o.CustomerID = c.CustomerID"
            cmd = New MySqlCommand(query, conn)
            da = New MySqlDataAdapter(cmd)
            Dim dt As New DataTable
            da.Fill(dt)
            DataGridView4.DataSource = dt
        Catch ex As Exception
            MsgBox("Error loading orders: " & ex.Message)
        Finally
            conn.Close()
        End Try
    End Sub

    ' Delete the selected order (and its child records) when Button14 is clicked
    Private Sub Button14_Click(sender As Object, e As EventArgs) Handles Button14.Click
        If DataGridView4.CurrentRow Is Nothing Then
            MsgBox("Please select an order first.")
            Return
        End If

        Dim id = DataGridView4.CurrentRow.Cells("OrderID").Value
        If MsgBox($"Delete order {id} and all its details/payments?", MsgBoxStyle.YesNo) <> MsgBoxResult.Yes Then
            Return
        End If

        Try
            conn.Open()
            ' Delete payments first
            cmd = New MySqlCommand($"DELETE FROM Payment WHERE OrderID = {id}", conn)
            cmd.ExecuteNonQuery()
            ' Delete order details next
            cmd = New MySqlCommand($"DELETE FROM OrderDetail WHERE OrderID = {id}", conn)
            cmd.ExecuteNonQuery()
            ' Finally delete the order
            cmd = New MySqlCommand($"DELETE FROM LaundryOrder WHERE OrderID = {id}", conn)
            cmd.ExecuteNonQuery()

            MsgBox("Order deleted.")
            ' Refresh the grid
            Button13.PerformClick()
        Catch ex As Exception
            MsgBox("Error deleting order: " & ex.Message)
        Finally
            conn.Close()
        End Try
    End Sub
    ' Calculate and display income when Button15 is clicked
    Private Sub Button15_Click(sender As Object, e As EventArgs) Handles Button15.Click
        Try
            conn.Open()

            ' 1) Read date range
            Dim fromD = DateTimePicker1.Value.ToString("yyyy-MM-dd")
            Dim toD = DateTimePicker2.Value.ToString("yyyy-MM-dd")

            ' 2) Detailed sales query
            Dim query = "SELECT " &
                    " o.OrderID, o.OrderDate, c.Name AS CustomerName, " &
                    " s.ServiceName, od.Quantity, s.UnitPrice, " &
                    " (od.Quantity * s.UnitPrice) AS Subtotal " &
                    "FROM LaundryOrder o " &
                    " JOIN Customer c ON o.CustomerID = c.CustomerID " &
                    " JOIN OrderDetail od ON o.OrderID = od.OrderID " &
                    " JOIN Service s ON od.ServiceID = s.ServiceID " &
                    "WHERE o.OrderDate BETWEEN '" & fromD & " 00:00:00' " &
                    "                    AND '" & toD & " 23:59:59' " &
                    "ORDER BY o.OrderDate, o.OrderID;"

            cmd = New MySqlCommand(query, conn)
            da = New MySqlDataAdapter(cmd)
            Dim dt As New DataTable
            da.Fill(dt)

            ' 3) Bind to grid
            DataGridView5.DataSource = dt

            ' 4) Compute total sales
            Dim total As Decimal = 0
            For Each r As DataRow In dt.Rows
                total += Convert.ToDecimal(r("Subtotal"))
            Next

            ' 5) Display total
            Label1.Text = "Total Sales: ₱" & total.ToString("F2")
        Catch ex As Exception
            MsgBox("Error generating sales report: " & ex.Message)
        Finally
            conn.Close()
        End Try
    End Sub
    ' ────────────────────────────────────
    ' Refresh the payments grid
    ' ────────────────────────────────────
    Private Sub RefreshPayments()
        Try
            conn.Open()
            query = "SELECT PaymentID, OrderID, PaymentDate, AmountPaid, Method FROM Payment"
            cmd = New MySqlCommand(query, conn)
            da = New MySqlDataAdapter(cmd)
            ds = New DataSet()
            da.Fill(ds, "Payment")
            DataGridView6.DataSource = ds.Tables("Payment")
        Catch ex As Exception
            MsgBox("Error loading payments: " & ex.Message)
        Finally
            conn.Close()
        End Try
    End Sub

    ' ────────────────────────────────────
    ' Populate OrderID combo
    ' ────────────────────────────────────
    Private Sub PopulateOrdersCombo()
        Try
            conn.Open()
            cmd = New MySqlCommand("SELECT OrderID FROM LaundryOrder", conn)
            da = New MySqlDataAdapter(cmd)
            Dim dt As New DataTable()
            da.Fill(dt)
            ComboBox1.Items.Clear()
            For Each r As DataRow In dt.Rows
                ComboBox1.Items.Add(r("OrderID").ToString())
            Next
        Catch ex As Exception
            MsgBox("Error loading orders: " & ex.Message)
        Finally
            conn.Close()
        End Try
    End Sub

    ' ────────────────────────────────────
    ' Tab change: hook TabPage6 (index 5)
    ' ────────────────────────────────────

    ' ────────────────────────────────────
    ' When a grid row is clicked, load into inputs
    ' ────────────────────────────────────
    Private Sub DataGridView6_CellClick(sender As Object, e As DataGridViewCellEventArgs) Handles DataGridView6.CellClick
        If e.RowIndex < 0 Then Return
        Dim row = DataGridView6.Rows(e.RowIndex)
        TextBox10.Text = row.Cells("PaymentID").Value.ToString()
        ComboBox1.Text = row.Cells("OrderID").Value.ToString()
        DateTimePicker4.Value = CDate(row.Cells("PaymentDate").Value)
        TextBox13.Text = row.Cells("AmountPaid").Value.ToString()
        ComboBox2.Text = row.Cells("Method").Value.ToString()
    End Sub

    ' ────────────────────────────────────
    ' Button16: Add Payment
    ' ────────────────────────────────────
    ' ────────────────────────────────────
    ' Button16: Add Payment (TextBox10 left blank)
    ' ────────────────────────────────────
    Private Sub Button16_Click(sender As Object, e As EventArgs) Handles Button16.Click
        ' Validate required fields (excluding TextBox10)
        If ComboBox1.Text = "" Or TextBox13.Text = "" Or ComboBox2.Text = "" Then
            MsgBox("Please select OrderID, enter AmountPaid and choose Method.")
            Return
        End If

        Try
            conn.Open()
            ' Insert without specifying PaymentID (auto-increment)
            query = $"INSERT INTO Payment (OrderID, PaymentDate, AmountPaid, Method) VALUES (
                    {ComboBox1.Text},
                    '{DateTimePicker4.Value:yyyy-MM-dd HH:mm:ss}',
                    {TextBox13.Text},
                    '{ComboBox2.Text}')"
            cmd = New MySqlCommand(query, conn)
            cmd.ExecuteNonQuery()
            MsgBox("Payment added.")
        Catch ex As Exception
            MsgBox("Error adding payment: " & ex.Message)
        Finally
            conn.Close()
            RefreshPayments()
        End Try
    End Sub

    ' ────────────────────────────────────
    ' Button17: Update Payment (requires only TextBox10 + any fields to change)
    ' ────────────────────────────────────
    Private Sub Button17_Click(sender As Object, e As EventArgs) Handles Button17.Click
        ' Must have a PaymentID
        If TextBox10.Text = "" Then
            MsgBox("Please enter the PaymentID to update.")
            Return
        End If

        ' You can choose to update any subset; here we update all updatable fields
        Try
            conn.Open()
            query = $"UPDATE Payment SET
                    OrderID = {ComboBox1.Text},
                    PaymentDate = '{DateTimePicker4.Value:yyyy-MM-dd HH:mm:ss}',
                    AmountPaid = {TextBox13.Text},
                    Method = '{ComboBox2.Text}'
                  WHERE PaymentID = {TextBox10.Text}"
            cmd = New MySqlCommand(query, conn)
            cmd.ExecuteNonQuery()
            MsgBox("Payment updated.")
        Catch ex As Exception
            MsgBox("Error updating payment: " & ex.Message)
        Finally
            conn.Close()
            RefreshPayments()
        End Try
    End Sub

    ' ────────────────────────────────────
    ' Button18: Delete Payment (requires only TextBox10)
    ' ────────────────────────────────────
    Private Sub Button18_Click(sender As Object, e As EventArgs) Handles Button18.Click
        ' Must have a PaymentID to delete
        If TextBox10.Text = "" Then
            MsgBox("Please enter the PaymentID to delete.")
            Return
        End If

        If MsgBox("Are you sure you want to delete payment ID " & TextBox10.Text & "?", MsgBoxStyle.YesNo) <> MsgBoxResult.Yes Then
            Return
        End If

        Try
            conn.Open()
            query = $"DELETE FROM Payment WHERE PaymentID = {TextBox10.Text}"
            cmd = New MySqlCommand(query, conn)
            cmd.ExecuteNonQuery()
            MsgBox("Payment deleted.")
        Catch ex As Exception
            MsgBox("Error deleting payment: " & ex.Message)
        Finally
            conn.Close()
            RefreshPayments()
        End Try
    End Sub




    Private Sub DateTimePicker1_ValueChanged(sender As Object, e As EventArgs) Handles DateTimePicker1.ValueChanged

    End Sub

    Private Sub DateTimePicker2_ValueChanged(sender As Object, e As EventArgs) Handles DateTimePicker2.ValueChanged

    End Sub

    Private Sub TextBox10_TextChanged(sender As Object, e As EventArgs)

    End Sub

    Private Sub Label1_Click(sender As Object, e As EventArgs) Handles Label1.Click

    End Sub

    Private Sub TextBox10_TextChanged_1(sender As Object, e As EventArgs) Handles TextBox10.TextChanged

    End Sub

    Private Sub ComboBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox1.SelectedIndexChanged

    End Sub

    Private Sub DateTimePicker4_ValueChanged(sender As Object, e As EventArgs) Handles DateTimePicker4.ValueChanged

    End Sub

    Private Sub TextBox13_TextChanged(sender As Object, e As EventArgs) Handles TextBox13.TextChanged

    End Sub

    Private Sub ComboBox2_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox2.SelectedIndexChanged

    End Sub

    Private Sub DataGridView6_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles DataGridView6.CellContentClick

    End Sub
End Class

