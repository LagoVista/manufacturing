using LagoVista.Client.Core;
using LagoVista.Core.Commanding;
using LagoVista.Core.IOC;
using LagoVista.Core.Models;
using LagoVista.Core.Models.Drawing;
using LagoVista.Core.ViewModels;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Vision;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace LagoVista.PickAndPlace.App.ViewModels
{
    public enum PositionType
    {
        ReferenceHole,
        FirstPart,
        CurrentPart,
        LastPart,
        TempPartIndex,
    }

    public class StripFeederViewModel : ViewModelBase
    {
        private PnPJobViewModel _jobVM;
        public IMachine _machine;
        string _pnpJobFileName;
        private int _tempPartIndex = 0;

        private MachineStagingPlate _plate;
        private StripFeeder _stripFeeder;

        private ILocatorViewModel _locatorViewModel;
        private IVisionProfileManagerViewModel _visionProfileManagerViewModel;

        public StripFeederViewModel(IMachine machine, IVisionProfileManagerViewModel visionProfileViewModel, ILocatorViewModel locatorViewModel, PnPJobViewModel jobVM)
        {
            _machine = machine;
            _jobVM = jobVM;
            _locatorViewModel = locatorViewModel;
            _visionProfileManagerViewModel = visionProfileViewModel; ;

            GoToCurrentPartCommand = new RelayCommand(() => GoToPart(PositionType.CurrentPart), () => _selectedStripFeederRow != null);
            GoToFirstPartCommand = new RelayCommand(() => GoToPart(PositionType.FirstPart), () => _selectedStripFeederRow != null);
            GoToLastPartCommand = new RelayCommand(() => GoToPart(PositionType.LastPart), () => _selectedStripFeederRow != null);
            NextPartCommand = new RelayCommand(NextPart, () => SelectedStripFeederRow != null && SelectedStripFeederRow != null && AvailablePartCount > _tempPartIndex);
            PrevPartCommand = new RelayCommand(PrevPart, () => SelectedStripFeederRow != null && SelectedStripFeederRow != null && _tempPartIndex > 0);

            SetStripFeederRowOffsetCommand = new RelayCommand(() =>
            {
                CurrentStripFeederRow.RefHoleOffset = (_machine.MachinePosition.ToPoint2D() - _stripFeeder.Origin).Round(2);
            }, () => CurrentStripFeederRow != null);

            SetCurrentPartIndexCommand = new RelayCommand(SetCurrentPartIndex, () => SelectedStripFeederRow != null);            
        }

        public int AvailablePartCount
        {
            get { return -1; }
        }

        public void SetMachine(string fileName)
        {
            _pnpJobFileName = fileName;
            SaveStripFeederCommand = new RelayCommand(async () =>
            {
                await SaveStripFeeder();
                
                CurrentStripFeederRow = null;

                RefreshCommandEnabled();
            }, () => CurrentStripFeederRow != null);

            CancelStripFeederCommand = new RelayCommand(() =>
            {
                CurrentStripFeederRow = null;
                RefreshCommandEnabled();
            }, () => CurrentStripFeederRow != null);


            GoToStripFeederCommand = new RelayCommand(GoToStripFeeder, () => CurrentStripFeederRow != null);

            ClearStripXOffsetCommand = new RelayCommand(() =>
            {
                CurrentStripFeederRow.RefHoleOffset = null;
            }, () => CurrentStripFeederRow != null);
        }

        public void RefreshCommandEnabled()
        {
            AddStripFeederCommand.RaiseCanExecuteChanged();
            AddStripFeederPackageCommand.RaiseCanExecuteChanged();

            SaveStripFeederCommand.RaiseCanExecuteChanged();
            
            CancelStripFeederCommand.RaiseCanExecuteChanged();
            SetStripFeederPackageBottomLeftCommand.RaiseCanExecuteChanged();
            SetStripFeederPackageSefaultXCommand.RaiseCanExecuteChanged();
            ClearStripXOffsetCommand.RaiseCanExecuteChanged();
            GoToStripFeederCommand.RaiseCanExecuteChanged();
            GoToFirstPartCommand.RaiseCanExecuteChanged();
            GoToLastPartCommand.RaiseCanExecuteChanged();
            GoToCurrentPartCommand.RaiseCanExecuteChanged();

            SetCurrentPartIndexCommand.RaiseCanExecuteChanged();
            NextPartCommand.RaiseCanExecuteChanged();
            PrevPartCommand.RaiseCanExecuteChanged();
        }

        public void GoToPart(PositionType positionType)
        {
            _machine.SendCommand($"G0 Z{_machine.Settings.ProbeSafeHeight.ToDim()}");

            var feedRate = _machine.Settings.FastFeedRate;
            var positon = GetCurrentPartPosition(_selectedStripFeederRow, positionType);

            var package = _selectedStripFeederRow.Component.Value.ComponentPackage.Value;
            _machine.GotoPoint(positon.X, positon.Y, feedRate);
            //_jobVM.PartSizeWidth = Convert.ToInt32(package.Width * 8);
            //_jobVM.PartSizeHeight = Convert.ToInt32(package.Length * 8);

            //_jobVM.SelectMVProfile("squarepart");

            if (positionType == PositionType.CurrentPart)
                _tempPartIndex = _selectedStripFeederRow.CurrentPartIndex;

            RefreshCommandEnabled();
        }

        private void GotoTempPartLocation()
        {
            _machine.SendCommand($"G0 Z{_machine.Settings.ProbeSafeHeight.ToDim()}");

            var location = GetCurrentPartPosition(SelectedStripFeederRow, PositionType.TempPartIndex);
            if (location != null)
            {
                var deltaX = Math.Abs(location.X - _machine.MachinePosition.X);
                var deltaY = Math.Abs(location.Y - _machine.MachinePosition.Y);

                //    var feedRate = (deltaX < 30 && deltaY < 30) ? 300 : Machine.Settings.FastFeedRate;

                _machine.GotoPoint(location.X, location.Y, _machine.Settings.FastFeedRate);
            }

            RefreshCommandEnabled();
        }

        public async void SetCurrentPartIndex()
        {

                SelectedStripFeederRow.CurrentPartIndex = _tempPartIndex;
                await SaveStripFeeder();
        }

        private async Task SaveStripFeeder()
        {
            var restClient = SLWIOC.Get<IRestClient>();
            await restClient.PutAsync("/api/mfg/stripfeeder", this._stripFeeder);
        }


        public void NextPart()
        {
            if (_tempPartIndex < AvailablePartCount)
            {
                _tempPartIndex++;
                GotoTempPartLocation();
            }
        }

        public void PrevPart()
        {
            if (_tempPartIndex > 0)
            {
                _tempPartIndex--;
                GotoTempPartLocation();
            }
        }
        private void GoToStripFeeder()
        {
            if (_stripFeeder == null)
            {
                MessageBox.Show("Can not go to strip feeder, no current feeder strip row selected.");
                return;
            }

            if(_stripFeeder.Origin == null)
            {
                MessageBox.Show("Can not go to strip feeder, could not find origin on strip feeder.");
                return;
            }

            _machine.SendSafeMoveHeight();            

            _visionProfileManagerViewModel.SelectProfile("tapehole");
            _machine.TopLightOn = false;
            _machine.BottomLightOn = false;
        }


        StripFeederRow _currentStripFeederRow;
        public StripFeederRow CurrentStripFeederRow
        {
            get => _currentStripFeederRow;
            set
            {
                Set(ref _currentStripFeederRow, value);
                if (value != null)
                {
                    GoToStripFeeder();
                    RaisePropertyChanged(nameof(SelectedStripFeederRow));
                }

                RefreshCommandEnabled();
            }
        }
        

        public Point2D<double> GetCurrentPartPosition(StripFeederRow feederRow, PositionType positionType = PositionType.ReferenceHole)
        {
                //var part = feederPackage.StripFeeders.FirstOrDefault(sf => sf.PartStrip?.Id == feederRow?.Id);
                //var referenceHoleX = feederPackage.LeftX + (part.RefHoleXOffset.HasValue ? part.RefHoleXOffset.Value : feederPackage.DefaultRefHoleXOffset);
                //var referenceHoleY = feederPackage.BottomY + part.RefHoleYOffset;
                //var package = _machineComponentManager.Packages.FirstOrDefault(pck => pck.Id == feederRow?.PackageId);
                //if (package == null)
                //{
                //    throw new ArgumentNullException("Could not find package for part.");
                //}

            if (feederRow.Component != null)
            {
                MessageBox.Show($"Can not get current position, row does not have a component associated with it Feeder: {_stripFeeder.Name}, Row: {feederRow.Id}. ");
                return null;
            }

            var component = feederRow.Component.Value;

            if(feederRow.Component.Value.ComponentPackage == null || !feederRow.Component.Value.ComponentPackage.HasValue)
            {
                MessageBox.Show($"Can not get part position, component {component.Name} row does not have a package associated with it Feeder: {_stripFeeder.Name}, Row: {feederRow.Id}. ");
                return null;
            }

            var package = component.ComponentPackage.Value;
            if(EntityHeader.IsNullOrEmpty(package.TapePitch))
            {
                MessageBox.Show($"Can not get part position, tape pitch on {component.Name}, package: {package.Name} does not have a package associated with it Feeder: {_stripFeeder.Name}, Row: {feederRow.Id}. ");
                return null;
            }

            var xScaler = 1;

            var referenceHole = new Point2D<double>(_stripFeeder.Origin.X + feederRow.RefHoleOffset.X, _stripFeeder.Origin.X + feederRow.RefHoleOffset.X);

            switch (positionType)
            {
                case PositionType.ReferenceHole:
                        _tempPartIndex = 0;                        
                    return referenceHole;

                case PositionType.FirstPart:
                        _tempPartIndex = 0;

                    return referenceHole + package.PickLocation;

                case PositionType.CurrentPart:
                    {
                        _tempPartIndex = feederRow.CurrentPartIndex;
                        var xOffset = feederRow.CurrentPartIndex * package.SpacingX.Value * xScaler;
                        return (referenceHole + package.PickLocation).AddToX(xOffset);
                    }
                case PositionType.TempPartIndex:
                    {
                        var xOffset = _tempPartIndex * package.SpacingX.Value * xScaler;
                        return (referenceHole + package.PickLocation).AddToX(xOffset);
                    }
                case PositionType.LastPart:
                    {
                        var xOffset = AvailablePartCount * package.SpacingX.Value * xScaler;
                        _tempPartIndex = Convert.ToInt32(AvailablePartCount);
                        return (referenceHole + package.PickLocation).AddToX(xOffset);
                    }
            }

            return null;
        }

        
        ObservableCollection<StripFeederRow> _partStrips;
        public ObservableCollection<StripFeederRow> PartStrips 
        {
            get => _partStrips;
            set => Set(ref _partStrips, value);
        }

        #region Commands
        public RelayCommand AddStripFeederCommand { get; private set; }
        public RelayCommand AddStripFeederPackageCommand { get; private set; }

        public RelayCommand SaveStripFeederCommand { get; private set; }

        public RelayCommand CancelStripFeederCommand { get; private set; }

        public RelayCommand SetStripFeederPackageBottomLeftCommand { get; private set; }

        public RelayCommand SetStripFeederPackageSefaultXCommand { get; private set; }

        public RelayCommand ClearStripXOffsetCommand { get; private set; }
        public RelayCommand SetStripFeederRowOffsetCommand { get; private set; }

        public RelayCommand GoToStripFeederCommand { get; private set; }

        public RelayCommand GoToCurrentPartCommand { get; }
        public RelayCommand GoToFirstPartCommand { get; }
        public RelayCommand GoToLastPartCommand { get; }

        public RelayCommand SetCurrentPartIndexCommand { get; }
        public RelayCommand NextPartCommand { get; }
        public RelayCommand PrevPartCommand { get; }
        #endregion

        private StripFeederRow _selectedStripFeederRow;
        public StripFeederRow SelectedStripFeederRow
        {
            get => _selectedStripFeederRow;
            set
            {
                Set(ref _selectedStripFeederRow, value);
            }
        }
    }
}
