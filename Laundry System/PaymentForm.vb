Imports MySql.Data.MySqlClient

Public Class PaymentForm
    Dim conn As New MySqlConnection(
        "server=localhost;userid=root;password=;database=laundrydb;")
    Dim cmd As MySqlCommand
    Dim da As MySqlDataAdapter

    ' Ensure the connection is open
    Private Sub EnsureOpen()
        If conn.State <> ConnectionState.Open Then conn.Open()
    End Sub

    ' ────────────────────────────────────────
    ' On load: fill orders combo, init grid, reset UI
    ' ────────────────────────────────────────
    Private Sub PaymentForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        PopulateOrderCombo()
        InitializeGrid()
        ResetPaymentInputs()
    End Sub

    ' Fill ComboBox1 with this customer’s pending orders
    Private Sub PopulateOrderCombo()
        ComboBox1.Items.Clear()
        EnsureOpen()
        cmd = New MySqlCommand(
            "SELECT OrderID FROM LaundryOrder " &
            "WHERE CustomerID=" & Form7.UserID & " AND Status<>'Paid'",
            conn)
        da = New MySqlDataAdapter(cmd)
        Dim dt As New DataTable
        da.Fill(dt)
        For Each r As DataRow In dt.Rows
            ComboBox1.Items.Add(r("OrderID").ToString())
        Next
        conn.Close()
    End Sub

    ' Set up the DataGridView1 columns
    Private Sub InitializeGrid()
        DataGridView1.Columns.Clear()
        DataGridView1.Columns.Add("Service", "Service")
        DataGridView1.Columns.Add("Qty", "Qty")
        DataGridView1.Columns.Add("Price", "Price")
        DataGridView1.Columns.Add("Subtotal", "Subtotal")
    End Sub

    ' Clear the grid and reset inputs
    Private Sub ResetPaymentInputs()
        DataGridView1.Rows.Clear()
        Label1.Text = "Amount Due: ₱0.00"
        TextBox1.Clear()
        ComboBox2.Items.Clear()
        ComboBox2.Items.AddRange(New String() {"Cash", "Card", "GCash", "Other"})
        ComboBox2.SelectedIndex = 0
        ComboBox1.Text = ""
    End Sub

    ' When an OrderID is selected, load its details and compute total due
    Private Sub ComboBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox1.SelectedIndexChanged
        If ComboBox1.Text = "" Then Return
        Dim oid = Convert.ToInt32(ComboBox1.Text)

        DataGridView1.Rows.Clear()
        EnsureOpen()

        cmd = New MySqlCommand(
        "SELECT s.ServiceName, od.Quantity, s.UnitPrice, " &
        "(od.Quantity * s.UnitPrice) AS Subtotal " &
        "FROM OrderDetail od " &
        "JOIN Service s ON od.ServiceID = s.ServiceID " &
        "WHERE od.OrderID = " & oid, conn)

        Dim totalDue As Decimal = 0D
        Using reader As MySqlDataReader = cmd.ExecuteReader()
            While reader.Read()
                Dim svcName As String = reader("ServiceName").ToString()
                Dim qtyVal As Decimal = Convert.ToDecimal(reader("Quantity"))
                Dim priceVal As Decimal = Convert.ToDecimal(reader("UnitPrice"))
                Dim subtotalValue As Decimal = Convert.ToDecimal(reader("Subtotal"))

                DataGridView1.Rows.Add(
                svcName,
                qtyVal,
                priceVal.ToString("F2"),
                subtotalValue.ToString("F2")
            )

                totalDue += subtotalValue
            End While
        End Using

        conn.Close()

        Label1.Text = $"Amount Due: ₱{totalDue:F2}"
        TextBox1.Text = totalDue.ToString("F2")
    End Sub



    ' Button1: Pay — insert Payment row with NOW(), update order status
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If ComboBox1.Text = "" Then
            MsgBox("Please select an order.")
            Return
        End If

        Dim paidAmt As Decimal
        If Not Decimal.TryParse(TextBox1.Text, paidAmt) OrElse paidAmt <= 0 Then
            MsgBox("Enter a valid payment amount.")
            Return
        End If

        Try
            EnsureOpen()
            ' 1) Insert payment with current timestamp
            cmd = New MySqlCommand(
                "INSERT INTO Payment (OrderID, AmountPaid, PaymentDate, Method) " &
                "VALUES (" & ComboBox1.Text & ", " & paidAmt & ", NOW(), '" & ComboBox2.Text & "')",
                conn)
            cmd.ExecuteNonQuery()

            ' 2) Update LaundryOrder.Status based on cumulative payments
            cmd = New MySqlCommand(
                "UPDATE LaundryOrder SET Status = " &
                  "CASE WHEN (" &
                    "SELECT IFNULL(SUM(AmountPaid),0) FROM Payment WHERE OrderID=" & ComboBox1.Text &
                  ") >= (" &
                    "SELECT IFNULL(SUM(od.Quantity*s.UnitPrice),0) FROM OrderDetail od " &
                    "JOIN Service s ON od.ServiceID=s.ServiceID " &
                    "WHERE od.OrderID=" & ComboBox1.Text &
                  ") THEN 'Paid' ELSE 'In Progress' END " &
                "WHERE OrderID=" & ComboBox1.Text,
                conn)
            cmd.ExecuteNonQuery()

            MsgBox("Payment recorded successfully.")
        Catch ex As Exception
            MsgBox("Error recording payment: " & ex.Message)
        Finally
            conn.Close()
            PopulateOrderCombo()
            ResetPaymentInputs()
        End Try
    End Sub

    ' Button2: Back to the customer portal
    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Me.Hide()
        Form7.Show()
    End Sub
End Class





