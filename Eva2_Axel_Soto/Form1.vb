Imports MySql.Data.MySqlClient

Public Class Form1

    Dim connectionString As String = "Server=localhost;Database=registropersonas;User ID='root';Password='';"
    Private connection As MySqlConnection

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Try
            'instancia de conexion
            connection = New MySqlConnection(connectionString)
            connection.Open()

            'funcion para cargar comunas
            CargarComunas()

        Catch ex As MySqlException
            'en caso de falla de conexion muestra mensaje de error
            MessageBox.Show("Error al conectar a la base de datos: " & ex.Message)
        Finally
            If connection IsNot Nothing Then connection.Close()
        End Try
    End Sub

    Private Sub CargarComunas()
        Try
            'consulta para obtener las comunas de la base de datos
            Dim query As String = "SELECT NombreComuna FROM comuna"
            Dim command As New MySqlCommand(query, connection)

            'ejecuta y lee la consulta
            Dim reader As MySqlDataReader = command.ExecuteReader()

            'limpia el combobox
            cboComuna.Items.Clear()

            'agregar "seleccione una comuna" como opcion
            cboComuna.Items.Add("Seleccione una comuna")

            'agrega las comunas al combobox
            While reader.Read()
                cboComuna.Items.Add(reader("NombreComuna").ToString())
            End While

            reader.Close()

            'para dejar como primer campo "seleccione comuna"
            cboComuna.SelectedIndex = 0

        Catch ex As MySqlException
            'mensaje para mostrar error en caso de consulta fallida
            MessageBox.Show("Error en la consulta: " & ex.Message)
        End Try
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles btnBuscar.Click
        Try
            'instancia de conexion
            connection = New MySqlConnection(connectionString)
            connection.Open()

            'consulta para buscar los datos de la tabla personas
            Dim query As String = "SELECT Nombre, Apellido, Sexo, Comuna, Ciudad, Observacion FROM personas WHERE RUT = @RUT"
            Dim command As New MySqlCommand(query, connection)
            command.Parameters.AddWithValue("@RUT", txtRut.Text)

            'ejecuta la consulta
            Dim reader As MySqlDataReader = command.ExecuteReader()

            If reader.HasRows Then
                'si el rut existe, se cargan los datos en los campos
                While reader.Read()
                    txtNombres.Text = reader("Nombre").ToString()
                    txtApellidos.Text = reader("Apellido").ToString()
                    cboComuna.Text = reader("Comuna").ToString()
                    txtCiudad.Text = reader("Ciudad").ToString()
                    txtObservacion.Text = reader("Observacion").ToString()

                    'metodo para asignar el dato al radioButton
                    Select Case reader("Sexo").ToString()
                        Case "Masculino"
                            rbtnMasculino.Checked = True
                        Case "Femenino"
                            rbtnFemenino.Checked = True
                        Case Else
                            rbtnNoEspecifica.Checked = True
                    End Select
                End While

                'desbloquea los botones de eliminar y actualizar si el rut existe
                btnEliminar.Enabled = True
                btnActualizar.Enabled = True

                'mantiene los campos bloqueados si el rut existe
                DesbloquearCampos(False)
            Else
                'si el rut no existe muestra un msgBox de opciones
                Dim resultado As DialogResult
                resultado = MessageBox.Show("RUT no encontrado. ¿Desea ingresar un nuevo registro?", "Confirmar Nuevo Registro", MessageBoxButtons.YesNo, MessageBoxIcon.Question)

                If resultado = DialogResult.Yes Then
                    'si se da opcion a si, se limpia y desbloquean los campos para nuevo ingreso
                    LimpiarCampos()
                    DesbloquearCampos(True)
                End If
            End If

            reader.Close()
        Catch ex As MySqlException
            MessageBox.Show("Error al conectar a la base de datos: " & ex.Message)
        Finally
            'cierre de conexion
            If connection IsNot Nothing Then connection.Close()
        End Try
    End Sub

    'funcion para desbloquear campos si el rut no existe
    Private Sub DesbloquearCampos(estado As Boolean)
        txtNombres.Enabled = estado
        txtApellidos.Enabled = estado
        rbtnMasculino.Enabled = estado
        rbtnFemenino.Enabled = estado
        rbtnNoEspecifica.Enabled = estado
        cboComuna.Enabled = estado
        txtCiudad.Enabled = estado
        txtObservacion.Enabled = estado
        btnGuardar.Enabled = estado
    End Sub

    'funcion para limpiar campos si el rut no existe
    Private Sub LimpiarCampos()
        txtNombres.Text = ""
        txtApellidos.Text = ""
        rbtnMasculino.Checked = False
        rbtnFemenino.Checked = False
        rbtnNoEspecifica.Checked = False
        cboComuna.Text = ""
        txtCiudad.Text = ""
        txtObservacion.Text = ""
    End Sub

    'funcion de limpiar campos incluido rut
    Private Sub LimpiarCampos2()
        txtNombres.Text = ""
        txtApellidos.Text = ""
        rbtnMasculino.Checked = False
        rbtnFemenino.Checked = False
        rbtnNoEspecifica.Checked = False
        cboComuna.Text = ""
        txtCiudad.Text = ""
        txtObservacion.Text = ""
        txtRut.Text = ""
    End Sub

    Private Sub btnGuardar_Click(sender As Object, e As EventArgs) Handles btnGuardar.Click
        Try
            connection = New MySqlConnection(connectionString)
            connection.Open()

            ' Consulta para verificar si el RUT ya existe
            Dim checkQuery As String = "SELECT COUNT(*) FROM personas WHERE RUT = @RUT"
            Dim checkCommand As New MySqlCommand(checkQuery, connection)
            checkCommand.Parameters.AddWithValue("@RUT", txtRut.Text)

            Dim count As Integer = Convert.ToInt32(checkCommand.ExecuteScalar())

            If count > 0 Then
                ' Si el RUT existe, se actualiza
                Dim updateQuery As String = "UPDATE personas SET Nombre = @Nombre, Apellido = @Apellido, Sexo = @Sexo, Comuna = @Comuna, Ciudad = @Ciudad, Observacion = @Observacion WHERE RUT = @RUT"
                Dim command As New MySqlCommand(updateQuery, connection)

                ' Asigna los datos a cada campo
                command.Parameters.AddWithValue("@RUT", txtRut.Text)
                command.Parameters.AddWithValue("@Nombre", txtNombres.Text)
                command.Parameters.AddWithValue("@Apellido", txtApellidos.Text)
                command.Parameters.AddWithValue("@Sexo", If(rbtnMasculino.Checked, "Masculino", If(rbtnFemenino.Checked, "Femenino", "Nulo")))
                command.Parameters.AddWithValue("@Comuna", cboComuna.Text)
                command.Parameters.AddWithValue("@Ciudad", txtCiudad.Text)
                command.Parameters.AddWithValue("@Observacion", txtObservacion.Text)

                ' Ejecuta la consulta
                command.ExecuteNonQuery()

                MessageBox.Show("Registro actualizado exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information)

            Else
                ' Si el RUT no existe, se inserta un nuevo registro
                Dim insertQuery As String = "INSERT INTO personas (RUT, Nombre, Apellido, Sexo, Comuna, Ciudad, Observacion) VALUES (@RUT, @Nombre, @Apellido, @Sexo, @Comuna, @Ciudad, @Observacion)"
                Dim command As New MySqlCommand(insertQuery, connection)

                ' Asigna los datos a cada campo
                command.Parameters.AddWithValue("@RUT", txtRut.Text)
                command.Parameters.AddWithValue("@Nombre", txtNombres.Text)
                command.Parameters.AddWithValue("@Apellido", txtApellidos.Text)
                command.Parameters.AddWithValue("@Sexo", If(rbtnMasculino.Checked, "Masculino", If(rbtnFemenino.Checked, "Femenino", "Nulo")))
                command.Parameters.AddWithValue("@Comuna", cboComuna.Text)
                command.Parameters.AddWithValue("@Ciudad", txtCiudad.Text)
                command.Parameters.AddWithValue("@Observacion", txtObservacion.Text)

                ' Ejecuta la consulta
                command.ExecuteNonQuery()

                MessageBox.Show("Registro guardado exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If

            LimpiarCampos2() ' Limpia los campos después de un ingreso exitoso
            DesbloquearCampos(False) ' Vuelve a bloquear los campos después de ingreso exitoso

        Catch ex As MySqlException
            ' Manejo de error en caso de falla de ingreso
            MessageBox.Show("Error al guardar el registro: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            If connection IsNot Nothing Then connection.Close()
        End Try
    End Sub

    'variable global para controlar el estado del botón actualizar
    Private modoEdicion As Boolean = False

    Private Sub btnActualizar_Click(sender As Object, e As EventArgs) Handles btnActualizar.Click
        'si está en modo de edición, se guarda o actualiza el registro
        If modoEdicion Then
            btnGuardar.PerformClick() ' Llama al método de guardar
            modoEdicion = False
            btnActualizar.Text = "Actualizar"
        Else
            DesbloquearCampos(True)
            modoEdicion = True
        End If
    End Sub

    Private Sub btnEliminar_Click(sender As Object, e As EventArgs) Handles btnEliminar.Click
        'elimina un registro si se confirma
        Dim resultado As DialogResult
        resultado = MessageBox.Show("¿Está seguro que desea eliminar el registro?", "Confirmar Eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)

        If resultado = DialogResult.Yes Then
            Try
                connection = New MySqlConnection(connectionString)
                connection.Open()

                'consulta para eliminar el registro
                Dim deleteQuery As String = "DELETE FROM personas WHERE RUT = @RUT"
                Dim command As New MySqlCommand(deleteQuery, connection)
                command.Parameters.AddWithValue("@RUT", txtRut.Text)

                command.ExecuteNonQuery()
                MessageBox.Show("Registro eliminado exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information)

                LimpiarCampos2() ' Limpia los campos después de eliminar
                DesbloquearCampos(False) ' Vuelve a bloquear los campos

            Catch ex As MySqlException
                MessageBox.Show("Error al eliminar el registro: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Finally
                If connection IsNot Nothing Then connection.Close()
            End Try
        End If
    End Sub

    'evento para mostrar la base de datos en un MessageBox
    Private Sub btnBD_Click(sender As Object, e As EventArgs) Handles btnBD.Click
        Try
            connection = New MySqlConnection(connectionString)
            connection.Open()

            ' Consulta para obtener todos los registros de la tabla personas
            Dim query As String = "SELECT RUT, Nombre, Apellido, Sexo, Comuna, Ciudad, Observacion FROM personas"
            Dim command As New MySqlCommand(query, connection)

            Dim reader As MySqlDataReader = command.ExecuteReader()

            Dim resultado As String = "Registros de la base de datos:" & Environment.NewLine & Environment.NewLine

            ' Verifica si hay filas y agrega cada registro a la cadena resultado
            If reader.HasRows Then
                While reader.Read()
                    resultado &= String.Format("RUT: {0}, Nombre: {1}, Apellido: {2}, Sexo: {3}, Comuna: {4}, Ciudad: {5}, Observación: {6}{7}",
                                               reader("RUT"), reader("Nombre"), reader("Apellido"), reader("Sexo"),
                                               reader("Comuna"), reader("Ciudad"), reader("Observacion"), Environment.NewLine)
                End While
            Else
                resultado = "No hay registros en la base de datos."
            End If

            reader.Close()

            ' Muestra el resultado en un MessageBox
            MessageBox.Show(resultado, "Contenido de la Base de Datos", MessageBoxButtons.OK, MessageBoxIcon.Information)

        Catch ex As MySqlException
            MessageBox.Show("Error al conectar a la base de datos: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            If connection IsNot Nothing Then connection.Close()
        End Try
    End Sub
End Class
