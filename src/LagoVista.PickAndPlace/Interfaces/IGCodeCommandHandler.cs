// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: c52e7b5613a2513ac59281642f6a1f91620b8da180f479e8776246b3f2b949f0
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.GCode.Commands;

namespace LagoVista.PickAndPlace.Interfaces
{
    public interface IGCodeCommandHandler
    {
        bool HasValidFile { get; }
        bool IsCompleted { get; }
        int CommandAcknowledged();

        GCodeCommand CurrentCommand { get; }

        GCodeCommand GetNextJobItem();
    }
}
