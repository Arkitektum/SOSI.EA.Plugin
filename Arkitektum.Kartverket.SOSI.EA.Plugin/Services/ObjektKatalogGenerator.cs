using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;
using Arkitektum.Kartverket.SOSI.Model;

namespace Arkitektum.Kartverket.SOSI.EA.Plugin.Services
{
    public class ObjektKatalogGenerator
    {

        readonly XNamespace _gco = "http://www.isotc211.org/2005/gco";
        readonly XNamespace _gfc = "http://www.isotc211.org/2005/gfc/1.1";
        readonly XNamespace _gmd = "http://www.isotc211.org/2005/gmd";
        readonly XNamespace _gmx = "http://www.isotc211.org/2005/gmx";
        readonly XNamespace _xlink = "http://www.w3.org/1999/xlink";
        readonly XNamespace _xsi = "http://www.w3.org/2001/XMLSchema-instance";


        /// <summary>
        /// Lage XML fil ihht ISO 19110 revidert versjon
        /// 
        /// http://www.kartverket.no/Standarder/SOSI/SOSI-standarden-del-1-og-2/
        /// </summary>
        /// <param name="navn"></param>
        /// <param name="beskrivelse"></param>
        /// <param name="objekttyper"></param>
        /// <param name="erFag"></param>
        /// <param name="kodelister"></param>
        public XDocument LagObjektKatalog(string versjon, string org, string pers, string navn, string beskrivelse, List<Objekttype> objekttyper, bool erFag, List<SosiKodeliste> kodelister)
        {
            var doc = new XDocument(
                new XElement(_gfc + "FC_FeatureCatalogue", 
                    new XAttribute("id", "FC"),
                    SetupNamespaces(),
                    BuildBasicInfo(versjon, org, pers, navn, beskrivelse),
                    BuildFeatureTypes(objekttyper)
                )
            );
            return doc;
        }
        
        private object[] SetupNamespaces()
        {
            object[] namespaces = new object[] 
                {
                    new XAttribute(XNamespace.Xmlns + "gco", _gco),
                    new XAttribute(XNamespace.Xmlns + "gfc", _gfc),
                    new XAttribute(XNamespace.Xmlns + "gmd", _gmd),
                    new XAttribute(XNamespace.Xmlns + "gmx", _gmx),
                    new XAttribute(XNamespace.Xmlns + "xlink", _xlink),
                    new XAttribute(XNamespace.Xmlns + "xsi", _xsi),
                    new XAttribute(_xsi + "schemaLocation", "http://www.isotc211.org/2005/gco http://standards.iso.org/ittf/PubliclyAvailableStandards/ISO_19139_Schemas/gco/gco.xsd http://www.isotc211.org/2005/gmd http://standards.iso.org/ittf/PubliclyAvailableStandards/ISO_19139_Schemas/gmd/gmd.xsd http://www.isotc211.org/2005/gmx http://standards.iso.org/ittf/PubliclyAvailableStandards/ISO_19139_Schemas/gmx/gmx.xsd http://www.isotc211.org/2005/gfc/1.1 ../../gfc/gfc.xsd")
                };
            return namespaces;
        }

        private object[] BuildBasicInfo(string versjon, string org, string pers, string navn, string beskrivelse)
        {
            object[] output = new object[]
                {
                    new XElement(_gmx + "name",
                        new XElement(_gco + "CharacterString", navn)
                    ),
                    new XElement(_gmx + "scope",
                        new XElement(_gco + "CharacterString", beskrivelse)
                    ),
                    new XElement(_gmx + "fieldOfApplication",
                        new XElement(_gco + "CharacterString", "")
                    ),
                    new XElement(_gmx + "versionNumber",
                        new XElement(_gco + "CharacterString", versjon)
                    ),
                    new XElement(_gmx + "versionDate",
                        new XElement(_gco + "Date", DateTime.Now.Year + "-" + DateTime.Now.Month + "-" + DateTime.Now.Day)
                    ),
                    new XElement(_gmx + "language",
                        new XElement(_gmd + "LanguageCode", "nor") // TODO
                    ),
                    new XElement(_gmx + "characterSet",
                        new XElement(_gmd + "MD_CharacterSetCode", "UTF-8")
                    ),
                    new XElement(_gmx + "locale"), // TODO
                    new XElement(_gfc + "producer",
                        new XElement(_gmd + "CI_ResponsibleParty",
                            new XElement(_gmd + "individualName",new XElement(_gco + "CharacterString", pers)), 
                            new XElement(_gmd + "organisationName", new XElement(_gco + "CharacterString", org)), 
                            new XElement(_gmd + "positionName", new XElement(_gco + "CharacterString", "")) ) // TODO
                    )
                };
            return output;
        }

        private object[] BuildFeatureTypes(List<Objekttype> objekttyper)
        {
            var output = new List<XElement>();

            for (int i = 0; i < objekttyper.Count; i++)
            {
                var objekttype = objekttyper[i];
                var element = new XElement(_gfc + "featureType",
                        new XElement(_gfc + "FC_FeatureType",
                            new XAttribute("id", "FT" + (i+1)),
                            new XElement(_gfc + "typeName",
                                new XElement(_gco + "LocalName", objekttype.UML_Navn)
                            ),
                            new XElement(_gfc + "definition",
                                new XElement(_gco + "CharacterString", objekttype.Notat)
                            ),
                            new XElement(_gfc + "isAbstract",
                                new XElement(_gco + "Boolean", "false")
                            ),
                            new XElement(_gfc + "featureCatalogue",
                                new XAttribute(_xlink + "href", "#FC")
                            ),
                            BuildFeatureTypeCharacteristics(objekttype.Egenskaper),
                            BuildconstrainedBy(objekttype)
                        )
                    );
                output.Add(element);
            }
            return output.ToArray();
        }

        private List<XElement> BuildconstrainedBy(Objekttype objekttype)
        {
            List<XElement> output = new List<XElement>();
            if (objekttype.Avgrenser.Count > 0) {
                var element = new XElement(_gfc + "constrainedBy",
                        new XElement(_gfc + "FC_Constraint",
                            new XElement(_gfc + "description", new XElement(_gco + "CharacterString", "Avgrenser:" + String.Join(",", objekttype.Avgrenser.ToArray(), 0, objekttype.Avgrenser.Count)))
                            )
                    );
                output.Add(element);
            }
            if (objekttype.AvgrensesAv.Count > 0)
            {
                var element = new XElement(_gfc + "constrainedBy",
                        new XElement(_gfc + "FC_Constraint",
                            new XElement(_gfc + "description", new XElement(_gco + "CharacterString", "Avgrenses av:" + String.Join(",", objekttype.AvgrensesAv.ToArray(), 0, objekttype.AvgrensesAv.Count)))
                            )
                    );
                output.Add(element);
            }
            
            foreach (Beskrankning beskr in objekttype.OCLconstraints)
            {

                var element = new XElement(_gfc + "constrainedBy",
                        new XElement(_gfc + "FC_Constraint",
                            new XElement(_gfc + "description", new XElement(_gco + "CharacterString", beskr.Navn + ":" + beskr.Notat))
                            )
                    );
                output.Add(element);
            }


            return output;
        }

        private object[] BuildFeatureTypeCharacteristics(List<AbstraktEgenskap> egenskaper)
        {
            var output = new List<XElement>();
            if (egenskaper != null)
            {
                foreach (var egenskap in egenskaper)
                {
                    //<gco:UnlimitedInteger xsi:nil="true" isInfinite="true"/>
                    string datatype = "";
                    if (egenskap is Basiselement)
                    {
                        
                        datatype = ((Basiselement)egenskap).Datatype;
                    }
                    else datatype = egenskap.UML_Navn;
                    var element = new XElement(_gfc + "carrierOfCharacteristics",
                            new XElement(_gfc + "FC_FeatureAttribute",
                                new XElement(_gfc + "memberName", new XElement(_gco + "LocalName", egenskap.SOSI_Navn))
                                , new XElement(_gfc + "cardinality", new XElement(_gco + "Multiplicity", new XElement(_gco + "range", new XElement(_gco + "MultiplicityRange", new XElement(_gco + "lower", new XElement(_gco + "Integer", "1")),new XElement(_gco + "upper", new XElement(_gco + "UnlimitedInteger", "1") )))))
                                , new XElement(_gfc + "valueType", new XElement(_gco + "TypeName", new XElement(_gco + "aName", new XElement(_gco + "CharacterString", datatype))))
                                , new XElement(_gfc + "definition", new XElement(_gco + "CharacterString", egenskap.UML_Navn + ": " + egenskap.Notat))
                                )
                        );
                    output.Add(element);
                }
            }

            return output.ToArray();
        }

        public XDocument CreateHtmlCatalogForXmlDocument(XDocument doc)
        {
            XDocument htmlDoc = new XDocument();
            using (XmlWriter writer = htmlDoc.CreateWriter())
            {
                using (Stream stream = GetType().Assembly.GetManifestResourceStream("Arkitektum.Kartverket.SOSI.EA.Plugin.xsl.html.xsl"))
                {
                    using (XmlReader reader = XmlReader.Create(stream))
                    {
                        XslCompiledTransform xslt = new XslCompiledTransform();
                        xslt.Load(reader);
                        xslt.Transform(doc.CreateReader(), writer);
                    }
                }
            }
            return htmlDoc;
        }
    }
}
