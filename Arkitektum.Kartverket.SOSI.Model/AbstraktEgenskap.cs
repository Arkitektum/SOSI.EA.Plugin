using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Arkitektum.Kartverket.SOSI.Model
{
    public abstract class AbstraktEgenskap
    {
        public string UML_Navn { get; set; }

        private string _sosiNavn;

        public string SOSI_Navn
        {
            get { return _sosiNavn; }
            set { _sosiNavn = value.Trim(); }
        }

        public string Standard { get; set; }
        public string Multiplisitet { get; set; }

        public string Notat { get; set; }
    }
}
