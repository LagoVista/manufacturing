﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PCB.Eagle.Models
{
    public class BOMEntry
    {
        public BOMEntry()
        {
            Components = new List<PcbComponent>();
        }

        public PhysicalPackage Package { get;  internal set; }

        public string Value { get; internal set; }

        public List<PcbComponent> Components { get; private set; }

        public override string ToString()
        {
            return $"{Package.Name} - {Value}, QTY: {Components.Count}";
        }
    }
}
