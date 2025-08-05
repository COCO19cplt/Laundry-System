Imports MySql.Data.MySqlClient

Public Class PaymentForm
    Dim conn As New MySqlConnection(
        "server=localhost;userid=root;password=;database=laundrydb;")
    Dim cmd As MySqlCommand
    Dim da As MySqlDataAdapter

    Private Sub PaymentForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Load pending orders for this customer
        ComboBox1.Items.Clear()
        conn.Open()
        cmd = New MySqlCommand(
            "SELECT OrderID FROM LaundryOrder " &
            "WHERE CustomerID=" & Form7.UserID & " AND Status<>'Paid'", conn)
        da = New MySqlDataAdapter(cmd)
        Dim dt As New DataTable
        da.Fill(dt)
        For Each r As DataRow In dt.Rows
            ComboBox1.Items.Add(r("OrderID").ToString())
        Next
        conn.Close()

        ' Prepare grid
        DataGridView1.Columns.Clear()
        DataGridView1.Columns.Add("Service", "Service")
        DataGridView1.Columns.Add("Qty", "Qty")
        DataGridView1.Columns.Add("UnitPrice", "Price")
        DataGridView1.Columns.Add("Subtotal", "Subtotal")

        ' Reset UI
        Label1.Text = "Amount Due: ₱0.00"
        TextBox1.Text = ""
        ComboBox2.Items.Clear()
        ComboBox2.Items.AddRange(New String() {"Cash", "Card", "GCash", "Other"})
        ComboBox2.SelectedIndex = 0
    End Sub

    Private Sub ComboBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox1.SelectedIndexChanged
        If ComboBox1.Text = "" Then Return
        Dim oid = Convert.ToInt32(ComboBox1.Text)

        DataGridView1.Rows.Clear()
        conn.Open()
        cmd = New MySqlCommand(
            "SELECT s.ServiceName, od.Quantity, s.UnitPrice, " &
            "(od.Quantity * s.UnitPrice) AS Subtotal " &
            "FROM OrderDetail od " &
            "JOIN Service s ON od.ServiceID = s.ServiceID " &
            "WHERE od.OrderID=" & oid, conn)
        Dim reader = cmd.ExecuteReader()
        Dim totalDue As Decimal = 0
        While reader.Read()
            Dim svcName = reader("ServiceName").ToString()
            Dim qty = Convert.ToDecimal(reader("Quantity"))
            Dim unitPrice = Convert.ToDecimal(reader("UnitPrice"))
            Dim subtotalValue = Convert.ToDecimal(reader("Subtotal"))

            ' Add to grid
            DataGridView1.Rows.Add(
                svcName,
                qty,
                unitPrice.ToString("F2"),
                subtotalValue.ToString("F2")
            )

            totalDue += subtotalValue
        End While
        reader.Close()
        conn.Close()

        Label1.Text = "Amount Due: ₱" & totalDue.ToString("F2")
        TextBox1.Text = totalDue.ToString("F2")
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If ComboBox1.Text = "" Then
            MsgBox("Please select your order.")
            Return
        End If

        Dim paidAmt As Decimal
        If Not Decimal.TryParse(TextBox1.Text, paidAmt) OrElse paidAmt <= 0 Then
            MsgBox("Enter a valid payment amount.")
            Return
        End If
        Dim method = ComboBox2.Text
        Dim oid = Convert.ToInt32(ComboBox1.Text)

        Try
            conn.Open()

            ' Insert this payment
            cmd = New MySqlCommand(
                "INSERT INTO Payment (OrderID, AmountPaid, Method) " &
                "VALUES (" & oid & ", " & paidAmt & ", '" & method & "')",
                conn)
            cmd.ExecuteNonQuery()

            ' Compute total due
            cmd = New MySqlCommand(
                "SELECT SUM(od.Quantity * s.UnitPrice) " &
                "FROM OrderDetail od JOIN Service s ON od.ServiceID=s.ServiceID " &
                "WHERE od.OrderID=" & oid,
                conn)
            Dim totalDue = Convert.ToDecimal(cmd.ExecuteScalar())

            ' Compute total paid so far
            cmd = New MySqlCommand(
                "SELECT SUM(AmountPaid) FROM Payment WHERE OrderID=" & oid,
                conn)
            Dim totalPaid = Convert.ToDecimal(cmd.ExecuteScalar())

            ' Update status
            Dim newStatus As String = If(totalPaid >= totalDue, "Paid", "In Progress")
            cmd = New MySqlCommand(
                "UPDATE LaundryOrder SET Status='" & newStatus & "' WHERE OrderID=" & oid,
                conn)
            cmd.ExecuteNonQuery()

            MsgBox($"Payment of ₱{paidAmt:F2} recorded. Total paid: ₱{totalPaid:F2} / ₱{totalDue:F2}. Status: {newStatus}.")

            ' Refresh
            PaymentForm_Load(Nothing, Nothing)
        Catch ex As Exception
            MsgBox("Error recording payment: " & ex.Message)
        Finally
            conn.Close()
        End Try
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Me.Hide()
        Form7.Show()
    End Sub
End Class


