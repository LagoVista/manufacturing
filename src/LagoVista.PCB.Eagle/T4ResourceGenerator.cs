/*12/24/2024 5:55:04 AM*/
using System.Globalization;
using System.Reflection;

//Resources:PcbResources:Layer_BoardOutline
namespace LagoVista.PCB.Eagle.Resources
{
	public class PcbResources
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
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("LagoVista.PCB.Eagle.Resources.PcbResources", typeof(PcbResources).GetTypeInfo().Assembly);
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
		
		public static string Layer_BoardOutline { get { return GetResourceString("Layer_BoardOutline"); } }
//Resources:PcbResources:Layer_BottomCopper

		public static string Layer_BottomCopper { get { return GetResourceString("Layer_BottomCopper"); } }
//Resources:PcbResources:Layer_BottomDocument

		public static string Layer_BottomDocument { get { return GetResourceString("Layer_BottomDocument"); } }
//Resources:PcbResources:Layer_BottomNames

		public static string Layer_BottomNames { get { return GetResourceString("Layer_BottomNames"); } }
//Resources:PcbResources:Layer_BottomRestrict

		public static string Layer_BottomRestrict { get { return GetResourceString("Layer_BottomRestrict"); } }
//Resources:PcbResources:Layer_BottomSilk

		public static string Layer_BottomSilk { get { return GetResourceString("Layer_BottomSilk"); } }
//Resources:PcbResources:Layer_BottomSolderMask

		public static string Layer_BottomSolderMask { get { return GetResourceString("Layer_BottomSolderMask"); } }
//Resources:PcbResources:Layer_BottomStencil

		public static string Layer_BottomStencil { get { return GetResourceString("Layer_BottomStencil"); } }
//Resources:PcbResources:Layer_BottomValues

		public static string Layer_BottomValues { get { return GetResourceString("Layer_BottomValues"); } }
//Resources:PcbResources:Layer_Drills

		public static string Layer_Drills { get { return GetResourceString("Layer_Drills"); } }
//Resources:PcbResources:Layer_Holes

		public static string Layer_Holes { get { return GetResourceString("Layer_Holes"); } }
//Resources:PcbResources:Layer_Other

		public static string Layer_Other { get { return GetResourceString("Layer_Other"); } }
//Resources:PcbResources:Layer_Pads

		public static string Layer_Pads { get { return GetResourceString("Layer_Pads"); } }
//Resources:PcbResources:Layer_TopCopper

		public static string Layer_TopCopper { get { return GetResourceString("Layer_TopCopper"); } }
//Resources:PcbResources:Layer_TopDocument

		public static string Layer_TopDocument { get { return GetResourceString("Layer_TopDocument"); } }
//Resources:PcbResources:Layer_TopNames

		public static string Layer_TopNames { get { return GetResourceString("Layer_TopNames"); } }
//Resources:PcbResources:Layer_TopRestrict

		public static string Layer_TopRestrict { get { return GetResourceString("Layer_TopRestrict"); } }
//Resources:PcbResources:Layer_TopSilk

		public static string Layer_TopSilk { get { return GetResourceString("Layer_TopSilk"); } }
//Resources:PcbResources:Layer_TopSolderMask

		public static string Layer_TopSolderMask { get { return GetResourceString("Layer_TopSolderMask"); } }
//Resources:PcbResources:Layer_TopStencil

		public static string Layer_TopStencil { get { return GetResourceString("Layer_TopStencil"); } }
//Resources:PcbResources:Layer_TopValues

		public static string Layer_TopValues { get { return GetResourceString("Layer_TopValues"); } }
//Resources:PcbResources:Layer_Unrouted

		public static string Layer_Unrouted { get { return GetResourceString("Layer_Unrouted"); } }
//Resources:PcbResources:Layer_Vias

		public static string Layer_Vias { get { return GetResourceString("Layer_Vias"); } }

		public static class Names
		{
			public const string Layer_BoardOutline = "Layer_BoardOutline";
			public const string Layer_BottomCopper = "Layer_BottomCopper";
			public const string Layer_BottomDocument = "Layer_BottomDocument";
			public const string Layer_BottomNames = "Layer_BottomNames";
			public const string Layer_BottomRestrict = "Layer_BottomRestrict";
			public const string Layer_BottomSilk = "Layer_BottomSilk";
			public const string Layer_BottomSolderMask = "Layer_BottomSolderMask";
			public const string Layer_BottomStencil = "Layer_BottomStencil";
			public const string Layer_BottomValues = "Layer_BottomValues";
			public const string Layer_Drills = "Layer_Drills";
			public const string Layer_Holes = "Layer_Holes";
			public const string Layer_Other = "Layer_Other";
			public const string Layer_Pads = "Layer_Pads";
			public const string Layer_TopCopper = "Layer_TopCopper";
			public const string Layer_TopDocument = "Layer_TopDocument";
			public const string Layer_TopNames = "Layer_TopNames";
			public const string Layer_TopRestrict = "Layer_TopRestrict";
			public const string Layer_TopSilk = "Layer_TopSilk";
			public const string Layer_TopSolderMask = "Layer_TopSolderMask";
			public const string Layer_TopStencil = "Layer_TopStencil";
			public const string Layer_TopValues = "Layer_TopValues";
			public const string Layer_Unrouted = "Layer_Unrouted";
			public const string Layer_Vias = "Layer_Vias";
		}
	}
}

