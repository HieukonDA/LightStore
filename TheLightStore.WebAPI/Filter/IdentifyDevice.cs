using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using TheLightStore.Domain.Constants;

namespace TheLightStore.WebAPI.Filter;

public class IdentifyDevice : ActionFilterAttribute
{
    public readonly bool _allowMobile;
    private readonly bool _allowBrowser;

    public IdentifyDevice(bool allowMobile = false, bool allowBrowser = false)
    {
        _allowMobile = allowMobile;
        _allowBrowser = allowBrowser;
    }
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var deviceDetectionService = context.HttpContext.RequestServices.GetService(typeof(DeviceDetectionService)) as DeviceDetectionService ?? throw new InvalidOperationException("DeviceDetectionService is not registered.");
        bool isMobile = deviceDetectionService?.IsMobile(context.HttpContext) ?? false;
        bool isBrowser = deviceDetectionService?.IsBrowser(context.HttpContext) ?? false;


        var response = new
        {
            ErrorMessage = string.Empty,
            SuccessMessage = string.Empty
        };
        if (isMobile && !_allowMobile)
        {
            response = new
            {
                ErrorMessage = Strings.Messages.DeniedMobile,
                SuccessMessage = string.Empty
            };
        }
        else if (isBrowser && !_allowBrowser)
        {
            response = new
            {
                ErrorMessage = Strings.Messages.DeniedBrowser,
                SuccessMessage = string.Empty
            };
        }
        if (!string.IsNullOrEmpty(response.ErrorMessage))
        {
            context.Result = new ContentResult
            {
                StatusCode = StatusCodes.Status403Forbidden,
                Content = JsonConvert.SerializeObject(response),
                ContentType = "application/json"
            };
        }
        else
        {
            base.OnActionExecuting(context);
        }
    }
}
