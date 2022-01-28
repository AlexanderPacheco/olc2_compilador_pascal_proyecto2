using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Windows.Forms;
using System.IO;
//using _OLC2_Proyecto2.Ejecucion;

namespace _OLC2_Proyecto2.Optimizacion
{
    class Controlador : Grammar
    {
        public static List<TokenError> lista_errores = new List<TokenError>();

        //Para analizar
        public static ParseTree arbol = null;

        //Arbol
        private static ParseTreeNode raiz = null;


        private Optimizar optimizar;

        public Boolean esCadenaValida(string cadena)
        {
            raiz = null;
            arbol = null;

            Gramatica gramatica = new Gramatica();
            LanguageData language = new LanguageData(gramatica);
            Parser parser = new Parser(language);
            arbol = parser.Parse(cadena);
            raiz = arbol.Root;

            lista_errores.Clear();

            //================================================== PARA ERRORES LEXICOS Y SINTACTICOS ====================================================================
            int cont = 0;

            for (int i = 0; i < arbol.ParserMessages.Count(); i++)
            {
                if (arbol.ParserMessages.ElementAt(i).Message.Contains("Invalid character"))
                {
                    cont = 1;
                    TokenError newError = new TokenError();
                    newError = new TokenError(
                    arbol.ParserMessages.ElementAt(i).Location.Line,
                    arbol.ParserMessages.ElementAt(i).Location.Column,
                    "Error Lexico",
                    arbol.ParserMessages.ElementAt(i).Message.Replace("Invalid character:", "Caracter No Reconocido: ")
                    );
                    lista_errores.Add(newError);

                }

                else if (arbol.ParserMessages.ElementAt(i).Message.Contains("Syntax error"))
                {
                    if (!(arbol.ParserMessages.ElementAt(i).Message.Contains(",, $")))

                    {
                        cont = 1;
                        TokenError newError = new TokenError();
                        newError = new TokenError(
                        arbol.ParserMessages.ElementAt(i).Location.Line,
                        arbol.ParserMessages.ElementAt(i).Location.Column,
                        "Error Sintactico",
                        arbol.ParserMessages.ElementAt(i).Message.Replace("Syntax error, expected:", "Se esperaba: ")
                        );
                        lista_errores.Add(newError);
                    }
                }
            }
            //======================================================================================================================

            if (raiz == null)
            {
                return false;
            }

            return true;
        }
        
        public void iniciarOptimizacion()
        {
            optimizar = new Optimizar();
            optimizar.lstError.Clear();

            optimizar.iniciarOptimizacion(raiz);

        }
        
        public void erroesOptimizacion()
        {
            Form1.Consola.AppendText("================== ERRORES LEXICOS Y SINTACTICOS ==================" + "\n");
            Form1.Consola.AppendText("Linea" + "\t" + "Columna" + "\t" + "Tipo" + "\t" + "Descripcion" + "\n");
            if (lista_errores.Count > 0)
            {
                foreach (TokenError error in lista_errores)
                {
                    Form1.Consola.AppendText(error.linea + "\t" + error.columna + "\t" + error.tipo + "\t" + error.descripcion + "\n");
                }
            }
        }
        /*
        
        private void ImprimirErrores()
        {
            if (optimizar.lstError.Any())
            {
                Form1.Consola.AppendText("================== Errores Semanticos ==================" + "\n");
                Form1.Consola.AppendText("Linea" + "\t" + "Columna" + "\t" + "Tipo" + "\t\t" + "Descripcion" + "\n");
                foreach (Error error in optimizar.lstError)
                {
                    Form1.Consola.AppendText(error.Linea + "\t" + error.Columna + "\t" + error.Tipo + "\t" + error.Descripcion + "\n");
                }
                Form1.Consola.AppendText("** Finalizo errores **" + "\n");
            }
        }
        */
        /*
        public void erroresLexicosSintacticos()
        {
            // Creamos el documento con el tamaño de página tradicional
            Document doc = new Document(PageSize.LETTER);
            // Indicamos donde vamos a guardar el documento
            PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream("C:\\compiladores2\\Errores.pdf", FileMode.Create));

            // Abrimos el archivo
            doc.Open();

            Paragraph title = new Paragraph(string.Format("REPORTE DE ERRORES LEXICOS, SINTACTICOS Y SEMANTICOS"), new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 15, iTextSharp.text.Font.BOLD));
            title.Alignment = Element.ALIGN_CENTER;
            doc.Add(title);

            // Creamos el tipo de Font que vamos utilizar
            iTextSharp.text.Font _standardFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);

            // Escribimos el encabezamiento en el documento
            doc.Add(new Paragraph("Creado por Alex Ixva"));
            doc.Add(Chunk.NEWLINE);

            // Creamos una tabla que nuestros listado de errores
            // de nuestros visitante.
            PdfPTable tblPrueba = new PdfPTable(5);
            tblPrueba.WidthPercentage = 100;

            // Configuramos el título de las columnas de la tabla
            PdfPCell clNumero = new PdfPCell(new Phrase("#", _standardFont));
            clNumero.BorderWidth = 1;
            clNumero.BorderWidthBottom = 0.75f;

            PdfPCell clTipo = new PdfPCell(new Phrase("Tipo", _standardFont));
            clTipo.BorderWidth = 1;
            clTipo.BorderWidthBottom = 0.75f;

            PdfPCell clDescripcion = new PdfPCell(new Phrase("Descripcion", _standardFont));
            clDescripcion.BorderWidth = 1;
            clDescripcion.BorderWidthBottom = 0.75f;

            PdfPCell clLinea = new PdfPCell(new Phrase("Linea", _standardFont));
            clLinea.BorderWidth = 1;
            clLinea.BorderWidthBottom = 0.75f;

            PdfPCell clColumna = new PdfPCell(new Phrase("Columna", _standardFont));
            clColumna.BorderWidth = 1;
            clColumna.BorderWidthBottom = 0.75f;

            // Añadimos las celdas a la tabla
            tblPrueba.AddCell(clNumero);
            tblPrueba.AddCell(clTipo);
            tblPrueba.AddCell(clDescripcion);
            tblPrueba.AddCell(clLinea);
            tblPrueba.AddCell(clColumna);

            // Llenamos la tabla con información
            int cont = 1;
            foreach (TokenError error in lista_errores)
            {
                //richTextBox3.AppendText(error.linea + "\t" + error.columna + "\t" + error.tipo + "\t" + error.descripcion + "\n");
                clNumero = new PdfPCell(new Phrase(cont + "", _standardFont));
                clNumero.BorderWidth = 0;

                clTipo = new PdfPCell(new Phrase(error.tipo, _standardFont));
                clTipo.BorderWidth = 1;

                clDescripcion = new PdfPCell(new Phrase(error.descripcion, _standardFont));
                clDescripcion.BorderWidth = 0;

                clLinea = new PdfPCell(new Phrase(error.linea + "", _standardFont));
                clLinea.BorderWidth = 1;

                clColumna = new PdfPCell(new Phrase(error.columna + "", _standardFont));
                clColumna.BorderWidth = 0;

                // Añadimos las celdas a la tabla
                tblPrueba.AddCell(clNumero);
                tblPrueba.AddCell(clTipo);
                tblPrueba.AddCell(clDescripcion);
                tblPrueba.AddCell(clLinea);
                tblPrueba.AddCell(clColumna);

                cont++;
            }
            if (ejecutar.lstError.Any())
            {
                foreach (Error error in ejecutar.lstError)
                {
                    clNumero = new PdfPCell(new Phrase(cont + "", _standardFont));
                    clNumero.BorderWidth = 0;

                    clTipo = new PdfPCell(new Phrase(error.Tipo, _standardFont));
                    clTipo.BorderWidth = 1;

                    clDescripcion = new PdfPCell(new Phrase(error.Descripcion, _standardFont));
                    clDescripcion.BorderWidth = 0;

                    clLinea = new PdfPCell(new Phrase(error.Linea, _standardFont));
                    clLinea.BorderWidth = 1;

                    clColumna = new PdfPCell(new Phrase(error.Columna, _standardFont));
                    clColumna.BorderWidth = 0;

                    // Añadimos las celdas a la tabla
                    tblPrueba.AddCell(clNumero);
                    tblPrueba.AddCell(clTipo);
                    tblPrueba.AddCell(clDescripcion);
                    tblPrueba.AddCell(clLinea);
                    tblPrueba.AddCell(clColumna);

                    cont++;
                }
            }

            // Finalmente, añadimos la tabla al documento PDF y cerramos el documento
            doc.Add(tblPrueba);

            doc.Close();
            writer.Close();

            MessageBox.Show("Genero PDF de errores.");
        }
        */
    }
}
