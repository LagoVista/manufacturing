// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 199d200004bdf6cbe4cccc1007643b53f5a38dc6fb555e74b6bfbdde6a01e16d
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Models.Drawing;

namespace LagoVista.PickAndPlace.Interfaces
{
    public interface IPointStabilizationFilter
    {
        void Add(Point2D<double> cameraOffsetPixels);

        bool HasStabilizedPoint { get; }

        int PointCount { get; }

        void Reset();

        Point2D<double> StabilizedPoint { get; }
    }
}
