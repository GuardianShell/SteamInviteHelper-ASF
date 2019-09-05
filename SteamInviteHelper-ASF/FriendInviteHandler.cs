﻿using ArchiSteamFarm;
using SteamKit2;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SteamInviteHelper_ASF
{
    class FriendInviteHandler : ClientMsgHandler
    {
        public async void processFriendRequest(SteamID SteamID, Bot bot)
        {
            SteamFriends steamFriends = Client.GetHandler<SteamFriends>();
            UserProfile userProfile = new UserProfile(SteamID.ConvertToUInt64(), bot);

            List<Action> actions = new List<Action>();

            actions.Add(processPrivateProfile(userProfile, bot));
            Logger.LogDebug("[ACTION PRIVATE PROFILE]: " + processPrivateProfile(userProfile, bot));

            actions.Add(await processSteamRepScammerAsync(userProfile, bot));
            Logger.LogDebug("[ACTION STAEMREP SCAMMER]: " + await processSteamRepScammerAsync(userProfile, bot));

            actions.Add(processSteamLevel(userProfile, bot));
            Logger.LogDebug("[ACTION STEAM LEVEL]: " + processSteamLevel(userProfile, bot));

            actions.Add(processVACBanned(userProfile, bot));
            Logger.LogDebug("[ACTION VAC BANNED]: " + processVACBanned(userProfile, bot));

            actions.Add(processGameBanned(userProfile, bot));
            Logger.LogDebug("[ACTION GAME BANNED]: " + processGameBanned(userProfile, bot));

            actions.Add(processDaysSinceLastBan(userProfile, bot));
            Logger.LogDebug("[ACTION DAYS SINCE LAST BAN]: " + processDaysSinceLastBan(userProfile, bot));

            actions.Add(processCommunityBanned(userProfile, bot));
            Logger.LogDebug("[ACTION COMMUNITY BANNED]: " + processCommunityBanned(userProfile, bot));

            actions.Add(processEconomyBanned(userProfile, bot));
            Logger.LogDebug("[ACTION ECONOMY BANNED]: " + processEconomyBanned(userProfile, bot));

            actions.Add(processProfileName(userProfile, bot));
            Logger.LogDebug("[ACTION PROFILE NAME]: " + processProfileName(userProfile, bot));

            List<string> actionpriority = Config.FriendInviteConfigs.GetOrAdd(bot, new Config(bot)).ActionPriority;

            foreach (string action_string in actionpriority)
            {
                Action action = new Action(action_string);
                if (actions.Contains(action))
                {
                    action = actions[actions.IndexOf(action)];

                    switch (action.action)
                    {
                        case "block":
                            await steamFriends.IgnoreFriend(SteamID);
                            break;
                        case "ignore":
                            steamFriends.RemoveFriend(SteamID);
                            break;
                        case "add":
                            steamFriends.AddFriend(SteamID);
                            break;
                        case "none":
                            break;
                    }

                    Logger.LogInfo("New pending invite from {0}", userProfile.personaName);
                    Logger.LogInfo("  ├─ SteamID: {0}", Convert.ToString(SteamID.ConvertToUInt64()));
                    Logger.LogInfo("  ├─ Profile url: {0}", userProfile.userProfileURL);
                    Logger.LogInfo("  └─ Action: {0} | Reason: {1}", action.action.ToUpper(), action.reason);
                }
            }
        }

        private static Action processPrivateProfile(UserProfile userProfile, Bot bot)
        {
            Config config = Config.FriendInviteConfigs.GetOrAdd(bot, new Config(bot));
            if (userProfile.communityVisibilityState == 1)
            {
                return new Action(config.PrivateProfile, "Private Profile");
            }
            else
            {
                return new Action("none");
            }
        }

        private static async Task<Action> processSteamRepScammerAsync(UserProfile userProfile, Bot bot)
        {
            if (await WebRequestsHelper.StreamRepIsScammer(userProfile.steamId64))
            {
                return new Action(Config.FriendInviteConfigs.GetOrAdd(bot, new Config(bot)).SteamRepScammer, "SteamRep scammer");
            }
            return new Action("none");
        }

        private static Action processSteamLevel(UserProfile userProfile, Bot bot)
        {
            Config config = Config.FriendInviteConfigs.GetOrAdd(bot, new Config(bot));
            string defaultAction = "none";

            foreach (ConfigItem item in config.SteamLevel)
            {
                int value = Convert.ToInt32(item.value);
                switch (item.condition)
                {
                    case "less_than":
                        if (userProfile.steamLevel < value)
                            return new Action(item.action, "Steam level < " + value);
                        break;
                    case "more_than":
                        if (userProfile.steamLevel > value)
                            return new Action(item.action, "Steam level > " + value);
                        break;
                    case "equal":
                        if (userProfile.steamLevel == value)
                            return new Action(item.action, "Steam level = " + value);
                        break;
                    case "default":
                        defaultAction = item.action;
                        break;
                }
            }
            return new Action(defaultAction);
        }

        private static Action processVACBanned(UserProfile userProfile, Bot bot)
        {
            Config config = Config.FriendInviteConfigs.GetOrAdd(bot, new Config(bot));
            string defaultAction = "none";

            foreach (ConfigItem item in config.VacBanned)
            {
                int value = Convert.ToInt32(item.value);
                switch (item.condition)
                {
                    case "less_than":
                        if (userProfile.numberOfVACBans < value)
                            return new Action(item.action, "Number of VAC bans < " + value);
                        break;
                    case "more_than":
                        if (userProfile.numberOfVACBans > value)
                            return new Action(item.action, "Number of VAC bans > " + value);
                        break;
                    case "equal":
                        if (userProfile.numberOfVACBans == value)
                            return new Action(item.action, "Number of VAC bans = " + value);
                        break;
                    case "default":
                        defaultAction = item.action;
                        break;
                }
            }
            return new Action(defaultAction);
        }

        private static Action processGameBanned(UserProfile userProfile, Bot bot)
        {
            Config config = Config.FriendInviteConfigs.GetOrAdd(bot, new Config(bot));
            string defaultAction = "none";

            foreach (ConfigItem item in config.GameBanned)
            {
                int value = Convert.ToInt32(item.value);
                switch (item.condition)
                {
                    case "less_than":
                        if (userProfile.numberOfGamebans < value)
                            return new Action(item.action, "Number of game bans < " + value);
                        break;
                    case "more_than":
                        if (userProfile.numberOfGamebans > value)
                            return new Action(item.action, "Number of game bans > " + value);
                        break;
                    case "equal":
                        if (userProfile.numberOfGamebans == value)
                            return new Action(item.action, "Number of game bans = " + value);
                        break;
                    case "default":
                        defaultAction = item.action;
                        break;
                }
            }
            return new Action(defaultAction);
        }

        private static Action processDaysSinceLastBan(UserProfile userProfile, Bot bot)
        {
            if (userProfile.vacBanned || userProfile.gameBanned)
            {
                Config config = Config.FriendInviteConfigs.GetOrAdd(bot, new Config(bot));
                string defaultAction = "none";

                foreach (ConfigItem item in config.DaysSinceLastBan)
                {
                    int value = Convert.ToInt32(item.value);
                    switch (item.condition)
                    {
                        case "less_than":
                            if (userProfile.daysSinceLastBan < value)
                                return new Action(item.action, "Days since last ban < " + value);
                            break;
                        case "more_than":
                            if (userProfile.daysSinceLastBan > value)
                                return new Action(item.action, "Days since last ban > " + value);
                            break;
                        case "equal":
                            if (userProfile.daysSinceLastBan == value)
                                return new Action(item.action, "Days since last ban = " + value);
                            break;
                        case "default":
                            defaultAction = item.action;
                            break;
                    }
                }
                return new Action(defaultAction);
            }
            else
            {
                return new Action("none");
            }
        }

        private static Action processCommunityBanned(UserProfile userProfile, Bot bot)
        {
            Config config = Config.FriendInviteConfigs.GetOrAdd(bot, new Config(bot));
            if (userProfile.communityBanned)
            {
                return new Action(config.CommunityBanned, "Community banned");
            }
            else
            {
                return new Action("none");
            }
        }

        private static Action processEconomyBanned(UserProfile userProfile, Bot bot)
        {
            Config config = Config.FriendInviteConfigs.GetOrAdd(bot, new Config(bot));
            if (!userProfile.economyBan.Equals("none"))
            {
                return new Action(config.EconomyBanned, "Economy banned");
            }
            else
            {
                return new Action("none");
            }
        }

        private static Action processProfileName(UserProfile userProfile, Bot bot)
        {
            Config config = Config.FriendInviteConfigs.GetOrAdd(bot, new Config(bot));
            string defaultAction = "none";

            foreach (ConfigItem item in config.ProfileName)
            {
                switch (item.condition)
                {
                    case "equal":
                        if (userProfile.personaName.Equals(item.value, StringComparison.OrdinalIgnoreCase))
                            return new Action(item.action, "Profile name equals " + item.value);
                        break;
                    case "contain":
                        if (userProfile.personaName.Contains(item.value, StringComparison.OrdinalIgnoreCase))
                            return new Action(item.action, "Profile name contains " + item.value);
                        break;
                    case "default":
                        defaultAction = item.action;
                        break;
                }
            }
            return new Action(defaultAction);
        }

        public override void HandleMsg(IPacketMsg packetMsg)
        {
        }
    }
}