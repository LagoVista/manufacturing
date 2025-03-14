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
