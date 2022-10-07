﻿using HG;
using Mono.Cecil;
using MonoMod.Cil;
using MonoMod.Utils;
using R2API;
using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.Extensions;
using RoR2Randomizer.Networking.CharacterReplacements;
using RoR2Randomizer.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityModdingUtility;
using static UnityEngine.UI.Image;

namespace RoR2Randomizer.RandomizerControllers
{
#if DEBUG
    public enum DebugMode : byte
    {
        None,
        Manual,
        Forced
    }
#endif

    public static class CharacterReplacements
    {
#if DEBUG
        public static DebugMode DebugMode => ConfigManager.Debug.CharacterDebugMode;
#endif

        public static readonly InitializeOnAccess<EquipmentIndex[]> AvailableDroneEquipments = new InitializeOnAccess<EquipmentIndex[]>(() =>
        {
            return EquipmentCatalog.equipmentDefs.Where(eq => eq && (eq.canDrop || eq.name == "BossHunterConsumed"))
                                                 .Select(e => e.equipmentIndex)
                                                 .ToArray();
        });

        static readonly InitializeOnAccess<int[]> _masterIndicesToRandomize = new InitializeOnAccess<int[]>(() =>
        {
            return MasterCatalog.masterPrefabs.Where(master =>
            {
                if (!master || !master.GetComponent<CharacterMaster>())
                    return false;

                switch (master.name)
                {
                    case "AncientWispMaster": // Does nothing
                    case "ArtifactShellMaster": // No model, does not attack, cannot be damaged
                    case "BrotherHauntMaster": // No model
                    case "ClaymanMaster": // No hitboxes
                    case "EngiBeamTurretMaster": // Seems to ignore the player
                    case "MinorConstructAttachableMaster": // Instantly dies
                    case "PlayerMaster": // Does not exist
                    case "RailgunnerMaster": // Does not exist
                    case "VoidRaidCrabJointMaster": // Balls
                    case "VoidRaidCrabMaster": // Beta voidling, half invisible
                        return false;
                }

                return true;
            }).Distinct().Select(go => (int)MasterCatalog.FindMasterIndex(go)).ToArray();
        });

        static readonly RunSpecific<bool> _hasReceivedMasterIndexReplacementsFromServer = new RunSpecific<bool>(1);
        static readonly RunSpecific<ReplacementDictionary<int>> _masterIndexReplacements = new RunSpecific<ReplacementDictionary<int>>((out ReplacementDictionary<int> result) =>
        {
            if (NetworkServer.active)
            {
                result = ReplacementDictionary<int>.CreateFrom(_masterIndicesToRandomize.Get);

                new SyncCharacterMasterReplacements(result).Send(NetworkDestination.Clients);

                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }, 1);

        static void onMasterReplacementsReceivedFromServer(ReplacementDictionary<int> masterIndexReplacements)
        {
#if DEBUG
            Log.Debug("Received master index replacements from server");
#endif

            _masterIndexReplacements.Value = masterIndexReplacements;
            _hasReceivedMasterIndexReplacementsFromServer.Value = true;
        }

        static readonly Dictionary<string, string> _characterNamesLanguageAdditions = new Dictionary<string, string>
        {
            { "ARCHWISP_BODY_NAME", "Arch Wisp" },

            { "BEETLE_CRYSTAL_BODY_NAME", "Crystal Beetle" },

            { "MAJORCONSTRUCT_BODY_NAME", "Major Construct" },
            { "MAJORCONSTRUCT_BODY_SUBTITLE", "Defense System" }
        };

        public static void Initialize()
        {
            LanguageAPI.Add(_characterNamesLanguageAdditions);

            SyncCharacterMasterReplacements.OnReceive += onMasterReplacementsReceivedFromServer;

#if DEBUG
            RoR2Application.onFixedUpdate += Update;
#endif
        }

        public static void Uninitialize()
        {
            _masterIndexReplacements.Dispose();
            _hasReceivedMasterIndexReplacementsFromServer.Dispose();

            SyncCharacterMasterReplacements.OnReceive -= onMasterReplacementsReceivedFromServer;

#if DEBUG
            RoR2Application.onFixedUpdate -= Update;
#endif
        }

#if DEBUG
        static int _forcedMasterIndex;
        static void Update()
        {
            if (DebugMode == DebugMode.Manual)
            {
                bool changed = false;
                if (Input.GetKeyDown(KeyCode.KeypadPlus))
                {
                    if (++_forcedMasterIndex >= _masterIndicesToRandomize.Get.Length)
                        _forcedMasterIndex = 0;

                    changed = true;
                }
                else if (Input.GetKeyDown(KeyCode.KeypadMinus))
                {
                    if (--_forcedMasterIndex < 0)
                        _forcedMasterIndex = _masterIndicesToRandomize.Get.Length - 1;

                    changed = true;
                }

                if (changed)
                {
                    Log.Debug($"Character master: {MasterCatalog.GetMasterPrefab((MasterCatalog.MasterIndex)_masterIndicesToRandomize.Get[_forcedMasterIndex])?.name} ({_forcedMasterIndex})");
                }
            }
        }
#endif

        public static GameObject GetReplacementMasterPrefab(string originalMasterName)
        {
            MasterCatalog.MasterIndex replacementIndex = GetReplacementForMasterIndex(MasterCatalog.FindMasterIndex(originalMasterName));
            if (replacementIndex.isValid)
            {
                GameObject replacement = MasterCatalog.GetMasterPrefab(replacementIndex);

#if DEBUG
                Log.Debug($"{nameof(CharacterReplacements)}: Replaced {originalMasterName} with {replacement}");
#endif

                return replacement;
            }
            else
            {
                return null;
            }
        }

        public static bool TryReplaceMasterPrefab(ref GameObject prefab)
        {
            GameObject replacement = GetReplacementMasterPrefab(prefab.name);
            if (replacement)
            {
                prefab = replacement;
                return true;
            }
            else
            {
                return false;
            }
        }

        public static MasterCatalog.MasterIndex GetReplacementForMasterIndex(MasterCatalog.MasterIndex original)
        {
            if (original.isValid && (NetworkServer.active || (NetworkClient.active && _hasReceivedMasterIndexReplacementsFromServer)))
            {
#if DEBUG
                if (NetworkServer.active)
                {
                    if (DebugMode == DebugMode.Manual)
                    {
                        int masterIndex = ArrayUtils.GetSafe(_masterIndicesToRandomize.Get, _forcedMasterIndex, -1);
                        if (masterIndex != -1)
                        {
                            return (MasterCatalog.MasterIndex)masterIndex;
                        }
                    }

                    if (DebugMode == DebugMode.Forced)
                    {
                        return MasterCatalog.FindMasterIndex(ConfigManager.Debug.ForcedMasterName);
                    }
                }
#endif

                if (_masterIndexReplacements.HasValue && _masterIndexReplacements.Value.TryGetReplacement((int)original, out int replacementIndex))
                {
                    return (MasterCatalog.MasterIndex)replacementIndex;
                }
            }

            return MasterCatalog.MasterIndex.none;
        }

        public static MasterCatalog.MasterIndex GetOriginalMasterIndex(MasterCatalog.MasterIndex replacement)
        {
            if (replacement.isValid && (NetworkServer.active || (NetworkClient.active && _hasReceivedMasterIndexReplacementsFromServer)))
            {
                if (_masterIndexReplacements.HasValue && _masterIndexReplacements.Value.TryGetOriginal((int)replacement, out int originalIndex))
                {
                    return (MasterCatalog.MasterIndex)originalIndex;
                }
            }

            return MasterCatalog.MasterIndex.none;
        }

        public static ILContext.Manipulator FixMasterIndexReferences(Func<bool> patchEnabled, params string[] names)
        {
            return il =>
            {
                ILCursor c = new ILCursor(il);

                MemberReference member = null;
                while (c.TryGotoNext(x => x.MatchGetMemberValue(out member)))
                {
                    c.Index++;

                    if (member != null && Array.IndexOf(names, member.Name) != -1)
                    {
                        TypeReference valueType = null;
                        if (member is MethodReference method)
                        {
                            valueType = method.ReturnType;
                        }
                        else if (member is FieldReference field)
                        {
                            valueType = field.FieldType;
                        }
                        else
                        {
                            Log.Warning($"{nameof(CharacterReplacements)}.{nameof(FixMasterIndexReferences)} unimplemented member type of {member.GetType().FullName}");
                        }

                        if (valueType != null)
                        {
                            MasterCatalog.MasterIndex replacefunc(MasterCatalog.MasterIndex original)
                            {
                                if (patchEnabled == null || patchEnabled())
                                {
                                    MasterCatalog.MasterIndex replacement = GetReplacementForMasterIndex(original);
                                    if (replacement.isValid)
                                    {
                                        return replacement;
                                    }
                                }

                                return original;
                            }

                            if (valueType.Is(typeof(MasterCatalog.MasterIndex)))
                            {
                                c.EmitDelegate(replacefunc);
                            }
                            else if (valueType.Is(typeof(int)))
                            {
                                c.EmitDelegate((int original) =>
                                {
                                    return (int)replacefunc((MasterCatalog.MasterIndex)original);
                                });
                            }
                            else
                            {
                                Log.Warning($"{nameof(CharacterReplacements)}.{nameof(FixMasterIndexReferences)} unimplemented value type of {valueType.FullName}");
                            }
                        }
                    }
                }
            };
        }
    }
}