using System;
using System.Collections.Generic;
using System.Text;
using Irony.Parsing;

namespace _OLC2_Proyecto2.Optimizacion
{
    class Gramatica : Grammar
    {
        public Gramatica() : base(caseSensitive: false) //True es Case Sensitive - False es No Case Sensitive
        {
            //TERMINALES
            KeyTerm puntocoma = ToTerm(";");
            KeyTerm dospuntos = ToTerm(":");
            KeyTerm coma = ToTerm(",");
            KeyTerm menos = ToTerm("-");
            KeyTerm parentA = ToTerm("(");
            KeyTerm parentC = ToTerm(")");
            KeyTerm corchA = ToTerm("[");
            KeyTerm corchC = ToTerm("]");
            KeyTerm llaveA = ToTerm("{");
            KeyTerm llaveC = ToTerm("}");
            ////DECLARACION DE TERMINALES POR MEDIO DE ER.
            RegexBasedTerminal etiqueta = new RegexBasedTerminal("etiqueta", "L[0-9]+");
            IdentifierTerminal id = new IdentifierTerminal("id");
            RegexBasedTerminal numero = new RegexBasedTerminal("numero", "[0-9]+");
            RegexBasedTerminal real = new RegexBasedTerminal("real", "[0-9]+[.][0-9]+");
            StringLiteral cadena = new StringLiteral("cadena", "\"", StringOptions.IsTemplate);
            //NO TERMINALES
            NonTerminal S = new NonTerminal("S");
            NonTerminal ENCABEZADO = new NonTerminal("ENCABEZADO");
            NonTerminal LIBRERIA = new NonTerminal("LIBRERIA");
            NonTerminal DECLARACIONES = new NonTerminal("DECLARACIONES");
            NonTerminal VARIABLES = new NonTerminal("VARIABLES");
            NonTerminal VARIABLE = new NonTerminal("VARIABLE");
            NonTerminal LSTID = new NonTerminal("LSTID");

            NonTerminal FUNCIONES = new NonTerminal("FUNCIONES");
            NonTerminal FUNCION = new NonTerminal("FUNCION");
            NonTerminal ETIQUETAS = new NonTerminal("ETIQUETAS");
            NonTerminal ETIQUETA = new NonTerminal("ETIQUETA");
            NonTerminal SENTENCIAS = new NonTerminal("SENTENCIAS");
            NonTerminal SENTENCIA = new NonTerminal("SENTENCIA");
            NonTerminal ASIGNACION = new NonTerminal("ASIGNACION");


            NonTerminal CONDICION = new NonTerminal("CONDICION");
            NonTerminal EXPRESION = new NonTerminal("EXPRESION");
            NonTerminal TERMINALES = new NonTerminal("TERMINALES");

            CommentTerminal comentarioLinea = new CommentTerminal("comentarioLinea", "//", "\n", "\r\n");//Comentario de una Linea
            
            NonGrammarTerminals.Add(comentarioLinea);

            //GRAMATICA
            //S.ErrorRule = SyntaxError + puntocoma;
            //S.ErrorRule = SyntaxError + punto;
            S.Rule = ENCABEZADO + FUNCIONES
                        ;

            ENCABEZADO.Rule = LIBRERIA + DECLARACIONES //+ FUNCIONES
                                ;

            LIBRERIA.Rule = ToTerm("#") + ToTerm("include") + ToTerm("<") + ToTerm("stdio.h") + ToTerm(">")
                            ;

            DECLARACIONES.Rule = DECLARACIONES + VARIABLES
                                | VARIABLES
                                ;

            VARIABLES.Rule = VARIABLES + VARIABLE
                            | VARIABLE
                            ;

            VARIABLE.Rule = ToTerm("float") + LSTID + puntocoma
                            | ToTerm("float") + id + corchA + numero + corchC + puntocoma
                            ;

            LSTID.Rule = LSTID + coma + id
                        | id
                        ;

            FUNCIONES.Rule = FUNCIONES + FUNCION
                            | FUNCION
                            //| Empty
                            ;

            FUNCION.Rule = ToTerm("void") + id + parentA + parentC + llaveA + ETIQUETAS + llaveC
                            ;

            ETIQUETAS.Rule = ETIQUETAS + ETIQUETA
                            | ETIQUETA
                            ;

            ETIQUETA.Rule = etiqueta + dospuntos + SENTENCIAS
                            ;

            SENTENCIAS.Rule = SENTENCIAS + SENTENCIA
                            | SENTENCIA;

            //SENTENCIA.ErrorRule = SyntaxError + puntocoma;
            SENTENCIA.Rule = ToTerm("printf") + parentA + cadena + coma + TERMINALES + parentC + puntocoma
                            | ToTerm("if") + parentA + CONDICION + parentC + ToTerm("goto") + etiqueta + puntocoma + ToTerm("goto") + etiqueta + puntocoma
                            | ToTerm("Heap") + corchA + parentA + ToTerm("int") + parentC + TERMINALES + corchC + ToTerm("=") + TERMINALES + puntocoma
                            | ToTerm("Stack") + corchA + parentA + ToTerm("int") + parentC + TERMINALES + corchC + ToTerm("=") + TERMINALES + puntocoma
                            | ToTerm("goto") + etiqueta + puntocoma
                            | ToTerm("return") + puntocoma
                            | id + parentA + parentC + puntocoma
                            | id + ToTerm("=") + ASIGNACION + puntocoma
                            ;

            ASIGNACION.Rule = ToTerm("Heap") + corchA + parentA + ToTerm("int") + parentC + TERMINALES + corchC
                            | ToTerm("Stack") + corchA + parentA + ToTerm("int") + parentC + TERMINALES + corchC
                            | EXPRESION
                            ;

            //CONDICIONES
            CONDICION.Rule = TERMINALES + ToTerm("<=") + TERMINALES
                            | TERMINALES + ToTerm(">=") + TERMINALES
                            | TERMINALES + ToTerm("<") + TERMINALES
                            | TERMINALES + ToTerm(">") + TERMINALES
                            | TERMINALES + ToTerm("==") + TERMINALES
                            | TERMINALES + ToTerm("!=") + TERMINALES
                            ;
            //EXPRESIONES
            EXPRESION.Rule = TERMINALES + ToTerm("+") + TERMINALES
                            | TERMINALES + ToTerm("-") + TERMINALES
                            | TERMINALES + ToTerm("*") + TERMINALES
                            | TERMINALES + ToTerm("/") + TERMINALES
                            | TERMINALES + ToTerm("%") + TERMINALES
                            | TERMINALES
                            ;

            TERMINALES.Rule = numero
                            | menos + numero
                            | id
                            | parentA + ToTerm("int") + parentC + numero
                            | parentA + ToTerm("int") + parentC + id
                            | parentA + ToTerm("float") + parentC + numero
                            | parentA + ToTerm("float") + parentC + id
                            ;

            this.Root = S;
        }
    }
}
