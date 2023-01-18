namespace BattleSimulator.Server.Models;

public class ApiError
{
    public string Message { get; private set; }
    public ApiError(string message)
    {
        this.Message = message;
    }
}