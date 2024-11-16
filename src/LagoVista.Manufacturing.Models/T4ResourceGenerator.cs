/*11/16/2024 4:21:15 PM*/
using System.Globalization;
using System.Reflection;

//Resources:ManufacturingResources:Common_Category
namespace LagoVista.Manufacturing.Models.Resources
{
	public class ManufacturingResources
	{
        private static global::System.Resources.ResourceManager _resourceManager;
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        private static global::System.Resources.ResourceManager ResourceManager 
		{
            get 
			{
                if (object.ReferenceEquals(_resourceManager, null)) 
				{
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("LagoVista.Manufacturing.Models.Resources.ManufacturingResources", typeof(ManufacturingResources).GetTypeInfo().Assembly);
                    _resourceManager = temp;
                }
                return _resourceManager;
            }
        }
        
        /// <summary>
        ///   Returns the formatted resource string.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        private static string GetResourceString(string key, params string[] tokens)
		{
			var culture = CultureInfo.CurrentCulture;;
            var str = ResourceManager.GetString(key, culture);

			for(int i = 0; i < tokens.Length; i += 2)
				str = str.Replace(tokens[i], tokens[i+1]);
										
            return str;
        }
        
        /// <summary>
        ///   Returns the formatted resource string.
        /// </summary>
		/*
        [global::System.FeederModel.EditorBrowsableAttribute(global::System.FeederModel.EditorBrowsableState.Advanced)]
        private static HtmlString GetResourceHtmlString(string key, params string[] tokens)
		{
			var str = GetResourceString(key, tokens);
							
			if(str.StartsWith("HTML:"))
				str = str.Substring(5);

			return new HtmlString(str);
        }*/
		
		public static string Common_Category { get { return GetResourceString("Common_Category"); } }
//Resources:ManufacturingResources:Common_CreatedBy

		public static string Common_CreatedBy { get { return GetResourceString("Common_CreatedBy"); } }
//Resources:ManufacturingResources:Common_CreationDate

		public static string Common_CreationDate { get { return GetResourceString("Common_CreationDate"); } }
//Resources:ManufacturingResources:Common_Description

		public static string Common_Description { get { return GetResourceString("Common_Description"); } }
//Resources:ManufacturingResources:Common_Icon

		public static string Common_Icon { get { return GetResourceString("Common_Icon"); } }
//Resources:ManufacturingResources:Common_IsPublic

		public static string Common_IsPublic { get { return GetResourceString("Common_IsPublic"); } }
//Resources:ManufacturingResources:Common_IsRequired

		public static string Common_IsRequired { get { return GetResourceString("Common_IsRequired"); } }
//Resources:ManufacturingResources:Common_IsValid

		public static string Common_IsValid { get { return GetResourceString("Common_IsValid"); } }
//Resources:ManufacturingResources:Common_Key

		public static string Common_Key { get { return GetResourceString("Common_Key"); } }
//Resources:ManufacturingResources:Common_Key_Help

		public static string Common_Key_Help { get { return GetResourceString("Common_Key_Help"); } }
//Resources:ManufacturingResources:Common_Key_Validation

		public static string Common_Key_Validation { get { return GetResourceString("Common_Key_Validation"); } }
//Resources:ManufacturingResources:Common_LastUpdated

		public static string Common_LastUpdated { get { return GetResourceString("Common_LastUpdated"); } }
//Resources:ManufacturingResources:Common_LastUpdatedBy

		public static string Common_LastUpdatedBy { get { return GetResourceString("Common_LastUpdatedBy"); } }
//Resources:ManufacturingResources:Common_Name

		public static string Common_Name { get { return GetResourceString("Common_Name"); } }
//Resources:ManufacturingResources:Common_Note

		public static string Common_Note { get { return GetResourceString("Common_Note"); } }
//Resources:ManufacturingResources:Common_Notes

		public static string Common_Notes { get { return GetResourceString("Common_Notes"); } }
//Resources:ManufacturingResources:Common_PageNumberOne

		public static string Common_PageNumberOne { get { return GetResourceString("Common_PageNumberOne"); } }
//Resources:ManufacturingResources:Common_Resources

		public static string Common_Resources { get { return GetResourceString("Common_Resources"); } }
//Resources:ManufacturingResources:Common_SelectCategory

		public static string Common_SelectCategory { get { return GetResourceString("Common_SelectCategory"); } }
//Resources:ManufacturingResources:Common_UniqueId

		public static string Common_UniqueId { get { return GetResourceString("Common_UniqueId"); } }
//Resources:ManufacturingResources:Common_ValidationErrors

		public static string Common_ValidationErrors { get { return GetResourceString("Common_ValidationErrors"); } }
//Resources:ManufacturingResources:CompomentPurchase_Title

		public static string CompomentPurchase_Title { get { return GetResourceString("CompomentPurchase_Title"); } }
//Resources:ManufacturingResources:Component_Attr1

		public static string Component_Attr1 { get { return GetResourceString("Component_Attr1"); } }
//Resources:ManufacturingResources:Component_Attr2

		public static string Component_Attr2 { get { return GetResourceString("Component_Attr2"); } }
//Resources:ManufacturingResources:Component_Bin

		public static string Component_Bin { get { return GetResourceString("Component_Bin"); } }
//Resources:ManufacturingResources:Component_ComponentType

		public static string Component_ComponentType { get { return GetResourceString("Component_ComponentType"); } }
//Resources:ManufacturingResources:Component_ComponentType_Select

		public static string Component_ComponentType_Select { get { return GetResourceString("Component_ComponentType_Select"); } }
//Resources:ManufacturingResources:Component_Cost

		public static string Component_Cost { get { return GetResourceString("Component_Cost"); } }
//Resources:ManufacturingResources:Component_DataSheet

		public static string Component_DataSheet { get { return GetResourceString("Component_DataSheet"); } }
//Resources:ManufacturingResources:Component_Description

		public static string Component_Description { get { return GetResourceString("Component_Description"); } }
//Resources:ManufacturingResources:Component_ExtendedPrice

		public static string Component_ExtendedPrice { get { return GetResourceString("Component_ExtendedPrice"); } }
//Resources:ManufacturingResources:Component_Feeder

		public static string Component_Feeder { get { return GetResourceString("Component_Feeder"); } }
//Resources:ManufacturingResources:Component_Feeder_Select

		public static string Component_Feeder_Select { get { return GetResourceString("Component_Feeder_Select"); } }
//Resources:ManufacturingResources:Component_MfgPartNumb

		public static string Component_MfgPartNumb { get { return GetResourceString("Component_MfgPartNumb"); } }
//Resources:ManufacturingResources:Component_PackageType

		public static string Component_PackageType { get { return GetResourceString("Component_PackageType"); } }
//Resources:ManufacturingResources:Component_PartNumber

		public static string Component_PartNumber { get { return GetResourceString("Component_PartNumber"); } }
//Resources:ManufacturingResources:Component_PartPack

		public static string Component_PartPack { get { return GetResourceString("Component_PartPack"); } }
//Resources:ManufacturingResources:Component_QuantityOnHand

		public static string Component_QuantityOnHand { get { return GetResourceString("Component_QuantityOnHand"); } }
//Resources:ManufacturingResources:Component_QuantityOnOrder

		public static string Component_QuantityOnOrder { get { return GetResourceString("Component_QuantityOnOrder"); } }
//Resources:ManufacturingResources:Component_Room

		public static string Component_Room { get { return GetResourceString("Component_Room"); } }
//Resources:ManufacturingResources:Component_Row

		public static string Component_Row { get { return GetResourceString("Component_Row"); } }
//Resources:ManufacturingResources:Component_Shelf

		public static string Component_Shelf { get { return GetResourceString("Component_Shelf"); } }
//Resources:ManufacturingResources:Component_ShelfUnit

		public static string Component_ShelfUnit { get { return GetResourceString("Component_ShelfUnit"); } }
//Resources:ManufacturingResources:Component_Title


		///<summary>
		///Component
		///</summary>
		public static string Component_Title { get { return GetResourceString("Component_Title"); } }
//Resources:ManufacturingResources:Component_Value

		public static string Component_Value { get { return GetResourceString("Component_Value"); } }
//Resources:ManufacturingResources:Component_VendorLink

		public static string Component_VendorLink { get { return GetResourceString("Component_VendorLink"); } }
//Resources:ManufacturingResources:ComponentPackage_CenterX

		public static string ComponentPackage_CenterX { get { return GetResourceString("ComponentPackage_CenterX"); } }
//Resources:ManufacturingResources:ComponentPackage_CenterX_Help

		public static string ComponentPackage_CenterX_Help { get { return GetResourceString("ComponentPackage_CenterX_Help"); } }
//Resources:ManufacturingResources:ComponentPackage_CenterY

		public static string ComponentPackage_CenterY { get { return GetResourceString("ComponentPackage_CenterY"); } }
//Resources:ManufacturingResources:ComponentPackage_CenterY_Help

		public static string ComponentPackage_CenterY_Help { get { return GetResourceString("ComponentPackage_CenterY_Help"); } }
//Resources:ManufacturingResources:ComponentPackage_Description

		public static string ComponentPackage_Description { get { return GetResourceString("ComponentPackage_Description"); } }
//Resources:ManufacturingResources:ComponentPackage_HoleSpacing

		public static string ComponentPackage_HoleSpacing { get { return GetResourceString("ComponentPackage_HoleSpacing"); } }
//Resources:ManufacturingResources:ComponentPackage_PackageId

		public static string ComponentPackage_PackageId { get { return GetResourceString("ComponentPackage_PackageId"); } }
//Resources:ManufacturingResources:ComponentPackage_PartHeight

		public static string ComponentPackage_PartHeight { get { return GetResourceString("ComponentPackage_PartHeight"); } }
//Resources:ManufacturingResources:ComponentPackage_PartLength

		public static string ComponentPackage_PartLength { get { return GetResourceString("ComponentPackage_PartLength"); } }
//Resources:ManufacturingResources:ComponentPackage_PartType

		public static string ComponentPackage_PartType { get { return GetResourceString("ComponentPackage_PartType"); } }
//Resources:ManufacturingResources:ComponentPackage_PartType_Select

		public static string ComponentPackage_PartType_Select { get { return GetResourceString("ComponentPackage_PartType_Select"); } }
//Resources:ManufacturingResources:ComponentPackage_PartWidth

		public static string ComponentPackage_PartWidth { get { return GetResourceString("ComponentPackage_PartWidth"); } }
//Resources:ManufacturingResources:ComponentPackage_Rotation

		public static string ComponentPackage_Rotation { get { return GetResourceString("ComponentPackage_Rotation"); } }
//Resources:ManufacturingResources:ComponentPackage_SpacingX

		public static string ComponentPackage_SpacingX { get { return GetResourceString("ComponentPackage_SpacingX"); } }
//Resources:ManufacturingResources:ComponentPackage_SpecificationPage

		public static string ComponentPackage_SpecificationPage { get { return GetResourceString("ComponentPackage_SpecificationPage"); } }
//Resources:ManufacturingResources:ComponentPackage_TapeWidth

		public static string ComponentPackage_TapeWidth { get { return GetResourceString("ComponentPackage_TapeWidth"); } }
//Resources:ManufacturingResources:ComponentPackage_TItle

		public static string ComponentPackage_TItle { get { return GetResourceString("ComponentPackage_TItle"); } }
//Resources:ManufacturingResources:ComponentPurchase_Description


		///<summary>
		///Records that identify purchased components
		///</summary>
		public static string ComponentPurchase_Description { get { return GetResourceString("ComponentPurchase_Description"); } }
//Resources:ManufacturingResources:ComponentPurchase_OrderDate

		public static string ComponentPurchase_OrderDate { get { return GetResourceString("ComponentPurchase_OrderDate"); } }
//Resources:ManufacturingResources:ComponentPurchase_OrderNumber

		public static string ComponentPurchase_OrderNumber { get { return GetResourceString("ComponentPurchase_OrderNumber"); } }
//Resources:ManufacturingResources:ComponentPurchase_Quantity

		public static string ComponentPurchase_Quantity { get { return GetResourceString("ComponentPurchase_Quantity"); } }
//Resources:ManufacturingResources:ComponentPurchase_Title

		public static string ComponentPurchase_Title { get { return GetResourceString("ComponentPurchase_Title"); } }
//Resources:ManufacturingResources:ComponentPurchase_Vendor

		public static string ComponentPurchase_Vendor { get { return GetResourceString("ComponentPurchase_Vendor"); } }
//Resources:ManufacturingResources:Feeder_Description

		public static string Feeder_Description { get { return GetResourceString("Feeder_Description"); } }
//Resources:ManufacturingResources:Feeder_Title

		public static string Feeder_Title { get { return GetResourceString("Feeder_Title"); } }
//Resources:ManufacturingResources:Feeders_Title

		public static string Feeders_Title { get { return GetResourceString("Feeders_Title"); } }
//Resources:ManufacturingResources:PackAndPlace_Description

		public static string PackAndPlace_Description { get { return GetResourceString("PackAndPlace_Description"); } }
//Resources:ManufacturingResources:PartPack_Description

		public static string PartPack_Description { get { return GetResourceString("PartPack_Description"); } }
//Resources:ManufacturingResources:PartPack_Title

		public static string PartPack_Title { get { return GetResourceString("PartPack_Title"); } }
//Resources:ManufacturingResources:PartPacks_Title

		public static string PartPacks_Title { get { return GetResourceString("PartPacks_Title"); } }
//Resources:ManufacturingResources:PartType_Hardware

		public static string PartType_Hardware { get { return GetResourceString("PartType_Hardware"); } }
//Resources:ManufacturingResources:PartType_SurfaceMount

		public static string PartType_SurfaceMount { get { return GetResourceString("PartType_SurfaceMount"); } }
//Resources:ManufacturingResources:PartType_ThroughHole

		public static string PartType_ThroughHole { get { return GetResourceString("PartType_ThroughHole"); } }
//Resources:ManufacturingResources:PickAndPlaceJob_Title

		public static string PickAndPlaceJob_Title { get { return GetResourceString("PickAndPlaceJob_Title"); } }
//Resources:ManufacturingResources:StripFeeder_Description

		public static string StripFeeder_Description { get { return GetResourceString("StripFeeder_Description"); } }
//Resources:ManufacturingResources:StripFeeder_Title

		public static string StripFeeder_Title { get { return GetResourceString("StripFeeder_Title"); } }
//Resources:ManufacturingResources:StripFeederRow_Description

		public static string StripFeederRow_Description { get { return GetResourceString("StripFeederRow_Description"); } }
//Resources:ManufacturingResources:StripFeederRow_Title

		public static string StripFeederRow_Title { get { return GetResourceString("StripFeederRow_Title"); } }
//Resources:ManufacturingResources:StripFeeders_Title

		public static string StripFeeders_Title { get { return GetResourceString("StripFeeders_Title"); } }

		public static class Names
		{
			public const string Common_Category = "Common_Category";
			public const string Common_CreatedBy = "Common_CreatedBy";
			public const string Common_CreationDate = "Common_CreationDate";
			public const string Common_Description = "Common_Description";
			public const string Common_Icon = "Common_Icon";
			public const string Common_IsPublic = "Common_IsPublic";
			public const string Common_IsRequired = "Common_IsRequired";
			public const string Common_IsValid = "Common_IsValid";
			public const string Common_Key = "Common_Key";
			public const string Common_Key_Help = "Common_Key_Help";
			public const string Common_Key_Validation = "Common_Key_Validation";
			public const string Common_LastUpdated = "Common_LastUpdated";
			public const string Common_LastUpdatedBy = "Common_LastUpdatedBy";
			public const string Common_Name = "Common_Name";
			public const string Common_Note = "Common_Note";
			public const string Common_Notes = "Common_Notes";
			public const string Common_PageNumberOne = "Common_PageNumberOne";
			public const string Common_Resources = "Common_Resources";
			public const string Common_SelectCategory = "Common_SelectCategory";
			public const string Common_UniqueId = "Common_UniqueId";
			public const string Common_ValidationErrors = "Common_ValidationErrors";
			public const string CompomentPurchase_Title = "CompomentPurchase_Title";
			public const string Component_Attr1 = "Component_Attr1";
			public const string Component_Attr2 = "Component_Attr2";
			public const string Component_Bin = "Component_Bin";
			public const string Component_ComponentType = "Component_ComponentType";
			public const string Component_ComponentType_Select = "Component_ComponentType_Select";
			public const string Component_Cost = "Component_Cost";
			public const string Component_DataSheet = "Component_DataSheet";
			public const string Component_Description = "Component_Description";
			public const string Component_ExtendedPrice = "Component_ExtendedPrice";
			public const string Component_Feeder = "Component_Feeder";
			public const string Component_Feeder_Select = "Component_Feeder_Select";
			public const string Component_MfgPartNumb = "Component_MfgPartNumb";
			public const string Component_PackageType = "Component_PackageType";
			public const string Component_PartNumber = "Component_PartNumber";
			public const string Component_PartPack = "Component_PartPack";
			public const string Component_QuantityOnHand = "Component_QuantityOnHand";
			public const string Component_QuantityOnOrder = "Component_QuantityOnOrder";
			public const string Component_Room = "Component_Room";
			public const string Component_Row = "Component_Row";
			public const string Component_Shelf = "Component_Shelf";
			public const string Component_ShelfUnit = "Component_ShelfUnit";
			public const string Component_Title = "Component_Title";
			public const string Component_Value = "Component_Value";
			public const string Component_VendorLink = "Component_VendorLink";
			public const string ComponentPackage_CenterX = "ComponentPackage_CenterX";
			public const string ComponentPackage_CenterX_Help = "ComponentPackage_CenterX_Help";
			public const string ComponentPackage_CenterY = "ComponentPackage_CenterY";
			public const string ComponentPackage_CenterY_Help = "ComponentPackage_CenterY_Help";
			public const string ComponentPackage_Description = "ComponentPackage_Description";
			public const string ComponentPackage_HoleSpacing = "ComponentPackage_HoleSpacing";
			public const string ComponentPackage_PackageId = "ComponentPackage_PackageId";
			public const string ComponentPackage_PartHeight = "ComponentPackage_PartHeight";
			public const string ComponentPackage_PartLength = "ComponentPackage_PartLength";
			public const string ComponentPackage_PartType = "ComponentPackage_PartType";
			public const string ComponentPackage_PartType_Select = "ComponentPackage_PartType_Select";
			public const string ComponentPackage_PartWidth = "ComponentPackage_PartWidth";
			public const string ComponentPackage_Rotation = "ComponentPackage_Rotation";
			public const string ComponentPackage_SpacingX = "ComponentPackage_SpacingX";
			public const string ComponentPackage_SpecificationPage = "ComponentPackage_SpecificationPage";
			public const string ComponentPackage_TapeWidth = "ComponentPackage_TapeWidth";
			public const string ComponentPackage_TItle = "ComponentPackage_TItle";
			public const string ComponentPurchase_Description = "ComponentPurchase_Description";
			public const string ComponentPurchase_OrderDate = "ComponentPurchase_OrderDate";
			public const string ComponentPurchase_OrderNumber = "ComponentPurchase_OrderNumber";
			public const string ComponentPurchase_Quantity = "ComponentPurchase_Quantity";
			public const string ComponentPurchase_Title = "ComponentPurchase_Title";
			public const string ComponentPurchase_Vendor = "ComponentPurchase_Vendor";
			public const string Feeder_Description = "Feeder_Description";
			public const string Feeder_Title = "Feeder_Title";
			public const string Feeders_Title = "Feeders_Title";
			public const string PackAndPlace_Description = "PackAndPlace_Description";
			public const string PartPack_Description = "PartPack_Description";
			public const string PartPack_Title = "PartPack_Title";
			public const string PartPacks_Title = "PartPacks_Title";
			public const string PartType_Hardware = "PartType_Hardware";
			public const string PartType_SurfaceMount = "PartType_SurfaceMount";
			public const string PartType_ThroughHole = "PartType_ThroughHole";
			public const string PickAndPlaceJob_Title = "PickAndPlaceJob_Title";
			public const string StripFeeder_Description = "StripFeeder_Description";
			public const string StripFeeder_Title = "StripFeeder_Title";
			public const string StripFeederRow_Description = "StripFeederRow_Description";
			public const string StripFeederRow_Title = "StripFeederRow_Title";
			public const string StripFeeders_Title = "StripFeeders_Title";
		}
	}
}

