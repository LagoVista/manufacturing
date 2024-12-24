using LagoVista.PCB.Eagle.Managers;
using NUnit.Framework;
using System;
using System.Xml.Linq;
using System.Linq;
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
        public void ParseEagle()
        {
            var doc = XDocument.Load("PCB_v78.brd");
            var result =  EagleParser.ReadPCB(doc);
            foreach(var pck in result.Packages)
            {
                Console.WriteLine(pck.Name + " " + pck.IsSMD);
            }

            foreach(var prt in result.Components)
            {
                Console.WriteLine(prt.PackageName + " " + prt.Name + " " + prt.Value + "  " + prt.Package.Value.Key);
            }
        }

        [Test]
        public void ParseKicad()
        {
            var parser = new KicadImport();
            var buffer = System.IO.File.ReadAllBytes("mobo.kicad_pcb");
            using(var ms = new MemoryStream(buffer))
            {
              var result =  KicadImport.ImportPCB(ms);

                foreach (var pck in result.Packages)
                {
                    Console.WriteLine($"{pck.Key}");
                }

                var json = JsonConvert.SerializeObject(result);

                foreach(var cmp in result.Components)
                {
                    Console.WriteLine($"{cmp.Name} - {cmp.Package.Key}");
                }
               
            }
        }
    }
} 