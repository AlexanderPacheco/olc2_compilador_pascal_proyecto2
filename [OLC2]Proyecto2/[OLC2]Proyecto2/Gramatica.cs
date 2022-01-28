using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Irony.Parsing;

namespace _OLC2_Proyecto2
{
    class Gramatica : Grammar
    {
        public Gramatica() : base(caseSensitive: false) //True es Case Sensitive - False es No Case Sensitive
        {
            //TERMINALES
            KeyTerm puntocoma = ToTerm(";");
            KeyTerm punto = ToTerm(".");
            KeyTerm dospuntos = ToTerm(":");
            KeyTerm coma = ToTerm(",");
            KeyTerm mas = ToTerm("+");
            KeyTerm menos = ToTerm("-");
            KeyTerm por = ToTerm("*");
            KeyTerm division = ToTerm("/");
            KeyTerm modulo = ToTerm("MOD");
            KeyTerm parentA = ToTerm("(");
            KeyTerm parentC = ToTerm(")");
            KeyTerm corchA = ToTerm("[");
            KeyTerm corchC = ToTerm("]");
            ////DECLARACION DE TERMINALES POR MEDIO DE ER.
            IdentifierTerminal id = new IdentifierTerminal("id");
            RegexBasedTerminal numero = new RegexBasedTerminal("numero", "[0-9]+");
            RegexBasedTerminal real = new RegexBasedTerminal("real", "[0-9]+[.][0-9]+");
            StringLiteral cadena = new StringLiteral("cadena", "\'", StringOptions.IsTemplate);
            //NO TERMINALES
            NonTerminal S = new NonTerminal("S");
            NonTerminal ESTRUCTURA = new NonTerminal("ESTRUCTURA");
            NonTerminal BLOQUE = new NonTerminal("BLOQUE");
            NonTerminal VARYTYPE = new NonTerminal("VARYTYPE");
            NonTerminal TIPOS = new NonTerminal("TIPOS");
            NonTerminal LSTVARS = new NonTerminal("LSTVARS");
            NonTerminal VARS = new NonTerminal("VARS");
            NonTerminal FUNCIONES = new NonTerminal("FUNCIONES");
            NonTerminal ARRAYATRIBS = new NonTerminal("ARRAYATRIBS");
            NonTerminal ARRAYATRIB = new NonTerminal("ARRAYATRIB");

            NonTerminal PROCEDIMIENTO = new NonTerminal("PROCEDIMIENTO");
            NonTerminal PARAMETROS = new NonTerminal("PARAMETROS");
            NonTerminal PARAMETRO = new NonTerminal("PARAMETRO");
            NonTerminal ASIGNAR_PARAMETRO = new NonTerminal("ASIGNAR_PARAMETRO");
            NonTerminal SENTENCIAS = new NonTerminal("SENTENCIAS");
            NonTerminal SENTENCIA = new NonTerminal("SENTENCIA");
            NonTerminal IF = new NonTerminal("IF");
            NonTerminal ELSEIF = new NonTerminal("ELSEIF");
            NonTerminal FINIF = new NonTerminal("FINIF");
            NonTerminal LISTIF = new NonTerminal("LISTIF");
            NonTerminal LSTID = new NonTerminal("LSTID");
            NonTerminal IDPARAM = new NonTerminal("IDPARAM");
            NonTerminal OPERAFOR = new NonTerminal("OPERAFOR");
            NonTerminal LSTARREGLO = new NonTerminal("LSTARREGLO");
            NonTerminal LSTCASE = new NonTerminal("LSTCASE");
            NonTerminal CASE = new NonTerminal("CASE");
            NonTerminal DEFAUL = new NonTerminal("DEFAUL");
            NonTerminal LSSWITCH = new NonTerminal("LSSWITCH");
            NonTerminal TERMCASO = new NonTerminal("TERMCASO");
            NonTerminal CONDICION = new NonTerminal("CONDICION");
            NonTerminal COND1 = new NonTerminal("COND1");
            NonTerminal COND2 = new NonTerminal("COND2");
            NonTerminal COND3 = new NonTerminal("COND3");
            NonTerminal COND4 = new NonTerminal("COND4");
            NonTerminal COND5 = new NonTerminal("COND5");
            NonTerminal COND6 = new NonTerminal("COND6");
            NonTerminal COND7 = new NonTerminal("COND7");
            NonTerminal COND8 = new NonTerminal("COND8");
            NonTerminal EXPRESION = new NonTerminal("EXPRESION");
            NonTerminal EXP1 = new NonTerminal("EXP1");
            NonTerminal EXP2 = new NonTerminal("EXP2");
            NonTerminal EXP3 = new NonTerminal("EXP3");
            NonTerminal EXP4 = new NonTerminal("EXP4");
            NonTerminal TERMINALES = new NonTerminal("TERMINALES");
            NonTerminal TIPODATOF = new NonTerminal("TIPODATOF");
            NonTerminal TIPODATO = new NonTerminal("TIPODATO");

            CommentTerminal comentarioLinea = new CommentTerminal("comentarioLinea", "//", "\n", "\r\n");//Comentario de una Linea
            CommentTerminal comentarioMulti1 = new CommentTerminal("comentarioBloque", "(*", "*)");//Comentario de muchas lineas
            CommentTerminal comentarioMulti2 = new CommentTerminal("comentarioBloque", "{", "}");//Comentario de muchas lineas
            NonGrammarTerminals.Add(comentarioLinea);
            NonGrammarTerminals.Add(comentarioMulti1);
            NonGrammarTerminals.Add(comentarioMulti2);

            //GRAMATICA
            //S.ErrorRule = SyntaxError + puntocoma;
            //S.ErrorRule = SyntaxError + punto;
            S.Rule = ToTerm("program") + id + puntocoma + ESTRUCTURA + ToTerm("begin") + SENTENCIAS + ToTerm("end") + punto
                    ;

            ESTRUCTURA.Rule = ESTRUCTURA + BLOQUE
                            | BLOQUE
                            | Empty //Vacio
                            ;

            BLOQUE.Rule = VARYTYPE
                        | FUNCIONES
                        | PROCEDIMIENTO
                        ;

            /*
             * Las constantes es obligatorio que se declaren con un valor. X
             * Si se definen variables con asignación de un valor, únicamente se permite agregar una variable en la definición como lo delimita el lenguaje pascal. X
             * Si una variable no se inicializa, tomará los valores por defecto del lenguaje Pascal.
             * Una constante no puede ser asignada. (variablex = unaconstante)?
             * Las variables solo aceptan un tipo de dato, el cual no puede cambiarse en tiempo de ejecución.
             * Las variables no pueden cambiar de tipo de dato, se deben mantener con el tipo declarado inicialmente.
             * No es posible cambiar el tipo de dato de una variable, a menos que el tipo de dato no se haya especificado al declarar la variable.
             * No puede declararse una variable o constante con un identificador que tengan el mismo nombre.
             */
            /*
             * Los arreglos pueden ser de cualquier tipo de dato válido (Incluso arreglos o types) y son dinámicos, además poseen la siguiente estructura:
             * type array-id = array[index-type] of element-type
             */
            //VARYTYPE.ErrorRule = SyntaxError + puntocoma;
            VARYTYPE.Rule = ToTerm("type") + LSTVARS //ToTerm("type") + LSTVARS + ToTerm("end") + puntocoma
                        | ToTerm("var") + LSTVARS
                        | ToTerm("const") + id + ToTerm("=") + CONDICION + puntocoma //Ex: const id = constant_value;
                        | id + ToTerm("=") + CONDICION + puntocoma //Ex: id = constant_value;
                        ;

            LSTVARS.Rule = LSTVARS + VARS
                        | VARS
                        ;

            //VARS.ErrorRule = SyntaxError + puntocoma;
            VARS.Rule = LSTID + dospuntos + TIPODATO + puntocoma //Var ... All variables must be declared before we use them in Pascal program
                        | LSTID + dospuntos + TIPODATO + ToTerm("=") + CONDICION + puntocoma //Var
                        | LSTID + ToTerm("=") + TIPODATO + puntocoma //Type
                        | LSTID + ToTerm("=") + ToTerm("object") + ToTerm("var") + LSTVARS + ToTerm("end") + puntocoma //OBJECT struct (solo se puede crear dentro del procedure general), VALIDAR SI LSTVAR ENTRA DE NUEVO A ESTA PRODUCCION FIJO F, o que LSTVAR SEA otra produccion diferente
                        | LSTID + ToTerm("=") + ToTerm("array") + corchA + ARRAYATRIBS + corchC + ToTerm("of") + TIPODATO + puntocoma //Array
                        ;

            LSTID.Rule = LSTID + coma + id
                        | id
                        ;

            ARRAYATRIBS.Rule = ARRAYATRIBS + coma + ARRAYATRIB
                            | ARRAYATRIB;

            ARRAYATRIB.Rule = numero
                            | menos + numero
                            | numero + ToTerm("..") + numero
                            | numero + ToTerm("..") + menos + numero
                            | menos + numero + ToTerm("..") + numero
                            | menos + numero + ToTerm("..") + menos + numero
                            | id
                            | cadena
                            ;

            /*
             * Void, es solo cuando las funciones retornan un null
             */
            //FUNCIONES.ErrorRule = SyntaxError + puntocoma;
            FUNCIONES.Rule = ToTerm("function") + id + parentA + PARAMETROS + parentC + dospuntos + TIPODATO + puntocoma + ESTRUCTURA + ToTerm("begin") + SENTENCIAS + ToTerm("end") + puntocoma
                            | ToTerm("function") + id + dospuntos + TIPODATO + puntocoma + ESTRUCTURA + ToTerm("begin") + SENTENCIAS + ToTerm("end") + puntocoma
                            ;

            //PROCEDIMIENTO.ErrorRule = SyntaxError + puntocoma;
            PROCEDIMIENTO.Rule = ToTerm("procedure") + id + parentA + PARAMETROS + parentC + puntocoma + ESTRUCTURA + ToTerm("begin") + SENTENCIAS + ToTerm("end") + puntocoma
                                | ToTerm("procedure") + id + puntocoma + ESTRUCTURA + ToTerm("begin") + SENTENCIAS + ToTerm("end") + puntocoma
                        ;

            SENTENCIAS.Rule = SENTENCIAS + SENTENCIA
                            | SENTENCIA;

            //SENTENCIA.ErrorRule = SyntaxError + puntocoma;
            SENTENCIA.Rule = ToTerm("write") + parentA + ASIGNAR_PARAMETRO + parentC + puntocoma
                            | ToTerm("writeln") + parentA + ASIGNAR_PARAMETRO + parentC + puntocoma
                            | ToTerm("graficar_ts") + parentA + cadena + parentC + puntocoma
                            | ToTerm("exit") + parentA + parentC + puntocoma //Retorna el valor por defecto del tipo de dato de retorno de la funcion
                            | ToTerm("exit") + parentA + CONDICION + parentC + puntocoma //Retorna el valor de condicion 
                            | ToTerm("continue") + puntocoma
                            | ToTerm("break") + puntocoma
                            | ToTerm("if") + CONDICION + ToTerm("then") + ToTerm("begin") + SENTENCIAS + ToTerm("end") + puntocoma //if
                            | ToTerm("if") + CONDICION + ToTerm("then") + ToTerm("begin") + SENTENCIAS + ToTerm("end") + ToTerm("else") + ToTerm("begin") + SENTENCIAS + ToTerm("end") + puntocoma //if-else
                            //| IF + puntocoma
                            | ToTerm("case") + TERMINALES + ToTerm("of") + LSTCASE + ToTerm("else") + ToTerm("begin") + SENTENCIAS + ToTerm("end") + puntocoma + ToTerm("end") + puntocoma
                            //| ToTerm("case") + TERMINALES + ToTerm("of") + LSTCASE + ToTerm("end") + puntocoma 
                            | ToTerm("while") + CONDICION + ToTerm("do") + ToTerm("begin") + SENTENCIAS + ToTerm("end") + puntocoma
                            | ToTerm("repeat") + SENTENCIAS + ToTerm("until") + CONDICION + puntocoma
                            | ToTerm("for") + id + ToTerm(":=") + TERMINALES + ToTerm("to") + TERMINALES + ToTerm("do") + ToTerm("begin") + SENTENCIAS + ToTerm("end") + puntocoma
                            | id + parentA + parentC + puntocoma
                            | id + parentA + ASIGNAR_PARAMETRO + parentC + puntocoma
                            | id + corchA + ASIGNAR_PARAMETRO + corchC + ToTerm(":=") + CONDICION + puntocoma
                            | id + ToTerm(":=") + CONDICION + puntocoma //Si id es igual al nombre de la funcion retorna lo que tiene en condicion
                            ;

            //IF ELSEIF ELSE
            //IF.ErrorRule = SyntaxError + "}";
            IF.Rule = ToTerm("if") + CONDICION + ToTerm("then") + ToTerm("begin") + LISTIF  + ELSEIF
                    ;

            ELSEIF.Rule = ToTerm("end") + ToTerm("else") + IF
                    | ToTerm("end") + ToTerm("else") + ToTerm("begin")  + LISTIF + ToTerm("end")
                    | Empty
                    ;

            FINIF.Rule = IF
                    |LISTIF + ToTerm("end")
                    ;

            LISTIF.Rule = LISTIF + SENTENCIAS
                    | SENTENCIAS
                    | Empty
                    ;

            ASIGNAR_PARAMETRO.Rule = ASIGNAR_PARAMETRO + coma + CONDICION
                                    | CONDICION
                                    ;

            PARAMETROS.Rule = PARAMETROS + puntocoma + PARAMETRO
                            | PARAMETRO
                            ;

            PARAMETRO.Rule = IDPARAM + dospuntos + TIPODATO
                            | ToTerm("var") + IDPARAM + dospuntos + TIPODATO
                            | Empty
                            ;

            IDPARAM.Rule = IDPARAM + coma + id
                            | id
                            ;

            //CASE
            LSTCASE.Rule = LSTCASE + CASE
                            | CASE
                            ;

            CASE.Rule = TERMINALES + ToTerm(":") + ToTerm("begin") + SENTENCIAS + ToTerm("end") + puntocoma;

            //CONDICIONES
            CONDICION.Rule = CONDICION + ToTerm("and") + COND1
                            | COND1
                            ;

            COND1.Rule = COND1 + ToTerm("or") + COND2
                        | COND2
                        ;

            COND2.Rule = ToTerm("not") + COND3
                        | COND3
                        ;

            COND3.Rule = COND3 + ToTerm("<=") + COND4
                        | COND4
                        ;

            COND4.Rule = COND4 + ToTerm(">=") + COND5
                        | COND5
                        ;

            COND5.Rule = COND5 + ToTerm("<") + COND6
                        | COND6
                        ;

            COND6.Rule = COND6 + ToTerm(">") + COND7
                        | COND7
                        ;

            COND7.Rule = COND7 + ToTerm("=") + COND8
                        | COND8
                        ;

            COND8.Rule = COND8 + ToTerm("<>") + EXPRESION
                        | EXPRESION
                        ;

            //EXPRESIONES
            EXPRESION.Rule = EXPRESION + mas + EXP1
                            | EXP1
                            ;

            EXP1.Rule = EXP1 + menos + EXP2
                        | EXP2
                        ;

            EXP2.Rule = EXP2 + por + EXP3
                        | EXP3
                        ;

            EXP3.Rule = EXP3 + division + EXP4
                        | EXP4
                        ;

            EXP4.Rule = EXP4 + modulo + TERMINALES
                        | TERMINALES
                        ;

            TERMINALES.Rule = numero
                            | real
                            | menos + numero // Especial coco a esto, VER SI EL MENOS(-) SE PUEDE PONER DESDE LA EXPRESION
                            | menos + real // Especial coco a esto
                            | cadena
                            | ToTerm("true")
                            | ToTerm("false")
                            | id // Esto puede ser un ARREGLO, OBJECT o ?
                            | id + parentA + parentC // Invocacion funcion sin parametros
                            | id + parentA + ASIGNAR_PARAMETRO + parentC // Invocacion funcion con parametros
                            | id + corchA + ASIGNAR_PARAMETRO + corchC // Obteniendo valor de un array, condicion debe ser entero para acceder a esa posicion del arreglo
                            | parentA + CONDICION + parentC
                            ;

            TIPODATO.Rule = ToTerm("String")
                            | ToTerm("Integer")
                            | ToTerm("Real")
                            | ToTerm("Boolean")
                            //| id
                            ;


            this.Root = S;
        }
    }
}
