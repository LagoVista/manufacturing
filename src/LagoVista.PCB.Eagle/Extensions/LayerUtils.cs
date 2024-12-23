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
        public static EntityHeader<PCBLayers> ToCommonLayer(this int layerNumber)
        {
            return switch (layerNumber)
            {
                1 => EntityHeader<PCBLayers>.Create(PCBLayers.TopCopper),
                16 => EntityHeader<PCBLayers>.Create(PCBLayers.BottomCopper),
                21 => EntityHeader<PCBLayers>.Create(PCBLayers.TopSilk),
                22 => EntityHeader<PCBLayers>.Create(PCBLayers.BottomSilk),
                44 => EntityHeader<PCBLayers>.Create(PCBLayers.Drills),
                45 => EntityHeader<PCBLayers>.Create(PCBLayers.Holes),
                20 => EntityHeader<PCBLayers>.Create(PCBLayers.BoardOutline),
                51 => EntityHeader<PCBLayers>.Create(PCBLayers.TopDocument),
                52 => EntityHeader<PCBLayers>.Create(PCBLayers.BottomDocument),
                25 => EntityHeader<PCBLayers>.Create(PCBLayers.TopNames),
                27 => EntityHeader<PCBLayers>.Create(PCBLayers.TopValues),
                29 => EntityHeader<PCBLayers>.Create(PCBLayers.TopSolderMask),
                30 => EntityHeader<PCBLayers>.Create(PCBLayers.BottomSolderMask),
                31 => EntityHeader<PCBLayers>.Create(PCBLayers.TopStencil),
                32 => EntityHeader<PCBLayers>.Create(PCBLayers.BottomStayOut),
                28 => EntityHeader<PCBLayers>.Create(PCBLayers.BottomValues),
                26 => EntityHeader<PCBLayers>.Create(PCBLayers.BottomNames),
                41 => EntityHeader<PCBLayers>.Create(PCBLayers.TopStayOut),
                42 => EntityHeader<PCBLayers>.Create(PCBLayers.BottomStayOut),
                _ => throw new Exception($"Unknown layer number {layerNumber}")
            };
    }
}
}
