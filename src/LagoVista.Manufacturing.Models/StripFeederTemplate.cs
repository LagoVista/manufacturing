using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.Manufacturing.Models
{
    public class StripFeederTemplate : MfgModelBase, ISummaryFactory
    {
        public StripFeederTemplateSummary CreateSummary()
        {
            return new StripFeederTemplateSummary()
            {
                Id = Id,
                Key = Key,
                Name = Name,
                Description = Description,
                IsPublic = IsPublic,
            };
        }

        ISummaryData ISummaryFactory.CreateSummary()
        {
            return CreateSummary();
        }
    }

    public class StripFeederTemplateSummary : SummaryData
    {
    }
}
