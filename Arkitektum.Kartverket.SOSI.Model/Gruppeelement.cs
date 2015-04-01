using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Arkitektum.Kartverket.SOSI.Model
{
    public class Gruppeelement : AbstraktEgenskap
    {

        public Gruppeelement Inkluder { get; set; }
        public List<AbstraktEgenskap> Egenskaper { get; set; }
        public List<Beskrankning> OCLconstraints { get; set; }
        


        public override bool Equals(object obj)
        {
            return this.SOSI_Navn == ((Gruppeelement)obj).SOSI_Navn;
        }
    }
}
