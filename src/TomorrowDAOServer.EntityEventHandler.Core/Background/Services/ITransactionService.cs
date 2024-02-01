using System.Threading.Tasks;
using AElf.Client.Dto;
using Google.Protobuf;

namespace TomorrowDAOServer.EntityEventHandler.Core.Background.Services;

public interface ITransactionService
{
    Task<string> SendTransactionAsync(string chainName, string privateKey, string toAddress,
        string methodName, IMessage txParam);

    Task<TransactionResultDto> GetTransactionById(string chainName, string txId);
}