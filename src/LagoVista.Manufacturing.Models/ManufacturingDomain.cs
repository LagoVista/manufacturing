using LagoVista.Core.Attributes;
using LagoVista.Core.Models.UIMetaData;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.Manufacturing.Models
{

    [DomainDescriptor]
    public class ManufacutringDomain
    {
        public const string Manufacturing = "Manufacturing";
        [DomainDescription(Manufacturing)]
        public static DomainDescription ManufacturingDomainDescription
        {
            get
            {
                return new DomainDescription()
                {
                    Description = "A set of classes that can be used to help manage manufacturing of prodcuts.",
                    DomainType = DomainDescription.DomainTypes.BusinessObject,
                    Name = "Manufacturing",
                    CurrentVersion = new Core.Models.VersionInfo()
                    {
                        Major = 0,
                        Minor = 8,
                        Build = 001,
                        DateStamp = new DateTime(2016, 12, 20),
                        Revision = 1,
                        ReleaseNotes = ""
                    }
                };
            }
        }
    }
}
