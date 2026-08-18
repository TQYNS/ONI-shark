using System;
using System.Collections.Generic;
using HarmonyLib;
using Klei.AI;
using UnityEngine;

namespace Shark
{
    public static class SharkTags
    {
        public static Tag SharkSpecies;

        private static bool speciesEnsured = false;

        public static void EnsureSharkSpecies()
        {
            if (speciesEnsured)
                return;
            speciesEnsured = true;
            SharkSpecies = TagManager.Create("SharkSpecies",
                ModStrings.CREATURES.FAMILY_PLURAL.SHARKSPECIES);
        }
    }

    // 监视喂鱼器异常
    [HarmonyPatch(typeof(FishFeeder), "OnStorageChange")]
    public static class TraceFishFeederOnStorageChange
    {
        static void Prefix(FishFeeder.Instance __instance, object data)
        {
            Debug.Log($"[Trace] OnStorageChange on FishFeeder {__instance?.gameObject?.GetInstanceID()}");
            Debug.Log(new System.Diagnostics.StackTrace().ToString());
        }
    }
    // 监视喂鱼器异常
    [HarmonyPatch(typeof(FishFeeder), "OnRefreshUserMenu")]
    public static class TraceFishFeederOnRefreshUserMenu
    {
        static void Prefix(FishFeeder.Instance __instance, object data)
        {
            Debug.Log($"[Trace] OnRefreshUserMenu on FishFeeder {__instance?.gameObject?.GetInstanceID()}");
            Debug.Log(new System.Diagnostics.StackTrace().ToString());
        }
    }



    [HarmonyPatch(typeof(Game), "OnPrefabInit")]
    public static class SharkNavGridPatch
    {
        static void Postfix()
        {
            //return;//已排除
            SharkNavGrid.EnsureRegistered();     
            SharkBabyNavGrid.EnsureRegistered();  
        }
    }

    public static class SharkFetchDiag_IsFetchable
    {
        static void Postfix(FetchableMonitor.Instance __instance, ref bool __result)
        {
            if (__instance == null || __instance.pickupable == null || __instance.pickupable.KPrefabID == null)
                return;
            var name = __instance.pickupable.KPrefabID.PrefabTag.Name;
            if (name != SharkBabyConfig.ID && name != "ParrotFish")
                return;
            var kp = __instance.pickupable.KPrefabID;
            /*
            Debug.Log($"[SharkFetch] IsFetchable name={name} result={__result} " +
                $"creature={kp.HasTag(GameTags.Creature)} deliverable={kp.HasTag(GameTags.Creatures.Deliverable)} " +
                $"storedPrivate={kp.HasTag(GameTags.StoredPrivate)} reservedByCreature={kp.HasTag(GameTags.Creatures.ReservedByCreature)} " +
                $"entombed={__instance.pickupable.IsEntombed} reachable={__instance.pickupable.IsReachable()}");
            */
        }
    }

    public static class SharkFetchDiag_IsFetchablePickup
    {
        static void Postfix(Pickupable pickup, FetchChore chore, ref bool __result)
        {
            if (pickup == null || pickup.KPrefabID == null)
                return;
            var name = pickup.KPrefabID.PrefabTag.Name;
            if (name != SharkBabyConfig.ID && name != "ParrotFish")
                return;
            var kp = pickup.KPrefabID;
            bool forbidden = chore?.forbiddenTags != null && chore.forbiddenTags.Length > 0
                && kp.HasAnyTags(chore.forbiddenTags);
            /*
            Debug.Log($"[SharkFetch] IsFetchablePickup name={name} result={__result} " +
                $"unreserved={pickup.UnreservedFetchAmount:F1} massPerUnit={pickup.PrimaryElement?.MassPerUnit:F1} " +
                $"choreAmount={chore?.originalAmount:F1} choreType={chore?.choreType?.Id} " +
                $"requiredTag={chore?.requiredTag} hasDeliverable={kp.HasTag(GameTags.Creatures.Deliverable)} " +
                $"tagsMatch={chore?.tags?.Contains(kp.PrefabTag)} forbidden={forbidden} " +
                $"markedForMove={kp.HasTag(GameTags.MarkedForMove)} " +
                $"choreAllowed={pickup.isChoreAllowedToPickup(chore?.choreType)} reachable={pickup.IsReachable()}");
            */
        }
    }

    public static class SharkReachDiag
    {
        static void Postfix(ReachabilityMonitor.Instance __instance)
        {
            var workable = __instance.master as Workable;
            var prefabId = workable != null ? workable.GetComponent<KPrefabID>() : null;
            if (prefabId == null)
                return;
            var name = prefabId.PrefabTag.Name;
            if (name != SharkBabyConfig.ID && name != "ParrotFish")
                return;

            int cell = workable.GetCell();
            var pos = Grid.CellToPos(cell, CellAlignment.Center, Grid.SceneLayer.Creatures);
            var offsets = workable.GetOffsets(cell);
            string offsetStr = "", reachStr = "";
            for (int i = 0; i < offsets.Length; i++)
            {
                int c = Grid.OffsetCell(cell, offsets[i]);
                offsetStr += $"({offsets[i].x},{offsets[i].y})";
                reachStr += MinionGroupProber.Get().IsReachable(c) ? "R" : "X";
                if (i < offsets.Length - 1) { offsetStr += " "; reachStr += " "; }
            }
            /*
            Debug.Log($"[SharkReach] {name} cell={cell} pos=({pos.x:F2},{pos.y:F2}) " +
                $"valid={Grid.IsValidCell(cell)} solid={Grid.Solid[cell]} liquid={Grid.Element[cell].IsLiquid} " +
                $"anchorReach={MinionGroupProber.Get().IsReachable(cell)} " +
                $"offsets=[{offsetStr}] offsetReach=[{reachStr}]");*/
        }
    }

    //可达性检查
    [HarmonyPatch(typeof(FetchableMonitor.Instance), "IsFetchable")]
    public static class SharkBabyFetchablePatch
    {
        static bool Prefix(FetchableMonitor.Instance __instance, ref bool __result)
        {
            //return true;
            if (__instance == null || __instance.pickupable == null)
                return true;
            if (__instance.pickupable.KPrefabID.PrefabTag.Name != SharkBabyConfig.ID)
                return true;   

            if (!__instance.pickupable.KPrefabID.HasTag(GameTags.Creatures.Bagged))
                return true;

            __result = !__instance.pickupable.IsEntombed
                && !__instance.pickupable.KPrefabID.HasTag(GameTags.StoredPrivate)
                && !__instance.pickupable.KPrefabID.HasTag(GameTags.Creatures.ReservedByCreature);
            return false;     
        }
    }

    public static class SharkBagDiag_Capture
    {
        static void Postfix(Capturable __instance)
        {
            var prefabId = __instance != null ? __instance.GetComponent<KPrefabID>() : null;
            if (prefabId == null || prefabId.PrefabTag.Name != SharkBabyConfig.ID)
                return;
            //Debug.Log($"[SharkBag] Capturable.OnCompleteWork (装袋 work 完成) sharkBaby");
        }
    }

    [HarmonyPatch(typeof(Storage), "Store", new Type[] { typeof(GameObject), typeof(bool), typeof(bool), typeof(bool), typeof(bool) })]
    public static class SharkBagDiag_Store
    {
        static void Postfix(Storage __instance, GameObject go)
        {
            KPrefabID kp = go != null ? go.GetComponent<KPrefabID>() : null;
            if (kp == null || (kp.PrefabTag.Name != SharkConfig.ID && kp.PrefabTag.Name != SharkBabyConfig.ID))
                return;
            KPrefabID owner = __instance.GetComponent<KPrefabID>();
            //Debug.Log("[SharkBag] Storage.Store " + kp.PrefabTag.Name + " 容器=" + (owner != null ? owner.PrefabTag.Name : "?"));
        }
    }

    [HarmonyPatch(typeof(Storage), "Drop", new Type[] { typeof(GameObject), typeof(bool) })]
    public static class SharkBagDiag_Drop
    {
        static void Postfix(Storage __instance, GameObject go)
        {
            KPrefabID kp = go != null ? go.GetComponent<KPrefabID>() : null;
            if (kp == null || (kp.PrefabTag.Name != SharkConfig.ID && kp.PrefabTag.Name != SharkBabyConfig.ID))
                return;
            KPrefabID owner = __instance.GetComponent<KPrefabID>();
            //Debug.Log("[SharkBag] Storage.Drop " + kp.PrefabTag.Name + " 容器=" + (owner != null ? owner.PrefabTag.Name : "?"));
        }
    }

    public static class SharkBagDiag_Wrangled
    {
        static void Postfix(Baggable __instance)
        {
            var prefabId = __instance != null ? __instance.GetComponent<KPrefabID>() : null;
            if (prefabId == null || (prefabId.PrefabTag.Name != SharkBabyConfig.ID && prefabId.PrefabTag.Name != SharkConfig.ID))
                return;
            int cell = Grid.PosToCell(__instance.transform.GetPosition());
            //Debug.Log("[SharkBag] SetWrangled " + prefabId.PrefabTag.Name + " 位置=" + cell + "(" + Grid.CellToXY(cell).x + "," + Grid.CellToXY(cell).y + ")");
        }
    }

    public static class SharkFetchExDiag_Begin
    {
        static void Postfix(FetchChore __instance)
        {
            var ft = __instance != null ? __instance.fetchTarget : null;
            if (ft == null || ft.KPrefabID == null) return;
            if (ft.KPrefabID.PrefabTag.Name != SharkBabyConfig.ID) return;
            //Debug.Log($"[SharkFetchEx] FetchChore.Begin target=SharkBaby choreType={__instance.choreType?.Id}");
        }
    }

    public static class SharkFetchExDiag_End
    {
        static void Postfix(FetchChore __instance, string reason)
        {
            var ft = __instance != null ? __instance.fetchTarget : null;
            if (ft == null || ft.KPrefabID == null) return;
            if (ft.KPrefabID.PrefabTag.Name != SharkBabyConfig.ID) return;
            //Debug.Log($"[SharkFetchEx] FetchChore.End reason={reason}");
        }
    }

    public static class SharkFetchExDiag_Register
    {
        static void Postfix(FetchableMonitor.Instance __instance)
        {
            var p = __instance != null ? __instance.pickupable : null;
            if (p == null || p.KPrefabID == null || p.KPrefabID.PrefabTag.Name != SharkBabyConfig.ID) return;
            //Debug.Log($"[SharkFetchEx] RegisterFetchable (幼崽注册进 fetchManager)");
        }
    }

    public static class SharkFetchExDiag_Unregister
    {
        static void Postfix(FetchableMonitor.Instance __instance)
        {
            var p = __instance != null ? __instance.pickupable : null;
            if (p == null || p.KPrefabID == null || p.KPrefabID.PrefabTag.Name != SharkBabyConfig.ID) return;
            //Debug.Log($"[SharkFetchEx] UnregisterFetchable (幼崽移出 fetchManager)");
        }
    }

    public static class SharkFetchExDiag_FindTarget
    {
        static void Postfix(Pickupable __result)
        {
            if (__result == null || __result.KPrefabID == null) return;
            if (__result.KPrefabID.PrefabTag.Name != SharkBabyConfig.ID) return;
            //Debug.Log($"[SharkFetchEx] FindFetchTarget 返回=SharkBaby（幼崽被选中为目标）");
        }
    }

    public static class SharkFAC_Shared
    {
        internal static bool SharkFetchIsTarget(FetchAreaChore.StatesInstance smi)
        {
            var ft = (smi != null && smi.sm != null) ? smi.sm.fetchTarget.Get(smi) : null;
            if (ft == null) return false;
            var kp = ft.GetComponent<KPrefabID>();
            return kp != null && kp.PrefabTag.Name == SharkBabyConfig.ID;
        }
    }

    public static class SharkFAC_Diag_SetupFetch
    {
        static void Postfix(FetchAreaChore.StatesInstance __instance)
        {
            if (__instance == null || __instance.master == null) return;
            if (!SharkFAC_Shared.SharkFetchIsTarget(__instance)) return;
            //Debug.Log($"[SharkFAC] SetupFetch (fetching.next → 准备抓取)");
        }
    }

    public static class SharkFAC_Diag_FetchFail
    {
        static void Postfix(FetchAreaChore.StatesInstance __instance)
        {
            if (__instance == null || __instance.master == null) return;
            if (!SharkFAC_Shared.SharkFetchIsTarget(__instance)) return;
            //Debug.Log($"[SharkFAC] FetchFail (抓取失败/移动失败)");
        }
    }

    public static class SharkFAC_Diag_FetchComplete
    {
        static void Postfix(FetchAreaChore.StatesInstance __instance)
        {
            if (__instance == null || __instance.master == null) return;
            if (!SharkFAC_Shared.SharkFetchIsTarget(__instance)) return;
            //Debug.Log($"[SharkFAC] FetchComplete (抓取成功——幼崽已被拿起)");
        }
    }

    public static class SharkFAC_Diag_SetupDelivery
    {
        static void Postfix(FetchAreaChore.StatesInstance __instance)
        {
            if (__instance == null || __instance.master == null) return;
            if (!SharkFAC_Shared.SharkFetchIsTarget(__instance)) return;
            //Debug.Log($"[SharkFAC] SetupDelivery (delivering.next → 准备投递，deliveries={__instance.deliveries.Count})");
        }
    }

    public static class SharkFAC_Diag_DeliverFail
    {
        static void Postfix(FetchAreaChore.StatesInstance __instance)
        {
            if (__instance == null || __instance.master == null) return;
            if (!SharkFAC_Shared.SharkFetchIsTarget(__instance)) return;
            //Debug.Log($"[SharkFAC] DeliverFail (投递失败)");
        }
    }

    public static class SharkFAC_Diag_DeliverComplete
    {
        static void Postfix(FetchAreaChore.StatesInstance __instance)
        {
            if (__instance == null || __instance.master == null) return;
            if (!SharkFAC_Shared.SharkFetchIsTarget(__instance)) return;
            //Debug.Log($"[SharkFAC] DeliverComplete (投递完成——幼崽已存入移取点)");
        }
    }

    [HarmonyPatch(typeof(KAnimControllerBase), "Play",
        new Type[] { typeof(HashedString), typeof(KAnim.PlayMode), typeof(float), typeof(float) })]
    public static class SharkIncubatorLoopPatch
    {
        static void Prefix(KAnimControllerBase __instance, ref KAnim.PlayMode mode, HashedString anim_name)
        {
            if (mode != KAnim.PlayMode.Once)
                return;
            if (anim_name != new HashedString("incubator_idle_loop"))
                return;
            KPrefabID prefabId = __instance.GetComponent<KPrefabID>();
            if (prefabId == null || prefabId.PrefabTag.Name != SharkBabyConfig.ID)
                return;

            mode = KAnim.PlayMode.Loop; 
        }
    }

    [HarmonyPatch(typeof(KAnimControllerBase), "Play",
        new Type[] { typeof(HashedString), typeof(KAnim.PlayMode), typeof(float), typeof(float) })]
    public static class SharkBabyExcitedLoopPatch
    {
        static void Prefix(KAnimControllerBase __instance, ref HashedString anim_name)
        {
            if (anim_name != new HashedString("excited_loop"))
                return;
            KPrefabID prefabId = __instance.GetComponent<KPrefabID>();
            if (prefabId == null || (prefabId.PrefabTag.Name != SharkConfig.ID && prefabId.PrefabTag.Name != SharkBabyConfig.ID))
                return;
            anim_name = new HashedString("idle_loop");
        }
    }

    public static class SharkDrinkCellPatch
    {
        public static void Postfix(object __instance, ref int __result)
        {
            if (__instance == null || __result == Grid.InvalidCell)
                return;
            StateMachine.Instance inst = __instance as StateMachine.Instance;
            if (inst == null)
                return;
            KPrefabID prefabId = inst.GetComponent<KPrefabID>();
            if (prefabId == null || prefabId.PrefabTag.Name != SharkConfig.ID)
                return;
            Navigator nav = inst.GetComponent<Navigator>();
            if (nav == null)
                return;
            int original = __result;
            int ox = Grid.CellToXY(original).x;
            int oy = Grid.CellToXY(original).y;
            if (nav.GetNavigationCost(original) != -1)
                return;
            int[] dys = new int[] { 0, -1, 1 };
            for (int i = 0; i < dys.Length; i++)
            {
                int dy = dys[i];
                for (int dx = -3; dx <= 3; dx++)
                {
                    int cand = Grid.OffsetCell(original, new CellOffset(dx, dy));
                    if (nav.GetNavigationCost(cand) != -1)
                    {
                        __result = cand;
                        //Debug.Log("[SharkDrink] GetDrinkCellOf target=" + original + "(" + ox + "," + oy + ") unreachable -> " + cand + "(" + Grid.CellToXY(cand).x + "," + Grid.CellToXY(cand).y + ")");
                        return;
                    }
                }
            }
            //Debug.Log("[SharkDrink] GetDrinkCellOf target=" + original + "(" + ox + "," + oy + ") unreachable NO_REACHABLE");
        }
    }

    [HarmonyPatch(typeof(DrinkMilkStates), "SetTarget")]
    public static class SharkDrinkDiag_SetTarget
    {
        static void Postfix(object smi)
        {
            if (smi == null)
                return;
            StateMachine.Instance inst = smi as StateMachine.Instance;
            if (inst == null)
                return;
            KPrefabID kp = inst.GetComponent<KPrefabID>();
            if (kp == null || kp.PrefabTag.Name != SharkConfig.ID)
                return;
            try
            {
                object feeder = typeof(DrinkMilkStates)
                    .GetMethod("GetTargetMilkFeeder", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                    .Invoke(null, new object[] { smi });
                if (feeder == null)
                {
                    Debug.Log("[SharkDrink] SetTarget feeder=null");
                    return;
                }
                var type = feeder.GetType();
                UnityEngine.Transform ft = (UnityEngine.Transform)type.GetProperty("transform").GetValue(feeder);
                int cell = Grid.PosToCell(ft.position);
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                sb.Append("cell=" + cell + "(" + Grid.CellToXY(cell).x + "," + Grid.CellToXY(cell).y + ")");
                foreach (string name in new string[] { "IsOperational", "IsStrawInstalled", "IsStrawOutsideLiquid", "IsStrawBlocked" })
                {
                    var p = type.GetProperty(name);
                    if (p != null)
                        sb.Append(" " + name + "=" + p.GetValue(feeder));
                }
                var shouldBeOn = typeof(MilkFeeder).GetMethod("ShouldBeOn", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (shouldBeOn != null)
                    sb.Append(" ShouldBeOn=" + shouldBeOn.Invoke(null, new object[] { feeder }));
                Debug.Log("[SharkDrink] SetTarget " + sb);
            }
            catch (System.Exception e)
            {
                Debug.Log("[SharkDrink] SetTarget err " + e.Message);
            }
        }
    }

    [HarmonyPatch(typeof(DrinkMilkStates), "GetAnimDrinkPre")]
    public static class SharkDrinkDiag_DrinkStart
    {
        static void Postfix(object smi)
        {
            if (smi == null)
                return;
            StateMachine.Instance inst = smi as StateMachine.Instance;
            if (inst == null)
                return;
            KPrefabID kp = inst.GetComponent<KPrefabID>();
            if (kp == null || kp.PrefabTag.Name != SharkConfig.ID)
                return;
            Facing facing = inst.GetComponent<Facing>();
            KAnimControllerBase kbac = inst.GetComponent<KAnimControllerBase>();
            int cell = Grid.PosToCell(inst.transform.position);
            string facingState = facing != null ? ("facingLeft=" + facing.GetFacing()) : "facing=null";
            string flipState = kbac != null ? ("FlipX=" + kbac.FlipX) : "kbac=null";
            //Debug.Log("[SharkDrink] drink_pre 开始 " + facingState + " " + flipState + " 鲨鱼格=" + cell + "(" + Grid.CellToXY(cell).x + "," + Grid.CellToXY(cell).y + ")");
        }
    }

    [HarmonyPatch(typeof(DrinkMilkStates), "FaceMilkFeeder")]
    public static class SharkDrinkFacingPatch
    {
        static void Postfix(object smi)
        {
            if (smi == null)
                return;
            StateMachine.Instance inst = smi as StateMachine.Instance;
            if (inst == null)
                return;
            KPrefabID kp = inst.GetComponent<KPrefabID>();
            if (kp == null || kp.PrefabTag.Name != SharkConfig.ID)
                return;
            try
            {
                object feeder = typeof(DrinkMilkStates)
                    .GetMethod("GetTargetMilkFeeder", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                    .Invoke(null, new object[] { smi });
                if (feeder == null)
                    return;
                UnityEngine.Transform ft = (UnityEngine.Transform)feeder.GetType().GetProperty("transform").GetValue(feeder);
                Facing facing = inst.GetComponent<Facing>();
                if (facing != null && ft != null)
                    facing.Face(ft.position.x);
            }
            catch (System.Exception)
            {
            }
        }
    }

    public static class SharkCaptureHelper
    {
        public static int FindRepairedCell(int baseCell, Navigator nav, bool isAdult)
        {
            if (baseCell == Grid.InvalidCell)
                return baseCell;
            // 基准格是建筑锚格(行3,固体)。水区仅 2 格高(行1-2),成年 2高×3宽只能锚在行1
            // (= 基准-2),dy∈{0,-1,+1} 永远摸不到 → 必须向下搜到 -3。
            int[] dys = new int[] { 0, -1, -2, -3, 1 };
            for (int i = 0; i < dys.Length; i++)
            {
                int dy = dys[i];
                for (int dx = -3; dx <= 3; dx++)
                {
                    int cand = Grid.OffsetCell(baseCell, new CellOffset(dx, dy));
                    if (isAdult ? !FitsAdult(cand) : !FitsBaby(cand))
                        continue;
                    if (nav != null && nav.PathGrid != null && nav.PathGrid.GetCost(cand) == -1)
                        continue;
                    return cand;
                }
            }
            return baseCell;
        }

        public static bool FitsAdult(int cell)
        {
            for (int i = -1; i <= 1; i++)
            {
                for (int j = 0; j <= 1; j++)
                {
                    if (!WaterOk(Grid.OffsetCell(cell, new CellOffset(i, j))))
                        return false;
                }
            }
            return true;
        }

        public static bool FitsBaby(int cell)
        {
            for (int i = 0; i <= 1; i++)
            {
                if (!WaterOk(Grid.OffsetCell(cell, new CellOffset(i, 0))))
                    return false;
            }
            return true;
        }

        private static bool WaterOk(int cell)
        {
            if (!Grid.IsValidCell(cell))
                return false;
            if (!Grid.Element[cell].IsLiquid)
                return false;
            if (!Grid.IsSubstantialLiquid(cell, 0.35f))
                return false;
            return true;
        }
    }

    //线程阻塞
    public static class SharkCaptureDiag
    {
        public static void SetShouldCreaturePostfix(object __instance, bool value)
        {
            StateMachine.Instance inst = __instance as StateMachine.Instance;
            if (inst == null)
                return;
            UnityEngine.Transform t = inst.transform;
            int cell = Grid.PosToCell(t.GetPosition());
            //Debug.Log("[SharkCapture] shouldCreatureGoGetCaptured=" + value + " 点格=(" + Grid.CellToXY(cell).x + "," + Grid.CellToXY(cell).y + ")");
        }

        private static float lastExecuteTime = -100f;
        private const float MIN_INTERVAL = 1f;

        public static void ShouldGoGetCapturedPostfix(object __instance, ref bool __result)
        {
            //轮询嫌疑
            /*
            float now = Time.unscaledTime;
            float elapsed = now - lastExecuteTime;

            // 如果未达到间隔，直接返回（不执行任何逻辑）
            if (elapsed < MIN_INTERVAL) return;
            lastExecuteTime = now;

            Debug.Log($"[SharkCapture] 执行时间={now:F2}s, 距上次={elapsed:F2}s, 鲨鱼ShouldGoGetCaptured={__result} ");
            */
            StateMachine.Instance inst = __instance as StateMachine.Instance;
            if (inst == null)
                return;
            KPrefabID kp = inst.GetComponent<KPrefabID>();
            if (kp == null || (kp.PrefabTag.Name != SharkConfig.ID && kp.PrefabTag.Name != SharkBabyConfig.ID))
                return;
            int cell = Grid.PosToCell(inst.transform.position);
            string targetInfo = "null";
            string running = "-";
            string should = "-";
            try
            {
                var t = __instance.GetType();
                var field = t.GetField("targetCapturePoint",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                object target = field != null ? field.GetValue(__instance) : null;
                if (target != null)
                {
                    var tt = target.GetType();
                    UnityEngine.Transform t2 = (UnityEngine.Transform)tt.GetProperty("transform").GetValue(target);
                    int pc = Grid.PosToCell(t2.GetPosition());
                    targetInfo = "(" + Grid.CellToXY(pc).x + "," + Grid.CellToXY(pc).y + ")";
                    var run = tt.GetMethod("IsRunning", new System.Type[0]);
                    if (run != null)
                        running = run.Invoke(target, null).ToString();
                    var sp = tt.GetProperty("shouldCreatureGoGetCaptured",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (sp != null)
                        should = sp.GetValue(target).ToString();
                }
            }
            catch (System.Exception)
            {
            }
            //阻塞嫌疑
            //Debug.Log("[SharkCapture] 鲨鱼ShouldGoGetCaptured=" + __result + " 格=(" + Grid.CellToXY(cell).x + "," + Grid.CellToXY(cell).y + ") 点=" + targetInfo + " IsRunning=" + running + " should=" + should);
        }
    }

    /// <summary>
    /// 绑定闸门:FixedCapturePoint.Instance.CanCapturableBeCapturedAtCapturePoint Postfix(Init.cs 动态挂载)。
    /// 原判定对鲨鱼恒失败的唯一原因是 ⑦ GetNavigationCost(判定格) != -1 —— 判定格(点正下方)在鲨鱼
    /// 自定义导航网格上无效(成年 3×2 顶行撞建筑本体 / 幼崽 2×1 贴墙)。此处对鲨鱼跳过 ⑦,
    /// 其余条件(②已绑其他点 / ③同腔室 / ④未装袋 / ⑤幼崽许可 / ⑥chore优先级 / ⑧容量)原样保留。
    /// </summary>
    public static class SharkCaptureBindPatch
    {
        //尝试Fix：轮询异常 Begin
        private static float _lastExecTime = -1f;
        private const float MIN_INTERVAL = 1f; // 每0.5秒检查一次
        //尝试Fix：轮询异常 End

        public static void Postfix(object capturable, object capture_point, CavityInfo capture_cavity_info, ref bool __result)
        {
            //尝试Fix：轮询异常 Begin
            float now = Time.unscaledTime;
            if (now - _lastExecTime < MIN_INTERVAL) return;
            _lastExecTime = now;
            //尝试Fix：轮询异常 End

            if (__result || capturable == null || capture_point == null)
                return;
            StateMachine.Instance inst = capturable as StateMachine.Instance;
            if (inst == null)
                return;
            KPrefabID kp = inst.GetComponent<KPrefabID>();
            if (kp == null || (kp.PrefabTag.Name != SharkConfig.ID && kp.PrefabTag.Name != SharkBabyConfig.ID))
                return;
            try
            {
                System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
                System.Type t = capturable.GetType();

                // ② 已被绑到其他运行中的点 → 拒绝(原判定)
                object target = t.GetField("targetCapturePoint", flags)?.GetValue(capturable);
                if (target != null && !ReferenceEquals(target, capture_point))
                {
                    var isRunning = target.GetType().GetMethod("IsRunning", new System.Type[0]);
                    if (isRunning != null && (bool)isRunning.Invoke(target, null))
                        return;
                }

                // ③ 同腔室(原判定,用判定格 = 点正下方格,与点同腔,自然通过)
                if (capture_cavity_info == null)
                    return;
                int sharkCell = Grid.PosToCell(inst.transform.position);
                CavityInfo sharkCavity = Game.Instance.roomProber.GetCavityForCell(sharkCell);
                if (sharkCavity == null || !ReferenceEquals(sharkCavity, capture_cavity_info))
                    return;

                // ④ 未装袋(原判定)
                if (kp.HasTag(GameTags.Creatures.Bagged))
                    return;

                // ⑤ 幼崽许可(原判定)
                bool isBaby = (bool)t.GetField("isBaby", flags).GetValue(capturable);
                if (isBaby && !AllowBabies(capture_point))
                    return;

                // ⑥ chore 优先级(原判定)
                ChoreConsumer cc = t.GetField("ChoreConsumer", flags)?.GetValue(capturable) as ChoreConsumer;
                if (cc != null)
                {
                    var m = typeof(ChoreConsumer).GetMethod("IsChoreEqualOrAboveCurrentChorePriority")
                        .MakeGenericMethod(typeof(FixedCaptureStates));
                    if (!(bool)m.Invoke(cc, null))
                        return;
                }

                // ⑧ 容量(原判定:amountStored > userMaxCapacity)——AmountStored 统计点下方腔室内生物数,
                // userMaxCapacity 默认值未知,2 只鲨鱼时 2 > 默认上限 恒 false → 绑定全组失败。
                // 此处跳过容量约束(装袋后 Storage 仍有容量兜底),仅记录原始判定供分析。
                object def = GetDef(capture_point);
                if (def == null)
                    return;
                System.Delegate cap = def.GetType().GetField("isAmountStoredOverCapacity", flags)
                    ?.GetValue(def) as System.Delegate;
                bool capacityOk = cap == null || (bool)cap.DynamicInvoke(capture_point, capturable);
                if (!capacityOk) Debug.Log("[SharkCapture] 容量判定不通过(已放行)");

                __result = true;
                int pc = Grid.PosToCell(((StateMachine.Instance)capture_point).transform.position);
                //Debug.Log("[SharkCapture] 绑定放行(仅跳过导航成本判定) 鲨鱼格=(" + Grid.CellToXY(sharkCell).x + "," + Grid.CellToXY(sharkCell).y + ") 点格=(" + Grid.CellToXY(pc).x + "," + Grid.CellToXY(pc).y + ")");
            }
            catch (System.Exception e)
            {
                //Debug.Log("[SharkCapture] 绑定放行异常 " + e.Message);
            }
        }

        private static bool AllowBabies(object capture_point)
        {
            try
            {
                object def = GetDef(capture_point);
                if (def == null)
                    return false;
                return (bool)def.GetType().GetField("allowBabies",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .GetValue(def);
            }
            catch
            {
                return false;
            }
        }

        private static object GetDef(object capture_point)
        {
            System.Type t = capture_point.GetType();
            var f = t.GetField("def",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (f != null)
                return f.GetValue(capture_point);
            var p = t.GetProperty("def",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return p != null ? p.GetValue(capture_point) : null;
        }
    }

    /// <summary>
    /// 移动目标修复(Init.cs 动态挂载,不依赖 PatchAll):FixedCaptureStates.GetTargetCaptureCell Postfix。
    /// 原目标 = def.getTargetCapturePoint = CanReach(深度格) ? 深度格 : 建筑锚点格(固体)。
    /// 鲨鱼尺寸在自定义导航网格上判定格不可达 → 恒返回固体锚点格 → MoveTo 永远失败。
    /// 此处就近修复:水格适配(按实际尺寸) + 该鲨鱼导航网格真实有效(PathGrid.GetCost != -1,绕过补丁的原生检查)。
    /// </summary>
    public static class SharkCaptureMovePatch
    {
        public static void Postfix(object smi, ref int __result)
        {
            if (smi == null || __result == Grid.InvalidCell)
                return;
            StateMachine.Instance inst = smi as StateMachine.Instance;
            if (inst == null)
                return;
            KPrefabID kp = inst.GetComponent<KPrefabID>();
            if (kp == null || (kp.PrefabTag.Name != SharkConfig.ID && kp.PrefabTag.Name != SharkBabyConfig.ID))
                return;
            bool isAdult = kp.PrefabTag.Name == SharkConfig.ID;
            Navigator nav = inst.GetComponent<Navigator>();
            if (nav == null)
                return;
            int original = __result;
            int repaired = SharkCaptureHelper.FindRepairedCell(original, nav, isAdult);
            int pos = Grid.PosToCell(inst.transform.position);
            //Debug.Log("[SharkCapture] move target=" + original + "(" + Grid.CellToXY(original).x + "," + Grid.CellToXY(original).y + ") -> " + repaired + "(" + Grid.CellToXY(repaired).x + "," + Grid.CellToXY(repaired).y + ") 鲨鱼格=(" + Grid.CellToXY(pos).x + "," + Grid.CellToXY(pos).y + ")");
            __result = repaired;
        }
    }

    public static class SharkNavCostDiag
    {
        static void Postfix(Navigator __instance, IApproachable approachable, ref int __result)
        {
            var workable = approachable as Workable;
            if (workable == null) return;
            var kp = workable.GetComponent<KPrefabID>();
            if (kp == null) return;
            var name = kp.PrefabTag.Name;
            if (name != SharkBabyConfig.ID && name != "ParrotFish") return;

            int cell = approachable.GetCell();
            var offsets = approachable.GetOffsets();
            string s = "";
            for (int i = 0; i < offsets.Length; i++)
            {
                int c = Grid.OffsetCell(cell, offsets[i]);
                int cost = __instance.GetNavigationCost(c);
                s += (cost == -1 ? "X" : cost.ToString());
                if (i < offsets.Length - 1) s += ",";
            }
            Debug.Log($"[SharkNavCost] {name} result={__result} cell={cell} " +
                $"solid={Grid.Solid[cell]} liquid={Grid.Element[cell].IsLiquid} " +
                $"offsetsN={offsets.Length} costs=[{s}]");
        }
    }

    [HarmonyPatch(typeof(Navigator), "GetNavigationCost",
        new Type[] { typeof(IApproachable) })]
    public static class SharkBabyNavCostPatch
    {
        static void Postfix(Navigator __instance, IApproachable approachable, ref int __result)
        {
            //return;//已排除
            if (__result != -1) return;  
            var workable = approachable as Workable;
            if (workable == null) return;
            var kp = workable.GetComponent<KPrefabID>();
            if (kp == null || kp.PrefabTag.Name != SharkBabyConfig.ID) return;
            __result = 1;   
            //Debug.Log($"[SharkNavCost] 幼崽分配检查放宽：-1 → 1");
        }
    }

    public static class SharkCDP_Diag
    {
        static void Postfix(CreatureDeliveryPoint __instance)
        {
            var capacity = __instance.critterCapacity;
            var smi = __instance.smi;
            string tags = "";
            var tf = __instance.GetComponent<TreeFilterable>();
            if (tf != null) tags = string.Join(",", tf.GetTags());
            Debug.Log($"[SharkCDP] Rebalance logic={__instance.LogicEnabled()} " +
                $"operational={smi?.IsOperational} strawBlocked={smi?.IsStrawBlocked} " +
                $"strawNoLiquid={smi?.IsStrawOutsideLiquid} stored={capacity?.storedCreatureCount} " +
                $"limit={capacity?.creatureLimit} tags=[{tags}]");
        }
    }

    public static class SharkWorldgenShared
    {
        internal static readonly ProcGen.Mob SharkMob = MakeSharkMob();

        private static bool logged1 = false;

        private static ProcGen.Mob MakeSharkMob()
        {
            var mob = new ProcGen.Mob(ProcGen.Mob.Location.Liquid);
            typeof(ProcGen.Mob).GetProperty("prefabName").SetValue(mob, "Shark");
            typeof(ProcGen.Mob).GetProperty("width").SetValue(mob, 3);
            typeof(ProcGen.Mob).GetProperty("height").SetValue(mob, 2);
            typeof(ProcGen.SampleDescriber).GetProperty("selectMethod").SetValue(mob,
                ProcGen.SampleDescriber.PointSelectionMethod.Centroid);
            typeof(ProcGen.SampleDescriber).GetProperty("density").SetValue(mob,
                new ProcGen.MinMax(0.03f, 0.04f));  
            return mob;
        }

        internal static void LogOnce1(string msg)
        {
            if (logged1) return;
            logged1 = true;
            Debug.Log(msg);
        }
    }

    [HarmonyPatch(typeof(ProcGen.WorldGenSettings), "HasMob")]
    public static class SharkWorldgenHasMobPatch
    {
        static void Postfix(string id, ref bool __result)
        {
            if (id == "Shark")
            {
                __result = true;
                SharkWorldgenShared.LogOnce1("[SharkWG-1] HasMob('Shark') 返回 true（mob 查表注入生效）");
            }
        }
    }

    [HarmonyPatch(typeof(ProcGen.WorldGenSettings), "GetMob")]
    public static class SharkWorldgenGetMobPatch
    {
        static void Postfix(string id, ref ProcGen.Mob __result)
        {
            if (id == "Shark" && __result == null)
            {
                __result = SharkWorldgenShared.SharkMob;
                SharkWorldgenShared.LogOnce1("[SharkWG-1] GetMob('Shark') 返回注入 mob（Liquid 3×2 density 0.03~0.04）");
            }
        }
    }

    [HarmonyPatch(typeof(ProcGenGame.MobSpawning), "PlaceBiomeAmbientMobs")]
    public static class SharkWorldgenPatch
    {
        static SharkWorldgenPatch()
        {
            Debug.Log("[SharkWG-0] SharkWorldgenPatch 已装配（PatchAll 生效）");
        }

        static void Prefix(ProcGenGame.TerrainCell tc)
        {
            var cell = tc?.node;
            if (cell == null) return;
            string subworld = cell.GetSubworld();
            if (subworld == null) return;
            bool isOceanEco = subworld.IndexOf("ocean", StringComparison.OrdinalIgnoreCase) >= 0
                           || subworld.IndexOf("reef", StringComparison.OrdinalIgnoreCase) >= 0;
            if (!isOceanEco) return;
            var tags = cell.biomeSpecificTags;
            if (tags != null && !tags.Contains(new Tag("Shark")))
            {
                tags.Add(new Tag("Shark"));
                //Debug.Log($"[SharkWG-2] node={cell.NodeId} subworld={subworld} biomeSpecificTags 已加 Shark，当前=[{TagSetToString(tags)}]");
            }
            else if (tags != null && tags.Contains(new Tag("Shark")))
            {
                //Debug.Log($"[SharkWG-3] node={cell.NodeId} subworld={subworld} biomeSpecificTags 含 Shark，进入放置评估");
            }
        }

        static void Postfix(ProcGenGame.TerrainCell tc, System.Collections.Generic.Dictionary<int, string> __result)
        {
            if (__result == null)
                return;
            int sharkCount = 0;
            foreach (var kv in __result)
                if (kv.Value == "Shark") sharkCount++;
            //if (sharkCount > 0)Debug.Log($"[SharkWG-4] node={tc?.node?.NodeId} 放置结果含 Shark × {sharkCount}（共 {__result.Count} 个 mob 放置点）");
        }

        private static string TagSetToString(global::TagSet set)
        {
            if (set == null) return "(null)";
            var sb = new System.Text.StringBuilder();
            foreach (var t in set)
            {
                if (sb.Length > 0) sb.Append(",");
                sb.Append(t.ToString());
            }
            return sb.ToString();
        }
    }

    public static class SharkWorldgenSpawnDiag
    {
        static void Postfix(ref Tag __result)
        {
            //if (__result == "Shark")Debug.Log("[SharkWG-5] WorldGenSpawner 运行时实例化 Shark（prefab 可用，世界生成结果落地）");
        }
    }

    public static class SharkPoopPatch
    {
        public static bool Prefix(object __instance)
        {
            var t = __instance.GetType();
            var owner = t.GetField("owner",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.GetValue(__instance) as GameObject;
            if (owner == null) return true;
            var kp = owner.GetComponent<KPrefabID>();
            if (kp == null || (kp.PrefabTag.Name != SharkConfig.ID && kp.PrefabTag.Name != SharkBabyConfig.ID))
                return true;  

            var diet = t.GetProperty("diet",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                ?.GetValue(__instance) as Diet;
            var entries = t.GetField("caloriesConsumed",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.GetValue(__instance) as System.Collections.IList;
            if (diet == null || entries == null) return true;

            float produced = 0f;
            bool isLivePrey = false;
            foreach (var e in entries)
            {
                if (e == null) continue;
                var et = e.GetType();
                float calories = (float)et.GetField("calories",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).GetValue(e);
                if (calories <= 0f) continue;
                var tag = (Tag)et.GetField("tag",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).GetValue(e);
                var info = diet.GetDietInfo(tag);
                if (info == null) continue;
                produced += info.ConvertConsumptionMassToProducedMass(
                    info.ConvertCaloriesToConsumptionMass(calories));
                if (info.producedElement == new Tag("Coquina"))  
                    isLivePrey = true;  
            }
            if (produced <= 0f) return true;  

            float limeKg = produced * SharkTuning.POOP_LIME_RATIO;
            float coquinaKg = produced * SharkTuning.POOP_COQUINA_RATIO;
            int cell = Grid.PosToCell(owner.transform.GetPosition());
            float temperature = owner.GetComponent<PrimaryElement>().Temperature;
            var lime = ElementLoader.GetElement(new Tag("Lime"));      
            var coquina = ElementLoader.GetElement(new Tag("Coquina"));
            if (lime != null && limeKg > 0f)
                lime.substance.SpawnResource(Grid.CellToPosCCC(cell, Grid.SceneLayer.Ore),
                    limeKg, temperature, byte.MaxValue, 0, false, false, false);
            if (coquina != null && coquinaKg > 0f)
                coquina.substance.SpawnResource(Grid.CellToPosCCC(cell, Grid.SceneLayer.Ore),
                    coquinaKg, temperature, byte.MaxValue, 0, false, false, false);

            entries.Clear();                      
            owner.Trigger(-1844238272, null);      

            //if (isLivePrey) Debug.Log($"[SharkDiet] 活体捕食 150% 确认：产出 {produced:F1}kg = Lime {limeKg:F1}kg + Coquina {coquinaKg:F1}kg");
            //else Debug.Log($"[SharkDiet] 生吃排便：产出 {produced:F1}kg = Lime {limeKg:F1}kg + Coquina {coquinaKg:F1}kg");
            return false;   
        }
    }

    [HarmonyPatch(typeof(Immigration), "ConfigureCarePackages")]
    public static class SharkCarePackagePatch
    {
        private const int CYCLE_GATE = 1;  

        static void Postfix(Immigration __instance)
        {
            var list = (System.Collections.Generic.List<CarePackageInfo>)typeof(Immigration)
                .GetField("carePackages",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.GetValue(__instance);
            if (list == null) return;

            list.Add(new CarePackageInfo(SharkBabyConfig.ID, 1f,
                () => GameClock.Instance.GetCycle() >= CYCLE_GATE));
            list.Add(new CarePackageInfo(SharkConfig.EGG_ID, 2f,
                () => GameClock.Instance.GetCycle() >= CYCLE_GATE));
            //Debug.Log($"[SharkPrint] 打印仓补给已注入：鲨鱼幼崽×1、鲨鱼蛋×2（周期门槛 {CYCLE_GATE}）");
        }
    }


    [HarmonyPatch(typeof(CodexCache), "CodexCacheInit")]
    public static class SharkCodexEntryPatch
    {
        static void Postfix()
        {
            if (CodexCache.entries == null || CodexCache.entries.ContainsKey("SHARK"))
                return;

            var containers = new List<ContentContainer>();

            containers.Add(new ContentContainer(new List<ICodexWidget>
            {
                new CodexText(Strings.Get("Shark.ModStrings.CREATURES.SPECIES.SHARK.DESC"),
                    CodexTextStyle.Body, null),
            }, ContentContainer.ContentLayout.Vertical));

            var img = new CodexImage
            {
                batchedAnimPrefabSourceID = SharkConfig.ID,
                preferredWidth = 128,
                preferredHeight = 128,
            };
            containers.Add(new ContentContainer(new List<ICodexWidget> { img },
                ContentContainer.ContentLayout.Vertical));

            GameObject prefab = Assets.GetPrefab(SharkConfig.ID);
            if (prefab != null)
            {
                var dietWidgets = new List<ICodexWidget>();

                float perSecondKcal = SharkTuning.STANDARD_CALORIES_PER_CYCLE / 600f; 
                float limePerSecond = (SharkTuning.POOP_MASS_KG / 600f) * SharkTuning.POOP_LIME_RATIO;  
                float coquinaPerSecond = (SharkTuning.POOP_MASS_KG / 600f) * SharkTuning.POOP_COQUINA_RATIO; 
                string[] preyIds = { "Pacu", "PacuCleaner", "PacuTropical", "ParrotFish", "PufferFish", "PrehistoricPacu" };
                foreach (string preyId in preyIds)
                {
                    var preyTag = new Tag(preyId);
                    GameObject preyPrefab = Assets.GetPrefab(preyTag);
                    float preyMass = preyPrefab != null ? preyPrefab.GetComponent<PrimaryElement>().Mass : 0f;
                    float kcalPerPrey = preyMass * SharkTuning.CALORIES_PER_KG_OF_PREY; 
                    float preyPerSecond = kcalPerPrey > 0f ? perSecondKcal / kcalPerPrey : 1f;
                    dietWidgets.Add(new CodexConversionPanel(
                        TagManager.GetProperName(preyTag, false),
                        new ElementUsage[] { new ElementUsage(preyTag, preyPerSecond, true,
                            GameUtil.GetFormattedPreyConsumptionValuePerCycle) },
                        new ElementUsage[]
                        {
                            new ElementUsage(SimHashes.Lime.CreateTag(), limePerSecond, true),
                            new ElementUsage(SimHashes.Coquina.CreateTag(), coquinaPerSecond, true),
                        }, prefab));
                }

                dietWidgets.Add(new CodexConversionPanel(
                    TagManager.GetProperName(new Tag("FishMeat"), false),
                    new ElementUsage[] { new ElementUsage(new Tag("FishMeat"), 1f / 600f, true) },
                    new ElementUsage[]
                    {
                        new ElementUsage(SimHashes.Lime.CreateTag(),
                            (SharkTuning.MEAT_CONVERSION_RATE * SharkTuning.POOP_LIME_RATIO) / 600f, true),
                        new ElementUsage(SimHashes.Coquina.CreateTag(),
                            (SharkTuning.MEAT_CONVERSION_RATE * SharkTuning.POOP_COQUINA_RATIO) / 600f, true),
                    }, prefab));

                dietWidgets.Add(new CodexConversionPanel(
                    TagManager.GetProperName(new Tag("CookedFish"), false),
                    new ElementUsage[] { new ElementUsage(new Tag("CookedFish"), 1f / 600f, true) },
                    new ElementUsage[]
                    {
                        new ElementUsage(SimHashes.Lime.CreateTag(),
                            (SharkTuning.MEAT_CONVERSION_RATE * SharkTuning.POOP_LIME_RATIO) / 600f, true),
                        new ElementUsage(SimHashes.Coquina.CreateTag(),
                            (SharkTuning.MEAT_CONVERSION_RATE * SharkTuning.POOP_COQUINA_RATIO) / 600f, true),
                    }, prefab));


                ContentContainer dietContainer = new ContentContainer(dietWidgets,
                    ContentContainer.ContentLayout.Vertical);
                containers.Add(new ContentContainer(new List<ICodexWidget>
                {
                    new CodexSpacer(),
                    new CodexCollapsibleHeader(global::STRINGS.CODEX.HEADERS.DIET, dietContainer),
                }, ContentContainer.ContentLayout.Vertical));
                containers.Add(dietContainer);
            }

            var entry = new CodexEntry("CREATURES",
                "Shark.ModStrings.CREATURES.SPECIES.SHARK.NAME", containers);
            entry.parentId = "CREATURES";
            entry.iconPrefabID = SharkConfig.ID;
            CodexCache.AddEntry("Shark", entry);
        }
    }


    [HarmonyPatch(typeof(CreatureCalorieMonitor.Def), "GetDescriptors")]
    public static class SharkDietDescriptorPatch
    {
        static void Postfix(CreatureCalorieMonitor.Def __instance, GameObject obj, ref List<Descriptor> __result)
        {
            var kp = obj != null ? obj.GetComponent<KPrefabID>() : null;
            if (kp == null || __instance.diet == null || __result == null) return;
            string id = kp.PrefabTag.Name;
            if (id != SharkConfig.ID && id != SharkBabyConfig.ID) return;

            string prefix = global::STRINGS.UI.BUILDINGEFFECTS.DIET_PRODUCED.text.Replace("{Items}", "");
            int idx = __result.FindIndex(d => d.text.StartsWith(prefix));
            if (idx < 0) return;
            var old = __result[idx];

            __result[idx] = new Descriptor(old.text,
                ModStrings.CREATURES.SPECIES.SHARK.DIET_PRODUCED_LIME + "\n" +
                ModStrings.CREATURES.SPECIES.SHARK.DIET_PRODUCED_COQUINA,
                Descriptor.DescriptorType.Effect, false);
        }
    }

    [HarmonyPatch(typeof(EatStates.Instance), "get_IsPredator")]
    public static class SharkPredatorPathPatch
    {
        static void Postfix(EatStates.Instance __instance, ref bool __result)
        {
            if (!__result) return;
            var selfKp = __instance != null ? __instance.GetComponent<KPrefabID>() : null;
            if (selfKp == null) return;
            string selfId = selfKp.PrefabTag.Name;
            if (selfId != SharkConfig.ID && selfId != SharkBabyConfig.ID) return;
            var prey = __instance.sm.target.Get(__instance);
            if (prey == null) return;
            var preyKp = prey.GetComponent<KPrefabID>();
            if (preyKp == null) return;
            if (!preyKp.HasTag(GameTags.Creature))
                __result = false;
        }
    }

    [HarmonyPatch(typeof(EatStates), "CheckHuntSuccess")]
    public static class SharkHuntSuccessPatch
    {
        private const float WILD_MIN_AGE = 0.2f;

        private const float RATE_BASE = 0.725f;
        private const float RATE_PER_T = 0.3f;

        static bool Prefix(EatStates.Instance smi, ref bool __result)
        {
            if (smi == null || smi.gameObject == null) return true;
            string predatorId = smi.gameObject.PrefabID().Name;
            if (predatorId != SharkConfig.ID && predatorId != SharkBabyConfig.ID)
                return true;

            GameObject prey = smi.sm.target.Get(smi);
            if (prey == null) { __result = false; return false; }


            if (!prey.HasTag(GameTags.Creature))
            {
                __result = true;
                return false;
            }

            WildnessMonitor.Instance predatorWild = smi.gameObject.GetSMI<WildnessMonitor.Instance>();
            WildnessMonitor.Instance preyWild = prey.GetSMI<WildnessMonitor.Instance>();
            bool isWildPath = predatorWild != null && predatorWild.IsWild()
                           && preyWild != null && preyWild.IsWild();

            AmountInstance age = Db.Get().Amounts.Age.Lookup(prey);
            float t = (age != null && age.GetMax() > 0f) ? age.value / age.GetMax() : 1f;

            bool passAgeGate = !isWildPath || t >= WILD_MIN_AGE;
            float rate = RATE_BASE + RATE_PER_T * t;
            __result = passAgeGate && UnityEngine.Random.Range(0f, 1f) < rate;
            /*
            Debug.Log($"[SharkHunt] predator={predatorId} prey={prey.PrefabID().Name} " +
                      $"age={age?.value.ToString("F1")}/{age?.GetMax().ToString("F1")} t={t.ToString("F3")} " +
                      $"wildPath={isWildPath} passAgeGate={passAgeGate} rate={rate.ToString("F3")} " +
                      $"roll={(__result ? "SUCCESS" : "FAIL")}");
            */
            return false;  
        }
    }
}
