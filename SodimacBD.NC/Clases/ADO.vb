Imports System.Data
'Imports System.Data.OracleClient
Imports Oracle.ManagedDataAccess.Client

Public Class ADO
    Implements IDisposable

#Region "[VARIABLES GLOBALES]"
    Public Excepcion As New LogMensajesError
    Private Conexion As Conexion
    Private Seguridad As New Seguridad
    Private CadenaConexionCurrent As String = String.Empty
    Private ConexionOrigen As String

#End Region

#Region "[CONSTRUCTORES]"
    Public Sub New(ByVal ConnectionString As String)
        Me.ConexionOrigen = ConnectionString
        Excepcion = Nothing
        CadenaConexionCurrent = Seguridad.GetCadenaConexionBaseDatos(ConnectionString)
        Conexion = New Conexion(ConnectionString)
        ValidarEstadoConexion()
    End Sub
    Public Sub New(ByRef Connection As OracleConnection)
        Excepcion = Nothing
        Me.ConexionOrigen = Connection.ConnectionString
        CadenaConexionCurrent = Connection.ConnectionString
        Conexion = New Conexion(Connection)
    End Sub
#End Region

    Private Structure Mensajes
        Public Const msjErrConnection As String = "Problemas de Conectividad con la Base de Datos"
    End Structure
    Protected Function ValidarEstadoConexion() As Boolean
        If Not Conexion.ConexionOk Then
            Excepcion = New LogMensajesError With {.Codigo = 101, .Mensaje = "Conexion Incorrecta." + ConexionOrigen + " " + Conexion._MensajeError, .IdAplicacion = -1, .IdProceso = -1, .Comentario = ""}
            Return False
        ElseIf IsNothing(Conexion.Connection) Then
            Excepcion = New LogMensajesError With {.Codigo = 102, .Mensaje = "Conexion Incorrecta" + ConexionOrigen, .IdAplicacion = -1, .IdProceso = -1, .Comentario = ""}
            Return False
        ElseIf Conexion.Connection.State = ConnectionState.Open Then
            Excepcion = Nothing
            Return True
        Else
            Excepcion = Nothing
            Conexion.Connection.Open()
            Return True
        End If

    End Function
    Public Function fncEsVacio(ByVal Valor As String) As Boolean
        If IsNothing(Valor) Then
            Return True
        ElseIf String.IsNullOrEmpty(Valor.Trim) Then
            Return True
        Else
            Return False
        End If
    End Function
    Protected Sub PrcSetExcepcion(ByVal Obj As Exception, ByVal Optional Comentario As String = "")

        Excepcion = New LogMensajesError With {.Codigo = -1,
                                            .Mensaje = Obj.Message,
                                            .Comentario = "Error desde la aplicación: " _
                                                            & System.Reflection.Assembly.GetExecutingAssembly().FullName & ". Usuario: " _
                                                            & System.Environment.UserName & " - " & Obj.Source & Comentario,
                                            .Objeto = System.Reflection.Assembly.GetExecutingAssembly().FullName,
                                            .Fecha = Now().ToString("dd/MM/yyyy HH:mm:ss")
                                            }
    End Sub
    Protected Sub PrcSetExcepcion(ByVal Codigo As Integer, ByVal Descripcion As String)

        Excepcion = New LogMensajesError With {.Codigo = -1,
                                            .Mensaje = Descripcion,
                                            .Comentario = "Error desde la aplicación: " _
                                                            & System.Reflection.Assembly.GetExecutingAssembly().FullName & ". Usuario: " _
                                                            & System.Environment.UserName,
                                            .Objeto = System.Reflection.Assembly.GetExecutingAssembly().FullName,
                                            .Fecha = Now().ToString("dd/MM/yyyy HH:mm:ss")
                                            }
    End Sub



#Region "[PROCEDIMIENTOS ALMACENADOS]"
    Private Function GetNormalizacionFecha(ByVal Fecha As String) As DateTime
        Try
            Dim resultado As DateTime
            Dim Dia = Fecha.Split("/").First
            Dim Mes = Fecha.Split("/").ElementAt(1)
            Dim Anio = Fecha.Split("/").ElementAt(2).Split(" ").First
            Dim Hora = ""
            Dim Minuto = ""
            Dim Segundo = ""
            If Fecha.Contains(":") Then
                Hora = Fecha.Split(" ").ElementAt(1).Split(":").First.Trim
                Minuto = Fecha.Split(" ").ElementAt(1).Split(":").ElementAt(1).Trim
                Segundo = Fecha.Split(" ").ElementAt(1).Split(":").ElementAt(2).Trim
            End If

            If (String.IsNullOrEmpty(Hora)) Then
                resultado = New DateTime(Anio, Mes, Dia)
            Else
                resultado = New DateTime(Convert.ToInt32(Anio), Convert.ToInt32(Mes), Convert.ToInt32(Dia), Convert.ToInt32(Hora), Convert.ToInt32(Minuto), Convert.ToInt32(Segundo))
            End If
            Return resultado
        Catch ex As Exception
            Return Nothing
        End Try
    End Function
    Protected Sub PrcAsignaParametros(ByRef cmd As OracleCommand, ByVal Parametros As List(Of SQLParametro))
        Try

            For Each Parametro In Parametros

                If Parametro.Entrada Then

                    If Parametro.Tipo = OracleDbType.Varchar2 Or Parametro.Tipo = OracleDbType.Clob Or Parametro.Tipo = OracleDbType.Char Then
                        cmd.Parameters.Add(Parametro.Nombre.ToUpper(), Parametro.Tipo, Parametro.stringValor.Length).Value = Parametro.stringValor
                    ElseIf Parametro.Tipo = OracleDbType.TimeStamp Then

                        Dim Fecha = GetNormalizacionFecha(Parametro.DateValor)
                        If IsNothing(Fecha) Then
                            cmd.Parameters.Add(Parametro.Nombre.ToUpper(), Parametro.Tipo, Parametro.DateValor.Length).Value = Parametro.DateValor
                        Else
                            cmd.Parameters.Add(Parametro.Nombre.ToUpper(), Parametro.Tipo, Parametro.DateValor.Length).Value = Fecha
                        End If

                    ElseIf Parametro.Tipo = OracleDbType.Int16 Or
                               Parametro.Tipo = OracleDbType.Int32 Or
                               Parametro.Tipo = OracleDbType.Decimal Or
                               Parametro.Tipo = OracleDbType.Double Then
                        Dim valor
                        If Parametro.intValor = 0 And Parametro.douValor = 0 Then
                            valor = 0
                        ElseIf Parametro.intValor <> 0 And Parametro.douValor = 0 Then
                            valor = Parametro.intValor
                        ElseIf Parametro.intValor = 0 And Parametro.douValor <> 0 Then
                            valor = Parametro.douValor
                        End If

                        cmd.Parameters.Add(Parametro.Nombre.ToUpper(), Parametro.Tipo).Value = valor
                    End If

                    cmd.Parameters(Parametro.Nombre.ToUpper()).Direction = ParameterDirection.Input
                Else

                    If Parametro.Tipo = OracleDbType.Varchar2 Or Parametro.Tipo = OracleDbType.Clob Then
                        cmd.Parameters.Add(Parametro.Nombre.ToUpper(), Parametro.Tipo, 20000).Direction = ParameterDirection.Output
                    Else
                        cmd.Parameters.Add(Parametro.Nombre.ToUpper(), Parametro.Tipo).Direction = ParameterDirection.Output
                    End If

                End If

            Next


        Catch ex As Exception

            cmd = Nothing
            PrcSetExcepcion(ex, "Falla Asignación de Parámetros")

        End Try

    End Sub
    Private Function fncRecuperaResultado(ByRef cmd As OracleCommand, ByVal Parametros As List(Of SQLParametro))

        Dim Parametro As SQLParametro = Parametros.Find(Function(obj As SQLParametro) obj.Entrada = False)

        If IsNothing(Parametro) Then
            Return cmd.ExecuteNonQuery()
        Else
            If Parametro.Tipo = OracleDbType.RefCursor Then
                Dim tbl As New DataTable
                tbl.Load(cmd.ExecuteReader)
                Return tbl
            ElseIf Parametro.Tipo = OracleDbType.Clob Then
                cmd.ExecuteScalar()

                If IsDBNull(cmd.Parameters(Parametro.Nombre).Value) Then
                    Return Nothing
                Else
                    'Return DirectCast(cmd.Parameters(Parametro.Nombre).Value(), System.Data.OracleClient.OracleLob).Value
                    Return cmd.Parameters(Parametro.Nombre).Value().Value
                End If
            Else
                cmd.ExecuteScalar()
                If IsDBNull(cmd.Parameters(Parametro.Nombre).Value) Then
                    Return Nothing
                Else
                    Return cmd.Parameters(Parametro.Nombre).Value()
                End If
            End If
        End If

        Return Nothing

    End Function
    Public Function StoreProcedure(ByVal Procedimiento As String, ByVal Parametros As List(Of SQLParametro), Optional ByVal DisposeConexion As Boolean = False)
        Excepcion = Nothing
        Dim Resultado
        If fncEsVacio(Procedimiento) Then
            Return Nothing
        ElseIf IsNothing(Parametros) Then
            Return Nothing
        ElseIf Parametros.Count = 0 Then
            Return Nothing
        End If

        Try
            Using cmd As New OracleCommand(Procedimiento, Conexion.Connection)
                cmd.CommandType = CommandType.StoredProcedure
                PrcAsignaParametros(cmd, Parametros)
                If IsNothing(cmd) Then
                    Resultado = -1
                ElseIf ValidarEstadoConexion() Then
                    Resultado = fncRecuperaResultado(cmd, Parametros)
                End If
                Return Resultado
            End Using
        Catch ex As OracleException
            PrcSetExcepcion(ex, Procedimiento)
            Return Nothing
        Finally
            Conexion.Connection.Close()

            If DisposeConexion Then
                Conexion.Dispose()
            End If

        End Try

    End Function
    Public Function StoreProcedureDataSet(ByVal Procedimiento As String, ByVal Parametros As List(Of SQLParametro), Optional ByVal DisposeConexion As Boolean = False) As DataSet
        Excepcion = Nothing
        Dim dsResultado As New DataSet

        If fncEsVacio(Procedimiento) Then
            Return Nothing
        ElseIf IsNothing(Parametros) Then
            Return Nothing
        ElseIf Parametros.Count = 0 Then
            Return Nothing
        End If

        Try
            Using cmd As New OracleCommand(Procedimiento, Conexion.Connection)
                cmd.CommandType = CommandType.StoredProcedure
                PrcAsignaParametros(cmd, Parametros)
                If IsNothing(cmd) Then
                    Return Nothing
                ElseIf ValidarEstadoConexion() Then
                    Dim da As New OracleDataAdapter(cmd)
                    da.Fill(dsResultado)
                End If

                Return dsResultado

            End Using
        Catch ex As OracleException
            PrcSetExcepcion(ex, Procedimiento)
            Return Nothing
        Finally
            Conexion.Connection.Close()

            If DisposeConexion Then
                Conexion.Dispose()
            End If

        End Try
    End Function
#End Region


#Region "[ESCALARES]"

    Private Function GetTblError(ByVal Obj As OracleException) As DataTable
        Dim TblResultado As New DataTable
        If IsNothing(Obj) Then
            Return TblResultado
        Else
            TblResultado.Columns.AddRange({New DataColumn("ID_ERROR"), New DataColumn("DESCRIPCION")})
            TblResultado.Rows.Add({Obj.ErrorCode, Obj.Message})
        End If

        Return TblResultado

    End Function
    Public Function GetEscalar(ByVal Sentencia As String, Optional ByVal DisposeConexion As Boolean = False) As String
        Excepcion = Nothing
        Dim Resultado As String
        Dim Mensaje As String = String.Empty
        Try

            If ValidarEstadoConexion() Then
                Using cmd As New OracleCommand(Sentencia, Conexion.Connection)
                    Resultado = cmd.ExecuteScalar().ToString
                End Using
            End If

            Return Resultado

        Catch ex As OracleException
            PrcSetExcepcion(ex, Sentencia)
            Return Nothing
        Finally
            Conexion.Connection.Close()
            If DisposeConexion Then
                Conexion.Dispose()
            End If
        End Try
    End Function
    Public Function GetDataTable(ByVal Sentencia As String, Optional ByVal DisposeConexion As Boolean = False) As DataTable
        Excepcion = Nothing
        Dim TblResultado As DataTable
        Dim Mensaje As String = String.Empty
        Try

            If ValidarEstadoConexion() Then
                Using cmd As New OracleCommand(Sentencia, Conexion.Connection)
                    TblResultado = New DataTable
                    TblResultado.Load(cmd.ExecuteReader)
                End Using
            End If

            Return TblResultado

        Catch ex As OracleException
            PrcSetExcepcion(ex, Sentencia)
            Return GetTblError(ex)
        Finally
            Conexion.Connection.Close()

            If DisposeConexion Then
                Conexion.Dispose()
            End If
        End Try
    End Function

#End Region

#Region "[SENTENCIAS]"
    Public Function EjecutarSentencia(ByVal Sentencia As String, Optional ByVal DisposeConexion As Boolean = False) As Integer
        Excepcion = Nothing
        Dim Resultado As Integer
        Dim Mensaje As String = String.Empty
        Try

            If ValidarEstadoConexion() Then
                Using cmd As New OracleCommand(Sentencia, Conexion.Connection)
                    Resultado = cmd.ExecuteNonQuery()
                End Using
            End If

            Return Resultado

        Catch ex As OracleException
            PrcSetExcepcion(ex, Sentencia)
            Return -1
        Finally
            Conexion.Connection.Close()

            If DisposeConexion Then
                Conexion.Connection.Dispose()
                Conexion.Dispose()
                OracleConnection.ClearPool(Conexion.Connection)
            End If

        End Try
    End Function
#End Region

#Region "[BULKCOLLECT]"

    Public Function SetDatosMasivos(ByVal Sentencia As String, ByVal ListaParametros As List(Of ParametroBulkCollect)) As Integer

        If IsNothing(Sentencia) Then
            Return -1
        ElseIf String.IsNullOrEmpty(Sentencia.Trim) Then
            Return -1
        ElseIf Not Sentencia.ToUpper.Trim.Contains("INSERT") Then
            Return -2
        Else

            Dim Bulk As BulkCollect = New BulkCollect(CadenaConexionCurrent)
            Dim Resultado As Integer = Bulk.EjecutarSentencia(Sentencia, ListaParametros)

            If String.IsNullOrEmpty(Bulk.MsjExcepcion) Then
                Return Resultado
            Else
                Dim ex As New Exception(Bulk.MsjExcepcion)
                PrcSetExcepcion(ex, Sentencia)
                Return -3
            End If

        End If


    End Function

#Region "IDisposable Support"
    'Private disposedValue As Boolean = False

    Protected Overridable Sub Dispose(disposing As Boolean)
        'If Not disposedValue Then
        '    If disposing Then
        '        Conexion.Dispose()
        '    End If
        'End If
        'disposedValue = True
        If disposing Then
            Conexion.Dispose()
        End If
    End Sub
    Protected Overrides Sub Finalize()
        Dispose(False)
        MyBase.Finalize()
    End Sub
    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
    End Sub
#End Region

#End Region


End Class
