using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Arkitektum.Kartverket.SOSI.Model
{
    public class Beskrankning : IEquatable<Beskrankning>
    {
        public string Navn { get; set; }
        public string OCL { get; set; }
        public string Notat { get; set; }

        public string OpprinneligFraElementNavn { get; set; }

        public bool ErArvet()
        {
            return !string.IsNullOrWhiteSpace(OpprinneligFraElementNavn);
        }

        public bool Equals(Beskrankning other)
        {
            return Notat.Equals(other.Notat);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Beskrankning) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Navn != null ? Navn.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (OCL != null ? OCL.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (Notat != null ? Notat.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (OpprinneligFraElementNavn != null ? OpprinneligFraElementNavn.GetHashCode() : 0);
                return hashCode;
            }
        }

     
    }
}
