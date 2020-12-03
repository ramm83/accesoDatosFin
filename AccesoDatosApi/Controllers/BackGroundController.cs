using AccesoDatos.Entidades;
using AccesoDatos.Log;
using AccesoDatos.Negocio;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;


namespace AccesoDatos.Controllers
{
    /// <summary>
    /// Controlador de procesos BackGround Cargues Masivos y Reportes
    /// </summary>
    [ApiController]
    [Route("api/BackGround")]
    public class BackGroundController : ControllerBase
    {
        private readonly string controldor = "AccesoDatos - BackGround";

        /// <summary>
        /// Persistir las peticiones de reportes y Cm de los usuarios de internet
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("PrcSetTareaEjecucion")]
        public ActionResult PrcSetTareaEjecucion([FromBody] string data)
        {
            Mensajes m = new Mensajes();

            try
            {
                //AccesoDatosBll.RegistrarError(data);
                m.Estado = true;
                m.Mensaje = "OK";
                m.Value = true;
                return Ok(m);
            }
            catch (Exception ex)
            {
                LogException.ErrorRegisterException(new Exception(AccesoDatosBll.ControlErrores(ex, controldor + " :: PrcSetTareaEjecucion")));
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Obtener la información del usuario respecto a sus procesos en BackGround
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("PrcGetTareaEjecucion")]
        public ActionResult PrcGetTareaEjecucion()
        {
            Mensajes m = new Mensajes();

            try
            {
                //AccesoDatosBll.RegistrarError(data);
                m.Estado = true;
                m.Mensaje = "OK";
                m.Value = true;
                return Ok(m);
            }
            catch (Exception ex)
            {
                LogException.ErrorRegisterException(new Exception(AccesoDatosBll.ControlErrores(ex, controldor + " :: PrcGetTareaEjecucion")));
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
