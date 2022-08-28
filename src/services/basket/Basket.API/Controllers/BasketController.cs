using System.Net;
using Basket.API.Entities;
using Basket.API.GrpcServices;
using Basket.API.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Basket.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class BasketController : ControllerBase
{
    private readonly IBasketRepository _repository;
    private readonly DiscountGrpcService _discountGrpc;

    public BasketController(IBasketRepository repository, DiscountGrpcService discountGrpc)
    {
        _repository = repository;
        _discountGrpc = discountGrpc;
    }

    [HttpGet("{username}", Name = "GetBasket")]
    [ProducesResponseType(typeof(ShoppingCart), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<ShoppingCart>> GetBasket(string username)
    {
        var basket = await _repository.GetBasket(username);
        return Ok(basket ?? new ShoppingCart(username));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ShoppingCart), (int)HttpStatusCode.OK)]
    public async Task<ActionResult<ShoppingCart>> UpdateBasket([FromBody] ShoppingCart basket)
    {
        foreach (var item in basket.Items)
        {
            var coupon = await _discountGrpc.GetDiscount(item.ProductName);
            item.Price -= coupon.Amount;
        }
        
        return Ok(await _repository.UpdateBasket(basket));
    }

    [HttpDelete("{username}", Name = "DeleteBasket")]
    [ProducesResponseType(typeof(void), (int)HttpStatusCode.OK)]
    public async Task<IActionResult> DeleteBasket(string username)
    {
        await _repository.DeleteBasket(username);
        return Ok();
    }
}