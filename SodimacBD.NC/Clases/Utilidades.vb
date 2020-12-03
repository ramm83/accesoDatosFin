Imports System.Data
Imports Oracle.ManagedDataAccess.Client

Friend Class Utilidades

    Private Seguridad As New Seguridad()
    Public Function FncGetCadenaConexion(ByVal DataBase As String, Optional ByVal Desencriptar As Boolean = True) As String

        Dim CadenaConexion As String

        If IsNothing(DataBase) Then
            CadenaConexion = Nothing
        ElseIf String.IsNullOrEmpty(DataBase) Then
            CadenaConexion = Nothing
        Else
            Dim ds As New DataSet()
            Try
                ds.ReadXml("DbConex.xml")
                Dim dr As DataRow = ds.Tables(0).Rows(0)

                CadenaConexion = dr(DataBase.ToUpper()).ToString()

                If Not IsNothing(CadenaConexion) AndAlso Not String.IsNullOrEmpty(CadenaConexion) Then

                    If Desencriptar Then
                        CadenaConexion = Seguridad.GetCadenaConexionBaseDatos(CadenaConexion)
                    End If
                Else
                    CadenaConexion = Nothing
                End If

            Catch ex As Exception
                CadenaConexion = Nothing
            End Try
        End If

        Return CadenaConexion

    End Function


    Public Function FncGetCadenaConexion(ByVal DataBase As String, ByVal Path As String, Optional ByVal Desencriptar As Boolean = True) As String

        Dim CadenaConexion As String

        If IsNothing(DataBase) Then
            CadenaConexion = Nothing
        ElseIf String.IsNullOrEmpty(DataBase) Then
            CadenaConexion = Nothing
        Else
            Dim ds As New DataSet()
            Try
                'ds.ReadXml(Path)
                'Dim dr As DataRow = ds.Tables(0).Rows(0)

                CadenaConexion = "43KSGSclC90pTqwTiB55qdJiUcZDN3c83ecALxebGQAbPIoYEgzjw0DF5+APLqzAyzGX1JUbXnqVyKK08UguI770Jz9grfJYxUcZ4y9koerzBy4y0jyAtEWSA69Bl6NVOeVe2yFMKJpMF6L+F1vlD1VMMxzSz2XcEMvzIA95dkRD0FKWdf6l0rIFE3ssGjfT/pvpPhWRF9qS2c+Wc2N62v8QQqQYvhKVYgaAnuvoJD6wCIpLSBQwdw==" 'dr(DataBase.ToUpper()).ToString()

                If Not IsNothing(CadenaConexion) AndAlso Not String.IsNullOrEmpty(CadenaConexion) Then
                    If Desencriptar Then
                        CadenaConexion = Seguridad.GetCadenaConexionBaseDatos(CadenaConexion)
                    End If
                Else
                    CadenaConexion = Nothing
                End If

            Catch ex As Exception
                CadenaConexion = Nothing
            End Try
        End If

        Return CadenaConexion

    End Function
    Public Function GetCadenaConexionBaseDatos(ByVal CadenaConexion As String) As String
        Return Seguridad.GetCadenaConexionBaseDatos(CadenaConexion)
    End Function
    Public Function GetEsquema(ByVal Conexion As OracleConnection) As String

        If IsNothing(Conexion) Then
            Return Nothing
        End If

        Dim Esquema As String = GetEsquema(Conexion.ConnectionString)

        Return Esquema

    End Function
    Public Function GetEsquema(ByVal CadenaConexion As String) As String
        CadenaConexion = CadenaConexion.ToUpper()
        Dim IxInicial As Integer = InStr(CadenaConexion, "USER ID") - 1
        Dim IxFinal As Integer = InStr(CadenaConexion, "PASSWORD") - IxInicial
        CadenaConexion = CadenaConexion.Substring(IxInicial, IxFinal)
        CadenaConexion = CadenaConexion.Substring(0, IxFinal)
        CadenaConexion = CadenaConexion.Split("=").ElementAt(1).Trim().Split(";").ElementAt(0)
        Return CadenaConexion
    End Function
    Public Function GetDataBase(ByVal CadenaConexion As String) As String
        Dim DataBase As String = String.Empty

        If IsNothing(CadenaConexion) Then
            Return DataBase
        ElseIf String.IsNullOrEmpty(CadenaConexion) Then
            Return DataBase
        Else
            DataBase = CadenaConexion.ToUpper.Split(";").ElementAt(0).Split("=").ElementAt(1).Trim
        End If

        Return DataBase
    End Function

    Public Function EncriptarCadenaConexion(ByVal CadenaConexion As String) As String
        Return Seguridad.EncriptarCadena(CadenaConexion)
    End Function

End Class
