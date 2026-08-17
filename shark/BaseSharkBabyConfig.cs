using System.Collections.Generic;
using Klei.AI;
using STRINGS;
using TUNING;
using UnityEngine;

namespace Shark
{
    public static class BaseSharkBabyConfig
    {
        public static GameObject CreatePrefab(string id, string name, string desc, string animFile)
        {
            GameObject go = EntityTemplates.CreatePlacedEntity(
                id, name, desc,
                SharkTuning.MASS,
                Assets.GetAnim(animFile),
                "idle_loop",                      
                Grid.SceneLayer.Creatures,
                2, 1,                              
                DECOR.BONUS.TIER0, default(EffectorValues),
                SimHashes.Creature,
                null,
                (SharkTuning.WARN_LOW_TEMP + SharkTuning.WARN_HIGH_TEMP) / 2f);


            go.GetComponent<KPrefabID>().AddTag(GameTags.SwimmingCreature, false);
            go.GetComponent<KPrefabID>().AddTag(GameTags.Creatures.Swimmer, false);

            Trait trait = Db.Get().CreateTrait(SharkConfig.BASE_TRAIT_ID, name, name,
                null, false, null, true, true);
            trait.Add(new AttributeModifier(
                Db.Get().Amounts.Calories.maxAttribute.Id,
                SharkTuning.STANDARD_STOMACH_SIZE, name, false, false, true));
            trait.Add(new AttributeModifier(
                Db.Get().Amounts.Calories.deltaAttribute.Id,
                -SharkTuning.STANDARD_CALORIES_PER_CYCLE / 600f, name, false, false, true));
            trait.Add(new AttributeModifier(
                Db.Get().Amounts.HitPoints.maxAttribute.Id,
                SharkTuning.HP, name, false, false, true));
            trait.Add(new AttributeModifier(
                Db.Get().Amounts.Age.maxAttribute.Id,
                SharkTuning.LIFESPAN, name, false, false, true));

            EntityTemplates.ExtendEntityToBasicCreature(
                isWarmBlooded: false,
                template: go,
                anim_filename: animFile,
                build_filename: null,
                symbol_override_prefix: null,
                faction: FactionManager.FactionID.Predator,  
                initialTraitID: SharkConfig.BASE_TRAIT_ID,
                NavGridName: SharkBabyNavGrid.ID,   
                navType: NavType.Swim,
                max_probing_radius: 32,
                moveSpeed: SharkTuning.MOVE_SPEED,
                onDeathDropID: "Tallow",         
                onDeathDropCount: 200f,           
                drownVulnerable: false,
                entombVulnerable: false,
                warningLowTemperature: SharkTuning.WARN_LOW_TEMP,
                warningHighTemperature: SharkTuning.WARN_HIGH_TEMP,
                lethalLowTemperature: SharkTuning.LETHAL_LOW_TEMP,
                lethalHighTemperature: SharkTuning.LETHAL_HIGH_TEMP);

            ChoreTable.Builder builder = new ChoreTable.Builder()
                .Add(new DeathStates.Def(), true, -1)           
                .Add(new AnimInterruptStates.Def(), true, -1)
                .Add(new DebugGoToStates.Def(), true, -1)
                .Add(new GrowUpStates.Def(), true, -1)          
                .Add(new IncubatingStates.Def(), true, -1)      
                .Add(new BaggedStates.Def(), true, -1)          
                .Add(new FixedCaptureStates.Def(), true, -1)    
                .Add(new FallStates.Def                         
                {
                    getLandAnim = smi => smi.GetSMI<CreatureFallMonitor.Instance>().CanSwimAtCurrentLocation()
                        ? "idle_loop" : "flop_loop",
                }, true, -1)
                .Add(new FlopStates.Def                         
                {
                    flipFacing = true,
                    frameToFlopStart = 8,
                    frameToFlopEnd = 29,
                }, true, -1)
                .PushInterruptGroup()
                .Add(new EatStates.Def(), true, -1)              
                .Add(new DrinkMilkStates.Def                     
                {
                    shouldBeBehindMilkTank = false,
                    drinkCellOffsetGetFn = new DrinkMilkStates.Def.DrinkCellOffsetGetFn(
                        DrinkMilkStates.Def.DrinkCellOffsetGet_TwoByTwo), 
                }, true, -1)
                .Add(new PoopStates.Def(                        
                    Assets.GetAnim(animFile),                  
                    STRINGS.CREATURES.STATUSITEMS.EXPELLING_SOLID.NAME,
                    STRINGS.CREATURES.STATUSITEMS.EXPELLING_SOLID.TOOLTIP,
                    false),                                    
                    true, -1)
                .PopInterruptGroup()
                .Add(new IdleStates.Def(), true, -1);           

            go.AddOrGetDef<CreatureFallMonitor.Def>().canSwim = true;
            go.AddOrGetDef<FlopMonitor.Def>();
            go.AddOrGetDef<FishOvercrowdingMonitor.Def>();
            go.AddOrGet<LoopingSounds>();

            SharkTags.EnsureSharkSpecies();
            EntityTemplates.AddCreatureBrain(go, builder,
                SharkTags.SharkSpecies, null);

            HashSet<Tag> preyTags = new HashSet<Tag>
            {
                "Pacu".ToTag(), "PacuCleaner".ToTag(), "PacuTropical".ToTag(),
                "ParrotFish".ToTag(), "PufferFish".ToTag(),
                "PrehistoricPacu".ToTag(), 
            };
            HashSet<Tag> meatTags = new HashSet<Tag> { "FishMeat".ToTag() };
            HashSet<Tag> cookedTags = new HashSet<Tag> { "CookedFish".ToTag() };
            Diet diet = new Diet(new Diet.Info[]
            {
                new Diet.Info(preyTags, SimHashes.Coquina.CreateTag(),
                    SharkTuning.CALORIES_PER_KG_OF_PREY,       
                    SharkTuning.PREY_CONVERSION_RATE,         
                    null, 0f, false,
                    Diet.Info.FoodType.EatPrey, false, null),
                new Diet.Info(meatTags, SimHashes.Lime.CreateTag(),
                    SharkTuning.CALORIES_PER_KG_OF_FISH_MEAT,  
                    SharkTuning.MEAT_CONVERSION_RATE,         
                    null, 0f, false,
                    Diet.Info.FoodType.EatSolid, false, null),
                new Diet.Info(cookedTags, SimHashes.Lime.CreateTag(),
                    SharkTuning.CALORIES_PER_KG_OF_COOKED_FISH, 
                    SharkTuning.MEAT_CONVERSION_RATE,
                    null, 0f, false,
                    Diet.Info.FoodType.EatSolid, false, null),
            });
            var calDef = go.AddOrGetDef<CreatureCalorieMonitor.Def>();
            calDef.diet = diet;
            calDef.minConsumedCaloriesBeforePooping =
                SharkTuning.MIN_CALORIES_BEFORE_POOP;         
            go.AddOrGetDef<SolidConsumerMonitor.Def>().diet = diet;

            go.GetComponent<KPrefabID>().prefabSpawnFn += delegate (GameObject inst)
            {
                inst.Subscribe((int)GameHashes.Died, delegate (object data)   
                {
                    int cell = Grid.PosToCell(inst.transform.GetPosition());
                    var dm = GameUtil.KInstantiate(Assets.GetPrefab("DinosaurMeat"),
                        Grid.CellToPos(cell, CellAlignment.Top, Grid.SceneLayer.Ore),
                        Grid.SceneLayer.Ore);
                    dm.GetComponent<PrimaryElement>().Mass = 6f;
                    dm.SetActive(true);
                });
            };

            var critterOrder = TUNING.CREATURES.SORTING.CRITTER_ORDER;
            if (!critterOrder.ContainsKey(id))
                critterOrder[id] = 16;           
            go.AddOrGet<Pickupable>().sortOrder = critterOrder[id];

            return go;
        }
    }
}
