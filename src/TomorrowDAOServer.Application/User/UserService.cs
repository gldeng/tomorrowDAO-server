using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using TomorrowDAOServer.Common;
using TomorrowDAOServer.Entities;
using TomorrowDAOServer.Enums;
using TomorrowDAOServer.Options;
using TomorrowDAOServer.User.Dtos;
using TomorrowDAOServer.User.Provider;
using Volo.Abp;
using Volo.Abp.Users;

namespace TomorrowDAOServer.User;

public class UserService : TomorrowDAOServerAppService, IUserService
{
    private readonly IUserProvider _userProvider;
    private readonly IOptionsMonitor<UserOptions> _userOptions;
    private readonly IUserVisitProvider _userVisitProvider;
    private readonly IUserVisitSummaryProvider _userVisitSummaryProvider;
    private readonly IUserPointsRecordProvider _userPointsRecordProvider;
    private readonly IUserTaskProvider _userTaskProvider;

    public UserService(IUserProvider userProvider, IOptionsMonitor<UserOptions> userOptions,
        IUserVisitProvider userVisitProvider, IUserVisitSummaryProvider userVisitSummaryProvider, 
        IUserTaskProvider userTaskProvider, IUserPointsRecordProvider userPointsRecordProvider)
    {
        _userProvider = userProvider;
        _userOptions = userOptions;
        _userVisitProvider = userVisitProvider;
        _userVisitSummaryProvider = userVisitSummaryProvider;
        _userTaskProvider = userTaskProvider;
        _userPointsRecordProvider = userPointsRecordProvider;
    }

    public async Task<UserSourceReportResultDto> UserSourceReportAsync(string chainId, string source)
    {
        var address = await _userProvider.GetAndValidateUserAddressAsync(
            CurrentUser.IsAuthenticated ? CurrentUser.GetId() : Guid.Empty, chainId);
        var userSourceList = _userOptions.CurrentValue.UserSourceList;
        if (!userSourceList.Contains(source, StringComparer.OrdinalIgnoreCase))
        {
            return new UserSourceReportResultDto
            {
                Success = false, Reason = "Invalid source."
            };
        }
        var matchedSource = userSourceList.FirstOrDefault(s => 
            string.Equals(s, source, StringComparison.OrdinalIgnoreCase));
        var now = TimeHelper.GetTimeStampInMilliseconds();
        var visitType = UserVisitType.Votigram;
        await _userVisitProvider.AddOrUpdateAsync(new UserVisitIndex
        {
            Id = GuidHelper.GenerateId(address, chainId, visitType.ToString(), matchedSource, now.ToString()),
            ChainId = chainId,
            Address = address,
            UserVisitType = visitType,
            Source = matchedSource!,
            VisitTime = now
        });
        var summaryId = GuidHelper.GenerateId(address, chainId, visitType.ToString(), matchedSource);
        var visitSummaryIndex = await _userVisitSummaryProvider.GetByIdAsync(summaryId);
        if (visitSummaryIndex == null)
        {
            visitSummaryIndex = new UserVisitSummaryIndex
            {
                Id = summaryId,
                ChainId = chainId,
                Address = address,
                UserVisitType = visitType,
                Source = matchedSource!,
                CreateTime = now,
                ModificationTime = now
            };
        }
        else
        {
            visitSummaryIndex.ModificationTime = now;
        }
        await _userVisitSummaryProvider.AddOrUpdateAsync(visitSummaryIndex);
        
        return new UserSourceReportResultDto
        {
            Success = true
        };
    }

    public async Task<bool> CompleteTaskAsync(CompleteTaskInput input)
    {
        var address = await _userProvider.GetAndValidateUserAddressAsync(
            CurrentUser.IsAuthenticated ? CurrentUser.GetId() : Guid.Empty, input.ChainId);
        var (userTask, userTaskDetail) = CheckUserTask(input);
        var completeTime = await _userTaskProvider.GetUserTaskCompleteTimeAsync(input.ChainId, address, userTask, userTaskDetail);
        if (completeTime < 0)
        {
            throw new UserFriendlyException("Complete Task Fail.");
        }

        var completeTimeDate = TimeHelper.GetDateTimeFromTimeStamp(completeTime);
        await _userTaskProvider.GenerateCompleteTaskAsync(input.ChainId, address, userTaskDetail, completeTimeDate);
        await _userPointsRecordProvider.GenerateTaskPointsRecordAsync(input.ChainId, address, userTaskDetail, completeTimeDate);
        return true;
    }

    private Tuple<UserTask, UserTaskDetail> CheckUserTask(CompleteTaskInput input)
    {
        if (!Enum.TryParse<UserTask>(input.UserTask, out var userTask))
        {
            throw new UserFriendlyException("Invalid UserTask.");
        }
        
        if (!Enum.TryParse<UserTaskDetail>(input.UserTask, out var userTaskDetail))
        {
            throw new UserFriendlyException("Invalid UserTaskDetail.");
        }

        if (userTask != TaskPointsHelper.GetUserTaskFromUserTaskDetail(userTaskDetail))
        {
            throw new UserFriendlyException("UserTaskDetail and UserTask not match.");
        }

        if (!TaskPointsHelper.FrontEndTaskDetails.Contains(userTaskDetail))
        {
            throw new UserFriendlyException("Can not complete UserTaskDetail.");
        }

        return new Tuple<UserTask, UserTaskDetail>(userTask, userTaskDetail);
    }
}