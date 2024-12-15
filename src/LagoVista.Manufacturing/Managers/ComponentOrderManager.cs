using LagoVista.Core.Interfaces;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.Manufacturing.Interfaces.Repos;
using LagoVista.Manufacturing.Models;
using System;
using static LagoVista.Core.Models.AuthorizeResult;
using System.Threading.Tasks;
using LagoVista.Manufacturing.Interfaces.Managers;
using LagoVista.Core.Managers;
using System.IO;
using System.Text.RegularExpressions;
using PdfSharpCore.Drawing;
using QRCoder;
using RingCentral;

namespace LagoVista.Manufacturing.Managers
{
    public class ComponentOrderManager : ManagerBase, IComponentOrderManager
    {
        private readonly IComponentOrderRepo _componentOrder;
        private readonly IComponentRepo _componentRepo;

        public ComponentOrderManager(IComponentOrderRepo compoentOrderRepo, IComponentRepo componentRepo,
            IAdminLogger logger, IAppConfig appConfig, IDependencyManager depmanager, ISecurity security) :
            base(logger, appConfig, depmanager, security)
        {
            _componentOrder = compoentOrderRepo ?? throw new ArgumentNullException(nameof(compoentOrderRepo));
            _componentRepo = componentRepo ?? throw new ArgumentNullException(nameof(componentRepo));
        }
        public async Task<InvokeResult> AddComponentOrderAsync(ComponentOrder order, EntityHeader org, EntityHeader user)
        {
            if (!String.IsNullOrEmpty(order.LineItemsCSV))
            {
                var lines = order.LineItemsCSV.Split('\n');
                var skippedHeader = false;
                foreach (var line in lines)
                {
                    if (!skippedHeader)
                        skippedHeader = true;
                    else
                    {
                        var trimmed = line.Trim();
                        if (!String.IsNullOrEmpty(trimmed))
                        {
                            var parser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
                            var parts = parser.Split(trimmed);
                            order.LineItems.Add(ComponentOrderLineItem.FromOrderLine(parts));
                        }
                    }
                }
            }

            await AuthorizeAsync(order, AuthorizeActions.Create, user, org);
            ValidationCheck(order, Actions.Create);
            await _componentOrder.AddComponentOrderAsync(order);

            return InvokeResult.Success;
        }

        public async Task<InvokeResult<Stream>> GenerateLabelsAsync(string id, EntityHeader org, EntityHeader user)
        {
            var order = await _componentOrder.GetComponentOrderAsync(id);
            await AuthorizeAsync(order, AuthorizeActions.Read, user, org, "Generate Part Labels");

            var ms = new MemoryStream();
            var pdf = new LagoVista.PDFServices.PDFGenerator();
            pdf.RowHeight = 52;
            pdf.RowBottomMargin = 20;
            pdf.ColumnRightMargin = 23;
            pdf.Margin = new PDFServices.Margin() { Left = 20, Right = 20, Top = 50, Bottom = 50 };
            pdf.StartDocument(false, false);
            pdf.StartTable(PDFServices.ColWidth.CreateStar(), PDFServices.ColWidth.CreateStar(), PDFServices.ColWidth.CreateStar());

            pdf.StartRow();
            var idx = 0;
            foreach (var lineItem in order.LineItems)
            {
                if (lineItem.Component != null)
                {
                    var component = await _componentRepo.GetComponentAsync(lineItem.Component.Id);
                    pdf.AddColText(PDFServices.Style.Small, idx, component.Name, align: XStringFormats.TopLeft);
                    pdf.AddColText(PDFServices.Style.Small, idx, $"{component.ComponentType.Text}/{component.ComponentPackage?.Text}", align: XStringFormats.CenterLeft);

                    var packageLine = component.ComponentType.Text;
                    if (!EntityHeader.IsNullOrEmpty(component.ComponentPackage))
                        packageLine += $"/{component.ComponentPackage.Text}";

                    pdf.AddColText(PDFServices.Style.Small, idx, $"{component.ComponentType.Text}/{component.ComponentPackage?.Text}", align: XStringFormats.CenterLeft);

                    var locationLine = String.Empty;
                    if (!String.IsNullOrEmpty(component.Room))
                        locationLine += component.Room;

                    if (!String.IsNullOrEmpty(component.ShelfUnit))
                        locationLine += $"/{component.ShelfUnit}";

                    if (!String.IsNullOrEmpty(component.Shelf))
                        locationLine += $"/{component.Shelf}";

                    if (!String.IsNullOrEmpty(component.Column))
                        locationLine += $"/{component.Column}";

                    if (!String.IsNullOrEmpty(component.Bin))
                        locationLine += $"/{component.Bin}";

                    pdf.AddColText(PDFServices.Style.Small, idx, locationLine, align: XStringFormats.BottomLeft);

                    using (var qrGenerator = new QRCodeGenerator())
                    using (var qrCodeData = qrGenerator.CreateQrCode($"https://www.nuviot.com/mfg/component/{lineItem.Component.Id}.", QRCodeGenerator.ECCLevel.Q))
                    using (var qrCode = new PngByteQRCode(qrCodeData))
                    using (var qrMS = new MemoryStream(qrCode.GetGraphic(20)))
                        pdf.AddColImage(idx, qrMS, 64, 64, align: XStringFormats.CenterRight);

                    if (++idx == 3)
                    {
                        pdf.EndRow();
                        pdf.StartRow();
                        idx = 0;
                    }
                }
            }

            pdf.EndTable();

            pdf.Write(ms, false);
            return InvokeResult<Stream>.Create(ms);
        }

  
        public async Task<DependentObjectCheckResult> CheckInUseAsync(string id, EntityHeader org, EntityHeader user)
        {
            var part = await _componentOrder.GetComponentOrderAsync(id);
            await AuthorizeAsync(part, AuthorizeActions.Read, user, org);
            return await base.CheckForDepenenciesAsync(part);
        }

        public Task<InvokeResult> DeleteCommponentAsync(string id, EntityHeader org, EntityHeader user)
        {
            throw new NotImplementedException();
        }

        public async Task<InvokeResult> DeleteComponentOrderAsync(string id, EntityHeader org, EntityHeader user)
        {
            var part = await _componentOrder.GetComponentOrderAsync(id);
            await ConfirmNoDepenenciesAsync(part);
            await AuthorizeAsync(part, AuthorizeActions.Delete, user, org);
            await _componentOrder.DeleteComponentOrderAsync(id);
            return InvokeResult.Success;
        }

        public async Task<ComponentOrder> GetComponentOrderAsync(string id, EntityHeader org, EntityHeader user)
        {
            var part = await _componentOrder.GetComponentOrderAsync(id);
            await AuthorizeAsync(part, AuthorizeActions.Read, user, org);
            return part;
        }

        public async Task<ListResponse<ComponentOrderSummary>> GetComponentOrdersSummariesAsync(ListRequest listRequest, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org.Id, typeof(ComponentOrder));
            return await _componentOrder.GetComponentOrderSummariesAsync(org.Id, listRequest);
        }

        public async Task<InvokeResult> UpdateComponentOrderAsync(ComponentOrder order, EntityHeader org, EntityHeader user)
        {
            await AuthorizeAsync(order, AuthorizeActions.Update, user, org);
            ValidationCheck(order, Actions.Update);
            await _componentOrder.UpdateComponentOrderAsync(order);

            return InvokeResult.Success;
        }
    }
}
