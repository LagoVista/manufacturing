using LagoVista.Core.Models;
using LagoVista.PCB.Eagle.Models;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace LagoVista.PCB.Eagle.Extensions
{
    public static class LayerUtils
    {
        public static EntityHeader<PCBLayers> FromEagleLayer(this int layerNumber)
        {
            switch (layerNumber)
            {
                case 1: return EntityHeader<PCBLayers>.Create(PCBLayers.TopCopper);
                case 16: return EntityHeader<PCBLayers>.Create(PCBLayers.BottomCopper);
                case 17: return EntityHeader<PCBLayers>.Create(PCBLayers.Pads);
                case 18: return EntityHeader<PCBLayers>.Create(PCBLayers.Vias);
                case 21: return EntityHeader<PCBLayers>.Create(PCBLayers.TopSilk);
                case 22: return EntityHeader<PCBLayers>.Create(PCBLayers.BottomSilk);
                case 44: return EntityHeader<PCBLayers>.Create(PCBLayers.Drills);
                case 45: return EntityHeader<PCBLayers>.Create(PCBLayers.Holes);
                case 20: return EntityHeader<PCBLayers>.Create(PCBLayers.BoardOutline);
                case 51: return EntityHeader<PCBLayers>.Create(PCBLayers.TopDocument);
                case 52: return EntityHeader<PCBLayers>.Create(PCBLayers.BottomDocument);
                case 25: return EntityHeader<PCBLayers>.Create(PCBLayers.TopNames);
                case 26: return EntityHeader<PCBLayers>.Create(PCBLayers.BottomNames);
                case 27: return EntityHeader<PCBLayers>.Create(PCBLayers.TopValues);
                case 28: return EntityHeader<PCBLayers>.Create(PCBLayers.BottomValues);
                case 29: return EntityHeader<PCBLayers>.Create(PCBLayers.TopSolderMask);
                case 30: return EntityHeader<PCBLayers>.Create(PCBLayers.BottomSolderMask);
                case 31: return EntityHeader<PCBLayers>.Create(PCBLayers.TopStencil);
                case 32: return EntityHeader<PCBLayers>.Create(PCBLayers.BottomRestrict);
                case 41: return EntityHeader<PCBLayers>.Create(PCBLayers.TopRestrict);
                case 42: return EntityHeader<PCBLayers>.Create(PCBLayers.BottomRestrict);
                case 19: return EntityHeader<PCBLayers>.Create(PCBLayers.Unrouted);
                default:
                    {
                        var eh = EntityHeader<PCBLayers>.Create(PCBLayers.Other);
                        eh.Text = $"Unspported layer: {layerNumber}";
                        return eh;
                    }
            };
        }

        public static string FromEagleColor(this int layerNumber)
        {
            switch (layerNumber)
            {
                case 1: return "#a10a0a";
                case 16: return "#0578ac";
                case 17: return "#31b079";
                case 18: return "#31b079";
                case 21: return "#a0a0a0";
                case 22: return "#a0a0a0";
                case 44: return "#000000";
                case 45: return "#000000";
                case 20: return "#e4be41";
                case 51: return "#a0a0a0";
                case 52: return "#a0a0a0";
                case 25: return "#a0a0a0";
                case 26: return "#a0a0a0";
                case 27: return "#a0a0a0";
                case 28: return "#a0a0a0";
                case 29: return "#007f7f";
                case 30: return "#00AFAF";
                case 31: return "#7f7f00";
                case 32: return "#AFAF00";
                case 41: return "#ff0000";
                case 42: return "#7f0000";
                case 19: return "#a0a028";
                default: return "#ffffff"; 
            };
        }

        public static EntityHeader<PCBLayers> FromKiCadLayer(this string layerName)
        {
            switch(layerName)
            {
                case "F.Cu": return EntityHeader<PCBLayers>.Create(PCBLayers.TopCopper);
                case "B.Cu": return EntityHeader<PCBLayers>.Create(PCBLayers.BottomCopper);
                case "F.Silkscreen": return EntityHeader<PCBLayers>.Create(PCBLayers.TopSilk);
                case "F.SilkS": return EntityHeader<PCBLayers>.Create(PCBLayers.TopSilk);
                case "B.Silkscreen": return EntityHeader<PCBLayers>.Create(PCBLayers.BottomSilk);
                case "B.SilkS": return EntityHeader<PCBLayers>.Create(PCBLayers.TopSilk);
                case "Edge.Cuts": return EntityHeader<PCBLayers>.Create(PCBLayers.BoardOutline);
                case "F.Fab": return EntityHeader<PCBLayers>.Create(PCBLayers.TopNames);
                case "B.Fab": return EntityHeader<PCBLayers>.Create(PCBLayers.BottomNames);
                case "F.Mask": return EntityHeader<PCBLayers>.Create(PCBLayers.TopSolderMask);
                case "B.Mask": return EntityHeader<PCBLayers>.Create(PCBLayers.BottomSolderMask);
                case "F.Paste": return EntityHeader<PCBLayers>.Create(PCBLayers.TopStencil);
                case "B.Paste": return EntityHeader<PCBLayers>.Create(PCBLayers.BottomStencil);
                case "F.CrtYd": return EntityHeader<PCBLayers>.Create(PCBLayers.TopRestrict);
                case "B.CrtYd": return EntityHeader<PCBLayers>.Create(PCBLayers.BottomRestrict);
                case "F.Courtyard": return EntityHeader<PCBLayers>.Create(PCBLayers.TopRestrict);
                case "B.Courtyard": return EntityHeader<PCBLayers>.Create(PCBLayers.BottomRestrict);
                default:
                    {
                        var eh = EntityHeader<PCBLayers>.Create(PCBLayers.Other);
                        eh.Text = $"Unsupported Layer: {layerName}";
                        return eh;
                    }
                 }
            }
    }
}
