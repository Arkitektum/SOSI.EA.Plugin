using System;
using EA;
using Attribute = EA.Attribute;

namespace Arkitektum.Kartverket.SOSI.Model
{
    public class RepositoryHelper
    {
        private readonly Repository _repository;
        public const string SosiNavn = "sosi_navn";
        public const string SosiDatatype = "sosi_datatype";
        public const string SosiLengde = "sosi_lengde";

        public RepositoryHelper(Repository repository)
        {
            _repository = repository;
        }



        /// <summary>
        /// Returns sosi name from tagged values. If not defined it looks for sosi name in classifier element.
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="repository"></param>
        /// <returns></returns>
        public string GetSosiNameForAttribute(Attribute attribute)
        {
            string sosiName = GetTaggedValue(attribute.TaggedValues, SosiNavn);

            if (string.IsNullOrEmpty(sosiName) && attribute.ClassifierID != 0)
            {
                Element elm = _repository.GetElementByID(attribute.ClassifierID);
                sosiName = GetTaggedValue(elm.TaggedValues, SosiNavn);
            }

            

            return sosiName;
        }

        public static string GetTaggedValue(Collection taggedValues, string searchString)
        {
            string result = null;
            foreach (dynamic tag in taggedValues)
            {
                string tagName = (string)tag.Name;
                if (string.Equals(tagName, searchString, StringComparison.OrdinalIgnoreCase))
                {
                    result = (string)tag.Value;
                    break;
                }
            }
            return result;
        }
        
        public void Log(string message)
        {
            _repository.WriteOutput("System", message, 0);
        }
    }
}
