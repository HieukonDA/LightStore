using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheLightStore.Application.Interfaces.Services;
using TheLightStore.Domain.Commons.Models;
using TheLightStore.Domain.Constants;
using static TheLightStore.Application.Dtos.ColorTemperatureDto;

namespace TheLightStore.WebAPI.Controllers
{
    [Route(Strings.ActionRoute)]
    [ApiController]
    public class ColorTemperatureController : ControllerBase
    {
        private readonly IColorTemperatureService _colorTemperatureService;

        public ColorTemperatureController(IColorTemperatureService colorTemperatureService)
        {
            _colorTemperatureService = colorTemperatureService;
        }

        [HttpGet]
        public async Task<ActionResult<ResponseResult>> GetAll([FromQuery] ColorTemperatureFilterParams parameters)
        {
            var result = await _colorTemperatureService.GetAll(parameters);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseResult>> GetById(long id)
        {
            var result = await _colorTemperatureService.GetById(id);
            return Ok(result);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<ResponseResult>> Create([FromBody] ColorTemperatureCreateDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _colorTemperatureService.Create(model);
            return Ok(result);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<ActionResult<ResponseResult>> Update(long id, [FromBody] ColorTemperatureUpdateDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (model.Id != id)
                return BadRequest("ID mismatch");

            var result = await _colorTemperatureService.Update(model);
            return Ok(result);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult<ResponseResult>> Delete(long id)
        {
            var result = await _colorTemperatureService.Remove(id);
            return Ok(result);
        }
    }
}
