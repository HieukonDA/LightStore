using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheLightStore.Application.Interfaces.Services;
using TheLightStore.Domain.Commons.Models;
using TheLightStore.Domain.Constants;
using static TheLightStore.Application.Dtos.BaseTypeDto;

namespace TheLightStore.WebAPI.Controllers
{
    [Route(Strings.ActionRoute)]
    [ApiController]
    public class BaseTypeController : ControllerBase
    {
        private readonly IBaseTypeService _baseTypeService;

        public BaseTypeController(IBaseTypeService baseTypeService)
        {
            _baseTypeService = baseTypeService;
        }

        [HttpGet]
        public async Task<ActionResult<ResponseResult>> GetAll([FromQuery] BaseTypeFilterParams parameters)
        {
            var result = await _baseTypeService.GetAll(parameters);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseResult>> GetById(long id)
        {
            var result = await _baseTypeService.GetById(id);
            return Ok(result);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<ResponseResult>> Create([FromBody] BaseTypeCreateDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _baseTypeService.Create(model);
            return Ok(result);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<ActionResult<ResponseResult>> Update(long id, [FromBody] BaseTypeUpdateDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (model.Id != id)
                return BadRequest("ID mismatch");

            var result = await _baseTypeService.Update(model);
            return Ok(result);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult<ResponseResult>> Delete(long id)
        {
            var result = await _baseTypeService.Remove(id);
            return Ok(result);
        }
    }
}
