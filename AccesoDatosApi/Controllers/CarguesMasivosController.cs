using AccesoDatos.Entidades;
using AccesoDatos.Log;
using AccesoDatos.Negocio;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Data;


namespace AccesoDatos.Controllers
{
    /// <summary>
    /// Controlador para cargues masivos
    /// </summary>
    [ApiController]
    [Route("api/CarguesMasivos")]
    public class CarguesMasivosController : ControllerBase
    {
        private readonly string controldor = "AccesoDatos - CarguesMasivos";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parametrosCM"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("EjecutarCM")]
        public ActionResult EjecutarCM([FromBody] ParametrosCM parametrosCM)
        {
            DataTable res;
            Mensajes m;
            try
            {
                m = new Mensajes();

                // Se limpia las cadenas de los \r con el fin de procesar correctamente el CM
                AccesoDatosBll.ReemplazarRetonoCarril(parametrosCM);

                res = AccesoDatosBll.EjecutarCm(parametrosCM);

                if (res.Rows[0].ItemArray.GetValue(0).ToString() == "mensaje")
                {
                    m.Estado = false;
                    m.Mensaje = res.Rows[0].ItemArray.GetValue(2).ToString();
                    m.Value = new DataTable();
                }
                else
                {
                    m.Estado = true;
                    m.Mensaje = "Cargue masivo realizado con éxito.";
                    m.Value = res;
                }

                return Ok(m);
            }
            catch (Exception ex)
            {
                LogException.ErrorRegisterException(new Exception(AccesoDatosBll.ControlErrores(ex, controldor + " :: EjecutarCM")));
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parametrosCMTipo"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetCargueMasivoTipo")]
        public ActionResult GetCargueMasivoTipo([FromBody] ParametrosCMTipo parametrosCMTipo)
        {
            DataTable res;
            Mensajes m;

            try
            {
                m = new Mensajes();
                res = AccesoDatosBll.GetCargueMasivoTipo(parametrosCMTipo);

                if (res == null)
                {
                    m.Estado = false;
                    m.Mensaje = "Resultado del cargue masivo fue Null.";
                    m.Value = new DataTable();
                }
                else
                {
                    if (res.Rows.Count == 0)
                    {
                        m.Estado = false;
                        m.Mensaje = "No hay datos para mostrar.";
                    }
                    else
                    {
                        if (res.Rows.Count > 0)
                        {
                            m.Estado = true;
                            m.Mensaje = "Cargue masivo realizado con éxito.";
                        }
                    }
                    m.Value = res;
                }

                return Ok(m);
            }
            catch (Exception ex)
            {
                LogException.ErrorRegisterException(new Exception(AccesoDatosBll.ControlErrores(ex, controldor + " :: GetCargueMasivoTipo")));
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
        
    }
}
