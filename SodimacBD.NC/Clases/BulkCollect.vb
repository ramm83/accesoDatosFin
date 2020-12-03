Imports System.Data
Imports Oracle.ManagedDataAccess.Client
Friend Class BulkCollect

    Private CadenaConexion As String = String.Empty
    Private _Connection As OracleConnection
    Public CodigoExcepcion As Integer = -1
    Public MsjExcepcion As String = String.Empty

    Public Sub New(ByVal CadenaConexion As String)
        Me.CadenaConexion = CadenaConexion
        _Connection = New OracleConnection(Me.CadenaConexion)
    End Sub
    Private Sub AsignarParametrosBulkCollect(ByRef Cmd As OracleCommand, ByVal LstParametros As List(Of ParametroBulkCollect))

        Dim AsignarCantidad As Boolean = True

        For Each Parametro In LstParametros

            If AsignarCantidad Then
                Cmd.ArrayBindCount = Parametro.ListaValores.Count
                AsignarCantidad = False
            End If

            If Parametro.Tipo = OracleDbType.Int16 Or Parametro.Tipo = OracleDbType.Int32 Or Parametro.Tipo = OracleDbType.Int64 Then
                Dim lst As List(Of Integer) = New List(Of Integer)
                lst = Parametro.ListaValores.Cast(Of Integer).ToList
                Cmd.Parameters.Add(Parametro.Nombre, Parametro.Tipo, lst.ToArray(), ParameterDirection.Input)
            ElseIf Parametro.Tipo = OracleDbType.Varchar2 Or Parametro.Tipo = OracleDbType.Clob Then
                Dim lst As List(Of String) = New List(Of String)
                lst = Parametro.ListaValores.Cast(Of String).ToList
                Cmd.Parameters.Add(Parametro.Nombre, Parametro.Tipo, lst.ToArray(), ParameterDirection.Input)
            End If


        Next

    End Sub

    Protected Function ValidarEstadoConexion() As Boolean

        Try
            If _Connection.State = ConnectionState.Open Then
                Return True
            Else
                _Connection.Open()
                Return True
            End If
        Catch ex As Exception
            Return False
        End Try

    End Function


    Public Function EjecutarSentencia(ByVal Sentencia As String, ByVal ListaParametros As List(Of ParametroBulkCollect)) As Integer
        CodigoExcepcion = -1
        MsjExcepcion = String.Empty
        Dim Resultado As Integer = 0
        Try

            If ValidarEstadoConexion() Then
                Using cmd As OracleCommand = _Connection.CreateCommand
                    AsignarParametrosBulkCollect(cmd, ListaParametros)
                    cmd.CommandText = Sentencia
                    Resultado = cmd.ExecuteNonQuery()
                    Return Resultado
                End Using
            End If

        Catch ex As OracleException
            CodigoExcepcion = ex.ErrorCode
            MsjExcepcion = ex.Message
        Finally
            _Connection.Close()
        End Try

        Return -1

    End Function


End Class
