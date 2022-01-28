using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace _OLC2_Proyecto2.Ejecucion
{
    class TablaSimbolos
    {
        public int Nivel;
        public String Tipo;
        public String Nombre;
        public String TipoDato;
        public Boolean Retorna;
        public Boolean Detener; //Break
        public List<Simbolo> ts = new List<Simbolo>();

        /*
         * Esta variable lleva el control del apuntador vacio actualmente
         * este apuntador funciona en los entornos de funciones y procedimientos
         */
        public int apuntadorRelativo;

        /*
         * Esta variable guarda la etiqueta de salto para return
         */
        public string etiquetaExit;
        /*
         * Esta variable guarda la etiqueta de salto para break
         */
        public string etiquetaBreak;
        /*
         * Esta variable guarda la etiqueta de salto para continue
         */
        public string etiquetaContinue;
        public Boolean Continuar;

        public TablaSimbolos(int Nivel, String Tipo, String Nombre, String TipoDato, Boolean Retorna, Boolean Detener, String EtiquetaExit, String EtiquetaBreak)
        {
            this.Nivel = Nivel;
            this.Tipo = Tipo;
            this.Nombre = Nombre;
            this.TipoDato = TipoDato;
            this.Retorna = Retorna;
            this.Detener = Detener;
            this.apuntadorRelativo = 0;
            this.etiquetaExit = EtiquetaExit;
            this.etiquetaBreak = EtiquetaBreak;
        }

        public void addSimbolo(int absolutaSP, int relativaSP, string ambito, string nombre, string valor, string tipo, string tipoobjeto, string linea, string columna, Boolean visibilidad, List<Celda> arreglo)
        {
            ts.Add(new Simbolo(absolutaSP, relativaSP, ambito, nombre, valor, tipo, tipoobjeto, linea, columna, visibilidad, arreglo));
        }

        public int getApuntador()
        {
            this.apuntadorRelativo++;
            return this.apuntadorRelativo;
        }

        public List<Simbolo> getTS()
        {
            if (!vacio())
            {
                return ts;
            }
            return null;
        }

        public Boolean existeSimbolo(string nombre)
        {
            if (!vacio())
            {
                foreach (Simbolo simbolo in ts)
                {
                    if (simbolo.Nombre.Equals(nombre))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public Boolean vacio()
        {
            if (!ts.Any())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public Simbolo RetornarSimbolo(String nombre)
        {
            if (!vacio())
            {
                foreach (Simbolo simbolo in ts)
                {
                    if (simbolo.Nombre.Equals(nombre))
                    {
                        return simbolo;
                    }
                }
            }
            return null;
        }

        public void LimpiarTS()
        {
            ts.Clear();
        }
    }
}
