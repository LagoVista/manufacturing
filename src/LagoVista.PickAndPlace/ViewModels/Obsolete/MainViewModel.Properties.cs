using LagoVista.Manufacturing.Models;
using LagoVista.PCB.Eagle.Models;
using LagoVista.PickAndPlace.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.ViewModels
{
    public partial class MainViewModel
    {
        public GCode.GCodeJobControlViewModel JobControlVM { get; private set; }
        public Machine.MachineControlViewModel MachineControlVM { get; private set; }

        PcbMillingProject _project;
        public PcbMillingProject Project
        {
            get { return _project; }
            set
            {
                _project = value;
                Machine.PCBManager.Project = value;
                RaisePropertyChanged();
            }
        }

        public MRUs MRUs
        {
            get; set;
        }
    }

    public class FileInfo
    {
        public string FullPath { get; set; }
        public string FileName { get; set; }
    }

    public class MRUs
    {
        public MRUs()
        {
            PnPJobs = new List<string>();
            GCodeFiles = new List<string>();
            BoardFiles = new List<string>();
            ProjectFiles = new List<string>();
        }

        public List<string> PnPJobs { get; set; }
        public List<string> GCodeFiles { get; set; }
        public List<string> BoardFiles { get; set; }
        public List<string> ProjectFiles { get; set; }
    }
}
