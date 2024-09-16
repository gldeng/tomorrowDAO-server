using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AElf.Indexing.Elasticsearch;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;
using TomorrowDAOServer.Common;
using TomorrowDAOServer.Entities;
using TomorrowDAOServer.Enums;
using TomorrowDAOServer.Options;
using TomorrowDAOServer.User.Dtos;
using TomorrowDAOServer.User.Provider;
using Volo.Abp;
using Volo.Abp.Auditing;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Users;

namespace TomorrowDAOServer.User;

[RemoteService(IsEnabled = false)]
[DisableAuditing]
public class UserAppService : TomorrowDAOServerAppService, IUserAppService
{
    private readonly INESTRepository<UserIndex, Guid> _userIndexRepository;
    private readonly ILogger<UserAppService> _logger;
    private readonly IObjectMapper _objectMapper;
    private readonly IUserProvider _userProvider;
    private readonly IUserSourceProvider _userSourceProvider;
    private readonly IOptionsMonitor<UserOptions> _userOptions;

    public UserAppService(INESTRepository<UserIndex, Guid> userIndexRepository, ILogger<UserAppService> logger,
        IObjectMapper objectMapper, IUserProvider userProvider, IUserSourceProvider userSourceProvider, 
        IOptionsMonitor<UserOptions> userOptions)
    {
        _userIndexRepository = userIndexRepository;
        _logger = logger;
        _objectMapper = objectMapper;
        _userProvider = userProvider;
        _userSourceProvider = userSourceProvider;
        _userOptions = userOptions;
    }

    public async Task CreateUserAsync(UserDto user)
    {
        try
        {
            var userIndex = _objectMapper.Map<UserDto, UserIndex>(user);
            await _userIndexRepository.AddOrUpdateAsync(userIndex);
            _logger.LogInformation("Create user success, userId:{userId}, appId:{appId}", user.UserId, user.AppId);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Create user error, userId:{userId}, appId:{appId}", user.UserId, user.AppId);
        }
    }

    public async Task<UserDto> GetUserByIdAsync(string userId)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<UserIndex>, QueryContainer>>() { };
        mustQuery.Add(q => q.Term(i => i.Field(t => t.UserId).Value(userId)));

        QueryContainer Filter(QueryContainerDescriptor<UserIndex> f) => f.Bool(b => b.Must(mustQuery));
        var (totalCount, users) = await _userIndexRepository.GetListAsync(Filter);
        if (totalCount != 1)
        {
            throw new UserFriendlyException("User count: {count}");
        }

        return ObjectMapper.Map<UserIndex, UserDto>(users.First());
    }

    public async Task<List<UserIndex>> GetUserByCaHashListAsync(List<string> caHashes)
    {
        if (caHashes.IsNullOrEmpty())
        {
            return new List<UserIndex>();
        }
        var mustQuery = new List<Func<QueryContainerDescriptor<UserIndex>, QueryContainer>>
        {
            q => q.Terms(i => i.Field(t => t.CaHash).Terms(caHashes))
        };
        QueryContainer Filter(QueryContainerDescriptor<UserIndex> f) => f.Bool(b => b.Must(mustQuery));
        var (_, list) = await _userIndexRepository.GetListAsync(Filter);
        return list;
    }

    public async Task<UserIndex> GetUserByCaHashAsync(string caHash)
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<UserIndex>, QueryContainer>>
        {
            q => q.Term(i => i.Field(t => t.CaHash).Value(caHash))
        };
        QueryContainer Filter(QueryContainerDescriptor<UserIndex> f) => f.Bool(b => b.Must(mustQuery));
        return await _userIndexRepository.GetAsync(Filter);
    }

    public async Task<string> GetUserAddressByCaHashAsync(string chainId, string caHash)
    {
        var user = await GetUserByCaHashAsync(caHash);
        return user?.AddressInfos?.FirstOrDefault(x => x.ChainId == chainId)?.Address ?? string.Empty;
    }

    public async Task<List<UserIndex>> GetUser()
    {
        var mustQuery = new List<Func<QueryContainerDescriptor<UserIndex>, QueryContainer>>();
        QueryContainer Filter(QueryContainerDescriptor<UserIndex> f) => f.Bool(b => b.Must(mustQuery));
        return (await _userIndexRepository.GetListAsync(Filter)).Item2;
    }

    public async Task<UserSourceReportResultDto> UserSourceReportAsync(string chainId, long source)
    {
        var address = await _userProvider.GetAndValidateUserAddressAsync(
            CurrentUser.IsAuthenticated ? CurrentUser.GetId() : Guid.Empty, chainId);
        var userSourceList = _userOptions.CurrentValue.UserSourceList;
        if (!userSourceList.Contains(source))
        {
            return new UserSourceReportResultDto
            {
                Success = false, Reason = "Invalid soure."
            };
        }
        var now = TimeHelper.GetTimeStampInMilliseconds();
        await _userSourceProvider.AddOrUpdateAsync(new UserSourceIndex
        {
            Id = GuidHelper.GenerateId(address, TimeHelper.GetTimeStampInMilliseconds().ToString()),
            Address = address,
            UserVisitType = UserVisitType.Votigram,
            Source = source,
            VisitTime = now
        });
        return new UserSourceReportResultDto
        {
            Success = true
        };
    }
}