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
                    return EntityHeader<PCBLayers>.Create(PCBLayers.Other);
            };
        }

        public static EntityHeader<PCBLayers> FromKiCadLayer(this string layerName)
        {
            switch(layerName)
            {
                case "F.Cu": return EntityHeader<PCBLayers>.Create(PCBLayers.TopCopper);
                case "B.Cu": return EntityHeader<PCBLayers>.Create(PCBLayers.BottomCopper);
                case "F.Silkscreen": return EntityHeader<PCBLayers>.Create(PCBLayers.TopSilk);
                case "B.Silkscreen": return EntityHeader<PCBLayers>.Create(PCBLayers.BottomSilk);
                case "Edge.Cuts": return EntityHeader<PCBLayers>.Create(PCBLayers.BoardOutline);
                case "F.Fab": return EntityHeader<PCBLayers>.Create(PCBLayers.TopNames);
                case "B.Fab": return EntityHeader<PCBLayers>.Create(PCBLayers.BottomNames);
                case "F.Mask": return EntityHeader<PCBLayers>.Create(PCBLayers.TopSolderMask);
                case "B.Mask": return EntityHeader<PCBLayers>.Create(PCBLayers.BottomSolderMask);
                case "F.Paste": return EntityHeader<PCBLayers>.Create(PCBLayers.TopStencil);
                case "B.Paste": return EntityHeader<PCBLayers>.Create(PCBLayers.BottomStencil);
                case "F.Courtyard": return EntityHeader<PCBLayers>.Create(PCBLayers.TopRestrict);
                case "B.Courtyard": return EntityHeader<PCBLayers>.Create(PCBLayers.BottomRestrict);
                default:
                    return EntityHeader<PCBLayers>.Create(PCBLayers.Other);
            }
        }
    }
}
