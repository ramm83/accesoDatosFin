using AccesoDatos.Entidades;
using AccesoDatos.Log;
using AccesoDatos.Negocio;
using AccesoDatos.Validator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Data;


namespace AccesoDatos.Controllers
{
    /// <summary>
    /// Controlador de Acceso a datos
    /// </summary>
    [ApiController]
    [Route("api/SGL")]
#pragma warning disable S101 // Types should be named in PascalCase
    public class SGLController : ControllerBase
#pragma warning restore S101 // Types should be named in PascalCase
    {
        private readonly string controldor = "AccesoDatos - SGL";

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("PrcSetLogError")]
        public ActionResult PrcSetLogError([FromBody] MensajeError me)
        {
            Mensajes m = new Mensajes();

            try
            {
                AccesoDatosBll.RegistrarError(me);
                m.Estado = true;
                m.Mensaje = "OK";
                m.Value = true;
                return Ok(m);
            }
            catch (Exception ex)
            {
                LogException.ErrorRegisterException(new Exception(AccesoDatosBll.ControlErrores(ex, controldor + " :: PrcSetLogError")));
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

        }

        /// <summary>
        /// Ejecuta un procedimiento almacenado con Tag.
        /// </summary>
        /// <param name="p">Homologación de tipos de parametros:
        /// Int32 = "i" o "I", Float = "d" o "D" "f" o "F", VarChar "s" o "S",
        /// Cursor = "c" o "C", Clob = "z" o "Z", Date = "dt" o "DT" "Dt" o "dT"</param>
        /// <returns>DataTable</returns>
        [HttpPost]
        [Route("FncStoreProcedureTagDt")]
        public ActionResult FncStoreProcedureTagDt([FromBody] ParametrosSpTag p)
        {
            DataTable res;
            Mensajes m;

            try
            {
                m = new Mensajes();
                
                res = AccesoDatosBll.FncStoreProcedureTagDt(p);

                if (res.Columns.Count > 1)
                {
                    if (res.Columns[0].ColumnName + "-" + res.Columns[1].ColumnName == "Código-Mensaje")
                    {
                        // Excepción Conexiones
                        // Excepción Base de datos
                        // Excepción Metodo en la capa de negocio

                        m.Estado = false;

                        // 4. Consulta ejecutada pero sin datos de devolución
                        if (res.Rows[0].ItemArray.GetValue(0).ToString() == "4")
                        {
                            m.Mensaje = "La sentencia se ejecuto pero no obtuvo registros.";
                        }
                        else
                        {
                            m.Mensaje = res.Rows[0].ItemArray.GetValue(0).ToString();
                        }
                    }
                    else
                    {
                        m.Estado = true;
                        m.Mensaje = "Sentencia ejecutada con éxito";
                    }
                }
                else
                {
                    if (res.Rows[0].ItemArray.GetValue(0).ToString() == "4")
                    {
                        m.Mensaje = "La sentencia se ejecuto pero no obtuvo registros.";
                    }
                    else
                    {
                        m.Estado = true;
                        m.Mensaje = "Sentencia ejecutada con éxito";
                    }
                }                

                m.Value = res;

                return Ok(m);
            }
            catch (Exception ex)
            {
                LogException.ErrorRegisterException(new Exception(AccesoDatosBll.ControlErrores(ex, controldor + " :: FncStoreProcedureTagDt")));
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Ejecuta un procedimiento almacenado con Tag.
        /// </summary>
        /// <param name="p">Homologación de tipos de parametros:
        /// Int32 = "i" o "I", Float = "d" o "D" "f" o "F", VarChar "s" o "S",
        /// Cursor = "c" o "C", Clob = "z" o "Z", Date = "dt" o "DT" "Dt" o "dT"</param>
        /// <returns>Int32</returns>
        [HttpPost]
        [Route("FncStoreProcedureTagInt32")]
        public ActionResult FncStoreProcedureTagInt32([FromBody] ParametrosSpTag p)
        {
            Int32 res;
            Mensajes m;

            try
            {
                m = new Mensajes();

                res = AccesoDatosBll.FncStoreProcedureTagInt32(p);

                if (res == -1)
                {
                    m.Estado = false;
                    m.Mensaje = "Resultado de la sentencia fallido.";
                    m.Value = -1;
                }
                else
                {
                    m.Estado = true;
                    m.Mensaje = "Sentencia ejecutada con éxito.";
                    m.Value = res;
                }

                return Ok(m);

            }
            catch (Exception ex)
            {
                LogException.ErrorRegisterException(new Exception(AccesoDatosBll.ControlErrores(ex, controldor + " :: FncStoreProcedureTagInt32")));
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Ejecuta un procedimiento almacenado con Tag.
        /// </summary>
        /// <param name="p">Homologación de tipos de parametros:
        /// Int32 = "i" o "I", Float = "d" o "D" "f" o "F", VarChar "s" o "S",
        /// Cursor = "c" o "C", Clob = "z" o "Z", Date = "dt" o "DT" "Dt" o "dT"</param>
        /// <returns>string</returns>
        [HttpPost]
        [Route("FncStoreProcedureTagStr")]
        public ActionResult FncStoreProcedureTagStr([FromBody] ParametrosSpTag p)
        {
            string res;
            Mensajes m;

            try
            {
                m = new Mensajes();

                res = AccesoDatosBll.FncStoreProcedureTagStr(p);

                if (res == string.Empty)
                {
                    m.Estado = false;
                    m.Mensaje = "Error al realizar la consulta";
                    m.Value = "";
                }
                else
                {
                    m.Estado = true;
                    m.Mensaje = "Sentencia ejecutada con éxito.";
                    m.Value = res;
                }

            return Ok(m);

            }
            catch (Exception ex)
            {
                LogException.ErrorRegisterException(new Exception(AccesoDatosBll.ControlErrores(ex, controldor + " :: FncStoreProcedureTagStr")));
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }



        /// <summary>
        /// Ejecuta un procedimiento almacenado con IdSql. 
        /// </summary>
        /// <param name="p">Homologación de tipos de parametros:
        /// Int32 = "i" o "I", Float = "d" o "D" "f" o "F", VarChar "s" o "S",
        /// Cursor = "c" o "C", Clob = "z" o "Z"</param>
        /// <returns>DataTable</returns>
        [HttpPost]
        [Route("FncStoreProcedureIdSqlDt")]
        public ActionResult FncStoreProcedureIdSqlDt([FromBody] ParametrosSpIdSql p)
        {
            DataTable res;
            Mensajes m;

            try
            {
                m = new Mensajes();
                res = AccesoDatosBll.FncStoreProcedureIdSqlDt(p);

                if (res.Columns[0].ColumnName + "-" + res.Columns[1].ColumnName == "Código-Mensaje")
                {
                    // Excepción Conexiones
                    // Excepción Base de datos
                    // Excepción Metodo en la capa de negocio

                    m.Estado = false;

                    // 4. Consulta ejecutada pero sin datos de devolución
                    if (res.Rows[0].ItemArray.GetValue(0).ToString() == "4")
                    {
                        m.Mensaje = "La sentencia se ejecuto pero no obtuvo registros.";
                    }
                    else
                    {
                        m.Mensaje = res.Rows[0].ItemArray.GetValue(0).ToString();
                    }
                }
                else
                {
                    m.Estado = true;
                    m.Mensaje = "Sentencia ejecutada con éxito";
                }

                m.Value = res;

                return Ok(m);
            }
            catch (Exception ex)
            {
                LogException.ErrorRegisterException(new Exception(AccesoDatosBll.ControlErrores(ex, controldor + " :: FncStoreProcedureIdSqlDt")));
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }


        /// <summary>
        /// Ejecuta un procedimiento almacenado con IdSql.
        /// </summary>
        /// <param name="p">Homologación de tipos de parametros:
        /// Int32 = "i" o "I", Float = "d" o "D" "f" o "F", VarChar "s" o "S",
        /// Cursor = "c" o "C", Clob = "z" o "Z", Date = "dt" o "DT" "Dt" o "dT"</param>
        /// <returns>Int32</returns>
        [HttpPost]
        [Route("FncStoreProcedureIdSqlInt32")]
        public ActionResult FncStoreProcedureIdSqlInt32([FromBody] ParametrosSpIdSql p)
        {
            Int32 res;
            Mensajes m;

            try
            {
                m = new Mensajes();

                res = AccesoDatosBll.FncStoreProcedureIdSqlInt32(p);

                if (res == -1)
                {
                    m.Estado = false;
                    m.Mensaje = "Resultado de la sentencia fallido.";
                    m.Value = -1;
                }
                else
                {
                    m.Estado = true;
                    m.Mensaje = "Sentencia ejecutada con éxito.";
                    m.Value = res;
                }

                return Ok(m);

            }
            catch (Exception ex)
            {
                LogException.ErrorRegisterException(new Exception(AccesoDatosBll.ControlErrores(ex, controldor + " :: FncStoreProcedureTagInt32")));
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Ejecuta un procedimiento almacenado con IdSql.
        /// </summary>
        /// <param name="p">Homologación de tipos de parametros:
        /// Int32 = "i" o "I", Float = "d" o "D" "f" o "F", VarChar "s" o "S",
        /// Cursor = "c" o "C", Clob = "z" o "Z", Date = "dt" o "DT" "Dt" o "dT"</param>
        /// <returns>string</returns>
        [HttpPost]
        [Route("FncStoreProcedureIdSqlStr")]
        public ActionResult FncStoreProcedureIdSqlStr([FromBody] ParametrosSpIdSql p)
        {
            string res;
            Mensajes m;

            try
            {
                m = new Mensajes();

                res = AccesoDatosBll.FncStoreProcedureIdSqlStr(p);

                if (res == string.Empty)
                {
                    m.Estado = false;
                    m.Mensaje = "Error al realizar la consulta";
                    m.Value = "";
                }
                else
                {
                    m.Estado = true;
                    m.Mensaje = "Sentencia ejecutada con éxito.";
                    m.Value = res;
                }

                return Ok(m);

            }
            catch (Exception ex)
            {
                LogException.ErrorRegisterException(new Exception(AccesoDatosBll.ControlErrores(ex, controldor + " :: FncStoreProcedureTagStr")));
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Ejecuta un procedimiento almacenado en SGL.
        /// </summary>
        /// <param name="p">Homologación de tipos de parametros:
        /// Int32 = "i" o "I", Float = "d" o "D" "f" o "F", VarChar "s" o "S",
        /// Cursor = "c" o "C", Clob = "z" o "Z"</param>
        /// <returns>DataTable</returns>
        [HttpPost]
        [Route("FncStoreProcedureSglDt")]
        public ActionResult FncStoreProcedureSglDt([FromBody] ParametrosSP p)
        {
            DataTable res;
            Mensajes m;

            try
            {
                m = new Mensajes();
                res = AccesoDatosBll.FncStoreProcedureSglDt(p);

                if (res.Columns[0].ColumnName + "-" + res.Columns[1].ColumnName == "Código-Mensaje")
                {
                    // Excepción Conexiones
                    // Excepción Base de datos
                    // Excepción Metodo en la capa de negocio

                    m.Estado = false;

                    // 4. Consulta ejecutada pero sin datos de devolución
                    if (res.Rows[0].ItemArray.GetValue(0).ToString() == "4")
                    {
                        m.Mensaje = "La sentencia se ejecuto pero no obtuvo registros.";
                    }
                    else
                    {
                        m.Mensaje = res.Rows[0].ItemArray.GetValue(0).ToString();
                    }
                }
                else
                {
                    m.Estado = true;
                    m.Mensaje = "Sentencia ejecutada con éxito";
                }

                m.Value = res;

                return Ok(m);
            }
            catch (Exception ex)
            {
                LogException.ErrorRegisterException(new Exception(AccesoDatosBll.ControlErrores(ex, controldor + " :: FncStoreProcedureSglDt")));
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Ejecuta un procedimiento almacenado.
        /// </summary>
        /// <param name="p">Homologación de tipos de parametros:
        /// Int32 = "i" o "I", Float = "d" o "D" "f" o "F", VarChar "s" o "S",
        /// Cursor = "c" o "C", Clob = "z" o "Z"</param>
        /// <returns>string</returns>
        [HttpPost]
        [Route("FncStoreProcedureSglSTR")]
        public ActionResult FncStoreProcedureSTR([FromBody] ParametrosSP p)
        {
            string res;
            Mensajes m;

            try
            {
                m = new Mensajes();
                res = AccesoDatosBll.FncStoreProcedureStr(p);

                if (res == null)
                {
                    m.Estado = false;
                    m.Mensaje = "Resultado de la sentencia Null.";
                    m.Value = "";
                }
                else
                {
                    if (res == string.Empty)
                    {
                        m.Estado = false;
                        m.Mensaje = "Resultado de la sentencia vacia.";
                        m.Value = "";
                    }
                    else
                    {
                        m.Estado = true;
                        m.Mensaje = "Sentencia ejecutada con éxito.";
                        m.Value = res;
                    }
                }

                return Ok(m);
            }
            catch (Exception ex)
            {
                LogException.ErrorRegisterException(new Exception(AccesoDatosBll.ControlErrores(ex, controldor + " :: FncStoreProcedureSglDt")));
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Ejecuta un procedimiento almacenado.
        /// </summary>
        /// <param name="p">Homologación de tipos de parametros:
        /// Int32 = "i" o "I", Float = "d" o "D" "f" o "F", VarChar "s" o "S",
        /// Cursor = "c" o "C", Clob = "z" o "Z"</param>
        /// <returns>Int32</returns>
        [HttpPost]
        [Route("FncStoreProcedureSglINT32")]
        public ActionResult FncStoreProcedureINT32([FromBody] ParametrosSP p)
        {
            Int32 res;
            Mensajes m;

            try
            {
                m = new Mensajes();
                res = AccesoDatosBll.FncStoreProcedureInt32(p);

                if (res == -1)
                {
                    m.Estado = false;
                    m.Mensaje = "Resultado de la sentencia fallido.";
                    m.Value = -1;
                }
                else
                {
                    m.Estado = true;
                    m.Mensaje = "Sentencia ejecutada con éxito.";
                    m.Value = res;
                }

                return Ok(m);
            }
            catch (Exception ex)
            {
                LogException.ErrorRegisterException(new Exception(AccesoDatosBll.ControlErrores(ex, controldor + " :: FncStoreProcedureINT32")));
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Ejecuta un procedimiento almacenado.
        /// </summary>
        /// <param name="p">Homologación de tipos de parametros:
        /// Int32 = "i" o "I", Float = "d" o "D" "f" o "F", VarChar "s" o "S",
        /// Cursor = "c" o "C", Clob = "z" o "Z"</param>
        /// <returns>DataSet</returns>
        [HttpPost]
        [Route("FncStoreProcedureSglDs")]
        public ActionResult FncStoreProcedureDs([FromBody] ParametrosSP p)
        {
            DataSet res;
            Mensajes m;

            try
            {
                // Este controlador esta pendiente por prbar por falta de datos
                m = new Mensajes();
                res = AccesoDatosBll.FncStoreProcedureDs(p);
                if (res.Tables.Count == 1)
                {
                    if (res.Tables[0].Columns[0].ColumnName + "-" + res.Tables[0].Columns[1].ColumnName == "Código-Mensaje")
                    {
                        // Excepción Conexiones
                        // Excepción Base de datos
                        // Excepción Metodo en la capa de negocio

                        m.Estado = false;

                        // 4. Consulta ejecutada pero sin datos de devolución
                        if (res.Tables[0].Rows[0].ItemArray.GetValue(0).ToString() == "4")
                        {
                            m.Mensaje = "La sentencia se ejecuto pero no obtuvo registros.";
                        }
                        else
                        {
                            m.Mensaje = res.Tables[0].Rows[0].ItemArray.GetValue(0).ToString();
                        }
                    }
                    else
                    {
                        m.Estado = true;
                        m.Mensaje = "Sentencia ejecutada con éxito";
                    }
                }

                m.Value = res;

                return Ok(m);
            }
            catch (Exception ex)
            {
                LogException.ErrorRegisterException(new Exception(AccesoDatosBll.ControlErrores(ex, controldor + " :: FncStoreProcedureDs")));
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Ejecuta una consulta con parametro objeto.
        /// </summary>
        /// <param name="p">Estructura con Tag y parametros</param>
        /// <returns>DataTable</returns>
        [HttpPost]
        [Route("GetDtObjTag")]
        public ActionResult GetDtObjTag([FromBody] Tags p)
        {
            DataTable res;
            Mensajes m;

            try
            {
                m = new Mensajes();
                res = AccesoDatosBll.FncGetCursorTagParametros(p);

                var a = res.Columns.Count;

                if (res.Columns.Count >= 2 && res.Columns[0].ColumnName + "-" + res.Columns[1].ColumnName == "Código-Mensaje")
                {
                    // Excepción Conexiones
                    // Excepción Base de datos
                    // Excepción Metodo en la capa de negocio

                    m.Estado = false;

                    // 4. Consulta ejecutada pero sin datos de devolución
                    if (res.Rows[0].ItemArray.GetValue(0).ToString() == "4")
                    {
                        m.Mensaje = "La sentencia se ejecuto pero no obtuvo registros.";
                    }
                    else
                    {
                        m.Mensaje = res.Rows[0].ItemArray.GetValue(0).ToString();
                    }
                }
                else
                {
                    m.Estado = true;
                    m.Mensaje = "Sentencia ejecutada con éxito";
                }

                m.Value = res;

                return Ok(m);
            }
            catch (Exception ex)
            {
                LogException.ErrorRegisterException(new Exception(AccesoDatosBll.ControlErrores(ex, controldor + " :: GetReporteObjTag")));
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }


        #region Metodos PRotegidos
        /// <summary>
        /// Metodo protegido
        /// Ejecuta un procedimiento almacenado con Tag.
        /// </summary>
        /// <param name="p">Homologación de tipos de parametros:
        /// Int32 = "i" o "I", Float = "d" o "D" "f" o "F", VarChar "s" o "S",
        /// Cursor = "c" o "C", Clob = "z" o "Z", Date = "dt" o "DT" "Dt" o "dT"</param>
        /// <returns>DataTable</returns>
        [Authorize]
        [HttpPost]
        [Route("ProFncStoreProcedureTagDt")]
        public ActionResult ProFncStoreProcedureTagDt([FromBody] Request p)
        {
            DataTable res;
            Mensajes m;

            try
            {
                m = new Mensajes();

                res = AccesoDatosBll.ProFncStoreProcedureTagDt(p.Parametros);

                if (res.Columns.Count > 1)
                {
                    if (res.Columns[0].ColumnName + "-" + res.Columns[1].ColumnName == "Código-Mensaje")
                    {
                        // Excepción Conexiones
                        // Excepción Base de datos
                        // Excepción Metodo en la capa de negocio

                        m.Estado = false;

                        // 4. Consulta ejecutada pero sin datos de devolución
                        if (res.Rows[0].ItemArray.GetValue(0).ToString() == "4")
                        {
                            m.Mensaje = "La sentencia se ejecuto pero no obtuvo registros.";
                        }
                        else
                        {
                            m.Mensaje = res.Rows[0].ItemArray.GetValue(0).ToString();
                        }
                    }
                    else
                    {
                        m.Estado = true;
                        m.Mensaje = "Sentencia ejecutada con éxito";
                    }
                }
                else
                {
                    if (res.Rows[0].ItemArray.GetValue(0).ToString() == "4")
                    {
                        m.Mensaje = "La sentencia se ejecuto pero no obtuvo registros.";
                    }
                    else
                    {
                        m.Estado = true;
                        m.Mensaje = "Sentencia ejecutada con éxito";
                    }
                }

                m.Value = res;

                return Ok(m);
            }
            catch (Exception ex)
            {
                LogException.ErrorRegisterException(new Exception(AccesoDatosBll.ControlErrores(ex, controldor + " :: FncStoreProcedureTagDt")));
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Metodo protegido
        /// Ejecuta un procedimiento almacenado con Tag.
        /// </summary>
        /// <param name="p">Homologación de tipos de parametros:
        /// Int32 = "i" o "I", Float = "d" o "D" "f" o "F", VarChar "s" o "S",
        /// Cursor = "c" o "C", Clob = "z" o "Z", Date = "dt" o "DT" "Dt" o "dT"</param>
        /// <returns>Int32</returns>
        //[BearerValidator]
        [Authorize]
        [HttpPost]
        [Route("ProFncStoreProcedureTagInt32")]
        public ActionResult ProFncStoreProcedureTagInt32([FromBody] Request p)
        {
            Int32 res;
            Mensajes m;

            try
            {
                m = new Mensajes();

                res = AccesoDatosBll.ProFncStoreProcedureTagInt32(p.Parametros);

                if (res == -1)
                {
                    m.Estado = false;
                    m.Mensaje = "Resultado de la sentencia fallido.";
                    m.Value = -1;
                }
                else
                {
                    m.Estado = true;
                    m.Mensaje = "Sentencia ejecutada con éxito.";
                    m.Value = res;
                }

                return Ok(m);

            }
            catch (Exception ex)
            {
                LogException.ErrorRegisterException(new Exception(AccesoDatosBll.ControlErrores(ex, controldor + " :: FncStoreProcedureTagInt32")));
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Metodo protegido
        /// Ejecuta un procedimiento almacenado con Tag.
        /// </summary>
        /// <param name="p">Homologación de tipos de parametros:
        /// Int32 = "i" o "I", Float = "d" o "D" "f" o "F", VarChar "s" o "S",
        /// Cursor = "c" o "C", Clob = "z" o "Z", Date = "dt" o "DT" "Dt" o "dT"</param>
        /// <returns>string</returns>
        //[BearerValidator]
        [Authorize]
        [HttpPost]
        [Route("ProFncStoreProcedureTagStr")]
        public ActionResult ProFncStoreProcedureTagStr([FromBody] Request p)
        {
            string res;
            Mensajes m;

            try
            {
                m = new Mensajes();

                res = AccesoDatosBll.ProFncStoreProcedureTagStr(p.Parametros);

                if (res == string.Empty)
                {
                    m.Estado = false;
                    m.Mensaje = "Error al realizar la consulta";
                    m.Value = "";
                }
                else
                {
                    m.Estado = true;
                    m.Mensaje = "Sentencia ejecutada con éxito.";
                    m.Value = res;
                }

                return Ok(m);

            }
            catch (Exception ex)
            {
                LogException.ErrorRegisterException(new Exception(AccesoDatosBll.ControlErrores(ex, controldor + " :: FncStoreProcedureTagStr")));
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
        #endregion

        /// <summary>
        /// flag de version para garantizar despliegue de datos actualizado
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("TestVersion")]
        public ActionResult TestVersion()
        {            
            Mensajes m;

            try
            {                
                m = new Mensajes();
                m.Estado = true;
                m.Mensaje = "V 1.2";
                m.Value = new {
                            cambio1 = "log4net",
                            cambio2 = "Rename Controllers"
                          };
    
            return Ok(m);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex);
            }
        }
    }
}
