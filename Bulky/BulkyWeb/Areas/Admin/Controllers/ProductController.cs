using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using BulkyWeb.DataAccess.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BulkyWeb.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = SD.Role_Admin)]

	public class ProductController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IWebHostEnvironment _webHostEnvironment;
		public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
		{
			_unitOfWork = unitOfWork;
			_webHostEnvironment = webHostEnvironment;
		}

		#region Index
		public IActionResult Index()
		{
			List<Product> objProductList = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();

			return View(objProductList);
		}
		#endregion

		#region Upsert
		public IActionResult Upsert(int? id)
		{

			//ViewData["CategoryList] şeklinde de yapabilirdik(html kodları da değişik)
			//ViewBag.CategoryList = CategoryList;
			ProductVM productVM = new()
			{

				CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
				{
					Text = u.Name,
					Value = u.Id.ToString()
				}),
				Product = new Product()
			};
			if (id == null || id == 0)
			{
				//create
				return View(productVM);
			}
			else
			{
				//update
				productVM.Product = _unitOfWork.Product.Get(u => u.Id == id,includeProperties:"ProductImages");
				return View(productVM);
			}

		}

		[HttpPost]
		public IActionResult Upsert(ProductVM productVM, List<IFormFile> files)
		{


			if (ModelState.IsValid)
			{
				if (productVM.Product.Id == 0)
				{
					_unitOfWork.Product.Add(productVM.Product);

				}
				else
				{
					_unitOfWork.Product.Update(productVM.Product);

				}

				_unitOfWork.Save();
				TempData["success"] = "Product created successfully";
				string wwwRootPath = _webHostEnvironment.WebRootPath;
				if (files != null)
				{
					foreach (IFormFile file in files)
					{
						string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
						string productPath = @"images\products\product-" + productVM.Product.Id;

						string FinalPath = Path.Combine(wwwRootPath, productPath);

						if (!Directory.Exists(FinalPath))
							Directory.CreateDirectory(FinalPath);

						using (var fileStream = new FileStream(Path.Combine(FinalPath, fileName), FileMode.Create))
						{
							file.CopyTo(fileStream);
						}
						ProductImage productImage = new()
						{
							ImageUrl = @"\" + productPath + @"\" + fileName,
							ProductId = productVM.Product.Id,
						};

						if (productVM.Product.ProductImages == null)
						{
							productVM.Product.ProductImages = new List<ProductImage>();
						}

						productVM.Product.ProductImages.Add(productImage);

					}
					_unitOfWork.Product.Update(productVM.Product);
					_unitOfWork.Save();


				}
				TempData["success"] = "Product created/updated successfully";
				return RedirectToAction("Index");
			}
			else
			{


				productVM.CategoryList = _unitOfWork.Category.GetAll().Select(u => new SelectListItem
				{
					Text = u.Name,
					Value = u.Id.ToString()
				});

			};
			return View(productVM);

		}


		#endregion

		#region DeleteProduct

		[HttpPost, ActionName("Delete")]
		public IActionResult DeletePOST(int? id)
		{
			Product obj = _unitOfWork.Product.Get(u => u.Id == id);
			if (obj == null)
			{
				return NotFound();
			}
			_unitOfWork.Product.Remove(obj);
			_unitOfWork.Save();
			TempData["success"] = "Product deleted successfully";
			return RedirectToAction("Index");
		}
		#endregion

		#region DeleteImage
		public IActionResult DeleteImage(int imageId)
		{
			var imageToBeDeleted = _unitOfWork.ProductImage.Get(u => u.Id == imageId);
			int productId=imageToBeDeleted.ProductId;
            if (imageToBeDeleted!= null)
            {
                if (!string.IsNullOrEmpty(imageToBeDeleted.ImageUrl))
				{
					var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, imageToBeDeleted.ImageUrl.TrimStart('\\'));
					if (System.IO.File.Exists(oldImagePath))
					{
						System.IO.File.Delete(oldImagePath);
					}

				}
				_unitOfWork.ProductImage.Remove(imageToBeDeleted);
				_unitOfWork.Save();

				TempData["success"] = "Deleted successfully";
			}
			return RedirectToAction(nameof(Upsert),new {id=productId});
        }

		#endregion

		#region API CALLS
		[HttpGet]
		public IActionResult GetAll()
		{
			List<Product> objProductList = _unitOfWork.Product.GetAll(includeProperties: "Category").ToList();
			return Json(new { data = objProductList });
		}

		[HttpDelete]
		public IActionResult Delete(int? id)
		{
			var productToBeDeleted = _unitOfWork.Product.Get(u => u.Id == id);
			if (productToBeDeleted == null)
			{
				return Json(new { success = false, message = "Error While Deleting!" });
			}
			
			string productPath = @"images\products\product-" + id;

			string FinalPath = Path.Combine(_webHostEnvironment.WebRootPath, productPath);

			if (Directory.Exists(FinalPath))
			{
				string[] filePaths=Directory.GetFiles(FinalPath);
                foreach (string filePath in filePaths)
                {
                    System.IO.File.Delete(filePath);
                }
                Directory.Delete(FinalPath);
			}

			_unitOfWork.Product.Remove(productToBeDeleted);
			_unitOfWork.Save();

			return Json(new { success = true, message = "Delete Successful" });
		}
		#endregion
	}
}
