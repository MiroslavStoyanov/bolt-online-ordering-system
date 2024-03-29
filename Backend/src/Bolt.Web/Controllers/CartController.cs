﻿namespace Bolt.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Bolt.DTOs.Orders;
    using Bolt.DTOs.Products;
    using Bolt.Models;
    using Bolt.Services.Interfaces;
    using Bolt.Web.Services;
    using Bolt.Web.ViewModels.Cart;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;

    [Authorize]
    public class CartController : Controller
    {
        private readonly ICookieCachingService _cookieCachingService;
        private readonly IProductsService _productService;
        private readonly IOrdersService _ordersService;
        private readonly IUsersService _usersService;

        public CartController(
            ICookieCachingService cookieCachingService,
            IProductsService productService,
            IOrdersService ordersService,
            IUsersService usersService)
        {
            this._cookieCachingService = cookieCachingService;
            this._productService = productService;
            this._ordersService = ordersService;
            this._usersService = usersService;
        }

        public async Task<IActionResult> Index()
        {
            var products = new List<ProductViewModel>();

            string cachedProducts = this._cookieCachingService.Get("products");

            if (string.IsNullOrEmpty(cachedProducts))
            {
                return this.View(products);
            }

            var deserializedProducts =
                JsonConvert.DeserializeObject<List<ProductShoppingCartCache>>(cachedProducts);

            IEnumerable<int> productIds = deserializedProducts.Select(a => a.Id);
            List<ProductDTO> productEntities = await this._productService.GetProductsByIDsAsync(productIds);

            foreach (ProductDTO productDTO in productEntities)
            {
                products.Add(new ProductViewModel
                {
                    Id = productDTO.Id,
                    Name = productDTO.Name,
                    Price = productDTO.Price,
                    Description = productDTO.Description,
                    Quantity = deserializedProducts.FirstOrDefault(p => p.Id == productDTO.Id)?.Quantity
                });
            }

            return this.View(products);
        }

        [HttpDelete]
        public bool RemoveItem(int? productId)
        {
            string cachedProducts = this._cookieCachingService.Get("products");

            var deserializedProducts = JsonConvert.DeserializeObject<List<ProductShoppingCartCache>>(cachedProducts);

            deserializedProducts.RemoveAll(pr => pr.Id == productId);

            this._cookieCachingService.Set("products", JsonConvert.SerializeObject(deserializedProducts), 30);

            return true;
        }

        [HttpPost]
        public void EditItemQuantity(int productId, int quantity)
        {
            string cachedProducts = this._cookieCachingService.Get("products");

            var deserializedProducts = JsonConvert.DeserializeObject<List<ProductShoppingCartCache>>(cachedProducts);

            ProductShoppingCartCache product = deserializedProducts.FirstOrDefault(p => p.Id == productId);
            product.Quantity = quantity;

            this._cookieCachingService.Set("products", JsonConvert.SerializeObject(deserializedProducts), 30);
        }

        public async Task<IActionResult> Order()
        {
            string cachedProducts = this._cookieCachingService.Get("products");

            var deserializedProducts =
                JsonConvert.DeserializeObject<List<ProductShoppingCartCache>>(cachedProducts);

            string username = this.User.Identity.Name;
            string userId = await this._usersService.GetUserIdByUsernameAsync(username);

            var orderDTO = new CreateOrderDTO
            {
                CreatedOn = DateTime.Now,
                OrderStatus = OrderStatus.Accepted,
                UserId = userId,
                Products = deserializedProducts
            };

            int orderId = await this._ordersService.AddOrderAsync(orderDTO);

            this._cookieCachingService.Remove("products");

            return this.RedirectToAction("Index", "OrderTracker", new {orderId});
        }
    }
}