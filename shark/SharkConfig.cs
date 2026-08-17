using UnityEngine;

namespace Shark
{
    [EntityConfigOrder(1)]
    public class SharkConfig : IEntityConfig
    {
        public const string ID = "Shark";                
        public const string BASE_TRAIT_ID = "SharkBaseTrait";
        public const string ANIM_FILE = "shark_kanim";   
        public const string EGG_ID = "SharkEgg";          
        public const string EGG_ANIM_FILE = "shark_egg_kanim";  
        public const string BABY_ID = "SharkBaby";       
        public const int EGG_SORT_ORDER = 500;          

        public string[] GetDlcIds() => DlcManager.DLC5;

        public GameObject CreatePrefab()
        {
            GameObject go = EntityTemplates.ExtendEntityToFertileCreature(
                CreateShark(ID,
                    ModStrings.CREATURES.SPECIES.SHARK.NAME,
                    ModStrings.CREATURES.SPECIES.SHARK.DESC,
                    ANIM_FILE),
                null,                                   
                EGG_ID,
                ModStrings.CREATURES.SPECIES.SHARK.EGG_NAME,
                ModStrings.CREATURES.SPECIES.SHARK.EGG_DESC,
                EGG_ANIM_FILE,
                SharkTuning.EGG_MASS,
                SharkTuning.EGG_SHELL_RATIO,
                BABY_ID,
                60f,                                     
                20f,                                      
                SharkTuning.EGG_CHANCES_BASE,
                EGG_SORT_ORDER,
                true,                                     
                true,                                    
                1f,                                      
                false,                                   
                false,                                    
                SharkTuning.EGG_MASS,                     
                true);                                   
            go.AddTag(GameTags.LargeCreature);
            go.AddTag(GameTags.OriginalCreature);
            return go;
        }

        public static GameObject CreateShark(string id, string name, string desc, string animFile)
        {
            var go = EntityTemplates.ExtendEntityToWildCreature(
                BaseSharkConfig.CreatePrefab(id, name, desc, animFile),
                SharkTuning.PEN_SIZE_PER_CREATURE,
                true);
            EntityTemplates.CreateAndRegisterBaggedCreature(go, false, true, true);
            go.AddTag(GameTags.OriginalCreature);
            return go;
        }

        public void OnPrefabInit(GameObject inst) { }
        public void OnSpawn(GameObject inst) { }
    }
}
