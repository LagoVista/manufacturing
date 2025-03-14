using LagoVista.Manufacturing.Interfaces;
using LagoVista.Manufacturing.Models;
using LagoVista.Manufacturing.Services;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
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

        [Test]
        public void OrderParseTest()
        {
            var orderCSV = @"1,541-4131-1-ND,CRCW08054K70FKEAC,Vishay Dale,RES 4.7K OHM 1% 1/8W 0805,,100,0,$0.01710,$1.71
2,A129741CT-ND,CRGCQ0805F220R,TE Connectivity Passive Product,RES 220 OHM 1% 1/8W 0805,,100,0,$0.01830,$1.83
3,399-C0805C220J5GACTUCT-ND,C0805C220J5GACTU,KEMET,CAP CER 22PF 50V C0G/NP0 0805,,100,0,$0.02360,$2.36";
            var lines = orderCSV.Split('\n'); 
            var skippedHeader = false;
            foreach (var line in lines)
            {
                if (!skippedHeader)
                    skippedHeader = true;
                else
                {
                    var trimmed = line.Trim();
                    if (!String.IsNullOrEmpty(trimmed))
                    {
                        var parser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
                        var parts = parser.Split(trimmed);
                        var lineItem = ComponentOrderLineItem.FromOrderLine(parts);
                    }

                    Console.WriteLine("----");
                }
            }
        }
    }

    public class MfgSettings : IManufacturingSettings
    {
        public string DigiKeyClientId { get; set; }

        public string DigiKeyClientSecret { get; set; }
    }


}