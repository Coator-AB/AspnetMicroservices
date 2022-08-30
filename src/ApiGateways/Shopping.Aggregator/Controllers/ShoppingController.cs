using System.Net;
using Microsoft.AspNetCore.Mvc;
using Shopping.Aggregator.Models;
using Shopping.Aggregator.Services;

namespace Shopping.Aggregator.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ShoppingController : ControllerBase
{
    private readonly ICatalogService _catalogService;
    private readonly IBasketService _basketService;
    private readonly IOrderService _orderService;

    public ShoppingController(ICatalogService catalogService, IBasketService basketService, IOrderService orderService)
    {
        _catalogService = catalogService;
        _basketService = basketService;
        _orderService = orderService;
    }

    [HttpGet("{username}", Name = "GetShopping")]
    [ProducesResponseType(typeof(ShoppingModel), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<ShoppingModel>> GetShopping(string username)
    {
        var basket = await _basketService.GetBasket(username);

        foreach (var item in basket.Items)
        {
            var product = await _catalogService.GetCatalog(item.ProductId);
            item.ProductName = product.Name;
            item.Category = product.Category;
            item.Summary = product.Summary;
            item.Description = product.Description;
            item.ImageFile = product.ImageFile;
        }

        var orders = await _orderService.GetOrdersByUsername(username);

        var shoppingModel = new ShoppingModel
        {
            Username = username,
            BasketWithProducts = basket,
            Orders = orders
        };

        return Ok(shoppingModel);
    }
}