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

    }
}
