﻿using System;

using CosmosResourceToken.Core.Model;
using Microsoft.Azure.Cosmos;
using static CosmosResourceToken.Core.Model.Constants;

namespace CosmosResourceTokenBroker
{
    internal static class Utility
    {
        internal static string ToPartitionKeyBy(this string userId, PermissionModeKind permissionMode) => permissionMode switch
        {
            PermissionModeKind.SharedRead => "shared",
            PermissionModeKind.UserReadWrite => $"user-{userId}",
            PermissionModeKind.UserRead => $"user-{userId}",
            _ => throw new ArgumentOutOfRangeException(nameof(permissionMode), permissionMode,
                "Unknown permission mode")
        };

        internal static string ToPermissionIdBy(this string userId, string scope) => $"{userId}{PermissionScopePrefix}{scope}";

        //internal static IPermissionScope ToPermissionScope(this PermissionProperties pp) =>
        //    KnownPermissionScopes?.FirstOrDefault(s => s?.Scope == pp.Id?.Split(PermissionScopePrefix)[1]);


        internal static PermissionMode ToCosmosPermissionMode(this PermissionModeKind permissionMode) => permissionMode switch
        {
            PermissionModeKind.UserReadWrite => PermissionMode.All,
            PermissionModeKind.UserRead => PermissionMode.Read,
            PermissionModeKind.SharedRead => PermissionMode.Read,
            _ => throw new ArgumentOutOfRangeException(nameof(permissionMode), permissionMode,
                "Unknown permission mode")
        };
    }
}
