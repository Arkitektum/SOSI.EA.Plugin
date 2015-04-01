using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Arkitektum.Kartverket.SOSI.Model
{
    public abstract class AbstraktEgenskap
    {
        public string UML_Navn { get; set; }
        public string SOSI_Navn { get; set; }
        public string Standard { get; set; }
        public string Multiplisitet { get; set; }

        public string Notat { get; set; }
    }
}
