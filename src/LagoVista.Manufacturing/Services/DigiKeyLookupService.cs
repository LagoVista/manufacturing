// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: eac68cdb9ccf5c2bf22dc326007aa9bd73784378ef1a87937b6be7aeb64bba10
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Interfaces;
using LagoVista.Manufacturing.Models;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using System.Linq;
using LagoVista.Manufacturing.Interfaces.Services;

namespace LagoVista.Manufacturing.Services
{
    public class DigiKeyLookupService : IDigiKeyLookupService
    {
        private readonly IManufacturingSettings _mfgSettings;

        public DigiKeyLookupService(IManufacturingSettings mfgSettings)
        {
            _mfgSettings = mfgSettings ?? throw new ArgumentNullException(nameof(mfgSettings));
        }

        public async Task<InvokeResult<Component>> ResolveProductInformation(Component component)
        {
            var client = new HttpClient();
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_id", _mfgSettings.DigiKeyClientId),
                new KeyValuePair<string, string>("client_secret", _mfgSettings.DigiKeyClientSecret)
            });

            var response = await client.PostAsync("https://api.digikey.com/v1/oauth2/token", content);
            if(!response.IsSuccessStatusCode)
                return InvokeResult<Component>.FromError("Could not authenticate with DigiKey");    

            var responseContent = await response.Content.ReadAsAsync<DigiKeyResponse>();         

            var partNumber = HttpUtility.UrlEncode(component.SupplierPartNumber);

            Console.WriteLine(partNumber);
            var requestUrl = $"https://api.digikey.com/products/v4/search/{partNumber}/productdetails";
            Console.WriteLine(requestUrl);

            client.DefaultRequestHeaders.Add("X-DIGIKEY-Client-Id", _mfgSettings.DigiKeyClientId);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(responseContent.TokenType, responseContent.AccessToken);

            var getResponse = await client.GetAsync(requestUrl);

            if (!getResponse.IsSuccessStatusCode)
            {
                var res = await getResponse.Content.ReadAsStringAsync();
                return InvokeResult<Component>.FromError($"Could not perform part number lookup: {getResponse.StatusCode} - {res}");
            }

            var json = await getResponse.Content.ReadAsAsync<DigiKeyProductInformation>();
            if (String.IsNullOrEmpty(component.DataSheet))
            {
                if(!json.Product.DatasheetUrl.StartsWith("https:"))
                    component.DataSheet = "https:" + json.Product.DatasheetUrl;
                else
                    component.DataSheet = json.Product.DatasheetUrl;
            }

            if (String.IsNullOrEmpty(component.VendorLink))
                component.VendorLink = json.Product.ProductUrl;

            foreach (var param in json.Product.Parameters)
            {
                if (!component.Attributes.Any(attr => attr.AttributeName == param.ParameterText))
                {
                    if(!String.IsNullOrEmpty(param.ValueText) && param.ValueText.Trim() != "-")
                        component.Attributes.Add(new ComponentAttribute() { 
                        AttributeName = param.ParameterText, 
                        AttributeValue = param.ValueText 
                    });
                }              
            }

            return InvokeResult<Component>.Create(component);
        }
    }

    public class DigiKeyResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }
    }

    public class SearchLocaleUsed
    {
        public string Site { get; set; }
        public string Language { get; set; }
        public string Currency { get; set; }
    }

    public class Description
    {
        public string ProductDescription { get; set; }
        public string DetailedDescription { get; set; }
    }

    public class Manufacturer
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class PackageType
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class StandardPricing
    {
        public int BreakQuantity { get; set; }
        public double UnitPrice { get; set; }
        public double TotalPrice { get; set; }
    }

    public class Supplier
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class ProductVariation
    {
        public string DigiKeyProductNumber { get; set; }
        public PackageType PackageType { get; set; }
        public IList<StandardPricing> StandardPricing { get; set; }
        public IList<object> MyPricing { get; set; }
        public bool MarketPlace { get; set; }
        public bool TariffActive { get; set; }
        public Supplier Supplier { get; set; }
        public int QuantityAvailableforPackageType { get; set; }
        public int MaxQuantityForDistribution { get; set; }
        public int MinimumOrderQuantity { get; set; }
        public int StandardPackage { get; set; }
        public double DigiReelFee { get; set; }
    }

    public class ProductStatus
    {
        public int Id { get; set; }
        public string Status { get; set; }
    }

    public class Parameter
    {
        public int ParameterId { get; set; }
        public string ParameterText { get; set; }
        public string ParameterType { get; set; }
        public string ValueId { get; set; }
        public string ValueText { get; set; }
    }

    public class ChildCategory
    {
        public int CategoryId { get; set; }
        public int ParentId { get; set; }
        public string Name { get; set; }
        public int ProductCount { get; set; }
        public int NewProductCount { get; set; }
        public string ImageUrl { get; set; }
        public string SeoDescription { get; set; }
        public IList<object> ChildCategories { get; set; }
    }

    public class Category
    {
        public int CategoryId { get; set; }
        public int ParentId { get; set; }
        public string Name { get; set; }
        public int ProductCount { get; set; }
        public int NewProductCount { get; set; }
        public string ImageUrl { get; set; }
        public string SeoDescription { get; set; }
        public IList<ChildCategory> ChildCategories { get; set; }
    }

    public class Series
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class Classifications
    {
        public string ReachStatus { get; set; }
        public string RohsStatus { get; set; }
        public string MoistureSensitivityLevel { get; set; }
        public string ExportControlClassNumber { get; set; }
        public string HtsusCode { get; set; }
    }

    public class Product
    {
        public Description Description { get; set; }
        public Manufacturer Manufacturer { get; set; }
        public string ManufacturerProductNumber { get; set; }
        public double UnitPrice { get; set; }
        public string ProductUrl { get; set; }
        public string DatasheetUrl { get; set; }
        public string PhotoUrl { get; set; }
        public IList<ProductVariation> ProductVariations { get; set; }
        public int QuantityAvailable { get; set; }
        public ProductStatus ProductStatus { get; set; }
        public bool BackOrderNotAllowed { get; set; }
        public bool NormallyStocking { get; set; }
        public bool Discontinued { get; set; }
        public bool EndOfLife { get; set; }
        public bool Ncnr { get; set; }
        public object PrimaryVideoUrl { get; set; }
        public IList<Parameter> Parameters { get; set; }
        public object BaseProductNumber { get; set; }
        public Category Category { get; set; }
        public object DateLastBuyChance { get; set; }
        public string ManufacturerLeadWeeks { get; set; }
        public int ManufacturerPublicQuantity { get; set; }
        public Series Series { get; set; }
        public object ShippingInfo { get; set; }
        public Classifications Classifications { get; set; }
    }

    public class DigiKeyProductInformation
    {
        public SearchLocaleUsed SearchLocaleUsed { get; set; }
        public Product Product { get; set; }
    }

}
