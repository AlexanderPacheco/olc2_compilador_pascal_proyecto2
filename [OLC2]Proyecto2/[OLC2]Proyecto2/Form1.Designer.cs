
namespace _OLC2_Proyecto2
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.richTxtEntrada = new System.Windows.Forms.RichTextBox();
            this.richTxtSalida = new System.Windows.Forms.RichTextBox();
            this.btnAnalizar = new System.Windows.Forms.Button();
            this.richTextConsola = new System.Windows.Forms.RichTextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.btnGraficarArbol = new System.Windows.Forms.Button();
            this.btnOptimizar = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.richTxtOptimizacion = new System.Windows.Forms.RichTextBox();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // richTxtEntrada
            // 
            this.richTxtEntrada.BackColor = System.Drawing.SystemColors.MenuText;
            this.richTxtEntrada.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.richTxtEntrada.ForeColor = System.Drawing.Color.Lime;
            this.richTxtEntrada.Location = new System.Drawing.Point(12, 12);
            this.richTxtEntrada.Name = "richTxtEntrada";
            this.richTxtEntrada.Size = new System.Drawing.Size(543, 402);
            this.richTxtEntrada.TabIndex = 0;
            this.richTxtEntrada.Text = resources.GetString("richTxtEntrada.Text");
            this.richTxtEntrada.WordWrap = false;
            // 
            // richTxtSalida
            // 
            this.richTxtSalida.BackColor = System.Drawing.SystemColors.MenuText;
            this.richTxtSalida.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.richTxtSalida.ForeColor = System.Drawing.Color.Lime;
            this.richTxtSalida.Location = new System.Drawing.Point(0, 0);
            this.richTxtSalida.Name = "richTxtSalida";
            this.richTxtSalida.Size = new System.Drawing.Size(536, 374);
            this.richTxtSalida.TabIndex = 1;
            this.richTxtSalida.Text = "";
            // 
            // btnAnalizar
            // 
            this.btnAnalizar.FlatAppearance.BorderSize = 0;
            this.btnAnalizar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAnalizar.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnAnalizar.Image = ((System.Drawing.Image)(resources.GetObject("btnAnalizar.Image")));
            this.btnAnalizar.Location = new System.Drawing.Point(561, 89);
            this.btnAnalizar.Name = "btnAnalizar";
            this.btnAnalizar.Size = new System.Drawing.Size(48, 35);
            this.btnAnalizar.TabIndex = 2;
            this.btnAnalizar.UseVisualStyleBackColor = true;
            this.btnAnalizar.Click += new System.EventHandler(this.btnAnalizar_Click);
            // 
            // richTextConsola
            // 
            this.richTextConsola.BackColor = System.Drawing.SystemColors.MenuText;
            this.richTextConsola.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.richTextConsola.ForeColor = System.Drawing.Color.Lime;
            this.richTextConsola.Location = new System.Drawing.Point(12, 420);
            this.richTextConsola.Name = "richTextConsola";
            this.richTextConsola.Size = new System.Drawing.Size(1147, 274);
            this.richTextConsola.TabIndex = 3;
            this.richTextConsola.Text = "";
            // 
            // button1
            // 
            this.button1.FlatAppearance.BorderSize = 0;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.ForeColor = System.Drawing.SystemColors.ControlText;
            this.button1.Image = ((System.Drawing.Image)(resources.GetObject("button1.Image")));
            this.button1.Location = new System.Drawing.Point(561, 199);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(48, 41);
            this.button1.TabIndex = 4;
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnGraficarArbol
            // 
            this.btnGraficarArbol.FlatAppearance.BorderSize = 0;
            this.btnGraficarArbol.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGraficarArbol.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnGraficarArbol.Image = ((System.Drawing.Image)(resources.GetObject("btnGraficarArbol.Image")));
            this.btnGraficarArbol.Location = new System.Drawing.Point(561, 246);
            this.btnGraficarArbol.Name = "btnGraficarArbol";
            this.btnGraficarArbol.Size = new System.Drawing.Size(48, 52);
            this.btnGraficarArbol.TabIndex = 5;
            this.btnGraficarArbol.UseVisualStyleBackColor = true;
            this.btnGraficarArbol.Click += new System.EventHandler(this.btnGraficarArbol_Click);
            // 
            // btnOptimizar
            // 
            this.btnOptimizar.FlatAppearance.BorderSize = 0;
            this.btnOptimizar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOptimizar.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnOptimizar.Image = ((System.Drawing.Image)(resources.GetObject("btnOptimizar.Image")));
            this.btnOptimizar.Location = new System.Drawing.Point(561, 148);
            this.btnOptimizar.Name = "btnOptimizar";
            this.btnOptimizar.Size = new System.Drawing.Size(48, 35);
            this.btnOptimizar.TabIndex = 6;
            this.btnOptimizar.UseVisualStyleBackColor = true;
            this.btnOptimizar.Click += new System.EventHandler(this.btnOptimizar_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(615, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(544, 402);
            this.tabControl1.TabIndex = 7;
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.Color.Black;
            this.tabPage1.Controls.Add(this.richTxtSalida);
            this.tabPage1.Location = new System.Drawing.Point(4, 24);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(536, 374);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "C3D";
            // 
            // tabPage2
            // 
            this.tabPage2.BackColor = System.Drawing.Color.Black;
            this.tabPage2.Controls.Add(this.richTxtOptimizacion);
            this.tabPage2.ForeColor = System.Drawing.SystemColors.Control;
            this.tabPage2.Location = new System.Drawing.Point(4, 24);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(536, 374);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Optimizado";
            // 
            // richTxtOptimizacion
            // 
            this.richTxtOptimizacion.BackColor = System.Drawing.SystemColors.MenuText;
            this.richTxtOptimizacion.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.richTxtOptimizacion.ForeColor = System.Drawing.Color.Lime;
            this.richTxtOptimizacion.Location = new System.Drawing.Point(0, 0);
            this.richTxtOptimizacion.Name = "richTxtOptimizacion";
            this.richTxtOptimizacion.Size = new System.Drawing.Size(536, 374);
            this.richTxtOptimizacion.TabIndex = 8;
            this.richTxtOptimizacion.Text = "";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1171, 706);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.btnOptimizar);
            this.Controls.Add(this.btnGraficarArbol);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.richTextConsola);
            this.Controls.Add(this.btnAnalizar);
            this.Controls.Add(this.richTxtEntrada);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.ForeColor = System.Drawing.SystemColors.Control;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "COMPI PASCAL";
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox richTxtEntrada;
        private System.Windows.Forms.RichTextBox richTxtSalida;
        private System.Windows.Forms.Button btnAnalizar;
        private System.Windows.Forms.RichTextBox richTextConsola;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btnGraficarArbol;
        private System.Windows.Forms.Button btnOptimizar;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.RichTextBox richTxtOptimizacion;
    }
}

