using _OLC2_Proyecto2.Ejecucion;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Threading;
using Irony.Parsing;
using System.Collections;
using _OLC2_Proyecto2.Arbol;
using _OLC2_Proyecto2.Optimizacion;

namespace _OLC2_Proyecto2
{
    public partial class Form1 : Form
    {

        public static RichTextBox Consola;
        public static RichTextBox Salida;
        public static RichTextBox Optimizacion;
        Analisis analisis = new Analisis();
        Controlador controlador = new Controlador();

        public Form1()
        {
            InitializeComponent();
            Consola = richTextConsola;
            Salida = richTxtSalida;
            Optimizacion = richTxtOptimizacion;
        }

        private void btnAnalizar_Click(object sender, EventArgs e)
        {
            richTextConsola.Clear();
            richTxtSalida.Clear();

            Boolean resultado = analisis.esCadenaValida(richTxtEntrada.Text);

            if (resultado)
            {
                //MessageBox.Show("Analisis Exitoso");
                Consola.AppendText("Analisis Exitoso!\n");
                
                analisis.EjecutarAccionesPerronas();
                tabControl1.SelectedIndex = 0;
            }
            else
            {
                MessageBox.Show("Entrada contiene errores lexicos o sintacticos");
                ImprimirErroresLexicoSintactico();
            }
        }

        private void ImprimirErroresLexicoSintactico()
        {
            richTextConsola.AppendText("================== ERRORES LEXICOS Y SINTACTICOS ==================" + "\n");
            richTextConsola.AppendText("Linea" + "\t" + "Columna" + "\t" + "Tipo" + "\t" + "Descripcion" + "\n");
            if(analisis.getErrores().Count > 0)
            {
                foreach (TokenError error in analisis.getErrores())
                {
                    richTextConsola.AppendText(error.linea + "\t" + error.columna + "\t" + error.tipo + "\t" + error.descripcion + "\n");
                }
            }            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            analisis.erroresLexicosSintacticos();
        }

        private void btnGraficarArbol_Click(object sender, EventArgs e)
        {
            GraficarArbol(analisis.getArbol());
        }

        public void GraficarArbol(ParseTreeNode root)
        {
            //La imagen se genera en la carpeta 'C:\\compiladores2'
            try
            {
                String grafica = "graph Arbol_Sintactico{\n\n" + GraficaNodos(root, "0") + "\n\n}";
                FileStream stream = new FileStream("C:\\compiladores2\\Arbol.dot", FileMode.Create, FileAccess.Write);
                StreamWriter writer = new StreamWriter(stream);
                writer.WriteLine(grafica);
                writer.Close();
                ExecuteCommand("dot -Tjpg C:\\compiladores2\\Arbol.dot -o C:\\compiladores2\\Arbol.jpg");
            }
            catch (Exception x)
            {
                MessageBox.Show("Error inesperado cuando se intento graficar: " + x.ToString(), "error");
            }
        }

        static void ExecuteCommand(string _Command)
        {
            //Indicamos que deseamos inicializar el proceso cmd.exe junto a un comando de arranque. 
            //(/C, le indicamos al proceso cmd que deseamos que cuando termine la tarea asignada se cierre el proceso).
            //Para mas informacion consulte la ayuda de la consola con cmd.exe /? 
            System.Diagnostics.ProcessStartInfo procStartInfo = new System.Diagnostics.ProcessStartInfo("cmd", "/c " + _Command);
            // Indicamos que la salida del proceso se redireccione en un Stream
            procStartInfo.RedirectStandardOutput = true;
            procStartInfo.UseShellExecute = false;
            //Indica que el proceso no despliegue una pantalla negra (El proceso se ejecuta en background)
            procStartInfo.CreateNoWindow = false;
            //Inicializa el proceso
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo = procStartInfo;
            proc.Start();
            //Consigue la salida de la Consola(Stream) y devuelve una cadena de texto
            string result = proc.StandardOutput.ReadToEnd();
            //Muestra en pantalla la salida del Comando
            //Console.WriteLine(result);
        }


        private String GraficaNodos(ParseTreeNode nodo, String i)
        {
            int k = 0;
            String r = "";
            String nodoTerm = nodo.Term.ToString();
            nodoTerm = nodoTerm.Replace("\"", "");
            nodoTerm = nodoTerm.Replace("\\", "\\\\");
            r = "node" + i + "[label = \"" + nodoTerm + "\"];\n";

            for (int j = 0; j <= nodo.ChildNodes.Count() - 1; j++)
            {  // Nodos padres
                r = r + "node" + i + " -- node" + i + k + "\n";
                r = r + GraficaNodos(nodo.ChildNodes[j], "" + i + k);
                k++;
            }

            if (nodo.Token != null)
            {
                String nodoToken = nodo.Token.Text;
                nodoToken = nodoToken.Replace("\"", "");
                nodoToken = nodoToken.Replace("\\", "\\\\");
                if (nodo.ChildNodes.Count() == 0 && nodoTerm != nodoToken)
                {  // Nodos Hojas
                    r += "node" + i + "c[label = \"" + nodoToken + "\"];\n";
                    r += "node" + i + " -- node" + i + "c\n";
                }
            }

            return r;
        }

        private void btnOptimizar_Click(object sender, EventArgs e)
        {
            Boolean resultado = controlador.esCadenaValida(richTxtSalida.Text);

            if (resultado)
            {
                //MessageBox.Show("Analisis Exitoso");
                Consola.AppendText("Analisis de C3D Exitoso!\n");

                controlador.iniciarOptimizacion();
                tabControl1.SelectedIndex = 1;
            }
            else
            {
                MessageBox.Show("C3D contiene errores lexicos o sintacticos");
                controlador.erroesOptimizacion();
            }
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
