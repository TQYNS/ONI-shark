using UnityEngine;

namespace Shark
{
    [EntityConfigOrder(2)]                   
    public class SharkBabyConfig : IEntityConfig
    {
        public const string ID = "SharkBaby";                
        public const string ANIM_FILE = "baby_shark_kanim";   

        public string[] GetDlcIds() => DlcManager.DLC5;

        public GameObject CreatePrefab()
        {
            var go = EntityTemplates.ExtendEntityToWildCreature(
                BaseSharkBabyConfig.CreatePrefab(ID,
                    ModStrings.CREATURES.SPECIES.SHARK.BABY_NAME,
                    ModStrings.CREATURES.SPECIES.SHARK.BABY_DESC,
                    ANIM_FILE),
                SharkTuning.PEN_SIZE_PER_CREATURE,
                true);   

            EntityTemplates.CreateAndRegisterBaggedCreature(go, false, true, true);

            EntityTemplates.ExtendEntityToBeingABaby(
                go,
                SharkConfig.ID,                               
                null,                                         
                true,                                          
                5f);                                           

            go.AddTag(GameTags.OriginalCreature);
            return go;
        }

        public void OnPrefabInit(GameObject inst) { }
        public void OnSpawn(GameObject inst) { }
    }
}
