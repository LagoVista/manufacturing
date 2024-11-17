using LagoVista.PCB.Eagle.Managers;
using NUnit.Framework;
using System;
using System.Xml.Linq;

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
    }
}