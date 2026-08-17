using System.Collections.Generic;

namespace Shark
{
    public static class SharkTuning
    {
        public static float MASS = 200f;                          
        public static float HP = 25f;                             
        public static float LIFESPAN = 75f;                      

        public static float STANDARD_CALORIES_PER_CYCLE = 1000f;  
        public static float STANDARD_STARVE_CYCLES = 5f;
        public static float STANDARD_STOMACH_SIZE =
            STANDARD_CALORIES_PER_CYCLE * STANDARD_STARVE_CYCLES; 

        public static int PEN_SIZE_PER_CREATURE = 16;            

        public static float MOVE_SPEED = 2f;                      

        public static float WARN_LOW_TEMP  = 273.15f;  
        public static float WARN_HIGH_TEMP = 313.15f;   
        public static float LETHAL_LOW_TEMP  = 253.15f; 
        public static float LETHAL_HIGH_TEMP = 333.15f; 

        public static float EGG_MASS = 3f;                            
        public static float EGG_SHELL_RATIO = 0.33333334f;           
        public static List<FertilityMonitor.BreedingChance> EGG_CHANCES_BASE =
            new List<FertilityMonitor.BreedingChance>
            {
                new FertilityMonitor.BreedingChance
                {
                    egg = "SharkEgg".ToTag(),                        
                    weight = 1f,
                },
            };

        public static float CALORIES_PER_KG_OF_FISH_MEAT = 1000f;    
        public static float CALORIES_PER_KG_OF_COOKED_FISH = 1600f; 
        public static float CALORIES_PER_KG_OF_PREY =
            CALORIES_PER_KG_OF_FISH_MEAT * 1.5f / PacuTuning.MASS; 
        public static float PREY_CONVERSION_RATE = 90f / PacuTuning.MASS; 
        public static float MEAT_CONVERSION_RATE = 60f;           

        public static float MIN_CALORIES_BEFORE_POOP = 1000f;      
        public static float POOP_LIME_RATIO = 0.3f;                 
        public static float POOP_COQUINA_RATIO = 0.7f;             
        public static float POOP_MASS_KG = 60f;                   
    }
}
