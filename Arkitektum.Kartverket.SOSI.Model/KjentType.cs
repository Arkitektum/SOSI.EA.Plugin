namespace Arkitektum.Kartverket.SOSI.Model
{
    public class KjentType
    {
        public string Navn { get; }
        public string Datatype { get; }

        public KjentType(string navn, string datatype)
        {
            Navn = navn;
            Datatype = datatype;
        }
    }
}
