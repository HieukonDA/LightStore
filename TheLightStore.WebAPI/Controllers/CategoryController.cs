using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheLightStore.Application.Dtos;
using TheLightStore.Application.Interfaces.Services;

namespace TheLightStore.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCategories([FromQuery] CategoryDto.CategoryFilterParams filterParams)
    {
        var result = await _categoryService.GetAll(filterParams);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategoryById(long id)
    {
        var result = await _categoryService.GetById(id);
        return Ok(result);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateCategory([FromBody] CategoryDto.CategoryCreateDto createDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _categoryService.Create(createDto);
        return Ok(result);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategory(long id, [FromBody] CategoryDto.CategoryUpdateDto updateDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (updateDto.Id != id)
            return BadRequest("ID trong URL không khớp với ID trong body");

        var result = await _categoryService.Update(updateDto);
        return Ok(result);
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(long id)
    {
        var result = await _categoryService.Remove(id);
        return Ok(result);
    }
}
