using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Arkitektum.Kartverket.SOSI.Model
{
    /// <summary>
    /// Inneholder kun de som bruker tagged value sosi_verdi. Brukes i Word rapport
    /// </summary>
    public class SosiKodeliste
    {
        public string Navn { get; set; }
        public List<SosiKode> Verdier { get; set; }
    }
}
