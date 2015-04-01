using EA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Arkitektum.Kartverket.SOSI.Model;

namespace Arkitektum.Kartverket.SOSI.EA.Plugin.Services
{
    public class SOSIKontrollGenerator
    {
        public void GenererDefFiler(List<Objekttype> liste, Repository _rep)
        {
            string produktgruppe = "";
            string kortnavn = "";
            string versjon = "";
            string versjonUtenP = "";
            bool fagområde = false;

            Package valgtPakke = _rep.GetTreeSelectedPackage();

            foreach (TaggedValue theTags in valgtPakke.Element.TaggedValues)
            {
                switch (theTags.Name.ToLower())
                {
                    case "sosi_spesifikasjonstype":
                        if (theTags.Value.ToLower() == "fagområde") fagområde = true;
                        break;
                    case "sosi_produktgruppe":
                        produktgruppe = theTags.Value.ToLower();
                        break;
                    case "sosi_kortnavn":
                        kortnavn = theTags.Value.ToLower();
                        break;
                    case "version":
                        versjon = theTags.Value;
                        versjonUtenP = versjon.Replace(".", "");
                        break;
                }

            }
            if (produktgruppe == "") _rep.WriteOutput("System", "FEIL: Mangler tagged value sosi_produktgruppe på " + valgtPakke.Element.Name, 0);
            if (kortnavn == "") _rep.WriteOutput("System", "FEIL: Mangler tagged value sosi_kortnavn på " + valgtPakke.Element.Name, 0);
            if (versjon == "") _rep.WriteOutput("System", "FEIL: Mangler tagged value version på " + valgtPakke.Element.Name, 0);


            //Lage kataloger
            string eadirectory = Path.GetDirectoryName(_rep.ConnectionString);
            string fullfil = eadirectory + @"\def\" + produktgruppe + @"\kap" + versjonUtenP + @"\" + kortnavn + @"_o." + versjonUtenP;
            string utvalgfil = eadirectory + @"\def\" + produktgruppe + @"\kap" + versjonUtenP + @"\" + kortnavn + @"_u." + versjonUtenP;
            string deffil = eadirectory + @"\def\" + produktgruppe + @"\kap" + versjonUtenP + @"\" + kortnavn + @"_d." + versjonUtenP;
            string defkatalogfil = eadirectory + @"\def\" + produktgruppe + @"\Def_" + kortnavn + "." + versjonUtenP;

            string katalog = Path.GetDirectoryName(fullfil);

            if (!Directory.Exists(katalog))
            {
                Directory.CreateDirectory(katalog);
            }

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(defkatalogfil, false, Encoding.GetEncoding(1252)))
            {
                file.WriteLine("[SyntaksDefinisjoner]");
                file.WriteLine(deffil.Replace(eadirectory + @"\def\" + produktgruppe, ""));
                file.WriteLine("");
                file.WriteLine("[KodeForklaringer]");
                file.WriteLine(@"\std\KODER." + versjonUtenP);
                file.WriteLine("");
                file.WriteLine("[UtvalgsRegler]");
                file.WriteLine(utvalgfil.Replace(eadirectory + @"\def\" + produktgruppe, ""));
                file.WriteLine("");
                file.WriteLine("[ObjektDefinisjoner]");
                file.WriteLine(fullfil.Replace(eadirectory + @"\def\" + produktgruppe, ""));
            }

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(deffil, false, Encoding.GetEncoding(1252)))
            {
                List<Basiselement> listUnikeBasiselementer = new List<Basiselement>();
                List<Gruppeelement> listUnikeGruppeelementer = new List<Gruppeelement>();

                file.WriteLine("! ***** SOSI - Syntaksdefinisjoner **************!");
                foreach (Objekttype o in liste)
                {
                    file.WriteLine(this.LagSosiSyntaks(o, listUnikeBasiselementer, listUnikeGruppeelementer));
                }

                file.WriteLine(this.LagSosiSyntaksGrupper(listUnikeGruppeelementer));
                file.WriteLine(this.LagSosiSyntaksBasiselementer(listUnikeBasiselementer));


                file.WriteLine("");
                file.WriteLine(".DEF");
                file.WriteLine("..BEZIER S");

                file.WriteLine("");
                file.WriteLine(".DEF");
                file.WriteLine("..BUEP S");

                file.WriteLine("");
                file.WriteLine(".DEF");
                file.WriteLine("..DEF S");

                file.WriteLine("");
                file.WriteLine(".DEF");
                file.WriteLine("..FLATE S");

                file.WriteLine("");
                file.WriteLine(".DEF");
                file.WriteLine("..HODE S");

                file.WriteLine("");
                file.WriteLine(".DEF");
                file.WriteLine("..KLOTOIDE S");

                file.WriteLine("");
                file.WriteLine(".DEF");
                file.WriteLine("..KURVE S");

                file.WriteLine("");
                file.WriteLine(".DEF");
                file.WriteLine("..OBJDEF S");

                file.WriteLine("");
                file.WriteLine(".DEF");
                file.WriteLine("..OBJEKT S");

                file.WriteLine("");
                file.WriteLine(".DEF");
                file.WriteLine("..PUNKT S");

                file.WriteLine("");
                file.WriteLine(".DEF");
                file.WriteLine("..RASTER S");

                file.WriteLine("");
                file.WriteLine(".DEF");
                file.WriteLine("..SIRKELP S");

                file.WriteLine("");
                file.WriteLine(".DEF");
                file.WriteLine("..SLUTT S");

                file.WriteLine("");
                file.WriteLine(".DEF");
                file.WriteLine("..SVERM S");

                file.WriteLine("");
                file.WriteLine(".DEF");
                file.WriteLine("..SYMBOL S");

                file.WriteLine("");
                file.WriteLine(".DEF");
                file.WriteLine("..TEKST S");

                file.WriteLine("");
                file.WriteLine(".DEF");
                file.WriteLine("..TRASE S");

            }

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(fullfil, false, Encoding.GetEncoding(1252)))
            {

                file.WriteLine("! ***************************************************************************!");
                file.WriteLine("! * SOSI-kontroll                                             " + kortnavn.ToUpper() + "-OBJEKTER *!");
                file.WriteLine("! * Objektdefinisjoner for " + kortnavn.ToUpper() + "          				                    *!");
                file.WriteLine("! *                            SOSI versjon  " + versjon + "                   *!");
                file.WriteLine("! ***************************************************************************!");
                file.WriteLine("! *           Følger databeskrivelsene i Del 1, Praktisk bruk               *!");
                file.WriteLine("! *            og databeskrivelsene i Objektkatalogen,                      *!");
                file.WriteLine("! *                   Generert fra SOSI UML modell                          *!");
                file.WriteLine("! *                                                                         *!");
                file.WriteLine("! *               Statens kartverk, SOSI-sekretariatet           " + DateTime.Now.ToShortDateString() + " *!");
                file.WriteLine("! *                          nn                                             *!");
                file.WriteLine("! ***************************************************************************!");
                file.WriteLine("! ------ Antall objekttyper i denne katalogen: " + liste.Count);

                foreach (Objekttype o in liste)
                {
                    file.WriteLine("");
                    file.WriteLine(LagSosiObjekt(o, fagområde));

                }

            }

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(utvalgfil, false, Encoding.GetEncoding(1252)))
            {

                file.WriteLine("! ***************************************************************************!");
                file.WriteLine("! * SOSI-kontroll                                             " + kortnavn.ToUpper() + "-UTVALG *!");
                file.WriteLine("! * Utvalgsregler for " + kortnavn.ToUpper() + "          				                    *!");
                file.WriteLine("! *                            SOSI versjon  " + versjon + "                   *!");
                file.WriteLine("! ***************************************************************************!");
                file.WriteLine("! *           Følger databeskrivelsene i Del 1, Praktisk bruk               *!");
                file.WriteLine("! *            og databeskrivelsene i Objektkatalogen,                      *!");
                file.WriteLine("! *                   Generert fra SOSI UML modell                          *!");
                file.WriteLine("! *                                                                         *!");
                file.WriteLine("! *               Statens kartverk, SOSI-sekretariatet           " + DateTime.Now.ToShortDateString() + " *!");
                file.WriteLine("! *                          nn                                             *!");
                file.WriteLine("! ***************************************************************************!");
                file.WriteLine("! ------ Antall definerte objekttyper i denne katalogen: " + liste.Count);

                foreach (Objekttype o in liste)
                {
                    file.WriteLine("");
                    file.WriteLine(".GRUPPE-UTVALG " + o.UML_Navn);
                    file.WriteLine("..VELG  \"..OBJTYPE\" = " + o.UML_Navn);
                    file.WriteLine("..BRUK-REGEL " + o.UML_Navn);

                }

            }


        }


        internal string LagSosiSyntaks(Objekttype o, List<Basiselement> listeUnikeBasiselementer, List<Gruppeelement> listeUnikeGruppeelementer)
        {
            string tmp = Environment.NewLine;

            foreach (var b in o.Egenskaper)
            {
                tmp = LagSosiSyntaksEgenskap(tmp, b, listeUnikeBasiselementer, listeUnikeGruppeelementer);
            }

            tmp = LagSosiSyntaksArvetObjekt(tmp, o, listeUnikeBasiselementer, listeUnikeGruppeelementer);

            return tmp;
        }

        internal string LagSosiSyntaksGrupper(List<Gruppeelement> listGruppe)
        {
            string tmp = "";
            foreach (Gruppeelement b in listGruppe)
            {
                tmp = tmp + "" + Environment.NewLine;
                tmp = tmp + ".DEF" + Environment.NewLine;
                tmp = LagSosiSyntaksEgenskap(tmp, b, null, null);

            }
            return tmp;
        }

        internal string LagSosiSyntaksBasiselementer(List<Basiselement> listBasis)
        {
            string tmp = "";
            foreach (Basiselement b in listBasis)
            {
                tmp = tmp + "" + Environment.NewLine;
                tmp = tmp + ".DEF" + Environment.NewLine;
                tmp = LagSosiSyntaksEgenskap(tmp, b, null, null);
            }
            return tmp;

        }


        private static string LagSosiSyntaksEgenskap(string tmp, AbstraktEgenskap b1, List<Basiselement> listeUnikeBasiselementer, List<Gruppeelement> listeUnikeGruppeelementer)
        {
            if (b1 is Basiselement)
            {
                Basiselement b = (Basiselement)b1;
                tmp = tmp + b.SOSI_Navn + " " + b.Datatype + Environment.NewLine;
                if (listeUnikeBasiselementer != null) if (listeUnikeBasiselementer.Contains(b) == false) listeUnikeBasiselementer.Add(b);
            }
            else
            {
                Gruppeelement g = (Gruppeelement)b1;
                tmp = tmp + g.SOSI_Navn + " *" + Environment.NewLine;
                if (listeUnikeGruppeelementer != null) if (listeUnikeGruppeelementer.Contains(g) == false) listeUnikeGruppeelementer.Add(g);
                foreach (var b2 in g.Egenskaper)
                {
                    tmp = LagSosiSyntaksEgenskap(tmp, b2, listeUnikeBasiselementer, listeUnikeGruppeelementer);
                }
            }
            return tmp;
        }

        private static string LagSosiSyntaksArvetObjekt(string tmp, Objekttype o, List<Basiselement> listeUnikeBasiselementer, List<Gruppeelement> listeUnikeGruppeelementer)
        {
            if (o.Inkluder != null)
            {

                foreach (var b1 in o.Inkluder.Egenskaper)
                {
                    tmp = LagSosiSyntaksEgenskap(tmp, b1, listeUnikeBasiselementer, listeUnikeGruppeelementer);
                }

                if (o.Inkluder.Inkluder != null) tmp = LagSosiSyntaksArvetObjekt(tmp, o.Inkluder, listeUnikeBasiselementer, listeUnikeGruppeelementer);
            }
            return tmp;

        }

        public string LagSosiObjekt(Objekttype o, bool isFagområde)
        {
            string tmp = Environment.NewLine;

            tmp = tmp + ".OBJEKTTYPE" + Environment.NewLine;
            tmp = tmp + "..TYPENAVN " + o.UML_Navn + Environment.NewLine;
            if (o.Geometrityper.Count > 0) tmp = tmp + "..GEOMETRITYPE " + String.Join(",", o.Geometrityper.ToArray(), 0, o.Geometrityper.Count) + Environment.NewLine;
            if (o.AvgrensesAv.Count > 0) tmp = tmp + "..AVGRENSES_AV " + String.Join(",", o.AvgrensesAv.ToArray(), 0, o.AvgrensesAv.Count) + Environment.NewLine;
            if (o.Avgrenser.Count > 0) tmp = tmp + "..AVGRENSER " + String.Join(",", o.Avgrenser.ToArray(), 0, o.Avgrenser.Count) + Environment.NewLine;

            tmp = tmp + "..PRODUKTSPEK " + o.Standard.ToUpper() + Environment.NewLine;

            if (isFagområde) tmp = tmp + "..INKLUDER SOSI_Objekt" + Environment.NewLine;

            foreach (var b1 in o.Egenskaper)
            {
                tmp = LagSosiEgenskap(tmp, b1);
            }
            tmp = LagSosiArvetObjekt(tmp, o);


            return tmp;

        }
        private static string LagSosiArvetObjekt(string tmp, Objekttype o)
        {
            if (o.Inkluder != null)
            {

                foreach (var b1 in o.Inkluder.Egenskaper)
                {
                    tmp = LagSosiEgenskap(tmp, b1);
                }

                if (o.Inkluder.Inkluder != null) tmp = LagSosiArvetObjekt(tmp, o.Inkluder);
            }
            return tmp;

        }


        private static string LagSosiEgenskap(string tmp, AbstraktEgenskap b1)
        {
            if (b1 is Basiselement)
            {
                Basiselement b = (Basiselement)b1;
                if (b.TillatteVerdier.Count > 0)
                    tmp = tmp + "..EGENSKAP \"" + b.UML_Navn + "\" * \"" + b.SOSI_Navn + "\"    " + b.Datatype + "  " + b.Multiplisitet.Replace("[", "").Replace("]", "").Replace("*", "N").Replace(".", " ") + "  " + b.Operator + " (" + String.Join(",", b.TillatteVerdier.ToArray(), 0, b.TillatteVerdier.Count) + ")" + Environment.NewLine;
                else
                    tmp = tmp + "..EGENSKAP \"" + b.UML_Navn + "\" * \"" + b.SOSI_Navn + "\"    " + b.Datatype + "  " + b.Multiplisitet.Replace("[", "").Replace("]", "").Replace("*", "N").Replace(".", " ") + "  >< ()" + Environment.NewLine;

            }
            else
            {
                Gruppeelement g = (Gruppeelement)b1;
                tmp = tmp + "..EGENSKAP \"" + g.UML_Navn + "\" * \"" + g.SOSI_Navn + "\"    *  " + g.Multiplisitet.Replace("[", "").Replace("]", "").Replace("*", "N").Replace(".", " ") + "  >< ()" + Environment.NewLine;

                foreach (var b2 in g.Egenskaper)
                {
                    tmp = LagSosiEgenskap(tmp, b2);
                }
                if (g.Inkluder != null)
                {
                    tmp = LagSosiEgenskap(tmp, g.Inkluder);
                }
            }
            return tmp;
        }




    }
}
