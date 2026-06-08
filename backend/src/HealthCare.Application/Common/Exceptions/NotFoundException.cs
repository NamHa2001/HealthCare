namespace HealthCare.Application.Common.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string name, object key)
        : base($"Không tìm thấy \"{name}\" với key ({key}).") { }
}