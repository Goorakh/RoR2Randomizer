﻿using RoR2;
using RoR2Randomizer.Extensions;
using RoR2Randomizer.Networking.BossRandomizer;
using RoR2Randomizer.Networking.Generic;
using System.Collections.Generic;
using UnityEngine;

namespace RoR2Randomizer.RandomizerControllers.Boss.BossReplacementInfo
{
    public abstract class BaseBossReplacement : CharacterReplacementInfo
    {
        protected abstract BossReplacementType replacementType { get; }

        protected virtual bool replaceBossDropEvenIfExisting => false;

        protected override bool isNetworked => true;

        protected virtual bool sendOriginalCharacterMasterIndex => false;

        protected override SetSubtitleMode subtitleOverrideMode => SetSubtitleMode.OnlyIfExistingIsNull;

        protected override void bodyResolved()
        {
            base.bodyResolved();

            CharacterBody originalBodyprefab = originalBodyPrefab;
            if (originalBodyprefab)
            {
                if (originalBodyprefab.TryGetComponent<DeathRewards>(out DeathRewards prefabDeathRewards) && prefabDeathRewards.bossDropTable)
                {
                    DeathRewards deathRewards = _body.gameObject.GetOrAddComponent<DeathRewards>();
                    if (!deathRewards.bossDropTable || deathRewards.bossDropTable == null || replaceBossDropEvenIfExisting)
                    {
                        deathRewards.bossDropTable = prefabDeathRewards.bossDropTable;
                    }
                }

                HealthComponent healthComponent = _body.healthComponent;
                if (healthComponent && originalBodyprefab.TryGetComponent<HealthComponent>(out HealthComponent prefabHealthComponent))
                {
#if DEBUG
                    float oldValue = healthComponent.globalDeathEventChanceCoefficient;
#endif

                    healthComponent.globalDeathEventChanceCoefficient = prefabHealthComponent.globalDeathEventChanceCoefficient;

#if DEBUG
                    if (oldValue != healthComponent.globalDeathEventChanceCoefficient)
                    {
                        Log.Debug($"overriding {nameof(HealthComponent.globalDeathEventChanceCoefficient)} for {replacementType} replacement {_master.name} ({oldValue} -> {healthComponent.globalDeathEventChanceCoefficient})");
                    }
#endif
                }
            }
        }

        protected override IEnumerable<NetworkMessageBase> getNetMessages()
        {
#if DEBUG
            Log.Debug($"Sending to clients");
#endif

            yield return new SyncBossReplacementCharacter(_master.gameObject, replacementType, sendOriginalCharacterMasterIndex ? originalMasterPrefab?.masterIndex : null);
        }
    }
}
