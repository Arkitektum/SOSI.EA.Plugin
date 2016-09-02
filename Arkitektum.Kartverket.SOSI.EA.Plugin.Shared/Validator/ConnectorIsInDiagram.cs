using System.Collections.Generic;
using EA;

namespace arkitektum.gistools.generators.Validator
{
    public class ConnectorIsInDiagram : BaseValidator
    {
        public static string MessageTemplate = "Assosiasjonen[{2}] mellom [{0}] og [{1}] vises ikke i noe diagram.";

        private readonly List<int> _connectorIdsWithError;

        public override string GetRuleUrl()
        {
            return "https://arkitektum.atlassian.net/wiki/display/gistools/Elementer+som+ikke+vises+i+diagram";
        }

        public override string GetRuleId()
        {
            return "V1019";
        }

        public ConnectorIsInDiagram(Repository repository, ValidatorSettings settings) : base(repository, settings)
        {
            _connectorIdsWithError = new List<int>();
        }

        public override void RunValidation(Element element)
        {
            foreach (Connector connector in element.Connectors)
            {
                var connectorId = connector.ConnectorID;
                string result = Repository.SQLQuery("SELECT DiagramID FROM t_diagramlinks where Hidden=false AND ConnectorID=" + connectorId);
                if (!result.Contains("DiagramID") && !ConnectorIsValidated(connector) && !IsNoteLinkConnector(connector))
                {
                    Element client = Repository.GetElementByID(connector.ClientID);
                    Element supplier = Repository.GetElementByID(connector.SupplierID);
                    AddWarning(string.Format(MessageTemplate, client.Name, supplier.Name,connector.MetaType), connectorId);
                    _connectorIdsWithError.Add(connectorId);
                }
            }
        }

        private bool ConnectorIsValidated(Connector connector)
        {
            return _connectorIdsWithError.Contains(connector.ConnectorID);
        }
        public override string GetFriendlyName()
        {
            return "Assosiasjoner bør vises i et diagram. - https://arkitektum.atlassian.net/wiki/display/gistools/Elementer+som+ikke+vises+i+diagram";
        }
    }
}
