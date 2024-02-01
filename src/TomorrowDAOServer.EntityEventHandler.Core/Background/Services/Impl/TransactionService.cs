using System.Threading.Tasks;
using AElf;
using AElf.Client.Dto;
using TomorrowDAOServer.EntityEventHandler.Core.Background.Providers;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Volo.Abp.DependencyInjection;

namespace TomorrowDAOServer.EntityEventHandler.Core.Background.Services.Impl;

public class TransactionService : ITransactionService, ITransientDependency
{
    private readonly IAElfClientProvider _clientProvider;
    private readonly ILogger<TransactionService> _logger;

    public TransactionService(IAElfClientProvider clientProvider, ILogger<TransactionService> logger)
    {
        _clientProvider = clientProvider;
        _logger = logger;
    }

    public async Task<string> SendTransactionAsync(string chainName, string privateKey, string toAddress,
        string methodName, IMessage txParam)
    {
        _logger.LogInformation("SendTransactionAsyncBegin ToAddress={toAddress} ChainName={chainName} MethodName={methodName}", toAddress, chainName, methodName);
        var client = _clientProvider.GetClient(chainName);
        var ownerAddress = client.GetAddressFromPrivateKey(privateKey);
        var transaction = await client.GenerateTransactionAsync(ownerAddress, toAddress, methodName, txParam);
        var signedTransaction = client.SignTransaction(privateKey, transaction);

        var result = await client.SendTransactionAsync(new SendTransactionInput
        {
            RawTransaction = signedTransaction.ToByteArray().ToHex()
        });
        _logger.LogInformation("SendTransactionAsyncEnd ToAddress={toAddress} ChainName={chainName} MethodName={methodName}, TransactionId={transactionId}", toAddress, chainName, methodName, result.TransactionId);
        return result.TransactionId;
    }

    public async Task<TransactionResultDto> GetTransactionById(string chainName, string txId)
    {
        var client = _clientProvider.GetClient(chainName);
        return await client.GetTransactionResultAsync(txId);
    }
}