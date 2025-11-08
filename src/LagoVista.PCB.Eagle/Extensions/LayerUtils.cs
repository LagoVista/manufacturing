// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 21f23f3eef97a483e35f0c98249448df10580bdba3549886e527d7ee003c0f43
// IndexVersion: 2
// --- END CODE INDEX META ---
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
        //public static EntityHeader<PCBLayers> FromEagleLayer(this int layerNumber)
        //{
        //    switch (layerNumber)
        //    {
        //        case 1: return (PCBLayers.TopCopper);
        //        case 16: return (PCBLayers.BottomCopper);
        //        case 17: return (PCBLayers.Pads);
        //        case 18: return (PCBLayers.Vias);
        //        case 21: return (PCBLayers.TopSilk);
        //        case 22: return (PCBLayers.BottomSilk);
        //        case 44: return (PCBLayers.Drills);
        //        case 45: return (PCBLayers.Holes);
        //        case 20: return (PCBLayers.BoardOutline);
        //        case 51: return (PCBLayers.TopDocument);
        //        case 52: return (PCBLayers.BottomDocument);
        //        case 25: return (PCBLayers.TopNames);
        //        case 26: return (PCBLayers.BottomNames);
        //        case 27: return (PCBLayers.TopValues);
        //        case 28: return (PCBLayers.BottomValues);
        //        case 29: return (PCBLayers.TopSolderMask);
        //        case 30: return (PCBLayers.BottomSolderMask);
        //        case 31: return (PCBLayers.TopStencil);
        //        case 32: return (PCBLayers.BottomRestrict);
        //        case 41: return (PCBLayers.TopRestrict);
        //        case 42: return (PCBLayers.BottomRestrict);
        //        case 49: return (PCBLayers.TopDocument);
        //        case 47: return (PCBLayers.Measures);
        //        case 19: return (PCBLayers.Unrouted);
        //        default:
        //            {
        //                var eh = (PCBLayers.Other);
        //                eh.Text = $"Unspported layer: {layerNumber}";
        //                return eh;
        //            }
        //    };
        //}

        public static PCBLayers FromEagleLayer(this int layerNumber)
        {
            switch (layerNumber)
            {
                case 1: return (PCBLayers.TopCopper);
                case 16: return (PCBLayers.BottomCopper);
                case 17: return (PCBLayers.Pads);
                case 18: return (PCBLayers.Vias);
                case 21: return (PCBLayers.TopSilk);
                case 22: return (PCBLayers.BottomSilk);
                case 44: return (PCBLayers.Drills);
                case 45: return (PCBLayers.Holes);
                case 20: return (PCBLayers.BoardOutline);
                case 51: return (PCBLayers.TopDocument);
                case 52: return (PCBLayers.BottomDocument);
                case 25: return (PCBLayers.TopNames);
                case 26: return (PCBLayers.BottomNames);
                case 27: return (PCBLayers.TopValues);
                case 28: return (PCBLayers.BottomValues);
                case 29: return (PCBLayers.TopSolderMask);
                case 30: return (PCBLayers.BottomSolderMask);
                case 31: return (PCBLayers.TopStencil);
                case 32: return (PCBLayers.BottomRestrict);
                case 41: return (PCBLayers.TopRestrict);
                case 42: return (PCBLayers.BottomRestrict);
                case 49: return (PCBLayers.TopDocument);
                case 47: return (PCBLayers.Measures);
                case 19: return (PCBLayers.Unrouted);
                default: return PCBLayers.Other;
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
                case 51:
                case 52:
                case 49: return "#a0a0a0";
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
                case 47: return "#5fffff";
                default: return "#ffffff";
            };
        }

        public static PCBLayers FromKiCadLayer(this string layerName)
        {
            switch (layerName)
            {
                case "F.Cu": return (PCBLayers.TopCopper);
                case "B.Cu": return (PCBLayers.BottomCopper);
                case "F.Silkscreen": return (PCBLayers.TopSilk);
                case "F.SilkS": return (PCBLayers.TopSilk);
                case "B.Silkscreen": return (PCBLayers.BottomSilk);
                case "B.SilkS": return (PCBLayers.TopSilk);
                case "Edge.Cuts": return (PCBLayers.BoardOutline);
                case "F.Fab": return (PCBLayers.TopNames);
                case "B.Fab": return (PCBLayers.BottomNames);
                case "F.Mask": return (PCBLayers.TopSolderMask);
                case "B.Mask": return (PCBLayers.BottomSolderMask);
                case "F.Paste": return (PCBLayers.TopStencil);
                case "B.Paste": return (PCBLayers.BottomStencil);
                case "F.CrtYd": return (PCBLayers.TopRestrict);
                case "B.CrtYd": return (PCBLayers.BottomRestrict);
                case "F.Courtyard": return (PCBLayers.TopRestrict);
                case "B.Courtyard": return (PCBLayers.BottomRestrict);
                default: return PCBLayers.Other;
            }
        }

        //public static EntityHeader<PCBLayers> FromKiCadLayer(this string layerName)
        //{
        //    switch (layerName)
        //    {
        //        case "F.Cu": return (PCBLayers.TopCopper);
        //        case "B.Cu": return (PCBLayers.BottomCopper);
        //        case "F.Silkscreen": return (PCBLayers.TopSilk);
        //        case "F.SilkS": return (PCBLayers.TopSilk);
        //        case "B.Silkscreen": return (PCBLayers.BottomSilk);
        //        case "B.SilkS": return (PCBLayers.TopSilk);
        //        case "Edge.Cuts": return (PCBLayers.BoardOutline);
        //        case "F.Fab": return (PCBLayers.TopNames);
        //        case "B.Fab": return (PCBLayers.BottomNames);
        //        case "F.Mask": return (PCBLayers.TopSolderMask);
        //        case "B.Mask": return (PCBLayers.BottomSolderMask);
        //        case "F.Paste": return (PCBLayers.TopStencil);
        //        case "B.Paste": return (PCBLayers.BottomStencil);
        //        case "F.CrtYd": return (PCBLayers.TopRestrict);
        //        case "B.CrtYd": return (PCBLayers.BottomRestrict);
        //        case "F.Courtyard": return (PCBLayers.TopRestrict);
        //        case "B.Courtyard": return (PCBLayers.BottomRestrict);
        //        default:
        //            {
        //                var eh = (PCBLayers.Other);
        //                eh.Text = $"Unsupported Layer: {layerName}";
        //                return eh;
        //            }
        //    }
        //}
    }
}
