using System;

namespace AccesoDatos.Entidades
{
    public class Mensajes
    {
        public Mensajes()
        {
            Estado = false;
            Mensaje = string.Empty;
            Value = null;
        }

        public bool Estado { get; set; }
        public string Mensaje { get; set; }
        public object Value { get; set; }
    }
}
