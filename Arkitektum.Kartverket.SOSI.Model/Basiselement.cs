using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Arkitektum.Kartverket.SOSI.Model
{
    public class Basiselement : AbstraktEgenskap
    {
       
        //Lovlige operatorer?
        public string Operator { get; set; }
        public List<string> TillatteVerdier { get; set; } //evt string array for kodelister
        public string Datatype { get; set; }
        
        

        public override bool Equals(object obj)
        {
            return this.SOSI_Navn == ((Basiselement)obj).SOSI_Navn;
        }

        public void LeggTilTillatteVerdier(List<string> verdier)
        {
            TillatteVerdier.AddRange(verdier);
        }
    }
}
