using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebLibrary.Data;
using WebLibrary.Models.Catalog;

namespace WebLibrary.Controllers
{
    public class CatalogController : Controller
    {
        private ILibraryAsset _assets;
        public CatalogController(ILibraryAsset assets)
        {
            _assets = assets;
        }

        public IActionResult Index()
        {
            var assetModels = _assets.GetAll();

            var listingResult = assetModels
                .Select(res => new AssetIndexListingModel
                    {
                        Id = res.Id,
                        ImageUrl = res.ImageUrl,
                        AuthorOrDirector = _assets.GetAuthorOrDirector(res.Id),
                        DeweyCallNumber = _assets.GetDeweyIndex(res.Id),
                        Title = res.Title,
                        Type = _assets.GetType(res.Id)
                    });

            var model = new AssetIndexModel() { Assets = listingResult };
            return View(model);
        }

        public IActionResult Detail(int id)
        {
            var asset = _assets.GetById(id);

            var model = new AssetDetailModel
            {
                AssetId = id,
                Title = asset.Title,
                Year = asset.Year,
                Cost = asset.Cost,
                Status = asset.Status.Name,
                ImageUrl = asset.ImageUrl,
                AuthorOrDirector = _assets.GetAuthorOrDirector(id),
                CurrentLocation = _assets.GetCurrentLocation(id)?.Name,
                DeweyCallNumber = _assets.GetDeweyIndex(id),
                ISBN = _assets.GetIsbn(id)
            };

            return View(model);
        }
    }
}