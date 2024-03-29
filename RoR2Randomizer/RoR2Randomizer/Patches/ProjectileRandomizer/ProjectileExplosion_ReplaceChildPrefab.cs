﻿using RoR2.Projectile;
using RoR2Randomizer.Patches.ProjectileParentChainTrackerPatches;
using RoR2Randomizer.RandomizerControllers.Projectile;
using UnityEngine;

namespace RoR2Randomizer.Patches.ProjectileRandomizer
{
    [PatchClass]
    static class ProjectileExplosion_ReplaceChildPrefab
    {
        static void Apply()
        {
            On.RoR2.Projectile.ProjectileExplosion.FireChild += ProjectileExplosion_FireChild;
        }

        static void Cleanup()
        {
            On.RoR2.Projectile.ProjectileExplosion.FireChild -= ProjectileExplosion_FireChild;
        }

        static void ProjectileExplosion_FireChild(On.RoR2.Projectile.ProjectileExplosion.orig_FireChild orig, ProjectileExplosion self)
        {
            ref GameObject projectilePrefab = ref self.childrenProjectilePrefab;

            ProjectileManager_InitializeProjectile_SetOwnerPatch.OwnerOfNextProjectile = self.gameObject;

            if (!ProjectileRandomizerController.TryReplaceProjectileInstantiateFire(ref projectilePrefab, out GameObject originalChildProjectilePrefab, self.transform.position, self.GetRandomDirectionForChild(), self.projectileDamage.damage * self.childrenDamageCoefficient, self.projectileDamage.force, self.projectileDamage.crit, new GenericFireProjectileArgs
            {
                Owner = self.projectileController.owner,
                Weapon = self.gameObject,
                DamageType = self.projectileDamage.damageType
            }))
            {
                return;
            }

            orig(self);

            if (originalChildProjectilePrefab)
            {
                projectilePrefab = originalChildProjectilePrefab;
            }
        }
    }
}
