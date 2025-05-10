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
