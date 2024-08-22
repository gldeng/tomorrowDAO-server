// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: Protobuf/contract/vote_contract.proto
// </auto-generated>
#pragma warning disable 0414, 1591
#region Designer generated code

using System.Collections.Generic;
using aelf = global::AElf.CSharp.Core;

namespace TomorrowDAO.Contracts.Vote {

  #region Events
  public partial class VoteSchemeCreated : aelf::IEvent<VoteSchemeCreated>
  {
    public global::System.Collections.Generic.IEnumerable<VoteSchemeCreated> GetIndexed()
    {
      return new List<VoteSchemeCreated>
      {
      };
    }

    public VoteSchemeCreated GetNonIndexed()
    {
      return new VoteSchemeCreated
      {
        VoteSchemeId = VoteSchemeId,
        VoteMechanism = VoteMechanism,
        WithoutLockToken = WithoutLockToken,
        VoteStrategy = VoteStrategy,
      };
    }
  }

  public partial class VotingItemRegistered : aelf::IEvent<VotingItemRegistered>
  {
    public global::System.Collections.Generic.IEnumerable<VotingItemRegistered> GetIndexed()
    {
      return new List<VotingItemRegistered>
      {
      };
    }

    public VotingItemRegistered GetNonIndexed()
    {
      return new VotingItemRegistered
      {
        DaoId = DaoId,
        VotingItemId = VotingItemId,
        SchemeId = SchemeId,
        AcceptedCurrency = AcceptedCurrency,
        RegisterTimestamp = RegisterTimestamp,
        StartTimestamp = StartTimestamp,
        EndTimestamp = EndTimestamp,
      };
    }
  }

  public partial class Voted : aelf::IEvent<Voted>
  {
    public global::System.Collections.Generic.IEnumerable<Voted> GetIndexed()
    {
      return new List<Voted>
      {
      };
    }

    public Voted GetNonIndexed()
    {
      return new Voted
      {
        VotingItemId = VotingItemId,
        Voter = Voter,
        Amount = Amount,
        VoteTimestamp = VoteTimestamp,
        Option = Option,
        VoteId = VoteId,
        DaoId = DaoId,
        VoteMechanism = VoteMechanism,
        StartTime = StartTime,
        EndTime = EndTime,
        Memo = Memo,
      };
    }
  }

  public partial class Withdrawn : aelf::IEvent<Withdrawn>
  {
    public global::System.Collections.Generic.IEnumerable<Withdrawn> GetIndexed()
    {
      return new List<Withdrawn>
      {
      };
    }

    public Withdrawn GetNonIndexed()
    {
      return new Withdrawn
      {
        DaoId = DaoId,
        Withdrawer = Withdrawer,
        WithdrawAmount = WithdrawAmount,
        WithdrawTimestamp = WithdrawTimestamp,
        VotingItemIdList = VotingItemIdList,
      };
    }
  }

  public partial class EmergencyStatusSet : aelf::IEvent<EmergencyStatusSet>
  {
    public global::System.Collections.Generic.IEnumerable<EmergencyStatusSet> GetIndexed()
    {
      return new List<EmergencyStatusSet>
      {
      };
    }

    public EmergencyStatusSet GetNonIndexed()
    {
      return new EmergencyStatusSet
      {
        DaoId = DaoId,
        EmergencyStatus = EmergencyStatus,
      };
    }
  }

  public partial class Staked : aelf::IEvent<Staked>
  {
    public global::System.Collections.Generic.IEnumerable<Staked> GetIndexed()
    {
      return new List<Staked>
      {
      };
    }

    public Staked GetNonIndexed()
    {
      return new Staked
      {
        DaoId = DaoId,
        Amount = Amount,
        Sender = Sender,
        Symbol = Symbol,
      };
    }
  }

  public partial class UnStakeRequested : aelf::IEvent<UnStakeRequested>
  {
    public global::System.Collections.Generic.IEnumerable<UnStakeRequested> GetIndexed()
    {
      return new List<UnStakeRequested>
      {
      };
    }

    public UnStakeRequested GetNonIndexed()
    {
      return new UnStakeRequested
      {
        DaoId = DaoId,
        Amount = Amount,
        Sender = Sender,
        TokenSymbol = TokenSymbol,
      };
    }
  }

  public partial class StakeableTokenSet : aelf::IEvent<StakeableTokenSet>
  {
    public global::System.Collections.Generic.IEnumerable<StakeableTokenSet> GetIndexed()
    {
      return new List<StakeableTokenSet>
      {
      };
    }

    public StakeableTokenSet GetNonIndexed()
    {
      return new StakeableTokenSet
      {
        DaoId = DaoId,
        AcceptedTokenList = AcceptedTokenList,
      };
    }
  }

  public partial class VotingPowerWeightSet : aelf::IEvent<VotingPowerWeightSet>
  {
    public global::System.Collections.Generic.IEnumerable<VotingPowerWeightSet> GetIndexed()
    {
      return new List<VotingPowerWeightSet>
      {
      };
    }

    public VotingPowerWeightSet GetNonIndexed()
    {
      return new VotingPowerWeightSet
      {
        DaoId = DaoId,
        TokenSymbol = TokenSymbol,
        Weight = Weight,
      };
    }
  }

  public partial class MaxVotingPowersSet : aelf::IEvent<MaxVotingPowersSet>
  {
    public global::System.Collections.Generic.IEnumerable<MaxVotingPowersSet> GetIndexed()
    {
      return new List<MaxVotingPowersSet>
      {
      };
    }

    public MaxVotingPowersSet GetNonIndexed()
    {
      return new MaxVotingPowersSet
      {
        DaoId = DaoId,
        TokenSymbol = TokenSymbol,
        MaxVotingPower = MaxVotingPower,
      };
    }
  }

  #endregion
  public static partial class VoteContractContainer
  {
    static readonly string __ServiceName = "VoteContract";

    #region Marshallers
    static readonly aelf::Marshaller<global::TomorrowDAO.Contracts.Vote.InitializeInput> __Marshaller_InitializeInput = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::TomorrowDAO.Contracts.Vote.InitializeInput.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::Google.Protobuf.WellKnownTypes.Empty> __Marshaller_google_protobuf_Empty = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::Google.Protobuf.WellKnownTypes.Empty.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::TomorrowDAO.Contracts.Vote.CreateVoteSchemeInput> __Marshaller_CreateVoteSchemeInput = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::TomorrowDAO.Contracts.Vote.CreateVoteSchemeInput.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::TomorrowDAO.Contracts.Vote.VotingRegisterInput> __Marshaller_VotingRegisterInput = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::TomorrowDAO.Contracts.Vote.VotingRegisterInput.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::TomorrowDAO.Contracts.Vote.VoteInput> __Marshaller_VoteInput = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::TomorrowDAO.Contracts.Vote.VoteInput.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::TomorrowDAO.Contracts.Vote.WithdrawInput> __Marshaller_WithdrawInput = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::TomorrowDAO.Contracts.Vote.WithdrawInput.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::TomorrowDAO.Contracts.Vote.SetEmergencyStatusInput> __Marshaller_SetEmergencyStatusInput = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::TomorrowDAO.Contracts.Vote.SetEmergencyStatusInput.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::AElf.Types.Hash> __Marshaller_aelf_Hash = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::AElf.Types.Hash.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::TomorrowDAO.Contracts.Vote.VoteScheme> __Marshaller_VoteScheme = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::TomorrowDAO.Contracts.Vote.VoteScheme.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::TomorrowDAO.Contracts.Vote.VotingItem> __Marshaller_VotingItem = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::TomorrowDAO.Contracts.Vote.VotingItem.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::TomorrowDAO.Contracts.Vote.VotingResult> __Marshaller_VotingResult = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::TomorrowDAO.Contracts.Vote.VotingResult.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::TomorrowDAO.Contracts.Vote.GetVotingRecordInput> __Marshaller_GetVotingRecordInput = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::TomorrowDAO.Contracts.Vote.GetVotingRecordInput.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::TomorrowDAO.Contracts.Vote.VotingRecord> __Marshaller_VotingRecord = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::TomorrowDAO.Contracts.Vote.VotingRecord.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::TomorrowDAO.Contracts.Vote.GetVirtualAddressInput> __Marshaller_GetVirtualAddressInput = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::TomorrowDAO.Contracts.Vote.GetVirtualAddressInput.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::AElf.Types.Address> __Marshaller_aelf_Address = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::AElf.Types.Address.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::TomorrowDAO.Contracts.Vote.GetDaoRemainAmountInput> __Marshaller_GetDaoRemainAmountInput = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::TomorrowDAO.Contracts.Vote.GetDaoRemainAmountInput.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::TomorrowDAO.Contracts.Vote.DaoRemainAmount> __Marshaller_DaoRemainAmount = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::TomorrowDAO.Contracts.Vote.DaoRemainAmount.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::TomorrowDAO.Contracts.Vote.GetProposalRemainAmountInput> __Marshaller_GetProposalRemainAmountInput = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::TomorrowDAO.Contracts.Vote.GetProposalRemainAmountInput.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::TomorrowDAO.Contracts.Vote.ProposalRemainAmount> __Marshaller_ProposalRemainAmount = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::TomorrowDAO.Contracts.Vote.ProposalRemainAmount.Parser.ParseFrom);
    static readonly aelf::Marshaller<global::TomorrowDAO.Contracts.Vote.AddressList> __Marshaller_AddressList = aelf::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::TomorrowDAO.Contracts.Vote.AddressList.Parser.ParseFrom);
    #endregion

    #region Methods
    static readonly aelf::Method<global::TomorrowDAO.Contracts.Vote.InitializeInput, global::Google.Protobuf.WellKnownTypes.Empty> __Method_Initialize = new aelf::Method<global::TomorrowDAO.Contracts.Vote.InitializeInput, global::Google.Protobuf.WellKnownTypes.Empty>(
        aelf::MethodType.Action,
        __ServiceName,
        "Initialize",
        __Marshaller_InitializeInput,
        __Marshaller_google_protobuf_Empty);

    static readonly aelf::Method<global::TomorrowDAO.Contracts.Vote.CreateVoteSchemeInput, global::Google.Protobuf.WellKnownTypes.Empty> __Method_CreateVoteScheme = new aelf::Method<global::TomorrowDAO.Contracts.Vote.CreateVoteSchemeInput, global::Google.Protobuf.WellKnownTypes.Empty>(
        aelf::MethodType.Action,
        __ServiceName,
        "CreateVoteScheme",
        __Marshaller_CreateVoteSchemeInput,
        __Marshaller_google_protobuf_Empty);

    static readonly aelf::Method<global::TomorrowDAO.Contracts.Vote.VotingRegisterInput, global::Google.Protobuf.WellKnownTypes.Empty> __Method_Register = new aelf::Method<global::TomorrowDAO.Contracts.Vote.VotingRegisterInput, global::Google.Protobuf.WellKnownTypes.Empty>(
        aelf::MethodType.Action,
        __ServiceName,
        "Register",
        __Marshaller_VotingRegisterInput,
        __Marshaller_google_protobuf_Empty);

    static readonly aelf::Method<global::TomorrowDAO.Contracts.Vote.VoteInput, global::Google.Protobuf.WellKnownTypes.Empty> __Method_Vote = new aelf::Method<global::TomorrowDAO.Contracts.Vote.VoteInput, global::Google.Protobuf.WellKnownTypes.Empty>(
        aelf::MethodType.Action,
        __ServiceName,
        "Vote",
        __Marshaller_VoteInput,
        __Marshaller_google_protobuf_Empty);

    static readonly aelf::Method<global::TomorrowDAO.Contracts.Vote.WithdrawInput, global::Google.Protobuf.WellKnownTypes.Empty> __Method_Withdraw = new aelf::Method<global::TomorrowDAO.Contracts.Vote.WithdrawInput, global::Google.Protobuf.WellKnownTypes.Empty>(
        aelf::MethodType.Action,
        __ServiceName,
        "Withdraw",
        __Marshaller_WithdrawInput,
        __Marshaller_google_protobuf_Empty);

    static readonly aelf::Method<global::TomorrowDAO.Contracts.Vote.SetEmergencyStatusInput, global::Google.Protobuf.WellKnownTypes.Empty> __Method_SetEmergencyStatus = new aelf::Method<global::TomorrowDAO.Contracts.Vote.SetEmergencyStatusInput, global::Google.Protobuf.WellKnownTypes.Empty>(
        aelf::MethodType.Action,
        __ServiceName,
        "SetEmergencyStatus",
        __Marshaller_SetEmergencyStatusInput,
        __Marshaller_google_protobuf_Empty);

    static readonly aelf::Method<global::AElf.Types.Hash, global::TomorrowDAO.Contracts.Vote.VoteScheme> __Method_GetVoteScheme = new aelf::Method<global::AElf.Types.Hash, global::TomorrowDAO.Contracts.Vote.VoteScheme>(
        aelf::MethodType.View,
        __ServiceName,
        "GetVoteScheme",
        __Marshaller_aelf_Hash,
        __Marshaller_VoteScheme);

    static readonly aelf::Method<global::AElf.Types.Hash, global::TomorrowDAO.Contracts.Vote.VotingItem> __Method_GetVotingItem = new aelf::Method<global::AElf.Types.Hash, global::TomorrowDAO.Contracts.Vote.VotingItem>(
        aelf::MethodType.View,
        __ServiceName,
        "GetVotingItem",
        __Marshaller_aelf_Hash,
        __Marshaller_VotingItem);

    static readonly aelf::Method<global::AElf.Types.Hash, global::TomorrowDAO.Contracts.Vote.VotingResult> __Method_GetVotingResult = new aelf::Method<global::AElf.Types.Hash, global::TomorrowDAO.Contracts.Vote.VotingResult>(
        aelf::MethodType.View,
        __ServiceName,
        "GetVotingResult",
        __Marshaller_aelf_Hash,
        __Marshaller_VotingResult);

    static readonly aelf::Method<global::TomorrowDAO.Contracts.Vote.GetVotingRecordInput, global::TomorrowDAO.Contracts.Vote.VotingRecord> __Method_GetVotingRecord = new aelf::Method<global::TomorrowDAO.Contracts.Vote.GetVotingRecordInput, global::TomorrowDAO.Contracts.Vote.VotingRecord>(
        aelf::MethodType.View,
        __ServiceName,
        "GetVotingRecord",
        __Marshaller_GetVotingRecordInput,
        __Marshaller_VotingRecord);

    static readonly aelf::Method<global::TomorrowDAO.Contracts.Vote.GetVirtualAddressInput, global::AElf.Types.Address> __Method_GetVirtualAddress = new aelf::Method<global::TomorrowDAO.Contracts.Vote.GetVirtualAddressInput, global::AElf.Types.Address>(
        aelf::MethodType.View,
        __ServiceName,
        "GetVirtualAddress",
        __Marshaller_GetVirtualAddressInput,
        __Marshaller_aelf_Address);

    static readonly aelf::Method<global::TomorrowDAO.Contracts.Vote.GetDaoRemainAmountInput, global::TomorrowDAO.Contracts.Vote.DaoRemainAmount> __Method_GetDaoRemainAmount = new aelf::Method<global::TomorrowDAO.Contracts.Vote.GetDaoRemainAmountInput, global::TomorrowDAO.Contracts.Vote.DaoRemainAmount>(
        aelf::MethodType.View,
        __ServiceName,
        "GetDaoRemainAmount",
        __Marshaller_GetDaoRemainAmountInput,
        __Marshaller_DaoRemainAmount);

    static readonly aelf::Method<global::TomorrowDAO.Contracts.Vote.GetProposalRemainAmountInput, global::TomorrowDAO.Contracts.Vote.ProposalRemainAmount> __Method_GetProposalRemainAmount = new aelf::Method<global::TomorrowDAO.Contracts.Vote.GetProposalRemainAmountInput, global::TomorrowDAO.Contracts.Vote.ProposalRemainAmount>(
        aelf::MethodType.View,
        __ServiceName,
        "GetProposalRemainAmount",
        __Marshaller_GetProposalRemainAmountInput,
        __Marshaller_ProposalRemainAmount);

    static readonly aelf::Method<global::Google.Protobuf.WellKnownTypes.Empty, global::TomorrowDAO.Contracts.Vote.AddressList> __Method_GetBPAddresses = new aelf::Method<global::Google.Protobuf.WellKnownTypes.Empty, global::TomorrowDAO.Contracts.Vote.AddressList>(
        aelf::MethodType.View,
        __ServiceName,
        "GetBPAddresses",
        __Marshaller_google_protobuf_Empty,
        __Marshaller_AddressList);

    #endregion

    #region Descriptors
    public static global::Google.Protobuf.Reflection.ServiceDescriptor Descriptor
    {
      get { return global::TomorrowDAO.Contracts.Vote.VoteContractReflection.Descriptor.Services[0]; }
    }

    public static global::System.Collections.Generic.IReadOnlyList<global::Google.Protobuf.Reflection.ServiceDescriptor> Descriptors
    {
      get
      {
        return new global::System.Collections.Generic.List<global::Google.Protobuf.Reflection.ServiceDescriptor>()
        {
          global::AElf.Standards.ACS12.Acs12Reflection.Descriptor.Services[0],
          global::TomorrowDAO.Contracts.Vote.VoteContractReflection.Descriptor.Services[0],
        };
      }
    }
    #endregion

    /// <summary>Base class for the contract of VoteContract</summary>
    // public abstract partial class VoteContractBase : AElf.Sdk.CSharp.CSharpSmartContract<TomorrowDAO.Contracts.Vote.VoteContractState>
    // {
    //   public virtual global::Google.Protobuf.WellKnownTypes.Empty Initialize(global::TomorrowDAO.Contracts.Vote.InitializeInput input)
    //   {
    //     throw new global::System.NotImplementedException();
    //   }
    //
    //   public virtual global::Google.Protobuf.WellKnownTypes.Empty CreateVoteScheme(global::TomorrowDAO.Contracts.Vote.CreateVoteSchemeInput input)
    //   {
    //     throw new global::System.NotImplementedException();
    //   }
    //
    //   public virtual global::Google.Protobuf.WellKnownTypes.Empty Register(global::TomorrowDAO.Contracts.Vote.VotingRegisterInput input)
    //   {
    //     throw new global::System.NotImplementedException();
    //   }
    //
    //   public virtual global::Google.Protobuf.WellKnownTypes.Empty Vote(global::TomorrowDAO.Contracts.Vote.VoteInput input)
    //   {
    //     throw new global::System.NotImplementedException();
    //   }
    //
    //   public virtual global::Google.Protobuf.WellKnownTypes.Empty Withdraw(global::TomorrowDAO.Contracts.Vote.WithdrawInput input)
    //   {
    //     throw new global::System.NotImplementedException();
    //   }
    //
    //   public virtual global::Google.Protobuf.WellKnownTypes.Empty SetEmergencyStatus(global::TomorrowDAO.Contracts.Vote.SetEmergencyStatusInput input)
    //   {
    //     throw new global::System.NotImplementedException();
    //   }
    //
    //   public virtual global::TomorrowDAO.Contracts.Vote.VoteScheme GetVoteScheme(global::AElf.Types.Hash input)
    //   {
    //     throw new global::System.NotImplementedException();
    //   }
    //
    //   public virtual global::TomorrowDAO.Contracts.Vote.VotingItem GetVotingItem(global::AElf.Types.Hash input)
    //   {
    //     throw new global::System.NotImplementedException();
    //   }
    //
    //   public virtual global::TomorrowDAO.Contracts.Vote.VotingResult GetVotingResult(global::AElf.Types.Hash input)
    //   {
    //     throw new global::System.NotImplementedException();
    //   }
    //
    //   public virtual global::TomorrowDAO.Contracts.Vote.VotingRecord GetVotingRecord(global::TomorrowDAO.Contracts.Vote.GetVotingRecordInput input)
    //   {
    //     throw new global::System.NotImplementedException();
    //   }
    //
    //   public virtual global::AElf.Types.Address GetVirtualAddress(global::TomorrowDAO.Contracts.Vote.GetVirtualAddressInput input)
    //   {
    //     throw new global::System.NotImplementedException();
    //   }
    //
    //   public virtual global::TomorrowDAO.Contracts.Vote.DaoRemainAmount GetDaoRemainAmount(global::TomorrowDAO.Contracts.Vote.GetDaoRemainAmountInput input)
    //   {
    //     throw new global::System.NotImplementedException();
    //   }
    //
    //   public virtual global::TomorrowDAO.Contracts.Vote.ProposalRemainAmount GetProposalRemainAmount(global::TomorrowDAO.Contracts.Vote.GetProposalRemainAmountInput input)
    //   {
    //     throw new global::System.NotImplementedException();
    //   }
    //
    //   public virtual global::TomorrowDAO.Contracts.Vote.AddressList GetBPAddresses(global::Google.Protobuf.WellKnownTypes.Empty input)
    //   {
    //     throw new global::System.NotImplementedException();
    //   }
    //
    // }
    //
    // public static aelf::ServerServiceDefinition BindService(VoteContractBase serviceImpl)
    // {
    //   return aelf::ServerServiceDefinition.CreateBuilder()
    //       .AddDescriptors(Descriptors)
    //       .AddMethod(__Method_Initialize, serviceImpl.Initialize)
    //       .AddMethod(__Method_CreateVoteScheme, serviceImpl.CreateVoteScheme)
    //       .AddMethod(__Method_Register, serviceImpl.Register)
    //       .AddMethod(__Method_Vote, serviceImpl.Vote)
    //       .AddMethod(__Method_Withdraw, serviceImpl.Withdraw)
    //       .AddMethod(__Method_SetEmergencyStatus, serviceImpl.SetEmergencyStatus)
    //       .AddMethod(__Method_GetVoteScheme, serviceImpl.GetVoteScheme)
    //       .AddMethod(__Method_GetVotingItem, serviceImpl.GetVotingItem)
    //       .AddMethod(__Method_GetVotingResult, serviceImpl.GetVotingResult)
    //       .AddMethod(__Method_GetVotingRecord, serviceImpl.GetVotingRecord)
    //       .AddMethod(__Method_GetVirtualAddress, serviceImpl.GetVirtualAddress)
    //       .AddMethod(__Method_GetDaoRemainAmount, serviceImpl.GetDaoRemainAmount)
    //       .AddMethod(__Method_GetProposalRemainAmount, serviceImpl.GetProposalRemainAmount)
    //       .AddMethod(__Method_GetBPAddresses, serviceImpl.GetBPAddresses).Build();
    // }

  }
}
#endregion
