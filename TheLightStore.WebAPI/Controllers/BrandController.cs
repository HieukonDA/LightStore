using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheLightStore.Application.Dtos;
using TheLightStore.Application.Interfaces.Services;

namespace TheLightStore.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BrandController : ControllerBase
{
    private readonly IBrandService _brandService;

    public BrandController(IBrandService brandService)
    {
        _brandService = brandService;
    }

    [HttpGet]
    public async Task<IActionResult> GetBrands([FromQuery] BrandDto.BrandFilterParams filterParams)
    {
        var result = await _brandService.GetAll(filterParams);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetBrandById(long id)
    {
        var result = await _brandService.GetById(id);
        return Ok(result);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateBrand([FromBody] BrandDto.BrandCreateDto createDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _brandService.Create(createDto);
        return Ok(result);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBrand(long id, [FromBody] BrandDto.BrandUpdateDto updateDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (updateDto.Id != id)
            return BadRequest("ID trong URL không khớp với ID trong body");

        var result = await _brandService.Update(updateDto);
        return Ok(result);
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBrand(long id)
    {
        var result = await _brandService.Remove(id);
        return Ok(result);
    }
}
