using System.Collections.Generic;
namespace AccesoDatos.Entidades
{
    public class ParametrosCM
    {
        public int IdTipoCM { get; set; }
        public int CantidadLineas { get; set; }
        public List<string> LstLineas { get; set; }
        public string Usuario { get; set; }
    }
}
