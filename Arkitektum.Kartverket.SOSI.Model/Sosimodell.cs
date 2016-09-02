using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using EA;
using Attribute = EA.Attribute;

namespace Arkitektum.Kartverket.SOSI.Model
{
    public class Sosimodell
    {
        private static readonly Dictionary<string, KjentType> KjenteTyper = new Dictionary<string, KjentType>
        {
            {"TM_Instant", new KjentType("TM_Instant", "DATOTID") },
            {"TM_Period", new KjentType("TM_Period", "PERIODE") }
        };

        private readonly Repository _repository;
  
        private List<Objekttype> _objekttyper;
        public Sosimodell(Repository repository)
        {
            this._repository = repository;
        }

        public List<Objekttype> ByggObjektstruktur()
        {
            _objekttyper = new List<Objekttype>();

            Package valgtPakke = _repository.GetTreeSelectedPackage();

            _objekttyper = LagObjekttyperForElementerIPakke(valgtPakke);

            if (SkalLeggeTilFlateavgrensning(_objekttyper))
            {
                Logg("Legger til Flateavgrensning");
                _objekttyper.Add(OpprettFlateavgrensning(HentApplicationSchemaPakkeNavn(valgtPakke.Element)));
            }

            if (SkalLeggeTilKantUtsnitt(_objekttyper))
            {
                Logg("Ingen objekter med navn KantUtsnitt funnet. Legger til KantUtsnitt");
                _objekttyper.Add(OpprettKantUtsnitt(HentApplicationSchemaPakkeNavn(valgtPakke.Element)));
            }


            return _objekttyper;
        }
        
        private bool SkalLeggeTilKantUtsnitt(List<Objekttype> objekttyper)
        {
            return objekttyper.Any(o => o.HarGeometri("flate"))
                && !objekttyper.Any(o => string.Equals(o.UML_Navn, "KantUtsnitt", StringComparison.InvariantCultureIgnoreCase));
        }

        private Objekttype OpprettKantUtsnitt(string standard)
        {
            return new Objekttype()
            {
                UML_Navn = "KantUtsnitt",
                Geometrityper = new List<string>() { "KURVE" },
                Standard = standard,
                Egenskaper = new List<AbstraktEgenskap>()
                {
                    new Basiselement()
                    {
                        SOSI_Navn = "..OBJTYPE",
                        Operator =  "=",
                        TillatteVerdier = new List<string> {"KantUtsnitt"},
                        Multiplisitet = "[1..1]",
                        Datatype = "T12"
                    }
                },
                OCLconstraints = new List<Beskrankning>
                {
                    new Beskrankning()
                    {
                        Navn = "KantUtsnitt",
                        Notat = "Objekttypen kan forekomme som et resultat av klipping av datasettet."
                    }
                }
            };
        }

        private bool SkalLeggeTilFlateavgrensning(List<Objekttype> objekttyper)
        {
            bool skalLeggeTilAvgrensning = true;
            foreach(Objekttype objekttype in objekttyper)
            {
                if (objekttype.HarGeometri("flate"))
                {
                    foreach (string avgrensesAv in objekttype.AvgrensesAv)
                    {
                        Objekttype avgrensesAvObjekttype = FinnObjekttypeMedNavn(objekttyper, avgrensesAv);
                        if (avgrensesAvObjekttype == null)
                        {
                            LoggDebug("Finner ikke objektet [{avgrensesAv}] som avgrenser objekttypen [{objekttype.UML_Navn}]");
                            continue;
                        }
                        if (avgrensesAvObjekttype.ErEnFlateavgrensning())
                        {
                            Logg($"Flaten i {objekttype.UML_Navn} blir avgrenset av kurven i {avgrensesAvObjekttype.UML_Navn}. Legger ikke til egen flateavgrensning.");
                            skalLeggeTilAvgrensning = false;
                            break;
                        }
                    }
                }
               else skalLeggeTilAvgrensning = false;
            }

            return skalLeggeTilAvgrensning;
        }

        private void LoggDebug(string melding)
        {
#if DEBUG
            _repository.WriteOutput("System", $"DEBUG: {melding}", 0);
#endif
        }

        private void Logg(string melding)
        {
            _repository.WriteOutput("System", melding, 0);
        }

        private Objekttype FinnObjekttypeMedNavn(List<Objekttype> objekttyper, string navn)
        {
            return objekttyper.FirstOrDefault(o => string.Equals(o.UML_Navn, navn, StringComparison.InvariantCultureIgnoreCase));
        }

        private static Objekttype OpprettFlateavgrensning(string standard)
        {
            return new Objekttype()
            {
                UML_Navn = "Flateavgrensning",
                Geometrityper = new List<string>() { "KURVE" },
                Standard = standard,
                Egenskaper = new List<AbstraktEgenskap>()
                {
                    new Basiselement()
                    {
                        SOSI_Navn = "..OBJTYPE",
                        Operator =  "=",
                        TillatteVerdier = new List<string> {"Flateavgrensning"},
                        Multiplisitet = "[1..1]",
                        Datatype = "T18"
                    }
                },
                OCLconstraints = new List<Beskrankning>
                {
                    new Beskrankning()
                    {
                        Navn = "Flateavgrensning",
                        Notat = "Objekttypen er lagt til for å avgrense flaten for å tilfredsstille geometrimodellen i SOSI-formatet."
                    }
                }
            };
        }

        private List<Objekttype> LagObjekttyperForElementerIPakke(Package pakke)
        {
            List<Objekttype> objekttyper = new List<Objekttype>();

            foreach (Element element in pakke.Elements)
            {
                if (SkalLageObjekttypeAvElement(element))
                {
                    objekttyper.Add(LagObjekttype(element, "..", true, null));
                }
            }
            foreach (Package underpakke in pakke.Packages)
            {
                objekttyper.AddRange(LagObjekttyperForElementerIPakke(underpakke));
            }
            return objekttyper;
        }

        private static bool SkalLageObjekttypeAvElement(Element el)
        {
            return el.Type == "Class" && (el.Stereotype.ToLower() == "featuretype" || el.Stereotype.ToLower() == "type") && el.Abstract == "0";
        }
        

        public Objekttype LagObjekttype(Element element, string prikknivå, bool lagobjekttype, List<Beskrankning> oclfraSubObjekt)
        {
            Objekttype objekttype = new Objekttype();
            objekttype.Egenskaper = new List<AbstraktEgenskap>();
            objekttype.Geometrityper = new List<string>();
            objekttype.OCLconstraints = new List<Beskrankning>();
            objekttype.Avgrenser= new List<string>();
            objekttype.AvgrensesAv = new List<string>();

            objekttype.UML_Navn = element.Name;
            objekttype.Notat = element.Notes;
            objekttype.SOSI_Navn = prikknivå;
            string standard = HentApplicationSchemaPakkeNavn(element);
            objekttype.Standard = standard;
            if (lagobjekttype)
            {
                Basiselement basiselement = new Basiselement();
                basiselement.Standard = standard;
                basiselement.SOSI_Navn = prikknivå + "OBJTYPE";
                basiselement.UML_Navn = "";
                
                basiselement.Operator = "=";
                basiselement.TillatteVerdier = new List<string>();
                basiselement.TillatteVerdier.Add(element.Name);
                basiselement.Multiplisitet = "[1..1]";
                basiselement.Datatype = "T32";
                objekttype.Egenskaper.Add(basiselement);

                if (element.Name.Length > 32)
                    _repository.WriteOutput("System", "FEIL: Objektnavn er lengre enn 32 tegn - " + element.Name, 0);
            }

            if (oclfraSubObjekt != null)
            {
                foreach (Beskrankning beskrankning in oclfraSubObjekt)
                {
                    objekttype.LeggTilBeskrankning(beskrankning);
                }
            }

            objekttype.LeggTilBeskrankninger(LagBeskrankninger(element));

            foreach (Attribute att in element.Attributes)
            {
                try
                {
                    if (att.Type.ToLower() == "flate" || att.Type.ToLower() == "punkt" || att.Type.ToLower() == "sverm")
                    {
                        objekttype.Geometrityper.Add(att.Type.ToUpper());
                    }
                    else if (att.Type.ToLower() == "kurve")
                    {
                        objekttype.Geometrityper.Add("KURVE");
                        objekttype.Geometrityper.Add("BUEP");
                        objekttype.Geometrityper.Add("SIRKELP");
                        objekttype.Geometrityper.Add("BEZIER");
                        objekttype.Geometrityper.Add("KLOTOIDE");
                    }
                    else
                    {
                        var basiselementBuilder =
                                new BasiselementBuilder(_repository, prikknivå, standard).ForAttributt(att);

                        if (att.ClassifierID != 0)
                        {
                            Element elm = _repository.GetElementByID(att.ClassifierID);
                            
                            if (BasiselementBuilder.ErBasistype(att.Type))
                            {
                                Basiselement basiselement = basiselementBuilder.MedMappingAvBasistyper().Opprett();
                                objekttype.LeggTilEgenskap(basiselement);
                            }
                            else if (elm.Stereotype.ToLower() == "codelist" || elm.Stereotype.ToLower() == "enumeration" ||
                                     elm.Type.ToLower() == "enumeration")
                            {
                                objekttype.LeggTilEgenskap(LagKodelisteEgenskap(prikknivå, elm, att,
                                    objekttype.OCLconstraints));
                            }
                            else if (elm.Stereotype.ToLower() == "union")
                            {
                                LagUnionEgenskaper(prikknivå, elm, att, objekttype, standard);
                            }
                            else
                            {
                                objekttype.LeggTilEgenskap(LagGruppeelement(elm, att, prikknivå, objekttype));
                            }
                        }
                        else
                        {
                            Basiselement basiselement = basiselementBuilder.MedMappingAvBasistyper().Opprett();
                            objekttype.LeggTilEgenskap(basiselement);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _repository.WriteOutput("System", "FEIL: Finner ikke datatype for " + att.Name + " på " + element.Name + " :" + ex.Message, 0);
                }

            }

            foreach (Connector connector in element.Connectors)
            {
                if (connector.MetaType == "Association" || connector.MetaType == "Aggregation" || connector.MetaType == "topo")
                {
                    Element source = _repository.GetElementByID(connector.SupplierID);
                    Element destination = _repository.GetElementByID(connector.ClientID);
                    bool is_source = false;
                    //Navigerbarhet på rollenavn
                    if (source.Name == element.Name) is_source = true;
                    else is_source = false;

                    if (connector.Stereotype.ToLower() == "topo" || connector.MetaType == "topo")
                    {
                        addTopo(element, objekttype, connector, source, destination);
                    }
                    else if (connector.SupplierEnd.Aggregation == 2 && connector.Direction == "Destination -> Source" && is_source == true) //Composite
                    {
                        Gruppeelement tmp = LagGruppeelementKomposisjon(connector, prikknivå, objekttype);
                        objekttype.Egenskaper.Add(tmp);
                    }
                    else if (connector.ClientEnd.Aggregation == 2 && connector.Direction == "Source -> Destination" && is_source == false) //Composite
                    {
                        Gruppeelement tmp = LagGruppeelementKomposisjon(connector, prikknivå, objekttype);
                        objekttype.Egenskaper.Add(tmp);
                    }
                    else
                    {
                        //if (connector.SupplierID == connector.ClientID)
                        //{
                            List<AbstraktEgenskap> eg = LagConnectorEgenskaper(prikknivå, connector, element, standard, objekttype);
                            objekttype.Egenskaper.AddRange(eg);
                        //}
                        //else if (connector.Direction == "Bi-Directional")
                        //{
                        //    List<AbstraktEgenskap> eg = LagConnectorEgenskaper(prikknivå, connector, e, _rep, standard, ot);
                        //    ot.Egenskaper.AddRange(eg);
                        //}
                        //else if (connector.Direction == "Source -> Destination" && is_source == false)
                        //{
                        //    List<AbstraktEgenskap> eg = LagConnectorEgenskaper(prikknivå, connector, e, _rep, standard, ot);
                        //    ot.Egenskaper.AddRange(eg);
                        //}
                        //else if (connector.Direction == "Destination -> Source" && is_source == true)
                        //{
                        //    List<AbstraktEgenskap> eg = LagConnectorEgenskaper(prikknivå, connector, e, _rep, standard, ot);
                        //    ot.Egenskaper.AddRange(eg);
                        //}
                        //else if (connector.Direction == "Unspecified")
                        //{
                        //    _repository.WriteOutput("System", "ADVARSEL: Assosiasjonen mangler angivelse av 'Direction' mellom " + source.Name + " og " + destination.Name, 0);
                        //}


                    }
                }
                else if (connector.MetaType == "Generalization")
                {
                    Element elm = _repository.GetElementByID(connector.SupplierID);
                    if (element.Name != elm.Name)
                    {
                        Objekttype tmp2 = LagObjekttype(elm, prikknivå, false, null);
                        objekttype.Inkluder = tmp2;
                        foreach (string geo in tmp2.Geometrityper)
                        {
                            if (!objekttype.Geometrityper.Contains(geo))
                                objekttype.Geometrityper.Add(geo);
                        }
                        foreach (string obj in tmp2.Avgrenser)
                        {
                            if (!objekttype.Avgrenser.Contains(obj))
                                objekttype.Avgrenser.Add(obj);
                        }
                        foreach (string obj in tmp2.AvgrensesAv)
                        {
                            if (!objekttype.AvgrensesAv.Contains(obj))
                                objekttype.AvgrensesAv.Add(obj);
                        }
                        
                        foreach (Beskrankning b in tmp2.OCLconstraints)
                        {
                            objekttype.LeggTilBeskrankning(b, tmp2.UML_Navn);
                        }
                        
                    }

                }
            }

            return objekttype;

        }

        private static List<Beskrankning> LagBeskrankninger(Element element)
        {
            List<Beskrankning> beskrankninger = new List<Beskrankning>();

            foreach (global::EA.Constraint constraint in element.Constraints)
            {
                Beskrankning beskrankning = new Beskrankning();
                beskrankning.Navn = constraint.Name;
                string ocldesc = "";
                if (constraint.Notes.Contains("/*") && constraint.Notes.Contains("*/"))
                {
                    var notesInLowercase = constraint.Notes.ToLower();
                    ocldesc = constraint.Notes.Substring(notesInLowercase.IndexOf("/*") + 2,
                        notesInLowercase.IndexOf("*/") - 2 - notesInLowercase.IndexOf("/*"));
                }
                beskrankning.Notat = ocldesc;
                beskrankning.OCL = constraint.Notes;

                beskrankninger.Add(beskrankning);
            }
            return beskrankninger;
        }

        private bool ErTomDatatype(Element element)
        {
            return element.Attributes.Count == 0;
        }

        private bool ErKjentType(string type)
        {
            return KjenteTyper.ContainsKey(type);
        }

        private Basiselement LagEgenskapForKjentType(string prikknivå, Attribute attributt, string standard)
        {
            KjentType kjentTypeForAttributt = KjenteTyper[attributt.Type];
            return LagBasiselementForKjentType(prikknivå, attributt, standard, kjentTypeForAttributt);
        }

        private void addTopo(Element e, Objekttype ot, Connector connector, Element source, Element destination)
        {
            //TODO Skal ikke avgrense på abstrakte objekter kun på realiserbare
            if (connector.Direction == "Bi-Directional")
            {
                _repository.WriteOutput("System", "FEIL: Topo assosiasjonen kan ikke ha 'Bi-Directional' mellom " + source.Name + " og " + destination.Name, 0);
            }
            else if (connector.Direction == "Source -> Destination")
            {
                if (source.Name != e.Name)
                {
                    if (source.Abstract == "0") ot.AvgrensesAv.Add(source.Name);
                    ot.AvgrensesAv.AddRange(HentRealiserbareObjektarvListe(source));
                }
                else
                {
                    if (destination.Abstract == "0") ot.Avgrenser.Add(destination.Name);
                    ot.Avgrenser.AddRange(HentRealiserbareObjektarvListe(destination));
                }
            }
            else if (connector.Direction == "Destination -> Source")
            {
                if (destination.Name != e.Name)
                {
                    if (destination.Abstract == "0") ot.AvgrensesAv.Add(destination.Name);
                    ot.AvgrensesAv.AddRange(HentRealiserbareObjektarvListe(destination));
                }
                else
                {
                    if (source.Abstract == "0") ot.Avgrenser.Add(source.Name);
                    ot.Avgrenser.AddRange(HentRealiserbareObjektarvListe(source));

                }
            }
            else if (connector.Direction == "Unspecified")
            {
                _repository.WriteOutput("System", "ADVARSEL: Topo assosiasjonen mangler angivelse av 'Direction' mellom " + source.Name + " og " + destination.Name, 0);
            }
        }

        private IEnumerable<string> HentRealiserbareObjektarvListe(Element source)
        {
            List<string> objektnavn = new List<string>();

            foreach (Connector connector in source.Connectors)
            {

                if (connector.MetaType == "Generalization")
                {

                    Element elm = _repository.GetElementByID(connector.ClientID);
                    if (source.Name != elm.Name)
                    {
                        if (elm.Abstract == "0")
                            objektnavn.Add(elm.Name);
                        else
                        {
                            IEnumerable<string> arvnavn = HentRealiserbareObjektarvListe(elm);
                            if (arvnavn.Count() > 0 )
                                objektnavn.AddRange(arvnavn);
                        }
                    }

                }
            }


            return objektnavn;
        }

        private void LagUnionEgenskaper(string prikknivå, Element elm, global::EA.Attribute att2, Objekttype ot, string standard)
        {
            List<string> attnavn = new List<string>();

            foreach (global::EA.Attribute att in elm.Attributes)
            {
                attnavn.Add(att.Name);

                if (att.ClassifierID != 0) 
                {
                    Element elm1 = _repository.GetElementByID(att.ClassifierID);

                    if (elm1.Stereotype.ToLower() == "codelist" || elm1.Stereotype.ToLower() == "enumeration"|| elm1.Type.ToLower() == "enumeration")
                    {
                        Basiselement eg = LagKodelisteEgenskap(prikknivå + ".", elm1, att, ot.OCLconstraints);
                        ot.Egenskaper.Add(eg);
                    }
                    else if (elm1.Stereotype.ToLower() == "union")
                    {
                        LagUnionEgenskaper(prikknivå, elm1, att, ot, standard);
                        
                    }
                    else if (att.Type.ToLower() == "integer" || att.Type.ToLower() == "characterstring" || att.Type.ToLower() == "real" || att.Type.ToLower() == "date" || att.Type.ToLower() == "datetime" || att.Type.ToLower() == "boolean")
                    {
                        Basiselement eg = LagEgenskap(prikknivå + ".", att, standard);
                        ot.Egenskaper.Add(eg);
                    }
                    else if (att.Type.ToLower() == "flate" || att.Type.ToLower() == "punkt" || att.Type.ToLower() == "kurve")
                    {

                        Basiselement eg = LagEgenskap(prikknivå + ".", att, standard);
                        ot.Egenskaper.Add(eg);
                    }
                    else
                    {
                        Gruppeelement tmp = LagGruppeelement(elm1, att, prikknivå,ot);
                        ot.Egenskaper.Add(tmp);
                    }
                }
                else
                {

                    Basiselement eg = LagEgenskap(prikknivå + ".", att, standard);
                    ot.Egenskaper.Add(eg);
                }

            }
            Beskrankning bs = new Beskrankning();
            bs.Navn = "Union " + elm.Name;
            bs.Notat = "et av elementene " + String.Join(",", attnavn.ToArray(), 0, attnavn.Count) + " er påkrevet";
            ot.OCLconstraints.Add(bs);
        }

        private List<AbstraktEgenskap> LagConnectorEgenskaper(string prikknivå, Connector connector, Element e, string standard, Objekttype ot)
        {
            bool is_sosi_navn = false;
            string sosi_navn = "";
            List<AbstraktEgenskap> retur = new List<AbstraktEgenskap>();

            string typeAssosiasjon = "";
            foreach (var tag in connector.TaggedValues)
            {
                switch (((string)((dynamic)tag).Name).ToLower())
                {
                    case "sosi_assosiasjon":
                        typeAssosiasjon = ((string)((dynamic)tag).Value).ToLower();
                        break;
                }
            }
            

            Element source = _repository.GetElementByID(connector.SupplierID);
            Element destination = _repository.GetElementByID(connector.ClientID);
            
            if (typeAssosiasjon.Length == 0)
            {
                _repository.WriteOutput("System", "ADVARSEL: Det er ikke definert type SOSI assosiasjon mellom " + source.Name + " og " + destination.Name + " assosiasjonen. Behandles som REF.", 0);
                typeAssosiasjon = "ref";
            }

            var isSource = false || source.Name == e.Name;

            if (isSource)
            {
                if (typeAssosiasjon == "primærnøkler")
                {
                    Gruppeelement assosiasjon = new Gruppeelement();
                    string sosi_navn_ref="";
                    foreach (var tag in connector.ClientEnd.TaggedValues)
                    {
                        switch (((string)((dynamic)tag).Tag).ToLower())
                        {
                            case "sosi_navn":
                                sosi_navn_ref = (string)((dynamic)tag).Value;
                                break;
                        }
                    }
                    if (sosi_navn_ref.Length == 0)
                    {
                        _repository.WriteOutput("System", "FEIL: Finner ikke tagged value SOSI_navn for " + connector.ClientEnd.Role + " på " + destination.Name, 0);
                        assosiasjon.SOSI_Navn = "";
                    }
                    else assosiasjon.SOSI_Navn = prikknivå + sosi_navn_ref;
                   
                    assosiasjon.Multiplisitet = "[" + connector.ClientEnd.Cardinality + "]";
                    assosiasjon.UML_Navn = connector.ClientEnd.Role + "(rolle)";
                    //Fikse multiplisitet
                    assosiasjon.Multiplisitet = assosiasjon.Multiplisitet.Replace("[0]", "[0..1]");
                    assosiasjon.Multiplisitet = assosiasjon.Multiplisitet.Replace("[0..]", "[0..*]");
                    assosiasjon.Multiplisitet = assosiasjon.Multiplisitet.Replace("[1..]", "[1..*]");
                    assosiasjon.Multiplisitet = assosiasjon.Multiplisitet.Replace("[1]", "[1..1]");
                    List<String> sjekkedeObjekter = new List<string>();
                    SosiEgenskapRetur ref_sosinavn = FinnPrimærnøkkel(destination, prikknivå + ".", standard, ot, sjekkedeObjekter);

                    if (ref_sosinavn != null)
                    {
                        foreach (AbstraktEgenskap item in ref_sosinavn.Egenskaper)
                        {
                            //Fikse multiplisitet
                            item.Multiplisitet = item.Multiplisitet.Replace("[0]", "[0..1]");
                            item.Multiplisitet = item.Multiplisitet.Replace("[0..]", "[0..*]");
                            item.Multiplisitet = item.Multiplisitet.Replace("[1..]", "[1..*]");
                            item.Multiplisitet = item.Multiplisitet.Replace("[1]", "[1..1]");
                            
                        }
                        assosiasjon.Egenskaper = ref_sosinavn.Egenskaper;
                        retur.Add(assosiasjon);
                        
                    }
                    else _repository.WriteOutput("System", "FEIL: Finner ikke primærnøkkel for " + connector.ClientEnd.Role + " på " + destination.Name, 0);
                }
                else if (typeAssosiasjon == "fremmednøkler")
                {

                    SosiEgenskapRetur pn_sosinavn = FinnFremmednøkkel(destination, source, connector, connector.ClientEnd);
                    if (pn_sosinavn != null)
                    {
                        Basiselement eg = new Basiselement();
                        eg.Datatype = pn_sosinavn.SOSI_Datatype + pn_sosinavn.SOSI_Lengde;
                        eg.SOSI_Navn = prikknivå + pn_sosinavn.SOSI_Navn;
                        eg.UML_Navn = connector.ClientEnd.Role + "(rolle)";
                        eg.Multiplisitet = "[" + connector.ClientEnd.Cardinality + "]";
                        eg.Standard = HentApplicationSchemaPakkeNavn(destination);
                        //Fikse multiplisitet
                        eg.Multiplisitet = eg.Multiplisitet.Replace("[0]", "[0..1]");
                        eg.Multiplisitet = eg.Multiplisitet.Replace("[0..]", "[0..*]");
                        eg.Multiplisitet = eg.Multiplisitet.Replace("[1..]", "[1..*]");
                        eg.Multiplisitet = eg.Multiplisitet.Replace("[1]", "[1..1]");

                        eg.TillatteVerdier = new List<string>();
                        retur.Add(eg);
                    }
                }
                else
                {
                    if (!String.IsNullOrEmpty(connector.ClientEnd.Role) && (connector.ClientEnd.Navigable == "Navigable" || connector.Direction == "Unspecified")) //pluss navigerbart
                    {
                        addClientEnd(prikknivå, connector, retur, source, destination);
                    }
                    if (connector.SupplierID == connector.ClientID && !String.IsNullOrEmpty(connector.SupplierEnd.Role) && (connector.SupplierEnd.Navigable == "Navigable" || connector.Direction == "Unspecified")) //pluss navigerbart
                    {
                        addSupplierEnd(prikknivå, connector, retur, source, destination);
                    }
                }
                

            }
            else
            {
                
                if (typeAssosiasjon == "primærnøkler")
                {
                    Gruppeelement assosiasjon = new Gruppeelement();
                    
                    string sosi_navn_ref="";
                    foreach (var tag in connector.SupplierEnd.TaggedValues)
                    {
                        switch (((string)((dynamic)tag).Tag).ToLower())
                        {
                            case "sosi_navn":
                                sosi_navn_ref = (string)((dynamic)tag).Value;
                                break;
                        }
                    }
                    if (sosi_navn_ref.Length == 0)
                    {
                        _repository.WriteOutput("System", "FEIL: Finner ikke tagged value SOSI_navn for " + connector.SupplierEnd.Role + " på " + source.Name, 0);
                        assosiasjon.SOSI_Navn = "";
                    }
                    else assosiasjon.SOSI_Navn = prikknivå + sosi_navn_ref;
                    
                    assosiasjon.SOSI_Navn = prikknivå + sosi_navn_ref;
                    assosiasjon.Multiplisitet = "[" + connector.SupplierEnd.Cardinality + "]";
                    assosiasjon.UML_Navn = connector.SupplierEnd.Role + "(rolle)";
                    //Fikse multiplisitet
                    assosiasjon.Multiplisitet = assosiasjon.Multiplisitet.Replace("[0]", "[0..1]");
                    assosiasjon.Multiplisitet = assosiasjon.Multiplisitet.Replace("[0..]", "[0..*]");
                    assosiasjon.Multiplisitet = assosiasjon.Multiplisitet.Replace("[1..]", "[1..*]");
                    assosiasjon.Multiplisitet = assosiasjon.Multiplisitet.Replace("[1]", "[1..1]");

                    List<String> sjekkedeObjekter = new List<string>();
                    SosiEgenskapRetur ref_sosinavn = FinnPrimærnøkkel(source, prikknivå+".", standard, ot, sjekkedeObjekter);
                    if (ref_sosinavn != null)
                    {
                        foreach (AbstraktEgenskap item in ref_sosinavn.Egenskaper)
                        {
                            item.Multiplisitet = "[" + connector.SupplierEnd.Cardinality + "]";
                            //Fikse multiplisitet
                            item.Multiplisitet = item.Multiplisitet.Replace("[0]", "[0..1]");
                            item.Multiplisitet = item.Multiplisitet.Replace("[0..]", "[0..*]");
                            item.Multiplisitet = item.Multiplisitet.Replace("[1..]", "[1..*]");
                            item.Multiplisitet = item.Multiplisitet.Replace("[1]", "[1..1]");


                            
                        }
                        
                        assosiasjon.Egenskaper=ref_sosinavn.Egenskaper;
                        retur.Add(assosiasjon);
                    }
                    else _repository.WriteOutput("System", "FEIL: Finner ikke primærnøkkel for " + connector.SupplierEnd.Role + " på " + source.Name, 0);
                }
                else if (typeAssosiasjon == "fremmednøkler")
                {
                    SosiEgenskapRetur pn_sosinavn = FinnFremmednøkkel(source, destination, connector, connector.SupplierEnd);
                    if (pn_sosinavn != null)
                    {
                        Basiselement eg = new Basiselement();
                        eg.UML_Navn = connector.SupplierEnd.Role + "(rolle)";
                        eg.Multiplisitet = "[" + connector.SupplierEnd.Cardinality + "]";
                        eg.Datatype = pn_sosinavn.SOSI_Datatype + pn_sosinavn.SOSI_Lengde;
                        eg.SOSI_Navn = prikknivå + pn_sosinavn.SOSI_Navn;
                        eg.Standard = HentApplicationSchemaPakkeNavn(source);
                        //Fikse multiplisitet
                        eg.Multiplisitet = eg.Multiplisitet.Replace("[0]", "[0..1]");
                        eg.Multiplisitet = eg.Multiplisitet.Replace("[0..]", "[0..*]");
                        eg.Multiplisitet = eg.Multiplisitet.Replace("[1..]", "[1..*]");
                        eg.Multiplisitet = eg.Multiplisitet.Replace("[1]", "[1..1]");

                        eg.TillatteVerdier = new List<string>();
                        retur.Add(eg);
                    } 
                }
                else
                {
                    if (!String.IsNullOrEmpty(connector.SupplierEnd.Role) && (connector.SupplierEnd.Navigable == "Navigable" || connector.Direction == "Unspecified")) //pluss navigerbart
                    {
                        addSupplierEnd(prikknivå, connector, retur, source, destination);
                    }
                    if (connector.SupplierID == connector.ClientID && !String.IsNullOrEmpty(connector.ClientEnd.Role) && (connector.ClientEnd.Navigable == "Navigable" || connector.Direction == "Unspecified")) //pluss navigerbart
                    {
                        addClientEnd(prikknivå, connector, retur, source, destination);
                    }
                }
            }
            return retur;
        }

        private void addClientEnd(string prikknivå, Connector connector, List<AbstraktEgenskap> retur, Element source, Element destination)
        {
            bool is_sosi_navn=false;
            string sosi_navn="FIX";
            Basiselement eg = new Basiselement();
            eg.Datatype = "REF";
            eg.UML_Navn = connector.ClientEnd.Role + "(rolle)";
            eg.Multiplisitet = "[" + connector.ClientEnd.Cardinality + "]";
            eg.Standard = HentApplicationSchemaPakkeNavn(destination);

            foreach (var tag in connector.ClientEnd.TaggedValues)
            {
                switch (((string)((dynamic)tag).Tag).ToLower())
                {
                    case "sosi_navn":
                        sosi_navn = (string)((dynamic)tag).Value;
                        is_sosi_navn = true;
                        break;
                }
            }
            eg.SOSI_Navn = prikknivå + sosi_navn;
            //Fikse multiplisitet
            eg.Multiplisitet = eg.Multiplisitet.Replace("[0]", "[0..1]");
            eg.Multiplisitet = eg.Multiplisitet.Replace("[0..]", "[0..*]");
            eg.Multiplisitet = eg.Multiplisitet.Replace("[1..]", "[1..*]");
            eg.Multiplisitet = eg.Multiplisitet.Replace("[1]", "[1..1]");

            eg.TillatteVerdier = new List<string>();
            retur.Add(eg);
            if (is_sosi_navn == false) _repository.WriteOutput("System", "FEIL: Mangler angivelse av tagged value sosi_navn på assosiasjonsende " + connector.ClientEnd.Role + " mellom " + source.Name + " og " + destination.Name, 0);
        }

        private void addSupplierEnd(string prikknivå, Connector connector, List<AbstraktEgenskap> retur, Element source, Element destination)
        {
            bool is_sosi_navn = false;
            string sosi_navn = "FIX";
            Basiselement eg = new Basiselement();
            eg.UML_Navn = connector.SupplierEnd.Role + "(rolle)";
            eg.Multiplisitet = "[" + connector.SupplierEnd.Cardinality + "]";
            eg.Datatype = "REF";
            foreach (var tag in connector.SupplierEnd.TaggedValues)
            {

                switch (((string)((dynamic)tag).Tag).ToLower())
                {

                    case "sosi_navn":
                        sosi_navn = (string)((dynamic)tag).Value;
                        is_sosi_navn = true;
                        break;
                }
            }
            eg.SOSI_Navn = prikknivå + sosi_navn;
            eg.Standard = HentApplicationSchemaPakkeNavn(source);
            //Fikse multiplisitet
            eg.Multiplisitet = eg.Multiplisitet.Replace("[0]", "[0..1]");
            eg.Multiplisitet = eg.Multiplisitet.Replace("[0..]", "[0..*]");
            eg.Multiplisitet = eg.Multiplisitet.Replace("[1..]", "[1..*]");
            eg.Multiplisitet = eg.Multiplisitet.Replace("[1]", "[1..1]");

            eg.TillatteVerdier = new List<string>();

            retur.Add(eg);
            if (is_sosi_navn == false) _repository.WriteOutput("System", "FEIL: Mangler angivelse av tagged value sosi_navn på assosiasjonsende " + connector.SupplierEnd.Role + " mellom " + source.Name + " og " + destination.Name, 0);
        }

     

        private SosiEgenskapRetur FinnFremmednøkkel(Element destination, Element source, Connector connector, ConnectorEnd end)
        {
            SosiEgenskapRetur retur = null;
            string attributtnavn = "";
            foreach (object tag in end.TaggedValues)
            {

                switch (((string)((dynamic)tag).Tag).ToLower())
                {
                    case "sosi_fremmednøkkel":
                        attributtnavn = (string)((dynamic)tag).Value;
                        break;
                }
            }

            if (attributtnavn.Length > 0)
            {
                string sosi_navn = "";
                string sosi_lengde = "";
                string sosi_datatype = "";
                foreach (global::EA.Attribute att in source.Attributes)
                {

                    if (attributtnavn.ToLower() == att.Name.ToLower())
                    {
                        foreach (object tag in att.TaggedValues)
                        {

                            switch (((string)((dynamic)tag).Name).ToLower())
                            {
                                case "sosi_navn":
                                    sosi_navn = (string)((dynamic)tag).Value;
                                    break;
                                case "sosi_lengde":
                                    sosi_lengde = (string)((dynamic)tag).Value;
                                    break;
                                case "sosi_datatype":
                                    sosi_datatype = (string)((dynamic)tag).Value;
                                    break;
                            }
                        }
                        retur = new SosiEgenskapRetur();
                        retur.SOSI_Navn = sosi_navn;
                        retur.SOSI_Lengde = sosi_lengde;
                        retur.SOSI_Datatype = sosi_datatype;
                        break;

                    }
                }
            }
            else 
            { 

                foreach (global::EA.Attribute att in source.Attributes)
                {
                    string sosi_navn = "";
                    string sosi_lengde = "";
                    string sosi_datatype = "";
                    string sosi_fremmednøkkel = "";
                    
                    foreach (object tag in att.TaggedValues)
                    {

                        switch (((string)((dynamic)tag).Name).ToLower())
                        {
                            case "sosi_fremmednøkkel":
                                sosi_fremmednøkkel = (string)((dynamic)tag).Value;
                                break;
                            case "sosi_navn":
                                sosi_navn = (string)((dynamic)tag).Value;
                                break;
                            case "sosi_lengde":
                                sosi_lengde = (string)((dynamic)tag).Value;
                                break;
                            case "sosi_datatype":
                                sosi_datatype = (string)((dynamic)tag).Value;
                                break;
                        }
                    }
                    if (sosi_fremmednøkkel.ToLower() == destination.Name.ToLower())
                    {
                        //Finnes alt så den skal ikke lages..
                        break;
                    }
                    else { 
                        _repository.WriteOutput("System", "FEIL: Ufullstendig assosiasjon: " + end.Role, 0);
                    }
                }
            }
            
            return retur;
        }

        private SosiEgenskapRetur FinnPrimærnøkkel(Element source, string prikknivå, string standard, Objekttype ot, List<String> arvedeobjekterSjekket)
        {
           
            SosiEgenskapRetur retur = null;
            retur = new SosiEgenskapRetur();
            retur.Egenskaper = new List<AbstraktEgenskap>();

            foreach (global::EA.Attribute att in source.Attributes)
            {
                bool erPrimærnøkkel = false;

                foreach (object tag in att.TaggedValues)
                {

                    switch (((string)((dynamic)tag).Name).ToLower())
                    {
                        case "sosi_primærnøkkel":
                            if (((string)((dynamic)tag).Value).ToLower() == "true") erPrimærnøkkel = true;
                            break;
                    }
                }
                if (erPrimærnøkkel)
                {
                    
                        if (att.Type.ToLower() == "integer" || att.Type.ToLower() == "characterstring" || att.Type.ToLower() == "real" || att.Type.ToLower() == "date" || att.Type.ToLower() == "datetime" || att.Type.ToLower() == "boolean")
                        {
                            Basiselement eg = LagEgenskap(prikknivå, att, standard);
                            retur.Egenskaper.Add(eg);
                        }
                        else //Kompleks type(datatype) som primærnøkkel
                        {

                            if (att.ClassifierID != 0) 
                            {
                                Element elm1 = _repository.GetElementByID(att.ClassifierID);
                                if (elm1.Stereotype.ToLower() == "codelist" || elm1.Stereotype.ToLower() == "enumeration" || elm1.Type.ToLower() == "enumeration")
                                {
                                    Basiselement tmp = LagKodelisteEgenskap(prikknivå, elm1, att, ot.OCLconstraints);
                                    retur.Egenskaper.Add(tmp);
                                }
                                else
                                {
                                    Gruppeelement tmp = LagGruppeelement(elm1, att, prikknivå, ot);
                                    retur.Egenskaper.Add(tmp);
                                }
                            }
                            else _repository.WriteOutput("System", "FEIL: Primærnøkkel er feil definert på : " + source.Name, 0);
                           
                        }
                   
                    
                }
               
                foreach (Connector connector in source.Connectors)
                {
                    if (connector.MetaType == "Generalization")
                    {
                        Element elmg = _repository.GetElementByID(connector.SupplierID);
                        if (source.Name != elmg.Name)
                        {
                            if (arvedeobjekterSjekket.Contains(elmg.Name) == false)
                            {
                                var ret = FinnPrimærnøkkel(elmg, prikknivå, standard, ot, arvedeobjekterSjekket);
                                arvedeobjekterSjekket.Add(elmg.Name);
                                retur.Egenskaper.AddRange(ret.Egenskaper);
                            }
                            
                        }

                    }
                }
               

            }
            return retur;
        }

       

        private Gruppeelement LagGruppeelement(Element elm, global::EA.Attribute att2, string prikknivå, Objekttype parentObjekttype)
        {
            Gruppeelement ot = new Gruppeelement();
            ot.Egenskaper = new List<AbstraktEgenskap>();
            ot.OCLconstraints = new List<Beskrankning>();
            bool sosi_navn = false;
            
            ot.Notat = elm.Notes;
            ot.SOSI_Navn = prikknivå;
            string standard = HentApplicationSchemaPakkeNavn(elm);
            ot.Standard = standard;
            if (att2 == null) { 
                ot.Multiplisitet = "[1..1]";
                ot.UML_Navn = elm.Name;
            }
            else
            {
                ot.UML_Navn = att2.Name;
                ot.Multiplisitet = "[" + att2.LowerBound + ".." + att2.UpperBound + "]";

                foreach (object tag in att2.TaggedValues)
                {
                    switch (((string)((dynamic)tag).Name).ToLower())
                    {
                        case "sosi_navn":
                            ot.SOSI_Navn = prikknivå + ((dynamic)tag).Value;
                            if (ot.SOSI_Navn.Length > 0) sosi_navn = true;
                            break;
                    }
                }
            }
            if (sosi_navn == false)
            {   
                foreach (TaggedValue theTags in elm.TaggedValues)
                {
                    switch (theTags.Name.ToLower())
                    {
                        case "sosi_navn":
                            ot.SOSI_Navn = prikknivå + theTags.Value;
                            if (ot.SOSI_Navn.Length > 0) sosi_navn = true;
                            break;
                    }
                }
            }
            if (sosi_navn == false) _repository.WriteOutput("System", "FEIL: Mangler tagged value sosi_navn på gruppeelement: " + elm.Name, 0);

            foreach (global::EA.Constraint constraint in elm.Constraints)
            {
                Beskrankning beskrankning = new Beskrankning();
                beskrankning.Navn = constraint.Name;
                string ocldesc = "";
                if (constraint.Notes.Contains("/*") && constraint.Notes.Contains("*/"))
                {
                    ocldesc = constraint.Notes.Substring(constraint.Notes.ToLower().IndexOf("/*") + 2, constraint.Notes.ToLower().IndexOf("*/") - 2 - constraint.Notes.ToLower().IndexOf("/*"));
                }
                beskrankning.Notat = ocldesc;
                beskrankning.OCL = constraint.Notes;

                parentObjekttype.LeggTilBeskrankning(beskrankning);
            }

            foreach (global::EA.Attribute att in elm.Attributes)
            {
                string nestePrikknivå = prikknivå + ".";
                if (att.ClassifierID != 0)
                {
                    Element elm1 = _repository.GetElementByID(att.ClassifierID);

                    if (ErKjentType(att.Type))
                    {
                        ot.Egenskaper.Add(LagEgenskapForKjentType(nestePrikknivå, att, standard));
                    }
                    //else if (ErTomDatatype(elm1))
                    //{
                    //    var basiselementBuilder =
                    //            new BasiselementBuilder(_repository, nestePrikknivå, standard).ForAttributt(att);
                    //    Basiselement basiselement = basiselementBuilder.MedMappingAvTomDatatype().Opprett();
                    //    ot.Egenskaper.Add(basiselement);
                    //}
                    else if (elm1.Stereotype.ToLower() == "codelist" || elm1.Stereotype.ToLower() == "enumeration" || elm1.Type.ToLower() == "enumeration")
                    {
                        Basiselement eg = LagKodelisteEgenskap(nestePrikknivå, elm1, att, parentObjekttype.OCLconstraints);
                        ot.Egenskaper.Add(eg);
                    }
                    else if (elm1.Stereotype.ToLower() == "union")
                    {
                        LagUnionEgenskaperForGruppeelement(nestePrikknivå, elm1, att, parentObjekttype, standard, ot);
                    }
                    else if (att.Type.ToLower() == "integer" || att.Type.ToLower() == "characterstring" || att.Type.ToLower() == "real" ||
                             att.Type.ToLower() == "date" || att.Type.ToLower() == "datetime" || att.Type.ToLower() == "boolean")
                    {
                        Basiselement eg = LagEgenskap(nestePrikknivå, att, standard);
                        ot.Egenskaper.Add(eg);
                    }
                    else if (att.Type.ToLower() == "flate" || att.Type.ToLower() == "punkt" || att.Type.ToLower() == "kurve")
                    {

                        Basiselement eg = LagEgenskap(nestePrikknivå, att, standard);
                        ot.Egenskaper.Add(eg);
                    }
                    else if (ErTomDatatype(elm1))
                    {
                        var basiselementBuilder =
                                new BasiselementBuilder(_repository, nestePrikknivå, standard).ForAttributt(att);
                        Basiselement basiselement = basiselementBuilder.MedMappingAvTomDatatype().Opprett();
                        ot.Egenskaper.Add(basiselement);
                    }
                    else
                    {
                        Gruppeelement tmp = LagGruppeelement(elm1, att, nestePrikknivå, parentObjekttype);
                        ot.Egenskaper.Add(tmp);
                    }
                }
                else
                {

                    Basiselement eg = LagEgenskap(nestePrikknivå, att, standard);
                    ot.Egenskaper.Add(eg);
                }
            }
            foreach (Connector connector in elm.Connectors)
            {
                if (connector.MetaType == "Association" || connector.MetaType == "Aggregation")
                {
                    Element source = _repository.GetElementByID(connector.SupplierID);
                    Element destination = _repository.GetElementByID(connector.ClientID);
                    bool is_source = false;

                    if (source.Name == elm.Name) is_source = true;
                    else is_source = false;

                    if (connector.SupplierEnd.Aggregation == 2 && connector.Direction == "Destination -> Source" && is_source == true) //Composite
                    {
                        Gruppeelement tmp = LagGruppeelementKomposisjon(connector, prikknivå, parentObjekttype);
                        ot.Egenskaper.Add(tmp);
                    }
                    else if (connector.ClientEnd.Aggregation == 2 && connector.Direction == "Source -> Destination" && is_source == false) //Composite
                    {
                        Gruppeelement tmp = LagGruppeelementKomposisjon(connector, prikknivå, parentObjekttype);
                        ot.Egenskaper.Add(tmp);
                    }
                    else
                    {

                        if (connector.Direction == "Bi-Directional")
                        {
                            List<AbstraktEgenskap> eg = LagConnectorEgenskaper(prikknivå, connector, elm, standard, parentObjekttype);
                            ot.Egenskaper.AddRange(eg);
                        }
                        else if (connector.Direction == "Source -> Destination" && is_source == false)
                        {
                            List<AbstraktEgenskap> eg = LagConnectorEgenskaper(prikknivå, connector, elm, standard, parentObjekttype);
                            ot.Egenskaper.AddRange(eg);
                        }
                        else if (connector.Direction == "Destination -> Source" && is_source == true)
                        {
                            List<AbstraktEgenskap> eg = LagConnectorEgenskaper(prikknivå, connector, elm, standard, parentObjekttype);
                            ot.Egenskaper.AddRange(eg);
                        }
                        else if (connector.Direction == "Unspecified")
                        {
                            _repository.WriteOutput("System", "ADVARSEL: Assosiasjonen mangler angivelse av 'Direction' mellom " + source.Name + " og " + destination.Name, 0);
                        }
                    }
                }
                else if (connector.MetaType == "Generalization")
                {

                    Element elmg = _repository.GetElementByID(connector.SupplierID);

                    if (elm.Name != elmg.Name)
                    {
                        Gruppeelement tmp2 = LagGruppeelement(elmg, null, prikknivå , parentObjekttype);
                        ot.Inkluder = tmp2;
                    }

                }
            }
            return ot;
        }
        

        private Gruppeelement LagGruppeelementKomposisjon(global::EA.Connector conn, string prikknivå, Objekttype pot)
        {
            Element elm = _repository.GetElementByID(conn.ClientID);
            
            Gruppeelement ot = new Gruppeelement();
            ot.Egenskaper = new List<AbstraktEgenskap>();
            ot.OCLconstraints = new List<Beskrankning>();
            bool sosi_navn = false;
            ot.UML_Navn = conn.ClientEnd.Role + "(rolle)";
            ot.Notat = elm.Notes;
            ot.SOSI_Navn = prikknivå;
            string standard = HentApplicationSchemaPakkeNavn(elm);
            ot.Standard = standard;
            ot.Multiplisitet = "["+ conn.ClientEnd.Cardinality + "]";
            
            foreach (object tag in conn.ClientEnd.TaggedValues)
            {
                switch (((string)((dynamic)tag).Tag).ToLower())
                {
                    case "sosi_navn":
                        ot.SOSI_Navn = prikknivå + ((dynamic)tag).Value;
                        if (ot.SOSI_Navn.Length > 0) sosi_navn = true;
                        break;
                }
            }

            if (sosi_navn == false) _repository.WriteOutput("System", "FEIL: Mangler tagged value sosi_navn på komposisjon: " + conn.ClientEnd.Role, 0);

            foreach (global::EA.Constraint constr in elm.Constraints)
            {
                Beskrankning bskr = new Beskrankning();
                bskr.Navn = constr.Name;
                string ocldesc = "";
                if (constr.Notes.Contains("/*") && constr.Notes.Contains("*/"))
                {
                    ocldesc = constr.Notes.Substring(constr.Notes.ToLower().IndexOf("/*") + 2, constr.Notes.ToLower().IndexOf("*/") - 2 - constr.Notes.ToLower().IndexOf("/*"));
                }
                bskr.Notat = ocldesc;
                bskr.OCL = constr.Notes;

                pot.OCLconstraints.Add(bskr);
            }

            foreach (global::EA.Attribute att in elm.Attributes)
            {

                if (att.ClassifierID != 0)
                {
                   
                    Element elm1 = _repository.GetElementByID(att.ClassifierID);

                    if (ErKjentType(att.Type))
                    {
                        ot.Egenskaper.Add(LagEgenskapForKjentType(prikknivå, att, standard));
                    }
                    else if (elm1.Stereotype.ToLower() == "codelist" || elm1.Stereotype.ToLower() == "enumeration" || elm1.Type.ToLower() == "enumeration")
                    {
                        Basiselement eg = LagKodelisteEgenskap(prikknivå + ".", elm1, att, pot.OCLconstraints);
                        ot.Egenskaper.Add(eg);
                    }
                    else if (elm1.Stereotype.ToLower() == "union")
                    {
                        LagUnionEgenskaperForGruppeelement(prikknivå, elm1, att, pot, standard, ot);
                    }
                    else if (att.Type.ToLower() == "integer" || att.Type.ToLower() == "characterstring" || att.Type.ToLower() == "real" || att.Type.ToLower() == "date" || att.Type.ToLower() == "datetime" || att.Type.ToLower() == "boolean")
                    {
                        Basiselement eg = LagEgenskap(prikknivå + ".", att, standard);
                        ot.Egenskaper.Add(eg);
                    }
                    else if (att.Type.ToLower() == "flate" || att.Type.ToLower() == "punkt" || att.Type.ToLower() == "kurve")
                    {
                        Basiselement eg = LagEgenskap(prikknivå + ".", att, standard);
                        ot.Egenskaper.Add(eg);
                    }
                    else
                    {
                        Gruppeelement tmp = LagGruppeelement(elm1, att, prikknivå + ".", pot);
                        ot.Egenskaper.Add(tmp);
                    }
                }
                else
                {
                    Basiselement eg = LagEgenskap(prikknivå + ".", att, standard);
                    ot.Egenskaper.Add(eg);
                }

            }

            foreach (Connector connector in elm.Connectors)
            {
                if (connector.MetaType == "Association" || connector.MetaType == "Aggregation")
                {
                    Element source = _repository.GetElementByID(connector.SupplierID);
                    Element destination = _repository.GetElementByID(connector.ClientID);
                    bool is_source = false;

                    if (source.Name == elm.Name) is_source = true;
                    else is_source = false;

                    if (connector.Direction == "Bi-Directional")
                    {
                        List<AbstraktEgenskap> eg = LagConnectorEgenskaper(prikknivå, connector, elm, standard, pot);
                        ot.Egenskaper.AddRange(eg);
                    }
                    else if (connector.Direction == "Source -> Destination" && is_source == false)
                    {
                        List<AbstraktEgenskap> eg = LagConnectorEgenskaper(prikknivå, connector, elm, standard, pot);
                        ot.Egenskaper.AddRange(eg);
                    }
                    else if (connector.Direction == "Destination -> Source" && is_source == true)
                    {
                        List<AbstraktEgenskap> eg = LagConnectorEgenskaper(prikknivå, connector, elm, standard, pot);
                        ot.Egenskaper.AddRange(eg);
                    }
                    else if (connector.Direction == "Unspecified")
                    {
                        _repository.WriteOutput("System", "ADVARSEL: Assosiasjonen mangler angivelse av 'Direction' mellom " + source.Name + " og " + destination.Name, 0);
                    }
                }
                if (connector.MetaType == "Generalization")
                {

                    Element elmg = _repository.GetElementByID(connector.SupplierID);
                    if (elm.Name != elmg.Name)
                    {
                        Gruppeelement tmp2 = LagGruppeelement(elmg, null, prikknivå, pot);
                        ot.Inkluder = tmp2;
                    }

                }
            }
            return ot;
        }

        private void LagUnionEgenskaperForGruppeelement(string prikknivå, Element elm, global::EA.Attribute att1, Objekttype pot, string standard, Gruppeelement ot)
        {
            List<string> attnavn = new List<string>();
            

            foreach (global::EA.Constraint constr in elm.Constraints)
            {
                Beskrankning bskr = new Beskrankning();
                bskr.Navn = constr.Name;
                string ocldesc = "";
                if (constr.Notes.Contains("/*") && constr.Notes.Contains("*/"))
                {
                    ocldesc = constr.Notes.Substring(constr.Notes.ToLower().IndexOf("/*") + 2, constr.Notes.ToLower().IndexOf("*/") - 2 - constr.Notes.ToLower().IndexOf("/*"));
                }
                bskr.Notat = ocldesc;
                bskr.OCL = constr.Notes;

                pot.OCLconstraints.Add(bskr);
            }

            foreach (global::EA.Attribute att in elm.Attributes)
            {
                attnavn.Add(att.Name);

                if (att.ClassifierID != 0)
                {
                   Element elm1 = _repository.GetElementByID(att.ClassifierID);
                   if (elm1.Stereotype.ToLower() == "codelist" || elm1.Stereotype.ToLower() == "enumeration" || elm1.Type.ToLower() == "enumeration")
                    {
                        Basiselement eg = LagKodelisteEgenskap(prikknivå + ".", elm1, att, pot.OCLconstraints);
                        ot.Egenskaper.Add(eg);
                    }
                    else if (elm1.Stereotype.ToLower() == "union")
                    {
                        LagUnionEgenskaperForGruppeelement(prikknivå, elm, att, pot, standard, ot);

                    }
                    else if (att.Type.ToLower() == "integer" || att.Type.ToLower() == "characterstring" || att.Type.ToLower() == "real" || att.Type.ToLower() == "date" || att.Type.ToLower() == "datetime" || att.Type.ToLower() == "boolean")
                    {
                        Basiselement eg = LagEgenskap(prikknivå + ".", att, standard);
                        ot.Egenskaper.Add(eg);
                    }
                   
                    else if (att.Type.ToLower() == "flate" || att.Type.ToLower() == "punkt" || att.Type.ToLower() == "kurve")
                    {

                        Basiselement eg = LagEgenskap(prikknivå + ".", att, standard);
                        ot.Egenskaper.Add(eg);
                    }
                    else
                    {
                        Gruppeelement tmp = LagGruppeelement(elm1, att, prikknivå + ".", pot);
                        ot.Egenskaper.Add(tmp);
                    }
                }
                else
                {

                    Basiselement eg = LagEgenskap(prikknivå + ".", att, standard);
                    ot.Egenskaper.Add(eg);
                }

            }
           
            Beskrankning bs = new Beskrankning();
            bs.Navn = "Union " + elm.Name;
            bs.Notat = "et av elementene " + String.Join(",", attnavn.ToArray(), 0, attnavn.Count) + " er påkrevet";
            pot.OCLconstraints.Add(bs);

            

        }

        private string HentApplicationSchemaPakkeNavn(Element elm)
        {
            string pnavn = "FIX";
           
            if (elm.PackageID != 0)
            {
                Package pk = _repository.GetPackageByID(elm.PackageID);
                if (pk.Element != null)
                {
                    if (pk.Element.Stereotype.ToLower() == "applicationschema" || pk.Element.Stereotype.ToLower() == "underarbeid")
                    {
                        string status = "";
                        if (pk.Element.Stereotype.ToLower() == "underarbeid") status = " (under arbeid)";
                        
                        pnavn = pk.Element.Name + status;
                        string kortnavn = "";
                        string versjon = "";
                       
                        foreach (TaggedValue tag in pk.Element.TaggedValues)
                        {
                            switch (tag.Name.ToLower())
                            {
                                case "sosi_kortnavn":
                                    kortnavn = tag.Value;
                                    break;
                                case "sosi_versjon":
                                    versjon = tag.Value;
                                    break;
                            }

                        }
                        if (kortnavn.Length > 0) {
                            pnavn = kortnavn + " " + versjon + status;
                        }
                    }
                    else pnavn = HentApplicationSchemaPakkeNavn(pk.Element);
                }
            }
            return pnavn;
        }
        private Basiselement LagKodelisteEgenskap(string prikknivå, Element elm, global::EA.Attribute att, List<Beskrankning> oclliste)
        {
            
            try
            {

                Basiselement eg = new Basiselement();
                eg.UML_Navn = att.Name;
                eg.SOSI_Navn = prikknivå;
                eg.Notat = att.Notes;
                eg.Standard = HentApplicationSchemaPakkeNavn(elm);
                
                eg.Multiplisitet = "[" + att.LowerBound + ".." + att.UpperBound + "]";
                bool sosi_navn = false;
                bool sosi_lengde = false;
                bool sosi_datatype = false;
                eg.Datatype = "T";
                eg.TillatteVerdier = new List<string>();
                eg.Operator = "=";
                
                bool beskrankVerdier = false;

                foreach (Beskrankning be in oclliste)
                {
                    if (be.OCL != null)
                    {

                        if (be.OCL.Contains(att.Name) && be.OCL.ToLower().Contains("inv:") && be.OCL.ToLower().Contains("notempty") == false && be.OCL.ToLower().Contains("implies") == false && be.OCL.ToLower().Contains("and") == false)
                        {

                            if (be.OCL.Contains("=")) eg.Operator = "=";
                            else if (be.OCL.Contains("&lt;&gt;")) eg.Operator = "!=";
                            else
                            {
                                _repository.WriteOutput("System", "ADVARSEL: Fant beskrankning for " + att.Name + " i " + elm.Name + " men klarte ikke å løse operator uttrykket.", 0);
                            }
                            //finne verdier neste token etter operator, TODO må forbedres mye!!!
                            string ocl = be.OCL.Substring(be.OCL.ToLower().IndexOf("inv:") + 4);
                            string[] separators = { "'", "=", "&lt;&gt;", "or" };
                            string[] tokens = ocl.Trim().Split(separators, StringSplitOptions.RemoveEmptyEntries);

                            foreach (string verdi in tokens)
                            {
                                if ((verdi.Contains(att.Name)) == false && verdi.Trim().Length > 0)
                                {
                                    eg.TillatteVerdier.Add(verdi);
                                    beskrankVerdier = true;
                                }
                            }


                        }
                    }

                }

                if (beskrankVerdier == false)
                {

                    foreach (global::EA.Attribute a in elm.Attributes)
                    {
                        if (a.Default.Trim().Length > 0) eg.TillatteVerdier.Add(a.Default.Trim());
                        else
                        {
                            bool sosi_verdi = false;
                            string verdi = "";
                            foreach (object tag in a.TaggedValues)
                            {
                                switch (((string)((dynamic)tag).Name).ToLower())
                                {

                                    case "sosi_verdi":
                                        verdi = ((dynamic)tag).Value;
                                        sosi_verdi = true;
                                        break;
                                }

                            }
                            if (sosi_verdi && verdi.Trim().Length>0) eg.TillatteVerdier.Add(verdi.Trim());
                            else
                            {
                                eg.TillatteVerdier.Add(a.Name);

                            }
                        }

                    }
                }

                string datatype = eg.Datatype;
                foreach (object tag in att.TaggedValues)
                {
                    switch (((string)((dynamic)tag).Name).ToLower())
                    {
                       
                        case "sosi_navn":
                            eg.SOSI_Navn = prikknivå + ((dynamic)tag).Value;
                            if (((dynamic)tag).Value.Length > 0) sosi_navn = true;
                            break;
                    }
                }
                if (sosi_navn == false)
                {
                    foreach (object theTags2 in elm.TaggedValues)
                    {
                        switch (((string)((dynamic)theTags2).Name).ToLower())
                        {
                            case "length":
                                eg.Datatype = datatype + ((dynamic)theTags2).Value;
                                sosi_lengde = true;
                                break;
                            case "sosi_lengde":
                                eg.Datatype = datatype + ((dynamic)theTags2).Value;
                                sosi_lengde = true;
                                break;
                            case "sosi_datatype":
                                datatype = datatype.Replace("T", ((dynamic)theTags2).Value);
                                eg.Datatype = datatype;
                                if (((dynamic)theTags2).Value.Length > 0) sosi_datatype = true;
                                break;
                            case "sosi_navn":
                                eg.SOSI_Navn = prikknivå + ((dynamic)theTags2).Value;
                                if (((dynamic)theTags2).Value.Length > 0) sosi_navn = true;
                                break;
                        }

                    }
                }
                if (sosi_lengde == false)
                {
                    foreach (object theTags2 in elm.TaggedValues)
                    {
                        switch (((string)((dynamic)theTags2).Name).ToLower())
                        {
                            case "length":
                                eg.Datatype = datatype + ((dynamic)theTags2).Value;
                                sosi_lengde = true;
                                break;
                            case "sosi_lengde":
                                eg.Datatype = datatype + ((dynamic)theTags2).Value;
                                sosi_lengde = true;
                                break;
                            case "sosi_datatype":
                                datatype = datatype.Replace("T", ((dynamic)theTags2).Value);
                                eg.Datatype = datatype;
                                if (((dynamic)theTags2).Value.Length > 0) sosi_datatype = true;
                                break;
                           
                        }

                    }
                }
                if (sosi_datatype == false)
                {
                    foreach (object theTags2 in elm.TaggedValues)
                    {
                        switch (((string)((dynamic)theTags2).Name).ToLower())
                        {
                            case "sosi_datatype":
                                datatype = datatype.Replace("T", ((dynamic)theTags2).Value);
                                eg.Datatype = datatype;
                                if (((dynamic)theTags2).Value.Length > 0) sosi_datatype = true;
                                break;

                        }

                    }
                }
                if (sosi_navn == false) _repository.WriteOutput("System", "FEIL: Mangler tagged value sosi_navn på kodeliste " + elm.Name + ", attributt: " + att.Name, 0);
                if (sosi_lengde == false) _repository.WriteOutput("System", "FEIL: Mangler tagged value sosi_lengde på kodeliste " + elm.Name + ", attributt: " + att.Name, 0);
                
                foreach (Connector connector in elm.Connectors)
                {
                    if (connector.MetaType == "Generalization")
                    {

                        Element elmg = _repository.GetElementByID(connector.SupplierID);
                        
                        if (elm.Name != elmg.Name)
                        {
                            foreach (global::EA.Attribute a in elmg.Attributes)
                            {
                               
                                if (a.Default.Trim().Length > 0) eg.TillatteVerdier.Add(a.Default.Trim());
                                else
                                {
                                   
                                    bool sosi_verdi = false;
                                    string verdi = "";
                                    foreach (object tag in a.TaggedValues)
                                    {
                                        switch (((string)((dynamic)tag).Name).ToLower())
                                        {

                                            case "sosi_verdi":
                                                verdi = ((dynamic)tag).Value;
                                                sosi_verdi = true;
                                                break;
                                        }

                                    }
                                    if (sosi_verdi && verdi.Trim().Length > 0) eg.TillatteVerdier.Add(verdi.Trim());
                                    else
                                    {
                                       
                                        eg.TillatteVerdier.Add(a.Name);
                                        
                                    }
                                }

                            }
                        }
                    }
                }
                return eg;
            }
            catch (Exception e)
            {
               _repository.WriteOutput("System", "FEIL: " +e.Message + " " + e.Source,0);
               return null;
            }

        }

        private Basiselement LagEgenskap(string prikknivå, global::EA.Attribute att, string standard)
        {
            bool sosi_navn = false;
            Basiselement eg = new Basiselement();
            eg.UML_Navn = att.Name;
            eg.SOSI_Navn = prikknivå;
            eg.Notat = att.Notes;
            eg.Standard = standard;
            
            eg.Multiplisitet = "[" + att.LowerBound + ".." + att.UpperBound + "]";
            eg.TillatteVerdier = new List<string>();
            eg.Datatype = "FIX";
            
            bool basistypeIkkeFunnet = true;

            switch (att.Type.ToLower())
            {
                case "characterstring":
                    eg.Datatype = "T";
                    if (att.Default.Length > 0) eg.TillatteVerdier.Add(att.Default);
                    
                    basistypeIkkeFunnet = false;
                    break;
                case "integer":
                    eg.Datatype = "H";
                    if (att.Default.Length > 0) eg.TillatteVerdier.Add(att.Default);
                    
                    basistypeIkkeFunnet = false;
                    break;
                case "real":
                    eg.Datatype = "D";
                    if (att.Default.Length > 0) eg.TillatteVerdier.Add(att.Default);
                    
                    basistypeIkkeFunnet = false;
                    break;
                case "date":
                    eg.Datatype = "DATO";
                    if (att.Default.Length > 0) eg.TillatteVerdier.Add(att.Default);
                   
                    basistypeIkkeFunnet = false;
                    break;
                case "datetime":
                    eg.Datatype = "DATOTID";
                    if (att.Default.Length > 0) eg.TillatteVerdier.Add(att.Default);
                    
                    basistypeIkkeFunnet = false;
                    break;
                case "boolean":
                    eg.Datatype = "BOOLSK";
                    eg.Operator = "=";
                    eg.TillatteVerdier.Add("JA");
                    eg.TillatteVerdier.Add("NEI");
                   
                    basistypeIkkeFunnet = false;
                    break;
                case "flate":
                    eg.Datatype = "FLATE";
                    
                    basistypeIkkeFunnet = false;
                    break;
                case "punkt":
                    eg.Datatype = "PUNKT";
                    basistypeIkkeFunnet = false;
                    break;
                case "kurve":
                    eg.Datatype = "KURVE";
                    
                    basistypeIkkeFunnet = false;
                    break;
            }
            if (basistypeIkkeFunnet)
            {
                _repository.WriteOutput("System", "FEIL: datatype er ikke korrekt/funnet: " + att.Name + " " + att.Type, 0);
            }
            string datatype = eg.Datatype;

            foreach (object theTags2 in att.TaggedValues)
            {
                
                string navn = (string)((dynamic)theTags2).Name;

                switch (navn.ToLower())
                {
                   
                    case "sosi_lengde":
                        eg.Datatype = datatype + ((dynamic)theTags2).Value;
                      
                        break;
                    case "sosi_navn":
                        string verdi = (string)((dynamic)theTags2).Value;
                        if (verdi.Length > 0)
                        {
                            eg.SOSI_Navn = prikknivå + ((dynamic)theTags2).Value;
                            sosi_navn = true;
                        }
                        break;

                }

            }
            if (sosi_navn == false) _repository.WriteOutput("System", "FEIL: Mangler tagged value sosi_navn på attributt: " + att.Name, 0);
            if (eg.SOSI_Navn.Length > 32) _repository.WriteOutput("System", "FEIL: tagged value SOSI_navn er lengre enn 32 tegn på attributt: " + att.Name, 0);
            return eg;
        }

        private Basiselement LagBasiselementForKjentType(string prikknivå, global::EA.Attribute att, string standard, KjentType kt)
        {
            bool sosi_navn = false;
            Basiselement eg = new Basiselement();
            eg.UML_Navn = att.Name;
            eg.SOSI_Navn = prikknivå;
            eg.Notat = att.Notes;
            eg.Standard = standard;

            eg.Multiplisitet = "[" + att.LowerBound + ".." + att.UpperBound + "]";
            eg.TillatteVerdier = new List<string>();
            eg.Datatype = kt.Datatype;

            foreach (object theTags2 in att.TaggedValues)
            {
                
                string navn = (string)((dynamic)theTags2).Name;

                switch (navn.ToLower())
                {
                    case "sosi_navn":
                        string verdi = (string)((dynamic)theTags2).Value;
                        if (verdi.Length > 0)
                        {
                            eg.SOSI_Navn = prikknivå + ((dynamic)theTags2).Value;
                            sosi_navn = true;
                        }
                        break;

                }

            }
            if (sosi_navn == false && att.ClassifierID != 0) 
            { 
           
                Element elm = _repository.GetElementByID(att.ClassifierID);

                //Finne SOSI_navn på klasse
                foreach (object theTags2 in elm.TaggedValues)
                {
                    switch (((string)((dynamic)theTags2).Name).ToLower())
                    {
                        
                        case "sosi_navn":
                            eg.SOSI_Navn = prikknivå + ((dynamic)theTags2).Value;
                            if (((dynamic)theTags2).Value.Length > 0) sosi_navn = true;
                            break;
                    }

                }

            }
            if (sosi_navn == false) _repository.WriteOutput("System", "FEIL: Mangler tagged value sosi_navn på attributt: " + att.Name, 0);

            return eg;
        }

        public List<SosiKodeliste> ByggSosiKodelister()
        {
            List<SosiKodeliste> kList = new List<SosiKodeliste>();

            Package valgtPakke = _repository.GetTreeSelectedPackage();

           

            foreach (Element el in valgtPakke.Elements)
            {

                if (el.Stereotype.ToLower() == "codelist" || el.Stereotype.ToLower() == "enumeration" || el.Type.ToLower() == "enumeration")
                {
                    LagSosiKodeliste(el,kList);
                }

            }
           
            HentKodelisterFraSubpakker(kList, valgtPakke);


            return kList;
        }

        private void HentKodelisterFraSubpakker(List<SosiKodeliste> kList, Package valgtPakke)
        {
            foreach (Package pk in valgtPakke.Packages)
            {
                foreach (Element ele in pk.Elements)
                {
                    if (ele.Stereotype.ToLower() == "codelist" || ele.Stereotype.ToLower() == "enumeration" || ele.Type.ToLower() == "enumeration")
                    {
                        LagSosiKodeliste(ele, kList);
                    }
                }
                
                HentKodelisterFraSubpakker(kList, pk);
            }
        }

        public void LagSosiKodeliste(Element e, List<SosiKodeliste> kList)
        {
            bool erSOSIKodeliste = false;

            SosiKodeliste kl = new SosiKodeliste();
            kl.Verdier = new List<SosiKode>();
            kl.Navn = e.Name;
            
            foreach (global::EA.Attribute a in e.Attributes)
            {
                SosiKode k = new SosiKode();

                k.Navn = a.Name;
                k.Beskrivelse = a.Notes.Trim();
                
                if (a.Default.Trim().Length > 0) k.SosiVerdi = a.Default.Trim();
                else
                {
                    
                    bool sosi_verdi = false;
                    string verdi = "";
                    foreach (object tag in a.TaggedValues)
                    {
                        switch (((string)((dynamic)tag).Name).ToLower())
                        {

                            case "sosi_verdi":
                                verdi = ((dynamic)tag).Value;
                                sosi_verdi = true;
                                
                                break;
                        }

                    }
                    if (sosi_verdi && verdi.Trim().Length > 0)
                    {
                        k.SosiVerdi = verdi.Trim();
                        erSOSIKodeliste = true;
                    }
                    else
                    {
                       
                        k.SosiVerdi = a.Name;
                    }
                }
                kl.Verdier.Add(k);

            }
            if (erSOSIKodeliste)
            {
                kList.Add(kl);
            }
        }
    }
}
