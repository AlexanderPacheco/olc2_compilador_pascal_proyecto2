using System;
using System.Collections.Generic;
using System.Text;

namespace _OLC2_Proyecto2.Ejecucion
{
    class Retorno
    {
        public String Temporal;
        public String C3D;
        public String Tipo;
        public String Valor;
        public String Linea;
        public String Columna;
        public Boolean RetornaVal;
        public Boolean Detener;
        public Boolean Retorna;

        public string labelTrue;
        public string labelFalse;
        public string ifC3D;
        public Retorno(String Temporal, String C3D, String Tipo, String Valor, String Linea, String Columna)
        {
            this.Temporal = Temporal;
            this.C3D = C3D;
            this.Tipo = Tipo;
            this.Valor = Valor;
            this.Linea = Linea;
            this.Columna = Columna;
            this.RetornaVal = false;
            this.Detener = false;
            this.Retorna = false;
            this.labelTrue = Reservada.nulo;
            this.labelFalse = Reservada.nulo;
            this.ifC3D = Reservada.nulo;
        }
    }
}
