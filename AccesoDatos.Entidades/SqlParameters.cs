using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccesoDatos.Entidades
{
    public class SqlParameters
    {
        public string Nombre;
        public string Tipo;
        public int IntValor;
        public double DouValor;
        public string StringValor;
        public string DateValor;
        public bool Entrada = true;

        public SqlParameters() { }
    }
}
