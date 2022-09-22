﻿#if DEBUG
using RoR2;
using RoR2Randomizer.Patches.Debug;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RoR2Randomizer.Utility
{
    public class DebugButtonsManager : MonoBehaviour
    {
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Keypad0))
            {
                SpawnDisabler.Instance.ToggleSpawnsDisabled();
            }
            else if (Input.GetKeyDown(KeyCode.Keypad1))
            {
                Stage.instance.BeginAdvanceStage(Run.instance.nextStageScene);
            }
            else if (Input.GetKeyDown(KeyCode.Keypad5))
            {
                foreach (PlayerCharacterMasterController masterController in PlayerCharacterMasterController.instances)
                {
                    CharacterMaster playerMaster = masterController.master;

                    playerMaster.inventory.GiveRandomItems(100, false, false);
                    playerMaster.inventory.SetEquipmentIndex(RoR2Content.Equipment.Gateway.equipmentIndex); // Vase
                }
            }
            else if (Input.GetKeyDown(KeyCode.Keypad6))
            {
                Run.instance.SetRunStopwatch(Run.instance.GetRunStopwatch() + (20f * 60f));
                Run.instance.AdvanceStage(SceneCatalog.GetSceneDefFromSceneName("moon2"));
            }
        }
    }
}
#endif