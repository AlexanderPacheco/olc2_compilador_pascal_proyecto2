using System;
using System.Collections.Generic;
using System.Text;

namespace _OLC2_Proyecto2.Ejecucion
{
    class Error
    {
        public string Tipo;
        public string Descripcion;
        public string Linea;
        public string Columna;

        public Error(string Tipo, string Descripcion, string Linea, string Columna)
        {
            this.Tipo = Tipo;
            this.Descripcion = Descripcion;
            this.Linea = Linea;
            this.Columna = Columna;
        }
    }
}
