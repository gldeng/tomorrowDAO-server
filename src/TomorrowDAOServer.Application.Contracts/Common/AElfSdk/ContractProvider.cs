using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AElf;
using AElf.Client.Dto;
using AElf.Client.Service;
using AElf.Types;
using Google.Protobuf;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using TomorrowDAOServer.Common.AElfSdk.Dtos;
using TomorrowDAOServer.Options;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Threading;

namespace TomorrowDAOServer.Common.AElfSdk;

public interface IContractProvider
{
    
    Task<(Hash transactionId, Transaction transaction)> CreateCallTransactionAsync(string chainId,
        string contractName, string methodName, IMessage param);
    
    Task<(Hash transactionId, Transaction transaction)> CreateTransactionAsync(string chainId, string senderPublicKey,
        string contractName, string methodName,
        IMessage param);

    // Task SendTransactionAsync(string chainId, Transaction transaction);

    Task<T> CallTransactionAsync<T>(string chainId, Transaction transaction) where T : class;

    Task<TransactionResultDto> QueryTransactionResultAsync(string transactionId, string chainId);
}

public class ContractProvider : IContractProvider, ISingletonDependency
{
    private readonly JsonSerializerSettings _settings = JsonSettingsBuilder.New().WithAElfTypesConverters().Build(); 
    private readonly Dictionary<string, AElfClient> _clients = new();
    private readonly Dictionary<string, SenderAccount> _accounts = new();
    private readonly Dictionary<string, string> _emptyDict = new();
    private readonly Dictionary<string, Dictionary<string, string>> _contractAddress = new();
    private readonly SenderAccount _callTxSender;

    private readonly ChainOptions _chainOptions;
    private readonly ILogger<ContractProvider> _logger;

    public ContractProvider(IOptions<ChainOptions> chainOption, ILogger<ContractProvider> logger)
    {
        _logger = logger;
        _chainOptions = chainOption.Value;
        InitAElfClient();
        _callTxSender = new SenderAccount(_chainOptions.PrivateKeyForCallTx);
    }


    private void InitAElfClient()
    {
        if (_chainOptions.ChainInfos.IsNullOrEmpty())
        {
            return;
        }

        foreach (var node in _chainOptions.ChainInfos)
        {
            _clients[node.Key] = new AElfClient(node.Value.BaseUrl);
            _logger.LogInformation("init AElfClient: {ChainId}, {Node}", node.Key, node.Value);
        }
    }

    private AElfClient Client(string chainId)
    {
        AssertHelper.IsTrue(_clients.ContainsKey(chainId), "AElfClient of {chainId} not found.", chainId);
        return _clients[chainId];
    }

    
    private string ContractAddress(string chainId, string contractName)
    {
        _ = _chainOptions.ChainInfos.TryGetValue(chainId, out var chainInfo);
        var contractAddress = _contractAddress.GetOrAdd(chainId, _ => new Dictionary<string, string>());
        return contractAddress.GetOrAdd(contractName, name =>
        {
            var address = (chainInfo?.ContractAddress ?? new Dictionary<string, Dictionary<string, string>>())
                .GetValueOrDefault(chainId, _emptyDict)
                .GetValueOrDefault(name, null);
            if (address.IsNullOrEmpty() && SystemContractName.All.Contains(name))
                address = AsyncHelper
                    .RunSync(() => Client(chainId).GetContractAddressByNameAsync(HashHelper.ComputeFrom(name)))
                    .ToBase58();

            AssertHelper.NotEmpty(address, "Address of contract {contractName} on {chainId} not exits.",
                name, chainId);
            return address;
        });
    }

    public Task<TransactionResultDto> QueryTransactionResultAsync(string transactionId, string chainId)
    {
        return Client(chainId).GetTransactionResultAsync(transactionId);
    }


    public async Task<(Hash transactionId, Transaction transaction)> CreateCallTransactionAsync(string chainId,
        string contractName, string methodName, IMessage param)
    {
        return await CreateTransactionAsync(chainId, _callTxSender.PublicKey.ToHex(), contractName, methodName,
            param);
    }

    public async Task<(Hash transactionId, Transaction transaction)> CreateTransactionAsync(string chainId,
        string senderPublicKey, string contractName, string methodName,
        IMessage param)
    {
        var address = ContractAddress(chainId, contractName);
        var client = Client(chainId);
        var status = await client.GetChainStatusAsync();
        var height = status.BestChainHeight;
        var blockHash = status.BestChainHash;

        // create raw transaction
        var transaction = new Transaction
        {
            From = Address.FromPublicKey(ByteArrayHelper.HexStringToByteArray(senderPublicKey)),
            To = Address.FromBase58(address),
            MethodName = methodName,
            Params = param.ToByteString(),
            RefBlockNumber = height,
            RefBlockPrefix = ByteString.CopyFrom(Hash.LoadFromHex(blockHash).Value.Take(4).ToArray())
        };

        return (transaction.GetHash(), transaction);
    }

    public async Task<T> CallTransactionAsync<T>(string chainId, Transaction transaction) where T : class
    {
        var client = Client(chainId);
        transaction.From = _callTxSender.Address;
        transaction.Signature = _callTxSender.GetSignatureWith(transaction.ToByteArray());
        var rawTransactionResult = await client.ExecuteRawTransactionAsync(new ExecuteRawTransactionDto()
        {
            RawTransaction = transaction.ToByteArray().ToHex(),
            Signature = transaction.Signature.ToHex()
        });

        if (typeof(T) == typeof(string))
        {
            return rawTransactionResult?.Substring(1, rawTransactionResult.Length - 2) as T;
        }

        return (T)JsonConvert.DeserializeObject(rawTransactionResult, typeof(T), _settings);
    }
    
}