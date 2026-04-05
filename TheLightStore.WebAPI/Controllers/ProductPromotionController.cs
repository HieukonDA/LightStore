using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheLightStore.Application.Dtos;
using TheLightStore.Application.Interfaces.Services;

namespace TheLightStore.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductPromotionController : ControllerBase
{
    private readonly IProductPromotionService _productPromotionService;

    public ProductPromotionController(IProductPromotionService productPromotionService)
    {
        _productPromotionService = productPromotionService;
    }

    [HttpGet]
    public async Task<IActionResult> GetProductPromotions([FromQuery] ProductPromotionDto.ProductPromotionFilterParams filterParams)
    {
        var result = await _productPromotionService.GetAll(filterParams);
        return Ok(result);
    }

    [HttpGet("promotion/{promotionId}/products")]
    public async Task<IActionResult> GetProductsByPromotionId(long promotionId)
    {
        var result = await _productPromotionService.GetProductsByPromotionId(promotionId);
        return Ok(result);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateProductPromotion([FromBody] ProductPromotionDto.ProductPromotionCreateDto createDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _productPromotionService.Create(createDto);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("batch")]
    public async Task<IActionResult> CreateMultiple([FromBody] ProductPromotionDto.ListProductPromotionCreateDto createDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _productPromotionService.CreateMultiple(createDto);
        return Ok(result);
    }

    [Authorize]
    [HttpPut("batch")]
    public async Task<IActionResult> UpdateMultiple([FromBody] ProductPromotionDto.ListProductPromotionUpdateDto updateDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _productPromotionService.UpdateMultiple(updateDto);
        return Ok(result);
    }

    [Authorize]
    [HttpDelete("batch")]
    public async Task<IActionResult> RemoveMultiple([FromBody] ProductPromotionDto.ListProductPromotionRemoveDto removeDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _productPromotionService.RemoveMultiple(removeDto);
        return Ok(result);
    }
}
