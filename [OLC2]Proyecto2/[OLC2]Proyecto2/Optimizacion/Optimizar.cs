using System;
using System.Collections.Generic;
using System.Text;
using Irony.Parsing;
using System.Diagnostics;
using System.Linq;
using _OLC2_Proyecto2.Ejecucion;

namespace _OLC2_Proyecto2.Optimizacion
{
    class Optimizar
    {
        public List<Error> lstError = new List<Error>();

        private Etiqueta etiquetas;

        private string cadC3D = "";

        public void iniciarOptimizacion(ParseTreeNode Nodo)
        {
            if (Nodo != null)
            {
                switch (Nodo.Term.Name)
                {
                    case "S":
                        //S.Rule = ENCABEZADO + FUNCIONES
                        if (Nodo.ChildNodes.Count == 2)
                        {
                            Encabezado(Nodo.ChildNodes[0]);
                            Funciones(Nodo.ChildNodes[1]);
                            Form1.Optimizacion.AppendText(cadC3D);
                        }
                        break;
                    default:
                        Debug.WriteLine("Error AST-->Nodo " + Nodo.Term.Name + " no existente/detectado");
                        break;
                }
            }
        }

        private void Encabezado(ParseTreeNode Nodo)
        {
            /*
             * ENCABEZADO.Rule = LIBRERIA + DECLARACIONES
            */
            try
            {
                cadC3D += "#include <stdio.h>   //Importar para el uso de printf\n";
                DeclaracionVar(Nodo.ChildNodes[1]);
            }
            catch (Exception e)
            {
                Debug.WriteLine("**Error Optimizacion (Encabezado)");
            }            
        }

        private void DeclaracionVar(ParseTreeNode Nodo)
        {
            /*
            DECLARACIONES.Rule = DECLARACIONES + VARIABLES
                                | VARIABLES
            VARIABLES.Rule = VARIABLES + VARIABLE
                            | VARIABLE
            */
            try
            {
                switch (Nodo.Term.Name)
                {
                    case "DECLARACIONES":
                        foreach (ParseTreeNode hijo in Nodo.ChildNodes)
                        {
                            DeclaracionVar(hijo);
                        }
                        break;
                    case "VARIABLES":
                        foreach (ParseTreeNode hijo in Nodo.ChildNodes)
                        {
                            DeclaracionVar(hijo);
                        }
                        break;
                    case "VARIABLE":
                        /*
                         VARIABLE.Rule = ToTerm("float") + LSTID + puntocoma
                                        | ToTerm("float") + id + corchA + numero + corchC + puntocoma
                        */
                        #region 
                        switch (Nodo.ChildNodes.Count)
                        {
                            case 3:
                                //ToTerm("float") + LSTID + puntocoma
                                cadC3D += "float ";
                                LstID(Nodo.ChildNodes[1]);
                                cadC3D += ";\n";
                                break;
                            case 6:
                                //ToTerm("float") + id + corchA + numero + corchC + puntocoma
                                String id = Nodo.ChildNodes[1].Token.Value.ToString();
                                string num = Nodo.ChildNodes[3].Token.Value.ToString();
                                cadC3D += "float " + id + "[" + num + "];\n";
                                break;
                        }
                        #endregion
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("**Error Optimizacion (DeclaracionVar)");
            }
        }

        private void LstID(ParseTreeNode Nodo)
        {
            /*
             LSTID.Rule = LSTID + coma + id
                        | id
            */
            try
            {
                switch (Nodo.Term.Name)
                {
                    case "LSTID":
                        foreach (ParseTreeNode hijo in Nodo.ChildNodes)
                        {
                            LstID(hijo);
                        }
                        break;
                    case "id":
                        cadC3D += Nodo.Token.Value.ToString();  
                        break;
                    default:
                        cadC3D += ", ";
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("**Error Optimizacion (LstID)");
            }
        }

        private void Funciones(ParseTreeNode Nodo)
        {
            /*
             * FUNCIONES.Rule = FUNCIONES + FUNCION
                            | FUNCION
             */
            try
            {
                switch (Nodo.Term.Name)
                {
                    case "FUNCIONES":
                        foreach (ParseTreeNode hijo in Nodo.ChildNodes)
                        {
                            Funciones(hijo); // SENTENCIA | SENTENCIAS
                        }
                        break;
                    case "FUNCION":
                        //FUNCION.Rule = ToTerm("void") + id + parentA + parentC + llaveA + ETIQUETAS + llaveC
                        #region 
                        etiquetas = new Etiqueta();
                        //Debug.WriteLine("funcion: " + Nodo.ChildNodes[1].Token.Text);
                        String id = Nodo.ChildNodes[1].Token.Value.ToString();

                        cadC3D += "void " + id + "(){\n";
                        SaveEtiquetas(Nodo.ChildNodes[5]);
                        cadC3D += "}\n";
                        #endregion
                        break;
                }
            }
            catch(Exception e)
            {
                Debug.WriteLine("**Error Optimizacion (Funciones)");
            }
        }

        private void SaveEtiquetas(ParseTreeNode Nodo)
        {
            /*
             ETIQUETAS.Rule = ETIQUETAS + ETIQUETA
                            | ETIQUETA
            */
            try
            {
                switch (Nodo.Term.Name)
                {
                    case "ETIQUETAS":
                        foreach (ParseTreeNode hijo in Nodo.ChildNodes)
                        {
                            SaveEtiquetas(hijo); // SENTENCIA | SENTENCIAS
                        }
                        break;
                    case "ETIQUETA":
                        //ETIQUETA.Rule = etiqueta + dospuntos + SENTENCIAS
                        #region 
                        //Debug.WriteLine("Etiqueta: " + Nodo.ChildNodes[0].Token.Text);
                        cadC3D += Nodo.ChildNodes[0].Token.Text + ":\n";
                        etiquetas.addEtiqueta(Nodo.ChildNodes[0].Token.Text);
                        Sentencias(Nodo.ChildNodes[2]);
                        #endregion
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("**Error Optimizacion (SaveEtiquetas)");
            }
        }

        private void Sentencias(ParseTreeNode Nodo)
        {
            //ETIQUETA.Rule = etiqueta + dospuntos + SENTENCIAS
            try
            {
                switch (Nodo.Term.Name)
                {
                    case "SENTENCIAS":
                        foreach (ParseTreeNode hijo in Nodo.ChildNodes)
                        {
                            Sentencias(hijo); // SENTENCIA | SENTENCIAS
                        }
                        break;
                    case "SENTENCIA":
                        /*
                         SENTENCIA.Rule = ToTerm("printf") + parentA + cadena + coma + TERMINALES + parentC + puntocoma
                            | ToTerm("if") + parentA + CONDICION + parentC + ToTerm("goto") + etiqueta + puntocoma + ToTerm("goto") + etiqueta + puntocoma
                            | ToTerm("Heap") + corchA + parentA + ToTerm("int") + parentC + TERMINALES + corchC + ToTerm("=") + TERMINALES + puntocoma
                            | ToTerm("Stack") + corchA + parentA + ToTerm("int") + parentC + TERMINALES + corchC + ToTerm("=") + TERMINALES + puntocoma
                            | ToTerm("goto") + etiqueta + puntocoma
                            | ToTerm("return") + puntocoma
                            | id + parentA + parentC + puntocoma
                            | id + ToTerm("=") + ASIGNACION + puntocoma
                        */
                        #region
                        switch (Nodo.ChildNodes.Count)
                        {
                            case 2:
                                //ToTerm("return") + puntocoma
                                cadC3D += "return;\n";
                                break;
                            case 3:
                                //ToTerm("goto") + etiqueta + puntocoma
                                cadC3D += "goto " + Nodo.ChildNodes[1].Token.Value.ToString() + ";\n";
                                break;
                            case 4:
                                //id + parentA + parentC + puntocoma
                                //id + ToTerm("=") + ASIGNACION + puntocoma
                                string id1 = Nodo.ChildNodes[0].Token.Value.ToString();
                                if (Nodo.ChildNodes[1].Term.Name.Equals("="))
                                {
                                    Asignacion(Nodo.ChildNodes[2], id1);
                                }
                                else
                                {
                                    cadC3D += id1 + "();\n";
                                }
                                break;
                            case 7:
                                //ToTerm("printf") + parentA + cadena + coma + TERMINALES + parentC + puntocoma
                                string trm = Terminales(Nodo.ChildNodes[4]);
                                string cad = Nodo.ChildNodes[2].Token.Value.ToString();
                                cadC3D += "printf(" + cad + "," + trm + ");\n";
                                break;
                            case 10:
                                //ToTerm("if") + parentA + CONDICION + parentC + ToTerm("goto") + etiqueta + puntocoma + ToTerm("goto") + etiqueta + puntocoma
                                //ToTerm("Heap") + corchA + parentA + ToTerm("int") + parentC + TERMINALES + corchC + ToTerm("=") + TERMINALES + puntocoma
                                //ToTerm("Stack") + corchA + parentA + ToTerm("int") + parentC + TERMINALES + corchC + ToTerm("=") + TERMINALES + puntocoma
                                if (Nodo.ChildNodes[0].Term.Name.Equals("Heap"))
                                {
                                    cadC3D += "Heap[(int)0] = term;\n";
                                }
                                else if (Nodo.ChildNodes[0].Term.Name.Equals("Heap"))
                                {
                                    cadC3D += "Heap[(int)0] = term;\n";
                                }
                                else
                                {
                                    string cnd = Condicion(Nodo.ChildNodes[2]);
                                    cadC3D += "if(" + cnd + ") goto Ltrue;\n";
                                    cadC3D += "goto Ltrue;\n";
                                }
                                break;
                        }
                        #endregion
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("**Error Optimizacion (Sentencias)");
            }
        }

        private void Asignacion(ParseTreeNode Nodo, string id)
        {
            /*
             ASIGNACION.Rule = ToTerm("Heap") + corchA + parentA + ToTerm("int") + parentC + TERMINALES + corchC
                            | ToTerm("Stack") + corchA + parentA + ToTerm("int") + parentC + TERMINALES + corchC
                            | EXPRESION
            */
            try
            {
                switch (Nodo.ChildNodes[0].Term.Name)
                {
                    case "Heap":
                        string trm1 = Terminales(Nodo.ChildNodes[5]);
                        cadC3D += id + " = Heap[(int)" + trm1 + "];\n";
                        break;
                    case "Stack":
                        string trm2 = Terminales(Nodo.ChildNodes[5]);
                        cadC3D += id + " = Stack[(int)" + trm2 + "];\n";
                        break;
                    case "EXPRESION":
                        Expresion(id, Nodo.ChildNodes[0]);
                        break;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("**Error Optimizacion (Asignacion)");
            }
        }

        private string Condicion(ParseTreeNode Nodo)
        {
            /*
             CONDICION.Rule = TERMINALES + ToTerm("<=") + TERMINALES
                            | TERMINALES + ToTerm(">=") + TERMINALES
                            | TERMINALES + ToTerm("<") + TERMINALES
                            | TERMINALES + ToTerm(">") + TERMINALES
                            | TERMINALES + ToTerm("==") + TERMINALES
                            | TERMINALES + ToTerm("!=") + TERMINALES
            */
            try
            {
                switch (Nodo.ChildNodes.Count)
                {
                    case 3:
                        string izq = Terminales(Nodo.ChildNodes[0]);
                        string der = Terminales(Nodo.ChildNodes[2]);
                        string sign = Nodo.ChildNodes[1].Token.Value.ToString();
                        return izq + sign + der;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("**Error Optimizacion (Condicion)");
            }
            return "null";
        }

        private string Expresion(string var, ParseTreeNode Nodo)
        {
            /*
             EXPRESION.Rule = TERMINALES + ToTerm("+") + TERMINALES
                            | TERMINALES + ToTerm("-") + TERMINALES
                            | TERMINALES + ToTerm("*") + TERMINALES
                            | TERMINALES + ToTerm("/") + TERMINALES
                            | TERMINALES + ToTerm("%") + TERMINALES
                            | TERMINALES
            */
            try
            {
                switch (Nodo.ChildNodes.Count)
                {
                    case 3:
                        string izq = Terminales(Nodo.ChildNodes[0]);
                        string der = Terminales(Nodo.ChildNodes[2]);
                        string sign = Nodo.ChildNodes[1].Token.Value.ToString();

                        if(var.Equals(izq) && der.Equals("0") && sign.Equals("+"))
                        {
                            cadC3D += "//Regla 6\n";
                        }
                        else if (var.Equals(izq) && der.Equals("0") && sign.Equals("-"))
                        {
                            cadC3D += "//Regla 7\n";
                        }
                        else if (var.Equals(izq) && der.Equals("1") && sign.Equals("*"))
                        {
                            cadC3D += "//Regla 8\n";
                        }
                        else if (var.Equals(izq) && der.Equals("1") && sign.Equals("/"))
                        {
                            cadC3D += "//Regla 9\n";
                        }
                        else if (sign.Equals("+") && der.Equals("1"))
                        {
                            cadC3D += "//Regla 10\n";
                            cadC3D += var + "=" + der + ";\n";
                        }
                        else if (sign.Equals("-") && der.Equals("1"))
                        {
                            cadC3D += "//Regla 11\n";
                            cadC3D += var + "=" + der + ";\n";
                        }
                        else if (sign.Equals("*") && der.Equals("1"))
                        {
                            cadC3D += "//Regla 12\n";
                            cadC3D += var + "=" + der + ";\n";
                        }
                        else if (sign.Equals("/") && der.Equals("1"))
                        {
                            cadC3D += "//Regla 13\n";
                            cadC3D += var + "=" + der + ";\n";
                        }
                        else if (sign.Equals("*") && der.Equals("2"))
                        {
                            cadC3D += "//Regla 14\n";
                            cadC3D += var + "=" + der + " + " + der + ";\n";
                        }
                        else if (sign.Equals("*") && der.Equals("0"))
                        {
                            cadC3D += "//Regla 15\n";
                            cadC3D += var + "= 0;\n";
                        }
                        else if (sign.Equals("/") && der.Equals("0"))
                        {
                            cadC3D += "//Regla 15\n";
                            cadC3D += var + "= 0;\n";
                        }

                        return izq + sign + der;

                    case 1:
                        string trm = Terminales(Nodo.ChildNodes[0]);
                        return trm;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("**Error Optimizacion (Expresion)");
            }
            return "null";
        }

        private string Terminales(ParseTreeNode Nodo)
        {
            /*
             TERMINALES.Rule = numero
                            | id
                            | menos + numero
                            | parentA + ToTerm("int") + parentC + numero
                            | parentA + ToTerm("int") + parentC + id
                            | parentA + ToTerm("float") + parentC + numero
                            | parentA + ToTerm("float") + parentC + id
             */
            try
            {
                switch (Nodo.ChildNodes.Count)
                {
                    case 1:
                        string id1 = Nodo.ChildNodes[0].Token.Value.ToString();
                        return id1;

                    case 2:
                        string num = Nodo.ChildNodes[1].Token.Value.ToString();
                        return "-"+num;

                    case 4:
                        String id = Nodo.ChildNodes[3].Token.Value.ToString();

                        if (Nodo.ChildNodes[3].Term.Name.Equals("id") && Nodo.ChildNodes[1].Term.Name.Equals("float"))
                        {
                            return "(float)" + id;
                        }
                        else if (Nodo.ChildNodes[3].Term.Name.Equals("id") && Nodo.ChildNodes[1].Term.Name.Equals("int"))
                        {
                            return "(int)" + id;
                        }
                        return "null";
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("**Error Optimizacion (Terminales)");
            }
            return "null";
        }
    }
}
