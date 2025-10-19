// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 7c122c8b124d6177c9603118ee244224633464e5a92c86202429ddb58ae688aa
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace
{
    public interface IPartInspectionViewModel
    {
        Task<InvokeResult> InspectAsync();
        Task<InvokeResult> InspectAsync(Component component);
        Task<InvokeResult> CenterPartAsync(Component component, PickAndPlaceJobPlacement placement, string algorithm = "", double? angle = null);

        Component CurrentComponent { get; set; }
    }
}
