using System.Collections.Generic;

namespace AccesoDatos.Entidades
{
    public class ParametrosSP
    {        
        public string Procedimiento { get; set; }
        // La clase en ingles es para recibir los argumentos del procedimiento almacenado
        // esta debe ser homologada a tipos de datos de oracle
        public List<SqlParameters> Parametros { get; set; }
    }
}
