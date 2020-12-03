using System;
using System.Collections.Generic;
using System.Data;
using AccesoDatos.Entidades;
using System.Text;
using SodimacBD.NC;
using AccesoDatos.Log;
using Oracle.ManagedDataAccess.Client;
using Newtonsoft.Json;

namespace AccesoDatos.Negocio
{
    public static class AccesoDatosBll
    {
        // IMPORTANTE
        // Descripción de Códigos en el dt:
        // 1. Excepción Conexiones
        // 2. Excepción Base de datos
        // 3. Excepción Metodo en la capa de negocio
        // 4. Consulta ejecutada pero sin datos de devolución

        #region Variables globales
        private static readonly string pathDbConex = @"C:\inetpub\wwwroot\DbConex.xml";//ConfigurationManager.AppSettings["RutaDBConex"];
        private static readonly string spSetEjecucion = "PRC_SGL_SET_EJECUCION_INTERNET";
        #endregion

        #region Metodos auxiliares
        private static DataTable SetMensaje(string id, string mensaje)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Ban");
            dt.Columns.Add("Id");
            dt.Columns.Add("Mensaje");
            DataRow row = dt.NewRow();
            row["Ban"] = "mensaje";
            row["Id"] = id;
            row["Mensaje"] = mensaje;
            dt.Rows.Add(row);
            return dt;
        }
        public static void RegistrarError(SodimacBD.NC.LogMensajesError log)
        {
            SGL sgl = new SGL(pathDbConex);
            sgl.prcLogExcepcion(log);
        }
        public static void RegistrarError(MensajeError m)
        {
            SGL sgl = new SGL(pathDbConex);
            SodimacBD.NC.LogMensajesError log = new SodimacBD.NC.LogMensajesError
            {
                Mensaje = m.Mensaje,
                Comentario = m.Comentario,
                Fecha = m.Fecha
            };

            sgl.prcLogExcepcion(log);
        }
        private static DataTable ControlExepcionesNegocio(Exception e)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Código", typeof(string));
            dt.Columns.Add("Mensaje", typeof(string));
            dt.Columns.Add("Comentario", typeof(string));
            dt.Columns.Add("Fecha", typeof(string));

            dt.Rows.Add("3", e.Message, e.StackTrace, DateTime.Now.ToShortDateString());
            LogException.ErrorRegisterException(e);

            return dt;
        }
        public static void ControlErroresControlador(Exception ex, string controlador, string metodo)
        {
            SodimacBD.NC.LogMensajesError l = new SodimacBD.NC.LogMensajesError();
            if (ex.InnerException != null)
            {
                l.Mensaje = ex.InnerException.Message + " :: " + ex.InnerException.StackTrace
                    + " :: " + ex.InnerException.Data;

            }
            else
            {
                l.Mensaje = ex.Message + " :: " + ex.StackTrace
                     + " :: " + ex.Data;
            }
            l.Comentario = controlador + " - " + metodo;
            l.Fecha = DateTime.Now.ToShortDateString();


            RegistrarError(l);
        }
        public static void ReemplazarRetonoCarril(ParametrosCM parametrosCM)
        {
            for (int i = 0; i < parametrosCM.LstLineas.Count; i++)
            {
                parametrosCM.LstLineas[i] = parametrosCM.LstLineas[i].Replace("\r", "");
            }
        }
        
        public static string ControlErrores(Exception ex, string comentario)
        {
            LogMensajesError l = new LogMensajesError();
            if (ex.InnerException != null)
            {
                l.Mensaje = ex.InnerException.Message + " :: " + ex.InnerException.StackTrace
                    + " :: " + ex.InnerException.Data;

            }
            else
            {
                l.Mensaje = ex.Message + " :: " + ex.StackTrace
                     + " :: " + ex.Data;
            }
            l.Comentario = comentario;
            l.Fecha = DateTime.Now.ToShortDateString();

            RegistrarError(l);

            return l.Mensaje + " :: " + l.Comentario + " :: " + l.Fecha;
        }
        #endregion

        #region Controlador Reportes
        public static DataTable FncGetCursorTag(string tag)
        {
            #region Varables
            DataTable dt;
            SGL sgl;
            #endregion

            try
            {
                sgl = new SGL(pathDbConex);

                if (sgl.ExcepcionSGL != null)
                {
                    dt = new DataTable();
                    dt.Columns.Add("Código", typeof(string));
                    dt.Columns.Add("Mensaje", typeof(string));
                    dt.Columns.Add("Comentario", typeof(string));
                    dt.Columns.Add("Fecha", typeof(string));

                    dt.Rows.Add("1", sgl.ExcepcionSGL.Mensaje, sgl.ExcepcionSGL.Comentario, sgl.ExcepcionSGL.Fecha);
                    LogException.ErrorRegisterException(new Exception(sgl.ExcepcionSGL.Mensaje + " :: " + sgl.ExcepcionSGL.Comentario + " :: " + sgl.ExcepcionSGL.Fecha));

                }
                else
                {
                    dt = sgl.fncGetCursorTag(tag);

                    if (sgl.ExcepcionSGL != null)
                    {
                        dt = new DataTable();
                        dt.Columns.Add("Código", typeof(string));
                        dt.Columns.Add("Mensaje", typeof(string));
                        dt.Columns.Add("Comentario", typeof(string));
                        dt.Columns.Add("Fecha", typeof(string));

                        dt.Rows.Add("2", sgl.ExcepcionSGL.Mensaje, sgl.ExcepcionSGL.Comentario, sgl.ExcepcionSGL.Fecha);
                        LogException.ErrorRegisterException(new Exception(sgl.ExcepcionSGL.Mensaje + " :: " + sgl.ExcepcionSGL.Comentario + " :: " + sgl.ExcepcionSGL.Fecha));
                    }

                    // La dll devuelve null cuando el tag no existe en bd y otras situaciones 
                    // que se implementaran en un futuro
                    if (dt == null)
                    {
                        DataTable dtTemp = new DataTable();
                        dtTemp.Columns.Add("Código", typeof(string));
                        dtTemp.Columns.Add("Mensaje", typeof(string));
                        dtTemp.Rows.Add("5", "Algún parametro no es correcto.");
                        return dtTemp;
                    }

                    if (dt.Rows.Count == 0)
                    {
                        DataTable dtTemp = new DataTable();
                        dtTemp.Columns.Add("Código", typeof(string));
                        dtTemp.Columns.Add("Mensaje", typeof(string));
                        dtTemp.Rows.Add("4", "La consulta no obtuvo registros.");
                        return dtTemp;
                    }
                }

                return dt;
            }
            catch (Exception e)
            {
                return ControlExepcionesNegocio(e);
            }
        }

        public static DataTable FncGetCursorIdSql(int idSQL)
        {
            #region Varables
            DataTable dt;
            SGL sgl;
            #endregion

            try
            {
                sgl = new SGL(pathDbConex);

                if (sgl.ExcepcionSGL != null)
                {
                    dt = new DataTable();
                    dt.Columns.Add("Código", typeof(string));
                    dt.Columns.Add("Mensaje", typeof(string));
                    dt.Columns.Add("Comentario", typeof(string));
                    dt.Columns.Add("Fecha", typeof(string));

                    dt.Rows.Add("1", sgl.ExcepcionSGL.Mensaje, sgl.ExcepcionSGL.Comentario, sgl.ExcepcionSGL.Fecha);
                    LogException.ErrorRegisterException(new Exception(sgl.ExcepcionSGL.Mensaje + " :: " + sgl.ExcepcionSGL.Comentario + " :: " + sgl.ExcepcionSGL.Fecha));
                }
                else
                {
                    dt = sgl.fncGetCursorIdSQL(idSQL);

                    if (sgl.ExcepcionSGL != null)
                    {
                        dt = new DataTable();
                        dt.Columns.Add("Código", typeof(string));
                        dt.Columns.Add("Mensaje", typeof(string));
                        dt.Columns.Add("Comentario", typeof(string));
                        dt.Columns.Add("Fecha", typeof(string));

                        dt.Rows.Add("2", sgl.ExcepcionSGL.Mensaje, sgl.ExcepcionSGL.Comentario, sgl.ExcepcionSGL.Fecha);
                        LogException.ErrorRegisterException(new Exception(sgl.ExcepcionSGL.Mensaje + " :: " + sgl.ExcepcionSGL.Comentario + " :: " + sgl.ExcepcionSGL.Fecha));
                    }

                    // La dll devuelve null cuando el tag no existe en bd y otras situaciones 
                    // que se implementaran en un futuro
                    if (dt == null)
                    {
                        DataTable dtTemp = new DataTable();
                        dtTemp.Columns.Add("Código", typeof(string));
                        dtTemp.Columns.Add("Mensaje", typeof(string));
                        dtTemp.Rows.Add("5", "Algún parametro no es correcto.");
                        return dtTemp;
                    }

                    if (dt.Rows.Count == 0)
                    {
                        DataTable dtTemp = new DataTable();
                        dtTemp.Columns.Add("Código", typeof(string));
                        dtTemp.Columns.Add("Mensaje", typeof(string));
                        dtTemp.Rows.Add("4", "La consulta no obtuvo registros.");
                        return dtTemp;
                    }
                }

                return dt;
            }
            catch (Exception e)
            {
                return ControlExepcionesNegocio(e);
            }
        }

        public static DataTable FncGetCursorTagParametros(Tags p)
        {
            #region Varables
            DataTable dt;
            SGL sgl;
            #endregion

            try
            {
                sgl = new SGL(pathDbConex);

                if (sgl.ExcepcionSGL != null)
                {
                    dt = new DataTable();
                    dt.Columns.Add("Código", typeof(string));
                    dt.Columns.Add("Mensaje", typeof(string));
                    dt.Columns.Add("Comentario", typeof(string));
                    dt.Columns.Add("Fecha", typeof(string));

                    dt.Rows.Add("1", sgl.ExcepcionSGL.Mensaje, sgl.ExcepcionSGL.Comentario, sgl.ExcepcionSGL.Fecha);
                    LogException.ErrorRegisterException(new Exception(sgl.ExcepcionSGL.Mensaje + " :: " + sgl.ExcepcionSGL.Comentario + " :: " + sgl.ExcepcionSGL.Fecha));
                }
                else
                {
                    dt = sgl.fncGetCursorTagParametros(p.Tag, p.Parametros, p.Separador);

                    if (sgl.ExcepcionSGL != null)
                    {
                        dt = new DataTable();
                        dt.Columns.Add("Código", typeof(string));
                        dt.Columns.Add("Mensaje", typeof(string));
                        dt.Columns.Add("Comentario", typeof(string));
                        dt.Columns.Add("Fecha", typeof(string));

                        dt.Rows.Add("2", sgl.ExcepcionSGL.Mensaje, sgl.ExcepcionSGL.Comentario, sgl.ExcepcionSGL.Fecha);
                        LogException.ErrorRegisterException(new Exception(sgl.ExcepcionSGL.Mensaje + " :: " + sgl.ExcepcionSGL.Comentario + " :: " + sgl.ExcepcionSGL.Fecha));
                    }

                    // La dll devuelve null cuando el tag no existe en bd y otras situaciones 
                    // que se implementaran en un futuro
                    if (dt == null)
                    {
                        DataTable dtTemp = new DataTable();
                        dtTemp.Columns.Add("Código", typeof(string));
                        dtTemp.Columns.Add("Mensaje", typeof(string));
                        dtTemp.Rows.Add("5", "Algún parametro no es correcto.");
                        return dtTemp;
                    }

                    if (dt.Rows.Count == 0)
                    {
                        DataTable dtTemp = new DataTable();
                        dtTemp.Columns.Add("Código", typeof(string));
                        dtTemp.Columns.Add("Mensaje", typeof(string));
                        dtTemp.Rows.Add("4", "La consulta no obtuvo registros.");
                        return dtTemp;
                    }
                }

                return dt;
            }
            catch (Exception e)
            {
                return ControlExepcionesNegocio(e);
            }
        }

        public static DataTable FncGetCursorIdSqlParametros(IdSqls p)
        {
            #region Varables
            DataTable dt;
            SGL sgl;
            #endregion

            try
            {
                sgl = new SGL(pathDbConex);

                if (sgl.ExcepcionSGL != null)
                {
                    dt = new DataTable();
                    dt.Columns.Add("Código", typeof(string));
                    dt.Columns.Add("Mensaje", typeof(string));
                    dt.Columns.Add("Comentario", typeof(string));
                    dt.Columns.Add("Fecha", typeof(string));

                    dt.Rows.Add("1", sgl.ExcepcionSGL.Mensaje, sgl.ExcepcionSGL.Comentario, sgl.ExcepcionSGL.Fecha);
                    LogException.ErrorRegisterException(new Exception(sgl.ExcepcionSGL.Mensaje + " :: " + sgl.ExcepcionSGL.Comentario + " :: " + sgl.ExcepcionSGL.Fecha));
                }
                else
                {
                    dt = sgl.fncGetCursorIdSQLParametros(p.IdSql, p.Parametros, p.Separador);

                    if (sgl.ExcepcionSGL != null)
                    {
                        dt = new DataTable();
                        dt.Columns.Add("Código", typeof(string));
                        dt.Columns.Add("Mensaje", typeof(string));
                        dt.Columns.Add("Comentario", typeof(string));
                        dt.Columns.Add("Fecha", typeof(string));

                        dt.Rows.Add("2", sgl.ExcepcionSGL.Mensaje, sgl.ExcepcionSGL.Comentario, sgl.ExcepcionSGL.Fecha);
                        LogException.ErrorRegisterException(new Exception(sgl.ExcepcionSGL.Mensaje + " :: " + sgl.ExcepcionSGL.Comentario + " :: " + sgl.ExcepcionSGL.Fecha));
                    }

                    // La dll devuelve null cuando el tag no existe en bd y otras situaciones 
                    // que se implementaran en un futuro
                    if (dt == null)
                    {
                        DataTable dtTemp = new DataTable();
                        dtTemp.Columns.Add("Código", typeof(string));
                        dtTemp.Columns.Add("Mensaje", typeof(string));
                        dtTemp.Rows.Add("5", "Algún parametro no es correcto.");
                        return dtTemp;
                    }

                    if (dt.Rows.Count == 0)
                    {
                        DataTable dtTemp = new DataTable();
                        dtTemp.Columns.Add("Código", typeof(string));
                        dtTemp.Columns.Add("Mensaje", typeof(string));
                        dtTemp.Rows.Add("4", "La consulta no obtuvo registros.");
                        return dtTemp;
                    }
                }

                return dt;
            }
            catch (Exception e)
            {
                return ControlExepcionesNegocio(e);
            }
        }
        #endregion

        #region Controlador Cargue Masivo
        public static DataTable EjecutarCm(ParametrosCM p)
        {
            #region Varables
            DataTable dt;
            SGL sgl;
            int idCargue = 0, lineas = 0, intProcesar = 0;
            #endregion

            try
            {
                sgl = new SGL(pathDbConex);
                idCargue = CrearCargueMasivo(p, sgl);

                if (idCargue > 0)
                {
                    lineas = CrearLineaCargueMasivo(idCargue, p.LstLineas, sgl);

                    if (lineas > 0)
                    {
                        intProcesar = ProcesarCargueMasivo(idCargue, sgl);

                        if (intProcesar > 0)
                        {
                            dt = GetCargueMasivo(idCargue, sgl);
                        }
                        else
                        {
                            dt = SetMensaje(intProcesar.ToString(), "Error 3. Falla al procesar el cargue masivo.");
                        }
                    }
                    else
                    {
                        dt = SetMensaje(lineas.ToString(), "Error 2. Falla cargando las líneas.");
                    }
                }
                else
                {
                    dt = SetMensaje(idCargue.ToString(), "Error 1. Falla al crear el cargue masivo.");
                }

                return dt;

            }
            catch (Exception e)
            {
                return ControlExepcionesNegocio(e);
            }
        }
        public static int CrearCargueMasivo(ParametrosCM p, SGL sgl)
        {
            return sgl.CrearCargueMasivo(p.IdTipoCM, p.CantidadLineas, p.Usuario);
        }
        public static int CrearLineaCargueMasivo(int idCargue, List<string> lst, SGL sgl)
        {
            return sgl.CrearLineaCargueMasivo(idCargue, lst);
        }
        private static int ProcesarCargueMasivo(int idCargue, SGL sgl)
        {
            return sgl.ProcesarCargueMasivo(idCargue);
        }
        private static DataTable GetCargueMasivo(int idCargue, SGL sgl)
        {
            return sgl.GetCargueMasivo(idCargue);
        }
        public static DataTable GetCargueMasivoTipo(ParametrosCMTipo p)
        {
            #region Varables
            DataTable res;
            SGL sgl;
            #endregion

            try
            {
                sgl = new SGL(pathDbConex);

                res = sgl.GetCargueMasivoTipo(p.IdTipoCM, p.IdCargue);

                return res;
            }
            catch (Exception e)
            {
                return ControlExepcionesNegocio(e);
            }
        }
        #endregion

        #region Controlador SGL

        #region SP DataTable
        public static DataTable FncStoreProcedureTagDt(ParametrosSpTag p)
        {
            #region Varables
            DataTable dt;
            SGL sgl;
            #endregion

            try
            {
                List<SodimacBD.NC.SQLParametro> lstParametros = ChangeToSQLParametro(p);
                sgl = new SGL(pathDbConex);

                if (sgl.ExcepcionSGL != null)
                {
                    dt = new DataTable();
                    dt.Columns.Add("Código", typeof(string));
                    dt.Columns.Add("Mensaje", typeof(string));
                    dt.Columns.Add("Comentario", typeof(string));
                    dt.Columns.Add("Fecha", typeof(string));

                    dt.Rows.Add("1", sgl.ExcepcionSGL.Mensaje, sgl.ExcepcionSGL.Comentario, sgl.ExcepcionSGL.Fecha);
                    LogException.ErrorRegisterException(new Exception(sgl.ExcepcionSGL.Mensaje + " :: " + sgl.ExcepcionSGL.Comentario + " :: " + sgl.ExcepcionSGL.Fecha));
                }
                else
                {                    
                    dt = (DataTable)sgl.StoreProcedureTag(p.Tag, p.Procedimiento, lstParametros);

                    if (sgl.ExcepcionSGL != null)
                    {
                        dt = new DataTable();
                        dt.Columns.Add("Código", typeof(string));
                        dt.Columns.Add("Mensaje", typeof(string));
                        dt.Columns.Add("Comentario", typeof(string));
                        dt.Columns.Add("Fecha", typeof(string));

                        dt.Rows.Add("2", sgl.ExcepcionSGL.Mensaje, sgl.ExcepcionSGL.Comentario, sgl.ExcepcionSGL.Fecha);
                        LogException.ErrorRegisterException(new Exception(sgl.ExcepcionSGL.Mensaje + " :: " + sgl.ExcepcionSGL.Comentario + " :: " + sgl.ExcepcionSGL.Fecha));
                    }

                    // La dll devuelve null cuando el tag no existe en bd y otras situaciones 
                    // que se implementaran en un futuro
                    if (dt == null)
                    {
                        DataTable dtTemp = new DataTable();
                        dtTemp.Columns.Add("Código", typeof(string));
                        dtTemp.Columns.Add("Mensaje", typeof(string));
                        dtTemp.Rows.Add("5", "Algún parametro no es correcto.");
                        return dtTemp;
                    }

                    if (dt.Rows.Count == 0)
                    {
                        DataTable dtTemp = new DataTable();
                        dtTemp.Columns.Add("Código", typeof(string));
                        dtTemp.Columns.Add("Mensaje", typeof(string));
                        dtTemp.Rows.Add("4", "La consulta no obtuvo registros.");
                        return dtTemp;
                    }
                }

                return dt;
            }
            catch (Exception e)
            {
                return ControlExepcionesNegocio(e);
            }
        }

        public static DataTable ProFncStoreProcedureTagDt(string parametros)
        {
            #region Varables
            DataTable dt;
            SGL sgl;
            #endregion

            try
            {
                string dataDecrypted = DeCryptFncStoreProcedureTagDt(parametros);
                var p = JsonConvert.DeserializeObject<ParametrosSpTag>(dataDecrypted);
                string json = JsonConvert.SerializeObject(p, Formatting.Indented);

                //Se guarda la ejecución de la sentencia
                SetEjecucionInternet(json);


                List<SodimacBD.NC.SQLParametro> lstParametros = ChangeToSQLParametro(p);
                sgl = new SGL(pathDbConex);

                if (sgl.ExcepcionSGL != null)
                {
                    dt = new DataTable();
                    dt.Columns.Add("Código", typeof(string));
                    dt.Columns.Add("Mensaje", typeof(string));
                    dt.Columns.Add("Comentario", typeof(string));
                    dt.Columns.Add("Fecha", typeof(string));

                    dt.Rows.Add("1", sgl.ExcepcionSGL.Mensaje, sgl.ExcepcionSGL.Comentario, sgl.ExcepcionSGL.Fecha);
                    LogException.ErrorRegisterException(new Exception(sgl.ExcepcionSGL.Mensaje + " :: " + sgl.ExcepcionSGL.Comentario + " :: " + sgl.ExcepcionSGL.Fecha));
                }
                else
                {
                    dt = (DataTable)sgl.StoreProcedureTag(p.Tag, p.Procedimiento, lstParametros);

                    if (sgl.ExcepcionSGL != null)
                    {
                        dt = new DataTable();
                        dt.Columns.Add("Código", typeof(string));
                        dt.Columns.Add("Mensaje", typeof(string));
                        dt.Columns.Add("Comentario", typeof(string));
                        dt.Columns.Add("Fecha", typeof(string));

                        dt.Rows.Add("2", sgl.ExcepcionSGL.Mensaje, sgl.ExcepcionSGL.Comentario, sgl.ExcepcionSGL.Fecha);
                        LogException.ErrorRegisterException(new Exception(sgl.ExcepcionSGL.Mensaje + " :: " + sgl.ExcepcionSGL.Comentario + " :: " + sgl.ExcepcionSGL.Fecha));
                    }

                    // La dll devuelve null cuando el tag no existe en bd y otras situaciones 
                    // que se implementaran en un futuro
                    if (dt == null)
                    {
                        DataTable dtTemp = new DataTable();
                        dtTemp.Columns.Add("Código", typeof(string));
                        dtTemp.Columns.Add("Mensaje", typeof(string));
                        dtTemp.Rows.Add("5", "Algún parametro no es correcto.");
                        return dtTemp;
                    }

                    if (dt.Rows.Count == 0)
                    {
                        DataTable dtTemp = new DataTable();
                        dtTemp.Columns.Add("Código", typeof(string));
                        dtTemp.Columns.Add("Mensaje", typeof(string));
                        dtTemp.Rows.Add("4", "La consulta no obtuvo registros.");
                        return dtTemp;
                    }
                }

                return dt;
            }
            catch (Exception e)
            {
                return ControlExepcionesNegocio(e);
            }
        }
        public static DataTable FncStoreProcedureIdSqlDt(ParametrosSpIdSql p)
        {
            #region Varables
            DataTable dt;
            SGL sgl;
            #endregion

            try
            {
                List<SodimacBD.NC.SQLParametro> lstParametros = ChangeToSQLParametro(p);
                sgl = new SGL(pathDbConex);

                if (sgl.ExcepcionSGL != null)
                {
                    dt = new DataTable();
                    dt.Columns.Add("Código", typeof(string));
                    dt.Columns.Add("Mensaje", typeof(string));
                    dt.Columns.Add("Comentario", typeof(string));
                    dt.Columns.Add("Fecha", typeof(string));

                    dt.Rows.Add("1", sgl.ExcepcionSGL.Mensaje, sgl.ExcepcionSGL.Comentario, sgl.ExcepcionSGL.Fecha);
                    LogException.ErrorRegisterException(new Exception(sgl.ExcepcionSGL.Mensaje + " :: " + sgl.ExcepcionSGL.Comentario + " :: " + sgl.ExcepcionSGL.Fecha));
                }
                else
                {
                    dt = (DataTable)sgl.StoreProcedureIdSQL(p.IdSql, p.Procedimiento, lstParametros);

                    if (sgl.ExcepcionSGL != null)
                    {
                        dt = new DataTable();
                        dt.Columns.Add("Código", typeof(string));
                        dt.Columns.Add("Mensaje", typeof(string));
                        dt.Columns.Add("Comentario", typeof(string));
                        dt.Columns.Add("Fecha", typeof(string));

                        dt.Rows.Add("2", sgl.ExcepcionSGL.Mensaje, sgl.ExcepcionSGL.Comentario, sgl.ExcepcionSGL.Fecha);
                        LogException.ErrorRegisterException(new Exception(sgl.ExcepcionSGL.Mensaje + " :: " + sgl.ExcepcionSGL.Comentario + " :: " + sgl.ExcepcionSGL.Fecha));
                    }

                    // La dll devuelve null cuando el tag no existe en bd y otras situaciones 
                    // que se implementaran en un futuro
                    if (dt == null)
                    {
                        DataTable dtTemp = new DataTable();
                        dtTemp.Columns.Add("Código", typeof(string));
                        dtTemp.Columns.Add("Mensaje", typeof(string));
                        dtTemp.Rows.Add("5", "Algún parametro no es correcto.");
                        return dtTemp;
                    }

                    if (dt.Rows.Count == 0)
                    {
                        DataTable dtTemp = new DataTable();
                        dtTemp.Columns.Add("Código", typeof(string));
                        dtTemp.Columns.Add("Mensaje", typeof(string));
                        dtTemp.Rows.Add("4", "La consulta no obtuvo registros.");
                        return dtTemp;
                    }
                }

                return dt;
            }
            catch (Exception e)
            {
                return ControlExepcionesNegocio(e);
            }
        }
        public static DataTable FncStoreProcedureSglDt(ParametrosSP p)
        {
            #region Varables
            DataTable dt;
            SGL sgl;
            #endregion

            try
            {
                List<SodimacBD.NC.SQLParametro> lstParametros = ChangeToSQLParametro(p);
                sgl = new SGL(pathDbConex);

                if (sgl.ExcepcionSGL != null)
                {
                    dt = new DataTable();
                    dt.Columns.Add("Código", typeof(string));
                    dt.Columns.Add("Mensaje", typeof(string));
                    dt.Columns.Add("Comentario", typeof(string));
                    dt.Columns.Add("Fecha", typeof(string));

                    dt.Rows.Add("1", sgl.ExcepcionSGL.Mensaje, sgl.ExcepcionSGL.Comentario, sgl.ExcepcionSGL.Fecha);
                    LogException.ErrorRegisterException(new Exception(sgl.ExcepcionSGL.Mensaje + " :: " + sgl.ExcepcionSGL.Comentario + " :: " + sgl.ExcepcionSGL.Fecha));
                }
                else
                {
                    dt = (DataTable)sgl.StoreProcedureSGL(p.Procedimiento, lstParametros);

                    if (sgl.ExcepcionSGL != null)
                    {
                        dt = new DataTable();
                        dt.Columns.Add("Código", typeof(string));
                        dt.Columns.Add("Mensaje", typeof(string));
                        dt.Columns.Add("Comentario", typeof(string));
                        dt.Columns.Add("Fecha", typeof(string));

                        dt.Rows.Add("2", sgl.ExcepcionSGL.Mensaje, sgl.ExcepcionSGL.Comentario, sgl.ExcepcionSGL.Fecha);
                        LogException.ErrorRegisterException(new Exception(sgl.ExcepcionSGL.Mensaje + " :: " + sgl.ExcepcionSGL.Comentario + " :: " + sgl.ExcepcionSGL.Fecha));
                    }

                    // La dll devuelve null cuando el tag no existe en bd y otras situaciones 
                    // que se implementaran en un futuro
                    if (dt == null)
                    {
                        DataTable dtTemp = new DataTable();
                        dtTemp.Columns.Add("Código", typeof(string));
                        dtTemp.Columns.Add("Mensaje", typeof(string));
                        dtTemp.Rows.Add("5", "Algún parametro no es correcto.");
                        return dtTemp;
                    }

                    if (dt.Rows.Count == 0)
                    {
                        DataTable dtTemp = new DataTable();
                        dtTemp.Columns.Add("Código", typeof(string));
                        dtTemp.Columns.Add("Mensaje", typeof(string));
                        dtTemp.Rows.Add("4", "La consulta no obtuvo registros.");
                        return dtTemp;
                    }
                }

                return dt;
            }
            catch (Exception e)
            {
                return ControlExepcionesNegocio(e);
            }
        }
        #endregion

        #region SP Especificos: Str, Int32 y Ds
        public static string FncStoreProcedureStr(ParametrosSP p)
        {
            #region Varables
            string res;
            SGL sgl;
            #endregion

            try
            {
                List<SodimacBD.NC.SQLParametro> lstParametros = ChangeToSQLParametro(p);

                sgl = new SGL(pathDbConex);

                res = sgl.StoreProcedureSGL(p.Procedimiento, lstParametros).ToString();

                return res;
            }
            catch (Exception e)
            {
                Exception ex = new Exception(e.Message + " :: " + e.StackTrace + " :: " + DateTime.Now.ToShortDateString());
                throw ex;
            }
        }
        public static Int32 FncStoreProcedureInt32(ParametrosSP p)
        {
            #region Varables
            Int32 res = -1;
            SGL sgl;
            #endregion

            try
            {
                List<SodimacBD.NC.SQLParametro> lstParametros = ChangeToSQLParametro(p);

                sgl = new SGL(pathDbConex);

                if (Int32.TryParse(sgl.StoreProcedureSGL(p.Procedimiento, lstParametros).ToString(), out res))
                {
                    return res;
                }

                return -1;
            }
            catch (Exception e)
            {
                Exception ex = new Exception(e.Message + " :: " + e.StackTrace + " :: " + DateTime.Now.ToShortDateString());
                throw ex;
            }
        }
        public static DataSet FncStoreProcedureDs(ParametrosSP p)
        {
            #region Varables
            DataSet ds;
            DataTable dt;
            SGL sgl;
            #endregion

            try
            {
                List<SodimacBD.NC.SQLParametro> lstParametros = ChangeToSQLParametro(p);
                sgl = new SGL(pathDbConex);

                if (sgl.ExcepcionSGL != null)
                {
                    ds = new DataSet();
                    dt = new DataTable();
                    dt.Columns.Add("Código", typeof(string));
                    dt.Columns.Add("Mensaje", typeof(string));
                    dt.Columns.Add("Comentario", typeof(string));
                    dt.Columns.Add("Fecha", typeof(string));

                    dt.Rows.Add("1", sgl.ExcepcionSGL.Mensaje, sgl.ExcepcionSGL.Comentario, sgl.ExcepcionSGL.Fecha);
                    LogException.ErrorRegisterException(new Exception(sgl.ExcepcionSGL.Mensaje + " :: " + sgl.ExcepcionSGL.Comentario + " :: " + sgl.ExcepcionSGL.Fecha));

                    ds.Tables.Add(dt);
                }
                else
                {
                    ds = (DataSet)sgl.StoreProcedureSGL(p.Procedimiento, lstParametros);

                    if (sgl.ExcepcionSGL != null)
                    {
                        dt = new DataTable();
                        dt.Columns.Add("Código", typeof(string));
                        dt.Columns.Add("Mensaje", typeof(string));
                        dt.Columns.Add("Comentario", typeof(string));
                        dt.Columns.Add("Fecha", typeof(string));

                        dt.Rows.Add("2", sgl.ExcepcionSGL.Mensaje, sgl.ExcepcionSGL.Comentario, sgl.ExcepcionSGL.Fecha);
                        LogException.ErrorRegisterException(new Exception(sgl.ExcepcionSGL.Mensaje + " :: " + sgl.ExcepcionSGL.Comentario + " :: " + sgl.ExcepcionSGL.Fecha));
                    }

                    if (ds.Tables.Count == 0)
                    {
                        dt = new DataTable();
                        dt.Columns.Add("Código", typeof(string));
                        dt.Columns.Add("Mensaje", typeof(string));
                        dt.Rows.Add("4", "La consulta no obtuvo registros.");

                        ds.Tables.Add(dt);

                        return ds;
                    }
                }

                return ds;
            }
            catch (Exception e)
            {
                ds = new DataSet();
                ds.Tables.Add(ControlExepcionesNegocio(e));
                return ds;
            }
        }
        #endregion

        #region SGL Controller Metodos especificos

        #region Tag
        public static Int32 FncStoreProcedureTagInt32(ParametrosSpTag p)
        {
            #region Varables
            Int32 res = -1;
            SGL sgl;
            #endregion

            try
            {
                List<SodimacBD.NC.SQLParametro> lstParametros = ChangeToSQLParametro(p);
                sgl = new SGL(pathDbConex);

                if (sgl.ExcepcionSGL != null)
                {
                    LogException.ErrorRegisterException(new Exception(sgl.ExcepcionSGL.Mensaje + " :: " + sgl.ExcepcionSGL.Comentario + " :: " + sgl.ExcepcionSGL.Fecha));
                }
                else
                {
                    var data = sgl.StoreProcedureTag(p.Tag, p.Procedimiento, lstParametros).ToString();

                    if (sgl.ExcepcionSGL != null)
                    {
                        LogException.ErrorRegisterException(new Exception(sgl.ExcepcionSGL.Mensaje + " :: " + sgl.ExcepcionSGL.Comentario + " :: " + sgl.ExcepcionSGL.Fecha));
                    }

                    if (Int32.TryParse(data, out res))
                    {
                        return res;
                    }
                }
                return res;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static Int32 ProFncStoreProcedureTagInt32(string parametros)
        {
            #region Varables
            Int32 res = -1;
            SGL sgl;
            #endregion

            try
            {
                string dataDecrypted = DeCryptFncStoreProcedureTagDt(parametros);
                var p = JsonConvert.DeserializeObject<ParametrosSpTag>(dataDecrypted);
                string json = JsonConvert.SerializeObject(p, Formatting.Indented);

                //Se guarda la ejecución de la sentencia
                SetEjecucionInternet(json);

                List<SodimacBD.NC.SQLParametro> lstParametros = ChangeToSQLParametro(p);
                sgl = new SGL(pathDbConex);

                if (sgl.ExcepcionSGL != null)
                {
                    LogException.ErrorRegisterException(new Exception(sgl.ExcepcionSGL.Mensaje + " :: " + sgl.ExcepcionSGL.Comentario + " :: " + sgl.ExcepcionSGL.Fecha));
                }
                else
                {
                    var data = sgl.StoreProcedureTag(p.Tag, p.Procedimiento, lstParametros).ToString();

                    if (sgl.ExcepcionSGL != null)
                    {
                        LogException.ErrorRegisterException(new Exception(sgl.ExcepcionSGL.Mensaje + " :: " + sgl.ExcepcionSGL.Comentario + " :: " + sgl.ExcepcionSGL.Fecha));
                    }

                    if (Int32.TryParse(data, out res))
                    {
                        return res;
                    }
                }
                return res;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public static string FncStoreProcedureTagStr(ParametrosSpTag p)
        {
            #region Varables
            string res = string.Empty;
            SGL sgl;
            #endregion

            try
            {
                List<SodimacBD.NC.SQLParametro> lstParametros = ChangeToSQLParametro(p);
                sgl = new SGL(pathDbConex);

                if (sgl.ExcepcionSGL != null)
                {
                    LogException.ErrorRegisterException(new Exception(sgl.ExcepcionSGL.Mensaje + " :: " + sgl.ExcepcionSGL.Comentario + " :: " + sgl.ExcepcionSGL.Fecha));
                }
                else
                {
                    res = sgl.StoreProcedureTag(p.Tag, p.Procedimiento, lstParametros).ToString();

                    if (sgl.ExcepcionSGL != null)
                    {
                        LogException.ErrorRegisterException(new Exception(sgl.ExcepcionSGL.Mensaje + " :: " + sgl.ExcepcionSGL.Comentario + " :: " + sgl.ExcepcionSGL.Fecha));
                    }

                }

                return res;
            }
            catch (Exception e)
            {
                throw e;
            }
        }


        public static string ProFncStoreProcedureTagStr(string parametros)//ParametrosSpTag
        {
            #region Varables
            string res = string.Empty;
            SGL sgl;
            #endregion

            try
            {
                string dataDecrypted = DeCryptFncStoreProcedureTagDt(parametros);
                var p = JsonConvert.DeserializeObject<ParametrosSpTag>(dataDecrypted);
                string json = JsonConvert.SerializeObject(p, Formatting.Indented);

                //Se guarda la ejecución de la sentencia
                SetEjecucionInternet(json);

                List<SodimacBD.NC.SQLParametro> lstParametros = ChangeToSQLParametro(p);
                sgl = new SGL(pathDbConex);

                if (sgl.ExcepcionSGL != null)
                {
                    LogException.ErrorRegisterException(new Exception(sgl.ExcepcionSGL.Mensaje + " :: " + sgl.ExcepcionSGL.Comentario + " :: " + sgl.ExcepcionSGL.Fecha));
                }
                else
                {
                    res = sgl.StoreProcedureTag(p.Tag, p.Procedimiento, lstParametros).ToString();

                    if (sgl.ExcepcionSGL != null)
                    {
                        LogException.ErrorRegisterException(new Exception(sgl.ExcepcionSGL.Mensaje + " :: " + sgl.ExcepcionSGL.Comentario + " :: " + sgl.ExcepcionSGL.Fecha));
                    }

                }

                return res;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        #endregion


        #region IdSql
        public static Int32 FncStoreProcedureIdSqlInt32(ParametrosSpIdSql p)
        {
            #region Varables
            Int32 res = -1;
            SGL sgl;
            #endregion

            try
            {
                List<SodimacBD.NC.SQLParametro> lstParametros = ChangeToSQLParametro(p);
                sgl = new SGL(pathDbConex);

                if (sgl.ExcepcionSGL != null)
                {
                    LogException.ErrorRegisterException(new Exception(sgl.ExcepcionSGL.Mensaje + " :: " + sgl.ExcepcionSGL.Comentario + " :: " + sgl.ExcepcionSGL.Fecha));
                }
                else
                {
                    var data = sgl.StoreProcedureIdSQL(p.IdSql, p.Procedimiento, lstParametros).ToString();

                    if (sgl.ExcepcionSGL != null)
                    {
                        LogException.ErrorRegisterException(new Exception(sgl.ExcepcionSGL.Mensaje + " :: " + sgl.ExcepcionSGL.Comentario + " :: " + sgl.ExcepcionSGL.Fecha));
                    }

                    if (Int32.TryParse(data, out res))
                    {
                        return res;
                    }

                }

                return res;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public static string FncStoreProcedureIdSqlStr(ParametrosSpIdSql p)
        {
            #region Varables
            string res = "-1";
            SGL sgl;
            #endregion

            try
            {
                List<SodimacBD.NC.SQLParametro> lstParametros = ChangeToSQLParametro(p);
                sgl = new SGL(pathDbConex);

                if (sgl.ExcepcionSGL != null)
                {
                    LogException.ErrorRegisterException(new Exception(sgl.ExcepcionSGL.Mensaje + " :: " + sgl.ExcepcionSGL.Comentario + " :: " + sgl.ExcepcionSGL.Fecha));
                }
                else
                {
                    res = sgl.StoreProcedureIdSQL(p.IdSql, p.Procedimiento, lstParametros).ToString();

                    if (sgl.ExcepcionSGL != null)
                    {
                        LogException.ErrorRegisterException(new Exception(sgl.ExcepcionSGL.Mensaje + " :: " + sgl.ExcepcionSGL.Comentario + " :: " + sgl.ExcepcionSGL.Fecha));
                    }

                }

                return res;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion
        #endregion

        private static List<SodimacBD.NC.SQLParametro> ChangeToSQLParametro(ParametrosSP p)
        {
            List<SodimacBD.NC.SQLParametro> lst = new List<SodimacBD.NC.SQLParametro>();
            SodimacBD.NC.SQLParametro sql;

            foreach (var item in p.Parametros)
            {
                sql = new SQLParametro
                {
                    Nombre = item.Nombre,
                    intValor = item.IntValor,
                    douValor = item.DouValor,
                    stringValor = item.StringValor,
                    DateValor = item.DateValor,
                    Entrada = item.Entrada
                };

                switch (item.Tipo)
                {
                    case "i":
                    case "I":
                        sql.Tipo = OracleDbType.Int32;
                        break;
                    case "d":
                    case "D":
                        sql.Tipo = OracleDbType.Double;
                        break;
                    case "f":
                    case "F":
                        sql.Tipo = OracleDbType.Decimal;
                        break;
                    case "s":
                    case "S":
                        sql.Tipo = OracleDbType.Varchar2;
                        break;
                    case "c":
                    case "C":
                        sql.Tipo = OracleDbType.RefCursor;
                        break;
                    case "z":
                    case "Z":
                        sql.Tipo = OracleDbType.Clob;
                        break;
                    case "dt":
                    case "DT":
                    case "dT":
                    case "Dt":
                        sql.Tipo = OracleDbType.TimeStamp;
                        break;
                }
                lst.Add(sql);
            }
            return lst;
        }
        #endregion

        #region Security
        public static void SetEjecucionInternet(string sentencia)
        {
            #region Varables
            SGL sgl;
            #endregion

            try
            {
                sgl = new SGL(pathDbConex);
                List<SodimacBD.NC.SQLParametro> lstParametros = new List<SQLParametro>
                {
                    new SQLParametro { Nombre = "P_ORIGEN", stringValor = "AD_INTERNET", Tipo = OracleDbType.Varchar2 },
                    new SQLParametro { Nombre = "P_SENTENCIA", stringValor = sentencia, Tipo = OracleDbType.Varchar2 }
                };

                if (sgl.ExcepcionSGL != null)
                {
                    LogException.ErrorRegisterException(new Exception(sgl.ExcepcionSGL.Mensaje + " :: " + sgl.ExcepcionSGL.Comentario + " :: " + sgl.ExcepcionSGL.Fecha));
                    LogException.ErrorRegisterException(new Exception("Error ejecutando sentencia:: " + sentencia));
                }
                else
                {
                    sgl.StoreProcedureSGL(spSetEjecucion, lstParametros);

                    if (sgl.ExcepcionSGL != null)
                    {
                        LogException.ErrorRegisterException(new Exception(sgl.ExcepcionSGL.Mensaje + " :: " + sgl.ExcepcionSGL.Comentario + " :: " + sgl.ExcepcionSGL.Fecha));
                        LogException.ErrorRegisterException(new Exception("Error ejecutando sentencia:: "));
                    }
                }
            }
            catch (Exception e)
            {
                LogException.ErrorRegisterException(new Exception("Error ejecutando sentencia:: " + sentencia + "::" + sentencia + DateTime.Now.ToString()));
            }
        }
        #endregion

        #region Des-encripción
        private static string DeCryptFncStoreProcedureTagDt(string parametros)
        {
            string data = Base64Decode(parametros);
            data = data.Replace("\"\\n  ", "");
            data = data.Replace(@"\\n\\n\\n  \", "");
            data = data.Replace(@"\\n\\n\\n  \", "");
            data = data.Replace("\\n  ", "");
            data = data.Replace("\\", "");
            data = data.Replace("nn", "");
            data = data.Replace("\"", @"'");
            data = data.Replace("}  ]}'", @" }]}");

            return data;
        }

        private static string Base64Decode(string plainText)
        {
            string base64Decoded;
            byte[] data = Convert.FromBase64String(plainText);
            base64Decoded = Encoding.ASCII.GetString(data);
            return Crypto.Crypto.OpenSSLDecrypt(base64Decoded);
        }

        #endregion
    }
}
