using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Arkitektum.Kartverket.SOSI.Model
{
    //stereotype FeatureType men ikke abstracte
    public class Objekttype
    {
        public string UML_Navn { get; set; }
        public string SOSI_Navn { get; set; }
        /// <summary>
        /// Arvet objekt
        /// </summary>
        public Objekttype Inkluder { get; set; }
        /// <summary>
        /// Navnet på pakken objekttypen hører til
        /// </summary>
        public string Standard { get; set; }
        /// <summary>
        /// Alle attributter fra UML modellen
        /// </summary>
        public List<AbstraktEgenskap> Egenskaper { get; set; }

        /// <summary>
        /// Hvilke geometrityper som gjelder
        /// </summary>
        public List<string> Geometrityper { get; set; }

        public List<string> Avgrenser { get; set; }

        public List<string> AvgrensesAv { get; set; }

        public List<Beskrankning> OCLconstraints { get; set; }

        public string Notat { get; set; }

        public Objekttype()
        {
            Egenskaper = new List<AbstraktEgenskap>();
            Geometrityper = new List<string>();
            Avgrenser = new List<string>();
            AvgrensesAv = new List<string>();
            OCLconstraints = new List<Beskrankning>();
        }

        public void LeggTilBeskrankning(Beskrankning bs, string fraObjektNavn = null)
        {
            if (!OCLconstraints.Contains(bs))
            {
                if (fraObjektNavn != null)
                    bs.OpprinneligFraElementNavn = fraObjektNavn;
                OCLconstraints.Add(bs);
            }
        }

        public void LeggTilEgenskap(AbstraktEgenskap egenskap)
        {
            Egenskaper.Add(egenskap);
        }

        public void LeggTilBeskrankninger(List<Beskrankning> list)
        {
            foreach (Beskrankning beskrankning in list)
            {
                LeggTilBeskrankning(beskrankning);
            }
        }

        public bool HarGeometri(string geometri)
        {
            return Geometrityper.Any(g => g.Equals(geometri, StringComparison.InvariantCultureIgnoreCase));
        }

        public bool ErEnFlateavgrensning()
        {
            bool harKurveGeometri = HarGeometri("kurve");
            //string avgrensning = "AVGRENSNING";
            //bool harAvgrensningINavnet = UML_Navn != null && UML_Navn.ToUpperInvariant().Contains(avgrensning);
            //bool harEgenskapSomHeterAvgrensning = Egenskaper.Any(e => e.UML_Navn.ToUpperInvariant().Contains(avgrensning));

            return harKurveGeometri;// && (harAvgrensningINavnet || harEgenskapSomHeterAvgrensning);
        }

        public bool ErFlateavgrensningObjekt()
        {
            return SammenlignMedUmlNavn("Flateavgrensning");
        }

        private bool SammenlignMedUmlNavn(string nameToCompare)
        {
            return string.Equals(UML_Navn, nameToCompare, StringComparison.InvariantCultureIgnoreCase);
        }

        public bool ErKantUtsnittObjekt()
        {
            return SammenlignMedUmlNavn("KantUtsnitt");
        }
    }
}
