using EA;
using System.Collections.Generic;
using Arkitektum.EA.Tools;
using System.Text;

namespace arkitektum.gistools.generators.Validator
{
    public class AssociationHasRequiredSosiNames : BaseValidator
    {
        public static readonly string MessageTemplate =
            "Assosiasjonen mellom [{0}] og [{1}] mangler tagged value [SOSI_navn] for [{2}]";

        private readonly List<int> _validatedConnectorsIDs;


        public override string GetRuleUrl()
        {
            return "https://arkitektum.atlassian.net/wiki/pages/viewpage.action?pageId=26836994";
        }

        public override string GetRuleId()
        {
            return "V1003";
        }

        public AssociationHasRequiredSosiNames(Repository repository, ValidatorSettings settings) : base(repository, settings)
        {
            _validatedConnectorsIDs = new List<int>();
        }

        public override void RunValidation(Element element)
        {
            if (!ValidateForSosi()) return;

            foreach (Connector connector in element.Connectors)
            {
                if (ConnectorIsValidated(connector))
                    continue;

                if (SosiNameIsRequired(connector) && SosiNameIsMissing(connector))
                    AddError(connector);

                _validatedConnectorsIDs.Add(connector.ConnectorID);
            }
        }

        private bool SosiNameIsRequired(Connector connector)
        {
            return (IsAssociationConnector(connector) || IsAggregationConnector(connector)) && !IsTopo(connector);
        }

        private bool ConnectorIsValidated(Connector connector)
        {
            return _validatedConnectorsIDs.Contains(connector.ConnectorID);
        }

        private bool SosiNameIsMissing(Connector connector)
        {
            return RequiredSosiNameIsMissing(connector.ClientEnd) || RequiredSosiNameIsMissing(connector.SupplierEnd);
        }

        private bool RequiredSosiNameIsMissing(ConnectorEnd connectorEnd)
        {
            return connectorEnd.IsNavigable && string.IsNullOrEmpty(RepositoryUtil.GetTaggedValueFromConnectorEnd(connectorEnd, "SOSI_navn"));
        }

        private void AddError(Connector connector)
        {
            string clientElementName = Repository.GetElementByID(connector.ClientID).Name;
            string supplierElementName = Repository.GetElementByID(connector.SupplierID).Name;

            string message = string.Format(MessageTemplate, clientElementName, supplierElementName, GetImplicatedRoles(connector));

            AddError(message, connector.ConnectorID);
        }

        private string GetImplicatedRoles(Connector connector)
        {
            var implicatedRoles = new StringBuilder();

            if (RequiredSosiNameIsMissing(connector.ClientEnd))
                implicatedRoles.Append(connector.ClientEnd.Role);

            if (RequiredSosiNameIsMissing(connector.SupplierEnd))
            {
                if (implicatedRoles.Length > 0)
                    implicatedRoles.Append(" og ");

                implicatedRoles.Append(connector.SupplierEnd.Role);
            }

            return implicatedRoles.ToString();
        }

        public override string GetFriendlyName()
        {
            string friendlyName = "SOSI-navn på assosiasjoner - https://arkitektum.atlassian.net/wiki/pages/viewpage.action?pageId=26836994";

            if (!ValidateForSosi())
                friendlyName += " (Deaktivert)";

            return friendlyName;
        }
    }
}
