using LagoVista.Core.Interfaces;
using LagoVista.Core.Managers;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Interfaces.Managers;
using LagoVista.Manufacturing.Interfaces.Repos;
using LagoVista.Manufacturing.Models;
using LagoVista.IoT.Logging.Loggers;
using System;
using System.Threading.Tasks;
using static LagoVista.Core.Models.AuthorizeResult;
using System.Linq;
using PdfSharpCore.Drawing;
using QRCoder;
using System.IO;

namespace LagoVista.Manufacturing.Managers
{
    public class ComponentManager : ManagerBase, IComponentManager
    {
        private readonly IComponentRepo _componentRepo;

        public ComponentManager(IComponentRepo partRepo, IAdminLogger logger, IAppConfig appConfig, IDependencyManager depmanager, ISecurity security) :
            base(logger, appConfig, depmanager, security)
        {
            _componentRepo = partRepo;
        }
        public async Task<InvokeResult> AddComponentAsync(Component part, EntityHeader org, EntityHeader user)
        {
            await AuthorizeAsync(part, AuthorizeActions.Create, user, org);
            ValidationCheck(part, Actions.Create);
            await _componentRepo.AddComponentAsync(part);

            return InvokeResult.Success;
        }

        public async Task<DependentObjectCheckResult> CheckInUseAsync(string id, EntityHeader org, EntityHeader user)
        {
            var part = await _componentRepo.GetComponentAsync(id);
            await AuthorizeAsync(part, AuthorizeActions.Read, user, org);
            return await base.CheckForDepenenciesAsync(part);
        }

        public Task<InvokeResult> DeleteCommponentAsync(string id, EntityHeader org, EntityHeader user)
        {
            throw new NotImplementedException();
        }

        public async Task<InvokeResult> DeleteComponentAsync(string id, EntityHeader org, EntityHeader user)
        {
            var part = await _componentRepo.GetComponentAsync(id);
            await ConfirmNoDepenenciesAsync(part);
            await AuthorizeAsync(part, AuthorizeActions.Delete, user, org);
            await _componentRepo.DeleteComponentAsync(id);
            return InvokeResult.Success;
        }

        public async Task<Component> GetComponentAsync(string id, EntityHeader org, EntityHeader user)
        {
            var part = await _componentRepo.GetComponentAsync(id);
            await AuthorizeAsync(part, AuthorizeActions.Read, user, org);
            return part;
        }


        public async Task<ListResponse<ComponentSummary>> GetComponentsSummariesAsync(ListRequest listRequest, string componentType, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org.Id, typeof(Component));
            if(!String.IsNullOrEmpty(componentType))
                return await _componentRepo.GetComponentSummariesByTypeAsync(org.Id, componentType, listRequest);
            else
                return await _componentRepo.GetComponentSummariesAsync(org.Id, listRequest);
        }

        public async Task<InvokeResult> UpdateComponentAsync(Component part, EntityHeader org, EntityHeader user)
        {
            await AuthorizeAsync(part, AuthorizeActions.Update, user, org);
            ValidationCheck(part, Actions.Update);
            await _componentRepo.UpdateComponentAsync(part);

            return InvokeResult.Success;
        }

        public async Task<InvokeResult<Stream>> GenerateLabelAsync(string compoentId, int row, int col, EntityHeader org, EntityHeader user)
        {
            if (col > 3)
                throw new Exception($"max column is 3, column provided is {col}");

            if (row > 10)
                throw new Exception($"max row is 3, row provided is {row}");

            var ms = new MemoryStream();
            var pdf = new LagoVista.PDFServices.PDFGenerator();
            pdf.RowHeight = 52;
            pdf.RowBottomMargin = 20;
            pdf.ColumnRightMargin = 23;
            pdf.Margin = new PDFServices.Margin() { Left = 20, Right = 20, Top = 50, Bottom = 50 };
            pdf.StartDocument(false, false);
            pdf.StartTable(PDFServices.ColWidth.CreateStar(), PDFServices.ColWidth.CreateStar(), PDFServices.ColWidth.CreateStar());

            pdf.StartRow(row - 1);
            var idx = col - 1;


            var component = await _componentRepo.GetComponentAsync(compoentId);
            await AuthorizeAsync(component, AuthorizeActions.Read, user, org, "Generate Part Labels");
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
            using (var qrCodeData = qrGenerator.CreateQrCode($"https://www.nuviot.com/mfg/component/{compoentId}.", QRCodeGenerator.ECCLevel.Q))
            using (var qrCode = new PngByteQRCode(qrCodeData))
            using (var qrMS = new MemoryStream(qrCode.GetGraphic(20)))
                pdf.AddColImage(idx, qrMS, 64, 64, align: XStringFormats.CenterRight);

            pdf.EndRow();
            pdf.EndTable();

            pdf.Write(ms, false);
            return InvokeResult<Stream>.Create(ms);
        }



        public async Task<InvokeResult> AddComponentPurchaseAsync(string componentId, ComponentPurchase purchase, EntityHeader org, EntityHeader user)
        {
            ValidationCheck(purchase, Actions.Update);

            var part = await GetComponentAsync(componentId, org, user);
            await AuthorizeAsync(part, AuthorizeActions.Update, user, org, "add purchase");

            part.Purchases.Add(purchase);

            part.QuantityOnOrder += purchase.QuantityOrdered;
            part.QuantityOnHand += purchase.QuantityReceived;

            await _componentRepo.UpdateComponentAsync(part);

            return InvokeResult.Success;
        }


        public async Task<InvokeResult> ReceiveComponentPurchaseAsync(string componentId, string orderId, decimal qty, EntityHeader org, EntityHeader user)
        {
            var part = await GetComponentAsync(componentId, org, user);
            await AuthorizeAsync(part, AuthorizeActions.Update, user, org, "add purchase");

            var purchase = part.Purchases.Single(prch => prch.OrderId == orderId);
            purchase.QuantityReceived += qty;

            part.QuantityOnHand += qty;
            part.QuantityOnOrder -= qty;

            await _componentRepo.UpdateComponentAsync(part);


            return InvokeResult.Success;
        }
    }
}
