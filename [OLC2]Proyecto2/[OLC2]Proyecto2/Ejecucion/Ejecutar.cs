using Irony.Parsing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace _OLC2_Proyecto2.Ejecucion
{
    class Heap
    {
        public String Alfanum;
        public int Ascii;

        public Heap(string alfanum, int ascii)
        {
            this.Alfanum = alfanum;
            this.Ascii = ascii;
        }
    }

    class Ejecutar
    {
        public TablaFunciones tablafunciones = new TablaFunciones();

        public Stack<TablaSimbolos> pilaSimbolos;
        private TablaSimbolos cima;                     //Esto tiene el ambito actual
        private TablaSimbolos cimaG;                    //Esto tiene el ambito de las variables globales

        private List<Parametro> lstParametros;          //Para los parametros de las funciones
        private List<Celda> arreglo;                    //Para los arreglos
        public List<String> lstPrint = new List<String>();
        public List<Error> lstError = new List<Error>();

        public List<Heap> HP = new List<Heap>();

        private Boolean BanderaCaso = false; //Controla que los casos no repitan sus condiciones
        private ParseTreeNode nodoFunctions = null;

        private int Temporal = -1;
        private int Etiqueta = -1;
        private int Apuntador = -1;
        private int ApuntadorHP = -1;

        private string cadC3D = "";
        private GraficarTS graficarts;

        //private int ContadorParams = 0;
        private int nivelActual = 1; //Este controla el nivel que se estara consultando para crear, buscar y modificar las variables locales dentro de metodos, condiciones, ciclos, etc...

        public void IniciarPrimeraPasada(ParseTreeNode Nodo)
        {
            //Debug.WriteLine("Ejecutando... Inserto TS - VariablesGlobales");
            IniciarEjecucion(Nodo);
        }

        public void IniciarEjecucion(ParseTreeNode Nodo)
        {
            pilaSimbolos = new Stack<TablaSimbolos>();

            //isRetornoG = false;

            TablaSimbolos varg = new TablaSimbolos(0, "main", Reservada.variable, Reservada.nulo, false, false, Reservada.nulo, Reservada.nulo);
            pilaSimbolos.Push(varg);
            cimaG = varg;

            cadC3D += ("void main() { //Initial Program\n");
            cadC3D += ("//*** Inicia Declaracion de Variables Globales *** \n");
            cadC3D += (getEtiqueta() + ":\n");
            InitialProgram(Nodo);
            cadC3D += ("//*** Finaliza Declaracion de Variables Globales *** \n\n");
            EjecutarX();
            cadC3D += ("return;\n");
            cadC3D += ("}\n\n");

            string principal = "";
            principal = cadC3D;
            cadC3D = "";
            Estructura3D(nodoFunctions);
            string rs = agregarEncabezado();
            Form1.Salida.AppendText(rs + "\n\n" + cadC3D + principal);

        }

        private void InitialProgram(ParseTreeNode Nodo)
        {
            if (Nodo != null)
            {
                switch (Nodo.Term.Name)
                {
                    case "S":
                        //S.Rule = ToTerm("program") + id + puntocoma + ESTRUCTURA + ToTerm("begin") + SENTENCIAS + ToTerm("end") + punto
                        if (Nodo.ChildNodes.Count == 8)
                        {
                            tablafunciones.addFuncion(Reservada.Program, Reservada.Program, Reservada.Program, "nulo", "nulo", null, Nodo.ChildNodes[5], "0", "0");
                            Estructura(Nodo);
                            nodoFunctions = Nodo;
                        }
                        break;
                    default:
                        Debug.WriteLine("Error AST-->Nodo " + Nodo.Term.Name + " no existente/detectado");
                        break;
                }
            }
        }

        private void Estructura(ParseTreeNode Nodo)
        {
            /*
            S.Rule = ToTerm("program") + id + puntocoma + ESTRUCTURA + ToTerm("begin") + SENTENCIAS + ToTerm("end") + punto

            ESTRUCTURA.Rule = ESTRUCTURA + BLOQUE | BLOQUE

            BLOQUE.Rule = VARYTYPE
                        | FUNCIONES
                        | PROCEDIMIENTO
                        | Empty //Vacio
            */
            if (Nodo != null)
            {
                switch (Nodo.Term.Name)
                {
                    case "S":
                        foreach (ParseTreeNode hijo in Nodo.ChildNodes)
                        {
                            Estructura(hijo);
                        }
                        break;
                    case "ESTRUCTURA":
                        #region
                        if (Nodo.ChildNodes.Count == 2)
                        {
                            Estructura(Nodo.ChildNodes[0]); // ChildNodes[0] --> ESTRUCTURA
                            Estructura(Nodo.ChildNodes[1]); // ChildNodes[1] --> BLOQUE
                        }
                        else
                        {
                            Estructura(Nodo.ChildNodes[0]); // ChildNodes[0] --> BLOQUE
                        }
                        #endregion
                        break;
                    case "BLOQUE":
                        #region                        
                        switch (Nodo.ChildNodes[0].Term.Name)
                        {
                            case "VARYTYPE":
                                VariablesGlobales(Nodo.ChildNodes[0]); // ChildNodes[0] --> VARYTYPE
                                break;
                            case "FUNCIONES":
                                AlmacenarFuncion(Nodo.ChildNodes[0]);
                                break;
                            case "PROCEDIMIENTO":
                                AlmacenarProcedimiento(Nodo.ChildNodes[0]);
                                break;
                            default:
                                Debug.WriteLine("Error AST-->Nodo " + Nodo.Term.Name + " es empty/null");
                                break;
                        }
                        #endregion
                        break;
                    case "SENTENCIAS":
                        //Debug.WriteLine("*** Iniciando Ejecucion de Sentencias ***");
                        break;
                }
            }
            else
            {
                Debug.WriteLine("Error AST-->Nodo en funcion Estructura no existente/detectado/null");
            }
        }

        private void AlmacenarProcedimiento(ParseTreeNode Nodo)
        {
            /*
             PROCEDIMIENTO.Rule = ToTerm("procedure") + id + parentA + PARAMETROS + parentC + puntocoma + ESTRUCTURA + ToTerm("begin") + SENTENCIAS + ToTerm("end") + puntocoma
                                | ToTerm("procedure") + id + puntocoma + ESTRUCTURA + ToTerm("begin") + SENTENCIAS + ToTerm("end") + puntocoma
            */
            String id = Nodo.ChildNodes[1].Token.Value.ToString();

            switch (Nodo.ChildNodes.Count)
            {
                case 11:
                    //ToTerm("procedure") + id + parentA + PARAMETROS + parentC + puntocoma + ESTRUCTURA + ToTerm("begin") + SENTENCIAS + ToTerm("end") + puntocoma
                    if (!tablafunciones.existeFuncionByKey(id))
                    {
                        lstParametros = new List<Parametro>(); //Creando lista para parametros
                        AgregarParametros(Nodo.ChildNodes[3]); //Llenando lista de parametros
                        tablafunciones.addFuncion(id, Reservada.Procedure, id, Reservada.nulo, Reservada.nulo, lstParametros, Nodo.ChildNodes[8], getLinea(Nodo.ChildNodes[0]), "1");
                    }
                    else
                    {
                        Debug.WriteLine("Error Semantico-->Funcion ya existente linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                        lstError.Add(new Error(Reservada.ErrorSemantico, "Funcion ya existente", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                    }
                    break;
                case 8:
                    //ToTerm("procedure") + id + puntocoma + ESTRUCTURA + ToTerm("begin") + SENTENCIAS + ToTerm("end") + puntocoma
                    if (!tablafunciones.existeFuncionByKey(id))
                    {
                        tablafunciones.addFuncion(id, Reservada.Procedure, id, Reservada.nulo, Reservada.nulo, null, Nodo.ChildNodes[5], getLinea(Nodo.ChildNodes[0]), "1");
                    }
                    else
                    {
                        Debug.WriteLine("Error Semantico-->Funcion ya existente linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                        lstError.Add(new Error(Reservada.ErrorSemantico, "Funcion ya existente", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                    }
                    break;
            }
        }

        private void AlmacenarFuncion(ParseTreeNode Nodo)
        {
            /*
             FUNCIONES.Rule = ToTerm("function") + id + parentA + PARAMETROS + parentC + dospuntos + TIPODATO + puntocoma + ESTRUCTURA + ToTerm("begin") + SENTENCIAS + ToTerm("end") + puntocoma
                            | ToTerm("function") + id + dospuntos + TIPODATO + puntocoma + ESTRUCTURA + ToTerm("begin") + SENTENCIAS + ToTerm("end") + puntocoma
            */

            String id = Nodo.ChildNodes[1].Token.Value.ToString();
            String tipodato = "";

            switch (Nodo.ChildNodes.Count)
            {
                case 13:
                    //ToTerm("function") + id + parentA + PARAMETROS + parentC + dospuntos + TIPODATO + puntocoma + ESTRUCTURA + ToTerm("begin") + SENTENCIAS + ToTerm("end") + puntocoma
                    tipodato = getTipoDatoFunction(Nodo.ChildNodes[6]);

                    if (!tablafunciones.existeFuncionByKey(id))
                    {
                        lstParametros = new List<Parametro>(); //Creando lista para parametros
                        AgregarParametros(Nodo.ChildNodes[3]); //Llenando lista de parametros
                        tablafunciones.addFuncion(id, Reservada.Funcion, id, getInicialDato(tipodato), tipodato, lstParametros, Nodo.ChildNodes[10], getLinea(Nodo.ChildNodes[0]), "1");
                    }
                    else
                    {
                        Debug.WriteLine("Error Semantico-->Funcion ya existente linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                        lstError.Add(new Error(Reservada.ErrorSemantico, "Funcion ya existente", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                    }
                    break;
                case 10:
                    //ToTerm("function") + id + dospuntos + TIPODATO + puntocoma + ESTRUCTURA + ToTerm("begin") + SENTENCIAS + ToTerm("end") + puntocoma
                    if(!tablafunciones.existeFuncionByKey(id))
                    {
                        tipodato = getTipoDatoFunction(Nodo.ChildNodes[3]);
                        tablafunciones.addFuncion(id, Reservada.Funcion, id, getInicialDato(tipodato), tipodato, null, Nodo.ChildNodes[7], getLinea(Nodo.ChildNodes[0]), "1");
                    }
                    else
                    {
                        Debug.WriteLine("Error Semantico-->Funcion ya existente linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                        lstError.Add(new Error(Reservada.ErrorSemantico, "Funcion ya existente", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                    }
                    break;
            }
        }

        private void AgregarParametros(ParseTreeNode Nodo)
        {
            /*
             PARAMETROS.Rule = PARAMETROS + puntocoma + PARAMETRO
                            | PARAMETRO
            */
            switch (Nodo.Term.Name)
            {
                case "PARAMETROS":
                    foreach (ParseTreeNode hijo in Nodo.ChildNodes)
                    {
                        AgregarParametros(hijo);
                    }
                    break;
                case "PARAMETRO":
                    Parametros(Nodo);
                    break;
                default:
                    //Debug.WriteLine("Error AST-->Nodo en funcion CrearParametros no existente/detectado");
                    break;
            }
        }

        private void Parametros(ParseTreeNode Nodo)
        {
            /*
            PARAMETRO.Rule = IDPARAM + dospuntos + TIPODATO
                            | ToTerm("var") + IDPARAM + dospuntos + TIPODATO
                            | Empty
             */
            switch (Nodo.Term.Name)
            {
                case "PARAMETRO":
                    //IDPARAM + dospuntos + TIPODATO
                    if (Nodo.ChildNodes.Count == 3)
                    {
                        String td = getTipoDato(Nodo.ChildNodes[2]);
                        DeclaracionAsignacionParamData(Reservada.byVal, td, Nodo.ChildNodes[0]);
                    }
                    //ToTerm("var") + IDPARAM + dospuntos + TIPODATO
                    else if (Nodo.ChildNodes.Count == 4)
                    {
                        String td = getTipoDato(Nodo.ChildNodes[3]);
                        DeclaracionAsignacionParamData(Reservada.byRef, td, Nodo.ChildNodes[1]);
                    }
                    break;
                default:
                    Debug.WriteLine("Error AST-->Nodo en funcion Parametros no existente/detectado");
                    break;
            }
        }

        private void DeclaracionAsignacionParamData(string ambit, string tipodato, ParseTreeNode Nodo)
        {
            /*
            IDPARAM.Rule = IDPARAM + coma + id
                            | id
             */
            if (Nodo != null)
            {
                switch (Nodo.Term.Name)
                {
                    case "IDPARAM":
                        foreach (ParseTreeNode hijo in Nodo.ChildNodes)
                        {
                            DeclaracionAsignacionParamData(ambit, tipodato, hijo);
                        }
                        break;

                    case "id":
                        String id = Nodo.Token.Value.ToString();
                        /*
                        Debug.WriteLine("LLEGO A RECONOCER LOS PARAMETROS A DECLARAR PAPU");
                        Debug.WriteLine("nombre variable: " + id);
                        Debug.WriteLine("tipo parametro: " + ambit);
                        Debug.WriteLine("tipo dato: " + tipodato);
                        */
                        if (!ExisteParametro(id))
                        {
                            lstParametros.Add(new Parametro(ambit, id, getInicialDato(tipodato), tipodato, getLinea(Nodo), getColumna(Nodo)));
                        }
                        else
                        {
                            Debug.WriteLine("Error Semantico-->Parametro ya existente linea:" + getLinea(Nodo) + " columna:" + getColumna(Nodo));
                            lstError.Add(new Error(Reservada.ErrorSemantico, "Parametro ya existente", getLinea(Nodo), getColumna(Nodo)));
                        }
                        break;

                    case ",": //No hace nada
                        break;

                    default:
                        Debug.WriteLine("Error AST-->Nodo en funcion DeclaracionAsignacionParamData no existente/detectado");
                        break;
                }
            }
            else
            {
                Debug.WriteLine("Error AST-->Nodo en funcion DeclaracionAsignacionParamData no existente/detectado/null");
            }
        }

        private Boolean ExisteParametro(string id)
        {
            if(!ParamsVacio())
            {
                foreach(Parametro param in this.lstParametros)
                {
                    if (param.Nombre.Equals(id))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private Boolean ParamsVacio()
        {
            if(this.lstParametros.Count == 0)
            {
                return true; //Esta vacio
            }
            else
            {
                return false; //No esta vacio
            }
        }

        private void Estructura3D(ParseTreeNode Nodo)
        {
            /*
            S.Rule = ToTerm("program") + id + puntocoma + ESTRUCTURA + ToTerm("begin") + SENTENCIAS + ToTerm("end") + punto

            ESTRUCTURA.Rule = ESTRUCTURA + BLOQUE | BLOQUE

            BLOQUE.Rule = VARYTYPE
                        | FUNCIONES
                        | PROCEDIMIENTO
                        | Empty //Vacio
            */
            if (Nodo != null)
            {
                switch (Nodo.Term.Name)
                {
                    case "S":
                        foreach (ParseTreeNode hijo in Nodo.ChildNodes)
                        {
                            Estructura3D(hijo);
                        }
                        break;
                    case "ESTRUCTURA":
                        #region
                        if (Nodo.ChildNodes.Count == 2)
                        {
                            Estructura3D(Nodo.ChildNodes[0]); // ChildNodes[0] --> ESTRUCTURA
                            Estructura3D(Nodo.ChildNodes[1]); // ChildNodes[1] --> BLOQUE
                        }
                        else
                        {
                            Estructura3D(Nodo.ChildNodes[0]); // ChildNodes[0] --> BLOQUE
                        }
                        #endregion
                        break;
                    case "BLOQUE":
                        #region                        
                        switch (Nodo.ChildNodes[0].Term.Name)
                        {
                            case "VARYTYPE":
                                
                                break;
                            case "FUNCIONES":
                                AlmacenarFuncion3D(Nodo.ChildNodes[0]);
                                break;
                            case "PROCEDIMIENTO":
                                AlmacenarProcedimiento3D(Nodo.ChildNodes[0]);
                                break;
                            default:
                                break;
                        }
                        #endregion
                        break;
                    case "SENTENCIAS":
                        break;
                }
            }
            else
            {
                Debug.WriteLine("Error AST-->Nodo en funcion Estructura no existente/detectado/null");
            }
        }

        private void AlmacenarProcedimiento3D(ParseTreeNode Nodo)
        {
            /*
             PROCEDIMIENTO.Rule = ToTerm("procedure") + id + parentA + PARAMETROS + parentC + puntocoma + ESTRUCTURA + ToTerm("begin") + SENTENCIAS + ToTerm("end") + puntocoma
                                | ToTerm("procedure") + id + puntocoma + ESTRUCTURA + ToTerm("begin") + SENTENCIAS + ToTerm("end") + puntocoma
            */
            String id = Nodo.ChildNodes[1].Token.Value.ToString();

            switch (Nodo.ChildNodes.Count)
            {
                case 11:
                    //ToTerm("procedure") + id + parentA + PARAMETROS + parentC + puntocoma + ESTRUCTURA + ToTerm("begin") + SENTENCIAS + ToTerm("end") + puntocoma
                    Funciones funct1 = tablafunciones.RetornarFuncion(id);

                    if (tablafunciones.existeFuncionByKey(id))
                    {
                        Funciones funct2 = tablafunciones.RetornarFuncion(id);

                        if (funct2 != null)
                        {
                            string l_return = getEtiqueta();

                            TablaSimbolos proc = new TablaSimbolos(1, Reservada.Procedure, id, Reservada.nulo, true, false, l_return, Reservada.nulo); //Esto depende de si es VOID
                            pilaSimbolos.Push(proc);
                            cima = proc; //Estableciendo la tabla de simbolos cima
                            nivelActual++; //Estableciendo el nivel actual

                            cadC3D += ("void " + id + "() {\n");
                            cadC3D += (getEtiqueta() + ":\n");

                            paramsFunction3D(funct2.getParametros());

                            varLocales(Nodo.ChildNodes[6]); //Escribe el C3D de las variables de la funcion

                            RetornoAc retorno = Sentencias(funct2.getCuerpo()); //Escribe el C3D de las sentencias de la funcion

                            cadC3D += (l_return + ":\n");
                            cadC3D += ("return;\n");
                            cadC3D += ("}\n\n");

                            nivelActual--; //Disminuimos el nivel actual ya que salimos del metodo invocado
                            pilaSimbolos.Pop(); //Eliminando la tabla de simbolos cima actual
                            cima = pilaSimbolos.Peek(); //Estableciendo la nueva tabla de simbolo cima
                        }
                        else
                        {
                            Debug.WriteLine("Error Semantico-->Procedimiento no existente linea:" + funct2.getLinea() + " columna:" + funct2.getColumna());
                            lstError.Add(new Error(Reservada.ErrorSemantico, "Procedimiento no existente", funct2.getLinea(), funct2.getColumna()));
                        }
                    }
                    else
                    {
                        Debug.WriteLine("Error Semantico-->Procedimiento no existente linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                        lstError.Add(new Error(Reservada.ErrorSemantico, "Funcion no existente", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                    }
                    break;
                case 8:
                    //ToTerm("procedure") + id + puntocoma + ESTRUCTURA + ToTerm("begin") + SENTENCIAS + ToTerm("end") + puntocoma
                    if (tablafunciones.existeFuncionByKey(id))
                    {
                        Funciones funct2 = tablafunciones.RetornarFuncion(id);

                        if (funct2 != null)
                        {
                            string l_return = getEtiqueta();

                            TablaSimbolos proc = new TablaSimbolos(1, Reservada.Procedure, id, Reservada.nulo, true, false, l_return, Reservada.nulo); //Esto depende de si es VOID
                            pilaSimbolos.Push(proc);
                            cima = proc; //Estableciendo la tabla de simbolos cima
                            nivelActual++; //Estableciendo el nivel actual

                            cadC3D += ("void " + id + "() {\n");
                            cadC3D += (getEtiqueta() + ":\n");

                            varLocales(Nodo.ChildNodes[3]); //Escribe el C3D de las variables de la funcion

                            RetornoAc retorno = Sentencias(funct2.getCuerpo()); //Escribe el C3D de las sentencias de la funcion

                            cadC3D += (l_return + ":\n");
                            cadC3D += ("return;\n");
                            cadC3D += ("}\n\n");

                            nivelActual--; //Disminuimos el nivel actual ya que salimos del metodo invocado
                            pilaSimbolos.Pop(); //Eliminando la tabla de simbolos cima actual
                            cima = pilaSimbolos.Peek(); //Estableciendo la nueva tabla de simbolo cima
                        }
                        else
                        {
                            Debug.WriteLine("Error Semantico-->Procedimiento no existente linea:" + funct2.getLinea() + " columna:" + funct2.getColumna());
                            lstError.Add(new Error(Reservada.ErrorSemantico, "Procedimiento no existente", funct2.getLinea(), funct2.getColumna()));
                        }
                    }
                    else
                    {
                        Debug.WriteLine("Error Semantico-->Procedimiento no existente linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                        lstError.Add(new Error(Reservada.ErrorSemantico, "Procedimiento no existente", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                    }
                    break;
            }
        }

        private void AlmacenarFuncion3D(ParseTreeNode Nodo)
        {
            /*
             FUNCIONES.Rule = ToTerm("function") + id + parentA + PARAMETROS + parentC + dospuntos + TIPODATO + puntocoma + ESTRUCTURA + ToTerm("begin") + SENTENCIAS + ToTerm("end") + puntocoma
                            | ToTerm("function") + id + dospuntos + TIPODATO + puntocoma + ESTRUCTURA + ToTerm("begin") + SENTENCIAS + ToTerm("end") + puntocoma
             */
            String id = Nodo.ChildNodes[1].Token.Value.ToString();

            switch (Nodo.ChildNodes.Count)
            {
                case 13:
                    #region
                    Funciones funct1 = tablafunciones.RetornarFuncion(id);

                    if (tablafunciones.existeFuncionByKey(id))
                    {
                        Funciones funct2 = tablafunciones.RetornarFuncion(id);

                        if (funct2 != null)
                        {
                            string l_return = getEtiqueta();

                            TablaSimbolos fun = new TablaSimbolos(1, Reservada.Funcion, id, funct2.getTipo(), true, false, l_return, Reservada.nulo); //Esto depende de si es VOID
                            pilaSimbolos.Push(fun);
                            cima = fun; //Estableciendo la tabla de simbolos cima
                            nivelActual++; //Estableciendo el nivel actual

                            cadC3D += ("void " + id + "() {\n");
                            cadC3D += (getEtiqueta() + ":\n");

                            paramsFunction3D(funct2.getParametros());

                            varLocales(Nodo.ChildNodes[8]); //Escribe el C3D de las variables de la funcion

                            RetornoAc retorno = Sentencias(funct2.getCuerpo()); //Escribe el C3D de las sentencias de la funcion

                            cadC3D += (l_return + ":\n");
                            cadC3D += ("return;\n");
                            cadC3D += ("}\n\n");

                            nivelActual--; //Disminuimos el nivel actual ya que salimos del metodo invocado
                            pilaSimbolos.Pop(); //Eliminando la tabla de simbolos cima actual
                            cima = pilaSimbolos.Peek(); //Estableciendo la nueva tabla de simbolo cima
                        }
                        else
                        {
                            Debug.WriteLine("Error Semantico-->Funcion no existente linea:" + funct2.getLinea() + " columna:" + funct2.getColumna());
                            lstError.Add(new Error(Reservada.ErrorSemantico, "Funcion no existente", funct2.getLinea(), funct2.getColumna()));
                        }
                    }
                    else
                    {
                        Debug.WriteLine("Error Semantico-->Funcion no existente linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                        lstError.Add(new Error(Reservada.ErrorSemantico, "Funcion no existente", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                    }
                    #endregion
                    break;
                case 10:
                    //ToTerm("function") + id + dospuntos + TIPODATO + puntocoma + ESTRUCTURA + ToTerm("begin") + SENTENCIAS + ToTerm("end") + puntocoma
                    #region
                    if (tablafunciones.existeFuncionByKey(id))
                    {
                        Funciones funct2 = tablafunciones.RetornarFuncion(id);

                        if (funct2 != null)
                        {
                            string l_return = getEtiqueta();

                            TablaSimbolos fun = new TablaSimbolos(1, Reservada.Funcion, id, funct2.getTipo(), true, false, l_return, Reservada.nulo); //Esto depende de si es VOID
                            pilaSimbolos.Push(fun);
                            cima = fun; //Estableciendo la tabla de simbolos cima
                            nivelActual++; //Estableciendo el nivel actual
                            
                            cadC3D += ("void " + id + "() {\n");
                            cadC3D += (getEtiqueta() + ":\n");

                            varLocales(Nodo.ChildNodes[5]); //Escribe el C3D de las variables de la funcion

                            RetornoAc retorno = Sentencias(funct2.getCuerpo()); //Escribe el C3D de las sentencias de la funcion

                            cadC3D += (l_return + ":\n");
                            cadC3D += ("return;\n");
                            cadC3D += ("}\n\n");
                            
                            nivelActual--; //Disminuimos el nivel actual ya que salimos del metodo invocado
                            pilaSimbolos.Pop(); //Eliminando la tabla de simbolos cima actual
                            cima = pilaSimbolos.Peek(); //Estableciendo la nueva tabla de simbolo cima
                        }
                        else
                        {
                            Debug.WriteLine("Error Semantico-->Funcion no existente linea:" + funct2.getLinea() + " columna:" + funct2.getColumna());
                            lstError.Add(new Error(Reservada.ErrorSemantico, "Funcion no existente", funct2.getLinea(), funct2.getColumna()));
                        }
                    }
                    else
                    {
                        Debug.WriteLine("Error Semantico-->Funcion no existente linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                        lstError.Add(new Error(Reservada.ErrorSemantico, "Funcion no existente", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                    }
                    #endregion
                    break;
            }
        }

        private void paramsFunction3D(List<Parametro> lstParam)
        {
            /*
             * No es necesario crear el codigo 3D 
             * ya que esto se hace al invocar la funcion
             * aca solo se reservan se crea la variable la su posicion relativa
             */
            if(lstParam != null)
            {
                foreach (Parametro param in lstParam)
                {
                    if (!ExisteSimbolo(param.Nombre))
                    {
                        cima.addSimbolo(-1, cima.getApuntador(), Reservada.variable, param.Nombre, param.Valor, param.Tipo, Reservada.variable, param.Linea, param.Columna, true, null);
                    }
                    else
                    {
                        Debug.WriteLine("Error Semantico-->Variable ya existente linea:" + param.Linea + " columna:" + param.Columna);
                        lstError.Add(new Error(Reservada.ErrorSemantico, "Variable ya existente", param.Linea, param.Columna));
                    }
                }                
            }
            else
            {
                Debug.WriteLine("Error Semantico-->Lista de parametros no existe");
            }
        }

        private void VariablesGlobales(ParseTreeNode Nodo)
        {
            switch (Nodo.ChildNodes[0].Term.Name)
            {
                case "type":
                    Debug.WriteLine("** Accion type no funcional");
                    break;
                case "var": //ToTerm("var") + LSTVARS
                    LstVars(Reservada.variable, Nodo.ChildNodes[1]);
                    break;
                case "const":
                    Debug.WriteLine("** Accion const no funcional");
                    break;
                case "id":
                    Debug.WriteLine("** Accion id no funcional");
                    break;
                default:
                    Debug.WriteLine("Error AST-->Nodo en funcion Variable_y_type no existente/detectado");
                    break;
            }
        }

        private void LstVars(string tipoObj, ParseTreeNode Nodo)
        {
            switch (Nodo.Term.Name)
            {
                case "LSTVARS":
                    foreach (ParseTreeNode hijo in Nodo.ChildNodes)
                    {
                        LstVars(tipoObj, hijo);
                    }
                    break;
                case "VARS":
                    Vars(tipoObj, Nodo);
                    break;
                default:
                    Debug.WriteLine("Error AST-->Nodo en funcion LstVars no existente/detectado");
                    break;
            }
        }

        private void Vars(string tipoObj, ParseTreeNode Nodo)
        {
            switch (Nodo.Term.Name)
            {
                case "VARS":
                    //LSTID + dospuntos + TIPODATO + puntocoma
                    if (Nodo.ChildNodes.Count == 4)
                    {
                        String td = getTipoDato(Nodo.ChildNodes[2]);
                        Retorno asignar = new Retorno(Reservada.nulo, Reservada.nulo, td, getInicialDato(td), getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1]));
                        DeclaracionAsignacionData(tipoObj, td, asignar, Nodo.ChildNodes[0]);
                    }
                    //LSTID + dospuntos + TIPODATO + ToTerm("=") + CONDICION + puntocoma
                    else if (Nodo.ChildNodes.Count == 6)
                    {
                        String td = getTipoDato(Nodo.ChildNodes[2]);
                        Retorno asignar = Condicion(Nodo.ChildNodes[4]);
                        DeclaracionAsignacionData(tipoObj, td, asignar, Nodo.ChildNodes[0]);
                    }
                    break;
                default:
                    Debug.WriteLine("Error AST-->Nodo en funcion Vars no existente/detectado");
                    break;
            }
        }

        private void DeclaracionAsignacionData(string tipoObj, String tipodato, Retorno ret, ParseTreeNode Nodo)
        {
            //METODO PARA LA DECLARACION DE VARIABLES
            /*
             LSTID.Rule = LSTID + coma + id
                        | id
             */
            if (Nodo != null)
            {
                switch (Nodo.Term.Name)
                {
                    case "LSTID":
                        foreach (ParseTreeNode hijo in Nodo.ChildNodes)
                        {
                            DeclaracionAsignacionData(tipoObj, tipodato, ret, hijo);
                        }
                        break;

                    case "id":
                        String id = Nodo.Token.Value.ToString();
                        /*
                        Debug.WriteLine("LLEGO A RECONOCER LAS VARIABLES A DECLARAR PAPU");
                        Debug.WriteLine("nombre variable: " + id);
                        Debug.WriteLine("tipo objeto: " + tipoObj);
                        Debug.WriteLine("tipo dato: " + tipodato);
                        Debug.WriteLine("Valor asignable: " + ret.Valor.ToString());
                        */
                        if (!cimaG.existeSimbolo(id))
                        {
                            if (ret != null)
                            {
                                if (ret.Tipo.Equals(tipodato)) //Si son del mismo tipo se pueden asignar (variable con variable)
                                {
                                    //Debug.WriteLine("Se creo variable: " + id + " --> " + ret.Valor + " (" + ret.Tipo + ")");

                                    int apuntador = newApuntador();

                                    if (tipodato.Equals(Reservada.Cadena))
                                    {
                                        int apHP = newApuntadorHP();
                                        string tmp = getTemp();

                                        stringToHeap(tmp, apHP, ret.Valor);
                                        setStack(apuntador + "", new Retorno(tmp, "", Reservada.Cadena, ret.Valor, getLinea(Nodo), getColumna(Nodo)), id);
                                        cimaG.addSimbolo(apuntador, -1, Reservada.variable, id, ret.Valor, tipodato, Reservada.variable, getLinea(Nodo), getColumna(Nodo), true, null);
                                    }
                                    else if (tipodato.Equals(Reservada.Booleano))
                                    {
                                        string lbltmp = getEtiqueta(); //Temporal de salida

                                        ret.Temporal = Reservada.nulo;
                                        cadC3D += ret.labelTrue + ":\n";   //True
                                        ret.Valor = "1";
                                        setStack(apuntador + "", ret, id);
                                        cadC3D += "goto " + lbltmp + ";\n";
                                        cadC3D += ret.labelFalse + ":\n";   //False
                                        //ret.Temporal = Reservada.nulo;
                                        ret.Valor = "0";
                                        setStack(apuntador + "", ret, id);
                                        cadC3D += lbltmp + ":\n";
                                        cimaG.addSimbolo(apuntador, -1, Reservada.variable, id, ret.Valor, tipodato, Reservada.variable, getLinea(Nodo), getColumna(Nodo), true, null);
                                    }
                                    else
                                    {
                                        setStack(apuntador + "", ret, id);
                                        cimaG.addSimbolo(apuntador, -1, Reservada.variable, id, ret.Valor, tipodato, Reservada.variable, getLinea(Nodo), getColumna(Nodo), true, null);
                                    }
                                }
                                else
                                {
                                    Debug.WriteLine("Error Semantico-->Asignacion no valida, tipo de dato incorrecto linea:" + getLinea(Nodo) + " columna:" + getColumna(Nodo));
                                    lstError.Add(new Error(Reservada.ErrorSemantico, "Asignacion no valida, tipo de dato incorrecto", getLinea(Nodo), getColumna(Nodo)));
                                }
                            }
                            else
                            {
                                Debug.WriteLine("Error Semantico-->Asignacion no valida, expresion incorrecta linea:" + getLinea(Nodo) + " columna:" + getColumna(Nodo));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Asignacion no valida, expresion incorrecta", getLinea(Nodo), getColumna(Nodo)));
                            }
                        }
                        else
                        {
                            Debug.WriteLine("Error Semantico-->Variable ya existente linea:" + getLinea(Nodo) + " columna:" + getColumna(Nodo));
                            lstError.Add(new Error(Reservada.ErrorSemantico, "Variable ya existente", getLinea(Nodo), getColumna(Nodo)));
                        }

                        break;

                    case ",": //No hace nada
                        break;

                    default:
                        //Debug.WriteLine("Error AST-->Nodo en funcion DeclaracionAsignacionData no existente/detectado");
                        break;
                }
            }
            else
                Debug.WriteLine("Error AST-->Nodo en funcion DeclaracionAsignacionData no existente/detectado/null");
        }

        private void varLocales(ParseTreeNode Nodo)
        {
            /*
            S.Rule = ToTerm("program") + id + puntocoma + ESTRUCTURA + ToTerm("begin") + SENTENCIAS + ToTerm("end") + punto

            ESTRUCTURA.Rule = ESTRUCTURA + BLOQUE | BLOQUE

            BLOQUE.Rule = VARYTYPE
                        | FUNCIONES
                        | PROCEDIMIENTO
                        | Empty //Vacio
            */
            if (Nodo != null)
            {
                switch (Nodo.Term.Name)
                {
                    case "S":
                        foreach (ParseTreeNode hijo in Nodo.ChildNodes)
                        {
                            varLocales(hijo);
                        }
                        break;
                    case "ESTRUCTURA":
                        #region
                        foreach (ParseTreeNode hijo in Nodo.ChildNodes)
                        {
                            varLocales(hijo);
                        }
                        /*if (Nodo.ChildNodes.Count == 2)
                        {
                            varLocales(Nodo.ChildNodes[0]); // ChildNodes[0] --> ESTRUCTURA
                            varLocales(Nodo.ChildNodes[1]); // ChildNodes[1] --> BLOQUE
                        }
                        else
                        {
                            varLocales(Nodo.ChildNodes[0]); // ChildNodes[0] --> BLOQUE
                        }*/
                        #endregion
                        break;
                    case "BLOQUE":
                        #region                        
                        switch (Nodo.ChildNodes[0].Term.Name)
                        {
                            case "VARYTYPE":
                                VariablesLocales(Nodo.ChildNodes[0]); // ChildNodes[0] --> VARYTYPE
                                break;
                            case "FUNCIONES":
                                //AlmacenarFuncion(Nodo.ChildNodes[0]);
                                break;
                            case "PROCEDIMIENTO":
                                //AlmacenarProcedimiento(Nodo.ChildNodes[0]);
                                break;
                            default:
                                break;
                        }
                        #endregion
                        break;
                    case "SENTENCIAS":
                        break;
                }
            }
            else
            {
                Debug.WriteLine("Error AST-->Nodo en funcion Estructura no existente/detectado/null");
            }
        }

        private void VariablesLocales(ParseTreeNode Nodo)
        {
            switch (Nodo.ChildNodes[0].Term.Name)
            {
                case "type":
                    Debug.WriteLine("** Accion type no funcional");
                    break;
                case "var": //ToTerm("var") + LSTVARS
                    LstVarsLocal(Reservada.variable, Nodo.ChildNodes[1]);
                    break;
                case "const":
                    Debug.WriteLine("** Accion const no funcional");
                    break;
                case "id":
                    Debug.WriteLine("** Accion id no funcional");
                    break;
                default:
                    Debug.WriteLine("Error AST-->Nodo en funcion Variable_y_type no existente/detectado");
                    break;
            }
        }

        private void LstVarsLocal(string tipoObj, ParseTreeNode Nodo)
        {
            switch (Nodo.Term.Name)
            {
                case "LSTVARS":
                    foreach (ParseTreeNode hijo in Nodo.ChildNodes)
                    {
                        LstVarsLocal(tipoObj, hijo);
                    }
                    break;
                case "VARS":
                    VarsLocal(tipoObj, Nodo);
                    break;
                default:
                    Debug.WriteLine("Error AST-->Nodo en funcion LstVars no existente/detectado");
                    break;
            }
        }

        private void VarsLocal(string tipoObj, ParseTreeNode Nodo)
        {
            switch (Nodo.Term.Name)
            {
                case "VARS":
                    //LSTID + dospuntos + TIPODATO + puntocoma
                    if (Nodo.ChildNodes.Count == 4)
                    {
                        String td = getTipoDato(Nodo.ChildNodes[2]);
                        Retorno asignar = new Retorno(Reservada.nulo, Reservada.nulo, td, getInicialDato(td), getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1]));
                        DeclaracionAsignacionDataLocal(tipoObj, td, asignar, Nodo.ChildNodes[0]);
                    }
                    //LSTID + dospuntos + TIPODATO + ToTerm("=") + CONDICION + puntocoma
                    else if (Nodo.ChildNodes.Count == 6)
                    {
                        String td = getTipoDato(Nodo.ChildNodes[2]);
                        Retorno asignar = Condicion(Nodo.ChildNodes[4]);
                        DeclaracionAsignacionDataLocal(tipoObj, td, asignar, Nodo.ChildNodes[0]);
                    }
                    break;
                default:
                    Debug.WriteLine("Error AST-->Nodo en funcion Vars no existente/detectado");
                    break;
            }
        }

        private void DeclaracionAsignacionDataLocal(string tipoObj, String tipodato, Retorno ret, ParseTreeNode Nodo)
        {
            //METODO PARA LA DECLARACION DE VARIABLES
            /*
             LSTID.Rule = LSTID + coma + id
                        | id
             */
            if (Nodo != null)
            {
                switch (Nodo.Term.Name)
                {
                    case "LSTID":
                        foreach (ParseTreeNode hijo in Nodo.ChildNodes)
                        {
                            DeclaracionAsignacionDataLocal(tipoObj, tipodato, ret, hijo);
                        }
                        break;

                    case "id":
                        String id = Nodo.Token.Value.ToString();
                        /*
                        Debug.WriteLine("LLEGO A RECONOCER LAS VARIABLES A DECLARAR PAPU");
                        Debug.WriteLine("nombre variable: " + id);
                        Debug.WriteLine("tipo objeto: " + tipoObj);
                        Debug.WriteLine("tipo dato: " + tipodato);
                        Debug.WriteLine("Valor asignable: " + ret.Valor.ToString());
                        */
                        if (!ExisteSimbolo(id))
                        {
                            if (ret != null)
                            {
                                if (ret.Tipo.Equals(tipodato)) //Si son del mismo tipo se pueden asignar (variable con variable)
                                {
                                    //Debug.WriteLine("Se creo variable: " + id + " --> " + ret.Valor + " (" + ret.Tipo + ")");

                                    int apunta =  cima.getApuntador();
                                    string tmp = getTemp();

                                    if (tipodato.Equals(Reservada.Cadena))
                                    {
                                        int apHP = newApuntadorHP();
                                        string tmp2 = getTemp();
                                        stringToHeap(tmp, apHP, ret.Valor);
                                        setStack(tmp2, apunta + "", new Retorno(tmp, "", Reservada.Cadena, ret.Valor, getLinea(Nodo), getColumna(Nodo)), id);
                                        cima.addSimbolo(-1, apunta, Reservada.variable, id, ret.Valor, tipodato, Reservada.variable, getLinea(Nodo), getColumna(Nodo), true, null);
                                    }
                                    else if (tipodato.Equals(Reservada.Booleano))
                                    {
                                        string lbltmp = getEtiqueta(); //Temporal de salida

                                        ret.Temporal = Reservada.nulo;
                                        cadC3D += ret.labelTrue + ":\n";   //True
                                        ret.Valor = "1";
                                        setStack(tmp, apunta + "", ret, id);
                                        cadC3D += "goto " + lbltmp + ";\n";
                                        cadC3D += ret.labelFalse + ":\n";   //False
                                        //ret.Temporal = Reservada.nulo;
                                        ret.Valor = "0";
                                        setStack(tmp, apunta + "", ret, id);
                                        cadC3D += lbltmp + ":\n";
                                        cima.addSimbolo(-1, apunta, Reservada.variable, id, ret.Valor, tipodato, Reservada.variable, getLinea(Nodo), getColumna(Nodo), true, null);
                                    }
                                    else
                                    {
                                        setStack(tmp, apunta + "", ret, id);
                                        cima.addSimbolo(-1, apunta, Reservada.variable, id, ret.Valor, tipodato, Reservada.variable, getLinea(Nodo), getColumna(Nodo), true, null);
                                    }
                                }
                                else
                                {
                                    Debug.WriteLine("Error Semantico-->Asignacion no valida, tipo de dato incorrecto linea:" + getLinea(Nodo) + " columna:" + getColumna(Nodo));
                                    lstError.Add(new Error(Reservada.ErrorSemantico, "Asignacion no valida, tipo de dato incorrecto", getLinea(Nodo), getColumna(Nodo)));
                                }
                            }
                            else
                            {
                                Debug.WriteLine("Error Semantico-->Asignacion no valida, expresion incorrecta linea:" + getLinea(Nodo) + " columna:" + getColumna(Nodo));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Asignacion no valida, expresion incorrecta", getLinea(Nodo), getColumna(Nodo)));
                            }
                        }
                        else
                        {
                            Debug.WriteLine("Error Semantico-->Variable ya existente linea:" + getLinea(Nodo) + " columna:" + getColumna(Nodo));
                            lstError.Add(new Error(Reservada.ErrorSemantico, "Variable ya existente", getLinea(Nodo), getColumna(Nodo)));
                        }

                        break;

                    case ",": //No hace nada
                        break;

                    default:
                        Debug.WriteLine("Error AST-->Nodo en funcion DeclaracionAsignacionData no existente/detectado");
                        break;
                }
            }
            else
                Debug.WriteLine("Error AST-->Nodo en funcion DeclaracionAsignacionData no existente/detectado/null");
        }

        //****************************************************** ENTORNOS INTERNOS ******************************************************

        private void EjecutarX()
        {
            Funciones funcion = tablafunciones.RetornarFuncion(Reservada.Program);

            if (funcion != null)
            {
                #region
                string l_return = getEtiqueta();

                TablaSimbolos program = new TablaSimbolos(1, Reservada.Program, "main", Reservada.nulo, false, false, l_return, Reservada.nulo); //Esto depende de si es VOID
                pilaSimbolos.Push(program);
                cima = program; //Estableciendo la tabla de simbolos cima
                nivelActual = 1; //Estableciendo el nivel actual

                RetornoAc retorno = Sentencias(funcion.getCuerpo());

                nivelActual--; //Disminuimos el nivel actual ya que salimos del metodo invocado
                pilaSimbolos.Pop(); //Eliminando la tabla de simbolos cima actual
                cima = pilaSimbolos.Peek(); //Estableciendo la nueva tabla de simbolo cima
                
                #endregion
            }
            else
            {
                Debug.WriteLine("Error Semantico-->Funcion no existente linea:" + funcion.getLinea() + " columna:" + funcion.getColumna());
                lstError.Add(new Error(Reservada.ErrorSemantico, "Funcion no existente", funcion.getLinea(), funcion.getColumna()));
            }
        }

        private RetornoAc Sentencias(ParseTreeNode Nodo)
        {
            //if (!isRetornoG)
            //{
            switch (Nodo.Term.Name)
            {
                case "SENTENCIAS":
                    foreach (ParseTreeNode hijo in Nodo.ChildNodes)
                    {
                        RetornoAc retorno = Sentencias(hijo); // SENTENCIA | SENTENCIAS

                        if (retorno.Retorna && cima.Retorna)
                        {
                            return retorno;
                        }
                        else if (retorno.Detener && cima.Detener)
                        {
                            return retorno;
                        }
                    }
                    break;
                case "SENTENCIA":
                    /*
                        SENTENCIA.Rule = ToTerm("write") + parentA + ASIGNAR_PARAMETRO + parentC + puntocoma
                            | ToTerm("writeln") + parentA + ASIGNAR_PARAMETRO + parentC + puntocoma
                            | ToTerm("while") + CONDICION + ToTerm("do") + ToTerm("begin") + SENTENCIAS + ToTerm("end") + puntocoma
                            | ToTerm("if") + CONDICION + ToTerm("then") + ToTerm("begin") + SENTENCIAS + ToTerm("end") + puntocoma
                            | ToTerm("if") + CONDICION + ToTerm("then") + ToTerm("begin") + SENTENCIAS + ToTerm("end") + ToTerm("else") + ToTerm("begin") + SENTENCIAS + ToTerm("end") + puntocoma
                            | ToTerm("for") + id + ToTerm(":=") + TERMINALES + ToTerm("to") + TERMINALES + ToTerm("do") + ToTerm("begin") + SENTENCIAS + ToTerm("end") + puntocoma
                            | ToTerm("continue") + puntocoma
                            | ToTerm("break") + puntocoma
                    */
                    #region
                    switch (Nodo.ChildNodes.Count)
                    {
                        case 2:
                            //ToTerm("continue") + puntocoma
                            //ToTerm("break") + puntocoma
                            #region
                            switch (Nodo.ChildNodes[0].Term.Name)
                            {
                                case "continue":
                                    if (cima.Continuar)
                                    {
                                        cadC3D += "goto " + cima.etiquetaContinue + "; //Salto Continue\n";
                                    }
                                    else
                                    {
                                        Console.WriteLine("Error Semantico-->Instruccion Continue no valida en ambito linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                                        lstError.Add(new Error(Reservada.ErrorSemantico, "Instruccion Continue no valida en ambito", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                                    }
                                    break;
                                case "break":

                                    if (cima.Detener)
                                    {
                                        cadC3D += "goto " + cima.etiquetaBreak + "; //Salto Break\n";
                                    }
                                    else
                                    {
                                        Console.WriteLine("Error Semantico-->Instruccion Break no valida en ambito linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                                        lstError.Add(new Error(Reservada.ErrorSemantico, "Instruccion Break no valida en ambito", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                                    }
                                    break;
                            }
                            #endregion
                            break;
                        case 4:
                            /*
                             * id + parentA + parentC + puntocoma
                             * id + ToTerm(":=") + CONDICION + puntocoma
                             * ToTerm("exit") + parentA + parentC + puntocoma
                             */
                            #region
                            if (Nodo.ChildNodes[0].Term.Name.Equals("id") && Nodo.ChildNodes[1].Term.Name.Equals(":="))
                            {
                                //id + ToTerm(":=") + CONDICION + puntocoma
                                #region
                                string id4 = Nodo.ChildNodes[0].Token.Value.ToString();
                                
                                Retorno ret = Condicion(Nodo.ChildNodes[2]);

                                if (id4.Equals(cima.Nombre))
                                {
                                    if (cima.Retorna) 
                                    { 
                                        if(ret != null)
                                        {
                                            if (!ret.Tipo.Equals(Reservada.Cadena))
                                            {
                                                setStack("SP", ret, "Return");
                                                cadC3D += "goto " + cima.etiquetaExit + ";\n";
                                            }
                                            else
                                            {
                                                //Cuando viene Exit('valor');
                                                int apHP = newApuntadorHP();
                                                string tmp1 = getTemp();
                                                stringToHeap(tmp1, apHP, ret.Valor);
                                                setStack("SP", new Retorno(tmp1, Reservada.nulo, ret.Tipo, ret.Valor, ret.Linea, ret.Columna), "Return");
                                                cadC3D += "goto " + cima.etiquetaExit + ";\n";
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine("Error Semantico-->Retono de expresion incorrecta linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                                            lstError.Add(new Error(Reservada.ErrorSemantico, "Retono de expresion incorrecta", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                                        }
                                    }
                                    else
                                    {
                                        Debug.WriteLine("Error Semantico-->Instruccion Exit no valida linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                                        lstError.Add(new Error(Reservada.ErrorSemantico, "Instruccion Exit no valida", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                                    }
                                }
                                else 
                                { 
                                    Simbolo var = RetornarSimbolo(id4); //Busco en mi nivel actual

                                    if (var == null) //Si no existe en mi nivel actual busco en las globales
                                    {
                                        var = cimaG.RetornarSimbolo(id4);
                                        //Debug.WriteLine(">>> Se busco en las globales <<<");
                                    }

                                    if (var != null) //Si la variable existe
                                    {
                                        if (ret != null)
                                        {
                                            if (ret.Tipo.Equals(var.Tipo)) //Si son del mismo tipo se pueden asignar (variable con variable)
                                            {
                                                if (var.ApuntadorAbsoluto == -1 && var.ApuntadorRelativo != -1) //Significa que esta en una funcion o procedimiento
                                                {
                                                    if (var.Tipo.Equals(Reservada.Booleano))
                                                    {
                                                        string lbltmp = getEtiqueta(); //Temporal de salida

                                                        string tmp = getTemp();
                                                        ret.Temporal = Reservada.nulo;
                                                        cadC3D += ret.labelTrue + ":\n";                       //True
                                                        ret.Valor = "1";
                                                        setStack(tmp, var.ApuntadorRelativo + "", ret, id4);
                                                        cadC3D += "goto" + lbltmp + ":\n";
                                                        cadC3D += ret.labelFalse + ":\n";                       //False
                                                        ret.Valor = "0";
                                                        setStack(tmp, var.ApuntadorRelativo + "", ret, id4);
                                                        cadC3D += lbltmp + ":\n";
                                                    }
                                                    else
                                                    {
                                                        string tmp = getTemp();
                                                        setStack(tmp, var.ApuntadorRelativo + "", ret, id4);
                                                    }
                                                }
                                                else //Significa que esta en el main
                                                {
                                                    if (var.Tipo.Equals(Reservada.Booleano))
                                                    {
                                                        var.Valor = "1"; // Asignamos el nuevo valor al id
                                                        setStack(var.ApuntadorAbsoluto + "", ret, id4);
                                                        cadC3D += "goto " + ret.labelFalse + ":\n";
                                                        var.Valor = "0";
                                                        setStack(var.ApuntadorAbsoluto + "", ret, id4);
                                                    }
                                                    else
                                                    {
                                                        var.Valor = ret.Valor; // Asignamos el nuevo valor al id
                                                        setStack(var.ApuntadorAbsoluto + "", ret, id4);
                                                    }
                                                }
                                            }
                                            #region ASIGNACION DE ARREGLO A ARREGLO
                                            /*
                                            else if (ret.Tipo.Equals(Reservada.arreglo) && var.TipoObjeto.Equals(Reservada.arreglo))
                                            {
                                                Simbolo arregloAsignar = RetornarSimbolo(ret.Valor); // ret.Valor contiene el nombre del arreglo a asignar

                                                if (arregloAsignar == null) //Si no existe en mi nivel actual busco en las globales
                                                {
                                                    arregloAsignar = cimaG.RetornarSimbolo(ret.Valor);
                                                    Debug.WriteLine(">>> Se busco en las globalbes <<<");
                                                }
                                                Debug.WriteLine(">>> SE RECONOCIO ASIGNACION DE ARREGLOS PRRONES <<<");

                                                if (arregloAsignar != null)
                                                {
                                                    Debug.WriteLine(">>> SE RECONOCIO ASIGNACION DE ARREGLOS PRRONES <<<");
                                                    Debug.WriteLine("Se asigno ARREGLO: " + id + " --> " + ret.Valor + " (" + ret.Tipo + ")");
                                                    if (arregloAsignar.Tipo.Equals(var.Tipo))
                                                    {
                                                        if (var.Arreglo.Count >= arregloAsignar.Arreglo.Count)
                                                        {
                                                            int i = 0;
                                                            foreach (Celda cel in arregloAsignar.Arreglo)
                                                            {
                                                                var.Arreglo.ElementAt(i).valor = arregloAsignar.Arreglo.ElementAt(i).valor;
                                                                i++;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            int i = 0;
                                                            foreach (Celda cel in var.Arreglo)
                                                            {
                                                                var.Arreglo.ElementAt(i).valor = arregloAsignar.Arreglo.ElementAt(i).valor;
                                                                i++;
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Debug.WriteLine("Error Semantico-->Asignacion no valida, tipo de dato incorrecto linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                                                        lstError.Add(new Error(Reservada.ErrorSemantico, "Asignacion no valida, tipo de dato incorrecto", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                                                    }
                                                }
                                                else
                                                {
                                                    Debug.WriteLine("Error Semantico-->Asignacion no valida de arreglo linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                                                    lstError.Add(new Error(Reservada.ErrorSemantico, "Asignacion no valida de arreglo", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                                                }
                                            }
                                            */
                                            #endregion
                                            else
                                            {
                                                Debug.WriteLine("Error Semantico-->Asignacion no valida, tipo de dato incorrecto linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                                                lstError.Add(new Error(Reservada.ErrorSemantico, "Asignacion no valida, tipo de dato incorrecto", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                                            }
                                        }
                                        else
                                        {
                                            Debug.WriteLine("Error Semantico-->Asignacion no valida, expresion incorrecta linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                                            lstError.Add(new Error(Reservada.ErrorSemantico, "Asignacion no valida, expresion incorrecta", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                                        }
                                    }
                                    else
                                    {
                                        Debug.WriteLine("Error Semantico-->Variable " + id4 + " no existente linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                                        lstError.Add(new Error(Reservada.ErrorSemantico, "Variable " + id4 + " no existente", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                                    }
                                }
                                #endregion
                            }
                            else if (Nodo.ChildNodes[0].Term.Name.Equals("id") && Nodo.ChildNodes[1].Term.Name.Equals("("))
                            {
                                //id + ToTerm(":=") + CONDICION + puntocoma
                                #region
                                String id3 = Nodo.ChildNodes[0].Token.Value.ToString();
                                Funciones func3 = tablafunciones.RetornarFuncion(id3);

                                if (func3 != null)
                                {
                                    string tmp = getTemp();
                                    int punt = newApuntador();
                                    cadC3D += "SP = SP + " + punt + ";\n";
                                    cadC3D += id3 + "();\n";
                                    string cd3d = getStack(tmp, "SP", "Return");
                                    cadC3D += "SP = SP - " + punt + ";\n";
                                    Apuntador--; //Disminuyo la posicion que aumente en SP
                                }
                                else
                                {
                                    Debug.WriteLine("Error Semantico-->Funcion no existente linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                                    lstError.Add(new Error(Reservada.ErrorSemantico, "Funcion no existente", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                                }
                                #endregion
                            }
                            else if (Nodo.ChildNodes[0].Term.Name.Equals("exit"))
                            {
                                //ToTerm("exit") + parentA + parentC + puntocoma

                                Debug.WriteLine("Error Semantico-->Instruccion Exit no valida linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Instruccion Exit no valida", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                            }
                            #endregion
                            break;
                        case 5: /*
                                 * ToTerm("repeat") + SENTENCIAS + ToTerm("until") + CONDICION + puntocoma
                                 * ToTerm("exit") + parentA + CONDICION + parentC + puntocoma
                                 * ToTerm("write") + parentA + ASIGNAR_PARAMETRO + parentC + puntocoma
                                 * ToTerm("writeln") + parentA + ASIGNAR_PARAMETRO + parentC + puntocoma
                                 * id + parentA + ASIGNAR_PARAMETRO + parentC + puntocoma
                                 * ToTerm("graficar_ts") + parentA + cadena + parentC + puntocoma
                                 */
                            #region
                            switch (Nodo.ChildNodes[0].Term.Name)
                            {
                                case "repeat":
                                    //ToTerm("repeat") + SENTENCIAS + ToTerm("until") + CONDICION + puntocoma
                                    #region
                                    cadC3D += "//==================== Inicio de repeat ==================\n";
                                    string lblBreak = getEtiqueta(); //Etiqueta de salto de break, tambien sirve para detener el ciclo while
                                    string lblContinue = getEtiqueta(); //Etiqueta de salto de continue

                                    string lblLoop = getEtiqueta(); //Etiqueta para mantener el ciclo repeat
                                    cadC3D += lblLoop + ":\n";

                                    TablaSimbolos dowhilee = new TablaSimbolos(nivelActual, cima.Tipo, Reservada.Repeat, Reservada.nulo, cima.Retorna, true, cima.etiquetaExit, lblBreak);
                                    dowhilee.etiquetaContinue = lblContinue;
                                    dowhilee.Continuar = true;
                                    pilaSimbolos.Push(dowhilee);
                                    cima = dowhilee; //Estableciendo la tabla de simbolos cima

                                    RetornoAc ret1 = Sentencias(Nodo.ChildNodes[1]); // Las sentencias se ejecutan al menos una vez en el Do-While

                                    pilaSimbolos.Pop(); //Eliminando la tabla de simbolos cima actual
                                    cima = pilaSimbolos.Peek(); //Estableciendo la nueva tabla de simbolo cima

                                    cadC3D += lblContinue + ":\n";      //Sirve para Continue y para que funcione el ciclo while
                                    Retorno condW = Condicion(Nodo.ChildNodes[3]);
                                    if (condW != null)
                                    {
                                        if (condW.Tipo.Equals(Reservada.Booleano)) // Si la condicion es booleana
                                        {
                                            evaluarCondicion(condW);
                                            cadC3D += condW.labelTrue + ":\n";                  //True
                                            cadC3D += "goto " + lblLoop + ";\n";
                                            cadC3D += condW.labelFalse + ": //End repeat\n";                 //False
                                            cadC3D += lblBreak + ": //Break\n";                 //Brake

                                            cadC3D += "//====================== Fin de repeat ===================\n";
                                        }
                                        else
                                        {
                                            Console.WriteLine("Valor de condicion invalida");
                                            Console.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[4]) + " columna:" + getColumna(Nodo.ChildNodes[4]));
                                            lstError.Add(new Error(Reservada.ErrorSemantico, "Valor de condicion invalida", getLinea(Nodo.ChildNodes[4]), getColumna(Nodo.ChildNodes[4])));
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Condicion invalida");
                                        Console.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[4]) + " columna:" + getColumna(Nodo.ChildNodes[4]));
                                        lstError.Add(new Error(Reservada.ErrorSemantico, "Condicion invalida", getLinea(Nodo.ChildNodes[4]), getColumna(Nodo.ChildNodes[4])));
                                    }
                                    #endregion
                                    break;
                                case "exit":
                                    #region
                                    Retorno retu = Condicion(Nodo.ChildNodes[2]);

                                    if (cima.Retorna) //Si la cima detecta que esta dentro del ambito de funcion
                                    {
                                        if (retu != null)
                                        {
                                            if (!retu.Tipo.Equals(Reservada.Cadena))
                                            {
                                                setStack("SP", retu, "Return");
                                                cadC3D += "goto " + cima.etiquetaExit + ";\n";
                                            }
                                            else
                                            {
                                                //Cuando viene Exit('valor');
                                                int apHP = newApuntadorHP();
                                                string tmp1 = getTemp();
                                                stringToHeap(tmp1, apHP, retu.Valor);
                                                setStack("SP", new Retorno(tmp1,Reservada.nulo,retu.Tipo,retu.Valor,retu.Linea,retu.Columna), "Return");
                                                cadC3D += "goto " + cima.etiquetaExit + ";\n";
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine("Error Semantico-->Retono de expresion incorrecta linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                                            lstError.Add(new Error(Reservada.ErrorSemantico, "Retono de expresion incorrecta", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                                        }
                                    }
                                    else
                                    {
                                        Debug.WriteLine("Error Semantico-->Instruccion Exit no valida linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                                        lstError.Add(new Error(Reservada.ErrorSemantico, "Instruccion Exit no valida", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                                    }
                                    #endregion
                                    break;
                                case "write":
                                    #region
                                    Retorno retWrite = getCadenaPrint(Nodo.ChildNodes[2]);

                                    if(retWrite != null)
                                    {
                                        //Form1.Impresiones.AppendText(retWrite.Valor);
                                        return new RetornoAc("-", "-", "0", "0");
                                    }
                                    else
                                    {
                                        Console.WriteLine("Error Semantico-->Impresion incorrecta linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                                        lstError.Add(new Error(Reservada.ErrorSemantico, "Impresion incorrecta", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                                    }
                                    #endregion
                                    break;
                                case "writeln":
                                    #region
                                    Retorno retWriteln = getCadenaPrint(Nodo.ChildNodes[2]);

                                    if(retWriteln != null)
                                    {
                                        cadC3D += ("printf(\"%c\", 10); //Backspace\n");

                                        //Form1.Impresiones.AppendText(retWriteln.Valor + "\n");
                                        return new RetornoAc("-", "-", "0", "0");
                                    }
                                    else
                                    {
                                        Console.WriteLine("Error Semantico-->Impresion incorrecta linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                                        lstError.Add(new Error(Reservada.ErrorSemantico, "Impresion incorrecta", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                                    }
                                    #endregion
                                    break;
                                case "id":
                                    #region
                                    // id + parentA + ASIGNAR_PARAMETRO + parentC + puntocoma
                                    //------------------------------------
                                    //PRIMERO OBTENGO LA CANTIDAD DE VALORES EN MIS PARAMETROS ACEPTADOS POR EL ARREGLO
                                    arreglo = new List<Celda>(); //Este arreglo es para almacenar los parametros del metodo que se invoco
                                    ValidarParametrosMetodo(Nodo.ChildNodes[2]); // Mandamos los parametros
                                    //AHORA BUSCO LA FUNCION EN BASE AL NOMBRE Y A MI ARREGLO DE PARAMETROS
                                    String id5 = Nodo.ChildNodes[0].Token.Value.ToString();
                                    //Funciones func5 = tablafunciones.RetornarFuncion(id5);
                                    Funciones func5 = tablafunciones.RetornarFuncionEvaluandoSobrecarga(id5, arreglo);
                                    //--------------------------------------

                                    if (func5 != null)
                                    {
                                        if (arreglo.Count == func5.getParametros().Count)
                                        {
                                            int punt = newApuntador(); //Guardo y reservo posicion del Return

                                            int cont = 0;
                                            string auxSH = getTemp();

                                            foreach (Parametro parametro in func5.getParametros())
                                            {
                                                if (parametro.Tipo.Equals(arreglo.ElementAt(cont).tipo))
                                                {
                                                    setStack(auxSH, newApuntador() + "", new Retorno(arreglo.ElementAt(cont).temporal, "", arreglo.ElementAt(cont).tipo, arreglo.ElementAt(cont).valor, parametro.Linea, parametro.Columna), parametro.Nombre);
                                                }
                                                else
                                                {
                                                    Console.WriteLine("Error Semantico-->Parametro introducido de tipo incompatible linea:" + parametro.Linea + " columna:" + parametro.Columna);
                                                    lstError.Add(new Error(Reservada.ErrorSemantico, "Parametro introducido de tipo incompatible", parametro.Linea, parametro.Columna));
                                                }
                                                cont++;
                                            }

                                            string tmp = getTemp();

                                            cadC3D += "SP = SP + " + punt + ";\n";
                                            cadC3D += id5 + "();\n";
                                            string cd3d = getStack(tmp, "SP", "Return");
                                            cadC3D += "SP = SP - " + punt + ";\n";
                                            Apuntador--; //Disminuyo la posicion que aumente en SP
                                        }
                                        else
                                        {
                                            Console.WriteLine("Error Semantico-->Cantidad de parametros introducidos no valida linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                                            lstError.Add(new Error(Reservada.ErrorSemantico, "Cantidad de parametros introducidos no valida", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Error Semantico-->Funcion no existente linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                                        lstError.Add(new Error(Reservada.ErrorSemantico, "Funcion no existente", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                                    }
                                    #endregion
                                    break;
                                case "graficar_ts":
                                    //ToTerm("graficar_ts") + parentA + cadena + parentC + puntocoma
                                    #region
                                    try
                                    {
                                        string nombre = Nodo.ChildNodes[2].Token.Value.ToString();
                                        graficarts = new GraficarTS(nombre);

                                        foreach (TablaSimbolos ts in pilaSimbolos)
                                        {
                                            foreach(Simbolo sim in ts.ts)
                                            {
                                                if(sim.ApuntadorAbsoluto != -1)
                                                {
                                                    graficarts.addSimbolo(ts.Tipo, ts.Nombre, sim.Nombre, sim.ApuntadorAbsoluto + "");
                                                }
                                                else
                                                {
                                                    graficarts.addSimbolo(ts.Tipo, ts.Nombre, sim.Nombre, sim.ApuntadorRelativo + "");
                                                }
                                            }
                                        }
                                        graficarts.Graficar();
                                    }
                                    catch(Exception e)
                                    {
                                        Debug.WriteLine("**Error, Generacion de Tabla de Simbolos");
                                    }
                                    #endregion
                                    break;
                            }
                            #endregion
                            break;
                        case 7:
                            //ToTerm("while") + CONDICION + ToTerm("do") + ToTerm("begin") + SENTENCIAS + ToTerm("end") + puntocoma
                            //ToTerm("if") + CONDICION + ToTerm("then") + ToTerm("begin") + SENTENCIAS + ToTerm("end") + puntocoma
                            #region
                            switch (Nodo.ChildNodes[0].Term.Name)
                            {
                                case "if":
                                    #region
                                    cadC3D += "//====================== Inicio de if ===================\n";
                                    Retorno cond8 = Condicion(Nodo.ChildNodes[1]);
                                    if (cond8 != null)
                                    {
                                        if (cond8.Tipo.Equals(Reservada.Booleano)) // Si la condicion es booleana
                                        {
                                            TablaSimbolos iff = new TablaSimbolos(nivelActual, cima.Tipo, Reservada.Iff, Reservada.nulo, cima.Retorna, cima.Detener, cima.etiquetaExit, cima.etiquetaBreak);
                                            iff.Continuar = cima.Continuar;
                                            iff.etiquetaContinue = cima.etiquetaContinue;
                                            pilaSimbolos.Push(iff);
                                            cima = iff; //Estableciendo la tabla de simbolos cima
                                            evaluarCondicion(cond8);
                                            cadC3D += cond8.labelTrue + ":\n";                  //True
                                            RetornoAc ret1 = Sentencias(Nodo.ChildNodes[4]);    //Ejecutando sentencias del If
                                            cadC3D += cond8.labelFalse + ":\n";                 //False

                                            pilaSimbolos.Pop(); //Eliminando la tabla de simbolos cima actual
                                            cima = pilaSimbolos.Peek(); //Estableciendo la nueva tabla de simbolo cima
                                            
                                            cadC3D += "//======================= Fin de if =====================\n";
                                        }
                                        else
                                        {
                                            Console.WriteLine("Valor de condicion invalida");
                                            Console.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                                            lstError.Add(new Error(Reservada.ErrorSemantico, "Valor de condicion invalida", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Condicion invalida");
                                        Console.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                                        lstError.Add(new Error(Reservada.ErrorSemantico, "Condicion invalida", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                                    }
                                    #endregion
                                    break;
                                case "while":
                                    #region
                                    cadC3D += "//==================== Inicio de while ===================\n";
                                    string lblContinue = getEtiqueta(); //Etiqueta de salto de continue, tambien sirve para mantener el ciclo while
                                    cadC3D += lblContinue + ":\n";      //Sirve para Continue y para que funcione el ciclo while

                                    Retorno cond7 = Condicion(Nodo.ChildNodes[1]);

                                    if (cond7 != null)
                                    {
                                        if (cond7.Tipo.Equals(Reservada.Booleano)) // Si la condicion es booleana
                                        {
                                            //string lblBreak = getEtiqueta(); //Etiqueta de salto de break, tambien para el final de while
                                            
                                            TablaSimbolos whilee = new TablaSimbolos(nivelActual, cima.Tipo, Reservada.Whilee, Reservada.nulo, cima.Retorna, true, cima.etiquetaExit, cond7.labelFalse);
                                            whilee.etiquetaContinue = lblContinue;
                                            whilee.Continuar = true;
                                            pilaSimbolos.Push(whilee);
                                            cima = whilee; //Estableciendo la tabla de simbolos cima
                                            
                                            evaluarCondicion(cond7);
                                            cadC3D += cond7.labelTrue + ":\n";                  //True

                                            RetornoAc ret1 = Sentencias(Nodo.ChildNodes[4]);

                                            cadC3D += "goto " + lblContinue + ";\n";                      //Enciclado de while
                                            cadC3D += cond7.labelFalse + ": //End while\n";                 //False

                                            cadC3D += "//===================== Fin de while =====================\n";

                                            pilaSimbolos.Pop(); //Eliminando la tabla de simbolos cima actual
                                            cima = pilaSimbolos.Peek(); //Estableciendo la nueva tabla de simbolo cima
                                        }
                                        else
                                        {
                                            Console.WriteLine("Valor de condicion invalida");
                                            Console.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                                            lstError.Add(new Error(Reservada.ErrorSemantico, "Valor de condicion invalida", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Condicion invalida");
                                        Console.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                                        lstError.Add(new Error(Reservada.ErrorSemantico, "Condicion invalida", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                                    }
                                    #endregion
                                    break;
                            }
                            #endregion
                            break;
                        case 11:
                            //ToTerm("for") + id + ToTerm(":=") + TERMINALES + ToTerm("to") + TERMINALES + ToTerm("do") + ToTerm("begin") + SENTENCIAS + ToTerm("end") + puntocoma
                            //ToTerm("if") + CONDICION + ToTerm("then") + ToTerm("begin") + SENTENCIAS + ToTerm("end") + ToTerm("else") + ToTerm("begin") + SENTENCIAS + ToTerm("end") + puntocoma
                            //ToTerm("case") + TERMINALES + ToTerm("of") + LSTCASE + ToTerm("else") + ToTerm("begin") + SENTENCIAS + ToTerm("end") + puntocoma + ToTerm("end") + puntocoma
                            #region
                            switch (Nodo.ChildNodes[0].Term.Name)
                            {
                                case "for":
                                    //ToTerm("for") + id + ToTerm(":=") + TERMINALES + ToTerm("to") + TERMINALES + ToTerm("do") + ToTerm("begin") + SENTENCIAS + ToTerm("end") + puntocoma
                                    #region
                                    cadC3D += "//===================== Inicio de for ====================\n";
                                    
                                    String id15 = Nodo.ChildNodes[1].Token.Value.ToString();
                                    Simbolo var15 = RetornarSimbolo(id15);

                                    if (var15 == null) //Si no existe en mi nivel actual busco en las globales
                                    {
                                        var15 = cimaG.RetornarSimbolo(id15);
                                        //Debug.WriteLine(">>> Se busco en las globales <<<");
                                    }

                                    if (var15 != null) //Si mi asignacion de variable es distinta de null
                                    {
                                        Retorno ret15 = Terminales(Nodo.ChildNodes[3]);
                                        Retorno condicional = Terminales(Nodo.ChildNodes[5]);

                                        if (ret15.Tipo.Equals(var15.Tipo) && (ret15.Tipo.Equals(Reservada.Real) || ret15.Tipo.Equals(Reservada.Entero))) //Si son del mismo tipo se pueden asignar (variable con expresion)
                                        {
                                            //var15.Valor = ret15.Valor; jsfdlkjadsfklsafjf  // Asignamos el nuevo valor a la variable
                                            if (var15.ApuntadorAbsoluto == -1 && var15.ApuntadorRelativo != -1) //Significa que esta en una funcion o procedimiento
                                            {
                                                string tmp = getTemp();
                                                setStack(tmp, var15.ApuntadorRelativo + "", ret15, id15);
                                            }
                                            else //Significa que esta en el main
                                            {
                                                var15.Valor = ret15.Valor; // Asignamos el nuevo valor al id
                                                setStack(var15.ApuntadorAbsoluto + "", ret15, id15);
                                            }
                                            
                                            if (condicional.Tipo.Equals(Reservada.Entero) || condicional.Tipo.Equals(Reservada.Real))
                                            {
                                                //string lblBreak = getEtiqueta(); //Etiqueta de salto de break, tambien sirve para detener el ciclo while
                                                string lblContinue = getEtiqueta(); //Etiqueta de salto de continue
                                                
                                                //Etiquetas de condicion del for
                                                string lblTrue = getEtiqueta();
                                                string lblFalse = getEtiqueta();

                                                cadC3D += lblContinue + ":\n";

                                                TablaSimbolos forr = new TablaSimbolos(nivelActual, cima.Tipo, Reservada.Forr, Reservada.nulo, cima.Retorna, true, cima.etiquetaExit, lblFalse);
                                                forr.etiquetaContinue = lblContinue;
                                                forr.Continuar = true;
                                                pilaSimbolos.Push(forr);
                                                cima = forr; //Estableciendo la tabla de simbolos cima

                                                string tmp = getTemp();
                                                string cd3d = "";
                                                if (var15.ApuntadorAbsoluto == -1 && var15.ApuntadorRelativo != -1) //Significa que esta en una funcion o procedimiento
                                                {
                                                    string tmp2 = getTemp();
                                                    cadC3D += tmp2 + " = SP + " + var15.ApuntadorRelativo + ";\n";
                                                    cd3d = getStack(tmp, tmp2, id15);
                                                }
                                                else //Significa que esta en el main
                                                {
                                                    cd3d = getStack(tmp, var15.ApuntadorAbsoluto, id15);
                                                }

                                                cadC3D += "//==================== if condicional ====================\n";
                                                
                                                cadC3D += "if(" + tmp + " <= " + condicional.Valor + ") goto " + lblTrue + ";\n";
                                                cadC3D += "goto " + lblFalse + ";\n";

                                                cadC3D += "//================== fin if condicional ==================\n";
                                                cadC3D += lblTrue + ":\n";

                                                RetornoAc ret1 = Sentencias(Nodo.ChildNodes[8]); //Sentencias

                                                //Simbolo inc = IncrementoFor(var15); //Ejecuta operacion incrementa/decremento
                                                string auxtmp = getTemp();
                                                cadC3D += auxtmp + " = " + auxtmp + " + 1; //Incrementando\n";

                                                ret15.Temporal = auxtmp;
                                                if (var15.ApuntadorAbsoluto == -1 && var15.ApuntadorRelativo != -1) //Significa que esta en una funcion o procedimiento
                                                {
                                                    string tmp2 = getTemp();
                                                    setStack(tmp2, var15.ApuntadorRelativo + "", ret15, id15);
                                                }
                                                else //Significa que esta en el main
                                                {
                                                    var15.Valor = ret15.Valor; // Asignamos el nuevo valor al id
                                                    setStack(var15.ApuntadorAbsoluto + "", ret15, id15);
                                                }

                                                cadC3D += "goto " + lblContinue + ";\n";
                                                cadC3D += lblFalse + ":\n";

                                                pilaSimbolos.Pop(); //Eliminando la tabla de simbolos cima actual
                                                cima = pilaSimbolos.Peek(); //Estableciendo la nueva tabla de simbolo cima

                                                cadC3D += "//====================== Fin de for ======================\n";
                                            }
                                            else
                                            {
                                                Console.WriteLine("Error Semantico-->Tipo de condicional incorrecta linea:" + getLinea(Nodo.ChildNodes[4]) + " columna:" + getColumna(Nodo.ChildNodes[4]));
                                                lstError.Add(new Error(Reservada.ErrorSemantico, "Tipo de condicional incorrecta", getLinea(Nodo.ChildNodes[4]), getColumna(Nodo.ChildNodes[4])));
                                            }
                                        }
                                        else
                                        {
                                            Console.WriteLine("Error Semantico-->Asignacion no valida, tipo de dato incorrecto linea:" + getLinea(Nodo.ChildNodes[2]) + " columna:" + getColumna(Nodo.ChildNodes[2]));
                                            lstError.Add(new Error(Reservada.ErrorSemantico, "Asignacion no valida, tipo de dato incorrecto", getLinea(Nodo.ChildNodes[2]), getColumna(Nodo.ChildNodes[2])));
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Error Semantico-->Variable no existente linea:" + getLinea(Nodo.ChildNodes[2]) + " columna:" + getColumna(Nodo.ChildNodes[2]));
                                        lstError.Add(new Error(Reservada.ErrorSemantico, "Variable no existente incorrecta", getLinea(Nodo.ChildNodes[2]), getColumna(Nodo.ChildNodes[2])));
                                    }
                                    #endregion
                                    break;
                                case "if":
                                    //ToTerm("if") + CONDICION + ToTerm("then") + ToTerm("begin") + SENTENCIAS + ToTerm("end") + ToTerm("else") + ToTerm("begin") + SENTENCIAS + ToTerm("end") + puntocoma
                                    #region
                                    cadC3D += "//===================== Inicio de if =====================\n";

                                    Retorno cond11 = Condicion(Nodo.ChildNodes[1]);

                                    if (cond11 != null)
                                    {
                                        if (cond11.Tipo.Equals(Reservada.Booleano)) // Si la condicion es booleana
                                        {
                                            TablaSimbolos iff = new TablaSimbolos(nivelActual, cima.Tipo, Reservada.Iff, Reservada.nulo, cima.Retorna, cima.Detener, cima.etiquetaExit, cima.etiquetaBreak);
                                            iff.Continuar = cima.Continuar;
                                            iff.etiquetaContinue = cima.etiquetaContinue;
                                            pilaSimbolos.Push(iff);
                                            cima = iff; //Estableciendo la tabla de simbolos cima
                                            
                                            string lblFin = getEtiqueta(); //Temporal para finalizar todo el if
                                            evaluarCondicion(cond11);
                                            cadC3D += cond11.labelTrue + ":\n";                 //True
                                            RetornoAc ret1 = Sentencias(Nodo.ChildNodes[4]);    //Ejecutando sentencias del If
                                            cadC3D += "goto " + lblFin + ";\n";                 //Etiqueta salto fin
                                            cadC3D += cond11.labelFalse + ":\n";                //False
                                            ret1 = Sentencias(Nodo.ChildNodes[8]);              //Ejecutando sentencias del Else
                                            cadC3D += lblFin + ":\n";                           //Etiqueta salto fin

                                            cadC3D += "//====================== Fin de if =======================\n";

                                            pilaSimbolos.Pop(); //Eliminando la tabla de simbolos cima actual
                                            cima = pilaSimbolos.Peek(); //Estableciendo la nueva tabla de simbolo cima
                                        }
                                        else
                                        {
                                            Console.WriteLine("Valor de condicion invalida");
                                            Console.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                                            lstError.Add(new Error(Reservada.ErrorSemantico, "Valor de condicion invalida", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Condicion invalida");
                                        Console.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                                        lstError.Add(new Error(Reservada.ErrorSemantico, "Condicion invalida", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                                    }
                                    #endregion
                                    break;
                                case "case":
                                    #region
                                    cadC3D += "//===================== Inicio de case ==================\n";

                                    Retorno cond = Terminales(Nodo.ChildNodes[1]);

                                    if (cond != null)
                                    {
                                        if (cond.Tipo.Equals(Reservada.Entero))
                                        {
                                            Casos(Nodo.ChildNodes[3], cond);

                                            string lblFalse = getEtiqueta();

                                            TablaSimbolos casse = new TablaSimbolos(nivelActual, cima.Tipo, Reservada.Iff, Reservada.nulo, cima.Retorna, cima.Detener, cima.etiquetaExit, lblFalse);
                                            //casse.Continuar = cima.Continuar;
                                            //casse.etiquetaContinue = cima.etiquetaContinue;
                                            pilaSimbolos.Push(casse);
                                            cima = casse; //Estableciendo la tabla de simbolos cima

                                            cadC3D += "//Else-case\n";
                                            Sentencias(Nodo.ChildNodes[6]);
                                            cadC3D += lblFalse + ":\n";

                                            pilaSimbolos.Pop(); //Eliminando la tabla de simbolos cima actual
                                            cima = pilaSimbolos.Peek(); //Estableciendo la nueva tabla de simbolo cima

                                        }
                                        else
                                        {
                                            Console.WriteLine("Error Semantico-->Parametro debe ser numerico linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                                            lstError.Add(new Error(Reservada.ErrorSemantico, "Parametro debe ser numerico", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Error Semantico-->Parametro de Case invalida linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                                        lstError.Add(new Error(Reservada.ErrorSemantico, "Parametro de Case invalida", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                                    }

                                    cadC3D += "//====================== Fin de case ====================\n";
                                    #endregion
                                    break;
                            }
                            #endregion
                            break;
                    }
                    #endregion
                    break;
            }
            return new RetornoAc("-", "-", "0", "0");
        }

        private Retorno Condicion(ParseTreeNode Nodo)
        {
            /*
            CONDICION.Rule = CONDICION + ToTerm("and") + COND1
                            | COND1
            COND1.Rule = COND1 + ToTerm("or") + COND2
                        | COND2
            COND2.Rule = ToTerm("not") + COND3
                        | COND3
            COND3.Rule = COND3 + ToTerm("<=") + COND4
                        | COND4
            COND4.Rule = COND4 + ToTerm(">=") + COND5
                        | COND5
            COND5.Rule = COND5 + ToTerm("<") + COND6
                        | COND6
            COND6.Rule = COND6 + ToTerm(">") + COND7
                        | COND7
            COND7.Rule = COND7 + ToTerm("=") + COND8
                        | COND8
            COND8.Rule = COND8 + ToTerm("<>") + EXPRESION
                        | EXPRESION
            */
            if (Nodo.ChildNodes.Count == 3)
            {
                switch (Nodo.ChildNodes[0].Term.Name)
                {
                    case "CONDICION": // MakePlusRule(CONDICION, ToTerm("and"), COND1);
                        #region
                        Retorno condB1 = Condicion(Nodo.ChildNodes[0]);
                        Retorno condB2 = Condicion(Nodo.ChildNodes[2]);

                        if ((condB1 != null) && (condB2 != null)) // Si ambos son distintos de null entra
                        {
                            if (condB1.Tipo.Equals(Reservada.Booleano) && condB2.Tipo.Equals(Reservada.Booleano)) // si ambos son booleanos
                            {
                                Retorno re = new Retorno(Reservada.control, Reservada.State_And, Reservada.Booleano, Reservada.nulo, getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1]));

                                if (!condB1.Temporal.Equals(Reservada.control) && !condB2.Temporal.Equals(Reservada.control))
                                {
                                    cadC3D += "//Cuando No viene OR/AND en los nodos\n";
                                    cadC3D += condB1.ifC3D + " goto " + condB1.labelTrue + ";\n";
                                    cadC3D += "goto " + condB1.labelFalse + ";\n";                  //FALSE = condB1.labelFalse
                                    cadC3D += condB1.labelTrue + ":\n";
                                    cadC3D += condB2.ifC3D + " goto " + condB2.labelTrue + ";\n";
                                    cadC3D += "goto " + condB1.labelFalse + ";\n";                  //FALSE = condB1.labelFalse
                                    //cadC3D += condB2.labelTrue + ":\n";
                                    re.labelTrue = condB2.labelTrue;
                                    re.labelFalse = condB1.labelFalse;                              //FALSE = condB1.labelFalse
                                }
                                else if (condB1.Temporal.Equals(Reservada.control) && !condB2.Temporal.Equals(Reservada.control))
                                {
                                    //Cuando viene OR/AND en el nodo izquierdo
                                    if (condB1.C3D.Equals(Reservada.State_Or)) /*El if tiene el mismo codigo que el else*/
                                    {
                                        cadC3D += "//AND - Cuando viene un OR en nodo izquierdo\n";
                                        cadC3D += condB1.labelTrue + ":\n";
                                        cadC3D += condB2.ifC3D + " goto " + condB2.labelTrue + ";\n";
                                        cadC3D += "goto " + condB1.labelFalse + ";\n";
                                        re.labelTrue = condB2.labelTrue;
                                        re.labelFalse = condB1.labelFalse;
                                    }
                                    else
                                    {
                                        cadC3D += "//AND - Cuando viene un AND en nodo izquierdo\n";
                                        cadC3D += condB1.labelTrue + ":\n";
                                        cadC3D += condB2.ifC3D + " goto " + condB2.labelTrue + ";\n";
                                        cadC3D += "goto " + condB1.labelFalse + ";\n";
                                        re.labelTrue = condB2.labelTrue;
                                        re.labelFalse = condB1.labelFalse;
                                    }
                                }
                                else if (!condB1.Temporal.Equals(Reservada.control) && condB2.Temporal.Equals(Reservada.control))
                                {
                                    //Cuando viene OR/AND en nodo derecho
                                    cadC3D += "//AND - Cuando viene un OR en nodo derecho\n";
                                    //cadC3D += condB1.labelTrue + ":\n";
                                    cadC3D += condB1.ifC3D + " goto " + condB2.labelTrue + ";\n";
                                    cadC3D += "goto " + condB1.labelFalse + ";\n";
                                    cadC3D += condB1.labelFalse + ":\n";
                                    re.labelTrue = condB2.labelTrue;
                                    re.labelFalse = condB1.labelFalse;
                                }
                                else
                                {
                                    //Cuando viene OR/AND en ambos nodos
                                    cadC3D += "//**Error, codigo detectado invalido en AND :V\n";
                                    cadC3D += "//**Error, cuando viene OR/AND en ambos nodos\n";
                                    
                                    cadC3D += condB1.labelFalse + ":\n";
                                    re.labelTrue = condB1.labelTrue + "," + condB2.labelTrue;
                                    re.labelFalse = condB2.labelFalse;
                                }
                                return re;
                            }
                            else
                            {
                                Debug.WriteLine("Imposible evaluar condicion AND con valores no booleanos");
                                Debug.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Imposible evaluar condicion AND con valores no booleanos", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                                return null;
                            }
                        }
                        else
                        {
                            Debug.WriteLine("Imposible evaluar condicion AND");
                            Debug.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                            lstError.Add(new Error(Reservada.ErrorSemantico, "Imposible evaluar condicion AND", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                            return null;
                        }
                        #endregion

                    case "COND1": // MakePlusRule(COND1, ToTerm("or"), COND2);
                        #region
                        Retorno condA1 = Condicion(Nodo.ChildNodes[0]);
                        Retorno condA2 = Condicion(Nodo.ChildNodes[2]);

                        if ((condA1 != null) && (condA2 != null)) // Si ambos son distintos de null entra
                        {
                            if (condA1.Tipo.Equals(Reservada.Booleano) && condA2.Tipo.Equals(Reservada.Booleano)) // si ambos son booleanos
                            {
                                Retorno re = new Retorno(Reservada.control, Reservada.State_Or, Reservada.Booleano, Reservada.nulo, getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1]));

                                if (!condA1.Temporal.Equals(Reservada.control) && !condA2.Temporal.Equals(Reservada.control))
                                {
                                    cadC3D += "//Cuando No viene OR/AND en los nodos\n";
                                    cadC3D += condA1.ifC3D + " goto " + condA1.labelTrue + ";\n";   //TRUE = condA1.labelTrue
                                    cadC3D += "goto " + condA1.labelFalse + ";\n";
                                    cadC3D += condA1.labelFalse + ":\n";
                                    cadC3D += condA2.ifC3D + " goto " + condA1.labelTrue + ";\n";   //TRUE = condA1.labelTrue
                                    cadC3D += "goto " + condA2.labelFalse + ";\n";
                                    //cadC3D += condA1.labelTrue + ":\n";                           //TRUE = condA1.labelTrue
                                    re.labelTrue = condA1.labelTrue;                                //TRUE = condA1.labelTrue
                                    re.labelFalse = condA2.labelFalse;
                                }
                                else if (condA1.Temporal.Equals(Reservada.control) && !condA2.Temporal.Equals(Reservada.control))
                                {
                                    //Cuando viene OR/AND en el nodo izquierdo
                                    if (condA1.C3D.Equals(Reservada.State_Or)) /*El if tiene el mismo codigo que el else*/
                                    {
                                        cadC3D += "//OR - Cuando viene un OR en nodo izquierdo\n";
                                        cadC3D += condA1.labelFalse + ":\n";
                                        cadC3D += condA2.ifC3D + " goto " + condA1.labelTrue + ";\n";
                                        cadC3D += "goto " + condA2.labelFalse + ";\n";
                                        re.labelTrue = condA1.labelTrue;
                                        re.labelFalse = condA2.labelFalse;
                                    }
                                    else
                                    {
                                        cadC3D += "//OR - Cuando viene un AND en nodo izquierdo\n";
                                        cadC3D += condA1.labelFalse + ":\n";
                                        cadC3D += condA2.ifC3D + " goto " + condA1.labelTrue + ";\n";
                                        cadC3D += "goto " + condA2.labelFalse + ";\n";
                                        re.labelTrue = condA1.labelTrue;
                                        re.labelFalse = condA2.labelFalse;
                                    }
                                }
                                else if (!condA1.Temporal.Equals(Reservada.control) && condA2.Temporal.Equals(Reservada.control))
                                {
                                    //Cuando viene OR/AND en nodo derecho
                                    cadC3D += "//OR - Cuando viene un OR en nodo derecho\n";
                                    //cadC3D += condA1.labelFalse + ":\n";
                                    cadC3D += condA1.ifC3D + " goto " + condA2.labelTrue + ";\n";
                                    cadC3D += "goto " + condA1.labelFalse + ";\n";
                                    cadC3D += condA1.labelFalse + ":\n";
                                    re.labelTrue = condA2.labelTrue;
                                    re.labelFalse = condA1.labelFalse;
                                }
                                else
                                {
                                    //Cuando viene OR/AND en ambos nodos
                                    cadC3D += "//**Error, codigo detectado invalido en OR :V\n";
                                    cadC3D += "//**Error, cuando viene OR/AND en ambos nodos\n";
                                    
                                    cadC3D += condA1.labelFalse + ":\n";
                                    re.labelTrue = condA1.labelTrue + ": " +condA2.labelTrue;
                                    re.labelFalse = condA2.labelFalse;
                                }
                                return re;
                            }
                            else
                            {
                                Debug.WriteLine("Imposible evaluar condicion OR con valores no booleanos");
                                Debug.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Imposible evaluar condicion OR con valores no booleanos", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                                return null;
                            }
                        }
                        else
                        {
                            Debug.WriteLine("Imposible evaluar condicion OR");
                            Debug.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                            lstError.Add(new Error(Reservada.ErrorSemantico, "Imposible evaluar condicion OR", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                            return null;
                        }
                        #endregion

                    case "COND3": // MakePlusRule(COND3, ToTerm("<="), COND4);
                        #region
                        Retorno condC1 = Condicion(Nodo.ChildNodes[0]);
                        Retorno condC2 = Condicion(Nodo.ChildNodes[2]);

                        if ((condC1 != null) && (condC2 != null)) // Si ambos son distintos de null entra
                        {
                            if ((condC1.Tipo.Equals(Reservada.Entero) && condC2.Tipo.Equals(Reservada.Real)) ||  // si uno es entero y otro real
                                     (condC1.Tipo.Equals(Reservada.Real) && condC2.Tipo.Equals(Reservada.Entero)) ||   // si uno es real y otro entero
                                         (condC1.Tipo.Equals(Reservada.Entero) && condC2.Tipo.Equals(Reservada.Entero)) ||     // si ambos son enteros
                                             (condC1.Tipo.Equals(Reservada.Real) && condC2.Tipo.Equals(Reservada.Real)))   // si ambos son real
                            {
                                string labelT = getEtiqueta();
                                string labelF = getEtiqueta();

                                Retorno re = new Retorno(Reservada.nulo, Reservada.nulo, Reservada.Booleano, Reservada.nulo, getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1]));
                                re.labelTrue = labelT;
                                re.labelFalse = labelF;
                                re.ifC3D = labelC3D(condC1, condC2, "<=");
                                return re;
                            }
                            else if ((condC1.Tipo.Equals(Reservada.Cadena) && condC2.Tipo.Equals(Reservada.Cadena)))    //Si ambos son String
                            {
                                string labelT = getEtiqueta();
                                string labelF = getEtiqueta();

                                Retorno re = new Retorno(Reservada.nulo, Reservada.nulo, Reservada.Booleano, Reservada.nulo, getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1]));
                                re.labelTrue = labelT;
                                re.labelFalse = labelF;
                                re.ifC3D = labelC3D(condC1, condC2, "<=");
                                return re;
                            }
                            else // valores no numericos
                            {
                                Debug.WriteLine("Imposible evaluar condicion <= con valores diferentes");
                                Debug.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Imposible evaluar condicion <= con valores diferentes", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                                return null;
                            }
                        }
                        else
                        {
                            Debug.WriteLine("Imposible evaluar condicion <=");
                            Debug.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                            lstError.Add(new Error(Reservada.ErrorSemantico, "Imposible evaluar condicion <=", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                            return null;
                        }
                        #endregion

                    case "COND4": // MakePlusRule(COND4, ToTerm(">="), COND5);
                        #region
                        Retorno condD1 = Condicion(Nodo.ChildNodes[0]);
                        Retorno condD2 = Condicion(Nodo.ChildNodes[2]);

                        if ((condD1 != null) && (condD2 != null)) // Si ambos son distintos de null entra
                        {
                            if ((condD1.Tipo.Equals(Reservada.Entero) && condD2.Tipo.Equals(Reservada.Real)) ||  // si uno es entero y otro real
                                     (condD1.Tipo.Equals(Reservada.Real) && condD2.Tipo.Equals(Reservada.Entero)) ||   // si uno es real y otro entero
                                         (condD1.Tipo.Equals(Reservada.Entero) && condD2.Tipo.Equals(Reservada.Entero)) ||     // si ambos son enteros
                                             (condD1.Tipo.Equals(Reservada.Real) && condD2.Tipo.Equals(Reservada.Real)))   // si ambos son real
                            {
                                string labelT = getEtiqueta();
                                string labelF = getEtiqueta();

                                Retorno re = new Retorno(Reservada.nulo, Reservada.nulo, Reservada.Booleano, Reservada.nulo, getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1]));
                                re.labelTrue = labelT;
                                re.labelFalse = labelF;
                                re.ifC3D = labelC3D(condD1, condD2, ">=");
                                return re;
                            }
                            else if ((condD1.Tipo.Equals(Reservada.Cadena) && condD2.Tipo.Equals(Reservada.Cadena)))     //Si ambos son String
                            {
                                string labelT = getEtiqueta();
                                string labelF = getEtiqueta();

                                Retorno re = new Retorno(Reservada.nulo, Reservada.nulo, Reservada.Booleano, Reservada.nulo, getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1]));
                                re.labelTrue = labelT;
                                re.labelFalse = labelF;
                                re.ifC3D = labelC3D(condD1, condD2, ">=");
                                return re;
                            }
                            else // valores no numericos
                            {
                                Debug.WriteLine("Imposible evaluar condicion >= con valores diferentes");
                                Debug.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Imposible evaluar condicion >= con valores diferentes", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                                return null;
                            }
                        }
                        else
                        {
                            Debug.WriteLine("Imposible evaluar condicion >=");
                            Debug.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                            lstError.Add(new Error(Reservada.ErrorSemantico, "Imposible evaluar condicion >=", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                            return null;
                        }
                    #endregion

                    case "COND5": // MakePlusRule(COND5, ToTerm("<"), COND6);
                        #region
                        Retorno condE1 = Condicion(Nodo.ChildNodes[0]);//COND6
                        Retorno condE2 = Condicion(Nodo.ChildNodes[2]);//COND7

                        if ((condE1 != null) && (condE2 != null)) // Si ambos son distintos de null entra
                        {
                            if ((condE1.Tipo.Equals(Reservada.Entero) && condE2.Tipo.Equals(Reservada.Real)) ||  // si uno es Entero y otro Real
                                     (condE1.Tipo.Equals(Reservada.Real) && condE2.Tipo.Equals(Reservada.Entero)) ||   // si uno es Real y otro Entero
                                         (condE1.Tipo.Equals(Reservada.Entero) && condE2.Tipo.Equals(Reservada.Entero)) ||     // si ambos son Enteros
                                             (condE1.Tipo.Equals(Reservada.Real) && condE2.Tipo.Equals(Reservada.Real)))   // si ambos son Real
                            {
                                string labelT = getEtiqueta();
                                string labelF = getEtiqueta();

                                Retorno re = new Retorno(Reservada.nulo, Reservada.nulo, Reservada.Booleano, Reservada.nulo, getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1]));
                                re.labelTrue = labelT;
                                re.labelFalse = labelF;
                                re.ifC3D = labelC3D(condE1, condE2, "<");
                                return re;
                            }
                            else if ((condE1.Tipo.Equals(Reservada.Cadena) && condE2.Tipo.Equals(Reservada.Cadena)))     //Si ambos son String
                            {
                                string labelT = getEtiqueta();
                                string labelF = getEtiqueta();

                                Retorno re = new Retorno(Reservada.nulo, Reservada.nulo, Reservada.Booleano, Reservada.nulo, getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1]));
                                re.labelTrue = labelT;
                                re.labelFalse = labelF;
                                re.ifC3D = labelC3D(condE1, condE2, "<");
                                return re;
                            }
                            else // valores no numericos
                            {
                                Debug.WriteLine("Imposible evaluar condicion < con valores diferentes");
                                Debug.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Imposible evaluar condicion < con valores diferentes", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                                return null;
                            }
                        }
                        else
                        {
                            Debug.WriteLine("Imposible evaluar condicion <");
                            Debug.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                            lstError.Add(new Error(Reservada.ErrorSemantico, "Imposible evaluar condicion <", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                            return null;
                        }
                    #endregion

                    case "COND6": // MakePlusRule(COND6, ToTerm(">"), COND7);
                        #region
                        Retorno condF1 = Condicion(Nodo.ChildNodes[0]);
                        Retorno condF2 = Condicion(Nodo.ChildNodes[2]);

                        if ((condF1 != null) && (condF2 != null)) // Si ambos son distintos de null entra
                        {
                            if ((condF1.Tipo.Equals(Reservada.Entero) && condF2.Tipo.Equals(Reservada.Real)) ||  // si uno es Entero y otro Real
                                     (condF1.Tipo.Equals(Reservada.Real) && condF2.Tipo.Equals(Reservada.Entero)) ||   // si uno es Real y otro Entero
                                         (condF1.Tipo.Equals(Reservada.Entero) && condF2.Tipo.Equals(Reservada.Entero)) ||     // si ambos son Enteros
                                             (condF1.Tipo.Equals(Reservada.Real) && condF2.Tipo.Equals(Reservada.Real)))   // si ambos son Real
                            {
                                string labelT = getEtiqueta();
                                string labelF = getEtiqueta();

                                Retorno re = new Retorno(Reservada.nulo, Reservada.nulo, Reservada.Booleano, Reservada.nulo, getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1]));
                                re.labelTrue = labelT;
                                re.labelFalse = labelF;
                                re.ifC3D = labelC3D(condF1, condF2, ">");
                                return re;
                            }
                            else if ((condF1.Tipo.Equals(Reservada.Cadena) && condF2.Tipo.Equals(Reservada.Cadena)))     //Si ambos son String
                            {
                                string labelT = getEtiqueta();
                                string labelF = getEtiqueta();

                                Retorno re = new Retorno(Reservada.nulo, Reservada.nulo, Reservada.Booleano, Reservada.nulo, getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1]));
                                re.labelTrue = labelT;
                                re.labelFalse = labelF;
                                re.ifC3D = labelC3D(condF1, condF2, ">");
                                return re;
                            }
                            else // valores no numericos
                            {
                                Debug.WriteLine("Imposible evaluar condicion > con valores diferentes");
                                Debug.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Imposible evaluar condicion > con valores diferentes", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                                return null;
                            }
                        }
                        else
                        {
                            Debug.WriteLine("Imposible evaluar condicion >");
                            Debug.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                            lstError.Add(new Error(Reservada.ErrorSemantico, "Imposible evaluar condicion >", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                            return null;
                        }
                        #endregion

                    case "COND7": // MakePlusRule(COND7, ToTerm("="), COND8);
                        #region
                        Retorno condG1 = Condicion(Nodo.ChildNodes[0]);
                        Retorno condG2 = Condicion(Nodo.ChildNodes[2]);

                        if ((condG1 != null) && (condG2 != null)) // Si ambos son distintos de null entra
                        {
                            if ((condG1.Tipo.Equals(Reservada.Entero) && condG2.Tipo.Equals(Reservada.Real)) ||  // si uno es Entero y otro Real
                                     (condG1.Tipo.Equals(Reservada.Real) && condG2.Tipo.Equals(Reservada.Entero)) ||   // si uno es Real y otro Entero
                                         (condG1.Tipo.Equals(Reservada.Entero) && condG2.Tipo.Equals(Reservada.Entero)) ||     // si ambos son Enteros
                                             (condG1.Tipo.Equals(Reservada.Real) && condG2.Tipo.Equals(Reservada.Real)))   // si ambos son Real
                            {
                                string labelT = getEtiqueta();
                                string labelF = getEtiqueta();

                                Retorno re = new Retorno(Reservada.nulo, Reservada.nulo, Reservada.Booleano, Reservada.nulo, getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1]));
                                re.labelTrue = labelT;
                                re.labelFalse = labelF;
                                re.ifC3D = labelC3D(condG1, condG2, "==");
                                return re;
                            }
                            else if ((condG1.Tipo.Equals(Reservada.Cadena) && condG2.Tipo.Equals(Reservada.Cadena)) ||      //Si ambos son String
                                    (condG1.Tipo.Equals(Reservada.Booleano) && condG2.Tipo.Equals(Reservada.Booleano)))     //Si ambos son Boolean
                            {
                                string labelT = getEtiqueta();
                                string labelF = getEtiqueta();

                                Retorno re = new Retorno(Reservada.nulo, Reservada.nulo, Reservada.Booleano, Reservada.nulo, getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1]));
                                re.labelTrue = labelT;
                                re.labelFalse = labelF;
                                re.ifC3D = labelC3D(condG1, condG2, "==");
                                return re;
                            }
                            else // valores no numericos
                            {
                                Debug.WriteLine("Imposible evaluar condicion = con valores diferentes");
                                Debug.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Imposible evaluar condicion = con valores diferentes", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                                return null;
                            }
                        }
                        else
                        {
                            Debug.WriteLine("Imposible evaluar condicion =");
                            Debug.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                            lstError.Add(new Error(Reservada.ErrorSemantico, "Imposible evaluar condicion =", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                            return null;
                        }
                    #endregion

                    case "COND8": // MakePlusRule(COND8, ToTerm("<>"), EXPRESION);
                        #region
                        Retorno condH1 = Condicion(Nodo.ChildNodes[0]);
                        Retorno condH2 = Expresion(Nodo.ChildNodes[2]);

                        if ((condH1 != null) && (condH2 != null)) // Si ambos son distintos de null entra
                        {
                            if ((condH1.Tipo.Equals(Reservada.Entero) && condH2.Tipo.Equals(Reservada.Real)) ||  // si uno es Entero y otro Real
                                     (condH1.Tipo.Equals(Reservada.Real) && condH2.Tipo.Equals(Reservada.Entero)) ||   // si uno es Real y otro Entero
                                         (condH1.Tipo.Equals(Reservada.Entero) && condH2.Tipo.Equals(Reservada.Entero)) ||     // si ambos son Enteros
                                             (condH1.Tipo.Equals(Reservada.Real) && condH2.Tipo.Equals(Reservada.Real)))   // si ambos son Real
                            {
                                string labelT = getEtiqueta();
                                string labelF = getEtiqueta();

                                Retorno re = new Retorno(Reservada.nulo, Reservada.nulo, Reservada.Booleano, Reservada.nulo, getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1]));
                                re.labelTrue = labelT;
                                re.labelFalse = labelF;
                                re.ifC3D = labelC3D(condH1, condH2, "!=");
                                return re;
                            }
                            else if ((condH1.Tipo.Equals(Reservada.Cadena) && condH2.Tipo.Equals(Reservada.Cadena)) ||      //Si ambos son String
                                    (condH1.Tipo.Equals(Reservada.Booleano) && condH2.Tipo.Equals(Reservada.Booleano)))     //Si ambos son Boolean
                            {
                                string labelT = getEtiqueta();
                                string labelF = getEtiqueta();

                                Retorno re = new Retorno(Reservada.nulo, Reservada.nulo, Reservada.Booleano, Reservada.nulo, getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1]));
                                re.labelTrue = labelT;
                                re.labelFalse = labelF;
                                re.ifC3D = labelC3D(condH1, condH2, "!=");
                                return re;
                            }
                            else // valores no numericos
                            {
                                Debug.WriteLine("Imposible evaluar condicion <> con valores diferentes");
                                Debug.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Imposible evaluar condicion <> con valores diferentes", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                                return null;
                            }
                        }
                        else
                        {
                            Debug.WriteLine("Imposible evaluar condicion <>");
                            Debug.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                            lstError.Add(new Error(Reservada.ErrorSemantico, "Imposible evaluar condicion <>", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                            return null;
                        }
                        #endregion
                }
            }
            else if (Nodo.ChildNodes.Count == 2)
            {
                #region
                Retorno condB1 = Condicion(Nodo.ChildNodes[1]);

                if (condB1 != null)
                {
                    if (condB1.Tipo.Equals(Reservada.Booleano)) // si es booleano
                    {
                        string labelT = getEtiqueta();
                        string labelF = getEtiqueta();

                        Retorno re = new Retorno(Reservada.nulo, Reservada.nulo, Reservada.Booleano, Reservada.nulo, getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0]));
                        re.labelTrue = labelT;
                        re.labelFalse = labelF;

                        if (!condB1.Temporal.Equals(Reservada.nulo))
                        {
                            re.ifC3D = "if(!" + condB1.Temporal + ")";
                        }
                        else
                        {
                            re.ifC3D = "if(!" + condB1.Valor + ")";
                        }
                        return re;
                    }
                    else
                    {
                        Debug.WriteLine("Imposible evaluar condicion NOT con valores no booleanos");
                        Debug.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                        lstError.Add(new Error(Reservada.ErrorSemantico, "Imposible evaluar condicion NOT con valores no booleanos", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                        return null;
                    }
                }
                else
                {
                    Debug.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                    lstError.Add(new Error(Reservada.ErrorSemantico, "Imposible evaluar condicion NOT", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                    return null;
                }
                #endregion
            }
            else if (Nodo.ChildNodes.Count == 1)
            {
                if (Nodo.ChildNodes[0].Term.Name.Equals("EXPRESION")) //EXPRESION
                {
                    return Expresion(Nodo.ChildNodes[0]);
                }
                else // COND1, COND2, COND3, COND4.... CONDX
                {
                    return Condicion(Nodo.ChildNodes[0]);
                }
            }
            return null;
        }

        private Retorno Expresion(ParseTreeNode Nodo)
        {
            /*
            EXPRESION.Rule = EXPRESION + mas + EXP1
                            | EXP1
            EXP1.Rule = EXP1 + menos + EXP2 
                        | EXP2
            EXP2.Rule = EXP2 + por + EXP3
                        | EXP3
            EXP3.Rule = EXP3 + division + EXP4
                        | EXP4
            EXP4.Rule = EXP4 + modulo + TERMINALES
                        | TERMINALES
            */
            if (Nodo.ChildNodes.Count == 3)
            {
                Retorno ra1 = Expresion(Nodo.ChildNodes[0]);
                Retorno ra2 = Expresion(Nodo.ChildNodes[2]);
                String linea1 = getLinea(Nodo.ChildNodes[1]);
                String colum1 = getColumna(Nodo.ChildNodes[1]);

                switch (Nodo.ChildNodes[0].Term.Name)
                {
                    case "EXPRESION": // EXPRESION + mas + EXP1
                        #region
                        if ((ra1 != null) && (ra2 != null)) // Si ambos son distintos de null entra
                        {
                            if (ra1.Tipo.Equals(Reservada.Cadena) || ra2.Tipo.Equals(Reservada.Cadena)) // Si alguno es String concateno
                            {
                                String concat = "";
                                //if (ra1.Tipo.Equals(Reservada.Cadena))
                                //{
                                //    concat = ra1.Valor + GetOperable(ra2);
                                //}
                                //else
                                //{
                                //    concat = GetOperable(ra1) + ra2.Valor;
                                //}
                                concat = GetOperable(ra1).Valor + GetOperable(ra2).Valor;
                                string tmp = getTemp();
                                string c3d = getC3D(tmp, ra1, "+", ra2);
                                return new Retorno(tmp, c3d, Reservada.Cadena, concat, linea1, colum1);
                            }
                            else if (ra1.Tipo.Equals(ra2.Tipo) && !ra1.Tipo.Equals(Reservada.Cadena)) // Si ambos son del mismo tipo y distinto de Cadena
                            {
                                double suma = 0;//double.Parse(GetOperable(ra1).Valor) + double.Parse(GetOperable(ra2).Valor);

                                string tmp = getTemp();
                                string c3d = getC3D(tmp, ra1, "+", ra2);

                                if (ra1.Tipo.Equals(Reservada.Booleano))
                                {
                                    return new Retorno(tmp, c3d, Reservada.Booleano, suma + "", linea1, colum1);
                                }
                                else
                                {
                                    return new Retorno(tmp, c3d, ra1.Tipo, suma + "", linea1, colum1);
                                }
                            }
                            else if ((ra1.Tipo.Equals(Reservada.Booleano) && ra2.Tipo.Equals(Reservada.Real)) || (ra1.Tipo.Equals(Reservada.Real) && ra2.Tipo.Equals(Reservada.Booleano)))
                            {
                                double suma = 0;//double.Parse(GetOperable(ra1).Valor) + double.Parse(GetOperable(ra2).Valor);
                                string tmp = getTemp();
                                string c3d = getC3D(tmp, ra1, "+", ra2);
                                return new Retorno(tmp, c3d, Reservada.Real, suma + "", linea1, colum1);
                            }
                            else if ((ra1.Tipo.Equals(Reservada.Booleano) && ra2.Tipo.Equals(Reservada.Entero)) || (ra1.Tipo.Equals(Reservada.Entero) && ra2.Tipo.Equals(Reservada.Booleano)))
                            {
                                double suma = 0;//double.Parse(GetOperable(ra1).Valor) + double.Parse(GetOperable(ra2).Valor);
                                string tmp = getTemp();
                                string c3d = getC3D(tmp, ra1, "+", ra2);
                                return new Retorno(tmp, c3d, Reservada.Entero, suma + "", linea1, colum1);
                            }
                            else if ((ra1.Tipo.Equals(Reservada.Real) && ra2.Tipo.Equals(Reservada.Entero)) || (ra1.Tipo.Equals(Reservada.Entero) && ra2.Tipo.Equals(Reservada.Real)))
                            {
                                double suma = 0;//double.Parse(GetOperable(ra1).Valor) + double.Parse(GetOperable(ra2).Valor);
                                string tmp = getTemp();
                                string c3d = getC3D(tmp, ra1, "+", ra2);
                                return new Retorno(tmp, c3d, Reservada.Real, suma + "", linea1, colum1);
                            }
                            else //SENOS vino un error INESPERADO PAPU (aiuda!!!)
                            {
                                Debug.WriteLine("Error Semantico--> Expresion no operable para suma linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Expresion no operable para suma", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                                return null;
                            }
                        }
                        else
                        {
                            if (ra2 == null)
                            {

                            }
                            Debug.WriteLine("Error Semantico-->Expresion no operable(null) linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                            lstError.Add(new Error(Reservada.ErrorSemantico, "Expresion no operable(null)", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                            return null;
                        }
                        #endregion

                    case "EXP1": // EXP1 + menos + EXP2
                        #region
                        if ((ra1 != null) && (ra2 != null)) // Si ambos son distintos de null entra
                        {
                            if ((ra1.Tipo.Equals(Reservada.Real) && ra2.Tipo.Equals(Reservada.Real)) //Cualquier combinacion de estos valores da Real
                                || (ra1.Tipo.Equals(Reservada.Booleano) && ra2.Tipo.Equals(Reservada.Real))
                                || (ra1.Tipo.Equals(Reservada.Real) && ra2.Tipo.Equals(Reservada.Booleano))
                                || (ra1.Tipo.Equals(Reservada.Real) && ra2.Tipo.Equals(Reservada.Entero))
                                || (ra1.Tipo.Equals(Reservada.Entero) && ra2.Tipo.Equals(Reservada.Real)))
                            {
                                double resta = 0; //double.Parse(GetOperable(ra1).Valor) - double.Parse(GetOperable(ra2).Valor);
                                string tmp = getTemp();
                                string c3d = getC3D(tmp, ra1, "-", ra2);
                                return new Retorno(tmp, c3d, Reservada.Real, resta + "", linea1, colum1);
                            }
                            else if ((ra1.Tipo.Equals(Reservada.Entero) && ra2.Tipo.Equals(Reservada.Entero)) //Cualquier combinacion de estos valores da Entero
                                || (ra1.Tipo.Equals(Reservada.Booleano) && ra2.Tipo.Equals(Reservada.Entero))
                                || (ra1.Tipo.Equals(Reservada.Entero) && ra2.Tipo.Equals(Reservada.Booleano)))
                            {
                                double resta = 0; //double.Parse(GetOperable(ra1).Valor) - double.Parse(GetOperable(ra2).Valor);
                                string tmp = getTemp();
                                string c3d = getC3D(tmp, ra1, "-", ra2);
                                return new Retorno(tmp, c3d, Reservada.Entero, resta + "", linea1, colum1);
                            }
                            else if ((ra1.Tipo.Equals(Reservada.Cadena) && ra2.Tipo.Equals(Reservada.Cadena)) //Cualquier combinacion de estos valores da Error
                                || (ra1.Tipo.Equals(Reservada.Booleano) && ra2.Tipo.Equals(Reservada.Booleano))
                                || (ra1.Tipo.Equals(Reservada.Booleano) && ra2.Tipo.Equals(Reservada.Cadena))
                                || (ra1.Tipo.Equals(Reservada.Cadena) && ra2.Tipo.Equals(Reservada.Booleano))
                                || (ra1.Tipo.Equals(Reservada.Cadena) && ra2.Tipo.Equals(Reservada.Real))
                                || (ra1.Tipo.Equals(Reservada.Real) && ra2.Tipo.Equals(Reservada.Cadena))
                                || (ra1.Tipo.Equals(Reservada.Cadena) && ra2.Tipo.Equals(Reservada.Entero))
                                || (ra1.Tipo.Equals(Reservada.Entero) && ra2.Tipo.Equals(Reservada.Cadena)))
                            {
                                Debug.WriteLine("Error Semantico--> Error al restar linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Expresion no operable para restar", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                                return null;
                            }
                            else //SENOS vino un error INESPERADO PAPU (aiuda!!!)
                            {
                                Debug.WriteLine("Error Semantico--> Expresion no operable para restar linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Expresion no operable para restar", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                                return null;
                            }
                        }
                        else
                        {
                            Debug.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                            lstError.Add(new Error(Reservada.ErrorSemantico, "Expresion no operable", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                            return null;
                        }
                        #endregion

                    case "EXP2": // EXP2 + por + EXP3
                        #region
                        if ((ra1 != null) && (ra2 != null)) // Si ambos son distintos de null entra
                        {
                            if ((ra1.Tipo.Equals(Reservada.Real) && ra2.Tipo.Equals(Reservada.Real)) //Cualquier combinacion de estos valores da Real
                                || (ra1.Tipo.Equals(Reservada.Booleano) && ra2.Tipo.Equals(Reservada.Real))
                                || (ra1.Tipo.Equals(Reservada.Real) && ra2.Tipo.Equals(Reservada.Booleano))
                                || (ra1.Tipo.Equals(Reservada.Real) && ra2.Tipo.Equals(Reservada.Entero))
                                || (ra1.Tipo.Equals(Reservada.Entero) && ra2.Tipo.Equals(Reservada.Real)))
                            {
                                double mul = 0; //double.Parse(GetOperable(ra1).Valor) * double.Parse(GetOperable(ra2).Valor);
                                string tmp = getTemp();
                                string c3d = getC3D(tmp, ra1, "*", ra2);
                                return new Retorno(tmp, c3d, Reservada.Real, mul + "", linea1, colum1);
                            }
                            else if ((ra1.Tipo.Equals(Reservada.Entero) && ra2.Tipo.Equals(Reservada.Entero))     //Cualquier combinacion de estos valores da Entero
                                || (ra1.Tipo.Equals(Reservada.Booleano) && ra2.Tipo.Equals(Reservada.Entero))
                                || (ra1.Tipo.Equals(Reservada.Entero) && ra2.Tipo.Equals(Reservada.Booleano)))
                            {
                                double mul = 0; //double.Parse(GetOperable(ra1).Valor) * double.Parse(GetOperable(ra2).Valor);
                                string tmp = getTemp();
                                string c3d = getC3D(tmp, ra1, "*", ra2);
                                return new Retorno(tmp, c3d, Reservada.Entero, mul + "", linea1, colum1);
                            }
                            else if ((ra1.Tipo.Equals(Reservada.Cadena) && ra2.Tipo.Equals(Reservada.Cadena)) //Cualquier combinacion de estos valores da Error
                                || (ra1.Tipo.Equals(Reservada.Booleano) && ra2.Tipo.Equals(Reservada.Booleano))
                                || (ra1.Tipo.Equals(Reservada.Booleano) && ra2.Tipo.Equals(Reservada.Cadena))
                                || (ra1.Tipo.Equals(Reservada.Cadena) && ra2.Tipo.Equals(Reservada.Booleano))
                                || (ra1.Tipo.Equals(Reservada.Cadena) && ra2.Tipo.Equals(Reservada.Real))
                                || (ra1.Tipo.Equals(Reservada.Real) && ra2.Tipo.Equals(Reservada.Cadena))
                                || (ra1.Tipo.Equals(Reservada.Cadena) && ra2.Tipo.Equals(Reservada.Entero))
                                || (ra1.Tipo.Equals(Reservada.Entero) && ra2.Tipo.Equals(Reservada.Cadena)))
                            {
                                Debug.WriteLine("Error Semantico--> Expresion no operable para multiplicar linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Expresion no operable para multiplicar", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                                return null;
                            }
                            else //SENOS vino un error INESPERADO PAPU (aiuda!!!)
                            {
                                Debug.WriteLine("Error Semantico--> Expresion no operable para multiplicar linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Expresion no operable para multiplicar", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                                return null;
                            }
                        }
                        else
                        {
                            Debug.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                            lstError.Add(new Error(Reservada.ErrorSemantico, "Expresion no operable", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                            return null;
                        }
                    #endregion

                    case "EXP3": // EXP3 + division + EXP4
                        #region
                        if ((ra1 != null) && (ra2 != null)) // Si ambos son distintos de null entra
                        {
                            if ((ra1.Tipo.Equals(Reservada.Real) && ra2.Tipo.Equals(Reservada.Real)) //Cualquier combinacion de estos valores da Real
                                || (ra1.Tipo.Equals(Reservada.Booleano) && ra2.Tipo.Equals(Reservada.Real))
                                || (ra1.Tipo.Equals(Reservada.Real) && ra2.Tipo.Equals(Reservada.Booleano))
                                || (ra1.Tipo.Equals(Reservada.Real) && ra2.Tipo.Equals(Reservada.Entero))
                                || (ra1.Tipo.Equals(Reservada.Entero) && ra2.Tipo.Equals(Reservada.Real))
                                || (ra1.Tipo.Equals(Reservada.Entero) && ra2.Tipo.Equals(Reservada.Entero))
                                || (ra1.Tipo.Equals(Reservada.Booleano) && ra2.Tipo.Equals(Reservada.Entero))
                                || (ra1.Tipo.Equals(Reservada.Entero) && ra2.Tipo.Equals(Reservada.Booleano)))
                            {
                                double div = 0; //double.Parse(GetOperable(ra1).Valor) / double.Parse(GetOperable(ra2).Valor);
                                string tmp = getTemp();
                                string c3d = getC3D(tmp, ra1, "/", ra2);
                                return new Retorno(tmp, c3d, Reservada.Real, div + "", linea1, colum1);
                            }
                            else if ((ra1.Tipo.Equals(Reservada.Cadena) && ra2.Tipo.Equals(Reservada.Cadena)) //Cualquier combinacion de estos valores da Error
                                || (ra1.Tipo.Equals(Reservada.Booleano) && ra2.Tipo.Equals(Reservada.Booleano))
                                || (ra1.Tipo.Equals(Reservada.Booleano) && ra2.Tipo.Equals(Reservada.Cadena))
                                || (ra1.Tipo.Equals(Reservada.Cadena) && ra2.Tipo.Equals(Reservada.Booleano))
                                || (ra1.Tipo.Equals(Reservada.Cadena) && ra2.Tipo.Equals(Reservada.Real))
                                || (ra1.Tipo.Equals(Reservada.Real) && ra2.Tipo.Equals(Reservada.Cadena))
                                || (ra1.Tipo.Equals(Reservada.Cadena) && ra2.Tipo.Equals(Reservada.Entero))
                                || (ra1.Tipo.Equals(Reservada.Entero) && ra2.Tipo.Equals(Reservada.Cadena)))
                            {
                                Debug.WriteLine("Error Semantico--> Expresion no operable para dividir linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Expresion no operable para dividir", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                                return null;
                            }
                            else //SENOS vino un error INESPERADO PAPU (aiuda!!!)
                            {
                                Debug.WriteLine("Error Semantico--> Expresion no operable para dividir linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Expresion no operable para dividir", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                                return null;
                            }
                        }
                        else
                        {
                            Debug.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                            lstError.Add(new Error(Reservada.ErrorSemantico, "Expresion no operable", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                            return null;
                        }
                    #endregion

                    case "EXP4": // EXP4 + modulo + TERMINALES 
                        #region
                        ra2 = Terminales(Nodo.ChildNodes[2]); //Aca vuelvo a asignar a ra2 el valor del TERMINAL

                        if ((ra1 != null) && (ra2 != null)) // Si ambos son distintos de null entra
                        {
                            if ((ra1.Tipo.Equals(Reservada.Real) && ra2.Tipo.Equals(Reservada.Real)) //Cualquier combinacion de estos valores da Real
                                || (ra1.Tipo.Equals(Reservada.Booleano) && ra2.Tipo.Equals(Reservada.Real))
                                || (ra1.Tipo.Equals(Reservada.Real) && ra2.Tipo.Equals(Reservada.Booleano))
                                || (ra1.Tipo.Equals(Reservada.Real) && ra2.Tipo.Equals(Reservada.Entero))
                                || (ra1.Tipo.Equals(Reservada.Entero) && ra2.Tipo.Equals(Reservada.Real))
                                || (ra1.Tipo.Equals(Reservada.Entero) && ra2.Tipo.Equals(Reservada.Entero))
                                || (ra1.Tipo.Equals(Reservada.Booleano) && ra2.Tipo.Equals(Reservada.Entero))
                                || (ra1.Tipo.Equals(Reservada.Entero) && ra2.Tipo.Equals(Reservada.Booleano)))
                            {
                                double mod = 0; //double.Parse(GetOperable(ra1).Valor) % double.Parse(GetOperable(ra2).Valor);
                                string tmp = getTemp();
                                string c3d = getC3D(tmp, ra1, "%", ra2);
                                return new Retorno(tmp, c3d, Reservada.Real, mod + "", linea1, colum1);
                            }
                            else if ((ra1.Tipo.Equals(Reservada.Cadena) && ra2.Tipo.Equals(Reservada.Cadena)) //Cualquier combinacion de estos valores da Error
                                || (ra1.Tipo.Equals(Reservada.Booleano) && ra2.Tipo.Equals(Reservada.Booleano))
                                || (ra1.Tipo.Equals(Reservada.Booleano) && ra2.Tipo.Equals(Reservada.Cadena))
                                || (ra1.Tipo.Equals(Reservada.Cadena) && ra2.Tipo.Equals(Reservada.Booleano))
                                || (ra1.Tipo.Equals(Reservada.Cadena) && ra2.Tipo.Equals(Reservada.Real))
                                || (ra1.Tipo.Equals(Reservada.Real) && ra2.Tipo.Equals(Reservada.Cadena))
                                || (ra1.Tipo.Equals(Reservada.Cadena) && ra2.Tipo.Equals(Reservada.Entero))
                                || (ra1.Tipo.Equals(Reservada.Entero) && ra2.Tipo.Equals(Reservada.Cadena)))
                            {
                                Debug.WriteLine("Error Semantico--> Expresion no operable para modulo linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Expresion no operable para modulo", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                                return null;
                            }
                            else //SENOS vino un error INESPERADO PAPU (aiuda!!!)
                            {
                                Debug.WriteLine("Error Semantico--> Expresion no operable para modulo linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Expresion no operable para modulo", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                                return null;
                            }
                        }
                        else
                        {
                            Debug.WriteLine("Error Semantico--> linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                            lstError.Add(new Error(Reservada.ErrorSemantico, "Expresion no operable", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                            return null;
                        }
                        #endregion
                }
            }
            else if (Nodo.ChildNodes.Count == 1)
            {
                if (Nodo.ChildNodes[0].Term.Name.Equals("TERMINALES"))
                {
                    return Terminales(Nodo.ChildNodes[0]);
                }
                else
                {
                    return Expresion(Nodo.ChildNodes[0]);
                }
            }
            return null;
        }

        private Retorno Terminales(ParseTreeNode Nodo)
        {
            /*
             TERMINALES.Rule = numero
                            | real
                            | menos + numero // Especial coco a esto
                            | menos + real // Especial coco a esto
                            | cadena
                            | ToTerm("true")
                            | ToTerm("false")
                            | id // Esto puede ser un ARREGLO, OBJECT o ?
                            | id + parentA + parentC // Invocacion funcion sin parametros
                            | id + parentA + ASIGNAR_PARAMETRO + parentC // Invocacion funcion con parametros
                            | id + corchA + ASIGNAR_PARAMETRO + corchC // Obteniendo valor de un array, condicion debe ser entero para acceder a esa posicion del arreglo
                            | parentA + CONDICION + parentC
             */
            switch (Nodo.ChildNodes.Count)
            {
                case 1:
                    //| numero
                    //| real
                    //| cadena
                    //| ToTerm("true")
                    //| ToTerm("false")
                    //| id // Esto puede ser un ARREGLO, OBJECT o ?
                    #region
                    switch (Nodo.ChildNodes[0].Term.Name)
                    {
                        case "numero":
                            return new Retorno(Reservada.nulo, Reservada.nulo, Reservada.Entero, Nodo.ChildNodes[0].Token.Value.ToString(), getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0]));

                        case "real":
                            return new Retorno(Reservada.nulo, Reservada.nulo, Reservada.Real, Nodo.ChildNodes[0].Token.Value.ToString(), getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0]));

                        case "cadena":
                            return new Retorno(Reservada.nulo, Reservada.nulo, Reservada.Cadena, Nodo.ChildNodes[0].Token.Value.ToString(), getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0]));

                        case "true":
                            return new Retorno(Reservada.nulo, Reservada.nulo, Reservada.Booleano, Nodo.ChildNodes[0].Token.Value.ToString(), getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0]));

                        case "false":
                            return new Retorno(Reservada.nulo, Reservada.nulo, Reservada.Booleano, Nodo.ChildNodes[0].Token.Value.ToString(), getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0]));

                        case "id": //Esto puede ser una VARIABLE o un ARREGLO

                            #region
                            String id = Nodo.ChildNodes[0].Token.Value.ToString();
                            Simbolo sim = RetornarSimbolo(id);

                            if (sim == null) //Si no existe en mi nivel actual busco en las globales
                            {
                                sim = cimaG.RetornarSimbolo(id);
                                //Debug.WriteLine(">>> Se busco en las globales <<<");
                            }

                            if (sim != null)
                            {
                                //return new Retorno("xxx", "xxx", Reservada.Real, "5.5", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0]));

                                if (sim.TipoObjeto.Equals(Reservada.array))
                                {
                                    Debug.WriteLine("Operacion no completada!");
                                    return new Retorno("xxx", "xxx", Reservada.array, id, getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0]));
                                }
                                else
                                {
                                    string tmp = getTemp();
                                    if (sim.ApuntadorAbsoluto == -1 && sim.ApuntadorRelativo != -1)
                                    {
                                        string tmp2 = getTemp();
                                        cadC3D += tmp2 + " = SP + " + sim.ApuntadorRelativo + ";\n";
                                        string cd3d = getStack(tmp, tmp2, id);
                                        return new Retorno(tmp, cd3d, sim.Tipo, sim.Valor, getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0]));
                                    }
                                    else
                                    {
                                        string cd3d = getStack(tmp, sim.ApuntadorAbsoluto, id);
                                        return new Retorno(tmp, cd3d, sim.Tipo, sim.Valor, getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0]));
                                    }                                    
                                }
                            }
                            else
                            {
                                Debug.WriteLine("Error Semantico-->Variable " + id + " no Existente linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Variable " + id + " no existente", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                                return null;
                            }
                            #endregion

                    }
                    #endregion
                    break;
                case 2:
                    //| menos + numero // Especial coco a esto
                    //| menos + real // Especial coco a esto
                    #region
                    switch (Nodo.ChildNodes[1].Term.Name)
                    {
                        case "numero":
                            Debug.WriteLine(Nodo.ChildNodes[0].Term.Name + Nodo.ChildNodes[1].Token.Value.ToString() + " <<================= SE ESTA RETORNANDO UN NEGATIVO BIEN PRRON");
                            return new Retorno(Reservada.nulo, Reservada.nulo, Reservada.Entero, Nodo.ChildNodes[0].Term.Name + Nodo.ChildNodes[1].Token.Value.ToString(), getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0]));
                        case "real":
                            Debug.WriteLine(Nodo.ChildNodes[0].Term.Name + Nodo.ChildNodes[1].Token.Value.ToString() + " <<================= SE ESTA RETORNANDO UN NEGATIVO BIEN PRRON");
                            return new Retorno(Reservada.nulo, Reservada.nulo, Reservada.Real, Nodo.ChildNodes[0].Term.Name + Nodo.ChildNodes[1].Token.Value.ToString(), getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0]));
                    }
                    #endregion
                    break;
                case 3:
                    //| id + parentA + parentC // Invocacion funcion sin parametros
                    //| parentA + CONDICION + parentC
                    #region
                    switch (Nodo.ChildNodes[0].Term.Name)
                    {
                        case "id": //| id + parentA + parentC // Invocacion funcion si parametros
                            
                            String id3 = Nodo.ChildNodes[0].Token.Value.ToString();
                            //Funciones func3 = tablafunciones.RetornarFuncion(id3);
                            Funciones func3 = tablafunciones.RetornarFuncion(id3);

                            if (func3 != null)
                            {
                                string tmp = getTemp();
                                int punt = newApuntador();
                                cadC3D += "SP = SP + " + punt + ";\n";
                                cadC3D += id3 + "();\n";
                                string cd3d = getStack(tmp, "SP", "Return");
                                cadC3D += "SP = SP - " + punt + ";\n";
                                Apuntador--; //Disminuyo la posicion que aumente en SP
                                return new Retorno(tmp, cd3d, func3.getTipo(), "", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0]));
                            }
                            else
                            {
                                Debug.WriteLine("Error Semantico-->Funcion no existente linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Funcion no existente", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                            }
                            break;

                        case "(": // parentA + CONDICION + parentC

                            Retorno ret = Condicion(Nodo.ChildNodes[1]);

                            if (ret != null)
                            {
                                return ret;
                                //return new Retorno(ret.Temporal, ret.C3D, ret.Tipo, ret.Valor, getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0]));
                            }
                            else
                            {
                                Debug.WriteLine("Error Semantico-->Retorno de parentesis mala linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Retorno de parentesis mala", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                            }
                            break;

                    }
                    #endregion
                    break;
                case 4:
                    //| id + parentA + ASIGNAR_PARAMETRO + parentC // Invocacion funcion con parametros
                    //| id + corchA + CONDICION + corchC // Obteniendo valor de un arreglo, condicion debe ser entero para acceder a esa posicion del arreglo
                    #region
                    switch (Nodo.ChildNodes[2].Term.Name)
                    {
                        case "ASIGNAR_PARAMETRO":
                            #region
                            //PRIMERO OBTENGO LA CANTIDAD DE VALORES EN MIS PARAMETROS ACEPTADOS POR EL ARREGLO
                            arreglo = new List<Celda>(); //Este arreglo es para almacenar los parametros del metodo que se invoco
                            ValidarParametrosMetodo(Nodo.ChildNodes[2]); // Mandamos los parametros
                            //AHORA BUSCO LA FUNCION EN BASE AL NOMBRE Y A MI ARREGLO DE PARAMETROS
                            String id4 = Nodo.ChildNodes[0].Token.Value.ToString();
                            //Funciones func4 = tablafunciones.RetornarFuncion(id4);
                            Funciones func4 = tablafunciones.RetornarFuncionEvaluandoSobrecarga(id4, arreglo);

                            if (func4 != null)
                            {
                                if (func4.getAmbito().Equals(Reservada.Funcion))
                                {
                                    if (!func4.getTipo().Equals(Reservada.Void)) //Si el metodo es de tipo VOID no retorna nada, ERROR
                                    {
                                        if (arreglo.Count == func4.getParametros().Count)
                                        {
                                            int punt = newApuntador(); //Guardo y reservo posicion del Return

                                            int cont = 0;
                                            string auxSH = getTemp();

                                            foreach (Parametro parametro in func4.getParametros())
                                            {
                                                if (parametro.Tipo.Equals(arreglo.ElementAt(cont).tipo))
                                                {
                                                    setStack(auxSH, newApuntador() + "", new Retorno(arreglo.ElementAt(cont).temporal, "", arreglo.ElementAt(cont).tipo, arreglo.ElementAt(cont).valor, parametro.Linea, parametro.Columna), parametro.Nombre);
                                                }
                                                else
                                                {
                                                    Console.WriteLine("Error Semantico-->Parametro introducido de tipo incompatible linea:" + parametro.Linea + " columna:" + parametro.Columna);
                                                    lstError.Add(new Error(Reservada.ErrorSemantico, "Parametro introducido de tipo incompatible", parametro.Linea, parametro.Columna));
                                                }
                                                cont++;
                                            }

                                            string tmp = getTemp();
                                            
                                            cadC3D += "SP = SP + " + punt + ";\n";
                                            cadC3D += id4 + "();\n";
                                            string cd3d = getStack(tmp, "SP", "Return");
                                            cadC3D += "SP = SP - " + punt + ";\n";
                                            Apuntador--; //Disminuyo la posicion que aumente en SP
                                            return new Retorno(tmp, cd3d, func4.getTipo(), "", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0]));
                                        }
                                        else
                                        {
                                            Console.WriteLine("Error Semantico-->Cantidad de parametros introducidos no valida linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                                            lstError.Add(new Error(Reservada.ErrorSemantico, "Cantidad de parametros introducidos no valida", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Error Semantico-->Funcion no retorna valor linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                                        lstError.Add(new Error(Reservada.ErrorSemantico, "Funcion no retorna valor", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Error Semantico-->Se esta invocando un procedimiento linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                                    lstError.Add(new Error(Reservada.ErrorSemantico, "Se esta invocando un procedimiento", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                                }
                            }
                            else
                            {
                                Console.WriteLine("Error Semantico-->Funcion no existente linea:" + getLinea(Nodo.ChildNodes[0]) + " columna:" + getColumna(Nodo.ChildNodes[0]));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Funcion no existente", getLinea(Nodo.ChildNodes[0]), getColumna(Nodo.ChildNodes[0])));
                            }
                            #endregion
                            break;
                        case "CONDICION":
                            break;
                    }
                    #endregion
                    break;

                default:
                    return null;

            }
            return null;
        }

        private void Casos(ParseTreeNode Nodo, Retorno cond)
        {
            /*
             * LSTCASE.Rule = LSTCASE + CASE
                            | CASE
                CASE.Rule = TERMINALES + ToTerm(":") + ToTerm("begin") + SENTENCIAS + ToTerm("end") + puntocoma;
             */
            switch (Nodo.Term.Name)
            {
                case "LSTCASE":
                    foreach(ParseTreeNode hijo in Nodo.ChildNodes)
                    {
                        Casos(hijo, cond);
                    }
                    break;
                case "CASE":
                    #region
                    Retorno css = Terminales(Nodo.ChildNodes[0]);

                    if(css != null)
                    {
                        if (css.Tipo.Equals(Reservada.Entero))
                        {
                            string lblTrue = getEtiqueta();
                            string lblFalse = getEtiqueta();

                            TablaSimbolos casse = new TablaSimbolos(nivelActual, cima.Tipo, Reservada.Iff, Reservada.nulo, cima.Retorna, cima.Detener, cima.etiquetaExit, lblFalse);
                            //casse.Continuar = cima.Continuar;
                            //casse.etiquetaContinue = cima.etiquetaContinue;
                            pilaSimbolos.Push(casse);
                            cima = casse; //Estableciendo la tabla de simbolos cima

                            

                            cadC3D += "//Case\n";
                            cadC3D += "if(" + cond.Temporal + "==" + css.Valor + ") goto " + lblTrue + ";\n";
                            cadC3D += "goto " + lblFalse + ";\n";
                            cadC3D += lblTrue + ":\n";
                            Sentencias(Nodo.ChildNodes[3]);
                            cadC3D += lblFalse + ":\n";

                            pilaSimbolos.Pop(); //Eliminando la tabla de simbolos cima actual
                            cima = pilaSimbolos.Peek(); //Estableciendo la nueva tabla de simbolo cima
                        }
                        else
                        {
                            Console.WriteLine("Error Semantico-->Parametro debe ser numerico linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                            lstError.Add(new Error(Reservada.ErrorSemantico, "Parametro debe ser numerico", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                        }
                    }
                    else
                    {
                        Console.WriteLine("Error Semantico-->Parametro de Case invalida linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                        lstError.Add(new Error(Reservada.ErrorSemantico, "Parametro de Case invalida", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                    }
                    #endregion
                    break;
            }
        }

        private Retorno getCadenaPrint(ParseTreeNode Nodo)
        {
            /*
            ASIGNAR_PARAMETRO.Rule = ASIGNAR_PARAMETRO + coma + CONDICION
                                    | CONDICION
            */
            string tmp = "";
            switch (Nodo.Term.Name)
            {
                case "ASIGNAR_PARAMETRO":
                    switch (Nodo.ChildNodes.Count)
                    {
                        case 3:

                            Retorno ret1 = getCadenaPrint(Nodo.ChildNodes[0]);
                            
                            Retorno ret2 = getCadenaPrint(Nodo.ChildNodes[2]);
                            
                            if(ret1 != null && ret2 != null)
                            {
                                ret1.Valor = ret1.Valor + ret2.Valor;
                                ret1.Tipo = Reservada.Cadena;
                                return ret1;
                            }
                            else
                            {
                                Debug.WriteLine("Error Semantico-->Concatenacion de cadena invalida linea:" + getLinea(Nodo.ChildNodes[1]) + " columna:" + getColumna(Nodo.ChildNodes[1]));
                                lstError.Add(new Error(Reservada.ErrorSemantico, "Concatenacion de cadena invalida", getLinea(Nodo.ChildNodes[1]), getColumna(Nodo.ChildNodes[1])));
                            }
                            return null;
                        case 1:
                            
                            ret1 = getCadenaPrint(Nodo.ChildNodes[0]);
                            return ret1;

                    }
                    break;
                case "CONDICION":
                    Retorno ret = Condicion(Nodo);
                    if (ret != null)
                    {
                        if (!ret.Temporal.Equals(Reservada.nulo))
                        {
                            if (ret.Tipo.Equals(Reservada.Cadena))
                            {
                                //Entra en este caso cuando lo que se quiere imprimir es una variable string
                                cadC3D += ("SP = SP + " + (Apuntador + 1) + ";\n"); //Aumentando apuntador
                                                                                    //tmp = getTemp();
                                setStack("SP", ret, "Heap Pointer");
                                cadC3D += ("print_function();\n");
                                cadC3D += ("SP = SP - " + (Apuntador + 1) + ";\n"); //Disminuyendo apuntador
                                //cadC3D += ("printf(\"%c\", 32); //Space\n");
                            }
                            else
                            {
                                cadC3D += ("printf(\"%f\", (float)" + ret.Temporal + ");\n");
                            }
                        }
                        else
                        {
                            //Entra en este caso cuando es una cadena dentro de writeln()
                            //int apuntador = newApuntador();
                            int apHP = newApuntadorHP();
                            tmp = getTemp();
                            stringToHeap(tmp, apHP, ret.Valor);
                            cadC3D += ("SP = SP + " + (Apuntador + 1) + ";\n");
                            cadC3D += ("Stack[(int)SP] = " + tmp + "; \t\t//Save cadena\n");
                            cadC3D += ("print_function();\n");
                            cadC3D += ("SP = SP - " + (Apuntador + 1) + ";\n");
                        }

                        return ret;
                    }
                    return null;
                default:
                    return null;
            }
            return null;
        }

        private void ValidarParametrosMetodo(ParseTreeNode Nodo)
        {
            //ASIGNAR_PARAMETRO.Rule = ASIGNAR_PARAMETRO + coma + CONDICION
            //                         | CONDICION
            #region
            switch (Nodo.Term.Name)
            {
                case "ASIGNAR_PARAMETRO":
                    foreach (ParseTreeNode hijo in Nodo.ChildNodes)
                    {
                        ValidarParametrosMetodo(hijo);
                    }
                    break;
                case "CONDICION":
                    Retorno retorno = Condicion(Nodo);
                    //Console.WriteLine("VALOR DE PARAMETRO DE UN METODO KACHEYUSE");
                    if (retorno != null)
                    {
                        arreglo.Add(new Celda(retorno.Temporal,retorno.Tipo, retorno.Valor));
                    }
                    else
                    {
                        Console.WriteLine("Error Semantico-->Asignacion de parametro incorrecta linea:" + "0" + " columna:" + "0");
                        lstError.Add(new Error(Reservada.ErrorSemantico, "Asignacion de parametro incorrecta", "0" + "", "0" + ""));
                    }
                    break;
            }
            #endregion
        }

        private string getTipoDatoFunction(ParseTreeNode Nodo)
        {
            switch (Nodo.ChildNodes[0].Term.Name)
            {
                case "String":
                    return Reservada.Cadena;
                case "Integer":
                    return Reservada.Entero;
                case "Real":
                    return Reservada.Real;
                case "Boolean":
                    return Reservada.Booleano;
            }
            return "Unknow";
        }

        private Retorno GetOperable(Retorno retornable)
        {
            switch (retornable.Tipo)
            {
                case "Char": //Cambia a ascii
                    retornable.Valor = GetAscii(retornable.Valor) + "";
                    return retornable;

                case "Boolean": //Cambia a 0 o 1
                    if (retornable.Valor.Equals("True"))
                    {
                        retornable.Valor = "1";
                        return retornable;
                    }
                    else
                    {
                        retornable.Valor = "0";
                        return retornable;
                    }

                default:
                    return retornable;
            }
        }

        private string getInicialDato(string tipodato)
        {
            if (tipodato.Equals(Reservada.Entero))
            {
                return "0";
            }
            else if (tipodato.Equals(Reservada.Real))
            {
                return "0.0";
            }
            else if (tipodato.Equals(Reservada.Cadena))
            {
                return "\"\"";
            }
            else if (tipodato.Equals(Reservada.Booleano))
            {
                return "False";
            }
            return "";
        }

        private string getTipoDato(ParseTreeNode Nodo)
        {
            switch (Nodo.ChildNodes[0].Term.Name)
            {
                case "Integer":
                    return "Integer";

                case "Real":
                    return "Real";

                case "Boolean":
                    return "Boolean";

                case "String":
                    return "String";

                default:
                    return "null";

            }
        }

        private int getCantAscii(String cadena)
        {
            Char[] Caracter = cadena.ToCharArray();

            int SumaAscii = 0;

            for (int i = 0; i < Caracter.Length; i++)
            {
                SumaAscii += GetAscii(Caracter[i] + "");
            }
            return SumaAscii;
        }

        private int GetAscii(String caracter)
        {
            return Encoding.ASCII.GetBytes(caracter)[0];
        }

        private string getC3D(string temp, Retorno izq, string op, Retorno der)
        {
            string rs = "";
            rs += temp + " = ";
            if (!izq.Temporal.Equals(Reservada.nulo))
            {
                rs += izq.Temporal;
            }
            else
            {
                rs += izq.Valor;
            }
            rs += " " + op + " ";
            if (!der.Temporal.Equals(Reservada.nulo))
            {
                rs += der.Temporal;
            }
            else
            {
                rs += der.Valor;
            }
            rs += ";\n";

            cadC3D += (rs);
            return rs;
        }

        private string getLinea(ParseTreeNode Nodo)
        {
            return (Nodo.Token.Location.Line + 1) + "";
        }

        private string getColumna(ParseTreeNode Nodo)
        {
            return (Nodo.Token.Location.Column + 1) + "";
        }

        //====================================================================================================== BUSQUEDAS AVANZADAS ===================================================================================

        private Boolean ExisteSimbolo(string nombre)
        {
            if (!Vacio())
            {
                foreach (TablaSimbolos ts in pilaSimbolos)
                {
                    if (ts.Nivel == nivelActual) //Busca el simbolo en el nivel que se maneja actualmente
                    {
                        //foreach (Simbolo simbolo in ts.getTS())
                        foreach (Simbolo simbolo in ts.ts)
                        {
                            if (simbolo.Nombre.Equals(nombre))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        private Simbolo RetornarSimbolo(String nombre)
        {
            if (!Vacio())
            {
                foreach (TablaSimbolos ts in pilaSimbolos)
                {
                    if (ts.Nivel == nivelActual) //Busca el simbolo en el nivel que se maneja actualmente
                    {
                        foreach (Simbolo simbolo in ts.ts)
                        {
                            if (simbolo.Nombre.Equals(nombre))
                            {
                                return simbolo;
                            }
                        }
                    }
                }
            }
            return null;
        }

        private void evaluarCondicion(Retorno cond)
        {
            if (cond.Temporal.Equals(Reservada.nulo))
            {
                cadC3D += cond.ifC3D + " goto " + cond.labelTrue + ";\n";
                cadC3D += "goto " + cond.labelFalse + ";\n";
            }
        }

        private Boolean Vacio()
        {
            if (!pilaSimbolos.Any())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private string agregarEncabezado()
        {
            string rs = "";
            rs = "#include <stdio.h> \t\t//Importar para el uso de printf \n";
            rs += "float Heap[100000]; \t\t//Estructura heap \n";
            rs += "float Stack[100000]; \t//Estructura stack \n";
            rs += "float SP; \t\t\t//Puntero Stack Pointer \n";
            rs += "float HP; \t\t\t//Puntero Heap Pointer \n";
            rs += "float N0, N1, N2; \t\t//Reservadas Print\n";
            rs += "\n//Declaración de temporales \n";
            rs += "float ";

            int i = 0;

            while (i <= Temporal)
            {
                rs += "T" + i;
                if (i != Temporal)
                {
                    rs += ", ";
                }
                i++;
            }
            rs += ";";

            rs += "\n\nvoid print_function() {\n";
            rs += "L03:\n";
            rs += "N0 = SP + 0;\n";
            rs += "N1 = Stack[(int)N0];\n";
            rs += "N2 = Heap[(int)N1];\n";
            rs += "L00:\n";
            rs += "if (N2 != -1) goto L01;\n";
            rs += "goto L02;\n";
            rs += "L01:\n";
            rs += "printf(\"%c\", (int)N2);\n";
            rs += "N1 = N1 + 1;\n";
            rs += "N2 = Heap[(int)N1];\n";
            rs += "goto L00;\n";
            rs += "L02:\n";
            rs += "return;\n";
            rs += "}\n";

            return rs;
        }

        private string getTemp()
        {
            //Crea y lleva el correlativo de los temporales creados
            Temporal++;
            return "T" + Temporal;
        }

        private string getEtiqueta()
        {
            //Crea y lleva el correlativo de los temporales creados
            Etiqueta++;
            return "L" + Etiqueta;
        }

        private int newApuntador()
        {
            //Crea y lleva el control del nuevo apuntador libre en el stack
            Apuntador++;
            return Apuntador;
        }

        private int newApuntadorHP()
        {
            //Crea y lleva el control del nuevo apuntador libre en el heap
            ApuntadorHP++;
            return ApuntadorHP;
        }

        private void setStack(String apuntad, Retorno res, string comment)
        {
            if (!res.Temporal.Equals(Reservada.nulo))
            {
                cadC3D += ("Stack[(int)" + apuntad + "] = " + res.Temporal + "; \t\t//Save " + comment + "\n");
            }
            else
            {
                if (!res.Tipo.Equals(Reservada.Cadena))
                {
                    cadC3D += ("Stack[(int)" + apuntad + "] = " + res.Valor + "; \t\t//Save " + comment + "\n");
                }
                else
                {
                    int apHP = newApuntadorHP();
                    string tmp = getTemp();
                    stringToHeap(tmp, apHP, res.Valor);
                    cadC3D += ("Stack[(int)" + apuntad + "] = " + tmp + "; \t\t//Save " + res.Valor + " \n");
                }
            }
        }

        private void setStack(String tmp, String apuntad, Retorno res, String comment)
        {
            if (!res.Temporal.Equals(Reservada.nulo))
            {
                cadC3D += (tmp + " = SP + " + apuntad + "; \n");
                cadC3D += ("Stack[(int)" + tmp + "] = " + res.Temporal + "; \t\t//Save " + comment + "\n");
            }
            else
            {
                if (!res.Tipo.Equals(Reservada.Cadena))
                {
                    cadC3D += (tmp + " = SP + " + apuntad + ";\n");
                    cadC3D += ("Stack[(int)" + tmp + "] = " + res.Valor + "; \t\t//Save " + comment + "\n");
                }
                else
                {
                    int apHP = newApuntadorHP();
                    string tmp2 = getTemp();
                    stringToHeap(tmp2, apHP, res.Valor);
                    cadC3D += (tmp + " = SP + " + apuntad + ";\n");
                    cadC3D += ("Stack[(int)" + tmp + "] = " + tmp2 + "; \t\t//Save " + comment + "\n");
                }
                
            }
        }

        private string getStack(string varTemp, int apuntador, string comment)
        {
            string rs = varTemp + " = Stack[(int)" + apuntador + "]; \t\t//Get " + comment + "\n";
            cadC3D += (rs);
            return rs;
        }

        private string getStack(string varTemp, string pos, string comment)
        {
            string rs = varTemp + " = Stack[(int)" + pos + "]; \t\t//Get " + comment + "\n";
            cadC3D += (rs);
            return rs;
        }

        private void stringToHeap(string tmp, int apHP, string val)
        {
            char[] arr;
            arr = val.ToCharArray();

            //cadC3D += (tmp + " = HP_" + apHP + "; \t\t//Guardando cadena \n");
            cadC3D += (tmp + " = HP + 0; \t\t//Guardando cadena \n");

            foreach (char ch in arr)
            {
                //cadC3D += ("Heap[(int)HP_" + apHP + "] = " + GetAscii(ch + "") + "; \t\t//Char " + ch + "\n");
                cadC3D += ("Heap[(int)HP] = " + GetAscii(ch + "") + "; \t\t//Char " + ch + "\n");
                cadC3D += ("HP = HP + 1;\n");
                apHP = newApuntadorHP(); //Aumento el apuntador del Heap

                /*
                 * El siguiente codigo es solo de apoyo a mi, Eliminar si es necesario
                HP.Add(new Heap(ch + "", GetAscii(ch + "")));
                Form1.Consola.AppendText("HEAP[" + ch + "" + "," + GetAscii(ch + "") + "]\n");
                */
            }

            //cadC3D += ("Heap[(int)HP_" + apHP + "] = -1;\n");
            cadC3D += ("Heap[(int)HP] = -1;\n");
            cadC3D += ("HP = HP + 1;\n");
            newApuntadorHP(); //Aumento el apuntador del Heap
        }

        private String labelC3D(Retorno izq, Retorno der, string simbolo)
        {
            if (!izq.Temporal.Equals(Reservada.nulo))
            {
                if (!der.Temporal.Equals(Reservada.nulo))
                {
                    return "if(" + izq.Temporal + simbolo + der.Temporal + ")";
                }
                else
                {
                    return "if(" + izq.Temporal + simbolo + der.Valor + ")";
                }
            }
            else
            {
                if (!der.Temporal.Equals(Reservada.nulo))
                {
                    return "if(" + izq.Valor + simbolo + der.Temporal + ")";

                }
                else
                {
                    return "if(" + izq.Valor + simbolo + der.Valor + ")";
                }
            }
        }
        
    }
}
