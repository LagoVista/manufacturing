/*12/4/2024 5:54:26 AM*/
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
//Resources:ManufacturingResources:Common_Quantity

		public static string Common_Quantity { get { return GetResourceString("Common_Quantity"); } }
//Resources:ManufacturingResources:Common_QuantityBackOrdered

		public static string Common_QuantityBackOrdered { get { return GetResourceString("Common_QuantityBackOrdered"); } }
//Resources:ManufacturingResources:Common_QuantityOrdered

		public static string Common_QuantityOrdered { get { return GetResourceString("Common_QuantityOrdered"); } }
//Resources:ManufacturingResources:Common_QuantityReceived

		public static string Common_QuantityReceived { get { return GetResourceString("Common_QuantityReceived"); } }
//Resources:ManufacturingResources:Common_Resources

		public static string Common_Resources { get { return GetResourceString("Common_Resources"); } }
//Resources:ManufacturingResources:Common_SelectCategory

		public static string Common_SelectCategory { get { return GetResourceString("Common_SelectCategory"); } }
//Resources:ManufacturingResources:Common_Status

		public static string Common_Status { get { return GetResourceString("Common_Status"); } }
//Resources:ManufacturingResources:Common_Status_Select

		public static string Common_Status_Select { get { return GetResourceString("Common_Status_Select"); } }
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
//Resources:ManufacturingResources:Component_Supplier

		public static string Component_Supplier { get { return GetResourceString("Component_Supplier"); } }
//Resources:ManufacturingResources:Component_SupplierPartNumb

		public static string Component_SupplierPartNumb { get { return GetResourceString("Component_SupplierPartNumb"); } }
//Resources:ManufacturingResources:Component_Title


		///<summary>
		///Component
		///</summary>
		public static string Component_Title { get { return GetResourceString("Component_Title"); } }
//Resources:ManufacturingResources:Component_Value

		public static string Component_Value { get { return GetResourceString("Component_Value"); } }
//Resources:ManufacturingResources:Component_VendorLink

		public static string Component_VendorLink { get { return GetResourceString("Component_VendorLink"); } }
//Resources:ManufacturingResources:ComponentOrder_Description

		public static string ComponentOrder_Description { get { return GetResourceString("ComponentOrder_Description"); } }
//Resources:ManufacturingResources:ComponentOrder_LineItemCsv

		public static string ComponentOrder_LineItemCsv { get { return GetResourceString("ComponentOrder_LineItemCsv"); } }
//Resources:ManufacturingResources:ComponentOrder_LineItems

		public static string ComponentOrder_LineItems { get { return GetResourceString("ComponentOrder_LineItems"); } }
//Resources:ManufacturingResources:ComponentOrder_OrderDate

		public static string ComponentOrder_OrderDate { get { return GetResourceString("ComponentOrder_OrderDate"); } }
//Resources:ManufacturingResources:ComponentOrder_ReceiveDate

		public static string ComponentOrder_ReceiveDate { get { return GetResourceString("ComponentOrder_ReceiveDate"); } }
//Resources:ManufacturingResources:ComponentOrder_Shipping

		public static string ComponentOrder_Shipping { get { return GetResourceString("ComponentOrder_Shipping"); } }
//Resources:ManufacturingResources:ComponentOrder_SubTotal

		public static string ComponentOrder_SubTotal { get { return GetResourceString("ComponentOrder_SubTotal"); } }
//Resources:ManufacturingResources:ComponentOrder_Supplier

		public static string ComponentOrder_Supplier { get { return GetResourceString("ComponentOrder_Supplier"); } }
//Resources:ManufacturingResources:ComponentOrder_SupplierOrderNumber

		public static string ComponentOrder_SupplierOrderNumber { get { return GetResourceString("ComponentOrder_SupplierOrderNumber"); } }
//Resources:ManufacturingResources:ComponentOrder_Tariff

		public static string ComponentOrder_Tariff { get { return GetResourceString("ComponentOrder_Tariff"); } }
//Resources:ManufacturingResources:ComponentOrder_Tax

		public static string ComponentOrder_Tax { get { return GetResourceString("ComponentOrder_Tax"); } }
//Resources:ManufacturingResources:ComponentOrder_Title

		public static string ComponentOrder_Title { get { return GetResourceString("ComponentOrder_Title"); } }
//Resources:ManufacturingResources:ComponentOrder_Total

		public static string ComponentOrder_Total { get { return GetResourceString("ComponentOrder_Total"); } }
//Resources:ManufacturingResources:ComponentOrderLineItem_Backordered

		public static string ComponentOrderLineItem_Backordered { get { return GetResourceString("ComponentOrderLineItem_Backordered"); } }
//Resources:ManufacturingResources:ComponentOrderLineItem_Component

		public static string ComponentOrderLineItem_Component { get { return GetResourceString("ComponentOrderLineItem_Component"); } }
//Resources:ManufacturingResources:ComponentOrderLineItem_Component_Select

		public static string ComponentOrderLineItem_Component_Select { get { return GetResourceString("ComponentOrderLineItem_Component_Select"); } }
//Resources:ManufacturingResources:ComponentOrderLineItem_Help

		public static string ComponentOrderLineItem_Help { get { return GetResourceString("ComponentOrderLineItem_Help"); } }
//Resources:ManufacturingResources:ComponentOrderLineItem_MfgPartNumber

		public static string ComponentOrderLineItem_MfgPartNumber { get { return GetResourceString("ComponentOrderLineItem_MfgPartNumber"); } }
//Resources:ManufacturingResources:ComponentOrderLineItem_Received

		public static string ComponentOrderLineItem_Received { get { return GetResourceString("ComponentOrderLineItem_Received"); } }
//Resources:ManufacturingResources:ComponentOrderLineItem_Received_Help

		public static string ComponentOrderLineItem_Received_Help { get { return GetResourceString("ComponentOrderLineItem_Received_Help"); } }
//Resources:ManufacturingResources:ComponentOrderLineItem_SupplierPartNumber

		public static string ComponentOrderLineItem_SupplierPartNumber { get { return GetResourceString("ComponentOrderLineItem_SupplierPartNumber"); } }
//Resources:ManufacturingResources:ComponentOrderLineItem_Title

		public static string ComponentOrderLineItem_Title { get { return GetResourceString("ComponentOrderLineItem_Title"); } }
//Resources:ManufacturingResources:ComponentOrderLineItem_UnitPrice

		public static string ComponentOrderLineItem_UnitPrice { get { return GetResourceString("ComponentOrderLineItem_UnitPrice"); } }
//Resources:ManufacturingResources:ComponentOrders_Title

		public static string ComponentOrders_Title { get { return GetResourceString("ComponentOrders_Title"); } }
//Resources:ManufacturingResources:ComponentOrders_TitleString

		public static string ComponentOrders_TitleString { get { return GetResourceString("ComponentOrders_TitleString"); } }
//Resources:ManufacturingResources:ComponentOrderStatusTypes_Ordered

		public static string ComponentOrderStatusTypes_Ordered { get { return GetResourceString("ComponentOrderStatusTypes_Ordered"); } }
//Resources:ManufacturingResources:ComponentOrderStatusTypes_Pending

		public static string ComponentOrderStatusTypes_Pending { get { return GetResourceString("ComponentOrderStatusTypes_Pending"); } }
//Resources:ManufacturingResources:ComponentOrderStatusTypes_Received

		public static string ComponentOrderStatusTypes_Received { get { return GetResourceString("ComponentOrderStatusTypes_Received"); } }
//Resources:ManufacturingResources:ComponentOrderStatusTypes_Stocked

		public static string ComponentOrderStatusTypes_Stocked { get { return GetResourceString("ComponentOrderStatusTypes_Stocked"); } }
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
//Resources:ManufacturingResources:ComponentPackage_TapePitch

		public static string ComponentPackage_TapePitch { get { return GetResourceString("ComponentPackage_TapePitch"); } }
//Resources:ManufacturingResources:ComponentPackage_TapePitch_Select

		public static string ComponentPackage_TapePitch_Select { get { return GetResourceString("ComponentPackage_TapePitch_Select"); } }
//Resources:ManufacturingResources:ComponentPackage_TapeRotation

		public static string ComponentPackage_TapeRotation { get { return GetResourceString("ComponentPackage_TapeRotation"); } }
//Resources:ManufacturingResources:ComponentPackage_TapeRotation_Select

		public static string ComponentPackage_TapeRotation_Select { get { return GetResourceString("ComponentPackage_TapeRotation_Select"); } }
//Resources:ManufacturingResources:ComponentPackage_TapeSize

		public static string ComponentPackage_TapeSize { get { return GetResourceString("ComponentPackage_TapeSize"); } }
//Resources:ManufacturingResources:ComponentPackage_TapeSize_Select

		public static string ComponentPackage_TapeSize_Select { get { return GetResourceString("ComponentPackage_TapeSize_Select"); } }
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
//Resources:ManufacturingResources:Machine_Description

		public static string Machine_Description { get { return GetResourceString("Machine_Description"); } }
//Resources:ManufacturingResources:Machine_Title

		public static string Machine_Title { get { return GetResourceString("Machine_Title"); } }
//Resources:ManufacturingResources:Machines_Title

		public static string Machines_Title { get { return GetResourceString("Machines_Title"); } }
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
//Resources:ManufacturingResources:Pcb_Description

		public static string Pcb_Description { get { return GetResourceString("Pcb_Description"); } }
//Resources:ManufacturingResources:Pcb_Revision

		public static string Pcb_Revision { get { return GetResourceString("Pcb_Revision"); } }
//Resources:ManufacturingResources:Pcb_Revision_BoardFile

		public static string Pcb_Revision_BoardFile { get { return GetResourceString("Pcb_Revision_BoardFile"); } }
//Resources:ManufacturingResources:Pcb_Revision_BomFile

		public static string Pcb_Revision_BomFile { get { return GetResourceString("Pcb_Revision_BomFile"); } }
//Resources:ManufacturingResources:Pcb_Revision_Description

		public static string Pcb_Revision_Description { get { return GetResourceString("Pcb_Revision_Description"); } }
//Resources:ManufacturingResources:Pcb_Revision_RevisionTimeStamp

		public static string Pcb_Revision_RevisionTimeStamp { get { return GetResourceString("Pcb_Revision_RevisionTimeStamp"); } }
//Resources:ManufacturingResources:Pcb_Revision_SchematicFile

		public static string Pcb_Revision_SchematicFile { get { return GetResourceString("Pcb_Revision_SchematicFile"); } }
//Resources:ManufacturingResources:Pcb_Revision_SchematicPdFile

		public static string Pcb_Revision_SchematicPdFile { get { return GetResourceString("Pcb_Revision_SchematicPdFile"); } }
//Resources:ManufacturingResources:Pcb_Revision_Title

		public static string Pcb_Revision_Title { get { return GetResourceString("Pcb_Revision_Title"); } }
//Resources:ManufacturingResources:Pcb_Revisions

		public static string Pcb_Revisions { get { return GetResourceString("Pcb_Revisions"); } }
//Resources:ManufacturingResources:Pcb_Sku

		public static string Pcb_Sku { get { return GetResourceString("Pcb_Sku"); } }
//Resources:ManufacturingResources:Pcb_Title

		public static string Pcb_Title { get { return GetResourceString("Pcb_Title"); } }
//Resources:ManufacturingResources:Pcb_Variant

		public static string Pcb_Variant { get { return GetResourceString("Pcb_Variant"); } }
//Resources:ManufacturingResources:Pcb_Variant_Description

		public static string Pcb_Variant_Description { get { return GetResourceString("Pcb_Variant_Description"); } }
//Resources:ManufacturingResources:Pcb_Variant_Sku

		public static string Pcb_Variant_Sku { get { return GetResourceString("Pcb_Variant_Sku"); } }
//Resources:ManufacturingResources:Pcb_Variants

		public static string Pcb_Variants { get { return GetResourceString("Pcb_Variants"); } }
//Resources:ManufacturingResources:Pcbs_Title

		public static string Pcbs_Title { get { return GetResourceString("Pcbs_Title"); } }
//Resources:ManufacturingResources:Pfb_Revision_Revision

		public static string Pfb_Revision_Revision { get { return GetResourceString("Pfb_Revision_Revision"); } }
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
//Resources:ManufacturingResources:TapePitch_12

		public static string TapePitch_12 { get { return GetResourceString("TapePitch_12"); } }
//Resources:ManufacturingResources:TapePitch_16

		public static string TapePitch_16 { get { return GetResourceString("TapePitch_16"); } }
//Resources:ManufacturingResources:TapePitch_2

		public static string TapePitch_2 { get { return GetResourceString("TapePitch_2"); } }
//Resources:ManufacturingResources:TapePitch_20

		public static string TapePitch_20 { get { return GetResourceString("TapePitch_20"); } }
//Resources:ManufacturingResources:TapePitch_24

		public static string TapePitch_24 { get { return GetResourceString("TapePitch_24"); } }
//Resources:ManufacturingResources:TapePitch_28

		public static string TapePitch_28 { get { return GetResourceString("TapePitch_28"); } }
//Resources:ManufacturingResources:TapePitch_32

		public static string TapePitch_32 { get { return GetResourceString("TapePitch_32"); } }
//Resources:ManufacturingResources:TapePitch_4

		public static string TapePitch_4 { get { return GetResourceString("TapePitch_4"); } }
//Resources:ManufacturingResources:TapePitch_8

		public static string TapePitch_8 { get { return GetResourceString("TapePitch_8"); } }
//Resources:ManufacturingResources:TapeRotation_0

		public static string TapeRotation_0 { get { return GetResourceString("TapeRotation_0"); } }
//Resources:ManufacturingResources:TapeRotation_180

		public static string TapeRotation_180 { get { return GetResourceString("TapeRotation_180"); } }
//Resources:ManufacturingResources:TapeRotation_90

		public static string TapeRotation_90 { get { return GetResourceString("TapeRotation_90"); } }
//Resources:ManufacturingResources:TapeRotation_Minus90

		public static string TapeRotation_Minus90 { get { return GetResourceString("TapeRotation_Minus90"); } }
//Resources:ManufacturingResources:TapeSize_12

		public static string TapeSize_12 { get { return GetResourceString("TapeSize_12"); } }
//Resources:ManufacturingResources:TapeSize_16

		public static string TapeSize_16 { get { return GetResourceString("TapeSize_16"); } }
//Resources:ManufacturingResources:TapeSize_20

		public static string TapeSize_20 { get { return GetResourceString("TapeSize_20"); } }
//Resources:ManufacturingResources:TapeSize_24

		public static string TapeSize_24 { get { return GetResourceString("TapeSize_24"); } }
//Resources:ManufacturingResources:TapeSize_32

		public static string TapeSize_32 { get { return GetResourceString("TapeSize_32"); } }
//Resources:ManufacturingResources:TapeSize_44

		public static string TapeSize_44 { get { return GetResourceString("TapeSize_44"); } }
//Resources:ManufacturingResources:TapeSize_8

		public static string TapeSize_8 { get { return GetResourceString("TapeSize_8"); } }

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
			public const string Common_Quantity = "Common_Quantity";
			public const string Common_QuantityBackOrdered = "Common_QuantityBackOrdered";
			public const string Common_QuantityOrdered = "Common_QuantityOrdered";
			public const string Common_QuantityReceived = "Common_QuantityReceived";
			public const string Common_Resources = "Common_Resources";
			public const string Common_SelectCategory = "Common_SelectCategory";
			public const string Common_Status = "Common_Status";
			public const string Common_Status_Select = "Common_Status_Select";
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
			public const string Component_Supplier = "Component_Supplier";
			public const string Component_SupplierPartNumb = "Component_SupplierPartNumb";
			public const string Component_Title = "Component_Title";
			public const string Component_Value = "Component_Value";
			public const string Component_VendorLink = "Component_VendorLink";
			public const string ComponentOrder_Description = "ComponentOrder_Description";
			public const string ComponentOrder_LineItemCsv = "ComponentOrder_LineItemCsv";
			public const string ComponentOrder_LineItems = "ComponentOrder_LineItems";
			public const string ComponentOrder_OrderDate = "ComponentOrder_OrderDate";
			public const string ComponentOrder_ReceiveDate = "ComponentOrder_ReceiveDate";
			public const string ComponentOrder_Shipping = "ComponentOrder_Shipping";
			public const string ComponentOrder_SubTotal = "ComponentOrder_SubTotal";
			public const string ComponentOrder_Supplier = "ComponentOrder_Supplier";
			public const string ComponentOrder_SupplierOrderNumber = "ComponentOrder_SupplierOrderNumber";
			public const string ComponentOrder_Tariff = "ComponentOrder_Tariff";
			public const string ComponentOrder_Tax = "ComponentOrder_Tax";
			public const string ComponentOrder_Title = "ComponentOrder_Title";
			public const string ComponentOrder_Total = "ComponentOrder_Total";
			public const string ComponentOrderLineItem_Backordered = "ComponentOrderLineItem_Backordered";
			public const string ComponentOrderLineItem_Component = "ComponentOrderLineItem_Component";
			public const string ComponentOrderLineItem_Component_Select = "ComponentOrderLineItem_Component_Select";
			public const string ComponentOrderLineItem_Help = "ComponentOrderLineItem_Help";
			public const string ComponentOrderLineItem_MfgPartNumber = "ComponentOrderLineItem_MfgPartNumber";
			public const string ComponentOrderLineItem_Received = "ComponentOrderLineItem_Received";
			public const string ComponentOrderLineItem_Received_Help = "ComponentOrderLineItem_Received_Help";
			public const string ComponentOrderLineItem_SupplierPartNumber = "ComponentOrderLineItem_SupplierPartNumber";
			public const string ComponentOrderLineItem_Title = "ComponentOrderLineItem_Title";
			public const string ComponentOrderLineItem_UnitPrice = "ComponentOrderLineItem_UnitPrice";
			public const string ComponentOrders_Title = "ComponentOrders_Title";
			public const string ComponentOrders_TitleString = "ComponentOrders_TitleString";
			public const string ComponentOrderStatusTypes_Ordered = "ComponentOrderStatusTypes_Ordered";
			public const string ComponentOrderStatusTypes_Pending = "ComponentOrderStatusTypes_Pending";
			public const string ComponentOrderStatusTypes_Received = "ComponentOrderStatusTypes_Received";
			public const string ComponentOrderStatusTypes_Stocked = "ComponentOrderStatusTypes_Stocked";
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
			public const string ComponentPackage_TapePitch = "ComponentPackage_TapePitch";
			public const string ComponentPackage_TapePitch_Select = "ComponentPackage_TapePitch_Select";
			public const string ComponentPackage_TapeRotation = "ComponentPackage_TapeRotation";
			public const string ComponentPackage_TapeRotation_Select = "ComponentPackage_TapeRotation_Select";
			public const string ComponentPackage_TapeSize = "ComponentPackage_TapeSize";
			public const string ComponentPackage_TapeSize_Select = "ComponentPackage_TapeSize_Select";
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
			public const string Machine_Description = "Machine_Description";
			public const string Machine_Title = "Machine_Title";
			public const string Machines_Title = "Machines_Title";
			public const string PackAndPlace_Description = "PackAndPlace_Description";
			public const string PartPack_Description = "PartPack_Description";
			public const string PartPack_Title = "PartPack_Title";
			public const string PartPacks_Title = "PartPacks_Title";
			public const string PartType_Hardware = "PartType_Hardware";
			public const string PartType_SurfaceMount = "PartType_SurfaceMount";
			public const string PartType_ThroughHole = "PartType_ThroughHole";
			public const string Pcb_Description = "Pcb_Description";
			public const string Pcb_Revision = "Pcb_Revision";
			public const string Pcb_Revision_BoardFile = "Pcb_Revision_BoardFile";
			public const string Pcb_Revision_BomFile = "Pcb_Revision_BomFile";
			public const string Pcb_Revision_Description = "Pcb_Revision_Description";
			public const string Pcb_Revision_RevisionTimeStamp = "Pcb_Revision_RevisionTimeStamp";
			public const string Pcb_Revision_SchematicFile = "Pcb_Revision_SchematicFile";
			public const string Pcb_Revision_SchematicPdFile = "Pcb_Revision_SchematicPdFile";
			public const string Pcb_Revision_Title = "Pcb_Revision_Title";
			public const string Pcb_Revisions = "Pcb_Revisions";
			public const string Pcb_Sku = "Pcb_Sku";
			public const string Pcb_Title = "Pcb_Title";
			public const string Pcb_Variant = "Pcb_Variant";
			public const string Pcb_Variant_Description = "Pcb_Variant_Description";
			public const string Pcb_Variant_Sku = "Pcb_Variant_Sku";
			public const string Pcb_Variants = "Pcb_Variants";
			public const string Pcbs_Title = "Pcbs_Title";
			public const string Pfb_Revision_Revision = "Pfb_Revision_Revision";
			public const string PickAndPlaceJob_Title = "PickAndPlaceJob_Title";
			public const string StripFeeder_Description = "StripFeeder_Description";
			public const string StripFeeder_Title = "StripFeeder_Title";
			public const string StripFeederRow_Description = "StripFeederRow_Description";
			public const string StripFeederRow_Title = "StripFeederRow_Title";
			public const string StripFeeders_Title = "StripFeeders_Title";
			public const string TapePitch_12 = "TapePitch_12";
			public const string TapePitch_16 = "TapePitch_16";
			public const string TapePitch_2 = "TapePitch_2";
			public const string TapePitch_20 = "TapePitch_20";
			public const string TapePitch_24 = "TapePitch_24";
			public const string TapePitch_28 = "TapePitch_28";
			public const string TapePitch_32 = "TapePitch_32";
			public const string TapePitch_4 = "TapePitch_4";
			public const string TapePitch_8 = "TapePitch_8";
			public const string TapeRotation_0 = "TapeRotation_0";
			public const string TapeRotation_180 = "TapeRotation_180";
			public const string TapeRotation_90 = "TapeRotation_90";
			public const string TapeRotation_Minus90 = "TapeRotation_Minus90";
			public const string TapeSize_12 = "TapeSize_12";
			public const string TapeSize_16 = "TapeSize_16";
			public const string TapeSize_20 = "TapeSize_20";
			public const string TapeSize_24 = "TapeSize_24";
			public const string TapeSize_32 = "TapeSize_32";
			public const string TapeSize_44 = "TapeSize_44";
			public const string TapeSize_8 = "TapeSize_8";
		}
	}
}

