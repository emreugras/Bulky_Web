using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BulkyWeb.Areas.Customer.Controllers
{
    [Area("customer")]
    [Authorize]
    public class CartController : Controller
	{
		
        private readonly IUnitOfWork _unitOfWork;
		[BindProperty]
		public ShoppingCartVm ShoppingCartVm { get; set; }
		public CartController (IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		#region Index
		public IActionResult Index()
		{

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

			ShoppingCartVm = new()
			{
				ShoppingCartList=_unitOfWork.ShoppingCart.GetAll(u=>u.ApplicationUserId == userId
				,includeProperties:"Product"),
                OrderHeader=new()
			};
			IEnumerable<ProductImage> productImages = _unitOfWork.ProductImage.GetAll();

            foreach (var cart in ShoppingCartVm.ShoppingCartList)
            {
				cart.Product.ProductImages=productImages.Where(u=>u.ProductId==cart.ProductId).ToList();
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVm.OrderHeader.OrderTotal += (cart.Price*cart.Count);
            }
            return View(ShoppingCartVm);
		}
		#endregion

		#region PriceForCount
		private double GetPriceBasedOnQuantity(ShoppingCart shoppingCart)
		{
            if (shoppingCart.Count<=50)
            {
				return shoppingCart.Product.Price;
            }
            else
            {
                if (shoppingCart.Count<=100)
                {
                    return shoppingCart.Product.Price50;
                }
                else
                {
                    return shoppingCart.Product.Price100;
                }
            }
        }
		#endregion

		#region SummaryOperations
		public IActionResult Summary()
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

			ShoppingCartVm = new()
			{
				ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId
				, includeProperties: "Product"),
				OrderHeader = new()
			};

			ShoppingCartVm.OrderHeader.ApplicationUser=_unitOfWork.ApplicationUser.Get(u=>u.Id==userId);

			ShoppingCartVm.OrderHeader.Name=ShoppingCartVm.OrderHeader.ApplicationUser.Name;
			ShoppingCartVm.OrderHeader.PhoneNumber = ShoppingCartVm.OrderHeader.ApplicationUser.PhoneNumber;
			ShoppingCartVm.OrderHeader.StreetAddress = ShoppingCartVm.OrderHeader.ApplicationUser.StreetAddress;
			ShoppingCartVm.OrderHeader.City = ShoppingCartVm.OrderHeader.ApplicationUser.City;
			ShoppingCartVm.OrderHeader.State = ShoppingCartVm.OrderHeader.ApplicationUser.State;
			ShoppingCartVm.OrderHeader.PostalCode = ShoppingCartVm.OrderHeader.ApplicationUser.PostalCode;

			foreach (var cart in ShoppingCartVm.ShoppingCartList)
			{
				cart.Price = GetPriceBasedOnQuantity(cart);
				ShoppingCartVm.OrderHeader.OrderTotal += (cart.Price * cart.Count);
			}
			return View(ShoppingCartVm);
        }
		[HttpPost]
		[ActionName("Summary")]
		public IActionResult SummaryPOST()
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

			ShoppingCartVm.ShoppingCartList = _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == userId
			, includeProperties: "Product");

			ShoppingCartVm.OrderHeader.OrderDate=System.DateTime.Now;
			ShoppingCartVm.OrderHeader.ApplicationUserId=userId;

			ApplicationUser applicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId);
						

			foreach (var cart in ShoppingCartVm.ShoppingCartList)
			{
				cart.Price = GetPriceBasedOnQuantity(cart);
				ShoppingCartVm.OrderHeader.OrderTotal += (cart.Price * cart.Count);
			}

            if (applicationUser.CompanyId.GetValueOrDefault()==0)
            {
                ShoppingCartVm.OrderHeader.PaymentStatus=SD.PaymentStatusPending;
				ShoppingCartVm.OrderHeader.OrderStatus = SD.StatusPending;
            }
            else
            {
				ShoppingCartVm.OrderHeader.PaymentStatus =SD.PaymentStatusDelayedPayment;
				ShoppingCartVm.OrderHeader.OrderStatus = SD.StatusApproved;
            }

			_unitOfWork.OrderHeader.Add(ShoppingCartVm.OrderHeader);
			_unitOfWork.Save();
            foreach (var cart in ShoppingCartVm.ShoppingCartList)
            {
				OrderDetail orderDetail = new()
				{
					ProductId = cart.ProductId,
					OrderHeaderId = ShoppingCartVm.OrderHeader.Id,
					Price = cart.Price,
					Count = cart.Count,
				};
				_unitOfWork.OrderDetail.Add(orderDetail);
				_unitOfWork.Save();
            }
			if (applicationUser.CompanyId.GetValueOrDefault() == 0)
			{ 
			
			}
			return RedirectToAction(nameof(OrderConfirmation), new {id=ShoppingCartVm.OrderHeader.Id});
		}
		#endregion

		#region OrderConfirmation
		public IActionResult OrderConfirmation(int id) {
			return View(id);
		}
		#endregion

		#region OrderCountOperationsForSummary
		public IActionResult Plus(int cartId)
        {
            var cartFromDb=_unitOfWork.ShoppingCart.Get(u=>u.Id==cartId);
            cartFromDb.Count += 1;
            _unitOfWork.ShoppingCart.Update(cartFromDb);
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

		public IActionResult Minus(int cartId)
		{
			var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId);
			

            if (cartFromDb.Count<=1)
            {
				_unitOfWork.ShoppingCart.Remove(cartFromDb);
                HttpContext.Session.SetInt32(SD.SessionCart, _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == cartFromDb.ApplicationUserId).Count() - 1);
		
            }
            else
            {
				cartFromDb.Count -= 1;
                _unitOfWork.ShoppingCart.Update(cartFromDb);
				
            }
           
			_unitOfWork.Save();
			return RedirectToAction(nameof(Index));
		}

		public IActionResult Remove(int cartId)
		{
			var cartFromDb = _unitOfWork.ShoppingCart.Get(u => u.Id == cartId);
			
			_unitOfWork.ShoppingCart.Remove(cartFromDb);
            HttpContext.Session.SetInt32(SD.SessionCart, _unitOfWork.ShoppingCart.GetAll(u => u.ApplicationUserId == cartFromDb.ApplicationUserId).Count() - 1);

            _unitOfWork.Save();
			return RedirectToAction(nameof(Index));
		}
		#endregion
	}
}
