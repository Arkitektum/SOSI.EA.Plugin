using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using EA;
using Arkitektum.Kartverket.SOSI.EA.Plugin.sosimodell;
using Office = Microsoft.Office.Core;
using Word = Microsoft.Office.Interop.Word;

namespace Arkitektum.Kartverket.SOSI.EA.Plugin
{
    public partial class frmDocument : Form
    {
        Repository _rep;

        public frmDocument()
        {
            InitializeComponent();
        }

        public void SetRep(Repository Repository)
        {
            _rep = Repository;
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            rtResult.Clear();
            //rtResult.AppendText("<Objekttype>");
            Sosimodell modell = new Sosimodell();
            List<Objekttype> otList = modell.ByggObjektstruktur(_rep);
            
            //Skriver filformat    
                
            StringBuilder strtResult = new StringBuilder();
            strtResult.Append(@"{\rtf1\ansi\deff0 {\fonttbl {\f0 Courier;}}{\colortbl;\red0\green0\blue0;\red255\green0\blue0;}\tx720\tx1440\tx2880\tx5760");

            foreach (Objekttype o in otList)
            {
                strtResult.Append(@"\trowd \trautofit1 ");
                strtResult.Append(@"\intbl ");
                strtResult.Append(@"\clftsWidth1\cellx1 ");
                strtResult.Append(o.UML_Navn + @"\intbl\cell");
                strtResult.Append(@"\row");
                
                strtResult.Append(@"\trowd \trautofit1 ");
                strtResult.Append(@"\intbl ");
                strtResult.Append(@"\clftsWidth1\cellx1 ");
                strtResult.Append(@"\clftsWidth1\cellx2 ");
                strtResult.Append(@"\clftsWidth1\cellx3 ");
                strtResult.Append(@"\clftsWidth1\cellx4 ");
                strtResult.Append(@"\clftsWidth1\cellx5 ");
                strtResult.Append(@"\clftsWidth1\cellx6 ");
                strtResult.Append(@"\clftsWidth1\cellx7 ");

                strtResult.Append(@"SOSI Egenskapsnavn\intbl\cell UML Egenskapsnavn\intbl\cell Tillatte verdier\intbl\cell E/G\intbl\cell Mult\intbl\cell Data-type\intbl\cell Standard\intbl\cell");
                strtResult.Append(@"\row");
                //TODO rekursiv rutine for å støtte uendelig mange nivåer
                if (o.Inkluder != null)
                {
                    PrintArvetObjekt(strtResult, o.Inkluder);
                }
                
                foreach (Egenskap b in o.Basiselementer)
                {
                    PrintBasiselement(strtResult, b);
                }
                foreach (Gruppeelement g in o.Gruppeelementer)
                {
                    PrintGruppeelement(strtResult, g);
                    
                    foreach (Egenskap b2 in g.Basiselementer)
                    {
                        PrintBasiselement(strtResult, b2);
                    }
                    foreach (Gruppeelement g2 in g.Gruppeelementer)
                    {
                        PrintGruppeelement(strtResult, g2);
                    }
                }
            }
            strtResult.Append(@"}");
            rtResult.Rtf = strtResult.ToString();


            //Skriver til Word?
            if (chWord.Checked)
            {

                Word.Application oWord = new Word.Application();

                Word.Document oDoc = oWord.Documents.Add();

                oWord.Visible = true;
                Object oMissing = System.Reflection.Missing.Value;

                foreach (Objekttype o in otList)
                {
                    //Overskrift
                    int I1 = oDoc.Paragraphs.Count;
                    oDoc.Paragraphs[I1].Range.InsertAfter(o.UML_Navn);
                    //Object styleHeading4 = "Heading 4";
                    //oDoc.Paragraphs[I1].Format.set_Style(ref styleHeading4);
                    
                    //int I2 = oDoc.Paragraphs.Count;
                    //oDoc.Paragraphs[I2].Range.InsertAfter("");
                    Object styleNormal = "Table Grid";
                    //oDoc.Paragraphs[I2].Format.set_Style(ref styleNormal);

                    int I3 = oDoc.Words.Count;
                    oDoc.Words[I3].Select();

                    Word.Table oTable = oDoc.Tables.Add(oDoc.Content.Application.Selection.Range, 1, 7);
                    oTable.set_Style(ref styleNormal);
                    //oTable.Range.ParagraphFormat.SpaceAfter = 6;
                    //oTable.AutoFormat(ref ,ref oMissing);
                    oTable.Cell(1, 1).Range.Text = "SOSI Egenskapsnavn";
                    oTable.Cell(1, 2).Range.Text = "UML Egenskapsnavn";
                    oTable.Cell(1, 3).Range.Text = "Tillatte verdier";
                    oTable.Cell(1, 4).Range.Text = "E/G";
                    oTable.Cell(1, 5).Range.Text = "Mult";
                    oTable.Cell(1, 6).Range.Text = "Data-type";
                    oTable.Cell(1, 7).Range.Text = "Standard";
                    //oTable.Rows[1].Shading.Texture =  Word.WdTextureIndex.wdTexture15Percent;

                    if (o.Inkluder != null)
                    {
                        PrintWordTableArvetObjekt(oTable, o.Inkluder);
                    }


                    foreach (Egenskap b in o.Basiselementer)
                    {
                        PrintWordTableBasiselement(oTable, b);
                        
                    }
                    foreach (Gruppeelement g in o.Gruppeelementer)
                    {
                        PrintWordTableGruppeelement(oTable, g);

                        foreach (Egenskap b2 in g.Basiselementer)
                        {
                            PrintWordTableBasiselement(oTable, b2);
                        }
                        foreach (Gruppeelement g2 in g.Gruppeelementer)
                        {
                            PrintWordTableGruppeelement(oTable, g2);
                        }
                    }


                }
               
            
            }


        }

        private void PrintWordTableArvetObjekt(Word.Table oTable, Objekttype o)
        {
            if (o.Inkluder != null)
            {
                PrintWordTableArvetObjekt(oTable, o.Inkluder);
            }

            foreach (Egenskap b in o.Basiselementer)
            {
                PrintWordTableBasiselement(oTable, b);
            }
            foreach (Gruppeelement g in o.Gruppeelementer)
            {
                PrintWordTableGruppeelement(oTable, g);

                foreach (Egenskap b2 in g.Basiselementer)
                {
                    PrintWordTableBasiselement(oTable, b2);
                }
                foreach (Gruppeelement g2 in g.Gruppeelementer)
                {
                    PrintWordTableGruppeelement(oTable, g2);
                }
            }
        }

        private static void PrintWordTableGruppeelement(Word.Table oTable, Gruppeelement g)
        {
            oTable.Rows.Add();
            int rowcounter = oTable.Rows.Count;
            oTable.Cell(rowcounter, 1).Range.Text = g.SOSI_Navn;
            oTable.Cell(rowcounter, 2).Range.Text = g.UML_Navn;
            oTable.Cell(rowcounter, 3).Range.Text = "*";
            oTable.Cell(rowcounter, 4).Range.Text = "G";
            oTable.Cell(rowcounter, 5).Range.Text = "*";
            oTable.Cell(rowcounter, 6).Range.Text = "*";
            oTable.Cell(rowcounter, 7).Range.Text = g.Standard;
        }

        private static void PrintWordTableBasiselement(Word.Table oTable, Egenskap b)
        {
            oTable.Rows.Add();
            int rowcounter = oTable.Rows.Count;
            oTable.Cell(rowcounter, 1).Range.Text = b.SOSI_Navn;
            oTable.Cell(rowcounter, 2).Range.Text = b.UML_Navn;
            if (b.TillatteVerdier.Count < 10)
                oTable.Cell(rowcounter, 3).Range.Text = String.Join(",", b.TillatteVerdier.ToArray(), 0, b.TillatteVerdier.Count);
            else oTable.Cell(rowcounter, 3).Range.Text = "Kodeliste";
            oTable.Cell(rowcounter, 4).Range.Text = b.TypeSosiElement;
            oTable.Cell(rowcounter, 5).Range.Text = b.Multiplisitet;
            oTable.Cell(rowcounter, 6).Range.Text = b.Datatype;
            oTable.Cell(rowcounter, 7).Range.Text = b.Standard;
        }

        private void PrintArvetObjekt(StringBuilder strtResult, Objekttype o)
        {
            if (o.Inkluder != null)
            {
                PrintArvetObjekt(strtResult, o.Inkluder);
            }

            foreach (Egenskap b in o.Basiselementer)
            {
                PrintBasiselement(strtResult, b);
            }
            foreach (Gruppeelement g in o.Gruppeelementer)
            {
                PrintGruppeelement(strtResult, g);

                foreach (Egenskap b2 in g.Basiselementer)
                {
                    PrintBasiselement(strtResult, b2);
                }
                foreach (Gruppeelement g2 in g.Gruppeelementer)
                {
                    PrintGruppeelement(strtResult, g2);
                }
            }
        }

        private static void PrintBasiselement(StringBuilder strtResult, Egenskap b)
        {
            strtResult.Append(@"\trowd \trautofit1 ");
            strtResult.Append(@"\intbl ");
            strtResult.Append(@"\clftsWidth1\cellx1 ");
            strtResult.Append(@"\clftsWidth1\cellx2 ");
            strtResult.Append(@"\clftsWidth1\cellx3 ");
            strtResult.Append(@"\clftsWidth1\cellx4 ");
            strtResult.Append(@"\clftsWidth1\cellx5 ");
            strtResult.Append(@"\clftsWidth1\cellx6 ");
            strtResult.Append(@"\clftsWidth1\cellx7 ");
            strtResult.Append(b.SOSI_Navn + @"\intbl\cell " + b.UML_Navn + @"\intbl\cell " + String.Join(",", b.TillatteVerdier.ToArray(), 0, b.TillatteVerdier.Count) + @"\intbl\cell " + b.TypeSosiElement + @"\intbl\cell " + b.Multiplisitet + @"\intbl\cell " + b.Datatype + @"\intbl\cell " + b.Standard + @"\intbl\cell ");
            strtResult.Append(@"\row");
        }

        private static void PrintGruppeelement(StringBuilder strtResult, Gruppeelement g2)
        {
            strtResult.Append(@"\trowd \trautofit1 ");
            strtResult.Append(@"\intbl ");
            strtResult.Append(@"\clftsWidth1\cellx1 ");
            strtResult.Append(@"\clftsWidth1\cellx2 ");
            strtResult.Append(@"\clftsWidth1\cellx3 ");
            strtResult.Append(@"\clftsWidth1\cellx4 ");
            strtResult.Append(@"\clftsWidth1\cellx5 ");
            strtResult.Append(@"\clftsWidth1\cellx6 ");
            strtResult.Append(g2.SOSI_Navn + @"\intbl\cell " + g2.UML_Navn + @"\intbl\cell " + " " + @"\intbl\cell " + "G" + @"\intbl\cell " + " " + @"\intbl\cell " + "*" + @"\intbl\cell " + g2.Standard + @"\intbl\cell ");
            strtResult.Append(@"\row");
        }

        



  
    }
}
