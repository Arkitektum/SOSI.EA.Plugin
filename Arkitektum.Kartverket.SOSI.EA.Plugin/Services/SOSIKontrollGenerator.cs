using EA;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Arkitektum.Kartverket.SOSI.Model;

namespace Arkitektum.Kartverket.SOSI.EA.Plugin.Services
{
    public class SosiKontrollGenerator
    {
        public void GenererDefFiler(List<Objekttype> liste, Repository repository)
        {
            string produktgruppe = "";
            string kortnavn = "";
            string versjon = "";
            string versjonUtenP = "";
            bool fagområde = false;

            Package valgtPakke = repository.GetTreeSelectedPackage();

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
            if (produktgruppe == "") repository.WriteOutput("System", "FEIL: Mangler tagged value sosi_produktgruppe på applikasjonsskjemapakke " + valgtPakke.Element.Name, 0);
            if (kortnavn == "") repository.WriteOutput("System", "FEIL: Mangler tagged value sosi_kortnavn på applikasjonsskjemapakke " + valgtPakke.Element.Name, 0);
            if (versjon == "") repository.WriteOutput("System", "FEIL: Mangler tagged value version på applikasjonsskjemapakke " + valgtPakke.Element.Name, 0);


            //Lage kataloger
            string eadirectory = Path.GetDirectoryName(repository.ConnectionString);
            string baseDirectory = eadirectory + @"\def\";
            string fullfil = baseDirectory + produktgruppe + @"\kap" + versjonUtenP + @"\" + kortnavn + @"_o." + versjonUtenP;
            string utvalgfil = baseDirectory + produktgruppe + @"\kap" + versjonUtenP + @"\" + kortnavn + @"_u." + versjonUtenP;
            string deffil = baseDirectory + produktgruppe + @"\kap" + versjonUtenP + @"\" + kortnavn + @"_d." + versjonUtenP;
            string defkatalogfil = baseDirectory + produktgruppe + @"\Def_" + kortnavn + "." + versjonUtenP;

            string katalog = Path.GetDirectoryName(fullfil);

            if (!Directory.Exists(katalog))
            {
                Directory.CreateDirectory(katalog);
            }

            using (var file = new StreamWriter(defkatalogfil, false, Encoding.GetEncoding(1252)))
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

            using (var file = new StreamWriter(deffil, false, Encoding.GetEncoding(1252)))
            {
                List<Basiselement> listUnikeBasiselementer = new List<Basiselement>();
                List<Gruppeelement> listUnikeGruppeelementer = new List<Gruppeelement>();

                file.WriteLine("! ***** SOSI - Syntaksdefinisjoner **************!");
                foreach (Objekttype o in liste)
                {
                    LagSosiSyntaks(o, listUnikeBasiselementer, listUnikeGruppeelementer);
                }

                file.WriteLine(LagSosiSyntaksGrupper(listUnikeGruppeelementer));
                file.WriteLine(LagSosiSyntaksBasiselementer(listUnikeBasiselementer));

                /* Remove default types
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

                file.WriteLine("");
                file.WriteLine(".DEF");
                file.WriteLine("..PERIODE *");
                file.WriteLine("...TIDSTART DATOTID");
                file.WriteLine("...TIDSLUTT DATOTID");
                */
            }

            using (var file = new StreamWriter(fullfil, false, Encoding.GetEncoding(1252)))
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

            using (StreamWriter file = new StreamWriter(utvalgfil, false, Encoding.GetEncoding(1252)))
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

            Process.Start(baseDirectory);
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
            StringBuilder builder = new StringBuilder(Environment.NewLine);
            builder.AppendLine(".OBJEKTTYPE");

            if (o.ErFlateavgrensningObjekt())
            {
                builder.Append("..INKLUDER Flateavgrensning");
            }
            else if (o.ErKantUtsnittObjekt())
            {
                builder.Append("..INKLUDER KantUtsnitt");
            }
            else
            {
                builder.Append("..TYPENAVN ").AppendLine(o.UML_Navn);
                if (o.Geometrityper.Count > 0)
                    builder.Append("..GEOMETRITYPE ").AppendLine(string.Join(",", o.Geometrityper.ToArray(), 0, o.Geometrityper.Count));
                if (o.AvgrensesAv.Count > 0)
                    builder.Append("..AVGRENSES_AV ").AppendLine(string.Join(",", o.AvgrensesAv.ToArray(), 0, o.AvgrensesAv.Count));
                if (o.Avgrenser.Count > 0)
                    builder.Append("..AVGRENSER ").AppendLine(string.Join(",", o.Avgrenser.ToArray(), 0, o.Avgrenser.Count));

                builder.Append("..PRODUKTSPEK ").AppendLine(o.Standard.ToUpper());

                if (isFagområde)
                    builder.AppendLine("..INKLUDER SOSI_Objekt");

                foreach (var b1 in o.Egenskaper)
                {
                    builder.Append(LagSosiEgenskap(b1));
                }
                builder.Append(LagSosiArvetObjekt(o));
            }
            return builder.ToString();
        }

       

        private static string LagSosiArvetObjekt(Objekttype o)
        {
            StringBuilder builder = new StringBuilder();
            if (o.Inkluder != null)
            {
                foreach (var b1 in o.Inkluder.Egenskaper)
                {
                    builder.Append(LagSosiEgenskap(b1));
                }

                if (o.Inkluder.Inkluder != null)
                    builder.Append(LagSosiArvetObjekt(o.Inkluder));
            }
            return builder.ToString();
        }


        private static string LagSosiEgenskap(AbstraktEgenskap b1)
        {
            StringBuilder builder = new StringBuilder();
            if (b1 is Basiselement)
            {
                Basiselement b = (Basiselement)b1;
                if (b.TillatteVerdier.Count > 0)
                    builder.Append("..EGENSKAP \"")
                        .Append(b.UML_Navn)
                        .Append("\" * \"")
                        .Append(b.SOSI_Navn)
                        .Append("\"    ")
                        .Append(b.Datatype)
                        .Append("  ")
                        .Append(b.Multiplisitet.Replace("[", "").Replace("]", "").Replace("*", "N").Replace(".", " "))
                        .Append("  ").Append(b.Operator)
                        .Append(" (")
                        .Append(string.Join(",", b.TillatteVerdier.ToArray(), 0, b.TillatteVerdier.Count))
                        .AppendLine(")");
                else if (b.Datatype == "REF")

                    builder.Append("..EGENSKAP \"")
                        .Append(b.UML_Navn)
                        .Append("\" * \"")
                        .Append(b.SOSI_Navn)
                        .Append("\"    ")
                        .Append("*")
                        .Append("  ")
                        .Append(b.Multiplisitet.Replace("[", "").Replace("]", "").Replace("*", "N").Replace(".", " "))
                        .AppendLine("  >< ()");
                else
                    builder.Append("..EGENSKAP \"")
                        .Append(b.UML_Navn)
                        .Append("\" * \"")
                        .Append(b.SOSI_Navn)
                        .Append("\"    ")
                        .Append(b.Datatype)
                        .Append("  ")
                        .Append(b.Multiplisitet.Replace("[", "").Replace("]", "").Replace("*", "N").Replace(".", " "))
                        .AppendLine("  >< ()");

            }
            else
            {
                Gruppeelement g = (Gruppeelement)b1;
                builder.Append("..EGENSKAP \"")
                    .Append(g.UML_Navn)
                    .Append("\" * \"")
                    .Append(g.SOSI_Navn)
                    .Append("\"    *  ")
                    .Append(g.Multiplisitet.Replace("[", "").Replace("]", "").Replace("*", "N").Replace(".", " "))
                    .AppendLine("  >< ()");

                foreach (var b2 in g.Egenskaper)
                {
                    builder.Append(LagSosiEgenskap(b2));
                }
                if (g.Inkluder != null)
                {
                    builder.Append(LagSosiEgenskap(g.Inkluder));
                }
            }
            return builder.ToString();
        }




    }
}
