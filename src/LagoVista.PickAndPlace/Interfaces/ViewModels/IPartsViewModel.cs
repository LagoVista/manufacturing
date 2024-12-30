using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels
{
    public interface IPartsViewModel
    {
        Task<InvokeResult> SaveCurrentFeederAsync();
        Task<InvokeResult> RefreshAsync();

        StripFeeder CurrentStripFeeder { get; }
        AutoFeeder CurrentAutoFeeder { get; }
        ComponentPackage CurrentPackage { get; }
        PlaceableParts CurrentPlaceableParts { get; }
        Component CurrentComponentToBePlaced { get; }
    }
}
