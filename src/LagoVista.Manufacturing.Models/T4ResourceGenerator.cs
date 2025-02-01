/*2/1/2025 2:03:57 PM*/
using System.Globalization;
using System.Reflection;

//Resources:ManufacturingResources:AssemblyInstruction_Description
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
		
		public static string AssemblyInstruction_Description { get { return GetResourceString("AssemblyInstruction_Description"); } }
//Resources:ManufacturingResources:AssemblyInstruction_Steps

		public static string AssemblyInstruction_Steps { get { return GetResourceString("AssemblyInstruction_Steps"); } }
//Resources:ManufacturingResources:AssemblyInstruction_Title

		public static string AssemblyInstruction_Title { get { return GetResourceString("AssemblyInstruction_Title"); } }
//Resources:ManufacturingResources:AssemblyInstructions_Title

		public static string AssemblyInstructions_Title { get { return GetResourceString("AssemblyInstructions_Title"); } }
//Resources:ManufacturingResources:AssemblyInstructionStep_Description

		public static string AssemblyInstructionStep_Description { get { return GetResourceString("AssemblyInstructionStep_Description"); } }
//Resources:ManufacturingResources:AssemblyInstructionStep_Images

		public static string AssemblyInstructionStep_Images { get { return GetResourceString("AssemblyInstructionStep_Images"); } }
//Resources:ManufacturingResources:AssemblyInstructionStep_Title

		public static string AssemblyInstructionStep_Title { get { return GetResourceString("AssemblyInstructionStep_Title"); } }
//Resources:ManufacturingResources:AssembyInstructionsStep_Instructions

		public static string AssembyInstructionsStep_Instructions { get { return GetResourceString("AssembyInstructionsStep_Instructions"); } }
//Resources:ManufacturingResources:AutoFeeder_Description

		public static string AutoFeeder_Description { get { return GetResourceString("AutoFeeder_Description"); } }
//Resources:ManufacturingResources:AutoFeeder_PickOffsetFromFiducial

		public static string AutoFeeder_PickOffsetFromFiducial { get { return GetResourceString("AutoFeeder_PickOffsetFromFiducial"); } }
//Resources:ManufacturingResources:AutoFeeder_PickOffsetFromFiducial_Help

		public static string AutoFeeder_PickOffsetFromFiducial_Help { get { return GetResourceString("AutoFeeder_PickOffsetFromFiducial_Help"); } }
//Resources:ManufacturingResources:AutoFeeder_Size_Help

		public static string AutoFeeder_Size_Help { get { return GetResourceString("AutoFeeder_Size_Help"); } }
//Resources:ManufacturingResources:AutoFeeder_Title

		public static string AutoFeeder_Title { get { return GetResourceString("AutoFeeder_Title"); } }
//Resources:ManufacturingResources:AutoFeeders_Title

		public static string AutoFeeders_Title { get { return GetResourceString("AutoFeeders_Title"); } }
//Resources:ManufacturingResources:AutoFeederTemplate_Description

		public static string AutoFeederTemplate_Description { get { return GetResourceString("AutoFeederTemplate_Description"); } }
//Resources:ManufacturingResources:AutoFeederTemplate_Title

		public static string AutoFeederTemplate_Title { get { return GetResourceString("AutoFeederTemplate_Title"); } }
//Resources:ManufacturingResources:AutoFeederTemplates_Title

		public static string AutoFeederTemplates_Title { get { return GetResourceString("AutoFeederTemplates_Title"); } }
//Resources:ManufacturingResources:Common_Category

		public static string Common_Category { get { return GetResourceString("Common_Category"); } }
//Resources:ManufacturingResources:Common_Color

		public static string Common_Color { get { return GetResourceString("Common_Color"); } }
//Resources:ManufacturingResources:Common_CreatedBy

		public static string Common_CreatedBy { get { return GetResourceString("Common_CreatedBy"); } }
//Resources:ManufacturingResources:Common_CreationDate

		public static string Common_CreationDate { get { return GetResourceString("Common_CreationDate"); } }
//Resources:ManufacturingResources:Common_Description

		public static string Common_Description { get { return GetResourceString("Common_Description"); } }
//Resources:ManufacturingResources:Common_Height

		public static string Common_Height { get { return GetResourceString("Common_Height"); } }
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
//Resources:ManufacturingResources:Common_Length

		public static string Common_Length { get { return GetResourceString("Common_Length"); } }
//Resources:ManufacturingResources:Common_Name

		public static string Common_Name { get { return GetResourceString("Common_Name"); } }
//Resources:ManufacturingResources:Common_Note

		public static string Common_Note { get { return GetResourceString("Common_Note"); } }
//Resources:ManufacturingResources:Common_Notes

		public static string Common_Notes { get { return GetResourceString("Common_Notes"); } }
//Resources:ManufacturingResources:Common_Offset

		public static string Common_Offset { get { return GetResourceString("Common_Offset"); } }
//Resources:ManufacturingResources:Common_Origin

		public static string Common_Origin { get { return GetResourceString("Common_Origin"); } }
//Resources:ManufacturingResources:Common_Origin_Help

		public static string Common_Origin_Help { get { return GetResourceString("Common_Origin_Help"); } }
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
//Resources:ManufacturingResources:Common_Size

		public static string Common_Size { get { return GetResourceString("Common_Size"); } }
//Resources:ManufacturingResources:Common_Status

		public static string Common_Status { get { return GetResourceString("Common_Status"); } }
//Resources:ManufacturingResources:Common_Status_Select

		public static string Common_Status_Select { get { return GetResourceString("Common_Status_Select"); } }
//Resources:ManufacturingResources:Common_TItle

		public static string Common_TItle { get { return GetResourceString("Common_TItle"); } }
//Resources:ManufacturingResources:Common_UniqueId

		public static string Common_UniqueId { get { return GetResourceString("Common_UniqueId"); } }
//Resources:ManufacturingResources:Common_ValidationErrors

		public static string Common_ValidationErrors { get { return GetResourceString("Common_ValidationErrors"); } }
//Resources:ManufacturingResources:Common_Value

		public static string Common_Value { get { return GetResourceString("Common_Value"); } }
//Resources:ManufacturingResources:Common_Width

		public static string Common_Width { get { return GetResourceString("Common_Width"); } }
//Resources:ManufacturingResources:CompomentPurchase_Title

		public static string CompomentPurchase_Title { get { return GetResourceString("CompomentPurchase_Title"); } }
//Resources:ManufacturingResources:Component_Attr1

		public static string Component_Attr1 { get { return GetResourceString("Component_Attr1"); } }
//Resources:ManufacturingResources:Component_Attr2

		public static string Component_Attr2 { get { return GetResourceString("Component_Attr2"); } }
//Resources:ManufacturingResources:Component_Attributes

		public static string Component_Attributes { get { return GetResourceString("Component_Attributes"); } }
//Resources:ManufacturingResources:Component_Bin

		public static string Component_Bin { get { return GetResourceString("Component_Bin"); } }
//Resources:ManufacturingResources:Component_Column

		public static string Component_Column { get { return GetResourceString("Component_Column"); } }
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
//Resources:ManufacturingResources:Component_Polarized

		public static string Component_Polarized { get { return GetResourceString("Component_Polarized"); } }
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
//Resources:ManufacturingResources:ComponentAttribute_Description

		public static string ComponentAttribute_Description { get { return GetResourceString("ComponentAttribute_Description"); } }
//Resources:ManufacturingResources:ComponentAttributes_Title

		public static string ComponentAttributes_Title { get { return GetResourceString("ComponentAttributes_Title"); } }
//Resources:ManufacturingResources:ComponentOrder_Description

		public static string ComponentOrder_Description { get { return GetResourceString("ComponentOrder_Description"); } }
//Resources:ManufacturingResources:ComponentOrder_LineItemCsv

		public static string ComponentOrder_LineItemCsv { get { return GetResourceString("ComponentOrder_LineItemCsv"); } }
//Resources:ManufacturingResources:ComponentOrder_LineItems

		public static string ComponentOrder_LineItems { get { return GetResourceString("ComponentOrder_LineItems"); } }
//Resources:ManufacturingResources:ComponentOrder_OrderDate

		public static string ComponentOrder_OrderDate { get { return GetResourceString("ComponentOrder_OrderDate"); } }
//Resources:ManufacturingResources:ComponentOrder_PrintLabels

		public static string ComponentOrder_PrintLabels { get { return GetResourceString("ComponentOrder_PrintLabels"); } }
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
//Resources:ManufacturingResources:ComponentPackage_CustomTapePitch

		public static string ComponentPackage_CustomTapePitch { get { return GetResourceString("ComponentPackage_CustomTapePitch"); } }
//Resources:ManufacturingResources:ComponentPackage_Description

		public static string ComponentPackage_Description { get { return GetResourceString("ComponentPackage_Description"); } }
//Resources:ManufacturingResources:ComponentPackage_HoleSpacing

		public static string ComponentPackage_HoleSpacing { get { return GetResourceString("ComponentPackage_HoleSpacing"); } }
//Resources:ManufacturingResources:ComponentPackage_MaterialType_Paper

		public static string ComponentPackage_MaterialType_Paper { get { return GetResourceString("ComponentPackage_MaterialType_Paper"); } }
//Resources:ManufacturingResources:ComponentPackage_MaterialType_Plastic

		public static string ComponentPackage_MaterialType_Plastic { get { return GetResourceString("ComponentPackage_MaterialType_Plastic"); } }
//Resources:ManufacturingResources:ComponentPackage_NozzleTip

		public static string ComponentPackage_NozzleTip { get { return GetResourceString("ComponentPackage_NozzleTip"); } }
//Resources:ManufacturingResources:ComponentPackage_NozzleTip_Help

		public static string ComponentPackage_NozzleTip_Help { get { return GetResourceString("ComponentPackage_NozzleTip_Help"); } }
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
//Resources:ManufacturingResources:ComponentPackage_PickVacuumLevel

		public static string ComponentPackage_PickVacuumLevel { get { return GetResourceString("ComponentPackage_PickVacuumLevel"); } }
//Resources:ManufacturingResources:ComponentPackage_Rotation

		public static string ComponentPackage_Rotation { get { return GetResourceString("ComponentPackage_Rotation"); } }
//Resources:ManufacturingResources:ComponentPackage_SpacingX

		public static string ComponentPackage_SpacingX { get { return GetResourceString("ComponentPackage_SpacingX"); } }
//Resources:ManufacturingResources:ComponentPackage_SpecificationPage

		public static string ComponentPackage_SpecificationPage { get { return GetResourceString("ComponentPackage_SpecificationPage"); } }
//Resources:ManufacturingResources:ComponentPackage_TapeAndReelActualImage

		public static string ComponentPackage_TapeAndReelActualImage { get { return GetResourceString("ComponentPackage_TapeAndReelActualImage"); } }
//Resources:ManufacturingResources:ComponentPackage_TapeAndReelSpecImage

		public static string ComponentPackage_TapeAndReelSpecImage { get { return GetResourceString("ComponentPackage_TapeAndReelSpecImage"); } }
//Resources:ManufacturingResources:ComponentPackage_TapeColor

		public static string ComponentPackage_TapeColor { get { return GetResourceString("ComponentPackage_TapeColor"); } }
//Resources:ManufacturingResources:ComponentPackage_TapeColor_Black

		public static string ComponentPackage_TapeColor_Black { get { return GetResourceString("ComponentPackage_TapeColor_Black"); } }
//Resources:ManufacturingResources:ComponentPackage_TapeColor_Clear

		public static string ComponentPackage_TapeColor_Clear { get { return GetResourceString("ComponentPackage_TapeColor_Clear"); } }
//Resources:ManufacturingResources:ComponentPackage_TapeColor_Select

		public static string ComponentPackage_TapeColor_Select { get { return GetResourceString("ComponentPackage_TapeColor_Select"); } }
//Resources:ManufacturingResources:ComponentPackage_TapeColor_White

		public static string ComponentPackage_TapeColor_White { get { return GetResourceString("ComponentPackage_TapeColor_White"); } }
//Resources:ManufacturingResources:ComponentPackage_TapeMaterialType

		public static string ComponentPackage_TapeMaterialType { get { return GetResourceString("ComponentPackage_TapeMaterialType"); } }
//Resources:ManufacturingResources:ComponentPackage_TapeMaterialType_Select

		public static string ComponentPackage_TapeMaterialType_Select { get { return GetResourceString("ComponentPackage_TapeMaterialType_Select"); } }
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
//Resources:ManufacturingResources:ComponentPackage_TapeSpecificationPage

		public static string ComponentPackage_TapeSpecificationPage { get { return GetResourceString("ComponentPackage_TapeSpecificationPage"); } }
//Resources:ManufacturingResources:ComponentPackage_TapeWidth

		public static string ComponentPackage_TapeWidth { get { return GetResourceString("ComponentPackage_TapeWidth"); } }
//Resources:ManufacturingResources:ComponentPackage_TItle

		public static string ComponentPackage_TItle { get { return GetResourceString("ComponentPackage_TItle"); } }
//Resources:ManufacturingResources:ComponentPackage_Verified

		public static string ComponentPackage_Verified { get { return GetResourceString("ComponentPackage_Verified"); } }
//Resources:ManufacturingResources:ComponentPackage_Verified_Help

		public static string ComponentPackage_Verified_Help { get { return GetResourceString("ComponentPackage_Verified_Help"); } }
//Resources:ManufacturingResources:ComponentPackage_XOffsetReferenceHole

		public static string ComponentPackage_XOffsetReferenceHole { get { return GetResourceString("ComponentPackage_XOffsetReferenceHole"); } }
//Resources:ManufacturingResources:ComponentPackage_YOffsetReferenceHole

		public static string ComponentPackage_YOffsetReferenceHole { get { return GetResourceString("ComponentPackage_YOffsetReferenceHole"); } }
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
//Resources:ManufacturingResources:ContourRetrieveMode_External

		public static string ContourRetrieveMode_External { get { return GetResourceString("ContourRetrieveMode_External"); } }
//Resources:ManufacturingResources:ContourRetrieveMode_FloodFill

		public static string ContourRetrieveMode_FloodFill { get { return GetResourceString("ContourRetrieveMode_FloodFill"); } }
//Resources:ManufacturingResources:ContourRetrieveMode_List

		public static string ContourRetrieveMode_List { get { return GetResourceString("ContourRetrieveMode_List"); } }
//Resources:ManufacturingResources:ContourRetrieveMode_Tree

		public static string ContourRetrieveMode_Tree { get { return GetResourceString("ContourRetrieveMode_Tree"); } }
//Resources:ManufacturingResources:ContourRetrieveMode_TwoLevelHierarchy

		public static string ContourRetrieveMode_TwoLevelHierarchy { get { return GetResourceString("ContourRetrieveMode_TwoLevelHierarchy"); } }
//Resources:ManufacturingResources:FeedDirection_Backwards

		public static string FeedDirection_Backwards { get { return GetResourceString("FeedDirection_Backwards"); } }
//Resources:ManufacturingResources:FeedDirection_Forwards

		public static string FeedDirection_Forwards { get { return GetResourceString("FeedDirection_Forwards"); } }
//Resources:ManufacturingResources:Feeder_AdvanceGCode

		public static string Feeder_AdvanceGCode { get { return GetResourceString("Feeder_AdvanceGCode"); } }
//Resources:ManufacturingResources:Feeder_AdvanceGCode_Help

		public static string Feeder_AdvanceGCode_Help( string slot, string feederid) { return GetResourceString("Feeder_AdvanceGCode_Help", "{slot}", slot, "{feederid}", feederid); }
//Resources:ManufacturingResources:Feeder_Component_Select

		public static string Feeder_Component_Select { get { return GetResourceString("Feeder_Component_Select"); } }
//Resources:ManufacturingResources:Feeder_FeederHeight_Help

		public static string Feeder_FeederHeight_Help { get { return GetResourceString("Feeder_FeederHeight_Help"); } }
//Resources:ManufacturingResources:Feeder_FeederId

		public static string Feeder_FeederId { get { return GetResourceString("Feeder_FeederId"); } }
//Resources:ManufacturingResources:Feeder_FeederId_Help

		public static string Feeder_FeederId_Help { get { return GetResourceString("Feeder_FeederId_Help"); } }
//Resources:ManufacturingResources:Feeder_FeederLength_Help

		public static string Feeder_FeederLength_Help { get { return GetResourceString("Feeder_FeederLength_Help"); } }
//Resources:ManufacturingResources:Feeder_FeederWidth_Help

		public static string Feeder_FeederWidth_Help { get { return GetResourceString("Feeder_FeederWidth_Help"); } }
//Resources:ManufacturingResources:Feeder_FiducialOffsetFromSlotOriign

		public static string Feeder_FiducialOffsetFromSlotOriign { get { return GetResourceString("Feeder_FiducialOffsetFromSlotOriign"); } }
//Resources:ManufacturingResources:Feeder_FiducialOffsetFromSlotOriign_Help

		public static string Feeder_FiducialOffsetFromSlotOriign_Help { get { return GetResourceString("Feeder_FiducialOffsetFromSlotOriign_Help"); } }
//Resources:ManufacturingResources:Feeder_Machine

		public static string Feeder_Machine { get { return GetResourceString("Feeder_Machine"); } }
//Resources:ManufacturingResources:Feeder_Machine_Select

		public static string Feeder_Machine_Select { get { return GetResourceString("Feeder_Machine_Select"); } }
//Resources:ManufacturingResources:Feeder_OriginalTemplate

		public static string Feeder_OriginalTemplate { get { return GetResourceString("Feeder_OriginalTemplate"); } }
//Resources:ManufacturingResources:Feeder_OriginOffset

		public static string Feeder_OriginOffset { get { return GetResourceString("Feeder_OriginOffset"); } }
//Resources:ManufacturingResources:Feeder_OriginOffset_Help

		public static string Feeder_OriginOffset_Help { get { return GetResourceString("Feeder_OriginOffset_Help"); } }
//Resources:ManufacturingResources:Feeder_PartCount

		public static string Feeder_PartCount { get { return GetResourceString("Feeder_PartCount"); } }
//Resources:ManufacturingResources:Feeder_PickHeight

		public static string Feeder_PickHeight { get { return GetResourceString("Feeder_PickHeight"); } }
//Resources:ManufacturingResources:Feeder_PickLocation

		public static string Feeder_PickLocation { get { return GetResourceString("Feeder_PickLocation"); } }
//Resources:ManufacturingResources:Feeder_PickOffsetFromFiducial

		public static string Feeder_PickOffsetFromFiducial { get { return GetResourceString("Feeder_PickOffsetFromFiducial"); } }
//Resources:ManufacturingResources:Feeder_PickOffsetFromSlotOrigin_Help

		public static string Feeder_PickOffsetFromSlotOrigin_Help { get { return GetResourceString("Feeder_PickOffsetFromSlotOrigin_Help"); } }
//Resources:ManufacturingResources:Feeder_Protocol

		public static string Feeder_Protocol { get { return GetResourceString("Feeder_Protocol"); } }
//Resources:ManufacturingResources:Feeder_Protocol_Select

		public static string Feeder_Protocol_Select { get { return GetResourceString("Feeder_Protocol_Select"); } }
//Resources:ManufacturingResources:Feeder_Rotation

		public static string Feeder_Rotation { get { return GetResourceString("Feeder_Rotation"); } }
//Resources:ManufacturingResources:Feeder_Rotation_Select

		public static string Feeder_Rotation_Select { get { return GetResourceString("Feeder_Rotation_Select"); } }
//Resources:ManufacturingResources:Feeder_Size

		public static string Feeder_Size { get { return GetResourceString("Feeder_Size"); } }
//Resources:ManufacturingResources:Feeder_Slot

		public static string Feeder_Slot { get { return GetResourceString("Feeder_Slot"); } }
//Resources:ManufacturingResources:Feeder_TapeAngle

		public static string Feeder_TapeAngle { get { return GetResourceString("Feeder_TapeAngle"); } }
//Resources:ManufacturingResources:FeederOrientation_Horizontal

		public static string FeederOrientation_Horizontal { get { return GetResourceString("FeederOrientation_Horizontal"); } }
//Resources:ManufacturingResources:FeederOrientation_Vertical

		public static string FeederOrientation_Vertical { get { return GetResourceString("FeederOrientation_Vertical"); } }
//Resources:ManufacturingResources:FeederProtocol_Other

		public static string FeederProtocol_Other { get { return GetResourceString("FeederProtocol_Other"); } }
//Resources:ManufacturingResources:FeederProtocol_Photon

		public static string FeederProtocol_Photon { get { return GetResourceString("FeederProtocol_Photon"); } }
//Resources:ManufacturingResources:FeederRotation0

		public static string FeederRotation0 { get { return GetResourceString("FeederRotation0"); } }
//Resources:ManufacturingResources:FeederRotation180

		public static string FeederRotation180 { get { return GetResourceString("FeederRotation180"); } }
//Resources:ManufacturingResources:FeederRotation90

		public static string FeederRotation90 { get { return GetResourceString("FeederRotation90"); } }
//Resources:ManufacturingResources:FeederRotationMinus90

		public static string FeederRotationMinus90 { get { return GetResourceString("FeederRotationMinus90"); } }
//Resources:ManufacturingResources:Feeders_Title

		public static string Feeders_Title { get { return GetResourceString("Feeders_Title"); } }
//Resources:ManufacturingResources:GCode_BottomLightOff

		public static string GCode_BottomLightOff { get { return GetResourceString("GCode_BottomLightOff"); } }
//Resources:ManufacturingResources:GCode_BottomLightOn

		public static string GCode_BottomLightOn { get { return GetResourceString("GCode_BottomLightOn"); } }
//Resources:ManufacturingResources:GCode_Description

		public static string GCode_Description { get { return GetResourceString("GCode_Description"); } }
//Resources:ManufacturingResources:GCode_Dwell

		public static string GCode_Dwell { get { return GetResourceString("GCode_Dwell"); } }
//Resources:ManufacturingResources:GCode_EmergencyStop

		public static string GCode_EmergencyStop { get { return GetResourceString("GCode_EmergencyStop"); } }
//Resources:ManufacturingResources:GCode_Exchaust1_Off

		public static string GCode_Exchaust1_Off { get { return GetResourceString("GCode_Exchaust1_Off"); } }
//Resources:ManufacturingResources:GCode_Exchaust1_On

		public static string GCode_Exchaust1_On { get { return GetResourceString("GCode_Exchaust1_On"); } }
//Resources:ManufacturingResources:GCode_Exchaust2_Off

		public static string GCode_Exchaust2_Off { get { return GetResourceString("GCode_Exchaust2_Off"); } }
//Resources:ManufacturingResources:GCode_Exchaust2_On

		public static string GCode_Exchaust2_On { get { return GetResourceString("GCode_Exchaust2_On"); } }
//Resources:ManufacturingResources:GCode_HomeAllCommand

		public static string GCode_HomeAllCommand { get { return GetResourceString("GCode_HomeAllCommand"); } }
//Resources:ManufacturingResources:GCode_HomeXCommand

		public static string GCode_HomeXCommand { get { return GetResourceString("GCode_HomeXCommand"); } }
//Resources:ManufacturingResources:GCode_HomeYCommand

		public static string GCode_HomeYCommand { get { return GetResourceString("GCode_HomeYCommand"); } }
//Resources:ManufacturingResources:GCode_HomeZCommand

		public static string GCode_HomeZCommand { get { return GetResourceString("GCode_HomeZCommand"); } }
//Resources:ManufacturingResources:GCode_Laser_Off

		public static string GCode_Laser_Off { get { return GetResourceString("GCode_Laser_Off"); } }
//Resources:ManufacturingResources:GCode_Laser_On

		public static string GCode_Laser_On { get { return GetResourceString("GCode_Laser_On"); } }
//Resources:ManufacturingResources:GCode_LeftVacuumOff

		public static string GCode_LeftVacuumOff { get { return GetResourceString("GCode_LeftVacuumOff"); } }
//Resources:ManufacturingResources:GCode_LeftVacuumOn

		public static string GCode_LeftVacuumOn { get { return GetResourceString("GCode_LeftVacuumOn"); } }
//Resources:ManufacturingResources:GCode_LeftVacuumResponseExample

		public static string GCode_LeftVacuumResponseExample { get { return GetResourceString("GCode_LeftVacuumResponseExample"); } }
//Resources:ManufacturingResources:GCode_ParseLeftVacuumRegEx

		public static string GCode_ParseLeftVacuumRegEx { get { return GetResourceString("GCode_ParseLeftVacuumRegEx"); } }
//Resources:ManufacturingResources:GCode_ParseRightVacuumRegEx

		public static string GCode_ParseRightVacuumRegEx { get { return GetResourceString("GCode_ParseRightVacuumRegEx"); } }
//Resources:ManufacturingResources:GCode_ParseStatus_RegularExpression

		public static string GCode_ParseStatus_RegularExpression { get { return GetResourceString("GCode_ParseStatus_RegularExpression"); } }
//Resources:ManufacturingResources:GCode_ParseStatusRegularExpressionHelp

		public static string GCode_ParseStatusRegularExpressionHelp { get { return GetResourceString("GCode_ParseStatusRegularExpressionHelp"); } }
//Resources:ManufacturingResources:GCode_ReadLeftVacuumCmd

		public static string GCode_ReadLeftVacuumCmd { get { return GetResourceString("GCode_ReadLeftVacuumCmd"); } }
//Resources:ManufacturingResources:GCode_ReadRightVacuumCmd

		public static string GCode_ReadRightVacuumCmd { get { return GetResourceString("GCode_ReadRightVacuumCmd"); } }
//Resources:ManufacturingResources:GCode_RequestStatusCommand

		public static string GCode_RequestStatusCommand { get { return GetResourceString("GCode_RequestStatusCommand"); } }
//Resources:ManufacturingResources:GCode_RightVacuumOff

		public static string GCode_RightVacuumOff { get { return GetResourceString("GCode_RightVacuumOff"); } }
//Resources:ManufacturingResources:GCode_RightVacuumOn

		public static string GCode_RightVacuumOn { get { return GetResourceString("GCode_RightVacuumOn"); } }
//Resources:ManufacturingResources:GCode_RightVacuumResponseExample

		public static string GCode_RightVacuumResponseExample { get { return GetResourceString("GCode_RightVacuumResponseExample"); } }
//Resources:ManufacturingResources:GCode_SpindleOff

		public static string GCode_SpindleOff { get { return GetResourceString("GCode_SpindleOff"); } }
//Resources:ManufacturingResources:GCode_SpindleOn

		public static string GCode_SpindleOn { get { return GetResourceString("GCode_SpindleOn"); } }
//Resources:ManufacturingResources:GCode_StatusResponseExample

		public static string GCode_StatusResponseExample { get { return GetResourceString("GCode_StatusResponseExample"); } }
//Resources:ManufacturingResources:GCode_ToolChange

		public static string GCode_ToolChange { get { return GetResourceString("GCode_ToolChange"); } }
//Resources:ManufacturingResources:GCode_TopLightOff

		public static string GCode_TopLightOff { get { return GetResourceString("GCode_TopLightOff"); } }
//Resources:ManufacturingResources:GCode_TopLightOn

		public static string GCode_TopLightOn { get { return GetResourceString("GCode_TopLightOn"); } }
//Resources:ManufacturingResources:GCodeMapping_Title

		public static string GCodeMapping_Title { get { return GetResourceString("GCodeMapping_Title"); } }
//Resources:ManufacturingResources:GCodeMappings_Title

		public static string GCodeMappings_Title { get { return GetResourceString("GCodeMappings_Title"); } }
//Resources:ManufacturingResources:JobPlacementStatus_Failed

		public static string JobPlacementStatus_Failed { get { return GetResourceString("JobPlacementStatus_Failed"); } }
//Resources:ManufacturingResources:JobPlacementStatus_InProcess

		public static string JobPlacementStatus_InProcess { get { return GetResourceString("JobPlacementStatus_InProcess"); } }
//Resources:ManufacturingResources:JobPlacementStatus_Pending

		public static string JobPlacementStatus_Pending { get { return GetResourceString("JobPlacementStatus_Pending"); } }
//Resources:ManufacturingResources:JobPlacementStatus_Placed

		public static string JobPlacementStatus_Placed { get { return GetResourceString("JobPlacementStatus_Placed"); } }
//Resources:ManufacturingResources:JobState_Aborted

		public static string JobState_Aborted { get { return GetResourceString("JobState_Aborted"); } }
//Resources:ManufacturingResources:JobState_Completed

		public static string JobState_Completed { get { return GetResourceString("JobState_Completed"); } }
//Resources:ManufacturingResources:JobState_Creted

		public static string JobState_Creted { get { return GetResourceString("JobState_Creted"); } }
//Resources:ManufacturingResources:JobState_Failed

		public static string JobState_Failed { get { return GetResourceString("JobState_Failed"); } }
//Resources:ManufacturingResources:JobState_Running

		public static string JobState_Running { get { return GetResourceString("JobState_Running"); } }
//Resources:ManufacturingResources:Machine_Cameras

		public static string Machine_Cameras { get { return GetResourceString("Machine_Cameras"); } }
//Resources:ManufacturingResources:Machine_ConnectToMQTT

		public static string Machine_ConnectToMQTT { get { return GetResourceString("Machine_ConnectToMQTT"); } }
//Resources:ManufacturingResources:Machine_Description

		public static string Machine_Description { get { return GetResourceString("Machine_Description"); } }
//Resources:ManufacturingResources:Machine_DeviceId

		public static string Machine_DeviceId { get { return GetResourceString("Machine_DeviceId"); } }
//Resources:ManufacturingResources:Machine_FeederRail_Description

		public static string Machine_FeederRail_Description { get { return GetResourceString("Machine_FeederRail_Description"); } }
//Resources:ManufacturingResources:Machine_FeederRail_FirstFeederOrigin

		public static string Machine_FeederRail_FirstFeederOrigin { get { return GetResourceString("Machine_FeederRail_FirstFeederOrigin"); } }
//Resources:ManufacturingResources:Machine_FeederRail_FirstFeederOrigin_Help

		public static string Machine_FeederRail_FirstFeederOrigin_Help { get { return GetResourceString("Machine_FeederRail_FirstFeederOrigin_Help"); } }
//Resources:ManufacturingResources:Machine_FeederRail_Height

		public static string Machine_FeederRail_Height { get { return GetResourceString("Machine_FeederRail_Height"); } }
//Resources:ManufacturingResources:Machine_FeederRail_NumberSlots

		public static string Machine_FeederRail_NumberSlots { get { return GetResourceString("Machine_FeederRail_NumberSlots"); } }
//Resources:ManufacturingResources:Machine_FeederRail_SlotWidth

		public static string Machine_FeederRail_SlotWidth { get { return GetResourceString("Machine_FeederRail_SlotWidth"); } }
//Resources:ManufacturingResources:Machine_FeederRail_StartSlotIndex

		public static string Machine_FeederRail_StartSlotIndex { get { return GetResourceString("Machine_FeederRail_StartSlotIndex"); } }
//Resources:ManufacturingResources:Machine_FeederRail_StartSlotIndex_Help

		public static string Machine_FeederRail_StartSlotIndex_Help { get { return GetResourceString("Machine_FeederRail_StartSlotIndex_Help"); } }
//Resources:ManufacturingResources:Machine_FeederRail_Title

		public static string Machine_FeederRail_Title { get { return GetResourceString("Machine_FeederRail_Title"); } }
//Resources:ManufacturingResources:Machine_FeederRail_Width

		public static string Machine_FeederRail_Width { get { return GetResourceString("Machine_FeederRail_Width"); } }
//Resources:ManufacturingResources:Machine_FeederRails

		public static string Machine_FeederRails { get { return GetResourceString("Machine_FeederRails"); } }
//Resources:ManufacturingResources:Machine_FrameSize

		public static string Machine_FrameSize { get { return GetResourceString("Machine_FrameSize"); } }
//Resources:ManufacturingResources:Machine_FrameSize_Help

		public static string Machine_FrameSize_Help { get { return GetResourceString("Machine_FrameSize_Help"); } }
//Resources:ManufacturingResources:Machine_GCode

		public static string Machine_GCode { get { return GetResourceString("Machine_GCode"); } }
//Resources:ManufacturingResources:Machine_GCode_Description

		public static string Machine_GCode_Description { get { return GetResourceString("Machine_GCode_Description"); } }
//Resources:ManufacturingResources:Machine_GCodeMapping

		public static string Machine_GCodeMapping { get { return GetResourceString("Machine_GCodeMapping"); } }
//Resources:ManufacturingResources:Machine_GCodeMapping_Select

		public static string Machine_GCodeMapping_Select { get { return GetResourceString("Machine_GCodeMapping_Select"); } }
//Resources:ManufacturingResources:Machine_HostName

		public static string Machine_HostName { get { return GetResourceString("Machine_HostName"); } }
//Resources:ManufacturingResources:Machine_JogFeedRate

		public static string Machine_JogFeedRate { get { return GetResourceString("Machine_JogFeedRate"); } }
//Resources:ManufacturingResources:Machine_MaxFeedRate

		public static string Machine_MaxFeedRate { get { return GetResourceString("Machine_MaxFeedRate"); } }
//Resources:ManufacturingResources:Machine_Password

		public static string Machine_Password { get { return GetResourceString("Machine_Password"); } }
//Resources:ManufacturingResources:Machine_Port

		public static string Machine_Port { get { return GetResourceString("Machine_Port"); } }
//Resources:ManufacturingResources:Machine_SecureConnection

		public static string Machine_SecureConnection { get { return GetResourceString("Machine_SecureConnection"); } }
//Resources:ManufacturingResources:Machine_Title

		public static string Machine_Title { get { return GetResourceString("Machine_Title"); } }
//Resources:ManufacturingResources:Machine_ToolHeads

		public static string Machine_ToolHeads { get { return GetResourceString("Machine_ToolHeads"); } }
//Resources:ManufacturingResources:Machine_UserName

		public static string Machine_UserName { get { return GetResourceString("Machine_UserName"); } }
//Resources:ManufacturingResources:Machine_WorkAreaOrigin

		public static string Machine_WorkAreaOrigin { get { return GetResourceString("Machine_WorkAreaOrigin"); } }
//Resources:ManufacturingResources:Machine_WorkAreaOrigin_Help

		public static string Machine_WorkAreaOrigin_Help { get { return GetResourceString("Machine_WorkAreaOrigin_Help"); } }
//Resources:ManufacturingResources:Machine_WorkAreaSize

		public static string Machine_WorkAreaSize { get { return GetResourceString("Machine_WorkAreaSize"); } }
//Resources:ManufacturingResources:Machine_WorkAreaSize_Help

		public static string Machine_WorkAreaSize_Help { get { return GetResourceString("Machine_WorkAreaSize_Help"); } }
//Resources:ManufacturingResources:Machine_WorkspaceFrameOffset

		public static string Machine_WorkspaceFrameOffset { get { return GetResourceString("Machine_WorkspaceFrameOffset"); } }
//Resources:ManufacturingResources:Machine_WorkspaceFrameOffset_Help

		public static string Machine_WorkspaceFrameOffset_Help { get { return GetResourceString("Machine_WorkspaceFrameOffset_Help"); } }
//Resources:ManufacturingResources:MachineCamera_AbsolutePosition

		public static string MachineCamera_AbsolutePosition { get { return GetResourceString("MachineCamera_AbsolutePosition"); } }
//Resources:ManufacturingResources:MachineCamera_Description

		public static string MachineCamera_Description { get { return GetResourceString("MachineCamera_Description"); } }
//Resources:ManufacturingResources:MachineCamera_DeviceId

		public static string MachineCamera_DeviceId { get { return GetResourceString("MachineCamera_DeviceId"); } }
//Resources:ManufacturingResources:MachineCamera_FocusHeight

		public static string MachineCamera_FocusHeight { get { return GetResourceString("MachineCamera_FocusHeight"); } }
//Resources:ManufacturingResources:MachineCamera_ImageSize

		public static string MachineCamera_ImageSize { get { return GetResourceString("MachineCamera_ImageSize"); } }
//Resources:ManufacturingResources:MachineCamera_MirrorX

		public static string MachineCamera_MirrorX { get { return GetResourceString("MachineCamera_MirrorX"); } }
//Resources:ManufacturingResources:MachineCamera_MirrorY

		public static string MachineCamera_MirrorY { get { return GetResourceString("MachineCamera_MirrorY"); } }
//Resources:ManufacturingResources:MachineCamera_PixelsPerMM

		public static string MachineCamera_PixelsPerMM { get { return GetResourceString("MachineCamera_PixelsPerMM"); } }
//Resources:ManufacturingResources:MachineCamera_PixelsPerMM_Help

		public static string MachineCamera_PixelsPerMM_Help { get { return GetResourceString("MachineCamera_PixelsPerMM_Help"); } }
//Resources:ManufacturingResources:MachineCamera_SupportZoom

		public static string MachineCamera_SupportZoom { get { return GetResourceString("MachineCamera_SupportZoom"); } }
//Resources:ManufacturingResources:MachineCamera_Title

		public static string MachineCamera_Title { get { return GetResourceString("MachineCamera_Title"); } }
//Resources:ManufacturingResources:MachineCamera_Type

		public static string MachineCamera_Type { get { return GetResourceString("MachineCamera_Type"); } }
//Resources:ManufacturingResources:MachineCamera_Type_Observation

		public static string MachineCamera_Type_Observation { get { return GetResourceString("MachineCamera_Type_Observation"); } }
//Resources:ManufacturingResources:MachineCamera_Type_PartInspection

		public static string MachineCamera_Type_PartInspection { get { return GetResourceString("MachineCamera_Type_PartInspection"); } }
//Resources:ManufacturingResources:MachineCamera_Type_Position

		public static string MachineCamera_Type_Position { get { return GetResourceString("MachineCamera_Type_Position"); } }
//Resources:ManufacturingResources:MachineCamera_Type_Select

		public static string MachineCamera_Type_Select { get { return GetResourceString("MachineCamera_Type_Select"); } }
//Resources:ManufacturingResources:Machines_Title

		public static string Machines_Title { get { return GetResourceString("Machines_Title"); } }
//Resources:ManufacturingResources:MachineStagingPlate_Description

		public static string MachineStagingPlate_Description { get { return GetResourceString("MachineStagingPlate_Description"); } }
//Resources:ManufacturingResources:MachineStagingPlate_FirstHole

		public static string MachineStagingPlate_FirstHole { get { return GetResourceString("MachineStagingPlate_FirstHole"); } }
//Resources:ManufacturingResources:MachineStagingPlate_FirstHole_Help

		public static string MachineStagingPlate_FirstHole_Help { get { return GetResourceString("MachineStagingPlate_FirstHole_Help"); } }
//Resources:ManufacturingResources:MachineStagingPlate_FirstRowOddOnly

		public static string MachineStagingPlate_FirstRowOddOnly { get { return GetResourceString("MachineStagingPlate_FirstRowOddOnly"); } }
//Resources:ManufacturingResources:MachineStagingPlate_FirstRowOddOnly_Help

		public static string MachineStagingPlate_FirstRowOddOnly_Help { get { return GetResourceString("MachineStagingPlate_FirstRowOddOnly_Help"); } }
//Resources:ManufacturingResources:MachineStagingPlate_FirstUsableColumn

		public static string MachineStagingPlate_FirstUsableColumn { get { return GetResourceString("MachineStagingPlate_FirstUsableColumn"); } }
//Resources:ManufacturingResources:MachineStagingPlate_FirstUsableColumn_Help

		public static string MachineStagingPlate_FirstUsableColumn_Help { get { return GetResourceString("MachineStagingPlate_FirstUsableColumn_Help"); } }
//Resources:ManufacturingResources:MachineStagingPlate_HoleSpacing

		public static string MachineStagingPlate_HoleSpacing { get { return GetResourceString("MachineStagingPlate_HoleSpacing"); } }
//Resources:ManufacturingResources:MachineStagingPlate_HolesStaggered

		public static string MachineStagingPlate_HolesStaggered { get { return GetResourceString("MachineStagingPlate_HolesStaggered"); } }
//Resources:ManufacturingResources:MachineStagingPlate_HolesStaggered_Help

		public static string MachineStagingPlate_HolesStaggered_Help { get { return GetResourceString("MachineStagingPlate_HolesStaggered_Help"); } }
//Resources:ManufacturingResources:MachineStagingPlate_LastUsableColumn

		public static string MachineStagingPlate_LastUsableColumn { get { return GetResourceString("MachineStagingPlate_LastUsableColumn"); } }
//Resources:ManufacturingResources:MachineStagingPlate_LastUsableColumn_Help

		public static string MachineStagingPlate_LastUsableColumn_Help { get { return GetResourceString("MachineStagingPlate_LastUsableColumn_Help"); } }
//Resources:ManufacturingResources:MachineStagingPlate_Origin_Help

		public static string MachineStagingPlate_Origin_Help { get { return GetResourceString("MachineStagingPlate_Origin_Help"); } }
//Resources:ManufacturingResources:MachineStagingPlate_Origin_Origin_Approx

		public static string MachineStagingPlate_Origin_Origin_Approx { get { return GetResourceString("MachineStagingPlate_Origin_Origin_Approx"); } }
//Resources:ManufacturingResources:MachineStagingPlate_ReferenceHoleCol_Help

		public static string MachineStagingPlate_ReferenceHoleCol_Help { get { return GetResourceString("MachineStagingPlate_ReferenceHoleCol_Help"); } }
//Resources:ManufacturingResources:MachineStagingPlate_ReferenceHoleCol1

		public static string MachineStagingPlate_ReferenceHoleCol1 { get { return GetResourceString("MachineStagingPlate_ReferenceHoleCol1"); } }
//Resources:ManufacturingResources:MachineStagingPlate_ReferenceHoleCol2

		public static string MachineStagingPlate_ReferenceHoleCol2 { get { return GetResourceString("MachineStagingPlate_ReferenceHoleCol2"); } }
//Resources:ManufacturingResources:MachineStagingPlate_ReferenceHoleLocation_Help

		public static string MachineStagingPlate_ReferenceHoleLocation_Help { get { return GetResourceString("MachineStagingPlate_ReferenceHoleLocation_Help"); } }
//Resources:ManufacturingResources:MachineStagingPlate_ReferenceHoleLocation1

		public static string MachineStagingPlate_ReferenceHoleLocation1 { get { return GetResourceString("MachineStagingPlate_ReferenceHoleLocation1"); } }
//Resources:ManufacturingResources:MachineStagingPlate_ReferenceHoleLocation2

		public static string MachineStagingPlate_ReferenceHoleLocation2 { get { return GetResourceString("MachineStagingPlate_ReferenceHoleLocation2"); } }
//Resources:ManufacturingResources:MachineStagingPlate_ReferenceHoleRow_Help

		public static string MachineStagingPlate_ReferenceHoleRow_Help { get { return GetResourceString("MachineStagingPlate_ReferenceHoleRow_Help"); } }
//Resources:ManufacturingResources:MachineStagingPlate_ReferenceHoleRow1

		public static string MachineStagingPlate_ReferenceHoleRow1 { get { return GetResourceString("MachineStagingPlate_ReferenceHoleRow1"); } }
//Resources:ManufacturingResources:MachineStagingPlate_ReferenceHoleRow2

		public static string MachineStagingPlate_ReferenceHoleRow2 { get { return GetResourceString("MachineStagingPlate_ReferenceHoleRow2"); } }
//Resources:ManufacturingResources:MachineStagingPlate_Title

		public static string MachineStagingPlate_Title { get { return GetResourceString("MachineStagingPlate_Title"); } }
//Resources:ManufacturingResources:MachineStagingPlates_Title

		public static string MachineStagingPlates_Title { get { return GetResourceString("MachineStagingPlates_Title"); } }
//Resources:ManufacturingResources:MachineToolHead_CurrentNozzle

		public static string MachineToolHead_CurrentNozzle { get { return GetResourceString("MachineToolHead_CurrentNozzle"); } }
//Resources:ManufacturingResources:MachineToolHead_CurrentNozzle_Select

		public static string MachineToolHead_CurrentNozzle_Select { get { return GetResourceString("MachineToolHead_CurrentNozzle_Select"); } }
//Resources:ManufacturingResources:MachineToolHead_DefaultOriginPosition

		public static string MachineToolHead_DefaultOriginPosition { get { return GetResourceString("MachineToolHead_DefaultOriginPosition"); } }
//Resources:ManufacturingResources:MachineToolHead_DefaultOriginPosition_Help

		public static string MachineToolHead_DefaultOriginPosition_Help { get { return GetResourceString("MachineToolHead_DefaultOriginPosition_Help"); } }
//Resources:ManufacturingResources:MachineToolHead_Description

		public static string MachineToolHead_Description { get { return GetResourceString("MachineToolHead_Description"); } }
//Resources:ManufacturingResources:MachineToolHead_HeadIndex

		public static string MachineToolHead_HeadIndex { get { return GetResourceString("MachineToolHead_HeadIndex"); } }
//Resources:ManufacturingResources:MachineToolHead_HeadIndex_Help

		public static string MachineToolHead_HeadIndex_Help { get { return GetResourceString("MachineToolHead_HeadIndex_Help"); } }
//Resources:ManufacturingResources:MachineToolHead_IdleVacuum

		public static string MachineToolHead_IdleVacuum { get { return GetResourceString("MachineToolHead_IdleVacuum"); } }
//Resources:ManufacturingResources:MachineToolHead_IdleVacuum_Help

		public static string MachineToolHead_IdleVacuum_Help { get { return GetResourceString("MachineToolHead_IdleVacuum_Help"); } }
//Resources:ManufacturingResources:MachineToolHead_NoPartVacuum

		public static string MachineToolHead_NoPartVacuum { get { return GetResourceString("MachineToolHead_NoPartVacuum"); } }
//Resources:ManufacturingResources:MachineToolHead_NoPartVacuum_Help

		public static string MachineToolHead_NoPartVacuum_Help { get { return GetResourceString("MachineToolHead_NoPartVacuum_Help"); } }
//Resources:ManufacturingResources:MachineToolHead_PartPresentVacuum

		public static string MachineToolHead_PartPresentVacuum { get { return GetResourceString("MachineToolHead_PartPresentVacuum"); } }
//Resources:ManufacturingResources:MachineToolHead_PartPresentVacuum_Help

		public static string MachineToolHead_PartPresentVacuum_Help { get { return GetResourceString("MachineToolHead_PartPresentVacuum_Help"); } }
//Resources:ManufacturingResources:MachineToolHead_PickHeight

		public static string MachineToolHead_PickHeight { get { return GetResourceString("MachineToolHead_PickHeight"); } }
//Resources:ManufacturingResources:MachineToolHead_PickHeight_Help

		public static string MachineToolHead_PickHeight_Help { get { return GetResourceString("MachineToolHead_PickHeight_Help"); } }
//Resources:ManufacturingResources:MachineToolHead_PlaceHeight

		public static string MachineToolHead_PlaceHeight { get { return GetResourceString("MachineToolHead_PlaceHeight"); } }
//Resources:ManufacturingResources:MachineToolHead_PlaceHight_Help

		public static string MachineToolHead_PlaceHight_Help { get { return GetResourceString("MachineToolHead_PlaceHight_Help"); } }
//Resources:ManufacturingResources:MachineToolHead_Title

		public static string MachineToolHead_Title { get { return GetResourceString("MachineToolHead_Title"); } }
//Resources:ManufacturingResources:MachineToolHead_Type

		public static string MachineToolHead_Type { get { return GetResourceString("MachineToolHead_Type"); } }
//Resources:ManufacturingResources:MachineToolHead_Type_Laser

		public static string MachineToolHead_Type_Laser { get { return GetResourceString("MachineToolHead_Type_Laser"); } }
//Resources:ManufacturingResources:MachineToolHead_Type_PartNozzle

		public static string MachineToolHead_Type_PartNozzle { get { return GetResourceString("MachineToolHead_Type_PartNozzle"); } }
//Resources:ManufacturingResources:MachineToolHead_Type_Select

		public static string MachineToolHead_Type_Select { get { return GetResourceString("MachineToolHead_Type_Select"); } }
//Resources:ManufacturingResources:MachineToolHead_Type_Spindle

		public static string MachineToolHead_Type_Spindle { get { return GetResourceString("MachineToolHead_Type_Spindle"); } }
//Resources:ManufacturingResources:MachineToolHead_VacuumTolerancePercent

		public static string MachineToolHead_VacuumTolerancePercent { get { return GetResourceString("MachineToolHead_VacuumTolerancePercent"); } }
//Resources:ManufacturingResources:MachineToolHead_VacuumTolerancePercent_Help

		public static string MachineToolHead_VacuumTolerancePercent_Help { get { return GetResourceString("MachineToolHead_VacuumTolerancePercent_Help"); } }
//Resources:ManufacturingResources:NozzleTip_Description

		public static string NozzleTip_Description { get { return GetResourceString("NozzleTip_Description"); } }
//Resources:ManufacturingResources:NozzleTip_Height

		public static string NozzleTip_Height { get { return GetResourceString("NozzleTip_Height"); } }
//Resources:ManufacturingResources:NozzleTip_IdleVacuum

		public static string NozzleTip_IdleVacuum { get { return GetResourceString("NozzleTip_IdleVacuum"); } }
//Resources:ManufacturingResources:NozzleTip_InnerDiameter

		public static string NozzleTip_InnerDiameter { get { return GetResourceString("NozzleTip_InnerDiameter"); } }
//Resources:ManufacturingResources:NozzleTip_OuterDiameter

		public static string NozzleTip_OuterDiameter { get { return GetResourceString("NozzleTip_OuterDiameter"); } }
//Resources:ManufacturingResources:NozzleTip_PartPickedVacuum

		public static string NozzleTip_PartPickedVacuum { get { return GetResourceString("NozzleTip_PartPickedVacuum"); } }
//Resources:ManufacturingResources:NozzleTip_Title

		public static string NozzleTip_Title { get { return GetResourceString("NozzleTip_Title"); } }
//Resources:ManufacturingResources:NozzleTip_ToolRackLocation

		public static string NozzleTip_ToolRackLocation { get { return GetResourceString("NozzleTip_ToolRackLocation"); } }
//Resources:ManufacturingResources:NozzleTips_Title

		public static string NozzleTips_Title { get { return GetResourceString("NozzleTips_Title"); } }
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
//Resources:ManufacturingResources:PickAndPlaceJob_Board

		public static string PickAndPlaceJob_Board { get { return GetResourceString("PickAndPlaceJob_Board"); } }
//Resources:ManufacturingResources:PickAndPlaceJob_Cost

		public static string PickAndPlaceJob_Cost { get { return GetResourceString("PickAndPlaceJob_Cost"); } }
//Resources:ManufacturingResources:PickAndPlaceJob_CurrentSerialNumber

		public static string PickAndPlaceJob_CurrentSerialNumber { get { return GetResourceString("PickAndPlaceJob_CurrentSerialNumber"); } }
//Resources:ManufacturingResources:PickAndPlaceJob_Description

		public static string PickAndPlaceJob_Description { get { return GetResourceString("PickAndPlaceJob_Description"); } }
//Resources:ManufacturingResources:PickAndPlaceJob_Extneded

		public static string PickAndPlaceJob_Extneded { get { return GetResourceString("PickAndPlaceJob_Extneded"); } }
//Resources:ManufacturingResources:PickAndPlaceJob_Title

		public static string PickAndPlaceJob_Title { get { return GetResourceString("PickAndPlaceJob_Title"); } }
//Resources:ManufacturingResources:PickAndPlaceJobRun_Description

		public static string PickAndPlaceJobRun_Description { get { return GetResourceString("PickAndPlaceJobRun_Description"); } }
//Resources:ManufacturingResources:PickAndPlaceJobRun_Title

		public static string PickAndPlaceJobRun_Title { get { return GetResourceString("PickAndPlaceJobRun_Title"); } }
//Resources:ManufacturingResources:PickAndPlaceJobRuns_Title

		public static string PickAndPlaceJobRuns_Title { get { return GetResourceString("PickAndPlaceJobRuns_Title"); } }
//Resources:ManufacturingResources:PickAndPlaceJobs_Title

		public static string PickAndPlaceJobs_Title { get { return GetResourceString("PickAndPlaceJobs_Title"); } }
//Resources:ManufacturingResources:PnPJobState_Aborted

		public static string PnPJobState_Aborted { get { return GetResourceString("PnPJobState_Aborted"); } }
//Resources:ManufacturingResources:PnPJobState_Completed

		public static string PnPJobState_Completed { get { return GetResourceString("PnPJobState_Completed"); } }
//Resources:ManufacturingResources:PnPJobState_Error

		public static string PnPJobState_Error { get { return GetResourceString("PnPJobState_Error"); } }
//Resources:ManufacturingResources:PnPJobState_Idle

		public static string PnPJobState_Idle { get { return GetResourceString("PnPJobState_Idle"); } }
//Resources:ManufacturingResources:PnPJobState_New

		public static string PnPJobState_New { get { return GetResourceString("PnPJobState_New"); } }
//Resources:ManufacturingResources:PnPJobState_Paused

		public static string PnPJobState_Paused { get { return GetResourceString("PnPJobState_Paused"); } }
//Resources:ManufacturingResources:PnPJobState_Running

		public static string PnPJobState_Running { get { return GetResourceString("PnPJobState_Running"); } }
//Resources:ManufacturingResources:PnPState_Advanced

		public static string PnPState_Advanced { get { return GetResourceString("PnPState_Advanced"); } }
//Resources:ManufacturingResources:PnpState_AtFeeder

		public static string PnpState_AtFeeder { get { return GetResourceString("PnpState_AtFeeder"); } }
//Resources:ManufacturingResources:PnPState_AtPlaceLocation

		public static string PnPState_AtPlaceLocation { get { return GetResourceString("PnPState_AtPlaceLocation"); } }
//Resources:ManufacturingResources:PnpState_DetectingPart

		public static string PnpState_DetectingPart { get { return GetResourceString("PnpState_DetectingPart"); } }
//Resources:ManufacturingResources:PnpState_Error

		public static string PnpState_Error { get { return GetResourceString("PnpState_Error"); } }
//Resources:ManufacturingResources:PnpState_FeederResolved

		public static string PnpState_FeederResolved { get { return GetResourceString("PnpState_FeederResolved"); } }
//Resources:ManufacturingResources:PnPState_Inspected

		public static string PnPState_Inspected { get { return GetResourceString("PnPState_Inspected"); } }
//Resources:ManufacturingResources:PnPState_Inspecting

		public static string PnPState_Inspecting { get { return GetResourceString("PnPState_Inspecting"); } }
//Resources:ManufacturingResources:PnPState_JobCompleted

		public static string PnPState_JobCompleted { get { return GetResourceString("PnPState_JobCompleted"); } }
//Resources:ManufacturingResources:PnPState_New

		public static string PnPState_New { get { return GetResourceString("PnPState_New"); } }
//Resources:ManufacturingResources:PnPState_OnBoard

		public static string PnPState_OnBoard { get { return GetResourceString("PnPState_OnBoard"); } }
//Resources:ManufacturingResources:PnpState_PartCenteredOnFeeder

		public static string PnpState_PartCenteredOnFeeder { get { return GetResourceString("PnpState_PartCenteredOnFeeder"); } }
//Resources:ManufacturingResources:PnPState_PartCompleted

		public static string PnPState_PartCompleted { get { return GetResourceString("PnPState_PartCompleted"); } }
//Resources:ManufacturingResources:PnpState_PartDetected

		public static string PnpState_PartDetected { get { return GetResourceString("PnpState_PartDetected"); } }
//Resources:ManufacturingResources:PnpState_PartNotDetected

		public static string PnpState_PartNotDetected { get { return GetResourceString("PnpState_PartNotDetected"); } }
//Resources:ManufacturingResources:PnpState_PartPicked

		public static string PnpState_PartPicked { get { return GetResourceString("PnpState_PartPicked"); } }
//Resources:ManufacturingResources:PnPState_PickErrorCompensated

		public static string PnPState_PickErrorCompensated { get { return GetResourceString("PnPState_PickErrorCompensated"); } }
//Resources:ManufacturingResources:PnPState_PickErrorCompensating

		public static string PnPState_PickErrorCompensating { get { return GetResourceString("PnPState_PickErrorCompensating"); } }
//Resources:ManufacturingResources:PnPState_PickErrorNotCompensated

		public static string PnPState_PickErrorNotCompensated { get { return GetResourceString("PnPState_PickErrorNotCompensated"); } }
//Resources:ManufacturingResources:PnPState_Placed

		public static string PnPState_Placed { get { return GetResourceString("PnPState_Placed"); } }
//Resources:ManufacturingResources:PnPState_PlacementCompleted

		public static string PnPState_PlacementCompleted { get { return GetResourceString("PnPState_PlacementCompleted"); } }
//Resources:ManufacturingResources:PnpState_Validated

		public static string PnpState_Validated { get { return GetResourceString("PnpState_Validated"); } }
//Resources:ManufacturingResources:StripFeeder_AngleOffset

		public static string StripFeeder_AngleOffset { get { return GetResourceString("StripFeeder_AngleOffset"); } }
//Resources:ManufacturingResources:StripFeeder_AngleOffset_Help

		public static string StripFeeder_AngleOffset_Help { get { return GetResourceString("StripFeeder_AngleOffset_Help"); } }
//Resources:ManufacturingResources:StripFeeder_CurrentPartindex

		public static string StripFeeder_CurrentPartindex { get { return GetResourceString("StripFeeder_CurrentPartindex"); } }
//Resources:ManufacturingResources:StripFeeder_Description

		public static string StripFeeder_Description { get { return GetResourceString("StripFeeder_Description"); } }
//Resources:ManufacturingResources:StripFeeder_Direction

		public static string StripFeeder_Direction { get { return GetResourceString("StripFeeder_Direction"); } }
//Resources:ManufacturingResources:StripFeeder_Direction_Select

		public static string StripFeeder_Direction_Select { get { return GetResourceString("StripFeeder_Direction_Select"); } }
//Resources:ManufacturingResources:StripFeeder_DualHoles

		public static string StripFeeder_DualHoles { get { return GetResourceString("StripFeeder_DualHoles"); } }
//Resources:ManufacturingResources:StripFeeder_DualHoles_Help

		public static string StripFeeder_DualHoles_Help { get { return GetResourceString("StripFeeder_DualHoles_Help"); } }
//Resources:ManufacturingResources:StripFeeder_FeederSize_Help

		public static string StripFeeder_FeederSize_Help { get { return GetResourceString("StripFeeder_FeederSize_Help"); } }
//Resources:ManufacturingResources:StripFeeder_Orientation

		public static string StripFeeder_Orientation { get { return GetResourceString("StripFeeder_Orientation"); } }
//Resources:ManufacturingResources:StripFeeder_Orientation_Select

		public static string StripFeeder_Orientation_Select { get { return GetResourceString("StripFeeder_Orientation_Select"); } }
//Resources:ManufacturingResources:StripFeeder_Origin_Help

		public static string StripFeeder_Origin_Help { get { return GetResourceString("StripFeeder_Origin_Help"); } }
//Resources:ManufacturingResources:StripFeeder_OriginOffset

		public static string StripFeeder_OriginOffset { get { return GetResourceString("StripFeeder_OriginOffset"); } }
//Resources:ManufacturingResources:StripFeeder_OriginOffset_Help

		public static string StripFeeder_OriginOffset_Help { get { return GetResourceString("StripFeeder_OriginOffset_Help"); } }
//Resources:ManufacturingResources:StripFeeder_PartCapacity

		public static string StripFeeder_PartCapacity { get { return GetResourceString("StripFeeder_PartCapacity"); } }
//Resources:ManufacturingResources:StripFeeder_PartCapacity_Help

		public static string StripFeeder_PartCapacity_Help { get { return GetResourceString("StripFeeder_PartCapacity_Help"); } }
//Resources:ManufacturingResources:StripFeeder_PickHeight

		public static string StripFeeder_PickHeight { get { return GetResourceString("StripFeeder_PickHeight"); } }
//Resources:ManufacturingResources:StripFeeder_PickHeight_Help

		public static string StripFeeder_PickHeight_Help { get { return GetResourceString("StripFeeder_PickHeight_Help"); } }
//Resources:ManufacturingResources:StripFeeder_ReferenceHole_Col

		public static string StripFeeder_ReferenceHole_Col { get { return GetResourceString("StripFeeder_ReferenceHole_Col"); } }
//Resources:ManufacturingResources:StripFeeder_ReferenceHole_Col_Help

		public static string StripFeeder_ReferenceHole_Col_Help { get { return GetResourceString("StripFeeder_ReferenceHole_Col_Help"); } }
//Resources:ManufacturingResources:StripFeeder_ReferenceHole_Row

		public static string StripFeeder_ReferenceHole_Row { get { return GetResourceString("StripFeeder_ReferenceHole_Row"); } }
//Resources:ManufacturingResources:StripFeeder_ReferenceHole_Row_Help

		public static string StripFeeder_ReferenceHole_Row_Help { get { return GetResourceString("StripFeeder_ReferenceHole_Row_Help"); } }
//Resources:ManufacturingResources:StripFeeder_ReferenceHoleOffset

		public static string StripFeeder_ReferenceHoleOffset { get { return GetResourceString("StripFeeder_ReferenceHoleOffset"); } }
//Resources:ManufacturingResources:StripFeeder_ReferenceHoleOffset_Help

		public static string StripFeeder_ReferenceHoleOffset_Help { get { return GetResourceString("StripFeeder_ReferenceHoleOffset_Help"); } }
//Resources:ManufacturingResources:StripFeeder_RowCount

		public static string StripFeeder_RowCount { get { return GetResourceString("StripFeeder_RowCount"); } }
//Resources:ManufacturingResources:StripFeeder_RowOneRefHoleOffset

		public static string StripFeeder_RowOneRefHoleOffset { get { return GetResourceString("StripFeeder_RowOneRefHoleOffset"); } }
//Resources:ManufacturingResources:StripFeeder_RowOneRefHoleOffset_Help

		public static string StripFeeder_RowOneRefHoleOffset_Help { get { return GetResourceString("StripFeeder_RowOneRefHoleOffset_Help"); } }
//Resources:ManufacturingResources:StripFeeder_Rows

		public static string StripFeeder_Rows { get { return GetResourceString("StripFeeder_Rows"); } }
//Resources:ManufacturingResources:StripFeeder_RowWidth

		public static string StripFeeder_RowWidth { get { return GetResourceString("StripFeeder_RowWidth"); } }
//Resources:ManufacturingResources:StripFeeder_RowWidth_Help

		public static string StripFeeder_RowWidth_Help { get { return GetResourceString("StripFeeder_RowWidth_Help"); } }
//Resources:ManufacturingResources:StripFeeder_StagingPlate

		public static string StripFeeder_StagingPlate { get { return GetResourceString("StripFeeder_StagingPlate"); } }
//Resources:ManufacturingResources:StripFeeder_StagingPlate_Select

		public static string StripFeeder_StagingPlate_Select { get { return GetResourceString("StripFeeder_StagingPlate_Select"); } }
//Resources:ManufacturingResources:StripFeeder_StagingPlateColumn

		public static string StripFeeder_StagingPlateColumn { get { return GetResourceString("StripFeeder_StagingPlateColumn"); } }
//Resources:ManufacturingResources:StripFeeder_TapeHolesOnTop

		public static string StripFeeder_TapeHolesOnTop { get { return GetResourceString("StripFeeder_TapeHolesOnTop"); } }
//Resources:ManufacturingResources:StripFeeder_TapeHolesOnTop_Help

		public static string StripFeeder_TapeHolesOnTop_Help { get { return GetResourceString("StripFeeder_TapeHolesOnTop_Help"); } }
//Resources:ManufacturingResources:StripFeeder_Title

		public static string StripFeeder_Title { get { return GetResourceString("StripFeeder_Title"); } }
//Resources:ManufacturingResources:StripFeederRow_CurrentPartIndex

		public static string StripFeederRow_CurrentPartIndex { get { return GetResourceString("StripFeederRow_CurrentPartIndex"); } }
//Resources:ManufacturingResources:StripFeederRow_Description

		public static string StripFeederRow_Description { get { return GetResourceString("StripFeederRow_Description"); } }
//Resources:ManufacturingResources:StripFeederRow_FirstTapeHoleOffset

		public static string StripFeederRow_FirstTapeHoleOffset { get { return GetResourceString("StripFeederRow_FirstTapeHoleOffset"); } }
//Resources:ManufacturingResources:StripFeederRow_FirstTapeHoleOffset_Help

		public static string StripFeederRow_FirstTapeHoleOffset_Help { get { return GetResourceString("StripFeederRow_FirstTapeHoleOffset_Help"); } }
//Resources:ManufacturingResources:StripFeederRow_LastTapeHoleOffset

		public static string StripFeederRow_LastTapeHoleOffset { get { return GetResourceString("StripFeederRow_LastTapeHoleOffset"); } }
//Resources:ManufacturingResources:StripFeederRow_LastTapeHoleOffset_Help

		public static string StripFeederRow_LastTapeHoleOffset_Help { get { return GetResourceString("StripFeederRow_LastTapeHoleOffset_Help"); } }
//Resources:ManufacturingResources:StripFeederRow_RowIndex

		public static string StripFeederRow_RowIndex { get { return GetResourceString("StripFeederRow_RowIndex"); } }
//Resources:ManufacturingResources:StripFeederRow_Title

		public static string StripFeederRow_Title { get { return GetResourceString("StripFeederRow_Title"); } }
//Resources:ManufacturingResources:StripFeederRowStatus_Empty

		public static string StripFeederRowStatus_Empty { get { return GetResourceString("StripFeederRowStatus_Empty"); } }
//Resources:ManufacturingResources:StripFeederRowStatus_None

		public static string StripFeederRowStatus_None { get { return GetResourceString("StripFeederRowStatus_None"); } }
//Resources:ManufacturingResources:StripFeederRowStatus_Planned

		public static string StripFeederRowStatus_Planned { get { return GetResourceString("StripFeederRowStatus_Planned"); } }
//Resources:ManufacturingResources:StripFeederRowStatus_Ready

		public static string StripFeederRowStatus_Ready { get { return GetResourceString("StripFeederRowStatus_Ready"); } }
//Resources:ManufacturingResources:StripFeeders_Title

		public static string StripFeeders_Title { get { return GetResourceString("StripFeeders_Title"); } }
//Resources:ManufacturingResources:StripFeederTemplate_Description

		public static string StripFeederTemplate_Description { get { return GetResourceString("StripFeederTemplate_Description"); } }
//Resources:ManufacturingResources:StripFeederTemplate_Title

		public static string StripFeederTemplate_Title { get { return GetResourceString("StripFeederTemplate_Title"); } }
//Resources:ManufacturingResources:StripFeederTemplates_Title

		public static string StripFeederTemplates_Title { get { return GetResourceString("StripFeederTemplates_Title"); } }
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
//Resources:ManufacturingResources:ToolNozzleTip_Description

		public static string ToolNozzleTip_Description { get { return GetResourceString("ToolNozzleTip_Description"); } }
//Resources:ManufacturingResources:ToolNozzleTip_Title

		public static string ToolNozzleTip_Title { get { return GetResourceString("ToolNozzleTip_Title"); } }
//Resources:ManufacturingResources:VisionProfile_BoardFiducial

		public static string VisionProfile_BoardFiducial { get { return GetResourceString("VisionProfile_BoardFiducial"); } }
//Resources:ManufacturingResources:VisionProfile_CountourRetrievealMode

		public static string VisionProfile_CountourRetrievealMode { get { return GetResourceString("VisionProfile_CountourRetrievealMode"); } }
//Resources:ManufacturingResources:VisionProfile_CountourRetrievealMode_Select

		public static string VisionProfile_CountourRetrievealMode_Select { get { return GetResourceString("VisionProfile_CountourRetrievealMode_Select"); } }
//Resources:ManufacturingResources:VisionProfile_Defauilt

		public static string VisionProfile_Defauilt { get { return GetResourceString("VisionProfile_Defauilt"); } }
//Resources:ManufacturingResources:VisionProfile_FeederFiducial

		public static string VisionProfile_FeederFiducial { get { return GetResourceString("VisionProfile_FeederFiducial"); } }
//Resources:ManufacturingResources:VisionProfile_FeederOrigin

		public static string VisionProfile_FeederOrigin { get { return GetResourceString("VisionProfile_FeederOrigin"); } }
//Resources:ManufacturingResources:VisionProfile_MachineFiducual

		public static string VisionProfile_MachineFiducual { get { return GetResourceString("VisionProfile_MachineFiducual"); } }
//Resources:ManufacturingResources:VisionProfile_Nozzle

		public static string VisionProfile_Nozzle { get { return GetResourceString("VisionProfile_Nozzle"); } }
//Resources:ManufacturingResources:VisionProfile_NozzleCalibration

		public static string VisionProfile_NozzleCalibration { get { return GetResourceString("VisionProfile_NozzleCalibration"); } }
//Resources:ManufacturingResources:VisionProfile_PartInBlackTape

		public static string VisionProfile_PartInBlackTape { get { return GetResourceString("VisionProfile_PartInBlackTape"); } }
//Resources:ManufacturingResources:VisionProfile_PartInClearTape

		public static string VisionProfile_PartInClearTape { get { return GetResourceString("VisionProfile_PartInClearTape"); } }
//Resources:ManufacturingResources:VisionProfile_PartInspection

		public static string VisionProfile_PartInspection { get { return GetResourceString("VisionProfile_PartInspection"); } }
//Resources:ManufacturingResources:VisionProfile_PartInWhiteTape

		public static string VisionProfile_PartInWhiteTape { get { return GetResourceString("VisionProfile_PartInWhiteTape"); } }
//Resources:ManufacturingResources:VisionProfile_PartOnBoard

		public static string VisionProfile_PartOnBoard { get { return GetResourceString("VisionProfile_PartOnBoard"); } }
//Resources:ManufacturingResources:VisionProfile_SquarePart

		public static string VisionProfile_SquarePart { get { return GetResourceString("VisionProfile_SquarePart"); } }
//Resources:ManufacturingResources:VisionProfile_StagingPlateHole

		public static string VisionProfile_StagingPlateHole { get { return GetResourceString("VisionProfile_StagingPlateHole"); } }
//Resources:ManufacturingResources:VisionProfile_TapeHole

		public static string VisionProfile_TapeHole { get { return GetResourceString("VisionProfile_TapeHole"); } }
//Resources:ManufacturingResources:VisionProfile_TapeHoleBlackTape

		public static string VisionProfile_TapeHoleBlackTape { get { return GetResourceString("VisionProfile_TapeHoleBlackTape"); } }
//Resources:ManufacturingResources:VisionProfile_TapeHoleClearTape

		public static string VisionProfile_TapeHoleClearTape { get { return GetResourceString("VisionProfile_TapeHoleClearTape"); } }
//Resources:ManufacturingResources:VisionProfile_TapeHoleWhiteTape

		public static string VisionProfile_TapeHoleWhiteTape { get { return GetResourceString("VisionProfile_TapeHoleWhiteTape"); } }

		public static class Names
		{
			public const string AssemblyInstruction_Description = "AssemblyInstruction_Description";
			public const string AssemblyInstruction_Steps = "AssemblyInstruction_Steps";
			public const string AssemblyInstruction_Title = "AssemblyInstruction_Title";
			public const string AssemblyInstructions_Title = "AssemblyInstructions_Title";
			public const string AssemblyInstructionStep_Description = "AssemblyInstructionStep_Description";
			public const string AssemblyInstructionStep_Images = "AssemblyInstructionStep_Images";
			public const string AssemblyInstructionStep_Title = "AssemblyInstructionStep_Title";
			public const string AssembyInstructionsStep_Instructions = "AssembyInstructionsStep_Instructions";
			public const string AutoFeeder_Description = "AutoFeeder_Description";
			public const string AutoFeeder_PickOffsetFromFiducial = "AutoFeeder_PickOffsetFromFiducial";
			public const string AutoFeeder_PickOffsetFromFiducial_Help = "AutoFeeder_PickOffsetFromFiducial_Help";
			public const string AutoFeeder_Size_Help = "AutoFeeder_Size_Help";
			public const string AutoFeeder_Title = "AutoFeeder_Title";
			public const string AutoFeeders_Title = "AutoFeeders_Title";
			public const string AutoFeederTemplate_Description = "AutoFeederTemplate_Description";
			public const string AutoFeederTemplate_Title = "AutoFeederTemplate_Title";
			public const string AutoFeederTemplates_Title = "AutoFeederTemplates_Title";
			public const string Common_Category = "Common_Category";
			public const string Common_Color = "Common_Color";
			public const string Common_CreatedBy = "Common_CreatedBy";
			public const string Common_CreationDate = "Common_CreationDate";
			public const string Common_Description = "Common_Description";
			public const string Common_Height = "Common_Height";
			public const string Common_Icon = "Common_Icon";
			public const string Common_IsPublic = "Common_IsPublic";
			public const string Common_IsRequired = "Common_IsRequired";
			public const string Common_IsValid = "Common_IsValid";
			public const string Common_Key = "Common_Key";
			public const string Common_Key_Help = "Common_Key_Help";
			public const string Common_Key_Validation = "Common_Key_Validation";
			public const string Common_LastUpdated = "Common_LastUpdated";
			public const string Common_LastUpdatedBy = "Common_LastUpdatedBy";
			public const string Common_Length = "Common_Length";
			public const string Common_Name = "Common_Name";
			public const string Common_Note = "Common_Note";
			public const string Common_Notes = "Common_Notes";
			public const string Common_Offset = "Common_Offset";
			public const string Common_Origin = "Common_Origin";
			public const string Common_Origin_Help = "Common_Origin_Help";
			public const string Common_PageNumberOne = "Common_PageNumberOne";
			public const string Common_Quantity = "Common_Quantity";
			public const string Common_QuantityBackOrdered = "Common_QuantityBackOrdered";
			public const string Common_QuantityOrdered = "Common_QuantityOrdered";
			public const string Common_QuantityReceived = "Common_QuantityReceived";
			public const string Common_Resources = "Common_Resources";
			public const string Common_SelectCategory = "Common_SelectCategory";
			public const string Common_Size = "Common_Size";
			public const string Common_Status = "Common_Status";
			public const string Common_Status_Select = "Common_Status_Select";
			public const string Common_TItle = "Common_TItle";
			public const string Common_UniqueId = "Common_UniqueId";
			public const string Common_ValidationErrors = "Common_ValidationErrors";
			public const string Common_Value = "Common_Value";
			public const string Common_Width = "Common_Width";
			public const string CompomentPurchase_Title = "CompomentPurchase_Title";
			public const string Component_Attr1 = "Component_Attr1";
			public const string Component_Attr2 = "Component_Attr2";
			public const string Component_Attributes = "Component_Attributes";
			public const string Component_Bin = "Component_Bin";
			public const string Component_Column = "Component_Column";
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
			public const string Component_Polarized = "Component_Polarized";
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
			public const string ComponentAttribute_Description = "ComponentAttribute_Description";
			public const string ComponentAttributes_Title = "ComponentAttributes_Title";
			public const string ComponentOrder_Description = "ComponentOrder_Description";
			public const string ComponentOrder_LineItemCsv = "ComponentOrder_LineItemCsv";
			public const string ComponentOrder_LineItems = "ComponentOrder_LineItems";
			public const string ComponentOrder_OrderDate = "ComponentOrder_OrderDate";
			public const string ComponentOrder_PrintLabels = "ComponentOrder_PrintLabels";
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
			public const string ComponentPackage_CustomTapePitch = "ComponentPackage_CustomTapePitch";
			public const string ComponentPackage_Description = "ComponentPackage_Description";
			public const string ComponentPackage_HoleSpacing = "ComponentPackage_HoleSpacing";
			public const string ComponentPackage_MaterialType_Paper = "ComponentPackage_MaterialType_Paper";
			public const string ComponentPackage_MaterialType_Plastic = "ComponentPackage_MaterialType_Plastic";
			public const string ComponentPackage_NozzleTip = "ComponentPackage_NozzleTip";
			public const string ComponentPackage_NozzleTip_Help = "ComponentPackage_NozzleTip_Help";
			public const string ComponentPackage_PackageId = "ComponentPackage_PackageId";
			public const string ComponentPackage_PartHeight = "ComponentPackage_PartHeight";
			public const string ComponentPackage_PartLength = "ComponentPackage_PartLength";
			public const string ComponentPackage_PartType = "ComponentPackage_PartType";
			public const string ComponentPackage_PartType_Select = "ComponentPackage_PartType_Select";
			public const string ComponentPackage_PartWidth = "ComponentPackage_PartWidth";
			public const string ComponentPackage_PickVacuumLevel = "ComponentPackage_PickVacuumLevel";
			public const string ComponentPackage_Rotation = "ComponentPackage_Rotation";
			public const string ComponentPackage_SpacingX = "ComponentPackage_SpacingX";
			public const string ComponentPackage_SpecificationPage = "ComponentPackage_SpecificationPage";
			public const string ComponentPackage_TapeAndReelActualImage = "ComponentPackage_TapeAndReelActualImage";
			public const string ComponentPackage_TapeAndReelSpecImage = "ComponentPackage_TapeAndReelSpecImage";
			public const string ComponentPackage_TapeColor = "ComponentPackage_TapeColor";
			public const string ComponentPackage_TapeColor_Black = "ComponentPackage_TapeColor_Black";
			public const string ComponentPackage_TapeColor_Clear = "ComponentPackage_TapeColor_Clear";
			public const string ComponentPackage_TapeColor_Select = "ComponentPackage_TapeColor_Select";
			public const string ComponentPackage_TapeColor_White = "ComponentPackage_TapeColor_White";
			public const string ComponentPackage_TapeMaterialType = "ComponentPackage_TapeMaterialType";
			public const string ComponentPackage_TapeMaterialType_Select = "ComponentPackage_TapeMaterialType_Select";
			public const string ComponentPackage_TapePitch = "ComponentPackage_TapePitch";
			public const string ComponentPackage_TapePitch_Select = "ComponentPackage_TapePitch_Select";
			public const string ComponentPackage_TapeRotation = "ComponentPackage_TapeRotation";
			public const string ComponentPackage_TapeRotation_Select = "ComponentPackage_TapeRotation_Select";
			public const string ComponentPackage_TapeSize = "ComponentPackage_TapeSize";
			public const string ComponentPackage_TapeSize_Select = "ComponentPackage_TapeSize_Select";
			public const string ComponentPackage_TapeSpecificationPage = "ComponentPackage_TapeSpecificationPage";
			public const string ComponentPackage_TapeWidth = "ComponentPackage_TapeWidth";
			public const string ComponentPackage_TItle = "ComponentPackage_TItle";
			public const string ComponentPackage_Verified = "ComponentPackage_Verified";
			public const string ComponentPackage_Verified_Help = "ComponentPackage_Verified_Help";
			public const string ComponentPackage_XOffsetReferenceHole = "ComponentPackage_XOffsetReferenceHole";
			public const string ComponentPackage_YOffsetReferenceHole = "ComponentPackage_YOffsetReferenceHole";
			public const string ComponentPurchase_Description = "ComponentPurchase_Description";
			public const string ComponentPurchase_OrderDate = "ComponentPurchase_OrderDate";
			public const string ComponentPurchase_OrderNumber = "ComponentPurchase_OrderNumber";
			public const string ComponentPurchase_Quantity = "ComponentPurchase_Quantity";
			public const string ComponentPurchase_Title = "ComponentPurchase_Title";
			public const string ComponentPurchase_Vendor = "ComponentPurchase_Vendor";
			public const string ContourRetrieveMode_External = "ContourRetrieveMode_External";
			public const string ContourRetrieveMode_FloodFill = "ContourRetrieveMode_FloodFill";
			public const string ContourRetrieveMode_List = "ContourRetrieveMode_List";
			public const string ContourRetrieveMode_Tree = "ContourRetrieveMode_Tree";
			public const string ContourRetrieveMode_TwoLevelHierarchy = "ContourRetrieveMode_TwoLevelHierarchy";
			public const string FeedDirection_Backwards = "FeedDirection_Backwards";
			public const string FeedDirection_Forwards = "FeedDirection_Forwards";
			public const string Feeder_AdvanceGCode = "Feeder_AdvanceGCode";
			public const string Feeder_AdvanceGCode_Help = "Feeder_AdvanceGCode_Help";
			public const string Feeder_Component_Select = "Feeder_Component_Select";
			public const string Feeder_FeederHeight_Help = "Feeder_FeederHeight_Help";
			public const string Feeder_FeederId = "Feeder_FeederId";
			public const string Feeder_FeederId_Help = "Feeder_FeederId_Help";
			public const string Feeder_FeederLength_Help = "Feeder_FeederLength_Help";
			public const string Feeder_FeederWidth_Help = "Feeder_FeederWidth_Help";
			public const string Feeder_FiducialOffsetFromSlotOriign = "Feeder_FiducialOffsetFromSlotOriign";
			public const string Feeder_FiducialOffsetFromSlotOriign_Help = "Feeder_FiducialOffsetFromSlotOriign_Help";
			public const string Feeder_Machine = "Feeder_Machine";
			public const string Feeder_Machine_Select = "Feeder_Machine_Select";
			public const string Feeder_OriginalTemplate = "Feeder_OriginalTemplate";
			public const string Feeder_OriginOffset = "Feeder_OriginOffset";
			public const string Feeder_OriginOffset_Help = "Feeder_OriginOffset_Help";
			public const string Feeder_PartCount = "Feeder_PartCount";
			public const string Feeder_PickHeight = "Feeder_PickHeight";
			public const string Feeder_PickLocation = "Feeder_PickLocation";
			public const string Feeder_PickOffsetFromFiducial = "Feeder_PickOffsetFromFiducial";
			public const string Feeder_PickOffsetFromSlotOrigin_Help = "Feeder_PickOffsetFromSlotOrigin_Help";
			public const string Feeder_Protocol = "Feeder_Protocol";
			public const string Feeder_Protocol_Select = "Feeder_Protocol_Select";
			public const string Feeder_Rotation = "Feeder_Rotation";
			public const string Feeder_Rotation_Select = "Feeder_Rotation_Select";
			public const string Feeder_Size = "Feeder_Size";
			public const string Feeder_Slot = "Feeder_Slot";
			public const string Feeder_TapeAngle = "Feeder_TapeAngle";
			public const string FeederOrientation_Horizontal = "FeederOrientation_Horizontal";
			public const string FeederOrientation_Vertical = "FeederOrientation_Vertical";
			public const string FeederProtocol_Other = "FeederProtocol_Other";
			public const string FeederProtocol_Photon = "FeederProtocol_Photon";
			public const string FeederRotation0 = "FeederRotation0";
			public const string FeederRotation180 = "FeederRotation180";
			public const string FeederRotation90 = "FeederRotation90";
			public const string FeederRotationMinus90 = "FeederRotationMinus90";
			public const string Feeders_Title = "Feeders_Title";
			public const string GCode_BottomLightOff = "GCode_BottomLightOff";
			public const string GCode_BottomLightOn = "GCode_BottomLightOn";
			public const string GCode_Description = "GCode_Description";
			public const string GCode_Dwell = "GCode_Dwell";
			public const string GCode_EmergencyStop = "GCode_EmergencyStop";
			public const string GCode_Exchaust1_Off = "GCode_Exchaust1_Off";
			public const string GCode_Exchaust1_On = "GCode_Exchaust1_On";
			public const string GCode_Exchaust2_Off = "GCode_Exchaust2_Off";
			public const string GCode_Exchaust2_On = "GCode_Exchaust2_On";
			public const string GCode_HomeAllCommand = "GCode_HomeAllCommand";
			public const string GCode_HomeXCommand = "GCode_HomeXCommand";
			public const string GCode_HomeYCommand = "GCode_HomeYCommand";
			public const string GCode_HomeZCommand = "GCode_HomeZCommand";
			public const string GCode_Laser_Off = "GCode_Laser_Off";
			public const string GCode_Laser_On = "GCode_Laser_On";
			public const string GCode_LeftVacuumOff = "GCode_LeftVacuumOff";
			public const string GCode_LeftVacuumOn = "GCode_LeftVacuumOn";
			public const string GCode_LeftVacuumResponseExample = "GCode_LeftVacuumResponseExample";
			public const string GCode_ParseLeftVacuumRegEx = "GCode_ParseLeftVacuumRegEx";
			public const string GCode_ParseRightVacuumRegEx = "GCode_ParseRightVacuumRegEx";
			public const string GCode_ParseStatus_RegularExpression = "GCode_ParseStatus_RegularExpression";
			public const string GCode_ParseStatusRegularExpressionHelp = "GCode_ParseStatusRegularExpressionHelp";
			public const string GCode_ReadLeftVacuumCmd = "GCode_ReadLeftVacuumCmd";
			public const string GCode_ReadRightVacuumCmd = "GCode_ReadRightVacuumCmd";
			public const string GCode_RequestStatusCommand = "GCode_RequestStatusCommand";
			public const string GCode_RightVacuumOff = "GCode_RightVacuumOff";
			public const string GCode_RightVacuumOn = "GCode_RightVacuumOn";
			public const string GCode_RightVacuumResponseExample = "GCode_RightVacuumResponseExample";
			public const string GCode_SpindleOff = "GCode_SpindleOff";
			public const string GCode_SpindleOn = "GCode_SpindleOn";
			public const string GCode_StatusResponseExample = "GCode_StatusResponseExample";
			public const string GCode_ToolChange = "GCode_ToolChange";
			public const string GCode_TopLightOff = "GCode_TopLightOff";
			public const string GCode_TopLightOn = "GCode_TopLightOn";
			public const string GCodeMapping_Title = "GCodeMapping_Title";
			public const string GCodeMappings_Title = "GCodeMappings_Title";
			public const string JobPlacementStatus_Failed = "JobPlacementStatus_Failed";
			public const string JobPlacementStatus_InProcess = "JobPlacementStatus_InProcess";
			public const string JobPlacementStatus_Pending = "JobPlacementStatus_Pending";
			public const string JobPlacementStatus_Placed = "JobPlacementStatus_Placed";
			public const string JobState_Aborted = "JobState_Aborted";
			public const string JobState_Completed = "JobState_Completed";
			public const string JobState_Creted = "JobState_Creted";
			public const string JobState_Failed = "JobState_Failed";
			public const string JobState_Running = "JobState_Running";
			public const string Machine_Cameras = "Machine_Cameras";
			public const string Machine_ConnectToMQTT = "Machine_ConnectToMQTT";
			public const string Machine_Description = "Machine_Description";
			public const string Machine_DeviceId = "Machine_DeviceId";
			public const string Machine_FeederRail_Description = "Machine_FeederRail_Description";
			public const string Machine_FeederRail_FirstFeederOrigin = "Machine_FeederRail_FirstFeederOrigin";
			public const string Machine_FeederRail_FirstFeederOrigin_Help = "Machine_FeederRail_FirstFeederOrigin_Help";
			public const string Machine_FeederRail_Height = "Machine_FeederRail_Height";
			public const string Machine_FeederRail_NumberSlots = "Machine_FeederRail_NumberSlots";
			public const string Machine_FeederRail_SlotWidth = "Machine_FeederRail_SlotWidth";
			public const string Machine_FeederRail_StartSlotIndex = "Machine_FeederRail_StartSlotIndex";
			public const string Machine_FeederRail_StartSlotIndex_Help = "Machine_FeederRail_StartSlotIndex_Help";
			public const string Machine_FeederRail_Title = "Machine_FeederRail_Title";
			public const string Machine_FeederRail_Width = "Machine_FeederRail_Width";
			public const string Machine_FeederRails = "Machine_FeederRails";
			public const string Machine_FrameSize = "Machine_FrameSize";
			public const string Machine_FrameSize_Help = "Machine_FrameSize_Help";
			public const string Machine_GCode = "Machine_GCode";
			public const string Machine_GCode_Description = "Machine_GCode_Description";
			public const string Machine_GCodeMapping = "Machine_GCodeMapping";
			public const string Machine_GCodeMapping_Select = "Machine_GCodeMapping_Select";
			public const string Machine_HostName = "Machine_HostName";
			public const string Machine_JogFeedRate = "Machine_JogFeedRate";
			public const string Machine_MaxFeedRate = "Machine_MaxFeedRate";
			public const string Machine_Password = "Machine_Password";
			public const string Machine_Port = "Machine_Port";
			public const string Machine_SecureConnection = "Machine_SecureConnection";
			public const string Machine_Title = "Machine_Title";
			public const string Machine_ToolHeads = "Machine_ToolHeads";
			public const string Machine_UserName = "Machine_UserName";
			public const string Machine_WorkAreaOrigin = "Machine_WorkAreaOrigin";
			public const string Machine_WorkAreaOrigin_Help = "Machine_WorkAreaOrigin_Help";
			public const string Machine_WorkAreaSize = "Machine_WorkAreaSize";
			public const string Machine_WorkAreaSize_Help = "Machine_WorkAreaSize_Help";
			public const string Machine_WorkspaceFrameOffset = "Machine_WorkspaceFrameOffset";
			public const string Machine_WorkspaceFrameOffset_Help = "Machine_WorkspaceFrameOffset_Help";
			public const string MachineCamera_AbsolutePosition = "MachineCamera_AbsolutePosition";
			public const string MachineCamera_Description = "MachineCamera_Description";
			public const string MachineCamera_DeviceId = "MachineCamera_DeviceId";
			public const string MachineCamera_FocusHeight = "MachineCamera_FocusHeight";
			public const string MachineCamera_ImageSize = "MachineCamera_ImageSize";
			public const string MachineCamera_MirrorX = "MachineCamera_MirrorX";
			public const string MachineCamera_MirrorY = "MachineCamera_MirrorY";
			public const string MachineCamera_PixelsPerMM = "MachineCamera_PixelsPerMM";
			public const string MachineCamera_PixelsPerMM_Help = "MachineCamera_PixelsPerMM_Help";
			public const string MachineCamera_SupportZoom = "MachineCamera_SupportZoom";
			public const string MachineCamera_Title = "MachineCamera_Title";
			public const string MachineCamera_Type = "MachineCamera_Type";
			public const string MachineCamera_Type_Observation = "MachineCamera_Type_Observation";
			public const string MachineCamera_Type_PartInspection = "MachineCamera_Type_PartInspection";
			public const string MachineCamera_Type_Position = "MachineCamera_Type_Position";
			public const string MachineCamera_Type_Select = "MachineCamera_Type_Select";
			public const string Machines_Title = "Machines_Title";
			public const string MachineStagingPlate_Description = "MachineStagingPlate_Description";
			public const string MachineStagingPlate_FirstHole = "MachineStagingPlate_FirstHole";
			public const string MachineStagingPlate_FirstHole_Help = "MachineStagingPlate_FirstHole_Help";
			public const string MachineStagingPlate_FirstRowOddOnly = "MachineStagingPlate_FirstRowOddOnly";
			public const string MachineStagingPlate_FirstRowOddOnly_Help = "MachineStagingPlate_FirstRowOddOnly_Help";
			public const string MachineStagingPlate_FirstUsableColumn = "MachineStagingPlate_FirstUsableColumn";
			public const string MachineStagingPlate_FirstUsableColumn_Help = "MachineStagingPlate_FirstUsableColumn_Help";
			public const string MachineStagingPlate_HoleSpacing = "MachineStagingPlate_HoleSpacing";
			public const string MachineStagingPlate_HolesStaggered = "MachineStagingPlate_HolesStaggered";
			public const string MachineStagingPlate_HolesStaggered_Help = "MachineStagingPlate_HolesStaggered_Help";
			public const string MachineStagingPlate_LastUsableColumn = "MachineStagingPlate_LastUsableColumn";
			public const string MachineStagingPlate_LastUsableColumn_Help = "MachineStagingPlate_LastUsableColumn_Help";
			public const string MachineStagingPlate_Origin_Help = "MachineStagingPlate_Origin_Help";
			public const string MachineStagingPlate_Origin_Origin_Approx = "MachineStagingPlate_Origin_Origin_Approx";
			public const string MachineStagingPlate_ReferenceHoleCol_Help = "MachineStagingPlate_ReferenceHoleCol_Help";
			public const string MachineStagingPlate_ReferenceHoleCol1 = "MachineStagingPlate_ReferenceHoleCol1";
			public const string MachineStagingPlate_ReferenceHoleCol2 = "MachineStagingPlate_ReferenceHoleCol2";
			public const string MachineStagingPlate_ReferenceHoleLocation_Help = "MachineStagingPlate_ReferenceHoleLocation_Help";
			public const string MachineStagingPlate_ReferenceHoleLocation1 = "MachineStagingPlate_ReferenceHoleLocation1";
			public const string MachineStagingPlate_ReferenceHoleLocation2 = "MachineStagingPlate_ReferenceHoleLocation2";
			public const string MachineStagingPlate_ReferenceHoleRow_Help = "MachineStagingPlate_ReferenceHoleRow_Help";
			public const string MachineStagingPlate_ReferenceHoleRow1 = "MachineStagingPlate_ReferenceHoleRow1";
			public const string MachineStagingPlate_ReferenceHoleRow2 = "MachineStagingPlate_ReferenceHoleRow2";
			public const string MachineStagingPlate_Title = "MachineStagingPlate_Title";
			public const string MachineStagingPlates_Title = "MachineStagingPlates_Title";
			public const string MachineToolHead_CurrentNozzle = "MachineToolHead_CurrentNozzle";
			public const string MachineToolHead_CurrentNozzle_Select = "MachineToolHead_CurrentNozzle_Select";
			public const string MachineToolHead_DefaultOriginPosition = "MachineToolHead_DefaultOriginPosition";
			public const string MachineToolHead_DefaultOriginPosition_Help = "MachineToolHead_DefaultOriginPosition_Help";
			public const string MachineToolHead_Description = "MachineToolHead_Description";
			public const string MachineToolHead_HeadIndex = "MachineToolHead_HeadIndex";
			public const string MachineToolHead_HeadIndex_Help = "MachineToolHead_HeadIndex_Help";
			public const string MachineToolHead_IdleVacuum = "MachineToolHead_IdleVacuum";
			public const string MachineToolHead_IdleVacuum_Help = "MachineToolHead_IdleVacuum_Help";
			public const string MachineToolHead_NoPartVacuum = "MachineToolHead_NoPartVacuum";
			public const string MachineToolHead_NoPartVacuum_Help = "MachineToolHead_NoPartVacuum_Help";
			public const string MachineToolHead_PartPresentVacuum = "MachineToolHead_PartPresentVacuum";
			public const string MachineToolHead_PartPresentVacuum_Help = "MachineToolHead_PartPresentVacuum_Help";
			public const string MachineToolHead_PickHeight = "MachineToolHead_PickHeight";
			public const string MachineToolHead_PickHeight_Help = "MachineToolHead_PickHeight_Help";
			public const string MachineToolHead_PlaceHeight = "MachineToolHead_PlaceHeight";
			public const string MachineToolHead_PlaceHight_Help = "MachineToolHead_PlaceHight_Help";
			public const string MachineToolHead_Title = "MachineToolHead_Title";
			public const string MachineToolHead_Type = "MachineToolHead_Type";
			public const string MachineToolHead_Type_Laser = "MachineToolHead_Type_Laser";
			public const string MachineToolHead_Type_PartNozzle = "MachineToolHead_Type_PartNozzle";
			public const string MachineToolHead_Type_Select = "MachineToolHead_Type_Select";
			public const string MachineToolHead_Type_Spindle = "MachineToolHead_Type_Spindle";
			public const string MachineToolHead_VacuumTolerancePercent = "MachineToolHead_VacuumTolerancePercent";
			public const string MachineToolHead_VacuumTolerancePercent_Help = "MachineToolHead_VacuumTolerancePercent_Help";
			public const string NozzleTip_Description = "NozzleTip_Description";
			public const string NozzleTip_Height = "NozzleTip_Height";
			public const string NozzleTip_IdleVacuum = "NozzleTip_IdleVacuum";
			public const string NozzleTip_InnerDiameter = "NozzleTip_InnerDiameter";
			public const string NozzleTip_OuterDiameter = "NozzleTip_OuterDiameter";
			public const string NozzleTip_PartPickedVacuum = "NozzleTip_PartPickedVacuum";
			public const string NozzleTip_Title = "NozzleTip_Title";
			public const string NozzleTip_ToolRackLocation = "NozzleTip_ToolRackLocation";
			public const string NozzleTips_Title = "NozzleTips_Title";
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
			public const string PickAndPlaceJob_Board = "PickAndPlaceJob_Board";
			public const string PickAndPlaceJob_Cost = "PickAndPlaceJob_Cost";
			public const string PickAndPlaceJob_CurrentSerialNumber = "PickAndPlaceJob_CurrentSerialNumber";
			public const string PickAndPlaceJob_Description = "PickAndPlaceJob_Description";
			public const string PickAndPlaceJob_Extneded = "PickAndPlaceJob_Extneded";
			public const string PickAndPlaceJob_Title = "PickAndPlaceJob_Title";
			public const string PickAndPlaceJobRun_Description = "PickAndPlaceJobRun_Description";
			public const string PickAndPlaceJobRun_Title = "PickAndPlaceJobRun_Title";
			public const string PickAndPlaceJobRuns_Title = "PickAndPlaceJobRuns_Title";
			public const string PickAndPlaceJobs_Title = "PickAndPlaceJobs_Title";
			public const string PnPJobState_Aborted = "PnPJobState_Aborted";
			public const string PnPJobState_Completed = "PnPJobState_Completed";
			public const string PnPJobState_Error = "PnPJobState_Error";
			public const string PnPJobState_Idle = "PnPJobState_Idle";
			public const string PnPJobState_New = "PnPJobState_New";
			public const string PnPJobState_Paused = "PnPJobState_Paused";
			public const string PnPJobState_Running = "PnPJobState_Running";
			public const string PnPState_Advanced = "PnPState_Advanced";
			public const string PnpState_AtFeeder = "PnpState_AtFeeder";
			public const string PnPState_AtPlaceLocation = "PnPState_AtPlaceLocation";
			public const string PnpState_DetectingPart = "PnpState_DetectingPart";
			public const string PnpState_Error = "PnpState_Error";
			public const string PnpState_FeederResolved = "PnpState_FeederResolved";
			public const string PnPState_Inspected = "PnPState_Inspected";
			public const string PnPState_Inspecting = "PnPState_Inspecting";
			public const string PnPState_JobCompleted = "PnPState_JobCompleted";
			public const string PnPState_New = "PnPState_New";
			public const string PnPState_OnBoard = "PnPState_OnBoard";
			public const string PnpState_PartCenteredOnFeeder = "PnpState_PartCenteredOnFeeder";
			public const string PnPState_PartCompleted = "PnPState_PartCompleted";
			public const string PnpState_PartDetected = "PnpState_PartDetected";
			public const string PnpState_PartNotDetected = "PnpState_PartNotDetected";
			public const string PnpState_PartPicked = "PnpState_PartPicked";
			public const string PnPState_PickErrorCompensated = "PnPState_PickErrorCompensated";
			public const string PnPState_PickErrorCompensating = "PnPState_PickErrorCompensating";
			public const string PnPState_PickErrorNotCompensated = "PnPState_PickErrorNotCompensated";
			public const string PnPState_Placed = "PnPState_Placed";
			public const string PnPState_PlacementCompleted = "PnPState_PlacementCompleted";
			public const string PnpState_Validated = "PnpState_Validated";
			public const string StripFeeder_AngleOffset = "StripFeeder_AngleOffset";
			public const string StripFeeder_AngleOffset_Help = "StripFeeder_AngleOffset_Help";
			public const string StripFeeder_CurrentPartindex = "StripFeeder_CurrentPartindex";
			public const string StripFeeder_Description = "StripFeeder_Description";
			public const string StripFeeder_Direction = "StripFeeder_Direction";
			public const string StripFeeder_Direction_Select = "StripFeeder_Direction_Select";
			public const string StripFeeder_DualHoles = "StripFeeder_DualHoles";
			public const string StripFeeder_DualHoles_Help = "StripFeeder_DualHoles_Help";
			public const string StripFeeder_FeederSize_Help = "StripFeeder_FeederSize_Help";
			public const string StripFeeder_Orientation = "StripFeeder_Orientation";
			public const string StripFeeder_Orientation_Select = "StripFeeder_Orientation_Select";
			public const string StripFeeder_Origin_Help = "StripFeeder_Origin_Help";
			public const string StripFeeder_OriginOffset = "StripFeeder_OriginOffset";
			public const string StripFeeder_OriginOffset_Help = "StripFeeder_OriginOffset_Help";
			public const string StripFeeder_PartCapacity = "StripFeeder_PartCapacity";
			public const string StripFeeder_PartCapacity_Help = "StripFeeder_PartCapacity_Help";
			public const string StripFeeder_PickHeight = "StripFeeder_PickHeight";
			public const string StripFeeder_PickHeight_Help = "StripFeeder_PickHeight_Help";
			public const string StripFeeder_ReferenceHole_Col = "StripFeeder_ReferenceHole_Col";
			public const string StripFeeder_ReferenceHole_Col_Help = "StripFeeder_ReferenceHole_Col_Help";
			public const string StripFeeder_ReferenceHole_Row = "StripFeeder_ReferenceHole_Row";
			public const string StripFeeder_ReferenceHole_Row_Help = "StripFeeder_ReferenceHole_Row_Help";
			public const string StripFeeder_ReferenceHoleOffset = "StripFeeder_ReferenceHoleOffset";
			public const string StripFeeder_ReferenceHoleOffset_Help = "StripFeeder_ReferenceHoleOffset_Help";
			public const string StripFeeder_RowCount = "StripFeeder_RowCount";
			public const string StripFeeder_RowOneRefHoleOffset = "StripFeeder_RowOneRefHoleOffset";
			public const string StripFeeder_RowOneRefHoleOffset_Help = "StripFeeder_RowOneRefHoleOffset_Help";
			public const string StripFeeder_Rows = "StripFeeder_Rows";
			public const string StripFeeder_RowWidth = "StripFeeder_RowWidth";
			public const string StripFeeder_RowWidth_Help = "StripFeeder_RowWidth_Help";
			public const string StripFeeder_StagingPlate = "StripFeeder_StagingPlate";
			public const string StripFeeder_StagingPlate_Select = "StripFeeder_StagingPlate_Select";
			public const string StripFeeder_StagingPlateColumn = "StripFeeder_StagingPlateColumn";
			public const string StripFeeder_TapeHolesOnTop = "StripFeeder_TapeHolesOnTop";
			public const string StripFeeder_TapeHolesOnTop_Help = "StripFeeder_TapeHolesOnTop_Help";
			public const string StripFeeder_Title = "StripFeeder_Title";
			public const string StripFeederRow_CurrentPartIndex = "StripFeederRow_CurrentPartIndex";
			public const string StripFeederRow_Description = "StripFeederRow_Description";
			public const string StripFeederRow_FirstTapeHoleOffset = "StripFeederRow_FirstTapeHoleOffset";
			public const string StripFeederRow_FirstTapeHoleOffset_Help = "StripFeederRow_FirstTapeHoleOffset_Help";
			public const string StripFeederRow_LastTapeHoleOffset = "StripFeederRow_LastTapeHoleOffset";
			public const string StripFeederRow_LastTapeHoleOffset_Help = "StripFeederRow_LastTapeHoleOffset_Help";
			public const string StripFeederRow_RowIndex = "StripFeederRow_RowIndex";
			public const string StripFeederRow_Title = "StripFeederRow_Title";
			public const string StripFeederRowStatus_Empty = "StripFeederRowStatus_Empty";
			public const string StripFeederRowStatus_None = "StripFeederRowStatus_None";
			public const string StripFeederRowStatus_Planned = "StripFeederRowStatus_Planned";
			public const string StripFeederRowStatus_Ready = "StripFeederRowStatus_Ready";
			public const string StripFeeders_Title = "StripFeeders_Title";
			public const string StripFeederTemplate_Description = "StripFeederTemplate_Description";
			public const string StripFeederTemplate_Title = "StripFeederTemplate_Title";
			public const string StripFeederTemplates_Title = "StripFeederTemplates_Title";
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
			public const string ToolNozzleTip_Description = "ToolNozzleTip_Description";
			public const string ToolNozzleTip_Title = "ToolNozzleTip_Title";
			public const string VisionProfile_BoardFiducial = "VisionProfile_BoardFiducial";
			public const string VisionProfile_CountourRetrievealMode = "VisionProfile_CountourRetrievealMode";
			public const string VisionProfile_CountourRetrievealMode_Select = "VisionProfile_CountourRetrievealMode_Select";
			public const string VisionProfile_Defauilt = "VisionProfile_Defauilt";
			public const string VisionProfile_FeederFiducial = "VisionProfile_FeederFiducial";
			public const string VisionProfile_FeederOrigin = "VisionProfile_FeederOrigin";
			public const string VisionProfile_MachineFiducual = "VisionProfile_MachineFiducual";
			public const string VisionProfile_Nozzle = "VisionProfile_Nozzle";
			public const string VisionProfile_NozzleCalibration = "VisionProfile_NozzleCalibration";
			public const string VisionProfile_PartInBlackTape = "VisionProfile_PartInBlackTape";
			public const string VisionProfile_PartInClearTape = "VisionProfile_PartInClearTape";
			public const string VisionProfile_PartInspection = "VisionProfile_PartInspection";
			public const string VisionProfile_PartInWhiteTape = "VisionProfile_PartInWhiteTape";
			public const string VisionProfile_PartOnBoard = "VisionProfile_PartOnBoard";
			public const string VisionProfile_SquarePart = "VisionProfile_SquarePart";
			public const string VisionProfile_StagingPlateHole = "VisionProfile_StagingPlateHole";
			public const string VisionProfile_TapeHole = "VisionProfile_TapeHole";
			public const string VisionProfile_TapeHoleBlackTape = "VisionProfile_TapeHoleBlackTape";
			public const string VisionProfile_TapeHoleClearTape = "VisionProfile_TapeHoleClearTape";
			public const string VisionProfile_TapeHoleWhiteTape = "VisionProfile_TapeHoleWhiteTape";
		}
	}
}

