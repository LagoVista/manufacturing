using LagoVista.Manufacturing.Interfaces;
using LagoVista.Manufacturing.Models;
using LagoVista.Manufacturing.Services;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text.Encodings.Web;
using System.Web;

namespace Mfg.ServicesTests
{
    public class Tests
    {
        DigiKeyLookupService _digiKeyLookup;

        [SetUp]
        public void Setup()
        {
            var mfgSettings = new MfgSettings()
            {
                DigiKeyClientId = Environment.GetEnvironmentVariable("NUVIOT_DIGIKEY_CLIENTID", EnvironmentVariableTarget.User),
                DigiKeyClientSecret = Environment.GetEnvironmentVariable("NUVIOT_DIGIKEY_CLIENTSECRET", EnvironmentVariableTarget.User)
            };

            _digiKeyLookup = new DigiKeyLookupService(mfgSettings);
        }

        [Test]
        public async Task Test1Async()
        {
            var component = new Component()
            {
                SupplierPartNumber = "1276-1066-2-ND"
            };

            var result = await _digiKeyLookup.ResolveProductInformation(component);

            Console.WriteLine(component.VendorLink);
            Console.WriteLine(component.DataSheet);
            foreach (var prt in component.Attributes)
                Console.WriteLine(prt.AttributeName + " : " + prt.AttributeValue);
        }
    }

    public class MfgSettings : IManufacturingSettings
    {
        public string DigiKeyClientId { get; set; }

        public string DigiKeyClientSecret { get; set; }
    }
}