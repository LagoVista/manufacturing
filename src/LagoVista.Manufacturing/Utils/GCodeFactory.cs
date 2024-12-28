using LagoVista.Core.Models;
using LagoVista.Manufacturing.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.Manufacturing.Utils
{

    public class GCodeFactory
    {
        Machine _machine;
        CircuitBoard _board;
        List<StripFeeder> _stripFeeders;
        List<AutoFeeder> _autoFeeders;

        public GCodeFactory(Machine machine, CircuitBoard board, List<StripFeeder> stripFeeders, List<AutoFeeder> autoFeeders)
        {
            _machine = machine;
            _board = board;
            _stripFeeders = stripFeeders;
            _autoFeeders = autoFeeders;
        }

        public string GetForPartInFeeder(EntityHeader component)
        {
            throw new NotImplementedException();
        }

        public string GetForPartOnBoard(EntityHeader part)
        {

            throw new NotImplementedException();
        }

        public string GetForMachineOrigin()
        {

            throw new NotImplementedException();
        }

        public string GetForBoardOrigin()
        {
            throw new NotImplementedException();
        }
    }
}
