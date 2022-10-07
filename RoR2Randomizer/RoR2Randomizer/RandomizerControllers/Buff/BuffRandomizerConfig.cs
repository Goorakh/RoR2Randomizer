﻿using BepInEx.Configuration;
using RoR2Randomizer.Configuration;
using RoR2Randomizer.Configuration.ConfigValue;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoR2Randomizer.RandomizerControllers.Buff
{
    public sealed class BuffRandomizerConfig : BaseRandomizerConfig
    {
        public BoolConfigValue MixBuffsAndDebuffs;

        public BoolConfigValue ExcludeInvincibility;

#if DEBUG
        public readonly EnumConfigValue<DebugMode> BuffDebugMode;

        public readonly StringConfigValue ForcedBuffName;
#endif

        public BuffRandomizerConfig(ConfigFile file) : base("Status Effect", file)
        {
            MixBuffsAndDebuffs = new BoolConfigValue(getEntry("Mix buffs and debuffs", "If the randomizer should be able to turn a buff (positive effect) into a debuff (negative effect) and vice versa.", true));

            ExcludeInvincibility = new BoolConfigValue(getEntry("Blacklist Invincibility Effects", "Disables randomization (both from and to) of status effects that make the user invincible. Turning this off can potentially softlock a run due to a boss becoming invincible.", true));

#if DEBUG
            BuffDebugMode = new EnumConfigValue<DebugMode>(getEntry("Debug Mode", DebugMode.None));
            ForcedBuffName = new StringConfigValue(getEntry("Forced Buff Name", string.Empty));
#endif
        }
    }
}