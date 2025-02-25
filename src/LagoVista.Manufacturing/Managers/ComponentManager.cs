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
        private readonly IComponentPackageRepo _packageRepo;

        public ComponentManager(IComponentRepo componentRepo, IComponentPackageRepo packageRepo, IAdminLogger logger, IAppConfig appConfig, IDependencyManager depmanager, ISecurity security) :
            base(logger, appConfig, depmanager, security)
        {
            _componentRepo = componentRepo;
            _packageRepo = packageRepo;
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
        
        public async Task<InvokeResult> DeleteComponentAsync(string id, EntityHeader org, EntityHeader user)
        {
            var part = await _componentRepo.GetComponentAsync(id);
            await ConfirmNoDepenenciesAsync(part);
            await AuthorizeAsync(part, AuthorizeActions.Delete, user, org);
            await _componentRepo.DeleteComponentAsync(id);
            return InvokeResult.Success;
        }

        public async Task<Component> GetComponentAsync(string id, bool loadPackage, EntityHeader org, EntityHeader user)
        {
            var component = await _componentRepo.GetComponentAsync(id);
            await AuthorizeAsync(component, AuthorizeActions.Read, user, org);
            if (!EntityHeader.IsNullOrEmpty(component.ComponentPackage) && loadPackage)
            {
                component.ComponentPackage.Value = await _packageRepo.GetComponentPackageAsync(component.ComponentPackage.Id);
            }
                         
            return component;
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

            if (component.Location != null)
            {
                var locationLine = String.Empty;
                if (!EntityHeader.IsNullOrEmpty(component.Location.Room))
                    locationLine += component.Location.Room.Text;

                if (!EntityHeader.IsNullOrEmpty(component.Location.ShelfUnit))
                    locationLine += $"/{component.Location.ShelfUnit.Text}";

                if (!EntityHeader.IsNullOrEmpty(component.Location.Shelf))
                    locationLine += $"/{component.Location.Shelf.Text}";

                if (!EntityHeader.IsNullOrEmpty(component.Location.Column))
                    locationLine += $"/{component.Location.Column.Text}";

                if (!EntityHeader.IsNullOrEmpty(component.Location.Bin))
                    locationLine += $"/{component.Location.Bin}";
                pdf.AddColText(PDFServices.Style.Small, idx, locationLine, align: XStringFormats.BottomLeft);
            }

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

        public async Task<InvokeResult<Stream>> GenerateLabelsAsync(string[] compoentIds, int row, int col, EntityHeader org, EntityHeader user)
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

            foreach (var compoentId in compoentIds)
            {
                var component = await _componentRepo.GetComponentAsync(compoentId);
                await AuthorizeAsync(component, AuthorizeActions.Read, user, org, "Generate Part Labels");
                pdf.AddColText(PDFServices.Style.Small, idx, component.Name, align: XStringFormats.TopLeft);
                pdf.AddColText(PDFServices.Style.Small, idx, $"{component.ComponentType.Text}/{component.ComponentPackage?.Text}", align: XStringFormats.CenterLeft);

                var packageLine = component.ComponentType.Text;
                if (!EntityHeader.IsNullOrEmpty(component.ComponentPackage))
                    packageLine += $"/{component.ComponentPackage.Text}";

                pdf.AddColText(PDFServices.Style.Small, idx, $"{component.ComponentType.Text}/{component.ComponentPackage?.Text}", align: XStringFormats.CenterLeft);

                if (component.Location != null)
                {
                    var locationLine = String.Empty;
                    if (!EntityHeader.IsNullOrEmpty(component.Location.Room))
                        locationLine += component.Location.Room.Text;

                    if (!EntityHeader.IsNullOrEmpty(component.Location.ShelfUnit))
                        locationLine += $"/{component.Location.ShelfUnit.Text}";

                    if (!EntityHeader.IsNullOrEmpty(component.Location.Shelf))
                        locationLine += $"/{component.Location.Shelf.Text}";

                    if (!EntityHeader.IsNullOrEmpty(component.Location.Column))
                        locationLine += $"/{component.Location.Column.Text}";

                    if (!EntityHeader.IsNullOrEmpty(component.Location.Bin))
                        locationLine += $"/{component.Location.Bin}";
                    pdf.AddColText(PDFServices.Style.Small, idx, locationLine, align: XStringFormats.BottomLeft);
                }


                using (var qrGenerator = new QRCodeGenerator())
                using (var qrCodeData = qrGenerator.CreateQrCode($"https://www.nuviot.com/mfg/component/{compoentId}.", QRCodeGenerator.ECCLevel.Q))
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


            pdf.EndRow();
            pdf.EndTable();

            pdf.Write(ms, false);
            return InvokeResult<Stream>.Create(ms);
        }



        public async Task<InvokeResult> AddComponentPurchaseAsync(string componentId, ComponentPurchase purchase, EntityHeader org, EntityHeader user)
        {
            ValidationCheck(purchase, Actions.Update);

            var part = await GetComponentAsync(componentId,false, org, user);
            await AuthorizeAsync(part, AuthorizeActions.Update, user, org, "add purchase");

            part.Purchases.Add(purchase);

            part.QuantityOnOrder += purchase.QuantityOrdered;
            part.QuantityOnHand += purchase.QuantityReceived;

            await _componentRepo.UpdateComponentAsync(part);

            return InvokeResult.Success;
        }


        public async Task<InvokeResult> ReceiveComponentPurchaseAsync(string componentId, string orderId, decimal qty, EntityHeader org, EntityHeader user)
        {
            var part = await GetComponentAsync(componentId, false, org, user);
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
