using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.IOC;
using LagoVista.Core.Models;
using LagoVista.Core.Resources;
using LagoVista.Core.Validation;
using LagoVista.Core;
using LagoVista.PCB.Eagle.Managers;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.Resources;
using NUnit.Framework;
using System;
using System.Globalization;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Linq;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;

namespace LagoVista.Pcb.Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            var doc = XDocument.Load("PCB_v186.brd");
            var result =  EagleParser.ReadPCB(doc);
            foreach(var pck in result.Packages)
            {
                Console.WriteLine(pck.Name + " " + pck.IsSMD);
            }

            foreach(var prt in result.Components)
            {
                Console.WriteLine(prt.PackageName + " " + prt.Name + " " + prt.Value);
            }
        }

        [Test]
        public void ParseKicad()
        {
            var parser = new KicadImport();
            var buffer = System.IO.File.ReadAllBytes("mobo.kicad_pcb");
            using(var ms = new MemoryStream(buffer))
            {
              var result =  KicadImport.ReadPCB(ms);
                var j5 = result.Components.Where(cmp => cmp.Name == "J5").FirstOrDefault();
                foreach(var pad in j5.SMDPads)
                {
                    Console.WriteLine(pad.OriginX + " " + pad.OriginY + " " + pad.DX + " " + pad.DY + " " + pad.RotateStr);
                }
            }
        }
    }
} 