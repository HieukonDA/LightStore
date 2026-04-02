using System;

namespace TheLightStore.Application.Exceptions;

public class UnauthorizedException : ApiException
{
    public UnauthorizedException(string message = "Unauthorized access") : base(message, 401) { }
}
