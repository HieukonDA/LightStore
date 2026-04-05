using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheLightStore.Application.Dtos;
using TheLightStore.Application.Interfaces.Services;

namespace TheLightStore.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PromotionController : ControllerBase
{
    private readonly IPromotionService _promotionService;

    public PromotionController(IPromotionService promotionService)
    {
        _promotionService = promotionService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPromotions([FromQuery] PromotionDto.PromotionFilterParams filterParams)
    {
        var result = await _promotionService.GetAll(filterParams);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPromotionById(long id)
    {
        var result = await _promotionService.GetById(id);
        return Ok(result);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreatePromotion([FromBody] PromotionDto.PromotionCreateDto createDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _promotionService.Create(createDto);
        return Ok(result);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePromotion(long id, [FromBody] PromotionDto.PromotionUpdateDto updateDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (updateDto.Id != id)
            return BadRequest("ID trong URL không khớp với ID trong body");

        var result = await _promotionService.Update(updateDto);
        return Ok(result);
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePromotion(long id)
    {
        var result = await _promotionService.Remove(id);
        return Ok(result);
    }

    [HttpGet("{id}/products-group-select")]
    public async Task<IActionResult> GetProductsForGroupSelect(long id)
    {
        var result = await _promotionService.GetProductsForGroupSelect(id);
        return Ok(result);
    }

    [HttpGet("valid/list")]
    public async Task<IActionResult> GetValidPromotions()
    {
        var result = await _promotionService.GetValidPromotions();
        return Ok(result);
    }
}
