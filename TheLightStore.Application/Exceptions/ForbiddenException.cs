using System;

namespace TheLightStore.Application.Exceptions;

public class ForbiddenException : ApiException
{
    public ForbiddenException(string message = "Forbidden access") : base(message, 403) { }
}
