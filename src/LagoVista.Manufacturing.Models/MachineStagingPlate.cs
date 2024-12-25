using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.Manufacturing.Models
{
    public class MachineStagingPlate : ModelBase, IFormDescriptor
    {

        private decimal _firstHoleXOffset;
        public decimal FirstHoleXOffset
        {
            get => _firstHoleXOffset;
            set => Set(ref _firstHoleXOffset, value);
        }

        private decimal _firstHoleYOffset;
        public decimal FirstHoleUOffset
        {
            get => _firstHoleYOffset;
            set => Set(ref _firstHoleYOffset, value);
        }

        public List<string> GetFormFields()
        {
            throw new NotImplementedException();
        }
    }
}
