// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: f9597e0452c4993b96795b906c083aec1b5b4f1512a02b9a0a28a539b8bb29a8
// IndexVersion: 2
// --- END CODE INDEX META ---
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
