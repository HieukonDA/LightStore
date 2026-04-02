using System;

namespace TheLightStore.Application.Exceptions;

public class BadRequestException : ApiException
{
    public BadRequestException(string message) : base(message, 400) { }
}
