using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020158.BusinessLayers;
using SV22T1020158.Models.Sales;
using SV22T1020158.Models.Shop;
using System.Security.Claims;
using System.Text.Json;

namespace SV22T1020158.Shop.Controllers
{
    public class CartController : Controller
    {
        private const string CART_SESSION_KEY = "ShoppingCart";

        private List<CartItem> GetCartFromSession()
        {
            var cartJson = HttpContext.Session.GetString(CART_SESSION_KEY);
            if (string.IsNullOrEmpty(cartJson))
            {
                return new List<CartItem>();
            }
            return JsonSerializer.Deserialize<List<CartItem>>(cartJson) ?? new List<CartItem>();
        }

        private void SaveCartToSession(List<CartItem> cart)
        {
            var cartJson = JsonSerializer.Serialize(cart);
            HttpContext.Session.SetString(CART_SESSION_KEY, cartJson);
        }

        [HttpGet]
        public IActionResult Index()
        {
            var cart = GetCartFromSession();
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            if (quantity <= 0) quantity = 1;

            var product = await CatalogDataService.GetProductAsync(productId);
            if (product == null)
            {
                return Json(new { success = false, message = "Sản phẩm không tồn tại" });
            }

            var cart = GetCartFromSession();
            var existingItem = cart.FirstOrDefault(x => x.ProductID == productId);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductID = product.ProductID,
                    ProductName = product.ProductName,
                    Photo = string.IsNullOrEmpty(product.Photo) ? "nophoto.png" : product.Photo,
                    Unit = product.Unit,
                    Quantity = quantity,
                    SalePrice = product.Price
                });
            }

            SaveCartToSession(cart);
            int totalItems = cart.Sum(x => x.Quantity);
            return Json(new { success = true, cartCount = totalItems });
        }

        [HttpPost]
        public IActionResult UpdateCart(int productId, int quantity)
        {
            if (quantity < 0) quantity = 0;

            var cart = GetCartFromSession();
            var item = cart.FirstOrDefault(x => x.ProductID == productId);

            if (item != null)
            {
                if (quantity == 0)
                {
                    cart.Remove(item);
                }
                else
                {
                    item.Quantity = quantity;
                }
                SaveCartToSession(cart);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult RemoveFromCart(int productId)
        {
            var cart = GetCartFromSession();
            var item = cart.FirstOrDefault(x => x.ProductID == productId);
            if (item != null)
            {
                cart.Remove(item);
                SaveCartToSession(cart);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult ClearCart()
        {
            HttpContext.Session.Remove(CART_SESSION_KEY);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult GetCartCount()
        {
            var cart = GetCartFromSession();
            int count = cart.Sum(x => x.Quantity);
            return Json(new { count });
        }

        [Authorize]
        [HttpGet]
        public IActionResult Checkout()
        {
            var cart = GetCartFromSession();
            if (cart.Count == 0)
            {
                return RedirectToAction("Index");
            }
            return View(cart);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Checkout(string deliveryProvince, string deliveryAddress)
        {
            var cart = GetCartFromSession();

            if (cart.Count == 0)
            {
                TempData["Error"] = "Giỏ hàng trống";
                return RedirectToAction("Index");
            }

            if (string.IsNullOrWhiteSpace(deliveryProvince))
            {
                TempData["Error"] = "Vui lòng chọn tỉnh/thành phố giao hàng";
                return RedirectToAction("Checkout");
            }

            if (string.IsNullOrWhiteSpace(deliveryAddress))
            {
                TempData["Error"] = "Vui lòng nhập địa chỉ giao hàng";
                return RedirectToAction("Checkout");
            }

            // Lấy CustomerID từ Claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập lại";
                return RedirectToAction("Login", "ShopAccount");
            }

            int customerId = int.Parse(userIdClaim.Value);

            var orderDetails = cart.Select(item => new OrderDetailViewInfo
            {
                ProductID = item.ProductID,
                ProductName = item.ProductName,
                Quantity = item.Quantity,
                SalePrice = item.SalePrice,
                Unit = item.Unit,
                Photo = item.Photo
            }).ToList();

            int orderId = await SalesDataService.AddOrderAsync(customerId, deliveryProvince, deliveryAddress, orderDetails);

            if (orderId > 0)
            {
                HttpContext.Session.Remove(CART_SESSION_KEY);
                return RedirectToAction("Success", new { orderId });
            }
            else
            {
                TempData["Error"] = "Đặt hàng thất bại, vui lòng thử lại sau";
                return RedirectToAction("Checkout");
            }
        }

        [Authorize]
        [HttpGet]
        public IActionResult Success(int orderId)
        {
            ViewBag.OrderId = orderId;
            return View();
        }
    }
}