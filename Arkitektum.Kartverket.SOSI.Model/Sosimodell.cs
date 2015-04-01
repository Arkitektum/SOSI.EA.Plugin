using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using EA;
using System.IO;
using System.Xml;

namespace Arkitektum.Kartverket.SOSI.Model
{
    public class Sosimodell
    {
        private List<KjentType> KjenteTyper;

        public Sosimodell() {
            KjenteTyper = new List<KjentType>();

            KjentType t1 = new KjentType();
            t1.Navn="Link";
            t1.Datatype = "T255";
            KjenteTyper.Add(t1);
            KjentType t2 = new KjentType();
            t2.Navn = "Navn";
            t2.Datatype = "T255";
            KjenteTyper.Add(t2);
            KjentType t3 = new KjentType();
            t3.Navn = "Geodataprodusent";
            t3.Datatype = "T35";
            KjenteTyper.Add(t3);
            KjentType t4 = new KjentType();
            t4.Navn = "Geodataeier";
            t4.Datatype = "T35";
            KjenteTyper.Add(t4);
            KjentType t5 = new KjentType();
            t5.Navn = "Kontaktperson";
            t5.Datatype = "T255";
            KjenteTyper.Add(t5);
            KjentType t6 = new KjentType();
            t6.Navn = "Høyde";
            t6.Datatype = "D10";
            KjenteTyper.Add(t6);
            KjentType t7 = new KjentType();
            t7.Navn = "Dybde";
            t7.Datatype = "D10";
            KjenteTyper.Add(t7);
            KjentType t8 = new KjentType();
            t8.Navn = "Temperatur";
            t8.Datatype = "D10";
            KjenteTyper.Add(t8);
            KjentType t9 = new KjentType();
            t9.Navn = "Misvisning";
            t9.Datatype = "D10";
            KjenteTyper.Add(t9);
            KjentType t10 = new KjentType();
            t10.Navn = "HøydeOverBakken";
            t10.Datatype = "D10";
            KjenteTyper.Add(t10);
            KjentType t11 = new KjentType();
            t11.Navn = "Organisasjonsnummer";
            t11.Datatype = "H8";
            KjenteTyper.Add(t11);


        }

        public List<Objekttype> ByggObjektstruktur(Repository _rep)
        {
            
            List<Objekttype> otList = new List<Objekttype>();

            Package valgtPakke = _rep.GetTreeSelectedPackage();

            foreach (Element el in valgtPakke.Elements)
            {

                if (el.Type == "Class" && (el.Stereotype.ToLower() == "featuretype" || el.Stereotype.ToLower() == "type") && el.Abstract == "0")
                {

                    _rep.WriteOutput("System", "INFO: Funne objekttype: " + el.Name, 0);
                    Objekttype ot = LagObjekttype(_rep, el, "..", true,null);
                    otList.Add(ot);
                }

            }

            HentElementerFraSubpakker(_rep, otList, valgtPakke);


            return otList;
        }

        private void HentElementerFraSubpakker(Repository _rep, List<Objekttype> otList, Package valgtPakke)
        {
            foreach (Package pk in valgtPakke.Packages)
            {
                foreach (Element ele in pk.Elements)
                {
                    if (ele.Type == "Class" && (ele.Stereotype.ToLower() == "featuretype" || ele.Stereotype.ToLower() == "type") && ele.Abstract == "0")
                    {
                        _rep.WriteOutput("System", "INFO: Funne objekttype: " + ele.Name, 0);
                        Objekttype ot = LagObjekttype(_rep, ele, "..", true,null);
                        otList.Add(ot);
                    }
                }
                HentElementerFraSubpakker(_rep, otList, pk);
            }
        }

        



        public Objekttype LagObjekttype(Repository _rep, Element e, string prikknivå, bool lagobjekttype, List<Beskrankning> oclfraSubObjekt)
        {
            Objekttype ot = new Objekttype();
            ot.Egenskaper = new List<AbstraktEgenskap>();
            ot.Geometrityper = new List<string>();
            ot.OCLconstraints = new List<Beskrankning>();
            ot.Avgrenser= new List<string>();
            ot.AvgrensesAv = new List<string>();

            ot.UML_Navn = e.Name;
            ot.Notat = e.Notes;
            ot.SOSI_Navn = prikknivå;
            string standard = HentApplicationSchemaPakkeNavn(e, _rep);
            ot.Standard = standard;
            if (lagobjekttype)
            {
                Basiselement objtype = new Basiselement();
                objtype.Standard = standard;
                objtype.SOSI_Navn = prikknivå + "OBJTYPE";
                objtype.UML_Navn = "";
                
                objtype.Operator = "=";
                objtype.TillatteVerdier = new List<string>();
                objtype.TillatteVerdier.Add(e.Name);
                objtype.Multiplisitet = "[1..1]";
                objtype.Datatype = "T32";
                ot.Egenskaper.Add(objtype);
                if (e.Name.Length > 32) _rep.WriteOutput("System", "FEIL: Objektnavn er lengre enn 32 tegn - " + e.Name, 0);
            }

            if (oclfraSubObjekt != null)
            {
                foreach (Beskrankning bs in oclfraSubObjekt)
                {
                    ot.OCLconstraints.Add(bs);
                }
            }
            foreach (global::EA.Constraint constr in e.Constraints)
            {
                Beskrankning bskr = new Beskrankning();
                bskr.Navn = constr.Name;
                string ocldesc = "";
                if (constr.Notes.Contains("/*") && constr.Notes.Contains("*/"))
                {
                    ocldesc = constr.Notes.Substring(constr.Notes.ToLower().IndexOf("/*")+2, constr.Notes.ToLower().IndexOf("*/")-2 - constr.Notes.ToLower().IndexOf("/*"));
                }
                bskr.Notat = ocldesc;
                bskr.OCL = constr.Notes;

                ot.OCLconstraints.Add(bskr);
            }

            foreach (global::EA.Attribute att in e.Attributes)
            {

                if (att.ClassifierID != 0) 
                {
                    try
                    {
                        Element elm = _rep.GetElementByID(att.ClassifierID);
                        Boolean kjentType = false;
                        foreach (KjentType kt in KjenteTyper)
	                    {
		                    if (kt.Navn == att.Type) {
                                kjentType=true;
                                Basiselement eg = LagEgenskapKjentType(prikknivå, att, _rep, standard,kt);
                                ot.Egenskaper.Add(eg);
                            }
	                    }
                        if (kjentType)
                        {
                            //Alt utført
                        }
                        else if (att.Type.ToLower() == "integer" || att.Type.ToLower() == "characterstring" || att.Type.ToLower() == "real" || att.Type.ToLower() == "date" || att.Type.ToLower() == "datetime" || att.Type.ToLower() == "boolean")
                        {
                            Basiselement eg = LagEgenskap(prikknivå, att, _rep, standard);
                            ot.Egenskaper.Add(eg);
                        }
                        else if (elm.Stereotype.ToLower() == "codelist" || elm.Stereotype.ToLower() == "enumeration")
                        {
                            Basiselement eg = LagKodelisteEgenskap(prikknivå, elm, _rep, att, ot.OCLconstraints);
                            ot.Egenskaper.Add(eg);
                        }
                        else if (att.Type.ToLower() == "flate" || att.Type.ToLower() == "punkt" || att.Type.ToLower() == "sverm")
                        {
                            ot.Geometrityper.Add(att.Type.ToUpper());
                        }
                        else if (att.Type.ToLower() == "kurve")
                        {
                            ot.Geometrityper.Add("KURVE");
                            ot.Geometrityper.Add("BUEP");
                            ot.Geometrityper.Add("SIRKELP");
                            ot.Geometrityper.Add("BEZIER");
                            ot.Geometrityper.Add("KLOTOIDE");
                        }
                        else if (elm.Stereotype.ToLower() == "union")
                        {
                            LagUnionEgenskaper(prikknivå, elm, _rep, att, ot, standard);

                        }
                        else
                        {
                            Gruppeelement tmp = LagGruppeelement(_rep, elm, att, prikknivå, ot);
                            ot.Egenskaper.Add(tmp);
                        }
                    }
                    catch (Exception ex)
                    {
                        _rep.WriteOutput("System", "FEIL: Finner ikke datatype for " + att.Name + " på " + e.Name + " :" + ex.Message, 0);
                    }
                }
                else
                {
                    if (att.Type.ToLower() == "flate" || att.Type.ToLower() == "punkt" || att.Type.ToLower() == "sverm")
                    {
                        ot.Geometrityper.Add(att.Type.ToUpper());
                    }
                    else if (att.Type.ToLower() == "kurve")
                    {
                        ot.Geometrityper.Add("KURVE");
                        ot.Geometrityper.Add("BUEP");
                        ot.Geometrityper.Add("SIRKELP");
                        ot.Geometrityper.Add("BEZIER");
                        ot.Geometrityper.Add("KLOTOIDE");
                        
                    }
                    else
                    {

                        Basiselement eg = LagEgenskap(prikknivå, att, _rep, standard);
                        ot.Egenskaper.Add(eg);
                    }
                }
            }

            foreach (Connector connector in e.Connectors)
            {
                if (connector.MetaType == "topo")
                {
                    if (connector.Stereotype.ToLower() == "topo")
                    {
                        Element source = _rep.GetElementByID(connector.SupplierID);
                        Element destination = _rep.GetElementByID(connector.ClientID);
                        if (connector.Direction == "Bi-Directional")
                        {
                            _rep.WriteOutput("System", "FEIL: Topo assosiasjonen kan ikke ha 'Bi-Directional' mellom " + source.Name + " og " + destination.Name, 0);
                        }
                        else if (connector.Direction == "Source -> Destination")
                        {
                            if (source.Name != e.Name)
                                ot.AvgrensesAv.Add(source.Name);
                            else
                            {
                                ot.Avgrenser.Add(destination.Name);
                            }
                        }
                        else if (connector.Direction == "Destination -> Source")
                        {
                            if (destination.Name != e.Name)
                                ot.AvgrensesAv.Add(destination.Name);
                            else
                            {
                                ot.Avgrenser.Add(source.Name);
                            }
                        }
                        else if (connector.Direction == "Unspecified")
                        {
                            _rep.WriteOutput("System", "ADVARSEL: Topo assosiasjonen mangler angivelse av 'Direction' mellom " + source.Name + " og " + destination.Name, 0);
                        }
                    }
                }
                if (connector.MetaType == "Association" || connector.MetaType == "Aggregation")
                {
                    Element source = _rep.GetElementByID(connector.SupplierID);
                    Element destination = _rep.GetElementByID(connector.ClientID);
                    bool is_source = false;

                    if (source.Name == e.Name) is_source = true;
                    else is_source = false;

                    if (connector.Stereotype.ToLower() == "topo")
                    {
                        if (connector.Direction == "Bi-Directional")
                        {
                            _rep.WriteOutput("System", "FEIL: Topo assosiasjonen kan ikke ha 'Bi-Directional' mellom " + source.Name + " og " + destination.Name, 0);
                        }
                        else if (connector.Direction == "Source -> Destination")
                        {
                            if (source.Name != e.Name)
                                ot.AvgrensesAv.Add(source.Name);
                            else
                            {
                                ot.Avgrenser.Add(destination.Name);
                            }
                        }
                        else if (connector.Direction == "Destination -> Source")
                        {
                            if (destination.Name != e.Name)
                                ot.AvgrensesAv.Add(destination.Name);
                            else
                            {
                                ot.Avgrenser.Add(source.Name);
                            }
                        }
                        else if (connector.Direction == "Unspecified")
                        {
                            _rep.WriteOutput("System", "ADVARSEL: Topo assosiasjonen mangler angivelse av 'Direction' mellom " + source.Name + " og " + destination.Name, 0);
                        }
                    }
                    else if (connector.SupplierEnd.Aggregation == 2 && connector.Direction == "Destination -> Source" && is_source == true) //Composite
                    {
                        _rep.WriteOutput("System", "INFO:  Komposisjon " + connector.ClientEnd.Role + " og " + e.Name, 0);
                        Gruppeelement tmp = LagGruppeelementKomposisjon(_rep, connector, prikknivå, ot);
                        ot.Egenskaper.Add(tmp);
                    }
                    else if (connector.ClientEnd.Aggregation == 2 && connector.Direction == "Source -> Destination" && is_source == false) //Composite
                    {
                        _rep.WriteOutput("System", "INFO:  Komposisjon " + connector.ClientEnd.Role + " og " + e.Name, 0);
                        Gruppeelement tmp = LagGruppeelementKomposisjon(_rep, connector, prikknivå, ot);
                        ot.Egenskaper.Add(tmp);
                    }
                    else
                    {
                        if (connector.SupplierID == connector.ClientID)
                        { 
                            //Kobling med seg selv...TODO må ha en test på bare en runde hvis denne ikke skal gå i loop
                        }
                        else if (connector.Direction == "Bi-Directional")
                        {
                            List<AbstraktEgenskap> eg = LagConnectorEgenskaper(prikknivå, connector, e, _rep, standard, ot);
                            ot.Egenskaper.AddRange(eg);
                        }
                        else if (connector.Direction == "Source -> Destination" && is_source == false)
                        {
                            List<AbstraktEgenskap> eg = LagConnectorEgenskaper(prikknivå, connector, e, _rep, standard, ot);
                            ot.Egenskaper.AddRange(eg);
                        }
                        else if (connector.Direction == "Destination -> Source" && is_source == true)
                        {
                            List<AbstraktEgenskap> eg = LagConnectorEgenskaper(prikknivå, connector, e, _rep, standard, ot);
                            ot.Egenskaper.AddRange(eg);
                        }
                        else if (connector.Direction == "Unspecified")
                        {
                            _rep.WriteOutput("System", "ADVARSEL: Assosiasjonen mangler angivelse av 'Direction' mellom " + source.Name + " og " + destination.Name, 0);
                        }


                    }
                }
                else if (connector.MetaType == "Generalization")
                {

                    Element elm = _rep.GetElementByID(connector.SupplierID);
                    if (e.Name != elm.Name)
                    {
                        Objekttype tmp2 = LagObjekttype(_rep, elm, prikknivå, false, ot.OCLconstraints);
                        ot.Inkluder = tmp2;
                        foreach (string geo in tmp2.Geometrityper)
                        {
                            ot.Geometrityper.Add(geo);
                        }
                        foreach (string obj in tmp2.Avgrenser)
                        {
                            ot.Avgrenser.Add(obj);
                        }
                        foreach (string obj in tmp2.AvgrensesAv)
                        {
                            ot.AvgrensesAv.Add(obj);
                        }
                    }

                }
            }

            return ot;

        }

        private void LagUnionEgenskaper(string prikknivå, Element elm, Repository _rep, global::EA.Attribute att2, Objekttype ot, string standard)
        {
            List<string> attnavn = new List<string>();

            foreach (global::EA.Attribute att in elm.Attributes)
            {
                attnavn.Add(att.Name);

                if (att.ClassifierID != 0) 
                {
                    Element elm1 = _rep.GetElementByID(att.ClassifierID);

                    if (elm1.Stereotype.ToLower() == "codelist")
                    {
                        Basiselement eg = LagKodelisteEgenskap(prikknivå + ".", elm1, _rep, att, ot.OCLconstraints);
                        ot.Egenskaper.Add(eg);
                    }
                    else if (elm1.Stereotype.ToLower() == "union")
                    {
                        LagUnionEgenskaper(prikknivå, elm1, _rep, att, ot, standard);
                        
                    }
                    else if (att.Type.ToLower() == "integer" || att.Type.ToLower() == "characterstring" || att.Type.ToLower() == "real" || att.Type.ToLower() == "date" || att.Type.ToLower() == "datetime" || att.Type.ToLower() == "boolean")
                    {
                        Basiselement eg = LagEgenskap(prikknivå + ".", att, _rep, standard);
                        ot.Egenskaper.Add(eg);
                    }
                    else if (att.Type.ToLower() == "flate" || att.Type.ToLower() == "punkt" || att.Type.ToLower() == "kurve")
                    {

                        Basiselement eg = LagEgenskap(prikknivå + ".", att, _rep, standard);
                        ot.Egenskaper.Add(eg);
                    }
                    else
                    {
                        Gruppeelement tmp = LagGruppeelement(_rep, elm1, att, prikknivå,ot);
                        ot.Egenskaper.Add(tmp);
                    }
                }
                else
                {

                    Basiselement eg = LagEgenskap(prikknivå + ".", att, _rep, standard);
                    ot.Egenskaper.Add(eg);
                }

            }
            Beskrankning bs = new Beskrankning();
            bs.Navn = "Union " + elm.Name;
            bs.Notat = "et av elementene " + String.Join(",", attnavn.ToArray(), 0, attnavn.Count) + " er påkrevet";
            ot.OCLconstraints.Add(bs);
        }

        private List<AbstraktEgenskap> LagConnectorEgenskaper(string prikknivå, Connector connector, Element e, Repository _rep, string standard, Objekttype ot)
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
            

            Element source = _rep.GetElementByID(connector.SupplierID);
            Element destination = _rep.GetElementByID(connector.ClientID);
            
            if (typeAssosiasjon.Length == 0)
            {
                _rep.WriteOutput("System", "ADVARSEL: Det er ikke definert type SOSI assosiasjon mellom " + source.Name + " og " + destination.Name + " assosiasjonen. Behandles som REF.", 0);
                typeAssosiasjon = "ref";
            }

            bool is_source = false;

            if (source.Name == e.Name) is_source = true;
            else is_source = false;

            if (is_source)
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
                        _rep.WriteOutput("System", "FEIL: Finner ikke tagged value SOSI_navn for " + connector.ClientEnd.Role + " på " + destination.Name, 0);
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
                    SosiEgenskapRetur ref_sosinavn = FinnPrimærnøkkel(destination, prikknivå + ".", _rep, standard, ot, sjekkedeObjekter);

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
                    else _rep.WriteOutput("System", "FEIL: Finner ikke primærnøkkel for " + connector.ClientEnd.Role + " på " + destination.Name, 0);
                }
                else if (typeAssosiasjon == "fremmednøkler")
                {

                    SosiEgenskapRetur pn_sosinavn = FinnFremmednøkkel(destination, source, connector, connector.ClientEnd, _rep);
                    if (pn_sosinavn != null)
                    {
                        Basiselement eg = new Basiselement();
                        eg.Datatype = pn_sosinavn.SOSI_Datatype + pn_sosinavn.SOSI_Lengde;
                        eg.SOSI_Navn = prikknivå + pn_sosinavn.SOSI_Navn;
                        eg.UML_Navn = connector.ClientEnd.Role + "(rolle)";
                        eg.Multiplisitet = "[" + connector.ClientEnd.Cardinality + "]";
                        eg.Standard = HentApplicationSchemaPakkeNavn(destination, _rep);
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
                    Basiselement eg = new Basiselement();
                    eg.Datatype = "REF";
                    eg.UML_Navn = connector.ClientEnd.Role + "(rolle)";
                    eg.Multiplisitet = "[" + connector.ClientEnd.Cardinality + "]";
                    eg.Standard = HentApplicationSchemaPakkeNavn(destination, _rep);

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
                    if (is_sosi_navn == false) _rep.WriteOutput("System", "FEIL: Mangler angivelse av tag sosi_navn på assosiasjonsende " + connector.ClientEnd.Role + " mellom " + connector.ClientEnd.RoleType +  " og " + connector.SupplierEnd.RoleType, 0);
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
                        _rep.WriteOutput("System", "FEIL: Finner ikke tagged value SOSI_navn for " + connector.SupplierEnd.Role + " på " + source.Name, 0);
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
                    SosiEgenskapRetur ref_sosinavn = FinnPrimærnøkkel(source,prikknivå+".",_rep,standard,ot,sjekkedeObjekter);
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
                    else _rep.WriteOutput("System", "FEIL: Finner ikke primærnøkkel for " + connector.SupplierEnd.Role + " på " + source.Name, 0);
                }
                else if (typeAssosiasjon == "fremmednøkler")
                {
                    SosiEgenskapRetur pn_sosinavn = FinnFremmednøkkel(source, destination, connector, connector.SupplierEnd, _rep);
                    if (pn_sosinavn != null)
                    {
                        Basiselement eg = new Basiselement();
                        eg.UML_Navn = connector.SupplierEnd.Role + "(rolle)";
                        eg.Multiplisitet = "[" + connector.SupplierEnd.Cardinality + "]";
                        eg.Datatype = pn_sosinavn.SOSI_Datatype + pn_sosinavn.SOSI_Lengde;
                        eg.SOSI_Navn = prikknivå + pn_sosinavn.SOSI_Navn;
                        eg.Standard = HentApplicationSchemaPakkeNavn(source, _rep);
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
                    eg.Standard = HentApplicationSchemaPakkeNavn(source, _rep);
                    //Fikse multiplisitet
                    eg.Multiplisitet = eg.Multiplisitet.Replace("[0]", "[0..1]");
                    eg.Multiplisitet = eg.Multiplisitet.Replace("[0..]", "[0..*]");
                    eg.Multiplisitet = eg.Multiplisitet.Replace("[1..]", "[1..*]");
                    eg.Multiplisitet = eg.Multiplisitet.Replace("[1]", "[1..1]");

                    eg.TillatteVerdier = new List<string>();

                    retur.Add(eg);
                    if (is_sosi_navn == false) _rep.WriteOutput("System", "FEIL: Mangler angivelse av tag sosi_navn på assosiasjonsende " + connector.SupplierEnd.Role + " mellom " + connector.ClientEnd.RoleType + " og " + connector.SupplierEnd.RoleType, 0);
                
                }
            }
            return retur;
        }

     

        private SosiEgenskapRetur FinnFremmednøkkel(Element destination, Element source, Connector connector, ConnectorEnd end, Repository _rep)
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
                        _rep.WriteOutput("System", "FEIL: Ufullstendig assosiasjon: " + end.Role, 0);
                    }
                }
            }
            
            return retur;
        }

        private SosiEgenskapRetur FinnPrimærnøkkel(Element source, string prikknivå, Repository _rep, string standard, Objekttype ot, List<String> arvedeobjekterSjekket)
        {
           
            SosiEgenskapRetur retur = null;
            retur = new SosiEgenskapRetur();
            retur.Egenskaper = new List<AbstraktEgenskap>();

            foreach (global::EA.Attribute att in source.Attributes)
            {
                bool er_primærnøkkel = false;

                foreach (object tag in att.TaggedValues)
                {

                    switch (((string)((dynamic)tag).Name).ToLower())
                    {
                        case "sosi_primærnøkkel":
                            if (((string)((dynamic)tag).Value).ToLower() == "true") er_primærnøkkel = true;
                            break;
                    }
                }
                if (er_primærnøkkel)
                {
                    
                        if (att.Type.ToLower() == "integer" || att.Type.ToLower() == "characterstring" || att.Type.ToLower() == "real" || att.Type.ToLower() == "date" || att.Type.ToLower() == "datetime" || att.Type.ToLower() == "boolean")
                        {
                            Basiselement eg = LagEgenskap(prikknivå, att, _rep, standard);
                            retur.Egenskaper.Add(eg);
                        }
                        else //Kompleks type(datatype) som primærnøkkel
                        {

                            if (att.ClassifierID != 0) 
                            {
                                Element elm1 = _rep.GetElementByID(att.ClassifierID);
                                if (elm1.Stereotype.ToLower() == "codelist")
                                {
                                    Basiselement tmp = LagKodelisteEgenskap(prikknivå, elm1, _rep, att, ot.OCLconstraints);
                                    retur.Egenskaper.Add(tmp);
                                }
                                else
                                {
                                    Gruppeelement tmp = LagGruppeelement(_rep, elm1, att, prikknivå, ot);
                                    retur.Egenskaper.Add(tmp);
                                }
                            }
                            else _rep.WriteOutput("System", "FEIL: Primærnøkkel er feil definert på : " + source.Name, 0);
                           
                        }
                   
                    
                }
               
                foreach (Connector connector in source.Connectors)
                {
                    if (connector.MetaType == "Generalization")
                    {
                        Element elmg = _rep.GetElementByID(connector.SupplierID);
                        if (source.Name != elmg.Name)
                        {
                            if (arvedeobjekterSjekket.Contains(elmg.Name) == false)
                            {
                                var ret = FinnPrimærnøkkel(elmg, prikknivå, _rep, standard, ot, arvedeobjekterSjekket);
                                arvedeobjekterSjekket.Add(elmg.Name);
                                retur.Egenskaper.AddRange(ret.Egenskaper);
                            }
                            
                        }

                    }
                }
               

            }
            return retur;
        }

       

        private Gruppeelement LagGruppeelement(Repository _rep, Element elm, global::EA.Attribute att2, string prikknivå, Objekttype pot)
        {
            Gruppeelement ot = new Gruppeelement();
            ot.Egenskaper = new List<AbstraktEgenskap>();
            ot.OCLconstraints = new List<Beskrankning>();
            bool sosi_navn = false;
            
            ot.Notat = elm.Notes;
            ot.SOSI_Navn = prikknivå;
            string standard = HentApplicationSchemaPakkeNavn(elm, _rep);
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
            if (sosi_navn == false) _rep.WriteOutput("System", "FEIL: Mangler sosi_navn på gruppeelement: " + elm.Name, 0);

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
                    Element elm1 = _rep.GetElementByID(att.ClassifierID);
                    
                    Boolean kjentType = false;
                    foreach (KjentType kt in KjenteTyper)
                    {
                        if (kt.Navn == att.Type)
                        {
                            kjentType = true;
                            Basiselement eg = LagEgenskapKjentType(prikknivå, att, _rep, standard, kt);
                            ot.Egenskaper.Add(eg);
                        }
                    }
                    if (kjentType)
                    {
                        //Alt utført
                    }
                    else if (elm1.Stereotype.ToLower() == "codelist")
                    {
                        Basiselement eg = LagKodelisteEgenskap(prikknivå + ".", elm1, _rep, att, pot.OCLconstraints);
                        ot.Egenskaper.Add(eg);
                    }
                    else if (elm1.Stereotype.ToLower() == "union")
                    {
                        LagUnionEgenskaperForGruppeelement(prikknivå, elm1, _rep, att, pot, standard, ot);
                    }
                    else if (att.Type.ToLower() == "integer" || att.Type.ToLower() == "characterstring" || att.Type.ToLower() == "real" || att.Type.ToLower() == "date" || att.Type.ToLower() == "datetime" || att.Type.ToLower() == "boolean")
                    {
                        Basiselement eg = LagEgenskap(prikknivå + ".", att, _rep, standard);
                        ot.Egenskaper.Add(eg);
                    }
                    else if (att.Type.ToLower() == "flate" || att.Type.ToLower() == "punkt" || att.Type.ToLower() == "kurve")
                    {

                        Basiselement eg = LagEgenskap(prikknivå + ".", att, _rep, standard);
                        ot.Egenskaper.Add(eg);
                    }
                    else
                    {
                        Gruppeelement tmp = LagGruppeelement(_rep, elm1, att, prikknivå + ".", pot);
                        ot.Egenskaper.Add(tmp);
                    }
                }
                else
                {

                    Basiselement eg = LagEgenskap(prikknivå + ".", att, _rep, standard);
                    ot.Egenskaper.Add(eg);
                }

            }
            foreach (Connector connector in elm.Connectors)
            {
                if (connector.MetaType == "Association" || connector.MetaType == "Aggregation")
                {
                    Element source = _rep.GetElementByID(connector.SupplierID);
                    Element destination = _rep.GetElementByID(connector.ClientID);
                    bool is_source = false;

                    if (source.Name == elm.Name) is_source = true;
                    else is_source = false;

                    if (connector.SupplierEnd.Aggregation == 2 && connector.Direction == "Destination -> Source" && is_source == true) //Composite
                    {
                        _rep.WriteOutput("System", "INFO:  Komposisjon " + connector.ClientEnd.Role + " og " + elm.Name, 0);
                        Gruppeelement tmp = LagGruppeelementKomposisjon(_rep, connector, prikknivå, pot);
                        ot.Egenskaper.Add(tmp);
                    }
                    else if (connector.ClientEnd.Aggregation == 2 && connector.Direction == "Source -> Destination" && is_source == false) //Composite
                    {
                        _rep.WriteOutput("System", "INFO:  Komposisjon " + connector.ClientEnd.Role + " og " + elm.Name, 0);
                        Gruppeelement tmp = LagGruppeelementKomposisjon(_rep, connector, prikknivå, pot);
                        ot.Egenskaper.Add(tmp);
                    }
                    else
                    {

                        if (connector.Direction == "Bi-Directional")
                        {
                            List<AbstraktEgenskap> eg = LagConnectorEgenskaper(prikknivå, connector, elm, _rep, standard, pot);
                            ot.Egenskaper.AddRange(eg);
                        }
                        else if (connector.Direction == "Source -> Destination" && is_source == false)
                        {
                            List<AbstraktEgenskap> eg = LagConnectorEgenskaper(prikknivå, connector, elm, _rep, standard, pot);
                            ot.Egenskaper.AddRange(eg);
                        }
                        else if (connector.Direction == "Destination -> Source" && is_source == true)
                        {
                            List<AbstraktEgenskap> eg = LagConnectorEgenskaper(prikknivå, connector, elm, _rep, standard, pot);
                            ot.Egenskaper.AddRange(eg);
                        }
                        else if (connector.Direction == "Unspecified")
                        {
                            _rep.WriteOutput("System", "ADVARSEL: Assosiasjonen mangler angivelse av 'Direction' mellom " + source.Name + " og " + destination.Name, 0);
                        }
                    }
                }
                else if (connector.MetaType == "Generalization")
                {

                    Element elmg = _rep.GetElementByID(connector.SupplierID);

                    if (elm.Name != elmg.Name)
                    {
                        Gruppeelement tmp2 = LagGruppeelement(_rep, elmg, null, prikknivå , pot);
                        ot.Inkluder = tmp2;
                    }

                }
            }
            return ot;
        }

        private Gruppeelement LagGruppeelementKomposisjon(Repository _rep, global::EA.Connector conn, string prikknivå, Objekttype pot)
        {
            Element elm = _rep.GetElementByID(conn.ClientID);
            
            Gruppeelement ot = new Gruppeelement();
            ot.Egenskaper = new List<AbstraktEgenskap>();
            ot.OCLconstraints = new List<Beskrankning>();
            bool sosi_navn = false;
            ot.UML_Navn = conn.ClientEnd.Role + "(rolle)";
            ot.Notat = elm.Notes;
            ot.SOSI_Navn = prikknivå;
            string standard = HentApplicationSchemaPakkeNavn(elm, _rep);
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

            if (sosi_navn == false) _rep.WriteOutput("System", "FEIL: Mangler sosi_navn på komposisjon: " + conn.ClientEnd.Role, 0);

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
                   
                    Element elm1 = _rep.GetElementByID(att.ClassifierID);

                    Boolean kjentType = false;
                    foreach (KjentType kt in KjenteTyper)
                    {
                        if (kt.Navn == att.Type)
                        {
                            kjentType = true;
                            Basiselement eg = LagEgenskapKjentType(prikknivå, att, _rep, standard, kt);
                            ot.Egenskaper.Add(eg);
                        }
                    }
                    if (kjentType)
                    {
                        //Alt utført
                    }
                    else if (elm1.Stereotype.ToLower() == "codelist")
                    {
                        Basiselement eg = LagKodelisteEgenskap(prikknivå + ".", elm1, _rep, att, pot.OCLconstraints);
                        ot.Egenskaper.Add(eg);
                    }
                    else if (elm1.Stereotype.ToLower() == "union")
                    {
                        LagUnionEgenskaperForGruppeelement(prikknivå, elm1, _rep, att, pot, standard, ot);
                    }
                    else if (att.Type.ToLower() == "integer" || att.Type.ToLower() == "characterstring" || att.Type.ToLower() == "real" || att.Type.ToLower() == "date" || att.Type.ToLower() == "datetime" || att.Type.ToLower() == "boolean")
                    {
                        Basiselement eg = LagEgenskap(prikknivå + ".", att, _rep, standard);
                        ot.Egenskaper.Add(eg);
                    }
                    else if (att.Type.ToLower() == "flate" || att.Type.ToLower() == "punkt" || att.Type.ToLower() == "kurve")
                    {
                        Basiselement eg = LagEgenskap(prikknivå + ".", att, _rep, standard);
                        ot.Egenskaper.Add(eg);
                    }
                    else
                    {
                        Gruppeelement tmp = LagGruppeelement(_rep, elm1, att, prikknivå + ".", pot);
                        ot.Egenskaper.Add(tmp);
                    }
                }
                else
                {
                    Basiselement eg = LagEgenskap(prikknivå + ".", att, _rep, standard);
                    ot.Egenskaper.Add(eg);
                }

            }

            foreach (Connector connector in elm.Connectors)
            {
                if (connector.MetaType == "Association" || connector.MetaType == "Aggregation")
                {
                    Element source = _rep.GetElementByID(connector.SupplierID);
                    Element destination = _rep.GetElementByID(connector.ClientID);
                    bool is_source = false;

                    if (source.Name == elm.Name) is_source = true;
                    else is_source = false;

                    if (connector.Direction == "Bi-Directional")
                    {
                        List<AbstraktEgenskap> eg = LagConnectorEgenskaper(prikknivå, connector, elm, _rep, standard, pot);
                        ot.Egenskaper.AddRange(eg);
                    }
                    else if (connector.Direction == "Source -> Destination" && is_source == false)
                    {
                        List<AbstraktEgenskap> eg = LagConnectorEgenskaper(prikknivå, connector, elm, _rep, standard, pot);
                        ot.Egenskaper.AddRange(eg);
                    }
                    else if (connector.Direction == "Destination -> Source" && is_source == true)
                    {
                        List<AbstraktEgenskap> eg = LagConnectorEgenskaper(prikknivå, connector, elm, _rep, standard, pot);
                        ot.Egenskaper.AddRange(eg);
                    }
                    else if (connector.Direction == "Unspecified")
                    {
                        _rep.WriteOutput("System", "ADVARSEL: Assosiasjonen mangler angivelse av 'Direction' mellom " + source.Name + " og " + destination.Name, 0);
                    }
                }
                if (connector.MetaType == "Generalization")
                {

                    Element elmg = _rep.GetElementByID(connector.SupplierID);
                    if (elm.Name != elmg.Name)
                    {
                        Gruppeelement tmp2 = LagGruppeelement(_rep, elmg, null, prikknivå, pot);
                        ot.Inkluder = tmp2;
                    }

                }
            }
            return ot;
        }

        private void LagUnionEgenskaperForGruppeelement(string prikknivå, Element elm, Repository _rep, global::EA.Attribute att1, Objekttype pot, string standard, Gruppeelement ot)
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
                   Element elm1 = _rep.GetElementByID(att.ClassifierID);
                   if (elm1.Stereotype.ToLower() == "codelist")
                    {
                        Basiselement eg = LagKodelisteEgenskap(prikknivå + ".", elm1, _rep, att, pot.OCLconstraints);
                        ot.Egenskaper.Add(eg);
                    }
                    else if (elm1.Stereotype.ToLower() == "union")
                    {
                        LagUnionEgenskaperForGruppeelement(prikknivå, elm, _rep, att, pot, standard, ot);

                    }
                    else if (att.Type.ToLower() == "integer" || att.Type.ToLower() == "characterstring" || att.Type.ToLower() == "real" || att.Type.ToLower() == "date" || att.Type.ToLower() == "datetime" || att.Type.ToLower() == "boolean")
                    {
                        Basiselement eg = LagEgenskap(prikknivå + ".", att, _rep, standard);
                        ot.Egenskaper.Add(eg);
                    }
                   
                    else if (att.Type.ToLower() == "flate" || att.Type.ToLower() == "punkt" || att.Type.ToLower() == "kurve")
                    {

                        Basiselement eg = LagEgenskap(prikknivå + ".", att, _rep, standard);
                        ot.Egenskaper.Add(eg);
                    }
                    else
                    {
                        Gruppeelement tmp = LagGruppeelement(_rep, elm1, att, prikknivå + ".", pot);
                        ot.Egenskaper.Add(tmp);
                    }
                }
                else
                {

                    Basiselement eg = LagEgenskap(prikknivå + ".", att, _rep, standard);
                    ot.Egenskaper.Add(eg);
                }

            }
           
            Beskrankning bs = new Beskrankning();
            bs.Navn = "Union " + elm.Name;
            bs.Notat = "et av elementene " + String.Join(",", attnavn.ToArray(), 0, attnavn.Count) + " er påkrevet";
            pot.OCLconstraints.Add(bs);

            

        }

        private string HentApplicationSchemaPakkeNavn(Element elm, Repository rep)
        {
            string pnavn = "FIX";
           
            if (elm.PackageID != 0)
            {
                Package pk = rep.GetPackageByID(elm.PackageID);
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
                    else pnavn = HentApplicationSchemaPakkeNavn(pk.Element, rep);
                }
            }
            return pnavn;
        }
        private Basiselement LagKodelisteEgenskap(string prikknivå, Element elm, Repository rep, global::EA.Attribute att, List<Beskrankning> oclliste)
        {
            
            try
            {

                Basiselement eg = new Basiselement();
                eg.UML_Navn = att.Name;
                eg.SOSI_Navn = prikknivå;
                eg.Notat = att.Notes;
                eg.Standard = HentApplicationSchemaPakkeNavn(elm, rep);
                
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
                                rep.WriteOutput("System", "ADVARSEL: Fant beskrankning for " + att.Name + " i " + elm.Name + " men klarte ikke å løse operator uttrykket.", 0);
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
                if (sosi_navn == false) rep.WriteOutput("System", "FEIL: Mangler sosi_navn på kodeliste: " + elm.Name + " attributt: " + att.Name, 0);
                if (sosi_lengde == false) rep.WriteOutput("System", "FEIL: Mangler sosi_lengde på kodeliste: " + elm.Name + " attributt: " + att.Name, 0);
                
                foreach (Connector connector in elm.Connectors)
                {
                    if (connector.MetaType == "Generalization")
                    {

                        Element elmg = rep.GetElementByID(connector.SupplierID);
                        
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
               rep.WriteOutput("System", "FEIL: " +e.Message + " " + e.Source,0);
               return null;
            }

        }

        private static Basiselement LagEgenskap(string prikknivå, global::EA.Attribute att, Repository rep, string standard)
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
                rep.WriteOutput("System", "FEIL: datatype er ikke korrekt/funnet: " + att.Name + " " + att.Type, 0);
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
            if (sosi_navn == false) rep.WriteOutput("System", "FEIL: Mangler sosi_navn på egenskap: " + att.Name, 0);
            if (eg.SOSI_Navn.Length > 32) rep.WriteOutput("System", "FEIL: SOSI_navn er lengre enn 32 tegn på attributt " + att.Name, 0);
            return eg;
        }

        private static Basiselement LagEgenskapKjentType(string prikknivå, global::EA.Attribute att, Repository rep, string standard, KjentType kt)
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
            if (sosi_navn == false) rep.WriteOutput("System", "FEIL: Mangler sosi_navn på egenskap: " + att.Name, 0);

            return eg;
        }


        



        public List<SosiKodeliste> ByggSosiKodelister(Repository _rep)
        {
            List<SosiKodeliste> kList = new List<SosiKodeliste>();

            Package valgtPakke = _rep.GetTreeSelectedPackage();

           

            foreach (Element el in valgtPakke.Elements)
            {

                if (el.Stereotype.ToLower() == "codelist")
                {
                   
                    _rep.WriteOutput("System", "INFO: Funne kodeliste: " + el.Name, 0);
                    LagSosiKodeliste(_rep, el,kList);
                    
                }

            }
           
            HentKodelisterFraSubpakker(_rep, kList, valgtPakke);


            return kList;
        }

        private void HentKodelisterFraSubpakker(Repository _rep, List<SosiKodeliste> kList, Package valgtPakke)
        {
            foreach (Package pk in valgtPakke.Packages)
            {
                foreach (Element ele in pk.Elements)
                {
                    if (ele.Stereotype.ToLower() == "codelist")
                    {
                       
                        _rep.WriteOutput("System", "INFO: Funne kodeliste: " + ele.Name, 0);
                        LagSosiKodeliste(_rep, ele, kList);
                       
                    }
                }
                
                HentKodelisterFraSubpakker(_rep, kList, pk);
            }
        }

        public void LagSosiKodeliste(Repository _rep, Element e, List<SosiKodeliste> kList)
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
                _rep.WriteOutput("System", "INFO: kodeliste " + e.Name + " har egne SOSI verdier og inkluderes", 0);
            }


        }
        
        
    }
}
