namespace Arkitektum.Kartverket.SOSI.EA.Plugin
{
    partial class frmDocument
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.btnGenerate = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.rtResult = new System.Windows.Forms.RichTextBox();
            this.chWord = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnGenerate
            // 
            this.btnGenerate.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnGenerate.Location = new System.Drawing.Point(739, 3);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(54, 34);
            this.btnGenerate.TabIndex = 0;
            this.btnGenerate.Text = "Generer";
            this.btnGenerate.UseVisualStyleBackColor = true;
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tableLayoutPanel1.Controls.Add(this.btnGenerate, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.rtResult, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.chWord, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(796, 577);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // rtResult
            // 
            this.tableLayoutPanel1.SetColumnSpan(this.rtResult, 2);
            this.rtResult.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtResult.Location = new System.Drawing.Point(3, 43);
            this.rtResult.Name = "rtResult";
            this.rtResult.Size = new System.Drawing.Size(790, 531);
            this.rtResult.TabIndex = 1;
            this.rtResult.Text = "";
            // 
            // chWord
            // 
            this.chWord.AutoSize = true;
            this.chWord.Checked = true;
            this.chWord.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chWord.Location = new System.Drawing.Point(3, 3);
            this.chWord.Name = "chWord";
            this.chWord.Size = new System.Drawing.Size(108, 17);
            this.chWord.TabIndex = 2;
            this.chWord.Text = "Skriv til MS Word";
            this.chWord.UseVisualStyleBackColor = true;
            // 
            // frmDocument
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(796, 577);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "frmDocument";
            this.Text = "Syntaks spesifikasjon";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.RichTextBox rtResult;
        private System.Windows.Forms.CheckBox chWord;
    }
}