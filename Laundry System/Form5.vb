Imports MySql.Data.MySqlClient

Public Class Form5
    Private conn As New MySqlConnection("server=localhost;user id=root;password=;database=laundrydb")
    Private cmd As MySqlCommand
    Private da As MySqlDataAdapter
    Private ds As DataSet
    Private query As String
    Private currentOrderID As Integer

    ' ────────────────────────────────
    ' Form Load: fill customer list & payment methods
    ' ────────────────────────────────
    Private Sub Form5_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            conn.Open()
            ' Load customers
            cmd = New MySqlCommand("SELECT CustomerID, Name FROM Customer", conn)
            da = New MySqlDataAdapter(cmd)
            Dim dt As New DataTable
            da.Fill(dt)
            ComboBox1.DataSource = dt
            ComboBox1.ValueMember = "CustomerID"
            ComboBox1.DisplayMember = "Name"

            ' Load payment methods
            ComboBox3.Items.Clear()
            ComboBox3.Items.AddRange(New String() {"Cash", "Card", "GCash", "Other"})
            ComboBox3.SelectedIndex = 0
        Catch ex As Exception
            MsgBox("Error loading form data: " & ex.Message)
        Finally
            conn.Close()
        End Try
    End Sub

    ' ────────────────────────────────
    ' Tab Change: load pending orders or clear report tab
    ' ────────────────────────────────
    Private Sub TabControl2_SelectedIndexChanged(sender As Object, e As EventArgs) Handles TabControl2.SelectedIndexChanged
        Select Case TabControl2.SelectedIndex
            Case 1  ' Payments tab
                LoadPendingOrders()

            Case 2  ' Income Report tab
                DataGridView2.DataSource = Nothing
                DateTimePicker2.Value = DateTime.Today
                DateTimePicker3.Value = DateTime.Today
        End Select
    End Sub

    ' ────────────────────────────────
    ' TAB1: Start New Order
    ' ────────────────────────────────
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Try
            conn.Open()

            ' Start order
            Dim custID = ComboBox1.SelectedValue
            query = $"INSERT INTO LaundryOrder (CustomerID, OrderDate) VALUES ({custID}, '{DateTimePicker1.Value:yyyy-MM-dd HH:mm:ss}')"
            cmd = New MySqlCommand(query, conn)
            cmd.ExecuteNonQuery()

            ' Get new OrderID
            cmd = New MySqlCommand("SELECT LAST_INSERT_ID()", conn)
            currentOrderID = Convert.ToInt32(cmd.ExecuteScalar())

            ' Load services into grid
            cmd = New MySqlCommand("SELECT ServiceID, ServiceName, UnitPrice FROM Service", conn)
            da = New MySqlDataAdapter(cmd)
            Dim dtSvc As New DataTable
            da.Fill(dtSvc)
            dtSvc.Columns.Add("Quantity", GetType(Decimal))
            DataGridView1.DataSource = dtSvc

            MsgBox("Order started. ID = " & currentOrderID)
        Catch ex As Exception
            MsgBox("Error: " & ex.Message)
        Finally
            conn.Close()
        End Try
    End Sub

    ' ────────────────────────────────
    ' TAB1: Save Order Details
    ' ────────────────────────────────
    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Try
            conn.Open()

            For Each row As DataGridViewRow In DataGridView1.Rows
                Dim qty As Decimal
                If Not Decimal.TryParse(Convert.ToString(row.Cells("Quantity").Value), qty) OrElse qty <= 0 Then
                    Continue For
                End If

                Dim sid = row.Cells("ServiceID").Value
                query = $"INSERT INTO OrderDetail (OrderID, ServiceID, Quantity) VALUES ({currentOrderID}, {sid}, '{qty}')"
                cmd = New MySqlCommand(query, conn)
                cmd.ExecuteNonQuery()
            Next

            MsgBox("Order details saved.")
        Catch ex As Exception
            MsgBox("Error: " & ex.Message)
        Finally
            conn.Close()
        End Try
    End Sub

    ' ────────────────────────────────
    ' Load Pending Orders for TAB2
    ' ────────────────────────────────
    Private Sub LoadPendingOrders()
        Try
            conn.Open()
            cmd = New MySqlCommand("SELECT OrderID FROM LaundryOrder WHERE Status <> 'Paid'", conn)
            da = New MySqlDataAdapter(cmd)
            Dim dt As New DataTable
            da.Fill(dt)
            ComboBox2.DataSource = dt
            ComboBox2.ValueMember = "OrderID"
            ComboBox2.DisplayMember = "OrderID"
        Catch ex As Exception
            MsgBox("Error loading orders: " & ex.Message)
        Finally
            conn.Close()
        End Try
    End Sub

    ' ────────────────────────────────
    ' TAB2: Mark Order as Paid
    ' ────────────────────────────────
    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        Try
            Dim oid = ComboBox2.SelectedValue
            Dim method = ComboBox3.Text

            conn.Open()

            ' Insert Payment
            query = $"INSERT INTO Payment (OrderID, AmountPaid, PaymentDate, Method)
                      VALUES ({oid},
                        (SELECT SUM(d.Quantity * s.UnitPrice) 
                         FROM OrderDetail d JOIN Service s ON d.ServiceID = s.ServiceID 
                         WHERE d.OrderID = {oid}),
                        '{DateTime.Now:yyyy-MM-dd HH:mm:ss}',
                        '{method}')"
            cmd = New MySqlCommand(query, conn)
            cmd.ExecuteNonQuery()

            ' Update LaundryOrder status
            cmd = New MySqlCommand($"UPDATE LaundryOrder SET Status = 'Paid' WHERE OrderID = {oid}", conn)
            cmd.ExecuteNonQuery()

            MsgBox("Order " & oid & " marked Paid.")

            ' TODO: Crystal Report Viewer stub
            'Dim rpt As New rptReceipt()
            'Dim viewer As New Form6()
            'viewer.CrystalReportViewer1.ReportSource = rpt
            'viewer.Show()

        Catch ex As Exception
            MsgBox("Error processing payment: " & ex.Message)
        Finally
            conn.Close()
        End Try
    End Sub

    ' ────────────────────────────────
    ' TAB3: Income Report
    ' ────────────────────────────────
    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        Try
            conn.Open()
            Dim fromD = DateTimePicker2.Value.ToString("yyyy-MM-dd")
            Dim toD = DateTimePicker3.Value.ToString("yyyy-MM-dd")

            query = $"SELECT PaymentID, OrderID, AmountPaid, PaymentDate, Method
                      FROM Payment
                      WHERE PaymentDate BETWEEN '{fromD} 00:00:00' AND '{toD} 23:59:59'"

            cmd = New MySqlCommand(query, conn)
            da = New MySqlDataAdapter(cmd)
            Dim dt As New DataTable
            da.Fill(dt)
            DataGridView2.DataSource = dt

            ' Total computation
            Dim total As Decimal = 0
            For Each r As DataRow In dt.Rows
                total += Convert.ToDecimal(r("AmountPaid"))
            Next
            MsgBox($"Total income from {fromD} to {toD}: {total:C2}")
        Catch ex As Exception
            MsgBox("Error loading income report: " & ex.Message)
        Finally
            conn.Close()
        End Try
    End Sub
End Class

