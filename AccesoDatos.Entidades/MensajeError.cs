using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccesoDatos.Entidades
{
    public class MensajeError
    {
        public string Mensaje { get; set; }
        public string Comentario { get; set; }
        public string Fecha { get; set; }
        public MensajeError() { }
    }
}
