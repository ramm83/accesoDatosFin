Imports System.Security.Cryptography
Imports System.IO
Imports System.Text
Imports System.Globalization
Imports System.DirectoryServices
Imports System.Configuration


Friend Class Seguridad
#Region "[VARIABLES GLOBALES]"
    Private Key_General As String = "Sodimac_SGL*"
#End Region


#Region "[FUNCIONES DE ENCRIPCION]"

    Public Function EncriptarCadena(ByVal Cadena As String) As String
        Dim CadenaEncriptada As String = String.Empty

        If String.IsNullOrEmpty(Cadena) Then
            Return CadenaEncriptada
        End If

        Try

            Dim Memdata As New MemoryStream
            Dim RC2 As New RC2CryptoServiceProvider 'RC2
            Dim Key_General_Byte() As Byte = Encoding.ASCII.GetBytes(Key_General)
            Dim Transforma_Datos As ICryptoTransform

            'Metodo de encripcion RC2
            Dim PlainText() As Byte = Encoding.ASCII.GetBytes(Cadena)
            RC2.Mode = CipherMode.CBC
            Transforma_Datos = RC2.CreateEncryptor(Key_General_Byte, Encoding.ASCII.GetBytes(Key_General))
            Dim EncodeStream As New CryptoStream(Memdata, Transforma_Datos, CryptoStreamMode.Write)
            EncodeStream.Write(PlainText, 0, PlainText.Length)
            EncodeStream.FlushFinalBlock()
            EncodeStream.Close()

            CadenaEncriptada = Convert.ToBase64String(Memdata.ToArray)

        Catch ex As Exception
            CadenaEncriptada = Nothing
        End Try

        Return CadenaEncriptada

    End Function
    Public Function DesencriptarCadena(ByVal CadenaEncriptada As String) As String

        Dim CadenaDesencriptada As String = String.Empty

        If String.IsNullOrEmpty(CadenaEncriptada) Then
            Return CadenaDesencriptada
        End If

        Try
            Dim Memdata As New MemoryStream
            Dim RC2 As New RC2CryptoServiceProvider 'RC2
            Dim Key_General_Byte() As Byte = Encoding.ASCII.GetBytes(Key_General)
            Dim Transforma_Datos As ICryptoTransform

            'Metodo de encripcion RC2
            Dim Plain_Text() As Byte = Convert.FromBase64String(CadenaEncriptada)
            RC2.Mode = CipherMode.CBC
            Transforma_Datos = RC2.CreateDecryptor(Key_General_Byte, Encoding.ASCII.GetBytes(Key_General))
            Dim EncodeStream As New CryptoStream(Memdata, Transforma_Datos, CryptoStreamMode.Write)
            EncodeStream.Write(Plain_Text, 0, Plain_Text.Length)
            EncodeStream.FlushFinalBlock()
            EncodeStream.Close()

            CadenaDesencriptada = Encoding.ASCII.GetString(Memdata.ToArray)

        Catch ex As Exception
            Return Nothing
        End Try

        Return CadenaDesencriptada

    End Function
#End Region

#Region "[FUNCIONES GENERALES]"

    Public Function GetCadenaConexionBaseDatos(ByVal CadenaConexion As String) As String
        Try

            Dim Cadena = DesencriptarCadena(CadenaConexion)

            If IsNothing(Cadena) Then
                Cadena = CadenaConexion
            End If

            If String.IsNullOrEmpty(Cadena) Then
                Cadena = CadenaConexion
            End If

            Dim ixIni = InStr(Cadena.ToUpper(), "DATA") - 1
            Return Cadena.Substring(ixIni, Cadena.Length - ixIni)

        Catch ex As Exception
            Return String.Empty
        End Try


    End Function

#End Region

End Class
