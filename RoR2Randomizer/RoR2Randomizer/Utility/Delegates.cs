﻿namespace RoR2Randomizer.Utility
{
    public delegate bool TryConvertDelegate<T>(T input, out T output);

    public delegate bool TryConvertToNextValue<T>(ref T value);
}
