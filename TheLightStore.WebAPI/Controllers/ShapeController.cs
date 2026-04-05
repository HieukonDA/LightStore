using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheLightStore.Application.Interfaces.Services;
using TheLightStore.Domain.Commons.Models;
using TheLightStore.Domain.Constants;
using static TheLightStore.Application.Dtos.ShapeDto;

namespace TheLightStore.WebAPI.Controllers
{
    [Route(Strings.ActionRoute)]
    [ApiController]
    public class ShapeController : ControllerBase
    {
        private readonly IShapeService _shapeService;

        public ShapeController(IShapeService shapeService)
        {
            _shapeService = shapeService;
        }

        [HttpGet]
        public async Task<ActionResult<ResponseResult>> GetAll([FromQuery] ShapeFilterParams parameters)
        {
            var result = await _shapeService.GetAll(parameters);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseResult>> GetById(long id)
        {
            var result = await _shapeService.GetById(id);
            return Ok(result);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<ResponseResult>> Create([FromBody] ShapeCreateDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _shapeService.Create(model);
            return Ok(result);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<ActionResult<ResponseResult>> Update(long id, [FromBody] ShapeUpdateDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (model.Id != id)
                return BadRequest("ID mismatch");

            var result = await _shapeService.Update(model);
            return Ok(result);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult<ResponseResult>> Delete(long id)
        {
            var result = await _shapeService.Remove(id);
            return Ok(result);
        }
    }
}
