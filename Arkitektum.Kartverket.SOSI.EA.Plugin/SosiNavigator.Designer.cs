namespace Arkitektum.Kartverket.SOSI.EA.Plugin
{
    partial class SosiNavigator
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.chVisSosiObjekt = new System.Windows.Forms.CheckBox();
            this.txtSosi = new System.Windows.Forms.TextBox();
            this.chSyntaks = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.chVisSosiObjekt, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.txtSosi, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.chSyntaks, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 31F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(415, 502);
            this.tableLayoutPanel1.TabIndex = 0;
            this.tableLayoutPanel1.Paint += new System.Windows.Forms.PaintEventHandler(this.tableLayoutPanel1_Paint);
            // 
            // chVisSosiObjekt
            // 
            this.chVisSosiObjekt.AutoSize = true;
            this.chVisSosiObjekt.Location = new System.Drawing.Point(4, 4);
            this.chVisSosiObjekt.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.chVisSosiObjekt.Name = "chVisSosiObjekt";
            this.chVisSosiObjekt.Size = new System.Drawing.Size(195, 21);
            this.chVisSosiObjekt.TabIndex = 0;
            this.chVisSosiObjekt.Text = "Vis SOSI kontrolldefinisjon";
            this.chVisSosiObjekt.UseVisualStyleBackColor = true;
            this.chVisSosiObjekt.CheckedChanged += new System.EventHandler(this.chVisSosiObjekt_CheckedChanged);
            // 
            // txtSosi
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.txtSosi, 2);
            this.txtSosi.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtSosi.Location = new System.Drawing.Point(4, 35);
            this.txtSosi.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtSosi.Multiline = true;
            this.txtSosi.Name = "txtSosi";
            this.txtSosi.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtSosi.Size = new System.Drawing.Size(407, 463);
            this.txtSosi.TabIndex = 1;
            // 
            // chSyntaks
            // 
            this.chSyntaks.AutoSize = true;
            this.chSyntaks.Location = new System.Drawing.Point(211, 4);
            this.chSyntaks.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.chSyntaks.Name = "chSyntaks";
            this.chSyntaks.Size = new System.Drawing.Size(109, 21);
            this.chSyntaks.TabIndex = 2;
            this.chSyntaks.Text = "Sosi syntaks";
            this.chSyntaks.UseVisualStyleBackColor = true;
            this.chSyntaks.Visible = false;
            this.chSyntaks.CheckedChanged += new System.EventHandler(this.chSyntaks_CheckedChanged);
            // 
            // SosiNavigator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "SosiNavigator";
            this.Size = new System.Drawing.Size(415, 502);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.CheckBox chVisSosiObjekt;
        private System.Windows.Forms.TextBox txtSosi;
        private System.Windows.Forms.CheckBox chSyntaks;
    }
}
