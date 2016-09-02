using System;
using System.Collections.Generic;
using System.Linq;
using EA;
using Attribute = EA.Attribute;

namespace Arkitektum.Kartverket.SOSI.Model
{
    public class BasiselementBuilder
    {
        private readonly RepositoryHelper _repositoryHelper;
        private readonly string _prikknivå;
        private readonly string _standard;
        private readonly Basiselement _basiselement;
        private Attribute _attributt;

        private static readonly Dictionary<string, MappingAvBasistype> MappingBasistyper = 
            new Dictionary<string, MappingAvBasistype>()
            {
                {"characterstring", new MappingAvBasistype("T")},
                {"integer", new MappingAvBasistype("H")},
                {"real", new MappingAvBasistype("D")},
                {"date", new MappingAvBasistype("DATO")},
                {"datetime", new MappingAvBasistype("DATOTID")},
                {"boolean", new MappingAvBasistype("BOOLSK", "=", "JA", "NEI")},
                {"flate", new MappingAvBasistype("FLATE")},
                {"punkt", new MappingAvBasistype("PUNKT")},
                {"kurve", new MappingAvBasistype("KURVE")},
                {"tm_instant", new MappingAvBasistype("DATOTID")},
                {"tm_period", new MappingAvBasistype("PERIODE")}
            };

        public BasiselementBuilder(Repository repository, string prikknivå, string standard)
        {
            _repositoryHelper = new RepositoryHelper(repository);
            _prikknivå = prikknivå;
            _standard = standard;

            _basiselement = new Basiselement();
        }

        public BasiselementBuilder ForAttributt(Attribute attributt)
        {
            _attributt = attributt;
            _basiselement.UML_Navn = _attributt.Name;
            _basiselement.SOSI_Navn = _prikknivå;
            _basiselement.Notat = _attributt.Notes;
            _basiselement.Standard = _standard;

            _basiselement.Multiplisitet = "[" + _attributt.LowerBound + ".." + _attributt.UpperBound + "]";
            _basiselement.TillatteVerdier = new List<string>();

            _basiselement.Datatype = "FIX";

            return this;
        }

        public BasiselementBuilder MedMappingAvBasistyper()
        {
            string key = _attributt.Type.ToLower();
            if (MappingBasistyper.ContainsKey(key))
            {
                MappingAvBasistype mapping = MappingBasistyper[key];

                string sosiLengde = FinnSosiLengde();
                _basiselement.Datatype = mapping.Datatype + sosiLengde;

                if (!string.IsNullOrWhiteSpace(mapping.OperatorVerdi))
                {
                    _basiselement.Operator = mapping.OperatorVerdi;
                }

                if (mapping.TillatteVerdier.Any())
                {
                    _basiselement.LeggTilTillatteVerdier(mapping.TillatteVerdier);
                }
                else if (_attributt.Default.Length > 0)
                {
                    _basiselement.LeggTilTillatteVerdier(new List<string> { _attributt.Default });
                }
            }
            else
            {
                _repositoryHelper.Log("FEIL: datatype er ikke korrekt/funnet: " + _attributt.Name + " " + _attributt.Type);
            }
            return this;
        }

        public BasiselementBuilder MedMappingAvTomDatatype()
        {
            _repositoryHelper.Log("Advarsel: Attributtet [" + _attributt.Name + "] er av typen " + _attributt.Type + " (tom datatype). Det er ikke anbefalt å benytte datatyper uten attributter, bruk innebygde primitiver istedenfor.");

            string datatype = FinnSosiDatatype();

            string lengde = FinnSosiLengde();

            _basiselement.Datatype = datatype + lengde;

            return this;
        }

        private string FinnSosiLengde()
        {
            string sosiLengde = RepositoryHelper.GetTaggedValue(_attributt.TaggedValues, RepositoryHelper.SosiLengde);

            if (string.IsNullOrWhiteSpace(sosiLengde))
            {
                sosiLengde = RepositoryHelper.GetTaggedValue(_attributt.TaggedValuesEx, RepositoryHelper.SosiLengde);
            }
            
            if (string.IsNullOrWhiteSpace(sosiLengde) && VisFeilmeldingForManglendeSosiLengde())
            {
                _repositoryHelper.Log("FEIL: Mangler tagged value sosi_lengde på attributt: " + _attributt.Name);
            }
            return sosiLengde;
        }

        private bool VisFeilmeldingForManglendeSosiLengde()
        {
            return _attributt.Type.Equals("CharacterString", StringComparison.OrdinalIgnoreCase);
        }

        private string FinnSosiDatatype()
        {
            string sosiDatatype = RepositoryHelper.GetTaggedValue(_attributt.TaggedValues, RepositoryHelper.SosiDatatype);

            if (string.IsNullOrWhiteSpace(sosiDatatype))
            {
                sosiDatatype = RepositoryHelper.GetTaggedValue(_attributt.TaggedValuesEx, RepositoryHelper.SosiDatatype);
            }
            
            if (string.IsNullOrWhiteSpace(sosiDatatype))
            {
                _repositoryHelper.Log("FEIL: Mangler tagged value sosi_datatype på attributt: " + _attributt.Name);
            }
            return sosiDatatype;
        }

        public Basiselement Opprett()
        {
            SettSosiNavn();
            return _basiselement;
        }

        private void SettSosiNavn()
        {
            string sosiNavn = _repositoryHelper.GetSosiNameForAttribute(_attributt);

            if (string.IsNullOrWhiteSpace(sosiNavn))
            {
                _repositoryHelper.Log("FEIL: Mangler tagged value sosi_navn på attributt: " + _attributt.Name);
            }

            _basiselement.SOSI_Navn = _prikknivå + sosiNavn;
        }


        public static bool ErBasistype(string type)
        {
            return MappingBasistyper.ContainsKey(type.ToLowerInvariant());
        }

        
    }

    public class MappingAvBasistype
    {
        public string Datatype { get; }
        public string OperatorVerdi { get; }
        public List<string> TillatteVerdier { get; }

        public MappingAvBasistype(string datatype, string operatorVerdi = null, params string[] tillatteVerdier)
        {
            Datatype = datatype;
            OperatorVerdi = operatorVerdi;
            TillatteVerdier = new List<string>(tillatteVerdier);
        }
    }
}
