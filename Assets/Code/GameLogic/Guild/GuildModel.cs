// This file is part of Metaplay SDK which is released under the Metaplay SDK License.

using Metaplay.Core;
using Metaplay.Core.Guild;
using Metaplay.Core.Model;
using System.Runtime.Serialization;

namespace Game.Logic
{
    /// <summary>
    /// Game-specific guild server event listener interface. Add all game-specific callbacks here.
    /// </summary>
    public interface IGuildModelServerListener
    {
    }

    public class EmptyGuildModelServerListener : IGuildModelServerListener
    {
        public static readonly EmptyGuildModelServerListener Instance = new EmptyGuildModelServerListener();
    }

    /// <summary>
    /// Game-specific guild client event listener interface. Add all game-specific callbacks here.
    /// </summary>
    public interface IGuildModelClientListener
    {
    }

    public class EmptyGuildModelClientListener : IGuildModelClientListener
    {
        public static readonly EmptyGuildModelClientListener Instance = new EmptyGuildModelClientListener();
    }

    [MetaSerializableDerived(1)]
    public class GuildMember : GuildMemberBase
    {
        // example feature: Player can "poke" other members that increases the poke count. See GuildPokeMember action.
        // example feature: Player can sell their pokes and receive Gold. See GuildSellPokes.
        [MetaMember(101)] public int NumTimesPoked;

        // example feature.
        // Players can increase "Vanity" by bying it with Gold (or with some other method). See GuildBuyVanity.
        // After after reaching certain Vanity thresholds, player unlocks a reward they can redeem. See GuildClaimVanityRankReward.
        // For example:  10 Vanity -> 500 gold, 40 vanity -> 5 gems.
        [MetaMember(102)] public int NumVanityPoints;
        [MetaMember(103)] public int NumVanityRanksConsumed;

         // Example limit of 10 invites.
        public override int MaxNumInvites => 10;

        public GuildMember() { }
        public GuildMember(int memberInstanceId, GuildMemberRole role, EntityId playerId) : base(memberInstanceId, role, playerId)
        {
        }
    }

    public class IdlerGuildRequirementsValidator : GuildRequirementsValidator
    {
        public override int MinDisplayNameLength => 5;
        public override int MaxDisplayNameLength => 20;
        public override int MinDescriptionLength => 0;
        public override int MaxDescriptionLength => 200;

        public override bool ValidateDisplayName(string displayName)
        {
            if (!base.ValidateDisplayName(displayName))
                return false;

            // [todo] Add more validation steps here as necessary, each one
            // returning false if the name fails to pass

            return true;
        }

        public override bool ValidateDescription(string description)
        {
            if (!base.ValidateDescription(description))
                return false;

            // [todo] Add more validation steps here as necessary, each one
            // returning false if the description fails to pass

            return true;
        }

        public override bool ValidateGuildCreation(GuildCreationParamsBase baseArgs)
        {
            if (!base.ValidateGuildCreation(baseArgs))
                return false;

            // [todo] Add more validation steps here as necessary, each one
            // returning false if the params fails to pass.
            //GuildCreationParams args = (GuildCreationParams)baseArgs;

            return true;
        }
    }

    [MetaSerializableDerived(2)]
    [SupportedSchemaVersions(1, 3)]
    public class GuildModel : GuildModelBase<GuildModel, GuildMember>
    {
        public const int TicksPerSecond = 10;
        protected override int GetTicksPerSecond() => TicksPerSecond;

        public override int MaxNumMembers => 20;

        [IgnoreDataMember] public new SharedGameConfig      GameConfig      => GetGameConfig<SharedGameConfig>();
        [IgnoreDataMember] public IGuildModelServerListener ServerListener  { get; set; } = EmptyGuildModelServerListener.Instance;
        [IgnoreDataMember] public IGuildModelClientListener ClientListener  { get; set; } = EmptyGuildModelClientListener.Instance;

        #region GuildModelBase implementation

        public override IModelRuntimeData<IGuildModelBase> GetRuntimeData() => new GuildModelRuntimeData(this);

        public override void OnTick()
        {
        }

        public override void OnFastForwardTime(MetaDuration elapsedTime)
        {
        }

        public override GuildMemberPrivateStateBase GetMemberPrivateState(EntityId memberPlayerId)
        {
            // no extra private data, use the default
            // If there is need for custom private data, inherit the GuildMemberPrivateStateBase and add custom fields
            return new GuildMemberPrivateStateBase(memberPlayerId, this);
        }

        public override void AddMember(EntityId memberPlayerId, int memberInstanceId, GuildMemberRole role, GuildMemberPlayerDataBase playerDataBase)
        {
            GuildMemberPlayerData playerData = (GuildMemberPlayerData)playerDataBase;

            GuildMember newMember = new GuildMember(
                memberInstanceId:   memberInstanceId,
                role:               role,
                playerId:           memberPlayerId
                );
            playerData.ApplyOnMember(newMember, guildBase: this, GuildMemberPlayerDataUpdateKind.NewMember);
            Members.Add(memberPlayerId, newMember);
        }

        public override void RemoveMember(EntityId memberPlayerId)
        {
            Members.Remove(memberPlayerId);
        }

        public override bool HasPermissionToKickMember(EntityId kickerPlayerId, EntityId kickedPlayerId)
        {
            // Example rule, allow kicking those with lower roles.
            GuildMemberRole kickerRole = Members[kickerPlayerId].Role;
            return kickerRole < Members[kickedPlayerId].Role;
        }

        public override bool HasPermissionToChangeRoleTo(EntityId requesterPlayerId, EntityId targetingPlayerId, GuildMemberRole targetRole)
        {
            // In this example, guild members have 3 roles. Leader, MiddleTier, and LowTier.
            // The roles are linear, and each higher role is a superset of the ones below.

            // Each member can only touch roles of lower-tier roles, not peers
            // Note that the lower role _value_ is a higher role.
            GuildMemberRole requesterRole = Members[requesterPlayerId].Role;
            if (requesterRole >= Members[targetingPlayerId].Role)
                return false;

            // Each member can promote up to (and including) their own tier.
            // Note that the lower role _value_ is a higher role.
            if (requesterRole > targetRole)
                return false;

            return true;
        }

        public override bool HasPermissionToInvite(EntityId memberPlayerId, GuildInviteType inviteType)
        {
            // Example rule: allow always all types of invites, by any player. Except if guild is full.
            if (Members.Count >= MaxNumMembers)
                return false;
            return true;
        }

        public override MetaDictionary<EntityId, GuildMemberRole> ComputeRoleChangesForRoleEvent(GuildMemberRoleEvent roleEvent, EntityId subjectMemberId, GuildMemberRole subjectRole)
        {
            MetaDictionary<EntityId, GuildMemberRole> roleChanges = new MetaDictionary<EntityId, GuildMemberRole>();
            switch(roleEvent)
            {
                case GuildMemberRoleEvent.MemberAdd:
                {
                    // Example. First member becomes Leader, others become lowTiers
                    if (Members.Count == 0)
                        roleChanges[subjectMemberId] = GuildMemberRole.Leader;
                    else
                        roleChanges[subjectMemberId] = GuildMemberRole.LowTier;
                    break;
                }

                case GuildMemberRoleEvent.MemberRemove:
                {
                    // Example. If Leader leaves, the next highest level member becomes leader
                    // This calculation should be deterministic, so we wanted to randomize the next leader,
                    // we would need to use some guild state as seed. If we were not deterministic, the
                    // client could speculate wrong, and request would be (randomly) rejected.
                    // Note that this tolerates there being multiple Leaders, which should not happen if other
                    // example policies here work.

                    if (subjectRole == GuildMemberRole.Leader)
                    {
                        GuildMemberRole? nextHighestRole = null;
                        foreach(GuildMember member in Members.Values)
                        {
                            // highest role == lowest integer
                            if (!nextHighestRole.HasValue || nextHighestRole.Value > member.Role)
                                nextHighestRole = member.Role;
                        }
                        foreach((EntityId memberId, GuildMember member) in Members)
                        {
                            if (member.Role == nextHighestRole.Value)
                            {
                                roleChanges[memberId] = GuildMemberRole.Leader;
                                break;
                            }
                        }
                    }
                    break;
                }

                case GuildMemberRoleEvent.MemberEdit:
                {
                    // Example:
                    // * In case somebody becomes a leader, all other leaders become MidTiers
                    // * In case a leader becomes a non-leader, some other player on next highest (excluding ex-leader) rank
                    //   is promoted to leader. If there is no such other player, player is kept a Leader.

                    if (subjectRole == GuildMemberRole.Leader)
                    {
                        foreach((EntityId memberId, GuildMember member) in Members)
                        {
                            if (member.Role == GuildMemberRole.Leader)
                                roleChanges[memberId] = GuildMemberRole.MiddleTier;
                        }
                        roleChanges[subjectMemberId] = GuildMemberRole.Leader;
                    }
                    else if (Members[subjectMemberId].Role == GuildMemberRole.Leader)
                    {
                        bool foundLeaderSuccessor = false;
                        GuildMemberRole? nextHighestRole = null;
                        foreach((EntityId memberId, GuildMember member) in Members)
                        {
                            if (memberId == subjectMemberId)
                                continue;
                            // highest role == lowest integer
                            if (!nextHighestRole.HasValue || nextHighestRole.Value > member.Role)
                                nextHighestRole = member.Role;
                        }
                        foreach((EntityId memberId, GuildMember member) in Members)
                        {
                            if (memberId == subjectMemberId)
                                continue;
                            if (member.Role == nextHighestRole.Value)
                            {
                                roleChanges[memberId] = GuildMemberRole.Leader;
                                foundLeaderSuccessor = true;
                                break;
                            }
                        }

                        if (foundLeaderSuccessor)
                        {
                            // success, allow the leader to become unleader
                            roleChanges[subjectMemberId] = subjectRole;
                        }
                        else
                        {
                            // cound not find successor, won't change leader
                        }
                    }
                    else
                    {
                        // normal change
                        roleChanges[subjectMemberId] = subjectRole;
                    }
                    break;
                }
            }

            // finally filter out any no-op
            roleChanges.RemoveWhere(kv =>
            {
                // \note: In case of a add, the new player is not in the Members yet
                if (Members.TryGetValue(kv.Key, out GuildMember member))
                    return member.Role == kv.Value;
                return false;
            });

            return roleChanges;
        }

        #endregion

        #region Schema migrations

        [MigrationFromVersion(fromVersion: 1)]
        [MigrationFromVersion(fromVersion: 2)]
        void TriggerNameSearchUpdate()
        {
            // // Made obsolete by GuildModel.SearchVersion
            // IsNameSearchValid = false;
        }

        #endregion
    }

    [MetaSerializableDerived(1)]
    public sealed class GuildCreationParams : GuildCreationParamsBase
    {
        // no custom data, yet
    }

    [MetaSerializableDerived(1)]
    public sealed class GuildCreationRequestParams : GuildCreationRequestParamsBase
    {
        // have user-supplied display name and description as an example.
        [MetaMember(101)] public string     DisplayName     { get; set; }
        [MetaMember(102)] public string     Description     { get; set; }
    }

    [MetaSerializableDerived(1)]
    public sealed class GuildMemberPlayerData : GuildMemberPlayerDataBase
    {
        // Example: Player has Coolness index and that is copied to guild for others to see
        //  [MetaMember(101)] public int CoolnessIndex { get; private set; }

        public GuildMemberPlayerData() { }
        public GuildMemberPlayerData(string displayName) : base(displayName)
        {
        }

        public override bool IsUpToDate(GuildMemberBase memberBase)
        {
            if (!base.IsUpToDate(memberBase))
                return false;

            // Example: Check that if player's coolness index is up-to-date with the guild's data
            //  GuildMember member = (GuildMember)memberBase;
            //  if (member.CoolnessIndex != CoolnessIndex)
            //      return false;

            return true;
        }

        public override void ApplyOnMember(GuildMemberBase memberBase, IGuildModelBase guildBase, GuildMemberPlayerDataUpdateKind updateKind)
        {
            base.ApplyOnMember(memberBase, guildBase, updateKind);

            // Example: Copy player's coolness index to guild
            //  GuildMember member = (GuildMember)memberBase;
            //  member.CoolnessIndex = CoolnessIndex
        }
    }

    /// <summary>
    /// <inheritdoc cref="GuildInviterAvatarBase"/>
    /// </summary>
    [MetaSerializableDerived(1)]
    public sealed class GuildInviterAvatar : GuildInviterAvatarBase
    {
        // custom information in guild-invite about the inviter
        //  [MetaMember(101)] public int CoolnessIndex { get; private set; }
    }
}
