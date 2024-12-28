using LagoVista.Core.Models.Drawing;
using LagoVista.Manufacturing.Models;
using LagoVista.PCB.Eagle.Models;
using LagoVista.PickAndPlace.Models;
using LagoVista.PickAndPlace.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.App.ViewModels
{
    public partial class PnPJobViewModel
    {
        private PickAndPlaceJob _job;
        public PickAndPlaceJob Job
        {
            get { return _job; }
            set
            {
                _isDirty = false;
                SaveCommand.RaiseCanExecuteChanged();
                Set(ref _job, value);
                RaisePropertyChanged(nameof(HasJob));
            }
        }

        public String ProgressOnPart
        {
            get
            {
                if (SelectedPart == null)
                {
                    return "-";
                }

                return $"Placing part {_partIndex} of {SelectedPart.Count}";
            }
        }

        public string FileName
        {
            get;
            set;
        }

        BuildFlavor _selectedBuildFlavor;
        public BuildFlavor SelectedBuildFlavor
        {
            get => _selectedBuildFlavor;
            set               
            {
                Set(ref _selectedBuildFlavor, value);
                PopulateConfigurationParts();
            }
        }

        public ObservableCollection<BuildFlavor> BuildFlavors { get; set; } = new ObservableCollection<BuildFlavor>();

        public ObservableCollection<PcbComponent> PartsToBePlaced { get; set; } = new ObservableCollection<PcbComponent>();

        public bool HasJob { get { return Job != null; } }

        public bool IsDirty
        {
            get { return _isDirty; }
            set { Set(ref _isDirty, value); }
        }

        public bool IsEditing
        {
            get { return _isEditing; }
            set { Set(ref _isEditing, value); }
        }

        public ObservableCollection<AutoFeeder> AutoFeeders { get; set; }
        public ObservableCollection<LagoVista.Manufacturing.Models.StripFeeder> StripFeeders { get; set; }


        public string TargetAngle { get => $"Rotation: {_targetAngle}"; }

        public ObservableCollection<string> CalibrationUpdates { get; } = new ObservableCollection<string>();


        public double? RotationInTape
        {
            get
            {
                if (SelectedPart != null && SelectedPartStrip != null && SelectedPartPackage != null)
                {
                    return SelectedPartPackage.RotationInTape;
                }
                else
                {
                    return null;
                }
            }
        }

        public double? XPartInTray
        {
            get
            {
                if (SelectedPart != null && SelectedPartStrip != null && SelectedPartPackage != null)
                {
                    //var xCorrection = SelectedPart.PartStrip.CorrectionAngleX * ((SelectedPartRow.RowNumber - 1) * SelectedPart.PartPack.RowCount);
                    var partLocationRatio = (double)SelectedPartStrip.CurrentPartIndex / (double)SelectedPartStrip.AvailablePartCount;
                    var xOffset = SelectedPartStrip.CorrectionFactorX * partLocationRatio;

                    return SelectedPart.PartStrip.ReferenceHoleX + (SelectedPartStrip.CurrentPartIndex * SelectedPartPackage.SpacingX) + SelectedPartPackage.CenterXFromHole + xOffset;
                }
                else
                {
                    return null;
                }
            }
        }

        public double? YPartInTray
        {
            get
            {
                if (SelectedPart != null && SelectedPartStrip != null && SelectedPartPackage != null)
                {
                    var partLocationRatio = (double)SelectedPartStrip.CurrentPartIndex / (double)SelectedPartStrip.AvailablePartCount;
                    var yOffset = SelectedPartStrip.CorrectionFactorY * partLocationRatio;
                    return SelectedPart.PartStrip.ReferenceHoleY + SelectedPartPackage.CenterYFromHole + yOffset;
                }
                else
                {
                    return null;
                }
            }
        }

        public PackageLibraryViewModel PackageLibraryVM
        {
            get;
        }

        public PartStripsViewModel PartStripsViewModel
        {
            get;
        }

        public StripFeederViewModel StripFeederVM
        {
            get;
        }

        PnPMachine _pnpMachine;
        public PnPMachine PnPMachine
        {
            get => _pnpMachine;
            set
            {
                Set(ref _pnpMachine, value);
                RaisePropertyChanged(nameof(Packages));
            }
        }

        public ObservableCollection<PickAndPlace.Models.Package> Packages
        {
            get { return _pnpMachine?.Packages; }
        }


        public ToolAlignmentViewModel ToolAlignmentVM { get; }



        PcbComponent _selectedComponent;
        public PcbComponent SelectedComponent
        {
            get => _selectedComponent;
            set { Set(ref _selectedComponent, value);  }
        }

        public PartStrip SelectedPartStrip
        {
            get => SelectedPart?.PartStrip;
        }

        public CircuitBoardRevision Board
        {
            get { return Job.BoardRevision; }
        }

        PickAndPlace.Models.Package _selectedPartPackage;
        public PickAndPlace.Models.Package SelectedPartPackage
        {
            get => _selectedPartPackage;
            set => Set(ref _selectedPartPackage, value);
        }

        PcbComponent _selectPartToBePlaced;
        public PcbComponent SelectedPartToBePlaced
        {
            get { return _selectPartToBePlaced; }
            set
            {

                Set(ref _selectPartToBePlaced, value);

                if (value != null)
                {
                    _partIndex = SelectedPart.Parts.IndexOf(value);                    
                    SetBoardOffsetCommand.RaiseCanExecuteChanged();
                    ClearBoardOffsetCommand.RaiseCanExecuteChanged();
                    GoToPartOnBoard();
                }
            }
        }

        public Point2D<double> BoardOffset =>new Point2D<double>(0, 0);// : _job.BoardOffset;

        //public ObservableCollection<PcbComponent> Parts
        //{
        //    //get { return _job == null ? new ObservableCollection<PcbComponent>() : Job.BoardRevision.PcbComponents; }
        //}

        public ObservableCollection<PlaceableParts> ConfigurationParts { get; } = new ObservableCollection<PlaceableParts>();

        private PlaceableParts _selectedPart;
        public PlaceableParts SelectedPart
        {
            get { return _selectedPart; }
            set
            {
                Set(ref _selectedPart, value);

                if (value != null && _pnpMachine != null)
                {
                    SelectedPartPackage = _pnpMachine.Packages.Where(pck => pck.Name == _selectedPart.Package).FirstOrDefault();
                }
                else
                {
                    SelectedPartPackage = null;
                }

                RaisePropertyChanged(nameof(SelectedPartStrip));
                RaisePropertyChanged(nameof(XPartInTray));
                RaisePropertyChanged(nameof(RotationInTape));
                RaisePropertyChanged(nameof(YPartInTray));

                MoveToNextComponentInTapeCommand.RaiseCanExecuteChanged();
                MoveToPreviousComponentInTapeCommand.RaiseCanExecuteChanged();
                ResetCurrentComponentCommand.RaiseCanExecuteChanged();
                PlaceCurrentPartCommand.RaiseCanExecuteChanged();
                PlaceAllPartsCommand.RaiseCanExecuteChanged();
                GoToCurrentPartInStripCommand.RaiseCanExecuteChanged();
                GoToRefHoleCommand.RaiseCanExecuteChanged();
                SetRefHoleCommand.RaiseCanExecuteChanged();
            }
        }

    }
}
