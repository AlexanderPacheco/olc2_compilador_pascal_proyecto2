using System;
using System.Collections.Generic;
using System.Text;
using Irony.Parsing;

namespace _OLC2_Proyecto2.Optimizacion
{
    class Sentencia
    {
        public string Instruccion;
        public ParseTreeNode Nodo;

        public Sentencia(string Instruccion, ParseTreeNode Nodo)
        {
            this.Instruccion = Instruccion;
            this.Nodo = Nodo;
        }
    }

    class Etiqueta
    {
        public string Nombre;
        public List<Sentencia> Sentencias = null;

        public void addEtiqueta(string Nombre)
        {
            this.Nombre = Nombre;
            this.Sentencias = new List<Sentencia>();
        }

        public void addSentencia(string tipo, ParseTreeNode Nodo)
        {
            Sentencias.Add(new Sentencia(tipo, Nodo));
        }
    }
}
