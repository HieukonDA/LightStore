using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheLightStore.Application.Interfaces.Services;
using TheLightStore.Domain.Commons.Models;
using TheLightStore.Domain.Constants;
using static TheLightStore.Application.Dtos.PowerDto;

namespace TheLightStore.WebAPI.Controllers
{
    [Route(Strings.ActionRoute)]
    [ApiController]
    public class PowerController : ControllerBase
    {
        private readonly IPowerService _powerService;

        public PowerController(IPowerService powerService)
        {
            _powerService = powerService;
        }

        [HttpGet]
        public async Task<ActionResult<ResponseResult>> GetAll([FromQuery] PowerFilterParams parameters)
        {
            var result = await _powerService.GetAll(parameters);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseResult>> GetById(long id)
        {
            var result = await _powerService.GetById(id);
            return Ok(result);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<ResponseResult>> Create([FromBody] PowerCreateDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _powerService.Create(model);
            return Ok(result);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<ActionResult<ResponseResult>> Update(long id, [FromBody] PowerUpdateDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (model.Id != id)
                return BadRequest("ID mismatch");

            var result = await _powerService.Update(model);
            return Ok(result);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult<ResponseResult>> Delete(long id)
        {
            var result = await _powerService.Remove(id);
            return Ok(result);
        }
    }
}
