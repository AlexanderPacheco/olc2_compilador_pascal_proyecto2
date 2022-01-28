using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;

namespace _OLC2_Proyecto2.Ejecucion
{
    class TablaFunciones
    {
        public String Nivel;
        public String Tipo;
        public Boolean Retorno;
        public Boolean Detener;
        public Boolean Continuar;

        public List<Funciones> tf = new List<Funciones>();

        public TablaFunciones()
        {

        }

        public void addFuncion(String Key, String Ambito, String Nombre, String ValorRetorno, String Tipo, List<Parametro> Parametros, ParseTreeNode Cuerpo, String Linea, String Columna)
        {
            tf.Add(new Funciones(Key, Ambito, Nombre, ValorRetorno, Tipo, Parametros, Cuerpo, Linea, Columna));
        }

        public Boolean existeFuncion(string nombre)
        {
            if (!vacio())
            {
                foreach (Funciones funcion in tf)
                {
                    if (funcion.getNombre().Equals(nombre))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public Boolean existeFuncionByKey(String key)
        {
            if (!vacio())
            {
                foreach (Funciones funcion in tf)
                {
                    if (funcion.getKey().Equals(key))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public Boolean vacio()
        {
            if (!tf.Any())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public Funciones RetornarFuncion(String nombre)
        {
            if (!vacio())
            {
                foreach (Funciones funcion in tf)
                {
                    if (funcion.getNombre().Equals(nombre))
                    {
                        return funcion;
                    }
                }
            }
            return null;
        }

        public Funciones RetornarFuncionEvaluandoSobrecarga(String nombre, List<Celda> arreglo)
        {
            if (!vacio())
            {
                foreach (Funciones funcion in tf)
                {
                    if (funcion.getNombre().Equals(nombre))
                    {
                        if (funcion.getParametros().Count == arreglo.Count)
                        {
                            Boolean bandera = true;
                            int cont = 0;

                            foreach (Parametro parametro in funcion.getParametros())
                            {
                                if (!parametro.Tipo.Equals(arreglo.ElementAt(cont).tipo))
                                {
                                    bandera = false; //Si entra aca quiere decir que uno de los parametros no es valida segun la sobrecarga
                                }
                                cont++;
                            }

                            if (bandera)
                            {
                                return funcion;
                            }
                        }
                    }
                }
            }
            return null;
        }

        public Funciones RetornarFuncionEvaluandoSobrecargaVoid(String nombre)
        {
            if (!vacio())
            {
                foreach (Funciones funcion in tf)
                {
                    if (funcion.getNombre().Equals(nombre))
                    {
                        if (funcion.getParametros() == null)
                        {
                            return funcion;
                        }
                    }
                }
            }
            return null;
        }
    }
}
