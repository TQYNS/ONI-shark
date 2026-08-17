using HarmonyLib;

namespace Shark
{
    public static class SharkNameHelper
    {
        internal static string GetDisplayName(string prefabId, bool withLink = false)
        {
            string key;
            switch (prefabId)
            {
                case SharkConfig.ID: key = "Shark.ModStrings.CREATURES.SPECIES.SHARK.NAME"; break;
                case SharkBabyConfig.ID: key = "Shark.ModStrings.CREATURES.SPECIES.SHARK.BABY_NAME"; break;
                case SharkConfig.EGG_ID: key = "Shark.ModStrings.CREATURES.SPECIES.SHARK.EGG_NAME"; break;
                default: return null;
            }
            string s = Strings.Get(key);
            if (s == null || s.StartsWith("MISSING")) return null;

            if (withLink && prefabId == SharkConfig.ID)
                return $"<link=\"SHARK\">{s}</link>";
            return s;
        }
    }

    [HarmonyPatch(typeof(KSelectable), "GetName")]
    public static class SharkDisplayNamePatch
    {
        static void Postfix(KSelectable __instance, ref string __result)
        {
            var kp = __instance != null ? __instance.GetComponent<KPrefabID>() : null;
            if (kp == null) return;
            string name = SharkNameHelper.GetDisplayName(kp.PrefabTag.Name);
            if (name != null) __result = name;
        }
    }

    [HarmonyPatch(typeof(TagManager), "GetProperName")]
    public static class SharkProperNamePatch
    {
        static void Postfix(Tag tag, ref string __result)
        {
            string name = SharkNameHelper.GetDisplayName(tag.Name, true); 
            if (name != null) __result = name;
        }
    }
}
