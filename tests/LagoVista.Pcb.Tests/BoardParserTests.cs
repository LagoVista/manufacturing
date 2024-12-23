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
                var j5 = result.Components.Where(cmp => cmp.Name == "J5").FirstOrDefault();
                foreach(var pad in j5.SMDPads)
                {
                    Console.WriteLine(pad.OriginX + " " + pad.OriginY + " " + pad.DX + " " + pad.DY + " " + pad.RotateStr);
                }

                foreach (var cmp in result.Components)
                {
                    cmp.Package.Value = null;
                }

                foreach (var layer in result.Layers)
                {
                    Console.WriteLine("Layer:" + layer.Name + "; " + layer.Key + ";  " + layer.Layer.Text);
                }

                var json = JsonConvert.SerializeObject(result);
                
            }
        }
    }
} 