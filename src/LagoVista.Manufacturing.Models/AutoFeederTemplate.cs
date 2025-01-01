using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.Manufacturing.Models
{
    public class AutoFeederTemplate : MfgModelBase, ISummaryFactory
    {

        public AutoFeederTemplateSummary CreateSummary()
        {
            return new AutoFeederTemplateSummary()
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

    public class AutoFeederTemplateSummary : SummaryData
    {
    }
}
