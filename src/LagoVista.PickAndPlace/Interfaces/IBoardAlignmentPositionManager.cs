// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 72c78dcca01d0026a0f1db9f8f3c002acbeb23c8e40ee9d95979f9ea06925238
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Models.Drawing;


namespace LagoVista.PickAndPlace.Interfaces
{
    public interface IBoardAlignmentPositionManager
    {
        Point2D<double> BoardOriginPoint { get; set; }
        Point2D<double> FirstLocated { get; set; }
        bool HasCalculatedOffset { get; set; }
        Point2D<double> OffsetPoint { get; set; }
        double RotationOffset { get; set; }
        Point2D<double> SecondExpected { get; }
        Point2D<double> SecondLocated { get; set; }

        void Reset();
    }
}