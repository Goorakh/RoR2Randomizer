﻿using RoR2;
using System;
using System.Collections.Generic;

namespace RoR2Randomizer.Utility
{
    public class RunSpecific<T> : IDisposable
    {
        public delegate bool TryGetNewValueDelegate(out T value);

        ulong? _callbackHandle;

        readonly TryGetNewValueDelegate _getNewValue;
        readonly T _defaultValue;

        public bool HasValue { get; private set; }

        T _value;
        public T Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;

                bool equal;
                if (value is IEquatable<T> equatable)
                {
                    equal = equatable.Equals(_defaultValue);
                }
                else if (_defaultValue is IEquatable<T> defaultEquatable)
                {
                    equal = defaultEquatable.Equals(value);
                }
                else
                {
                    equal = EqualityComparer<T>.Default.Equals(value, _defaultValue);
                }

                HasValue = !equal;
            }
        }

        public RunSpecific(int priority = 0, T defaultValue = default) : this(getNewValue: null, priority, defaultValue)
        {
        }

        public RunSpecific(Func<T> getValueFunc, int priority = 0, T defaultValue = default) : this((out T value) => { value = getValueFunc(); return true; }, priority, defaultValue)
        {
        }

        public RunSpecific(TryGetNewValueDelegate getNewValue, int priority = 0, T defaultValue = default)
        {
            _getNewValue = getNewValue;
            Value = (_defaultValue = defaultValue);
            HasValue = false;

            _callbackHandle = RunSpecificCallbacksManager.AddEntry(onRunStart, onRunEnd, priority);
        }

        ~RunSpecific()
        {
            if (_callbackHandle.HasValue)
            {
#if DEBUG
                Log.Warning($"{HarmonyLib.GeneralExtensions.FullDescription(GetType())} was not properly disposed");
#endif

                Dispose();
            }
        }

        void onRunStart(Run instance)
        {
            if (_getNewValue != null && !HasValue)
            {
                if (_getNewValue(out T value))
                {
                    Value = value;
                }
                else
                {
                    Value = _defaultValue;
                }
            }
        }

        void onRunEnd(Run instance)
        {
            Value = _defaultValue;
        }

        public void Dispose()
        {
            if (_callbackHandle.HasValue)
            {
                RunSpecificCallbacksManager.RemoveEntry(_callbackHandle.Value);
                _callbackHandle = null;
            }
        }

        public static implicit operator T(RunSpecific<T> runSpecific)
        {
            return runSpecific.Value;
        }
    }
}
