using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _OLC2_Proyecto2
{
    class TokenError
    {
        public int linea;
        public int columna;
        public string tipo;
        public string descripcion;

        public TokenError()
        {
            this.linea = 0;
            this.columna = 0;
            this.tipo = "";
            this.descripcion = "";
        }
        public TokenError(int linea, int columna, string tipo, string descripcion)
        {
            this.linea = linea + 1;
            this.columna = columna + 1;
            this.tipo = tipo;
            this.descripcion = descripcion;
        }
    }
}
