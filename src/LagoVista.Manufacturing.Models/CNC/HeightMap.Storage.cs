﻿using LagoVista.Core.IOC;
using LagoVista.Core.PlatformSupport;
using LagoVista.Manufacturing.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Models
{
    public partial class HeightMap
    {
        public static string FileFilterHeightMap = "Height Maps|*.hmap|All Files|*.*";


        public async Task SaveAsync()
        {
            if (!String.IsNullOrEmpty(_fileName))
            {
                var popupService = SLWIOC.Get<IPopupServices>();
                _fileName = await popupService.ShowSaveFileAsync(FileFilterHeightMap);
            }

            if (!String.IsNullOrEmpty(_fileName))
            {
                await Core.PlatformSupport.Services.Storage.StoreAsync(this, _fileName);
            }
        }

        public async Task SaveAsAsync()
        {
            var popupService = SLWIOC.Get<IPopupServices>();
            _fileName = await popupService.ShowSaveFileAsync(FileFilterHeightMap);
            if (!String.IsNullOrEmpty(_fileName))
            {
                await Core.PlatformSupport.Services.Storage.StoreAsync(this, _fileName);
            }
        }

        public static async Task<HeightMap> OpenAsync(LagoVista.Manufacturing.Models.Machine settings)
        {
            var popupService = SLWIOC.Get<IPopupServices>();
            var fileName = await popupService.ShowSaveFileAsync(FileFilterHeightMap);
            if (fileName != null)
            {
                var heightMap = await Core.PlatformSupport.Services.Storage.GetAsync<HeightMap>(fileName);
                heightMap.Refresh();
                return heightMap;
            }
            else
            {
                return null;
            }
        }
    }
}
