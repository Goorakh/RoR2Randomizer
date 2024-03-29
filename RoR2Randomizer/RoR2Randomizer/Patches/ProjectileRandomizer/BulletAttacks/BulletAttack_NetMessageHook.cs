﻿using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2Randomizer.Patches.ProjectileParentChainTrackerPatches;
using RoR2Randomizer.Utility;
using UnityEngine.Networking;

namespace RoR2Randomizer.Patches.ProjectileRandomizer.BulletAttacks
{
    [PatchClass]
    static class BulletAttack_NetMessageHook
    {
        static void Apply()
        {
            IL.RoR2.BulletAttack.DefaultHitCallbackImplementation += BulletAttack_DefaultHitCallbackImplementation;

            IL.RoR2.BulletAttack.HandleBulletDamage += BulletAttack_HandleBulletDamage;
        }

        static void Cleanup()
        {
            IL.RoR2.BulletAttack.DefaultHitCallbackImplementation -= BulletAttack_DefaultHitCallbackImplementation;

            IL.RoR2.BulletAttack.HandleBulletDamage -= BulletAttack_HandleBulletDamage;
        }

        static void BulletAttack_DefaultHitCallbackImplementation(ILContext il)
        {
            const int MESSAGE_ID = 53;

            ILCursor c = new ILCursor(il);

            ILCursor[] foundCursors;
            if (c.TryFindNext(out foundCursors,
                              x => x.MatchLdcI4(MESSAGE_ID),
                              x => x.MatchCallOrCallvirt<NetworkWriter>(nameof(NetworkWriter.StartMessage)),
                              x => x.MatchCallOrCallvirt<NetworkWriter>(nameof(NetworkWriter.FinishMessage))))
            {
                ILCursor cursor = foundCursors[2];
                cursor.Emit(OpCodes.Dup);
                cursor.EmitDelegate(static (NetworkWriter messageWriter) =>
                {
                    ProjectileParentChainNode owner = ProjectileManager_InitializeProjectile_SetOwnerPatch.ResolveChainNodeForCurrentOwner();

                    bool hasOwner = owner != null;
                    messageWriter.Write(hasOwner);
                    if (hasOwner)
                    {
                        owner.Serialize(messageWriter);
                    }
                });
            }
            else
            {
                Log.Warning("unable to find hook location");
            }
        }

        static NetworkReader _currentReader;
        static void BulletAttack_HandleBulletDamage(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            // Just assumes there is only one ReadBoolean, not really a safe assumption, but it works for now, a more accurate hook can be made if it turns out to be a problem :P
            if (c.TryGotoNext(x => x.MatchCallOrCallvirt<NetworkReader>(nameof(NetworkReader.ReadBoolean))))
            {
                c.Emit(OpCodes.Dup);
                c.EmitDelegate(static (NetworkReader reader) =>
                {
                    _currentReader = reader;
                });

                c.Index++;
                c.EmitDelegate(static () =>
                {
                    if (_currentReader.ReadBoolean())
                    {
                        ProjectileParentChainNode parentChainNode = new ProjectileParentChainNode();
                        parentChainNode.Deserialize(_currentReader);

                        ProjectileManager_InitializeProjectile_SetOwnerPatch.BulletOwnerNodeOfNextProjectile = parentChainNode;
                    }

                    _currentReader = null;
                });

                if (c.TryGotoNext(x => x.MatchRet()))
                {
                    c.EmitDelegate(static () =>
                    {
                        ProjectileManager_InitializeProjectile_SetOwnerPatch.BulletOwnerNodeOfNextProjectile = null;
                    });
                }
                else
                {
                    Log.Warning("unable to find ret");
                }
            }
            else
            {
                Log.Warning("unable to find hook location");
            }
        }
    }
}
