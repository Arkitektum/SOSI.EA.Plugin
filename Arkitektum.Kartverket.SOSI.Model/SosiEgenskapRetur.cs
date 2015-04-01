using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Arkitektum.Kartverket.SOSI.Model
{
    class SosiEgenskapRetur
    {
       
        public string SOSI_Navn { get; set; }
        
        public string SOSI_Lengde { get; set; }
        
        public string SOSI_Datatype { get; set; }
        /// <summary>
        /// Egenskaper som er definert som primærnøkkel. Kan være flere atributte og en samensatt datatype(gruppeelement)
        /// </summary>
        public List<AbstraktEgenskap> Egenskaper { get; set; }
    }
}
