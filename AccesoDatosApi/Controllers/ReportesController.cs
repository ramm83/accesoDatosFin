using AccesoDatos.Entidades;
using AccesoDatos.Log;
using AccesoDatos.Negocio;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data;


namespace AccesoDatos.Controllers
{
    /// <summary>
    /// Controlador para reportes
    /// </summary>
    [ApiController]
    [Route("api/Reportes")]
    public class ReportesController : ControllerBase
    {
        private readonly string controldor = "AccesoDatos - Reportes";

        /// <summary>
        /// Ejecuta una consulta con Tag
        /// </summary>
        /// <returns>DataTable</returns>
        [HttpGet]
        [Route("GetReporteTag/{tag}")]
        public ActionResult GetReporteTag(string tag)
        {
            DataTable res;
            Mensajes m;

            try
            {
                m = new Mensajes();

                res = AccesoDatosBll.FncGetCursorTag(tag);

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
                LogException.ErrorRegisterException(new Exception(AccesoDatosBll.ControlErrores(ex, controldor + " :: GetReporteTag")));
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Ejecuta una consulta con IdSql
        /// </summary>
        /// <returns>DataTable</returns>
        [HttpGet]
        [Route("GetReporteIdSQL/{idSQL}")]
        public ActionResult GetReporteIdSQL(int idSQL)
        {
            DataTable res;
            Mensajes m;

            try
            {
                m = new Mensajes();
                res = AccesoDatosBll.FncGetCursorIdSql(idSQL);

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
                LogException.ErrorRegisterException(new Exception(AccesoDatosBll.ControlErrores(ex, controldor + " :: GetReporteIdSQL")));
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Ejecuta una consulta con parametro objeto.
        /// </summary>
        /// <param name="p">Estructura con Tag y parametros</param>
        /// <returns>DataTable</returns>
        [HttpPost]
        [Route("GetReporteObjTag")]
        public ActionResult GetReporteObjTag([FromBody] Tags p)
        {
            DataTable res;
            Mensajes m;

            try
            {
                m = new Mensajes();
                res = AccesoDatosBll.FncGetCursorTagParametros(p);

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

        /// <summary>
        /// Ejecuta una consulta con parametro objeto.
        /// </summary>
        /// <param name="p">Estructura con IdSql y parametros</param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetReporteObjIdSql")]
        public ActionResult GetReporteObjIdSql([FromBody] IdSqls p)
        {
            DataTable res;
            Mensajes m;

            try
            {
                m = new Mensajes();
                res = AccesoDatosBll.FncGetCursorIdSqlParametros(p);

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
                LogException.ErrorRegisterException(new Exception(AccesoDatosBll.ControlErrores(ex, controldor + " :: GetReporteObjIdSql")));
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
