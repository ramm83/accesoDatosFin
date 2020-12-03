Imports System.Data.OracleClient
Imports Oracle.ManagedDataAccess.Client

Public Class SQLParametro
    Public Nombre As String
    Public Tipo As OracleDbType
    Public intValor As Integer = 0
    Public douValor As Double = 0
    Public stringValor As String = Nothing
    Public DateValor As String = Nothing
    Public Entrada As Boolean = True
End Class
