using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheLightStore.Application.Dtos;
using TheLightStore.Application.Interfaces.Services;

namespace TheLightStore.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts([FromQuery] ProductDto.ProductFilterParams filterParams)
    {
        var result = await _productService.GetAll(filterParams);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProductById(long id)
    {
        var result = await _productService.GetById(id);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("admin")]
    public async Task<IActionResult> CreateProductByAdmin([FromBody] ProductDto.ProductCreateDto createDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _productService.CreateByAdmin(createDto);
        return Ok(result);
    }

    [Authorize]
    [HttpPut("admin/{id}")]
    public async Task<IActionResult> UpdateProductByAdmin(long id, [FromBody] ProductDto.UpdateExtraDto updateDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        updateDto.Id = id;
        var result = await _productService.UpdateByAdmin(updateDto);
        return Ok(result);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProductByUser(long id, [FromBody] ProductDto.ProductUpdateDto updateDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        updateDto.Id = id;
        var result = await _productService.UpdateByUserLogin(updateDto);
        return Ok(result);
    }

    [Authorize]
    [HttpDelete("admin/{id}")]
    public async Task<IActionResult> DeleteProductByAdmin(long id)
    {
        var result = await _productService.RemoveByAdmin(id);
        return Ok(result);
    }
}
