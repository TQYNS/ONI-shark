using System.Linq;
using HarmonyLib;
using KMod;
using PeterHan.PLib.Core;
using PeterHan.PLib.Database;
using UnityEngine;

namespace Shark
{
    public class SharkMod : UserMod2
    {
        public override void OnLoad(Harmony harmony)
        {
            PUtil.InitLibrary(true);
            new PLocalization().Register(GetType().Assembly); 

            LocString.CreateLocStringKeys(typeof(ModStrings.CREATURES), "STRINGS.");

            base.OnLoad(harmony); 

            var stomachType = typeof(CreatureCalorieMonitor).GetNestedTypes(
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)
                .FirstOrDefault(t => t.Name == "Stomach");
            var poopMethod = stomachType?.GetMethods(
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                .FirstOrDefault(m => m.Name == "PoopInStorage");
            if (poopMethod != null)
            {
                harmony.Patch(poopMethod,
                    prefix: new HarmonyMethod(typeof(SharkPoopPatch).GetMethod("Prefix")));
                Debug.Log("[SharkDiet] SharkPoopPatch 动态挂载成功（Stomach.PoopInStorage Prefix）");
            }
            else
            {
                Debug.LogError($"[SharkDiet] SharkPoopPatch 挂载失败：stomachType={(stomachType != null ? stomachType.FullName : "NULL")} " +
                    $"poopMethod={(poopMethod != null ? poopMethod.Name : "NULL")} " +
                    $"nestedTypes={string.Join(",", typeof(CreatureCalorieMonitor).GetNestedTypes(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public).Select(t => t.Name))}");
            }

            var drinkMonitorType = typeof(DrinkMilkMonitor).GetNestedTypes(
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)
                .FirstOrDefault(t => t.Name == "Instance");
            var drinkCellMethod = drinkMonitorType?.GetMethod("GetDrinkCellOf",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (drinkCellMethod != null)
            {
                harmony.Patch(drinkCellMethod,
                    postfix: new HarmonyMethod(typeof(SharkDrinkCellPatch).GetMethod("Postfix")));
                Debug.Log("[SharkDrink] SharkDrinkCellPatch 动态挂载成功（DrinkMilkMonitor.Instance.GetDrinkCellOf Postfix）");
            }
            else
            {
                Debug.LogError($"[SharkDrink] SharkDrinkCellPatch 挂载失败：drinkMonitorType={(drinkMonitorType != null ? drinkMonitorType.FullName : "NULL")} " +
                    $"nestedTypes={string.Join(",", typeof(DrinkMilkMonitor).GetNestedTypes(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public).Select(t => t.Name))}");
            }

            var capturePointType = typeof(FixedCapturePoint).GetNestedTypes(
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)
                .FirstOrDefault(t => t.Name == "Instance");
            var bindMethod = capturePointType?.GetMethod("CanCapturableBeCapturedAtCapturePoint",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            if (bindMethod != null)
            {
                harmony.Patch(bindMethod,
                    postfix: new HarmonyMethod(typeof(SharkCaptureBindPatch).GetMethod("Postfix")));
                Debug.Log("[SharkCapture] CanCapturableBeCapturedAtCapturePoint 动态挂载成功（绑定闸门跳过导航成本判定）");
            }
            else
            {
                Debug.LogError($"[SharkCapture] CanCapturableBeCapturedAtCapturePoint 挂载失败：capturePointType={(capturePointType != null ? capturePointType.FullName : "NULL")} " +
                    $"nestedTypes={string.Join(",", typeof(FixedCapturePoint).GetNestedTypes(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public).Select(t => t.Name))}");
            }

            var moveMethod = typeof(FixedCaptureStates).GetMethod("GetTargetCaptureCell",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            if (moveMethod != null)
            {
                harmony.Patch(moveMethod,
                    postfix: new HarmonyMethod(typeof(SharkCaptureMovePatch).GetMethod("Postfix")));
                Debug.Log("[SharkCapture] GetTargetCaptureCell 动态挂载成功（移动目标就近修复）");
            }
            else
            {
                Debug.LogError("[SharkCapture] GetTargetCaptureCell 挂载失败");
            }

            var setShouldCreatureMethod = capturePointType?.GetProperty("shouldCreatureGoGetCaptured")?.GetSetMethod(true);
            if (setShouldCreatureMethod != null)
            {
                harmony.Patch(setShouldCreatureMethod,
                    postfix: new HarmonyMethod(typeof(SharkCaptureDiag).GetMethod("SetShouldCreaturePostfix")));
                Debug.Log("[SharkCapture] set_shouldCreatureGoGetCaptured 动态挂载成功");
            }
            else
            {
                Debug.LogError("[SharkCapture] set_shouldCreatureGoGetCaptured 挂载失败");
            }

            var monitorType = typeof(FixedCapturableMonitor).GetNestedTypes(
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public)
                .FirstOrDefault(t => t.Name == "Instance");
            var shouldGoGetCapturedMethod = monitorType?.GetMethod("ShouldGoGetCaptured",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (shouldGoGetCapturedMethod != null)
            {
                harmony.Patch(shouldGoGetCapturedMethod,
                    postfix: new HarmonyMethod(typeof(SharkCaptureDiag).GetMethod("ShouldGoGetCapturedPostfix")));
                Debug.Log("[SharkCapture] ShouldGoGetCaptured 动态挂载成功（FixedCapturableMonitor.Instance）");
            }
            else
            {
                Debug.LogError($"[SharkCapture] ShouldGoGetCaptured 挂载失败：monitorType={(monitorType != null ? monitorType.FullName : "NULL")} " +
                    $"nestedTypes={string.Join(",", typeof(FixedCapturableMonitor).GetNestedTypes(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public).Select(t => t.Name))}");
            }
        }
    }
}
