using System;
using System.Collections.Generic;
using System.Text;

namespace _OLC2_Proyecto2.Ejecucion
{
    class Simbolo
    {
        public int ApuntadorAbsoluto;
        public int ApuntadorRelativo;
        public String Ambito; //En caso de ser parametros (Por referencia (Cuando es referencia en el VALOR va el id de la variable referenciada) | Por valor), en caso de variables (Por valor)
        public String Nombre;
        public String Valor; //PARA UN ARREGLO EL VALOR SERAN LAS DIMENSIONES
        public String Tipo;
        public String TipoObjeto; //PARAMETRO | VARIABLE | CONSTANTE | ARREGLO
        public String Linea;
        public String Columna;
        public Boolean Activo;
        public List<Celda> Arreglo;

        public Simbolo(int absolutoSP, int relativoSP, string ambito, string nombre, string valor, string tipo, String tipoobjeto, string linea, string columna, Boolean activo, List<Celda> Arreglo)
        {
            this.ApuntadorAbsoluto = absolutoSP;
            this.ApuntadorRelativo = relativoSP;
            this.Ambito = ambito;
            this.Nombre = nombre;
            this.Valor = valor;
            this.Tipo = tipo;
            this.TipoObjeto = tipoobjeto;
            this.Linea = linea;
            this.Columna = columna;
            this.Activo = activo;
            this.Arreglo = Arreglo;
        }
    }
}
