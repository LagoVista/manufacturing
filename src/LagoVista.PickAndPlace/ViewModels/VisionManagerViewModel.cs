using LagoVista.Core.Commanding;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.Core.ViewModels;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels;
using LagoVista.PickAndPlace.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LagoVista.PickAndPlace.ViewModels
{
    public class VisionManagerViewModel : ViewModelBase, IVisionProfileManagerViewModel
    {
        private readonly IMachine _machine;

        private VisionSettings _topCameraProfile;
        private VisionSettings _bottomCameraProfile;

        public VisionManagerViewModel(IMachine machine)
        {
            _machine = machine;

            MVProfiles = new List<EntityHeader>()
            {
                EntityHeader.Create("default", "Default"),
                EntityHeader.Create("brdfiducual", "Board Fiducial"),
                EntityHeader.Create("mchfiducual", "Machine Fiducial"),
                EntityHeader.Create("tapehole", "Tape Hole"),
                EntityHeader.Create("tapeholewhite", "Tape Hole (White Tape)"),
                EntityHeader.Create("tapeholeblack", "Tape Hole (Black Tape)"),
                EntityHeader.Create("tapeholeclear", "Tape Hole (Clear Tape)"),
                EntityHeader.Create("squarepart", "Square Part"),
                EntityHeader.Create("nozzle", "Nozzle"),
                EntityHeader.Create("nozzlecalibration", "Nozzle Calibration"),
                EntityHeader.Create("partinspection", "Part Inspection"),
            };


            _currentMVProfile = MVProfiles.SingleOrDefault(mv => mv.Id == "default");
            LoadProfiles();

            SaveCommand = new RelayCommand(() => SaveProfile());
        }

        public InvokeResult SelectProfile(string profile )
        {
            return InvokeResult.Success;
        }

        public List<EntityHeader> MVProfiles { get; }

        EntityHeader _currentMVProfile;
        public EntityHeader CurrentMVProfile
        {
            get
            {
                if (_currentMVProfile == null)
                {
                    _currentMVProfile = MVProfiles.Single(pr => pr.Id == "default");
                }
                return _currentMVProfile;
            }
            set
            {
                SaveProfile();
                Set(ref _currentMVProfile, value);
                LoadProfiles();
            }
        }

        public void SelectMVProfile(string profile)
        {
            CurrentMVProfile = MVProfiles.SingleOrDefault(mvp => mvp.Id == profile);
        }

        public void SaveProfile()
        {
            if (CurrentMVProfile != null)
            {
                var existing = _machine.Settings.VisionProfiles.FirstOrDefault(vp => vp.Id == CurrentMVProfile.Id);
                if (existing == null)
                {
                    _machine.Settings.VisionProfiles.Add(new VisionProfile()
                    {
                        Name = CurrentMVProfile.Text,
                        Id = CurrentMVProfile.Id,
                        BottomProfile = _bottomCameraProfile,
                        TopProfile = _topCameraProfile,
                    });

                }
            }
        }

        private void LoadProfiles()
        {
            var profile = _machine.Settings.VisionProfiles.SingleOrDefault(prf => prf.Id == CurrentMVProfile.Id);
            if (profile == null)
            {
                var defaultProfile = _machine.Settings.VisionProfiles.FirstOrDefault(prof => prof.Name == "default");
                if (defaultProfile == null)
                {
                    defaultProfile = new VisionProfile()
                    {
                        Name = "Default",
                        Id = "fefault",
                        BottomProfile = new VisionSettings(),
                        TopProfile = new VisionSettings()
                    };
                }
                else
                {
                    profile = new VisionProfile()
                    {
                        Name = CurrentMVProfile.Id,
                        Id = CurrentMVProfile.Id,
                    };

                    // lazy way to clone an object, maybe not that efficient, but this won't happen often.
                    profile.BottomProfile = JsonConvert.DeserializeObject<VisionSettings>(JsonConvert.SerializeObject(defaultProfile.BottomProfile));
                    profile.TopProfile = JsonConvert.DeserializeObject<VisionSettings>(JsonConvert.SerializeObject(defaultProfile.TopProfile));
                };
            }
            else
            {
                _bottomCameraProfile = profile.BottomProfile;
                _topCameraProfile = profile.TopProfile;
            }


            SaveProfile();

            _machine.TopLightOn = _topCameraProfile.LightOn;
            _machine.TopRed = _topCameraProfile.LightRed;
            _machine.TopGreen = _topCameraProfile.LightGreen;
            _machine.TopBlue = _topCameraProfile.LightBlue;
            _machine.TopPower = _topCameraProfile.LightPower;

            _machine.BottomLightOn = _bottomCameraProfile.LightOn;
            _machine.BottomRed = _bottomCameraProfile.LightRed;
            _machine.BottomGreen = _bottomCameraProfile.LightGreen;
            _machine.BottomBlue = _bottomCameraProfile.LightBlue;
            _machine.BottomPower = _bottomCameraProfile.LightPower;
        }

        public VisionSettings BottomCameraProfile
        {
            get => _bottomCameraProfile;
            set => Set(ref _bottomCameraProfile, value);
        }

        public VisionSettings TopCameraProfile
        {
            get => _topCameraProfile;
            set => Set(ref _topCameraProfile, value);
        }


        public RelayCommand SaveCommand { get; }
    }
}
