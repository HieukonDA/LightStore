using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheLightStore.Application.Dtos;
using TheLightStore.Application.Interfaces.Services;

namespace TheLightStore.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductDetailController : ControllerBase
{
    private readonly IProductDetailService _productDetailService;

    public ProductDetailController(IProductDetailService productDetailService)
    {
        _productDetailService = productDetailService;
    }

    [HttpGet]
    public async Task<IActionResult> GetProductDetails([FromQuery] ProductDetailDto.ProductDetailFilterParams filterParams)
    {
        var result = await _productDetailService.GetAll(filterParams);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProductDetailById(long id)
    {
        var result = await _productDetailService.GetById(id);
        return Ok(result);
    }

    [HttpGet("product/{productId}/remaining-quantity")]
    public async Task<IActionResult> GetRemainingProductQuantity(long productId)
    {
        var result = await _productDetailService.RemainingProductQuantity(productId);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("batch")]
    public async Task<IActionResult> CreateMultiple([FromBody] ProductDetailDto.ListProductDetailCreateDto createDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _productDetailService.CreateMultiple(createDto);
        return Ok(result);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProductDetail(long id, [FromBody] ProductDetailDto.ProductDetailUpdateDto updateDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        updateDto.Id = id;
        var result = await _productDetailService.Update(updateDto);
        return Ok(result);
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProductDetail(long id)
    {
        var result = await _productDetailService.Remove(id);
        return Ok(result);
    }

    [Authorize]
    [HttpDelete("batch")]
    public async Task<IActionResult> RemoveMultiple([FromBody] ProductDetailDto.ListProductDetailRemoveDto removeDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _productDetailService.RemoveMultiple(removeDto);
        return Ok(result);
    }
}
