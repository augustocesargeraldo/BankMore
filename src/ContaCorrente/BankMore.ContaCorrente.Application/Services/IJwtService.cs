namespace BankMore.ContaCorrente.Application.Services
{
    public interface IJwtService
    {
        string GerarToken(string idContaCorrente);
    }
}
