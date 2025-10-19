// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 8764b1fed09a3892c01c04b75c853f5612be4260e14ccb90bce2ec8c5cfec0e0
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Models;

namespace LagoVista.PickAndPlace.ViewModels.PcbFab
{
    public partial class NewHeightMapViewModel : MachineViewModelBase
    {
      
        public NewHeightMapViewModel(IMachineRepo machineRepo) : base(machineRepo)
        {
            InitCommands();
        }

        public double MinX
        {
            get { return HeightMap.Min.X; }
            set { HeightMap.Min = new Core.Models.Drawing.Vector2(value, HeightMap.Min.Y); }
        }

        public double MinY
        {
            get { return HeightMap.Min.Y; }
            set { HeightMap.Min = new Core.Models.Drawing.Vector2(HeightMap.Min.Y, value); }
        }

        public double MaxX
        {
            get { return HeightMap.Max.X; }
            set { HeightMap.Max = new Core.Models.Drawing.Vector2(value, HeightMap.Max.Y); }
        }

        public double MaxY
        {
            get { return HeightMap.Max.Y; }
            set { HeightMap.Max = new Core.Models.Drawing.Vector2(HeightMap.Max.X, value); }
        }

        public double GridSize
        {
            get { return HeightMap.GridSize; }
            set { HeightMap.GridSize = value; }
        }

        private void InitCommands()
        {
            GenerateTestPatternCommand = new Core.Commanding.RelayCommand(GenerateTestPattern);
        }

        public Core.Commanding.RelayCommand GenerateTestPatternCommand { get; private set; }

        private HeightMap _heightMap;
        public HeightMap HeightMap
        {
            get { return _heightMap; }
            set { Set(ref _heightMap, value); }
        }

        public bool Validate()
        {
            if (HeightMap.Min.X > HeightMap.Max.X)
            {
                var originalMinX = HeightMap.Min.X;
                HeightMap.Min = new Core.Models.Drawing.Vector2()
                {
                    X = HeightMap.Max.X,
                    Y = HeightMap.Min.Y
                };

                HeightMap.Max = new Core.Models.Drawing.Vector2()
                {
                    X = originalMinX,
                    Y = HeightMap.Max.Y
                };
            }

            if (HeightMap.Min.Y > HeightMap.Max.Y)
            {
                var originalMinY = HeightMap.Min.Y;
                HeightMap.Min = new Core.Models.Drawing.Vector2()
                {
                    X = HeightMap.Min.X,
                    Y = HeightMap.Max.Y
                };

                HeightMap.Max = new Core.Models.Drawing.Vector2()
                {
                    X = HeightMap.Max.Y,
                    Y = originalMinY
                };
            }
            return true;
        }


        public void GenerateTestPattern()
        {

        }

    }
}
