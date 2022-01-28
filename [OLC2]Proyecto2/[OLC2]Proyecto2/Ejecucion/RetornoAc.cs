using System;
using System.Collections.Generic;
using System.Text;

namespace _OLC2_Proyecto2.Ejecucion
{
    class RetornoAc
    {
        public String Tipo;
        public String Valor;
        public String Linea;
        public String Columna;
        public Boolean Detener;
        public Boolean Retorna;

        public RetornoAc(String Tipo, String Valor, String Linea, String Columna)
        {
            this.Tipo = Tipo;
            this.Valor = Valor;
            this.Linea = Linea;
            this.Columna = Columna;
            this.Detener = false;
            this.Retorna = false;
        }
    }
}
