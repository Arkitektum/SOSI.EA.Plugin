using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Office = Microsoft.Office.Core;
using Word = Microsoft.Office.Interop.Word;
using Arkitektum.Kartverket.SOSI.Model;

namespace Arkitektum.Kartverket.SOSI.EA.Plugin.Services
{
    public class WordSOSIRealiseringGenerator
    {
        public void LagWordRapportSosiSyntaks(List<Objekttype> otList, bool isFag, List<SosiKodeliste> kodeList, string pakke)
        {

            try
            {
               
                Word.Application oWord = new Word.Application();

                Word.Document oDoc = oWord.Documents.Add();
                //oDoc.ActiveWindow.View.Type = Word.WdViewType.wdNormalView;
                //oWord.Options.Pagination = false;
                oWord.ScreenUpdating = false;

                oWord.Visible = false;

                int I0 = oDoc.Paragraphs.Count;
                if (isFag) oDoc.Paragraphs[I0].Range.InsertAfter("Fagområde: " + pakke);
                else oDoc.Paragraphs[I0].Range.InsertAfter("Produktspesifikasjon: " + pakke);
                Object styleHeading2 = Word.WdBuiltinStyle.wdStyleHeading2;
                Object styleHeading3 = Word.WdBuiltinStyle.wdStyleHeading3;

                oDoc.Paragraphs[I0].Format.set_Style(ref styleHeading2);
                oDoc.Paragraphs[I0].Range.InsertParagraphAfter();

                Object oMissing = System.Reflection.Missing.Value;

                if (otList.Count > 0)
                {
                    int I1 = oDoc.Paragraphs.Count;
                    oDoc.Paragraphs[I1].Range.InsertAfter("Objekttyper");
                    oDoc.Paragraphs[I1].Format.set_Style(ref styleHeading3);
                    oDoc.Paragraphs[I1].Range.InsertParagraphAfter();
                }

                foreach (Objekttype o in otList)
                {
                    //Overskrift
                    int I1 = oDoc.Paragraphs.Count;
                    oDoc.Paragraphs[I1].Range.InsertAfter(o.UML_Navn);
                    Object styleHeading4 = Word.WdBuiltinStyle.wdStyleHeading4;
                    oDoc.Paragraphs[I1].Format.set_Style(ref styleHeading4);
                    oDoc.Paragraphs[I1].Range.InsertParagraphAfter();

                    int I3 = oDoc.Words.Count;
                    oDoc.Words[I3].Select();
                    Word.Table oTable;
                    if (isFag)
                    {
                        oTable = oDoc.Tables.Add(oDoc.Content.Application.Selection.Range, 1, 6);
                        oTable.Columns[1].Width = 110;
                        
                        oTable.Columns[2].Width = 110;
                        
                        oTable.Columns[3].Width = 110;
                       
                        oTable.Columns[4].Width = 40;
                       
                        oTable.Columns[5].Width = 40;
                        
                        oTable.Columns[6].Width = 100;
                    }
                    else
                    {
                        oTable = oDoc.Tables.Add(oDoc.Content.Application.Selection.Range, 1, 5);
                        oTable.Columns[1].Width = 140;
                        
                        oTable.Columns[2].Width = 140;
                        
                        oTable.Columns[3].Width = 130;
                        
                        oTable.Columns[4].Width = 50;
                        
                        oTable.Columns[5].Width = 50;
                        

                    }

                    oTable.Borders.OutsideColor = Word.WdColor.wdColorBlack;
                    oTable.Borders.OutsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
                    oTable.Borders.InsideColor = Word.WdColor.wdColorBlack;
                    oTable.Borders.InsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;

                    oTable.Cell(1, 1).Range.Text = "UML Egenskapsnavn";
                    oTable.Cell(1, 1).Range.Bold = 1;
                    oTable.Cell(1, 1).Range.Shading.BackgroundPatternColor = Word.WdColor.wdColorGray05;

                    oTable.Cell(1, 2).Range.Text = "SOSI Egenskapsnavn";
                    oTable.Cell(1, 2).Range.Bold = 1;
                    oTable.Cell(1, 2).Range.Shading.BackgroundPatternColor = Word.WdColor.wdColorGray05;
                    oTable.Cell(1, 3).Range.Text = "Tillatte verdier";
                    oTable.Cell(1, 3).Range.Bold = 1;
                    oTable.Cell(1, 3).Range.Shading.BackgroundPatternColor = Word.WdColor.wdColorGray05;
                   
                    oTable.Cell(1, 4).Range.Text = "Mult";
                    oTable.Cell(1, 4).Range.Bold = 1;
                    oTable.Cell(1, 4).Range.Shading.BackgroundPatternColor = Word.WdColor.wdColorGray05;
                    oTable.Cell(1, 5).Range.Text = "SOSI-type";
                    oTable.Cell(1, 5).Range.Bold = 1;
                    oTable.Cell(1, 5).Range.Shading.BackgroundPatternColor = Word.WdColor.wdColorGray05;
                    //Hvis det er fagområde så er denne aktuell, ikke på produktspek.
                    if (isFag)
                    {
                        oTable.Cell(1, 6).Range.Text = "Standard";
                        oTable.Cell(1, 6).Range.Bold = 1;
                        oTable.Cell(1, 6).Range.Shading.BackgroundPatternColor = Word.WdColor.wdColorGray05;
                    }

                    if (o.Geometrityper.Count > 0) PrintWordTableGeometri(oTable, o.Geometrityper, isFag);

                    foreach (AbstraktEgenskap b in o.Egenskaper)
                    {
                        PrintWordTableEgenskap(oTable, b, isFag);

                    }

                    if (o.Inkluder != null)
                    {
                        PrintWordTableArvetObjekt(oTable, o, isFag);
                    }

                    if (o.OCLconstraints.Count > 0 || o.Avgrenser.Count > 0 || o.AvgrensesAv.Count > 0)
                    {
                        
                        oTable.Rows.Add();
                        int rowcounter2 = oTable.Rows.Count;
                        oTable.Rows[rowcounter2].Cells.Merge();
                        oTable.Cell(rowcounter2, 1).Range.Text = "Restriksjoner";
                        oTable.Cell(rowcounter2, 1).Range.Bold = 1;
                        oTable.Cell(rowcounter2, 1).Range.Shading.BackgroundPatternColor = Word.WdColor.wdColorGray05;
                        

                        if (o.Avgrenser.Count > 0)
                        {
                            oTable.Rows.Add();
                            int rowcounter = oTable.Rows.Count;
                            oTable.Cell(rowcounter, 1).Range.Text = "Avgrenser: " + String.Join(",", o.Avgrenser.ToArray(), 0, o.Avgrenser.Count);
                            oTable.Cell(rowcounter, 1).Range.Bold = 0;
                            oTable.Cell(rowcounter, 1).Range.Shading.BackgroundPatternColor = Word.WdColor.wdColorAutomatic;
 
                        }
                        if (o.AvgrensesAv.Count > 0)
                        {
                            oTable.Rows.Add();
                            int rowcounter = oTable.Rows.Count;
                            oTable.Cell(rowcounter, 1).Range.Text = "Avgrenses av: " + String.Join(",", o.AvgrensesAv.ToArray(), 0, o.AvgrensesAv.Count);
                            oTable.Cell(rowcounter, 1).Range.Bold = 0;
                            oTable.Cell(rowcounter, 1).Range.Shading.BackgroundPatternColor = Word.WdColor.wdColorAutomatic;
                        }

                        foreach (Beskrankning b in o.OCLconstraints)
                        {
                            oTable.Rows.Add();
                            int rowcounter = oTable.Rows.Count;
                            oTable.Cell(rowcounter, 1).Range.Text = b.Navn + ": " + b.Notat;
                            oTable.Cell(rowcounter, 1).Range.Bold = 0;
                            oTable.Cell(rowcounter, 1).Range.Shading.BackgroundPatternColor = Word.WdColor.wdColorAutomatic;
                        }
                       
                    }
                }

                if (kodeList.Count > 0)
                {
                    int I1 = oDoc.Paragraphs.Count;
                    oDoc.Paragraphs[I1].Range.InsertAfter("Kodelister");
                    oDoc.Paragraphs[I1].Format.set_Style(ref styleHeading3);
                    oDoc.Paragraphs[I1].Range.InsertParagraphAfter();
                }

                foreach (SosiKodeliste k in kodeList)
                {
                    //Overskrift
                    int I1 = oDoc.Paragraphs.Count;
                    oDoc.Paragraphs[I1].Range.InsertAfter(k.Navn);
                    Object styleHeading4 = Word.WdBuiltinStyle.wdStyleHeading4;
                    oDoc.Paragraphs[I1].Format.set_Style(ref styleHeading4);
                    oDoc.Paragraphs[I1].Range.InsertParagraphAfter();

                   

                    int I3 = oDoc.Words.Count;
                    oDoc.Words[I3].Select();
                    Word.Table oTable;
                    oTable = oDoc.Tables.Add(oDoc.Content.Application.Selection.Range, 1, 3);
                    oTable.Borders.OutsideColor = Word.WdColor.wdColorBlack;
                    oTable.Borders.OutsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
                    oTable.Borders.InsideColor = Word.WdColor.wdColorBlack;
                    oTable.Borders.InsideLineStyle = Word.WdLineStyle.wdLineStyleSingle;
                    oTable.Columns[1].Width = 100;
                    oTable.Columns[2].Width = 110;
                    oTable.Columns[3].Width = 300;
                    
                    oTable.Cell(1, 1).Range.Text = "Kode";
                    oTable.Cell(1, 1).Range.Bold = 1;
                    oTable.Cell(1, 1).Range.Shading.BackgroundPatternColor = Word.WdColor.wdColorGray05;
                    oTable.Cell(1, 2).Range.Text = "Navn";
                    oTable.Cell(1, 2).Range.Bold = 1;
                    oTable.Cell(1, 2).Range.Shading.BackgroundPatternColor = Word.WdColor.wdColorGray05;
                    oTable.Cell(1, 3).Range.Text = "Beskrivelse";
                    oTable.Cell(1, 3).Range.Bold = 1;
                    oTable.Cell(1, 3).Range.Shading.BackgroundPatternColor = Word.WdColor.wdColorGray05;

                    foreach (SosiKode o in k.Verdier)
                    {
                        oTable.Rows.Add();
                        int rowcounter = oTable.Rows.Count;
                        oTable.Cell(rowcounter, 1).Range.Text = o.SosiVerdi;
                        oTable.Cell(rowcounter, 2).Range.Text = o.Navn;
                        oTable.Cell(rowcounter, 1).Range.Bold = 0;
                        oTable.Cell(rowcounter, 1).Range.Shading.BackgroundPatternColor = Word.WdColor.wdColorAutomatic;
                        oTable.Cell(rowcounter, 2).Range.Bold = 0;
                        oTable.Cell(rowcounter, 2).Range.Shading.BackgroundPatternColor = Word.WdColor.wdColorAutomatic;
                        oTable.Cell(rowcounter, 3).Range.Text = o.Beskrivelse;
                        oTable.Cell(rowcounter, 3).Range.Bold = 0;
                        oTable.Cell(rowcounter, 3).Range.Shading.BackgroundPatternColor = Word.WdColor.wdColorAutomatic;
                    }

                }
                oWord.ScreenUpdating = true;
                oWord.Visible = true;

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + " " + e.Source);
            }

        }

        private void PrintWordTableEgenskap(Word.Table oTable, AbstraktEgenskap b1, bool isFag)
        {
            if (b1 is Basiselement)
            {
                Basiselement b = (Basiselement)b1;
                PrintWordTableBasiselement(oTable, b, isFag);
            }
            else
            {
                Gruppeelement g = (Gruppeelement)b1;
                PrintWordTableGruppeelement(oTable, g, isFag);

                foreach (var b2 in g.Egenskaper)
                {
                    PrintWordTableEgenskap(oTable, b2, isFag);
                }
                if (g.Inkluder != null)
                {
                    PrintWordTableArvetGruppeelement(oTable, g, isFag);
                }
            }
        }
        private void PrintWordTableArvetGruppeelement(Word.Table oTable, Gruppeelement o, bool isFag)
        {
            if (o.Inkluder != null)
            {

                foreach (var b1 in o.Inkluder.Egenskaper)
                {
                    PrintWordTableEgenskap(oTable, b1, isFag);
                }

                if (o.Inkluder.Inkluder != null) PrintWordTableArvetGruppeelement(oTable, o.Inkluder, isFag);
            }
        }

        private void PrintWordTableArvetObjekt(Word.Table oTable, Objekttype o, bool isFag)
        {
            if (o.Inkluder != null)
            {

                foreach (var b1 in o.Inkluder.Egenskaper)
                {
                    PrintWordTableEgenskap(oTable, b1, isFag);
                }

                if (o.Inkluder.Inkluder != null) PrintWordTableArvetObjekt(oTable, o.Inkluder, isFag);
            }
        }

        private static void PrintWordTableGruppeelement(Word.Table oTable, Gruppeelement g, bool isFag)
        {
            oTable.Rows.Add();
            int rowcounter = oTable.Rows.Count;
            oTable.Cell(rowcounter, 1).Range.Text = g.UML_Navn;
            oTable.Cell(rowcounter, 1).Range.Bold = 0;
            oTable.Cell(rowcounter, 1).Range.Shading.BackgroundPatternColor = Word.WdColor.wdColorAutomatic;
            oTable.Cell(rowcounter, 2).Range.Text = g.SOSI_Navn;
            oTable.Cell(rowcounter, 2).Range.Bold = 0;
            oTable.Cell(rowcounter, 2).Range.Shading.BackgroundPatternColor = Word.WdColor.wdColorAutomatic;
            oTable.Cell(rowcounter, 3).Range.Text = "*";
            oTable.Cell(rowcounter, 3).Range.Bold = 0;
            oTable.Cell(rowcounter, 3).Range.Shading.BackgroundPatternColor = Word.WdColor.wdColorAutomatic;
            
            oTable.Cell(rowcounter, 4).Range.Text = g.Multiplisitet;
            oTable.Cell(rowcounter, 4).Range.Bold = 0;
            oTable.Cell(rowcounter, 4).Range.Shading.BackgroundPatternColor = Word.WdColor.wdColorAutomatic;
            oTable.Cell(rowcounter, 5).Range.Text = "*";
            oTable.Cell(rowcounter, 5).Range.Bold = 0;
            oTable.Cell(rowcounter, 5).Range.Shading.BackgroundPatternColor = Word.WdColor.wdColorAutomatic;
            if (isFag)
            {
                oTable.Cell(rowcounter, 6).Range.Text = g.Standard;
                oTable.Cell(rowcounter, 6).Range.Bold = 0;
                oTable.Cell(rowcounter, 6).Range.Shading.BackgroundPatternColor = Word.WdColor.wdColorAutomatic;
            }
        }

        private static void PrintWordTableGeometri(Word.Table oTable, List<string> geom, bool isFag)
        {
            oTable.Rows.Add();
            int rowcounter = oTable.Rows.Count;
            oTable.Cell(rowcounter, 1).Range.Text = "Geometri";
            oTable.Cell(rowcounter, 1).Range.Bold = 0;
            oTable.Cell(rowcounter, 1).Range.Shading.BackgroundPatternColor = Word.WdColor.wdColorAutomatic;
            oTable.Cell(rowcounter, 2).Range.Text = String.Join(",", geom.ToArray(), 0, geom.Count);
            oTable.Cell(rowcounter, 2).Range.Bold = 0;
            oTable.Cell(rowcounter, 2).Range.Shading.BackgroundPatternColor = Word.WdColor.wdColorAutomatic;
           
            oTable.Cell(rowcounter, 3).Range.Bold = 0;
            oTable.Cell(rowcounter, 3).Range.Shading.BackgroundPatternColor = Word.WdColor.wdColorAutomatic;
            
            oTable.Cell(rowcounter, 4).Range.Text = "";
            oTable.Cell(rowcounter, 4).Range.Bold = 0;
            oTable.Cell(rowcounter, 4).Range.Shading.BackgroundPatternColor = Word.WdColor.wdColorAutomatic;
            oTable.Cell(rowcounter, 5).Range.Text = "";
            oTable.Cell(rowcounter, 5).Range.Bold = 0;
            oTable.Cell(rowcounter, 5).Range.Shading.BackgroundPatternColor = Word.WdColor.wdColorAutomatic;
            if (isFag)
            {
                oTable.Cell(rowcounter, 6).Range.Text = "";
                oTable.Cell(rowcounter, 6).Range.Bold = 0;
                oTable.Cell(rowcounter, 6).Range.Shading.BackgroundPatternColor = Word.WdColor.wdColorAutomatic;
            }
        }

        private static void PrintWordTableBasiselement(Word.Table oTable, Basiselement b, bool isFag)
        {
            oTable.Rows.Add();
            int rowcounter = oTable.Rows.Count;
            oTable.Cell(rowcounter, 1).Range.Text = b.UML_Navn;
            oTable.Cell(rowcounter, 1).Range.Bold = 0;
            oTable.Cell(rowcounter, 1).Range.Shading.BackgroundPatternColor = Word.WdColor.wdColorAutomatic;
            oTable.Cell(rowcounter, 2).Range.Text = b.SOSI_Navn;
            oTable.Cell(rowcounter, 2).Range.Bold = 0;
            oTable.Cell(rowcounter, 2).Range.Shading.BackgroundPatternColor = Word.WdColor.wdColorAutomatic;
            if (b.TillatteVerdier.Count < 10)
                oTable.Cell(rowcounter, 3).Range.Text = b.Operator + String.Join(",", b.TillatteVerdier.ToArray(), 0, b.TillatteVerdier.Count);
            else oTable.Cell(rowcounter, 3).Range.Text = "Kodeliste";
            oTable.Cell(rowcounter, 3).Range.Bold = 0;
            oTable.Cell(rowcounter, 3).Range.Shading.BackgroundPatternColor = Word.WdColor.wdColorAutomatic;
            
            oTable.Cell(rowcounter, 4).Range.Text = b.Multiplisitet;
            oTable.Cell(rowcounter, 4).Range.Bold = 0;
            oTable.Cell(rowcounter, 4).Range.Shading.BackgroundPatternColor = Word.WdColor.wdColorAutomatic;
            oTable.Cell(rowcounter, 5).Range.Text = b.Datatype;
            oTable.Cell(rowcounter, 5).Range.Bold = 0;
            oTable.Cell(rowcounter, 5).Range.Shading.BackgroundPatternColor = Word.WdColor.wdColorAutomatic;
            if (isFag)
            {
                oTable.Cell(rowcounter, 6).Range.Text = b.Standard;
                oTable.Cell(rowcounter, 6).Range.Bold = 0;
                oTable.Cell(rowcounter, 6).Range.Shading.BackgroundPatternColor = Word.WdColor.wdColorAutomatic;
            }
        }
    }
}
