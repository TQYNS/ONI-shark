namespace Shark
{
    public static class SharkNavGrid
    {
        public const string ID = "SharkSwimmerGrid";

        public static void EnsureRegistered()
        {
            Pathfinding pf = Pathfinding.Instance;
            if (pf == null)
                return;

            foreach (NavGrid grid in pf.GetNavGrids())
            {
                if (grid.id == ID)
                    return;
            }

            pf.AddNavGrid(Create());
        }

        public static NavGrid Create()
        {
            CellOffset[] boundingOffsets = new CellOffset[]
            {
                new CellOffset(-1, 0), new CellOffset(0, 0), new CellOffset(1, 0),
                new CellOffset(-1, 1), new CellOffset(0, 1), new CellOffset(1, 1),
            };

            NavGrid.Transition[] transitions = new NavGrid.Transition[]
            {
                new NavGrid.Transition(NavType.Swim, NavType.Swim, -1, 0, NavAxis.NA, true, true, true, 2, "swim_swim_1_0",
                    new CellOffset[]
                    {
                        new CellOffset(-2, 0), new CellOffset(-1, 0), new CellOffset(0, 0),
                        new CellOffset(-2, 1), new CellOffset(-1, 1), new CellOffset(0, 1),
                    },
                    new CellOffset[0], new NavOffset[0], new NavOffset[0], true, 1f, false),

                new NavGrid.Transition(NavType.Swim, NavType.Swim, 1, 0, NavAxis.NA, true, true, true, 2, "swim_swim_1_0",
                    new CellOffset[]
                    {
                        new CellOffset(0, 0), new CellOffset(1, 0), new CellOffset(2, 0),
                        new CellOffset(0, 1), new CellOffset(1, 1), new CellOffset(2, 1),
                    },
                    new CellOffset[0], new NavOffset[0], new NavOffset[0], true, 1f, false),

                new NavGrid.Transition(NavType.Swim, NavType.Swim, 0, -1, NavAxis.NA, true, true, true, 3, "swim_swim_1_0",
                    new CellOffset[]
                    {
                        new CellOffset(-1, -1), new CellOffset(0, -1), new CellOffset(1, -1),
                        new CellOffset(-1, 0), new CellOffset(0, 0), new CellOffset(1, 0),
                    },
                    new CellOffset[0], new NavOffset[0], new NavOffset[0], true, 1f, false),

                new NavGrid.Transition(NavType.Swim, NavType.Swim, 0, 1, NavAxis.NA, true, true, true, 3, "swim_swim_1_0",
                    new CellOffset[]
                    {
                        new CellOffset(-1, 1), new CellOffset(0, 1), new CellOffset(1, 1),
                        new CellOffset(-1, 2), new CellOffset(0, 2), new CellOffset(1, 2),
                    },
                    new CellOffset[0], new NavOffset[0], new NavOffset[0], true, 1f, false),

                new NavGrid.Transition(NavType.Swim, NavType.Swim, -1, -1, NavAxis.NA, true, true, true, 2, "swim_swim_1_0",
                    new CellOffset[]
                    {
                        new CellOffset(-2, -1), new CellOffset(-1, -1), new CellOffset(0, -1),
                        new CellOffset(-2, 0), new CellOffset(-1, 0), new CellOffset(0, 0),
                    },
                    new CellOffset[0], new NavOffset[0], new NavOffset[0], true, 1f, false),

                new NavGrid.Transition(NavType.Swim, NavType.Swim, 1, -1, NavAxis.NA, true, true, true, 2, "swim_swim_1_0",
                    new CellOffset[]
                    {
                        new CellOffset(0, -1), new CellOffset(1, -1), new CellOffset(2, -1),
                        new CellOffset(0, 0), new CellOffset(1, 0), new CellOffset(2, 0),
                    },
                    new CellOffset[0], new NavOffset[0], new NavOffset[0], true, 1f, false),

                new NavGrid.Transition(NavType.Swim, NavType.Swim, -1, 1, NavAxis.NA, true, true, true, 2, "swim_swim_1_0",
                    new CellOffset[]
                    {
                        new CellOffset(-2, 1), new CellOffset(-1, 1), new CellOffset(0, 1),
                        new CellOffset(-2, 2), new CellOffset(-1, 2), new CellOffset(0, 2),
                    },
                    new CellOffset[0], new NavOffset[0], new NavOffset[0], true, 1f, false),

                new NavGrid.Transition(NavType.Swim, NavType.Swim, 1, 1, NavAxis.NA, true, true, true, 2, "swim_swim_1_0",
                    new CellOffset[]
                    {
                        new CellOffset(0, 1), new CellOffset(1, 1), new CellOffset(2, 1),
                        new CellOffset(0, 2), new CellOffset(1, 2), new CellOffset(2, 2),
                    },
                    new CellOffset[0], new NavOffset[0], new NavOffset[0], true, 1f, false),

                new NavGrid.Transition(NavType.Swim, NavType.Swim, 2, 1, NavAxis.NA, true, true, true, 4, "swim_swim_1_0",
                    new CellOffset[]
                    {
                        new CellOffset(1, 1), new CellOffset(2, 1), new CellOffset(3, 1),
                        new CellOffset(1, 2), new CellOffset(2, 2), new CellOffset(3, 2),
                    },
                    new CellOffset[0], new NavOffset[0], new NavOffset[0], true, 1f, false),

                new NavGrid.Transition(NavType.Swim, NavType.Swim, 2, -1, NavAxis.NA, true, true, true, 4, "swim_swim_1_0",
                    new CellOffset[]
                    {
                        new CellOffset(1, -1), new CellOffset(2, -1), new CellOffset(3, -1),
                        new CellOffset(1, 0), new CellOffset(2, 0), new CellOffset(3, 0),
                    },
                    new CellOffset[0], new NavOffset[0], new NavOffset[0], true, 1f, false),

                new NavGrid.Transition(NavType.Swim, NavType.Swim, -2, 1, NavAxis.NA, true, true, true, 4, "swim_swim_1_0",
                    new CellOffset[]
                    {
                        new CellOffset(-3, 1), new CellOffset(-2, 1), new CellOffset(-1, 1),
                        new CellOffset(-3, 2), new CellOffset(-2, 2), new CellOffset(-1, 2),
                    },
                    new CellOffset[0], new NavOffset[0], new NavOffset[0], true, 1f, false),

                new NavGrid.Transition(NavType.Swim, NavType.Swim, -2, -1, NavAxis.NA, true, true, true, 4, "swim_swim_1_0",
                    new CellOffset[]
                    {
                        new CellOffset(-3, -1), new CellOffset(-2, -1), new CellOffset(-1, -1),
                        new CellOffset(-3, 0), new CellOffset(-2, 0), new CellOffset(-1, 0),
                    },
                    new CellOffset[0], new NavOffset[0], new NavOffset[0], true, 1f, false),
            };

            NavGrid.NavTypeData[] navTypeData = new NavGrid.NavTypeData[]
            {
                new NavGrid.NavTypeData
                {
                    navType = NavType.Swim,
                    idleAnim = "idle_loop",
                },
            };

            return new NavGrid(
                ID,
                transitions,
                navTypeData,
                boundingOffsets,
                new NavTableValidator[] { new GameNavGrids.SwimValidator(true, true) },
                3,
                2,
                16);
        }
    }
}
