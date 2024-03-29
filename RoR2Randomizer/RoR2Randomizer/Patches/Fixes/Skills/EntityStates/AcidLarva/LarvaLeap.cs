﻿#if !DISABLE_SKILL_RANDOMIZER
using RoR2;
using RoR2Randomizer.ChildTransformAdditions;
using RoR2Randomizer.Utility;

namespace RoR2Randomizer.Patches.Fixes.Skills.EntityStates.AcidLarva
{
    [PatchClass]
    public static class LarvaLeap
    {
        static void Apply()
        {
            On.EntityStates.AcidLarva.LarvaLeap.OnEnter += LarvaLeap_OnEnter;
            On.EntityStates.AcidLarva.LarvaLeap.OnExit += LarvaLeap_OnExit;
        }

        static void Cleanup()
        {
            On.EntityStates.AcidLarva.LarvaLeap.OnEnter -= LarvaLeap_OnEnter;
            On.EntityStates.AcidLarva.LarvaLeap.OnExit -= LarvaLeap_OnExit;
        }

        static void LarvaLeap_OnEnter(On.EntityStates.AcidLarva.LarvaLeap.orig_OnEnter orig, global::EntityStates.AcidLarva.LarvaLeap self)
        {
            Shared.TryAddTemporaryComponentIfMissing(self.characterBody, ref self.outer.commonComponents.characterMotor);

            CustomChildTransformManager.AutoAddChildTransform(self, self.spinEffectMuzzleString);

            orig(self);
        }

        static void LarvaLeap_OnExit(On.EntityStates.AcidLarva.LarvaLeap.orig_OnExit orig, global::EntityStates.AcidLarva.LarvaLeap self)
        {
            orig(self);

            TempComponentsTracker.TryRemoveTempComponent<CharacterMotor>(self.characterBody);
        }
    }
}
#endif
