using Godot;
using System;
using System.Collections.Generic;

namespace BMGridWorld
{
    public class ArenaGenerator : Node
    {
        [Export]
        public int GridSize = 27;

        [Export]
        public PackedScene TileDisplay;

        private Node parentNode;
        private Vector2 spaceSize;

        private string spriteAssetPath = "res://Sprites/BiomeTiles/";
        private string biomeName;
        private ArenaGrid grid;
        private List<string> terrains;

        /// <summary>
        /// Stores the filepaths of textures, indexed by type of texture. Used to streamline generating the visuals in the arena.
        /// </summary>
        private Dictionary<string, List<Texture>> sprites;

        public override void _Ready()
        {
            if (parentNode == null) parentNode = this;

            //GenerateGrid();
        }

        /// <summary>
        /// Generates a combat arena grid based on the grid space on which it is generated.
        /// </summary>
        /// <param name="space">The grid space this combat will take place on</param>
        public ArenaGrid GenerateGrid(Int32 GenerationSeed = 1)
        {
            /*BMGridWorld.GridSpace space = GlobalDataManager.instance.WorldInfoInstance.LoadedWorlds
                                        [GlobalDataManager.instance.WorldInfoInstance.ActiveWorld].WorldStorage
                                        [GlobalDataManager.instance.WorldInfoInstance.PlayerWorldXPos][GlobalDataManager.instance.WorldInfoInstance.PlayerWorldYPos];*/

            //biomeName = space.BiomeName;
            biomeName = "TestBiome";
            terrains = new List<string>();
            terrains.Add("Base");
            terrains.Add("DifficultTerrain");
            terrains.Add("Obstacle");
            /*foreach (ArenaFeature feature in GlobalDataManager.instance.WorldInfoInstance.BiomeInfo[biomeName].features)
            {
                terrains.Add(feature.folderName);
            }*/ //On hold until combat is hooked up to the full game scene since it needs the GlobalDataManager to be implemented -Kieran
            BuildSpriteDB();
            //BuildSpriteDB(biomeName);

            grid = new ArenaGrid(GridSize, GridSize);
            grid.randomSeed = GenerationSeed; //no longer numpad smash for testing purposes
            //grid.randomSeed = (GenerationSeed + ((space.GridX * 100) + space.GridY)) % int.MaxValue;
            BuildGridDisplay();
            //CombatManager.ActiveArenaGrid = grid; //Sets active grid upon generation
            return grid;
        }

        /// <summary>
        /// Displays a pre-existing combat arena grid from JSON.
        /// </summary>
        /// <param name="gridRef">Path to the combat arena file</param>
        public void GenerateGrid(string gridRef)
        {
            //Use an already created grid from file
        }

        /// <summary>
        /// Loads the sprites associated with the GridSpace's biome
        /// </summary>
        private void BuildSpriteDB()
        {
            sprites = new Dictionary<string, List<Texture>>();
            for (int i = 0; i < terrains.Count; i++)
            {
                sprites.Add(terrains[i], RetrieveTextures(spriteAssetPath + biomeName + '/' + terrains[i] + '/'));
            }
        }

        /// <summary>
        /// Places the grid sprites in the scene and applies the base ground layer texture.
        /// </summary>
        /// <param name="grid">Arena grid being displayed</param>
        private void BuildGridDisplay()
        {
            Random rand = new Random(grid.randomSeed);
            spaceSize = sprites["Base"][0].GetSize();
            parentNode = new Node();
            AddChild(parentNode);
            float startingXPos = spaceSize.x, startingYPos = spaceSize.y * grid.GridRows;
            float currentXPos = startingXPos, currentYPos = startingYPos;
            parentNode.Name = "Arena Parent";

            for (int x = 0; x < grid.GridColumns; x++)
            {
                grid.GridStorage.Add(new List<ArenaSpace>());
                for (int y = 0; y < grid.GridRows; y++)
                {
                    ArenaSpace current = new ArenaSpace(x, y);
                    grid.GridStorage[x].Add(current);

                    CombatTileDisplayController instance = TileDisplay.Instance<CombatTileDisplayController>();
                    instance.Position = new Vector2(currentXPos, currentYPos);
                    int toUse = rand.Next(0, sprites["Base"].Count);
                    instance.Texture = sprites["Base"][toUse];

                    instance.associatedSpace = current;
                    current.coordinates = new Vector2(currentXPos, currentYPos);
                    current.AssociatedTileDisplay = instance;
                    current.WorldPosition = new Vector2(currentXPos, currentYPos); //Pathfinding doesn't work without this
                    if (current.CurrentSpaceTags == null) { current.CurrentSpaceTags = new List<GridSpaceTags>(); }
                    current.CurrentSpaceTags.Add(GridSpaceTags.Pathable);
                    parentNode.AddChild(instance);

                    currentYPos -= spaceSize.y;
                }
                currentXPos += spaceSize.x;
                currentYPos = startingYPos;
            }

            FeatureGeneration();
        }

        /// <summary>
        /// Places features, such as terrain and obstacles.
        /// </summary>
        /// <param name="grid">Arena grid being displayed</param>
        private void FeatureGeneration()
        {
            //TESTING while GlobalDataManager is not initialized -Kieran
            File file = new File();
            file.Open("res://GenerationAssets/OpenWorldStructures/Biomes/TestBiome.json", File.ModeFlags.Read);
            string objAsString = file.GetAsText();
            Biome biome = System.Text.Json.JsonSerializer.Deserialize<Biome>(objAsString);

            Random rand = new Random(grid.randomSeed);
            foreach (ArenaFeature feature in biome.features)
            {
                int genNum = 0;
                while (genNum < feature.minNum ||
                (genNum < feature.maxNum && rand.NextDouble() < (feature.genChanceBase - feature.genChanceDecay * (genNum - feature.minNum))))
                {
                    typeof(ArenaGenerator).GetMethod($"GenerateFeatures{feature.genPattern.ToString()}").Invoke(this, new object[] { feature, rand }); //yay reflection
                    genNum++;
                }
            }
        }

        /// <summary>
        /// Places a single feature randomly on the grid.
        /// </summary>
        public void GenerateFeaturesSingle(ArenaFeature feature, Random rand)
        {
            (int x, int y) coords;
            bool complete = false;
            while (!complete)
            {
                coords.x = rand.Next(0, grid.GridStorage.Count);
                coords.y = rand.Next(0, grid.GridStorage[coords.x].Count);
                complete = PlaceSprite(feature, rand, coords);
            }
        }

        /// <summary>
        /// Places an elliptical group of features using both the feature's X and Y sizes.
        /// </summary>
        public void GenerateFeaturesEllipse(ArenaFeature feature, Random rand)
        {

        }

        /// <summary>
        /// Places a rectanglular group of features using both the feature's X and Y sizes.
        /// </summary>
        public void GenerateFeaturesRectangle(ArenaFeature feature, Random rand)
        {
            List<(int x, int y)> coords = new List<(int x, int y)>();
            List<(int x, int y)> topRow = new List<(int x, int y)>();
            List<(int x, int y)> bottomRow = new List<(int x, int y)>();
            (int x, int y) origin, tl, tr, bl, br;
            do
            {
                origin = (rand.Next(0, grid.GridColumns), rand.Next(0, grid.GridRows));
            } while (!grid.GridStorage[origin.x][origin.y].AssociatedTileDisplay.IsSpaceAvailable);

            int xSize = rand.Next(feature.minXSize, feature.maxXSize);
            int ySize = rand.Next(feature.minYSize, feature.maxYSize);
            GD.Print("X size: " + xSize + "\nY size: " + ySize);

            tl = (origin.x - xSize / 2, origin.y - ySize / 2);
            tr = (origin.x + xSize - xSize / 2 - 1, tl.y);
            bl = (tl.x, origin.y + ySize - ySize / 2 - 1);
            br = (tr.x, bl.y);
            topRow = Util.GridWorldUtilities.LineFinderLerp(tl.x, tl.y, tr.x, tr.y);
            bottomRow = Util.GridWorldUtilities.LineFinderLerp(bl.x, bl.y, br.x, br.y);

            for (int i = 0; i < (topRow.Count <= bottomRow.Count ? topRow.Count : bottomRow.Count); i++)
            {
                coords.AddRange(Util.GridWorldUtilities.LineFinderLerp(topRow[i].x, topRow[i].y, bottomRow[i].x, bottomRow[i].y));
            }
            foreach ((int x, int y) coord in coords)
            {
                if (coord.x >= 0 && coord.x < grid.GridColumns && coord.y >= 0 && coord.y < grid.GridRows)
                { PlaceSprite(feature, rand, coord); }
            }
        }

        /// <summary>
        /// Generates a single-tile width line with the feature's X size
        /// </summary>
        public void GenerateFeaturesLine(ArenaFeature feature, Random rand)
        {
            (int x, int y) coord1, coord2;
            List<(int x, int y)> coords = new List<(int x, int y)>();
            float dist;
            do
            {
                coord1 = (rand.Next(0, grid.GridColumns), rand.Next(0, grid.GridRows));
                coord2 = (rand.Next(-(feature.maxXSize / 2), grid.GridColumns + feature.maxXSize / 2),
                        rand.Next(-(feature.maxXSize / 2), grid.GridRows + feature.maxXSize / 2));
                dist = Util.GridWorldUtilities.Distance(coord1.x, coord1.y, coord2.x, coord2.y);
            } while (!grid.GridStorage[coord1.x][coord1.y].AssociatedTileDisplay.IsSpaceAvailable
                    && !grid.GridStorage[coord2.x][coord2.y].AssociatedTileDisplay.IsSpaceAvailable && coord1 != coord2
                    && dist <= feature.minXSize && dist >= feature.minXSize);

            coords = Util.GridWorldUtilities.LineFinderLerp(coord1.x, coord1.y, coord2.x, coord2.y);
            (int x, int y) prev = coords[0];
            List<(int x, int y)> gaps = new List<(int x, int y)>();
            foreach ((int x, int y) coord in coords)
            {
                if ((coord.x >= 0 && coord.x < grid.GridColumns) && (coord.y >= 0 && coord.y < grid.GridRows))
                { PlaceSprite(feature, rand, coord); }

                if (coord.x != prev.x || coord.y != prev.y)
                {
                    if (prev.x + 1 == coord.x) { gaps.Add((prev.x + 1, prev.y)); }
                    else if (prev.x - 1 == coord.x) { gaps.Add((prev.x - 1, prev.y)); }
                }
                prev = coord;
            }
            if (gaps.Count > 0)
            {
                foreach ((int x, int y) gap in gaps)
                {
                    if ((gap.x >= 0 && gap.x < grid.GridColumns) && (gap.y >= 0 && gap.y < grid.GridRows))
                    { PlaceSprite(feature, rand, gap); }
                }
            }
        }

        /// <summary>
        /// Generates a single-tile width line that starts and ends at the map edges
        /// </summary>
        public void GenerateFeaturesBisector(ArenaFeature feature, Random rand)
        {
            List<(int x, int y)> ends;
            List<(int x, int y)> coords = new List<(int x, int y)>();
            List<int> sides = new List<int>();
            sides.Add(rand.Next(0, 4));
            sides.Add(rand.Next(0, 4));
            while (sides[0] == sides[1])
            {
                sides[1] = rand.Next(0, 4);
            }
            do
            {
                ends = new List<(int x, int y)>();
                for (int i = 0; i < 2; i++)
                {
                    switch (sides[i])
                    {
                        case 0:
                            ends.Add((0, rand.Next(0, grid.GridRows)));
                            break;
                        case 1:
                            ends.Add((grid.GridColumns - 1, rand.Next(0, grid.GridRows)));
                            break;
                        case 2:
                            ends.Add((rand.Next(0, grid.GridColumns), 0));
                            break;
                        case 3:
                            ends.Add((rand.Next(0, grid.GridColumns), grid.GridRows - 1));
                            break;
                        default:
                            ends.Add((0, 0));
                            break;
                    }
                }
            } while (!grid.GridStorage[ends[0].x][ends[0].y].AssociatedTileDisplay.IsSpaceAvailable
                    && !grid.GridStorage[ends[1].x][ends[1].y].AssociatedTileDisplay.IsSpaceAvailable);

            coords = Util.GridWorldUtilities.LineFinderLerp(ends[0].x, ends[0].y, ends[1].x, ends[1].y);
            (int x, int y) prev = coords[0];
            List<(int x, int y)> gaps = new List<(int x, int y)>();
            foreach ((int x, int y) coord in coords)
            {
                if ((coord.x >= 0 && coord.x < grid.GridColumns) && (coord.y >= 0 && coord.y < grid.GridRows))
                { PlaceSprite(feature, rand, coord); }

                if (coord.x != prev.x || coord.y != prev.y)
                {
                    if (prev.x + 1 == coord.x) { gaps.Add((prev.x + 1, prev.y)); }
                    else if (prev.x - 1 == coord.x) { gaps.Add((prev.x - 1, prev.y)); }
                }
                prev = coord;
            }
            if (gaps.Count > 0)
            {
                foreach ((int x, int y) gap in gaps)
                {
                    if ((gap.x >= 0 && gap.x < grid.GridColumns) && (gap.y >= 0 && gap.y < grid.GridRows))
                    { PlaceSprite(feature, rand, gap); }
                }
            }
        }

        /// <summary>
        /// Places a randomized cluster of features where the number of tiles is determined by the feature's X size.
        /// </summary>
        public void GenerateFeaturesCluster(ArenaFeature feature, Random rand)
        {
            (int x, int y) coords;
            do
            {
                coords = (rand.Next(0, grid.GridColumns), rand.Next(0, grid.GridRows));
            } while (!grid.GridStorage[coords.x][coords.y].AssociatedTileDisplay.IsSpaceAvailable);
            PlaceSprite(feature, rand, coords);

            int genNum = 0, genCycle = 0, step = 0;
            (int x, int y) offset;
            while (genNum < feature.minXSize || rand.Next(0, feature.maxXSize) > genNum)
            {
                offset = SpiralMath(genCycle, step);
                step++;
                if (step / 8 == genCycle) //turns out the number of tiles around the center will always be (8 * distance from center)
                {
                    genCycle++;
                    step = 0;
                }

                if (rand.NextDouble() < feature.clusterDensity)
                {
                    genNum++; //Count it as generated even if it's outside the arena
                    if ((coords.x + offset.x >= 0 && coords.x + offset.x < grid.GridColumns) && (coords.y + offset.y >= 0 && coords.y + offset.y < grid.GridRows))
                    {
                        PlaceSprite(feature, rand, (coords.x + offset.x, coords.y + offset.y));
                    }
                }
            }
        }

        private bool PlaceSprite(ArenaFeature feature, Random rand, (int x, int y) coords)
        {
            Texture toUse = sprites[feature.folderName][rand.Next(0, sprites[feature.folderName].Count)];
            Sprite newSprite = new Sprite();
            newSprite.Texture = toUse;
            return grid.GridStorage[coords.x][coords.y].AssociatedTileDisplay.UpdateFeatures(feature, newSprite);
        }

        /// <summary>
        /// Calculates the position in a spiral based on the given parameters.
        /// </summary>
        /// <param name="cycle">The circular layer in which the step takes place. In other words, how far away from the center it is.</param>
        /// <param name="step">How many steps have passed in the current cycle.</param>
        /// <returns>The coordinates (x, y) of the given step in the given cycle.</returns>
        private (int x, int y) SpiralMath(int cycle, int step)
        {
            float v = Mathf.Clamp(((4 * cycle * Mathf.Acos(Mathf.Cos((((0.5f * step) + (1.5f * cycle)) * Mathf.Pi) / (2 * cycle))) / Mathf.Pi) - (2 * cycle)), -cycle, cycle);
            float w = Mathf.Clamp(((4 * cycle * Mathf.Acos(Mathf.Cos((((0.5f * step) + (-1.5f * cycle)) * Mathf.Pi) / (2 * cycle))) / Mathf.Pi) - (2 * cycle)), -cycle, cycle);
            return (Mathf.RoundToInt(v), Mathf.RoundToInt(w));
        }

        private List<Texture> RetrieveTextures(string directoryPath)
        {
            Directory dir = new Directory();
            if (dir.Open(directoryPath) == Error.Ok)
            {
                List<Texture> textures = new List<Texture>();
                dir.ListDirBegin(true, true);
                string filename = dir.GetNext();
                while (filename != "")
                {
                    if (filename.EndsWith(".png"))
                    {
                        textures.Add((Texture)GD.Load(directoryPath + filename));
                    }
                    filename = dir.GetNext();
                }

                return textures;
            }
            else
            {
                GD.PrintErr("Error: directory " + directoryPath + " not found.");
                return null;
            }
        }
    }
}