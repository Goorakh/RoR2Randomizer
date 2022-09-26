﻿using R2API.Networking;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2Randomizer.Extensions;
using RoR2Randomizer.Networking.BossRandomizer;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityModdingUtility;

namespace RoR2Randomizer.RandomizerController.Boss.BossReplacementInfo
{
    public abstract class BaseBossReplacement : MonoBehaviour
    {
        protected CharacterMaster _master;
        protected CharacterBody _body;

        protected abstract BossReplacementType ReplacementType { get; }

        public void Initialize()
        {
            _master = GetComponent<CharacterMaster>();

            StartCoroutine(waitForBodyInitialized());

            if (NetworkServer.active)
            {
                initializeServer();
            }

            initializeClient();
        }

        IEnumerator waitForBodyInitialized()
        {
            if (!_body)
            {
                _body = _master.GetBody();
                if (!_body)
                {
                    while (_master && !_master.hasBody)
                    {
                        yield return 0;
                    }

                    if (!_master)
                        yield break;

                    _body = _master.GetBody();
                }
            }

            if (_body)
            {
                bodyResolved();
            }
        }

        protected virtual void bodyResolved()
        {
            if (_body.bodyIndex == BodyCatalog.FindBodyIndex("EquipmentDroneBody"))
            {
                Inventory inventory = _master.inventory;
                if (inventory && inventory.GetEquipmentIndex() == EquipmentIndex.None)
                {
                    EquipmentIndex equipment = BossRandomizerController.AvailableDroneEquipments.Get.GetRandomOrDefault(EquipmentIndex.None);
                    inventory.SetEquipmentIndex(equipment);

#if DEBUG
                    Log.Debug($"Gave {Language.GetString(EquipmentCatalog.GetEquipmentDef(equipment).nameToken)} to {Language.GetString(_body.baseNameToken)}");
#endif
                }
            }
            else if (_body.bodyIndex == BodyCatalog.FindBodyIndex("DroneCommanderBody")) // Col. Droneman
            {
                Inventory inventory = _master.inventory;
                if (inventory)
                {
                    const int NUM_DRONE_PARTS = 1;

                    Patches.Reverse.DroneWeaponsBehavior.SetNumDroneParts(inventory, NUM_DRONE_PARTS);

#if DEBUG
                    Log.Debug($"Gave {NUM_DRONE_PARTS} drone parts to {Language.GetString(_body.baseNameToken)}");
#endif
                }
            }
        }

        protected virtual void initializeClient()
        {
#if DEBUG
            Log.Debug($"{nameof(BaseBossReplacement)} {nameof(initializeClient)}");
#endif
        }

        protected virtual void initializeServer()
        {
#if DEBUG
            Log.Debug($"{nameof(BaseBossReplacement)} {nameof(initializeServer)}");
#endif

            new SyncBossReplacementCharacter(_master.gameObject, ReplacementType).Send(NetworkDestination.Clients);

#if DEBUG
            Log.Debug($"Sent {nameof(SyncBossReplacementCharacter)} to clients");
#endif
        }

        protected void setBodySubtitle(string subtitleToken)
        {
            if (_body && _body.subtitleNameToken != subtitleToken)
            {
                _body.subtitleNameToken = subtitleToken;

                // Update BossGroup
                if (_master.isBoss)
                {
                    resetBossGroupSubtitle();
                }
            }
        }

        void resetBossGroupSubtitle()
        {
            BossGroup[] bossGroups = GameObject.FindObjectsOfType<BossGroup>();
            foreach (BossGroup group in bossGroups)
            {
                for (int i = 0; i < group.bossMemoryCount; i++)
                {
                    if (group.bossMemories[i].cachedMaster == _master)
                    {
                        // Force a refresh of the boss subtitle
                        group.bestObservedSubtitle = string.Empty;
                        return;
                    }
                }
            }
        }
    }
}
