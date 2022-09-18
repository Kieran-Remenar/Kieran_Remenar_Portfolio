using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace BMGridWorld
{
    public class ArenaGrid : BMGrid
    {
        public ArenaGrid(int rows, int columnns)
        {
            gridRows = rows;
            gridColumns = columnns;
            gridStorage = new List<List<ArenaSpace>>();
        }

        private List<List<ArenaSpace>> gridStorage;
        public List<List<ArenaSpace>> GridStorage
        {
            get { return gridStorage; }
            protected set { gridStorage = value; }
        }

        public int randomSeed { get; set; }

        public override BMGridSpace[][] GetBMGridInfo()
        {
            var toReturn = new BMGridSpace[gridColumns][];
            for (int X = 0; X < gridStorage.Count; X++)
            {
                toReturn[X] = gridStorage[X].ToArray();
            }
            return toReturn;
        }
    }
}