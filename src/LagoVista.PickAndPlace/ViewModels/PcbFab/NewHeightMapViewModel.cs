using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.ViewModels.Obsolete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.ViewModels.PcbFab
{
    public partial class NewHeightMapViewModel : GCodeAppViewModelBase
    {
      
        public NewHeightMapViewModel(IMachine machine) : base(machine)
        {
            InitCommands();
        }
    }
}
