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

	public class CompanyController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;

		public CompanyController(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;

		}

		#region
		public IActionResult Index()
		{
			List<Company> objCompanyList = _unitOfWork.Company.GetAll().ToList();

			return View(objCompanyList);
		}
		#endregion

		#region Upsert
		public IActionResult Upsert(int? id)  //Güncelleme ya da Yeni kayıt yapmak
		{
			if (id == null || id == 0)
			{        //create
				return View(new Company());
			}
			else
			{       //update
				Company companyObj = _unitOfWork.Company.Get(u => u.Id == id);
				return View(companyObj);
			}

		}

		[HttpPost]
		public IActionResult Upsert(Company CompanyObj)
		{
			if (ModelState.IsValid)
			{
				if (CompanyObj.Id == 0)
				{
					_unitOfWork.Company.Add(CompanyObj);
				}
				else
				{
					_unitOfWork.Company.Update(CompanyObj);
				}

				_unitOfWork.Save();
				TempData["success"] = "Company created successfully";
				return RedirectToAction("Index");
			}
			else
			{
				return View(CompanyObj);
			}

		}
		#endregion

		#region API CALLS
		[HttpGet]
		public IActionResult GetAll()
		{
			List<Company> objCompanyList = _unitOfWork.Company.GetAll().ToList();
			return Json(new { data = objCompanyList });
		}

		[HttpDelete]
		public IActionResult Delete(int? id)
		{
			var CompanyToBeDeleted = _unitOfWork.Company.Get(u => u.Id == id);
			if (CompanyToBeDeleted == null)
			{
				return Json(new { success = false, message = "Error While Deleting!" });
			}

			_unitOfWork.Company.Remove(CompanyToBeDeleted);
			_unitOfWork.Save();

			return Json(new { success = true, message = "Delete Successful" });
		}
		#endregion
	}
}
