using System;
using System.Collections.Generic;
using System.Text;

namespace _OLC2_Proyecto2.Ejecucion
{
    class Celda
    {
        public String temporal;
        public String tipo;
        public String valor;

        public Celda(String temporal, String tipo, String valor)
        {
            this.temporal = temporal;
            this.tipo = tipo;
            this.valor = valor;
        }
    }
}
