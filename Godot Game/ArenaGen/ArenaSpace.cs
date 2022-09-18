using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BMGridWorld
{
    public enum ArenaSpaceTags
    {
        UnsetAtBuildTime,
        Pathable,
        Unpathable,
        Difficult,
        BlocksLOS
    }
    public class ArenaSpace : BMGridSpace
    {

        public ArenaSpace(int xCoord, int yCoord)
        {
            gridX = xCoord;
            gridY = yCoord;
        }

        [JsonIgnore]
        public Vector2 coordinates;

        [JsonIgnore]
        public CombatTileDisplayController AssociatedTileDisplay;
    }
}