// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: eff3a4225ceb6777c210bc2960256693461906de583d23c6a93aa021b8acae51
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Commanding;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Models;
using System.Collections.ObjectModel;
using System.Linq;

namespace LagoVista.PickAndPlace.ViewModels.Machine
{
    public class ErrorsViewModel : MachineViewModelBase, IErrorsViewModel
    {
        public ErrorsViewModel(IMachineRepo machineRepo) : base(machineRepo)
        {
            ClearCommand = RelayCommand<StatusMessage>.Create((StatusMessage) => Errors.Remove(StatusMessage));
        }

        protected override void MachineChanged(IMachine machine)
        {
            machine.Messages.CollectionChanged += Messages_CollectionChanged;

            base.MachineChanged(machine);
        }

        private void Messages_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            e.NewItems?.OfType<StatusMessage>().ToList().Where(msg=> 
            msg.MessageType == Manufacturing.Models.StatusMessageTypes.FatalError ||
            msg.MessageType == Manufacturing.Models.StatusMessageTypes.Warning
            ).ToList().ForEach(err => Errors.Add(err));
        }

        public ObservableCollection<StatusMessage> Errors { get; } = new ObservableCollection<StatusMessage>();

        public RelayCommand<StatusMessage> ClearCommand { get; }

        public void ClearAll()
        {
            Errors.Clear();
        }
    }
}
