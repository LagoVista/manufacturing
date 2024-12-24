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
            var result = EagleParser.ReadPCB(doc);
            var r1 = result.Components.FirstOrDefault(res => res.Name == "R1");

            foreach (var pad in r1.Package.Value.SmdPads)
            {
                Console.WriteLine($"{pad.Name} + {pad.X1}x{pad.Y1} - ({pad.X2}x{pad.Y2}) O={pad.OriginX}x{pad.OriginY} - size={pad.DX}x{pad.DY} ");
            }
        }


        [Test]
        public void FindResistors()
        {
            {
                var doc = XDocument.Load("PCB_v78.brd");
                var result = EagleParser.ReadPCB(doc);
                var r1 = result.Components.FirstOrDefault(res => res.Name == "R1");

                foreach (var pad in r1.Package.Value.SmdPads)
                {
                    Console.WriteLine($"F350  -> Start=({pad.X1}x{pad.Y1}) - End=({pad.X2} x {pad.Y2}) O=({pad.OriginX} x {pad.OriginY}) - size=({pad.DX} x {pad.DY})");
                }
            }

            var parser = new KicadImport();
            var buffer = System.IO.File.ReadAllBytes("mobo.kicad_pcb");
            using (var ms = new MemoryStream(buffer))
            {
                var result = KicadImport.ImportPCB(ms);
                var json = JsonConvert.SerializeObject(result);
                var r1 = result.Components.FirstOrDefault(res => res.Name == "R1");

                foreach (var pad in r1.Package.Value.SmdPads)
                {
                    Console.WriteLine($"KICAD -> Start=({pad.X1}x{pad.Y1}) - End=({pad.X2} x {pad.Y2}) O=({pad.OriginX} x {pad.OriginY}) - size=({pad.DX} x {pad.DY})");
                }
            }
        }

        [Test]
        public void ParseKicad()
        {
            var parser = new KicadImport();
            var buffer = System.IO.File.ReadAllBytes("mobo.kicad_pcb");
            using (var ms = new MemoryStream(buffer))
            {
                var result = KicadImport.ImportPCB(ms);
                var json = JsonConvert.SerializeObject(result);
                var r1 = result.Components.FirstOrDefault(res => res.Name == "R1");

                foreach (var pad in r1.Package.Value.SmdPads)
                {
                    Console.WriteLine($"{pad.Name} + {pad.X1}x{pad.Y1} - ({pad.X2}x{pad.Y2}) O={pad.OriginX}x{pad.OriginY} - size={pad.DX}x{pad.DY} ");
                }
            }
        }
    }
}