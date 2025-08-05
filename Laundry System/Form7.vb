Imports MySql.Data.MySqlClient

Public Class Form7
    ' Connection & current CustomerID
    Dim connection As New MySqlConnection("server=localhost;userid=root;password=;database=laundrydb;")
    Public Shared UserID As Integer

    ' ───────────────────────────────────────────
    ' On form load: populate services & init grid
    ' ───────────────────────────────────────────
    Private Sub Form7_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        LoadServices()
        InitializeGrid()
        UpdateTotal()
    End Sub

    ' ───────────────────────────────────────────
    ' Fill ComboBox1 with "Name|Price"
    ' ───────────────────────────────────────────
    Private Sub LoadServices()
        ComboBox1.Items.Clear()
        Dim cmd As New MySqlCommand("SELECT ServiceName, UnitPrice FROM Service", connection)
        connection.Open()
        Dim reader = cmd.ExecuteReader()
        While reader.Read()
            ComboBox1.Items.Add(reader("ServiceName") & "|" & reader("UnitPrice"))
        End While
        reader.Close()
        connection.Close()
    End Sub

    ' ───────────────────────────────────────────
    ' Set up DataGridView1: Service, Qty, Subtotal
    ' ───────────────────────────────────────────
    Private Sub InitializeGrid()
        DataGridView1.Columns.Clear()
        DataGridView1.Columns.Add("Service", "Service")
        DataGridView1.Columns.Add("Quantity", "Qty")
        DataGridView1.Columns.Add("Subtotal", "Subtotal")
    End Sub

    ' ───────────────────────────────────────────
    ' Recompute Total & show in Label1
    ' ───────────────────────────────────────────
    Private Sub UpdateTotal()
        Dim total As Decimal = 0
        For Each row As DataGridViewRow In DataGridView1.Rows
            If Not row.IsNewRow Then
                total += Convert.ToDecimal(row.Cells("Subtotal").Value)
            End If
        Next
        Label1.Text = "Total: ₱" & total.ToString("F2")
    End Sub

    ' ───────────────────────────────────────────
    ' Button1: ADD TO CART
    ' ───────────────────────────────────────────
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If ComboBox1.Text = "" Then
            MsgBox("Select a service.")
            Return
        End If
        If NumericUpDown1.Value <= 0 Then
            MsgBox("Quantity must be greater than zero.")
            Return
        End If

        ' Parse service name and unit price
        Dim parts = ComboBox1.Text.Split("|"c)
        Dim svcName As String = parts(0)
        Dim price As Decimal = Convert.ToDecimal(parts(1))
        Dim qty As Decimal = NumericUpDown1.Value
        Dim subtotalValue As Decimal = price * qty

        ' Add to cart grid
        DataGridView1.Rows.Add(svcName, qty, subtotalValue.ToString("F2"))
        UpdateTotal()
    End Sub


    ' ───────────────────────────────────────────
    ' Button2: PLACE ORDER
    ' ───────────────────────────────────────────
    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        ' Ensure cart not empty
        If DataGridView1.Rows.Count = 0 Then
            MsgBox("Cart is empty.")
            Return
        End If

        Dim tx As MySqlTransaction = Nothing
        Try
            connection.Open()
            tx = connection.BeginTransaction()

            ' 1) Insert order header
            Dim nowStr = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            Dim cmdOrder As New MySqlCommand(
                "INSERT INTO LaundryOrder (CustomerID, OrderDate) " &
                "VALUES (" & UserID & ", '" & nowStr & "')",
                connection, tx)
            cmdOrder.ExecuteNonQuery()

            ' 2) Get new OrderID
            cmdOrder = New MySqlCommand("SELECT LAST_INSERT_ID()", connection, tx)
            Dim orderId = Convert.ToInt32(cmdOrder.ExecuteScalar())

            ' 3) Insert one OrderDetail per cart row
            For Each gridRow As DataGridViewRow In DataGridView1.Rows
                If Not gridRow.IsNewRow Then
                    Dim svc = gridRow.Cells("Service").Value.ToString().Replace("'", "''")
                    Dim qtyVal = Convert.ToDecimal(gridRow.Cells("Quantity").Value)

                    ' Lookup ServiceID
                    Dim cmdSID As New MySqlCommand(
                        "SELECT ServiceID FROM Service WHERE ServiceName='" & svc & "'",
                        connection, tx)
                    Dim sid = Convert.ToInt32(cmdSID.ExecuteScalar())

                    ' Insert detail row
                    Dim cmdDet As New MySqlCommand(
                        "INSERT INTO OrderDetail (OrderID, ServiceID, Quantity) " &
                        "VALUES (" & orderId & ", " & sid & ", " & qtyVal & ")",
                        connection, tx)
                    cmdDet.ExecuteNonQuery()
                End If
            Next

            ' 4) Commit all or nothing
            tx.Commit()
            MsgBox("Order #" & orderId & " placed successfully.")

            ' 5) Clear UI for next order
            DataGridView1.Rows.Clear()
            UpdateTotal()

        Catch ex As Exception
            ' Roll back on any error
            If tx IsNot Nothing Then
                Try
                    tx.Rollback()
                Catch
                End Try
            End If
            MsgBox("Error placing order: " & ex.Message)
        Finally
            connection.Close()
        End Try
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        PaymentForm.Show()
        Me.Close()
    End Sub
End Class




