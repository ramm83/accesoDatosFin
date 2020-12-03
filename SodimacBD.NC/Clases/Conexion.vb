Imports System.Data
Imports System.Data.OracleClient
Imports Oracle.ManagedDataAccess.Client

Friend Class Conexion : Inherits Utilidades
    Implements IDisposable

    Private _Connection As OracleConnection
    Private _FechaConexion As String
    Private _EnableConnection As Boolean
    Public _MensajeError As String = String.Empty
    Public ConnectionString As String

    Private Sub CrearConexion(ByVal ConnectionString As String)
        Me.ConnectionString = ConnectionString
        If IsNothing(ConnectionString) Then
            _Connection = Nothing
        ElseIf String.IsNullOrEmpty(ConnectionString) Then
            _Connection = Nothing
        Else
            Try
                _Connection = New OracleConnection(ConnectionString)
            Catch ex As Exception
                _Connection = Nothing
                _EnableConnection = False
                _MensajeError = ex.Message
            End Try
        End If

    End Sub
    Private Sub ValidarConexion()
        If Not IsNothing(_Connection) Then
            Try
                _Connection.Open()
                If _Connection.State = ConnectionState.Open Then
                    _EnableConnection = True
                    _MensajeError = String.Empty
                Else
                    _EnableConnection = False
                    _MensajeError = "Error. No se Pudo Establecer Conexión con la Base de Datos"
                End If
            Catch ex As Exception
                _EnableConnection = False
                _MensajeError = ex.Message
            Finally
                _Connection.Close()
            End Try
        End If
    End Sub
    Public Sub New(ByRef Connection As OracleConnection)
        _Connection = Connection
        Me.ConnectionString = Connection.ConnectionString
        _FechaConexion = Date.Now().ToString("dd/MM/yyyy HH:MM:ss")
        ValidarConexion()
        _Connection.Close()
    End Sub
    Public Sub New(ByVal ConnectionString As String)
        Me.ConnectionString = ConnectionString
        Dim CadenaConexion = GetCadenaConexionBaseDatos(ConnectionString)
        If IsNothing(CadenaConexion) Then
            CadenaConexion = ConnectionString
        ElseIf String.IsNullOrEmpty(CadenaConexion) Then
            CadenaConexion = ConnectionString
        End If

        CrearConexion(CadenaConexion)
        _FechaConexion = Date.Now().ToString("dd/MM/yyyy HH:MM:ss")
        ValidarConexion()
        _Connection.Close()
    End Sub
    Public ReadOnly Property Connection As OracleConnection
        Get
            Return _Connection
        End Get
    End Property
    Public ReadOnly Property Esquema As String
        Get
            Return GetEsquema(_Connection)
        End Get
    End Property
    Public ReadOnly Property FechaConexion As String
        Get
            Return _FechaConexion
        End Get
    End Property
    Public ReadOnly Property ConexionOk As Boolean
        Get
            Return _EnableConnection
        End Get
    End Property
    Public ReadOnly Property MensajeConexion As String
        Get
            Return _MensajeError
        End Get
    End Property


    Private Sub IDisposable_Dispose() Implements IDisposable.Dispose
        DirectCast(_Connection, IDisposable).Dispose()
    End Sub

    Public Sub Dispose()
        IDisposable_Dispose()
    End Sub
End Class
