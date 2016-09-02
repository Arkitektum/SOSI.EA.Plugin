using EA;
using System.Collections.Generic;
using System;

namespace arkitektum.gistools.generators.Validator
{
    public class AssociationHasRequiredRoleNames : BaseValidator
    {
        public static readonly string MessageTemplate =
            "Assosiasjonen mellom [{0}] og [{1}] mangler angivelse av rollenavn.";

        private readonly List<int> _validatedConnectorsIDs;

        public override string GetFriendlyName()
        {
            return "Rollenavn på assosiasjoner - https://arkitektum.atlassian.net/wiki/pages/viewpage.action?pageId=26017838";
        }

        public override string GetRuleUrl()
        {
            return "https://arkitektum.atlassian.net/wiki/pages/viewpage.action?pageId=26017838";
        }

        public override string GetRuleId()
        {
            return "V1002";
        }

        public AssociationHasRequiredRoleNames(Repository repository, ValidatorSettings settings) : base(repository, settings)
        {
            _validatedConnectorsIDs = new List<int>();
        }

        public override void RunValidation(Element element)
        {
            foreach (Connector connector in element.Connectors)
            {
                if (ConnectorIsValidated(connector))
                    continue;

                if (RoleNameIsRequired(connector) && RoleNameIsMissing(connector))
                    AddError(connector);

                _validatedConnectorsIDs.Add(connector.ConnectorID);
            }
        }

        private bool RoleNameIsRequired(Connector connector)
        {
            return (IsAssociationConnector(connector) || IsAggregationConnector(connector)) && !IsTopo(connector);
        }

        

   
        private bool ConnectorIsValidated(Connector connector)
        {
            return _validatedConnectorsIDs.Contains(connector.ConnectorID);
        }

        private bool RoleNameIsMissing(Connector connector)
        {
            return RequiredRoleNameIsMissing(connector.ClientEnd) || RequiredRoleNameIsMissing(connector.SupplierEnd);
        }

        private bool RequiredRoleNameIsMissing(ConnectorEnd connectorEnd)
        {
            return (connectorEnd.IsNavigable && string.IsNullOrEmpty(connectorEnd.Role));
        }

        private void AddError(Connector connector)
        {
            string clientElementName = Repository.GetElementByID(connector.ClientID).Name;
            string supplierElementName = Repository.GetElementByID(connector.SupplierID).Name;

            string message = string.Format(MessageTemplate, clientElementName, supplierElementName);

            AddError(message, connector.ConnectorID);


        }
    }
}
