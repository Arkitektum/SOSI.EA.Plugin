using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using EA;
using Attribute = EA.Attribute;

namespace Arkitektum.EA.Tools
{
    public static class geoUtil
    {
        public static bool IsNCName(string name)
        {
            bool isOk = false;
            try
            {
                string tmp = XmlConvert.VerifyNCName(name);
                isOk = true;
            }
            catch { }
            return isOk;
        }

        public static void SaveTaggedValueOnElement(global::EA.Element elm, string taggedValueName, string taggedValue)
        {
            string searchString = taggedValueName;
            bool found = false;
            foreach (dynamic tag in elm.TaggedValues)
            {
                string tagName = (string)tag.Name;
                if (tagName == searchString)
                {
                    tag.Value = taggedValue;
                    tag.Update();
                    found = true;
                    break;
                }
            }
            if (found == false)
            {
                global::EA.TaggedValue tv1 = elm.TaggedValues.AddNew(taggedValueName, "");
                tv1.Value = taggedValue;
                tv1.Update();
            }
            
        }

        public static void SaveTaggedValueOnConnectorEnd(global::EA.ConnectorEnd elm, string taggedValueName, string taggedValue)
        {
            string searchString = taggedValueName;
            bool found = false;
            foreach (dynamic tag in elm.TaggedValues)
            {
                string tagName = (string)tag.Tag;
                if (tagName == searchString)
                {
                    tag.Value = taggedValue;
                    tag.Update();
                    found = true;
                    break;
                }
            }
            if (found == false)
            {
                var tv1 = elm.TaggedValues.AddNew(taggedValueName, "");
                tv1.Value = taggedValue;
                tv1.Update();
            }

        }

        public static void SaveTaggedValueOnAttribute(global::EA.Attribute elm, string taggedValueName, string taggedValue)
        {
            string searchString = taggedValueName;
            bool found = false;
            foreach (dynamic tag in elm.TaggedValues)
            {
                string tagName = (string)tag.Name;
                if (tagName == searchString)
                {
                    tag.Value = taggedValue;
                    tag.Update();
                    found = true;
                    break;
                }
            }
            if (found == false)
            {
                var tv1 = elm.TaggedValues.AddNew(taggedValueName, "");
                tv1.Value = taggedValue;
                tv1.Update();
            }

        }

        public static string GetTaggedValueFromElement(global::EA.Element attribute, string taggedValueName)
        {
            string searchString = taggedValueName;
            string result = null;
            foreach (dynamic tag in attribute.TaggedValues)
            {
                string tagName = (string)tag.Name;
                if (tagName == searchString)
                {
                    result = (string)tag.Value;
                    break;
                }
            }
            return result;
        }

        public static string GetTaggedValueFromAttribute(global::EA.Attribute attribute, string taggedValueName)
        {
            string searchString = taggedValueName.ToLower();
            string result = null;
            foreach (dynamic tag in attribute.TaggedValues)
            {
                string tagName = (string)tag.Name;
                if (tagName.ToLower() == searchString)
                {
                    result = (string)tag.Value;
                    break;
                }
            }
            return result;
        }

        public static string GetTaggedValueFromConnectorEnd(global::EA.ConnectorEnd connectorEnd, string name)
        {
            string searchString = name.ToLower();
            string result = null;
            foreach (dynamic tag in connectorEnd.TaggedValues)
            {
                string tagName = (string)tag.Tag;
                if (tagName.ToLower() == searchString)
                {
                    result = (string)tag.Value;
                    break;
                }
            }
            return result;
        }

        public static string GetTaggedValue(Collection taggedValues, string name)
        {
            string searchString = name.ToLower();
            string result = null;
            foreach (dynamic tag in taggedValues)
            {
                string tagName = (string)tag.Name;
                if (tagName.ToLower() == searchString)
                {
                    result = (string)tag.Value;
                    break;
                }
            }
            return result;
        }
        /// <summary>
        /// Bad behaviour! returns attribute name if sosi name is not found! - Use GetSosiNameOnly for more logical behaviour.
        /// </summary>
        /// <param name="att"></param>
        /// <param name="repository"></param>
        /// <returns></returns>
        public static string HentSOSI_Navn(global::EA.Attribute att, global::EA.Repository repository)
        {
            string ret = att.Name;
            foreach (object theTags2 in att.TaggedValues)
            {
                string navn = (string)((dynamic)theTags2).Name;
                switch (navn.ToLower())
                {
                    case "sosi_navn":
                        ret = (string)((dynamic)theTags2).Value;
                        break;
                }
            }
            //Eller hent typenavn
            if (ret.Length == 0 && att.ClassifierID != 0)
            {
                global::EA.Element elm = repository.GetElementByID(att.ClassifierID);
                foreach (object theTags2 in elm.TaggedValues)
                {
                    string navn = (string)((dynamic)theTags2).Name;
                    switch (navn.ToLower())
                    {
                        case "sosi_navn":
                            ret = (string)((dynamic)theTags2).Value;
                            break;
                    }
                }

            }

            return Æøåfix(ret);
        }
        /// <summary>
        /// Returns sosi name from tagged values. If not defined it looks for sosi name in classifier element.
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="repository"></param>
        /// <returns></returns>
        public static string GetSosiNameOnly(Attribute attribute, Repository repository)
        {
            string sosiName = GetTaggedValue(attribute.TaggedValues, "sosi_navn");

            if (string.IsNullOrEmpty(sosiName) && attribute.ClassifierID != 0)
            {
                Element elm = repository.GetElementByID(attribute.ClassifierID);
                sosiName = GetTaggedValue(elm.TaggedValues, "sosi_navn");
            }

            return sosiName;
        }



        public static string Utf8fix(string tekst)
        {
        //Æ = &#198;
        //Ø = &#216;
        //Å = &#197;
        //æ = &#230;
        //ø = &#248;
        //å = &#229;
        //og spesialtegn slik som - til _
            tekst = tekst.Replace("æ", "&#230;");
            tekst = tekst.Replace("ø", "&#248;");
            tekst = tekst.Replace("å", "&#229;");
            tekst = tekst.Replace("Æ", "&#198;");
            tekst = tekst.Replace("Ø", "&#216;");
            tekst = tekst.Replace("Å", "&#197;");
            
            return tekst;
        }

        public static string Æøåfix(string tekst)
        {
            tekst = tekst.Replace("æ", "ae");
            tekst = tekst.Replace("ø", "oe");
            tekst = tekst.Replace("å", "aa");
            tekst = tekst.Replace("Æ", "AE");
            tekst = tekst.Replace("Ø", "OE");
            tekst = tekst.Replace("Å", "AA");
            tekst = tekst.Replace("-", "_");
            tekst = tekst.Replace(" ", "_");
            tekst = tekst.Replace(".", "_");
            return tekst;
        }

        public static bool IsMultippel(string p)
        {
            bool ret = false;
            if (p.Contains("*")) ret = true;
            return ret;
        }
    }
}
