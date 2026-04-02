using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using TheLightStore.Application.Interfaces;
using TheLightStore.Domain.Commons.Models;
using TheLightStore.Domain.Constants;
using TheLightStore.WebAPI.Filter;
using static TheLightStore.Application.Dtos.AuthDto;

namespace TheLightStore.WebAPI.Controllers.SysControllers
{
    [Route(Strings.ActionRoute)]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService ;

        public AuthController( IAuthService authService)
        {
            _authService = authService;
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<ResponseResult>> Me()
        {
            return await _authService.Me();
        }

        [HttpPost]
        [IdentifyDevice(allowBrowser: true, allowMobile: true)]
        public async Task<ActionResult<SuccessResponseResult>> Login([FromBody] LoginDto model)
        {
            return await _authService.Login(model);
        }

        [HttpPost]
        public async Task<ActionResult<SuccessResponseResult>> RefreshToken([FromBody] RefreshRequest model)
        {
            return await _authService.RefreshToken(model.RefreshToken);
        }

        [HttpPost]
        public async Task<ActionResult<ResponseResult>> Register([FromBody] RegisterDto model)
        {
            return await _authService.Register(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ResponseResult>> ChangePassword([FromBody] ChangePasswordDto model)
        {
            if (!ModelState.IsValid)
                return new BadRequestObjectResult(ModelState);
            return await _authService.ChangePassword(model);
        }

        [HttpPost]
        public async Task<ActionResult<ResponseResult>> ForgotPassword([FromBody] ForgotPasswordDto model)
        {
            if (!ModelState.IsValid)
                return new BadRequestObjectResult(ModelState);
            return await _authService.ForgotPassword(model);
        }

        [HttpPost]
        public ActionResult<ResponseResult> VerifyOTP([FromBody] VerifyOtpDto model)
        {
            return _authService.VerifyOTP(model);
        }
        [HttpPost]
        public async Task<ActionResult<ResponseResult>> ResetPassword([FromBody] ResetPasswordDto model)
        {
            if (!ModelState.IsValid)
                return new BadRequestObjectResult(ModelState);
            return await _authService.ResetPassword(model);
        }

        [HttpPost]
        public async Task<ActionResult<ResponseResult>> ConfirmEmail([FromBody] ConfirmEmailDto model)
        {
            return await _authService.ConfirmEmail(model.Otp, model.Email);
        }

        [HttpPut]
        [Authorize]
        public async Task<ActionResult<RegisterResponse>> UpdateProfile([FromBody] UpdateProfileDto model)
        {
            if (!ModelState.IsValid)
                return new BadRequestObjectResult(ModelState);
            return await _authService.UpdateProfile(model);
        }

        // [HttpGet]
        // public ActionResult<ResponseResult> GetClientId([FromQuery] string provider, string? platform = "website")
        // {
        //     var res = _oauthService.GetClientId(provider, platform!);
        //     return res;
        // }

        // [HttpPost]
        // public async Task<ActionResult<LoginResponse>> ExternalLogin([FromQuery] string code, string provider)
        // {
        //     if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(provider))
        //     {
        //         return new LoginResponse
        //         {
        //             Message = Strings.Messages.OauthRequire,
        //             IsSuccess = false
        //         };
        //     }
        //     else if (provider.Trim().ToLower().Equals("google"))
        //     {
        //         return await _oauthService.GetUserByGoogle(code);
        //     }
        //     else if (provider.Trim().ToLower().Equals("facebook"))
        //     {
        //         return await _oauthService.GetUserByFacebook(code);
        //     }
        //     else
        //     {
        //         return new LoginResponse
        //         {
        //             Message = Strings.Messages.OauthUnsupportedProvider,
        //             IsSuccess = false
        //         };
        //     }
        // }

        // [HttpPost]
        // public async Task<ActionResult<LoginResponse>> ExternalLoginMobile([FromQuery] OauthVModel model)
        // {
        //     return await _oauthService.OauthLogin(model);
        // }
        
        [HttpPost]
        public async Task<ActionResult<RegisterResponse>> ResendOTPRegister([FromBody] ResendOTPDto model)
        {
            if (!ModelState.IsValid)
                return new BadRequestObjectResult(ModelState);
            return await _authService.ResendOTPRegister(model.Email);
        }



    }
}
