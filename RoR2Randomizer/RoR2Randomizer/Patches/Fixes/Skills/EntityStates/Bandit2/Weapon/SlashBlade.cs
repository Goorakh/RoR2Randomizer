﻿#if !DISABLE_SKILL_RANDOMIZER
using RoR2Randomizer.ChildTransformAdditions;

namespace RoR2Randomizer.Patches.Fixes.Skills.EntityStates.Bandit2.Weapon
{
    [PatchClass]
    public static class SlashBlade
    {
        static void Apply()
        {
            On.EntityStates.Bandit2.Weapon.SlashBlade.OnEnter += SlashBlade_OnEnter;
        }

        static void Cleanup()
        {
            On.EntityStates.Bandit2.Weapon.SlashBlade.OnEnter -= SlashBlade_OnEnter;
        }

        static void SlashBlade_OnEnter(On.EntityStates.Bandit2.Weapon.SlashBlade.orig_OnEnter orig, global::EntityStates.Bandit2.Weapon.SlashBlade self)
        {
            CustomChildTransformManager.AutoAddChildTransform(self, "BladeMesh", CustomChildTransformManager.ChildFlags.ForceNewObject);

            orig(self);
        }
    }
}
#endif