using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace _OLC2_Proyecto2.Ejecucion
{
    class Objeto
    {
        public string posicion;
        public string funcion;
        public string ambito;
        public string nombre;

        public Objeto(string funcion, string ambito, string nombre, string posicion)
        {
            this.funcion = funcion;
            this.ambito = ambito;
            this.nombre = nombre;
            this.posicion = posicion;
        }
    }
    class GraficarTS
    {
        public List<Objeto> lstTS = new List<Objeto>();
        public string nombre;

        public GraficarTS(string nombre)
        {
            this.nombre = nombre;
        }

        public void addSimbolo(string funcion, string ambito, string nombre, string posicion)
        {
            lstTS.Add(new Objeto(funcion, ambito, nombre, posicion));
        }

        public void Graficar()
        {
            // Creamos el documento con el tamaño de página tradicional
            Document doc = new Document(PageSize.LETTER);
            // Indicamos donde vamos a guardar el documento
            PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream("C:\\compiladores2\\"+nombre+".pdf", FileMode.Create));

            // Abrimos el archivo
            doc.Open();

            Paragraph title = new Paragraph(string.Format("REPORTE DE TABLA DE SIMBOLOS"), new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 15, iTextSharp.text.Font.BOLD));
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

            PdfPCell clTipo = new PdfPCell(new Phrase("Funcion", _standardFont));
            clTipo.BorderWidth = 1;
            clTipo.BorderWidthBottom = 0.75f;

            PdfPCell clDescripcion = new PdfPCell(new Phrase("Ambito", _standardFont));
            clDescripcion.BorderWidth = 1;
            clDescripcion.BorderWidthBottom = 0.75f;

            PdfPCell clLinea = new PdfPCell(new Phrase("Nombre", _standardFont));
            clLinea.BorderWidth = 1;
            clLinea.BorderWidthBottom = 0.75f;

            PdfPCell clColumna = new PdfPCell(new Phrase("Posicion", _standardFont));
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
            foreach (Objeto ts in lstTS)
            {
                //richTextBox3.AppendText(error.linea + "\t" + error.columna + "\t" + error.tipo + "\t" + error.descripcion + "\n");
                clNumero = new PdfPCell(new Phrase(cont + "", _standardFont));
                clNumero.BorderWidth = 0;

                clTipo = new PdfPCell(new Phrase(ts.funcion, _standardFont));
                clTipo.BorderWidth = 1;

                clDescripcion = new PdfPCell(new Phrase(ts.ambito, _standardFont));
                clDescripcion.BorderWidth = 0;

                clLinea = new PdfPCell(new Phrase(ts.nombre+ "", _standardFont));
                clLinea.BorderWidth = 1;

                clColumna = new PdfPCell(new Phrase(ts.posicion + "", _standardFont));
                clColumna.BorderWidth = 0;

                // Añadimos las celdas a la tabla
                tblPrueba.AddCell(clNumero);
                tblPrueba.AddCell(clTipo);
                tblPrueba.AddCell(clDescripcion);
                tblPrueba.AddCell(clLinea);
                tblPrueba.AddCell(clColumna);

                cont++;
            }

            // Finalmente, añadimos la tabla al documento PDF y cerramos el documento
            doc.Add(tblPrueba);

            doc.Close();
            writer.Close();

            Form1.Consola.AppendText("Se genero tabla de simbolos (" + nombre + ")\n");
        }
    }
}
