namespace HealthCare.Application.Common.Exceptions;

public class ForbiddenException : Exception
{
    public ForbiddenException() : base("Không có quyền thực hiện thao tác này.") { }
    public ForbiddenException(string message) : base(message) { }
}