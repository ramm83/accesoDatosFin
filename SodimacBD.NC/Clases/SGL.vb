Imports System.Data
Imports System.Data.OracleClient
Imports Oracle.ManagedDataAccess.Client

Public Class SGL

    Implements IDisposable


    Private ConexionSGL As Conexion
    Private Const GetConexionId As String = "SELECT SGL.PKG_SGL_SQL.FNC_GET_CONEXION_ID_sQL({0}) FROM DUAL"
    Private Const GetConexionTag As String = "SELECT SGL.PKG_SGL_SQL.FNC_GET_CONEXION_TAG('{0}') FROM DUAL"
    Public ExcepcionSGL As LogMensajesError
    Private Utilidad As New Utilidades
    Private AdoSGL As ADO
    Private CargueMasivo As CargueMasivo

    Public Sub New()
        Dim CadenaConexion As String = Utilidad.FncGetCadenaConexion("SGL", False)
        CrearConexionBase(CadenaConexion)

    End Sub

    Public Sub New(ByVal Path As String)
        Dim CadenaConexion As String = Utilidad.FncGetCadenaConexion("SGL", Path, False)
        CrearConexionBase(CadenaConexion)

    End Sub

    Private Sub CrearConexionBase(ByVal CadenaBaseConexion As String)
        If Not IsNothing(CadenaBaseConexion) AndAlso CadenaBaseConexion.Trim.Length > 0 Then
            ConexionSGL = New Conexion(CadenaBaseConexion)
            If ConexionSGL.ConexionOk Then
                ExcepcionSGL = Nothing
                AdoSGL = New ADO(ConexionSGL.Connection)
            Else
                ExcepcionSGL = New LogMensajesError With {.Codigo = -1, .Comentario = "Falla en la creación de la conexión", .Mensaje = ConexionSGL.MensajeConexion}
                ConexionSGL = Nothing
            End If
        Else
            ExcepcionSGL = New LogMensajesError With {.Codigo = -1, .Comentario = "Falla Fuente Conexión"}
            ConexionSGL = Nothing
        End If
    End Sub
    Private Structure Procedimientos
        Public Const prc_get_sentencia = "PKG_SGL_SQL.PRC_GET_SQL_ORDER"
        Public Const prc_get_reg_script = "PKG_SGL_SQL.PRC_GET_SQL"
        Public Const prc_log As String = "PRC_LOG_EJECUCION_APP"
        Public Const prc_excepcion As String = "PKG_LOG_EXCEPCIONES.PRC_CAPTURA_ERROR"
        Public Const prc_exe_sp As String = "PKG_SGL_SQL.PRC_EXE_SQL"
        Public Const prc_exe_sp_order As String = "PKG_SGL_SQL.PRC_EXE_SQL_ORDER"
        Public Const prc_auditoria_tmp_id As String = "PKG_SGL_SQL.PRC_SET_AUDITORIA_ID"
        Public Const prc_auditoria_tmp_tag As String = "PKG_SGL_SQL.PRC_SET_AUDITORIA_TAG"
    End Structure


#Region "[FUNCIONES PARA OBTENER SENTENCIAS]"
    Public Function fncGetSentencia(ByVal IdSQL As Integer) As String

        If IsNothing(ConexionSGL) Then
            Return Nothing
        End If

        Dim sentencia As String = String.Empty
        Dim lstParametros As New List(Of SQLParametro)
        lstParametros.Add(New SQLParametro With {.Nombre = "P_ID_SQL", .intValor = IdSQL, .Tipo = OracleDbType.Int32})
        lstParametros.Add(New SQLParametro With {.Nombre = "CUR_DATOS", .Entrada = False, .Tipo = OracleDbType.RefCursor})
        Dim TblSQL As DataTable = AdoSGL.StoreProcedure(Procedimientos.prc_get_reg_script, lstParametros)

        If IsNothing(TblSQL) Then
            Return Nothing
        ElseIf TblSQL.Rows.Count < 1 Then
            Return Nothing
        Else
            sentencia = TblSQL.Rows(0).Item("CODIGO").ToString()
        End If

        If IsNothing(sentencia) Then
            prcLogExcepcion(IdSQL, Nothing, Nothing, AdoSGL.Excepcion)
        End If

        Return sentencia


    End Function
    Public Function fncGetSentencia(ByVal Tag As String) As String
        If IsNothing(ConexionSGL) Then
            Return Nothing
        End If


        Dim sentencia As String = String.Empty
        Dim lstParametros As New List(Of SQLParametro)
        lstParametros.Add(New SQLParametro With {.Nombre = "P_TAG", .stringValor = Tag.ToUpper, .Tipo = OracleDbType.Varchar2})
        lstParametros.Add(New SQLParametro With {.Nombre = "CUR_DATOS", .Entrada = False, .Tipo = OracleDbType.RefCursor})
        Dim TblSQL As DataTable = AdoSGL.StoreProcedure(Procedimientos.prc_get_reg_script, lstParametros)

        If IsNothing(TblSQL) Then
            Return Nothing
        ElseIf TblSQL.Rows.Count < 1 Then
            Return Nothing
        Else
            sentencia = TblSQL.Rows(0).Item("CODIGO").ToString()
        End If

        If IsNothing(sentencia) Then
            prcLogExcepcion(Tag, Nothing, Nothing, ExcepcionSGL)
        End If

        Return sentencia


    End Function
    Public Function fncGetSentencia(ByVal IdSQL As Integer, ByVal Parametros As String, ByVal Separador As String) As String

        If IsNothing(ConexionSGL) Then
            Return Nothing
        End If


        If AdoSGL.fncEsVacio(Parametros) Or AdoSGL.fncEsVacio(Separador) Then
            Return Nothing
        End If

        Dim sentencia As String = String.Empty
        Dim lstParametros As New List(Of SQLParametro)
        lstParametros.Add(New SQLParametro With {.Nombre = "P_ID_SQL", .intValor = IdSQL, .Tipo = OracleDbType.Int32})
        lstParametros.Add(New SQLParametro With {.Nombre = "P_PARAMETROS", .stringValor = Parametros, .Tipo = OracleDbType.Varchar2})
        lstParametros.Add(New SQLParametro With {.Nombre = "P_SEPARADOR", .stringValor = Separador, .Tipo = OracleDbType.Varchar2})
        lstParametros.Add(New SQLParametro With {.Nombre = "P_RESULTADO", .Entrada = False, .Tipo = OracleDbType.Clob})
        sentencia = AdoSGL.StoreProcedure(Procedimientos.prc_get_sentencia, lstParametros)

        If IsNothing(sentencia) Then
            prcLogExcepcion(-1, sentencia, Parametros & " - Id SQL: " & IdSQL, AdoSGL.Excepcion)
        End If

        Return sentencia


    End Function
    Public Function fncGetSentencia(ByVal Tag As String, ByVal Parametros As String, ByVal Separador As String) As String

        If IsNothing(ConexionSGL) Then
            Return Nothing
        End If


        If AdoSGL.fncEsVacio(Parametros) Or AdoSGL.fncEsVacio(Separador) Or AdoSGL.fncEsVacio(Tag) Then
            Return Nothing
        End If

        Dim sentencia As String = String.Empty
        Dim lstParametros As New List(Of SQLParametro)
        lstParametros.Add(New SQLParametro With {.Nombre = "P_TAG", .stringValor = Tag.ToUpper, .Tipo = OracleDbType.Varchar2})
        lstParametros.Add(New SQLParametro With {.Nombre = "P_PARAMETROS", .stringValor = Parametros, .Tipo = OracleDbType.Varchar2})
        lstParametros.Add(New SQLParametro With {.Nombre = "P_SEPARADOR", .stringValor = Separador, .Tipo = OracleDbType.Varchar2})
        lstParametros.Add(New SQLParametro With {.Nombre = "P_RESULTADO", .Entrada = False, .Tipo = OracleDbType.Clob})
        sentencia = AdoSGL.StoreProcedure(Procedimientos.prc_get_sentencia, lstParametros)

        If IsNothing(sentencia) Then
            prcLogExcepcion(Tag, sentencia, Parametros, AdoSGL.Excepcion)
        End If

        Return sentencia

    End Function
#End Region
    Private Function FncGetCadenaConexionSQL(ByVal IdSQL As Integer) As String
        Dim CadenaScript As String = AdoSGL.GetEscalar(String.Format(GetConexionId, IdSQL))

        If Not IsNothing(CadenaScript) AndAlso Not String.IsNullOrEmpty(CadenaScript) AndAlso Not (CadenaScript.ToUpper.Trim.Contains("DEFAULT")) Then
            'CadenaScript = Utilidad.GetCadenaConexionBaseDatos(CadenaScript)
            Return CadenaScript
        ElseIf Not IsNothing(CadenaScript) AndAlso Not String.IsNullOrEmpty(CadenaScript) AndAlso CadenaScript.ToUpper.Trim.Contains("DEFAULT") Then
            CadenaScript = Nothing
        End If
        Return CadenaScript

    End Function
    Private Function FncGetCadenaConexionSQL(ByVal Tag As String) As String
        Dim CadenaScript As String = AdoSGL.GetEscalar(String.Format(GetConexionTag, Tag))

        If Not IsNothing(CadenaScript) AndAlso Not String.IsNullOrEmpty(CadenaScript) AndAlso Not (CadenaScript.ToUpper.Trim.Contains("DEFAULT")) Then
            Return CadenaScript
        Else
            Return Nothing
        End If
        Return CadenaScript

    End Function
    Public Sub prcLogExcepcion(obj As LogMensajesError)

        If IsNothing(obj) Then
            Return
        End If

        Dim lstParametros As New List(Of SQLParametro)
        lstParametros.Add(New SQLParametro With {.Nombre = "P_CODIGO", .intValor = obj.Codigo, .Tipo = OracleDbType.Int32})
        lstParametros.Add(New SQLParametro With {.Nombre = "P_DESCRIPCION", .stringValor = obj.Mensaje, .Tipo = OracleDbType.Varchar2})
        lstParametros.Add(New SQLParametro With {.Nombre = "P_COMENTARIO", .stringValor = obj.Comentario & ". Fecha Error: " & obj.Fecha, .Tipo = OracleDbType.Varchar2})
        lstParametros.Add(New SQLParametro With {.Nombre = "P_OBJETO", .stringValor = obj.Objeto, .Tipo = OracleDbType.Varchar2})
        lstParametros.Add(New SQLParametro With {.Nombre = "P_EMAIL", .stringValor = obj.Email, .Tipo = OracleDbType.Varchar2})
        lstParametros.Add(New SQLParametro With {.Nombre = "P_ID_PROCESO", .intValor = obj.IdProceso, .Tipo = OracleDbType.Int32})
        lstParametros.Add(New SQLParametro With {.Nombre = "P_ID_APLICACION", .intValor = obj.IdAplicacion, .Tipo = OracleDbType.Int32})
        AdoSGL.StoreProcedure(Procedimientos.prc_excepcion, lstParametros)
    End Sub
    Public Sub prcLogExcepcion(ByVal IdSQL As Integer, ByVal Sentencia As String, ByVal Parametros As String, obj As LogMensajesError)

        If IsNothing(obj) Then
            Return
        ElseIf obj.Codigo < 0 AndAlso Not obj.Codigo.Equals(-1) Then
            Return
        End If

        Dim lstParametros As New List(Of SQLParametro)
        lstParametros.Add(New SQLParametro With {.Nombre = "P_CODIGO", .intValor = obj.Codigo, .Tipo = OracleDbType.Int32})
        lstParametros.Add(New SQLParametro With {.Nombre = "P_DESCRIPCION", .stringValor = obj.Mensaje, .Tipo = OracleDbType.Varchar2})
        lstParametros.Add(New SQLParametro With {.Nombre = "P_COMENTARIO", .stringValor = obj.Comentario, .Tipo = OracleDbType.Varchar2})
        lstParametros.Add(New SQLParametro With {.Nombre = "P_OBJETO", .stringValor = "[SodimacBD 1.2.0] - " + obj.Objeto, .Tipo = OracleDbType.Varchar2})
        lstParametros.Add(New SQLParametro With {.Nombre = "P_EMAIL", .stringValor = obj.Email, .Tipo = OracleDbType.Varchar2})
        lstParametros.Add(New SQLParametro With {.Nombre = "P_ID_PROCESO", .intValor = obj.IdProceso, .Tipo = OracleDbType.Int32})
        lstParametros.Add(New SQLParametro With {.Nombre = "P_ID_APLICACION", .intValor = obj.IdAplicacion, .Tipo = OracleDbType.Int32})
        AdoSGL.StoreProcedure(Procedimientos.prc_excepcion, lstParametros)
        Dim ObjAuditoriaTmp As New SQLAuditoriaScriptTMP

        If IsNothing(Sentencia) Then
            Sentencia = ""
        End If

        If IsNothing(Parametros) Then
            Parametros = ""
        End If

        ObjAuditoriaTmp.IdSQL = IdSQL
        ObjAuditoriaTmp.TagSQL = String.Empty
        ObjAuditoriaTmp.Sentencia = Sentencia
        ObjAuditoriaTmp.Parametros = Parametros
        ObjAuditoriaTmp.Resultado = "ERROR"
        ObjAuditoriaTmp.Observacion = obj.Mensaje
        ObjAuditoriaTmp.CodigoInterno = "-1"
        ObjAuditoriaTmp.EstadoInterno = obj.Comentario
        prcLogAuditoriaTmp(ObjAuditoriaTmp)

    End Sub

    Public Sub prcAuditoriaTmp(ByVal IdSQL As Integer, ByVal Sentencia As String, ByVal Parametros As String, ByVal Observacion As String, ByVal Resultado As String)

        Dim ObjAuditoriaTmp As New SQLAuditoriaScriptTMP

        If IsNothing(Sentencia) Then
            Sentencia = ""
        End If

        If IsNothing(Parametros) Then
            Parametros = ""
        End If

        ObjAuditoriaTmp.IdSQL = IdSQL
        ObjAuditoriaTmp.TagSQL = String.Empty
        ObjAuditoriaTmp.Sentencia = Sentencia
        ObjAuditoriaTmp.Parametros = Parametros
        ObjAuditoriaTmp.Resultado = Resultado
        ObjAuditoriaTmp.Observacion = Observacion
        ObjAuditoriaTmp.CodigoInterno = "-1"
        ObjAuditoriaTmp.EstadoInterno = "-1"
        prcLogAuditoriaTmp(ObjAuditoriaTmp)

    End Sub


    Public Sub prcLogExcepcion(ByVal Tag As String, ByVal Sentencia As String, ByVal Parametros As String, obj As LogMensajesError)

        If IsNothing(obj) Then
            Return
        ElseIf obj.Codigo < 0 AndAlso Not obj.Codigo.Equals(-1) Then
            Return
        End If

        Dim lstParametros As New List(Of SQLParametro)
        lstParametros.Add(New SQLParametro With {.Nombre = "P_CODIGO", .intValor = obj.Codigo, .Tipo = OracleDbType.Int32})
        lstParametros.Add(New SQLParametro With {.Nombre = "P_DESCRIPCION", .stringValor = obj.Mensaje, .Tipo = OracleDbType.Varchar2})
        lstParametros.Add(New SQLParametro With {.Nombre = "P_COMENTARIO", .stringValor = obj.Comentario & ". Fecha Error: " & obj.Fecha, .Tipo = OracleDbType.Varchar2})
        lstParametros.Add(New SQLParametro With {.Nombre = "P_OBJETO", .stringValor = obj.Objeto, .Tipo = OracleDbType.Varchar2})
        lstParametros.Add(New SQLParametro With {.Nombre = "P_EMAIL", .stringValor = obj.Email, .Tipo = OracleDbType.Varchar2})
        lstParametros.Add(New SQLParametro With {.Nombre = "P_ID_PROCESO", .intValor = obj.IdProceso, .Tipo = OracleDbType.Int32})
        lstParametros.Add(New SQLParametro With {.Nombre = "P_ID_APLICACION", .intValor = obj.IdAplicacion, .Tipo = OracleDbType.Int32})
        AdoSGL.StoreProcedure(Procedimientos.prc_excepcion, lstParametros)
        Dim ObjAuditoriaTmp As New SQLAuditoriaScriptTMP

        If IsNothing(Sentencia) Then
            Sentencia = ""
        End If

        If IsNothing(Parametros) Then
            Parametros = ""
        End If

        ObjAuditoriaTmp.IdSQL = -1
        ObjAuditoriaTmp.TagSQL = Tag
        ObjAuditoriaTmp.Sentencia = Sentencia
        ObjAuditoriaTmp.Parametros = Parametros
        ObjAuditoriaTmp.Resultado = "ERROR"
        ObjAuditoriaTmp.Observacion = obj.Mensaje
        ObjAuditoriaTmp.CodigoInterno = "-1"
        ObjAuditoriaTmp.EstadoInterno = obj.Comentario
        prcLogAuditoriaTmp(ObjAuditoriaTmp)

    End Sub
    Public Sub prcLogAuditoriaTmp(ByVal obj As SQLAuditoriaScriptTMP)

        If IsNothing(obj) Then
            Return
        End If
        Dim prc As String = String.Empty

        Dim lstParametros As New List(Of SQLParametro)


        If obj.IdSQL > 0 Then
            lstParametros.Add(New SQLParametro With {.Nombre = "P_ID_SQL", .intValor = obj.IdSQL, .Tipo = OracleDbType.Int32})
            prc = Procedimientos.prc_auditoria_tmp_id
        ElseIf Not String.IsNullOrEmpty(obj.TagSQL) Then
            lstParametros.Add(New SQLParametro With {.Nombre = "P_TAG", .stringValor = obj.TagSQL, .Tipo = OracleDbType.Int32})
            prc = Procedimientos.prc_auditoria_tmp_tag
        Else
            obj.IdSQL = -1
            lstParametros.Add(New SQLParametro With {.Nombre = "P_ID_SQL", .intValor = obj.IdSQL, .Tipo = OracleDbType.Int32})
            prc = Procedimientos.prc_auditoria_tmp_id
        End If

        lstParametros.Add(New SQLParametro With {.Nombre = "P_SENTENCIA", .stringValor = obj.Sentencia, .Tipo = OracleDbType.Varchar2})
        lstParametros.Add(New SQLParametro With {.Nombre = "P_PARAMETROS", .stringValor = obj.Parametros, .Tipo = OracleDbType.Varchar2})
        lstParametros.Add(New SQLParametro With {.Nombre = "P_RESULTADO", .stringValor = obj.Resultado, .Tipo = OracleDbType.Varchar2})
        lstParametros.Add(New SQLParametro With {.Nombre = "P_CODIGO_INTERNO", .stringValor = obj.CodigoInterno, .Tipo = OracleDbType.Varchar2})
        lstParametros.Add(New SQLParametro With {.Nombre = "P_ESTADO_INTERNO", .stringValor = obj.EstadoInterno, .Tipo = OracleDbType.Varchar2})
        lstParametros.Add(New SQLParametro With {.Nombre = "P_OBSERVACION", .stringValor = obj.Observacion, .Tipo = OracleDbType.Varchar2})
        Dim Resultado = AdoSGL.StoreProcedure(prc, lstParametros)

    End Sub

#Region "[CURSORES]"
    Private Function GetDataTable(ByVal Sentencia As String, ByVal CadenaConexionSentencia As String, ByVal IdSQL As Integer, ByVal Parametros As String, ByVal Separador As String) As DataTable
        Dim DataBaseTmp As ADO

        If IsNothing(CadenaConexionSentencia) Then
            DataBaseTmp = New ADO(ConexionSGL.Connection)
        ElseIf String.IsNullOrEmpty(CadenaConexionSentencia) Then
            DataBaseTmp = New ADO(ConexionSGL.Connection)
        Else
            DataBaseTmp = New ADO(CadenaConexionSentencia)
        End If


        If Not IsNothing(DataBaseTmp.Excepcion) Then
            prcLogExcepcion(IdSQL, Sentencia, Parametros, DataBaseTmp.Excepcion)

        Else
            Dim Resultado = DataBaseTmp.GetDataTable(Sentencia)

            If Not IsNothing(DataBaseTmp.Excepcion) Then
                prcLogExcepcion(IdSQL, Sentencia, Parametros, DataBaseTmp.Excepcion)
            End If

            Return Resultado

        End If
        Return Nothing
    End Function
    Private Function GetDataTable(ByVal Sentencia As String, ByVal CadenaConexionSentencia As String, ByVal Tag As String, ByVal Parametros As String, ByVal Separador As String) As DataTable
        Dim DataBaseTmp As ADO

        If IsNothing(CadenaConexionSentencia) Then
            DataBaseTmp = New ADO(ConexionSGL.Connection)
        ElseIf String.IsNullOrEmpty(CadenaConexionSentencia) Then
            DataBaseTmp = New ADO(ConexionSGL.Connection)
        Else
            DataBaseTmp = New ADO(CadenaConexionSentencia)
        End If

        If Not IsNothing(DataBaseTmp.Excepcion) Then
            prcLogExcepcion(Tag, Sentencia, Parametros, DataBaseTmp.Excepcion)

        Else
            Dim Resultado = DataBaseTmp.GetDataTable(Sentencia)

            If Not IsNothing(DataBaseTmp.Excepcion) Then
                prcLogExcepcion(Tag, Sentencia, Parametros, DataBaseTmp.Excepcion)
            End If

            Return Resultado
        End If
        Return Nothing
    End Function
    Public Overloads Function fncGetCursorIdSQLParametros(ByVal IdSQL As Integer, ByVal Parametros As String, ByVal Separador As String) As DataTable

        Dim Sentencia As String = fncGetSentencia(IdSQL, Parametros, Separador)

        If IsNothing(Sentencia) Then
            Return Nothing
        ElseIf AdoSGL.fncEsVacio(Sentencia) Then
            Return Nothing
        Else
            Dim CadenaConexionTemporal = FncGetCadenaConexionSQL(IdSQL)
            Return GetDataTable(Sentencia, CadenaConexionTemporal, IdSQL, Parametros, Separador)
        End If
        Return Nothing
    End Function
    Public Overloads Function fncGetCursorTagParametros(ByVal Tag As String, ByVal Parametros As String, ByVal Separador As String) As DataTable

        Dim Sentencia As String = fncGetSentencia(Tag, Parametros, Separador)

        If IsNothing(Sentencia) Then
            Return Nothing
        ElseIf AdoSGL.fncEsVacio(Sentencia) Then
            Return Nothing
        Else
            Dim CadenaConexionTemporal = FncGetCadenaConexionSQL(Tag)

            Return GetDataTable(Sentencia, CadenaConexionTemporal, Tag, Parametros, Separador)

        End If
        Return Nothing
    End Function
    Public Overloads Function fncGetCursorIdSQL(ByVal IdSQL As Integer) As DataTable

        Dim Sentencia As String = fncGetSentencia(IdSQL)


        If IsNothing(Sentencia) Then
            Return Nothing
        ElseIf AdoSGL.fncEsVacio(Sentencia) Then
            Return Nothing
        Else
            Dim CadenaConexionTemporal = FncGetCadenaConexionSQL(IdSQL)
            Return GetDataTable(Sentencia, CadenaConexionTemporal, IdSQL, "", "")
        End If
        Return Nothing
    End Function
    Public Overloads Function fncGetCursorTag(ByVal Tag As String) As DataTable

        Dim Sentencia As String = fncGetSentencia(Tag)

        If IsNothing(Sentencia) Then
            Return Nothing
        ElseIf AdoSGL.fncEsVacio(Sentencia) Then
            Return Nothing
        Else
            Dim CadenaConexionTemporal = FncGetCadenaConexionSQL(Tag)
            Return GetDataTable(Sentencia, CadenaConexionTemporal, Tag, "", "")
        End If
        Return Nothing

    End Function

#End Region

#Region "[CARGUE MASIVO]"

    Public Function CrearCargueMasivo(ByVal IdTipoCm As Integer, ByVal Lineas As Integer, usuario As String) As Integer
        Dim prc As String = "PKG_CM.PRC_CREA_CARGUE"
        CargueMasivo = Nothing


        Dim lst As New List(Of SQLParametro)
        lst.Add(New SQLParametro With {.Nombre = "P_ID_TIPO_CM", .intValor = IdTipoCm, .Tipo = OracleDbType.Int32})
        lst.Add(New SQLParametro With {.Nombre = "P_LINEAS", .intValor = Lineas, .Tipo = OracleDbType.Int32})
        lst.Add(New SQLParametro With {.Nombre = "P_USUARIO", .stringValor = usuario, .Tipo = OracleDbType.Varchar2})
        lst.Add(New SQLParametro With {.Nombre = "P_ID_ESTADO", .intValor = 5, .Tipo = OracleDbType.Int32})
        lst.Add(New SQLParametro With {.Nombre = "P_RESULTADO", .Entrada = False, .Tipo = OracleDbType.Int32})

        Dim resultado = CInt(AdoSGL.StoreProcedure(prc, lst))

        If resultado > 0 Then

            Dim tbl = fncGetCursorTagParametros("GETINFTICM", "#" & IdTipoCm.ToString, "#")

            If Not IsNothing(tbl) AndAlso tbl.Rows.Count > 0 Then

                CargueMasivo = New CargueMasivo
                CargueMasivo.IdTipoCm = IdTipoCm
                CargueMasivo.Descripcion = tbl(0)("DESCRIPCION").ToString
                CargueMasivo.IdDataBase = CInt(tbl(0)("ID_DATABASE"))
                CargueMasivo.IdSQLGet = CInt(tbl(0)("ID_SQL_GET"))
                CargueMasivo.IdSQLSet = CInt(tbl(0)("ID_SQL_SET"))
                CargueMasivo.IdEstado = CInt(tbl(0)("ID_ESTADO"))
                CargueMasivo.IdProceso = CInt(tbl(0)("ID_PROCESO"))
                CargueMasivo.EjecutarTodasLasLineas = tbl(0)("ALL_ROWS_EXE").ToString


            End If

        End If

        Return resultado
    End Function
    Public Function CrearLineaCargueMasivo(ByVal IdCargue As Integer, ByVal Lineas As List(Of String)) As Integer
        Dim prc As String = "PKG_CM.PRC_CREA_DETALLE_CARGUE"
        Dim Ix As Integer = 0
        For Each Linea As String In Lineas
            Dim lst As New List(Of SQLParametro)
            Ix += 1
            lst.Add(New SQLParametro With {.Nombre = "P_ID_CM_CARGUE", .intValor = IdCargue, .Tipo = OracleDbType.Int32})
            lst.Add(New SQLParametro With {.Nombre = "P_DATA", .stringValor = Linea, .Tipo = OracleDbType.Varchar2})
            lst.Add(New SQLParametro With {.Nombre = "P_ID_ESTADO", .intValor = 5, .Tipo = OracleDbType.Int32})
            lst.Add(New SQLParametro With {.Nombre = "P_LINEA", .intValor = Ix, .Tipo = OracleDbType.Int32})
            Dim resultado = AdoSGL.StoreProcedure(prc, lst)

        Next

        If Not IsNothing(AdoSGL.Excepcion) Then
            Return -1
        End If

        Return 1

    End Function
    Private Function EjecutaSentencia(ByVal Sentencia As String, ByVal CadenaConexionSentencia As String, ByVal IdSQL As Integer, ByVal Parametros As String, ByVal Separador As String) As Integer
        Dim DataBaseTmp As ADO

        If IsNothing(CadenaConexionSentencia) Then
            DataBaseTmp = New ADO(ConexionSGL.Connection)
        ElseIf String.IsNullOrEmpty(CadenaConexionSentencia) Then
            DataBaseTmp = New ADO(ConexionSGL.Connection)
        Else
            DataBaseTmp = New ADO(CadenaConexionSentencia)
        End If


        If Not IsNothing(DataBaseTmp.Excepcion) Then
            prcLogExcepcion(IdSQL, Sentencia, Parametros, DataBaseTmp.Excepcion)

        Else
            Dim Resultado = DataBaseTmp.EjecutarSentencia(Sentencia)

            If Not IsNothing(DataBaseTmp.Excepcion) Then
                prcLogExcepcion(IdSQL, Sentencia, Parametros, DataBaseTmp.Excepcion)
            End If

            Return Resultado

        End If
        Return Nothing
    End Function
    Public Function ProcesarCargueMasivo(ByVal IdCargue As Integer) As Integer

        If Not IsNothing(CargueMasivo) AndAlso CargueMasivo.IdSQLSet > 0 Then
            Dim Sentencia As String = fncGetSentencia(CargueMasivo.IdSQLSet, "#" + IdCargue.ToString, "#")

            If IsNothing(Sentencia) Then
                prcAuditoriaTmp(CargueMasivo.IdSQLSet, "Vacia", "Vacio", "Error Obteniendo Sentencia SET", "ERROR")
                Return -2
            ElseIf AdoSGL.fncEsVacio(Sentencia) Then
                prcAuditoriaTmp(CargueMasivo.IdSQLSet, "Vacia", "Vacio", "Error Obteniendo Sentencia SET", "ERROR")
                Return -2
            Else
                Dim CadenaConexionTemporal = FncGetCadenaConexionSQL(CargueMasivo.IdSQLSet)
                prcAuditoriaTmp(CargueMasivo.IdSQLSet, Sentencia, "#" + IdCargue.ToString(), "Ejecucion SET", "PENDIENTE")
                Return EjecutaSentencia(Sentencia, CadenaConexionTemporal, CargueMasivo.IdSQLSet, "#" + IdCargue.ToString, "#")
            End If
        Else
            Return -2
        End If

    End Function
    Public Function GetCargueMasivo(ByVal IdCargue As Integer) As DataTable

        If Not IsNothing(CargueMasivo) AndAlso CargueMasivo.IdSQLGet > 0 Then
            Dim Sentencia As String = fncGetSentencia(CargueMasivo.IdSQLGet, "#" + IdCargue.ToString, "#")

            If IsNothing(Sentencia) Then
                Return Nothing
            ElseIf AdoSGL.fncEsVacio(Sentencia) Then
                Return Nothing
            Else
                Dim CadenaConexionTemporal = FncGetCadenaConexionSQL(CargueMasivo.IdSQLGet)
                Return GetDataTable(Sentencia, CadenaConexionTemporal, CargueMasivo.IdSQLGet, "#" + IdCargue.ToString, "#")
            End If
        Else
            Return Nothing
        End If

    End Function

    Public Function GetCargueMasivoTipo(ByVal IdTIpoCM As Integer, ByVal IdCargue As Integer) As DataTable
        CargueMasivo = Nothing
        Dim tbl = fncGetCursorTagParametros("GETINFTICM", "#" & IdTIpoCM.ToString, "#")

        If Not IsNothing(tbl) AndAlso tbl.Rows.Count > 0 Then

            CargueMasivo = New CargueMasivo
            CargueMasivo.IdTipoCm = IdTIpoCM
            CargueMasivo.Descripcion = tbl(0)("DESCRIPCION").ToString
            CargueMasivo.IdDataBase = CInt(tbl(0)("ID_DATABASE"))
            CargueMasivo.IdSQLGet = CInt(tbl(0)("ID_SQL_GET"))
            CargueMasivo.IdSQLSet = CInt(tbl(0)("ID_SQL_SET"))
            CargueMasivo.IdEstado = CInt(tbl(0)("ID_ESTADO"))
            CargueMasivo.IdProceso = CInt(tbl(0)("ID_PROCESO"))
            CargueMasivo.EjecutarTodasLasLineas = tbl(0)("ALL_ROWS_EXE").ToString

            Return GetCargueMasivo(IdCargue)

        End If

        Return Nothing

    End Function

#End Region

#Region "[PROCEDIMIENTOS ALMACENADOS]"


    Private Function ExeStoreProcedure(ByVal Tag As String, Procedimiento As String, ByVal Parametros As List(Of SQLParametro), ByVal CadenaConexionSP As String)

        Dim DataBaseTmp As ADO
        ExcepcionSGL = Nothing

        If IsNothing(CadenaConexionSP) Then
            DataBaseTmp = New ADO(ConexionSGL.Connection)
        ElseIf String.IsNullOrEmpty(CadenaConexionSP) Then
            DataBaseTmp = New ADO(ConexionSGL.Connection)
        Else
            DataBaseTmp = New ADO(CadenaConexionSP)
        End If


        If Not IsNothing(DataBaseTmp.Excepcion) Then
            prcLogExcepcion(Tag, Procedimiento, "", DataBaseTmp.Excepcion)

        Else
            Dim Obj = DataBaseTmp.StoreProcedure(Procedimiento, Parametros)

            If Not IsNothing(DataBaseTmp.Excepcion) Then
                ExcepcionSGL = New LogMensajesError
                ExcepcionSGL.Codigo = DataBaseTmp.Excepcion.Codigo
                ExcepcionSGL.Mensaje = DataBaseTmp.Excepcion.Mensaje
                prcLogExcepcion(Tag, Procedimiento, "", DataBaseTmp.Excepcion)
            End If

            Return Obj

        End If
        Return Nothing

    End Function
    Private Function ExeStoreProcedure(ByVal IdSQL As Integer, Procedimiento As String, ByVal Parametros As List(Of SQLParametro), ByVal CadenaConexionSP As String)

        Dim DataBaseTmp As ADO

        If IsNothing(CadenaConexionSP) Then
            DataBaseTmp = New ADO(ConexionSGL.Connection)
        ElseIf String.IsNullOrEmpty(CadenaConexionSP) Then
            DataBaseTmp = New ADO(ConexionSGL.Connection)
        Else
            DataBaseTmp = New ADO(CadenaConexionSP)
        End If


        If Not IsNothing(DataBaseTmp.Excepcion) Then
            prcLogExcepcion(IdSQL, Procedimiento, "", DataBaseTmp.Excepcion)

        Else
            Dim Obj = DataBaseTmp.StoreProcedure(Procedimiento, Parametros)

            If Not IsNothing(DataBaseTmp.Excepcion) Then
                ExcepcionSGL = New LogMensajesError
                ExcepcionSGL.Codigo = DataBaseTmp.Excepcion.Codigo
                ExcepcionSGL.Mensaje = DataBaseTmp.Excepcion.Mensaje
                prcLogExcepcion(IdSQL, Procedimiento, "", DataBaseTmp.Excepcion)
            End If

            Return Obj

        End If
        Return Nothing

    End Function
    Private Function ExeStoreProcedure(Procedimiento As String, ByVal Parametros As List(Of SQLParametro))

        Dim DataBaseTmp As ADO
        DataBaseTmp = New ADO(ConexionSGL.Connection)

        If Not IsNothing(DataBaseTmp.Excepcion) Then
            prcLogExcepcion(-1, Procedimiento, "", DataBaseTmp.Excepcion)

        Else
            Dim Obj = DataBaseTmp.StoreProcedure(Procedimiento, Parametros)

            If Not IsNothing(DataBaseTmp.Excepcion) Then
                ExcepcionSGL = New LogMensajesError
                ExcepcionSGL.Objeto = Procedimiento
                ExcepcionSGL.Codigo = DataBaseTmp.Excepcion.Codigo
                ExcepcionSGL.Mensaje = DataBaseTmp.Excepcion.Mensaje
                prcLogExcepcion(ExcepcionSGL)
            End If

            Return Obj

        End If
        Return Nothing

    End Function
    Public Function StoreProcedureIdSQL(ByVal IdSQL As Integer, Procedimiento As String, ByVal Parametros As List(Of SQLParametro))

        Dim CadenaConexionTemporal = FncGetCadenaConexionSQL(IdSQL)
        Return ExeStoreProcedure(IdSQL, Procedimiento, Parametros, CadenaConexionTemporal)


    End Function
    Public Function StoreProcedureTag(ByVal Tag As String, Procedimiento As String, ByVal Parametros As List(Of SQLParametro))

        Dim CadenaConexionTemporal = FncGetCadenaConexionSQL(Tag)
        Return ExeStoreProcedure(Tag, Procedimiento, Parametros, CadenaConexionTemporal)


    End Function
    Public Function StoreProcedureSGL(Procedimiento As String, ByVal Parametros As List(Of SQLParametro))
        Return ExeStoreProcedure(Procedimiento, Parametros)
    End Function

#End Region
#Region "[SENTENCIAS]"
    Public Function EjecutarSentencia(ByVal Sentencia As String) As Integer
        Dim DataBaseTmp As ADO
        DataBaseTmp = New ADO(ConexionSGL.Connection)
        Return DataBaseTmp.EjecutarSentencia(Sentencia)
    End Function

    Public Function EjecutarTag(ByVal Tag As String, ByVal Parametros As String, ByVal Separador As String) As Integer

        Dim Sentencia As String = fncGetSentencia(Tag, Parametros, Separador)

        If Not IsNothing(Sentencia) Then

            If Not String.IsNullOrEmpty(Sentencia) Then

                Dim CadenaConexionTemporal = FncGetCadenaConexionSQL(Tag)

                Dim DataBaseTmp As ADO
                ExcepcionSGL = Nothing

                If IsNothing(CadenaConexionTemporal) Then
                    DataBaseTmp = New ADO(ConexionSGL.Connection)
                ElseIf String.IsNullOrEmpty(CadenaConexionTemporal) Then
                    DataBaseTmp = New ADO(ConexionSGL.Connection)
                Else
                    DataBaseTmp = New ADO(CadenaConexionTemporal)
                End If


                If Not IsNothing(DataBaseTmp.Excepcion) Then
                    prcLogExcepcion(Tag, Tag, "", DataBaseTmp.Excepcion)
                Else
                    Dim Resultado = DataBaseTmp.EjecutarSentencia(Sentencia)
                    Dim Obj As SQLAuditoriaScriptTMP = New SQLAuditoriaScriptTMP

                    If Resultado > 0 Then
                        Obj.Resultado = "EXITO"
                    Else
                        Obj.Resultado = "ERROR"
                    End If

                    Obj.TagSQL = Tag
                    Obj.Parametros = ""
                    Obj.Sentencia = Sentencia
                    Obj.Observacion = ""
                    Obj.CodigoInterno = "-1"
                    Obj.EstadoInterno = "-1"
                    Obj.IdSQL = -1
                    prcLogAuditoriaTmp(Obj)

                    Return Resultado

                End If

            End If

        End If

        Return -1

    End Function

#Region "IDisposable Support"
    'Private disposedValue As Boolean ' Para detectar llamadas redundantes

    Protected Overridable Sub Dispose(disposing As Boolean)
        'If Not disposedValue Then
        '    If disposing Then
        '        AdoSGL.Dispose()
        '    End If
        'End If
        'disposedValue = True
        If disposing Then
            AdoSGL.Dispose()
            MyBase.Finalize()
        End If
    End Sub

    ' TODO: reemplace Finalize() solo si el anterior Dispose(disposing As Boolean) tiene código para liberar recursos no administrados.
    'Protected Overrides Sub Finalize()
    '    ' No cambie este código. Coloque el código de limpieza en el anterior Dispose(disposing As Boolean).
    '    Dispose(False)
    '    MyBase.Finalize()
    'End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
    End Sub
#End Region
#End Region


End Class
