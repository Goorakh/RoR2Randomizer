﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityModdingUtility;

namespace RoR2Randomizer.Utility
{
    public static class CoroutineUtils
    {
        public static IEnumerator AddTimeout(this IEnumerator baseRoutine, float timeout, CoroutineOut<TimeoutActionResult> result = null)
        {
            float timeStarted = Time.unscaledTime;

            while (baseRoutine.MoveNext())
            {
                yield return baseRoutine.Current;

                float elapsed = Time.unscaledTime - timeStarted;
                if (elapsed >= timeout)
                {
#if DEBUG
                    Log.Debug($"Routine {baseRoutine} timed out");
#endif

                    if (result != null)
                        result.Result = new TimeoutActionResult(elapsed, TimeoutActionResultState.TimedOut);

                    yield break;
                }
            }

            if (result != null)
                result.Result = new TimeoutActionResult(Time.unscaledTime - timeStarted, TimeoutActionResultState.Finished);
        }
    }
}