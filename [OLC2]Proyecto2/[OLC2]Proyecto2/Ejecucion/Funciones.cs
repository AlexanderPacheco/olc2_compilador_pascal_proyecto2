using System;
using System.Collections.Generic;
using System.Text;
using Irony.Parsing;

namespace _OLC2_Proyecto2.Ejecucion
{
    class Parametro
    {
        public String Ambito; //Valor o Referencia
        public String Nombre;
        public String Valor;
        public String Tipo;
        public String Linea;
        public String Columna;
        public Boolean Activo;

        public Parametro(string ambito, string nombre, string valor, string tipo, string linea, string columna)
        {
            this.Ambito = ambito;
            this.Nombre = nombre;
            this.Valor = valor;
            this.Tipo = tipo;
            this.Linea = linea;
            this.Columna = columna;
        }
    }

    class Funciones
    {
        private String Key;
        private String Ambito;
        private String Nombre;
        private String ValorRetorno;
        private String Tipo;
        private List<Parametro> Parametros;
        private ParseTreeNode Cuerpo;
        private String Linea;
        private String Columna;

        public Funciones(String Key, String Ambito, String Nombre, String ValorRetorno, String Tipo, List<Parametro> Parametros, ParseTreeNode Cuerpo, String Linea, String Columna)
        {
            this.Key = Key;
            this.Ambito = Ambito;
            this.Nombre = Nombre;
            this.ValorRetorno = ValorRetorno;
            this.Tipo = Tipo;
            this.Parametros = Parametros;
            this.Cuerpo = Cuerpo;
            this.Linea = Linea;
            this.Columna = Columna;
        }

        //SETS
        public void setKey(string Key)
        {
            this.Key = Key;
        }
        public void setAmbito(string Ambito)
        {
            this.Ambito = Ambito;
        }
        public void setNombre(string Nombre)
        {
            this.Nombre = Nombre;
        }
        public void setValorRetorno(string ValorRetorno)
        {
            this.ValorRetorno = ValorRetorno;
        }
        public void setTipo(string Tipo)
        {
            this.Tipo = Tipo;
        }
        public void setParametros(List<Parametro> Parametros)
        {
            this.Parametros = Parametros;
        }
        public void setcuerpo(ParseTreeNode Cuerpo)
        {
            this.Cuerpo = Cuerpo;
        }
        public void setLinea(string Linea)
        {
            this.Linea = Linea;
        }
        public void setColumna(string Columna)
        {
            this.Columna = Columna;
        }

        //GETS
        public string getKey()
        {
            return this.Key;
        }
        public string getAmbito()
        {
            return this.Ambito;
        }
        public string getNombre()
        {
            return this.Nombre;
        }
        public string getValorRetorno()
        {
            return this.ValorRetorno;
        }
        public string getTipo()
        {
            return this.Tipo;
        }
        public List<Parametro> getParametros()
        {
            return this.Parametros;
        }
        public ParseTreeNode getCuerpo()
        {
            return this.Cuerpo;
        }
        public string getLinea()
        {
            return this.Linea;
        }
        public string getColumna()
        {
            return this.Columna;
        }
    }
}
