using Godot;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Linq;
using BMGridWorld;

namespace BMQuestSystem
{
    public class QuestSystem : Node
    {
        public static QuestSystem instance;

        private Dictionary<int, Quest> masterQuestDictionary;
        public Dictionary<int, Quest> QuestDictionary { get {return masterQuestDictionary; } }
        private QuestManager manager 
        {
            get 
            {
                if (GlobalUIController.instance == null || GlobalUIController.instance.QuestMenu == null)
                {
                    return null;
                }
                else return GlobalUIController.instance.QuestMenu;
            }
        }

        private bool loadedFromSave = false; //This is for testing because no file is loaded when running directly in the scene

        //Quest-related events
        public event Action<int> OnQuestStart;
        public event Action<int> OnQuestAdvance;
        public event Action<int> OnQuestEnd;

        private bool buildNextFrame = false;

        public override void _EnterTree()
        {
            if (instance == null) instance = this;
            masterQuestDictionary = new Dictionary<int, Quest>();
        }

        public override void _Process(float delta)
        {
            if (buildNextFrame && manager != null)
            {
                if (loadedFromSave)
                {
                    foreach (KeyValuePair<int, Quest> pair in masterQuestDictionary)
                    {
                        if (pair.Value.isKnown && !pair.Value.isComplete)
                        {
                            pair.Value.StartQuest();
                        }
                    }
                }
                manager.BuildQuestTree();
                buildNextFrame = false;
            }
        }

        /// <summary>
        /// Called when the UI is loaded in to the scene
        /// </summary>
        public void UILoaded()
        {
            if (!loadedFromSave)
            {
                WorldGov.instance.OnPlayerReady -= ReadyQuestData;
                WorldGov.instance.OnPlayerReady += ReadyQuestData;
            }
            buildNextFrame = true;
        }

        /// <summary>
        /// Generates quest data from files when the game is first run. Only necessary when loading directly into scene without loading a save file.
        /// </summary>
        public void ReadyQuestData()
        {
            BuildQuestDictionary();
            buildNextFrame = true;
        }

        /// <summary>
        /// Adds a quest in the master dictionary to the quest window.
        /// </summary>
        /// <param name="id">ID of the quest to activate</param>
        public void ActivateQuest(int id)
        {
            GenerateLocationData(id);
            masterQuestDictionary[id].StartQuest();
            OnQuestStart?.Invoke(id);
        }

        public void AdvanceQuest(int id)
        {
            OnQuestAdvance?.Invoke(id);
        }

        public void EndQuest(int id)
        {
            OnQuestEnd?.Invoke(id);
        }

        /// <summary>
        /// Generates the data for the location in each step of a given quest. Generates based on current location and the generation seed.
        /// </summary>
        /// <param name="id">ID of the quest to generate info for.</param>
        private void GenerateLocationData(int id)
        {
            Quest quest = masterQuestDictionary[id];
            QuestStep tempStep = null;

            foreach (QuestStep step in quest.steps)
            {
                switch (step.locVicinity)
                {
                    case LocationVicinity.PreviousQuest:
                        tempStep = masterQuestDictionary[step.previousID].steps[step.previousStepNum];
                        goto case LocationVicinity.PreviousStep;

                    case LocationVicinity.PreviousStep:
                        if (tempStep == null) tempStep = quest.steps[step.previousStepNum];
                        step.worldName = tempStep.worldName;
                        step.locDesc = tempStep.locDesc;
                        step.gridX = tempStep.gridX;
                        step.gridY = tempStep.gridY;
                        tempStep = null;
                        break;

                    case LocationVicinity.Local:
                        step.worldName = GlobalDataManager.instance.WorldInfoInstance.ActiveWorld;
                        GetLocationInfo(step.worldName, step);
                        break;

                    case LocationVicinity.System:
                        GetLocationInfo("", step);
                        break;

                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Gets the information about the chosen location and puts it in the quest step.
        /// </summary>
        /// <param name="world">The name of the world the quest is on.</param>
        /// <param name="step">The quest step this is for.</param>
        private void GetLocationInfo(string world, QuestStep step)
        {
            int seed = WorldGov.instance.GlobalRandomSeed;
            Random rand = (step.seedOffset == 0) ? new Random() : new Random(seed + step.seedOffset); //seed the rng

            if (world == "") //Pick a random, different world than the player is on. Can be influenced with a seed offset
            {
                int repeats = 1;
                while (world != GlobalDataManager.instance.WorldInfoInstance.ActiveWorld && world != "")
                {
                    if (step.seedOffset == 0)
                    {
                        world = GlobalDataManager.instance.WorldInfoInstance.LoadedWorlds.ElementAt(
                            new Random().Next(0, GlobalDataManager.instance.WorldInfoInstance.LoadedWorlds.Count)).Key;
                    }
                    else
                    {
                        world = GlobalDataManager.instance.WorldInfoInstance.LoadedWorlds.ElementAt(
                            new Random(seed + step.seedOffset * repeats).Next(0, GlobalDataManager.instance.WorldInfoInstance.LoadedWorlds.Count)).Key;
                    }
                    repeats++;
                }
            }
            ZoneGrid grid = GlobalDataManager.instance.WorldInfoInstance.LoadedWorlds[world];

            switch (step.locType)
            {
                case LocationType.City:
                    step.locRef = rand.Next(0, grid.Cities.Count);
                    City city = grid.Cities[step.locRef];
                    step.gridX = city.XCoord;
                    step.gridY = city.YCoord;
                    if (step.locDesc == default(string)) step.locDesc = $"{city.FeatureName} on {world}:";
                    break;

                case LocationType.Encounter:
                    step.locRef = rand.Next(0, grid.EncounterAreas.Count);
                    EncounterArea encounter = grid.EncounterAreas[step.locRef];
                    step.gridX = encounter.XCoord;
                    step.gridY = encounter.YCoord;
                    if (step.locDesc == default(string)) step.locDesc = "the location:";
                    break;

                case LocationType.NewEncounter:
                    goto case LocationType.Point;
                case LocationType.Point:
                    GridSpace space = null;
                    bool empty = false;
                    (int x, int y) coords;
                    int repeats = 1;
                    do 
                    {
                        if (step.seedOffset != 0)
                        {
                            coords.x = new Random(seed + step.seedOffset * repeats).Next(0, grid.GridColumns);
                            coords.y = new Random(seed - step.seedOffset * repeats).Next(0, grid.GridRows);
                        }
                        else 
                        {
                            coords.x = new Random().Next(0, grid.GridColumns);
                            coords.y = new Random().Next(0, grid.GridRows);
                        }

                        if (grid.WorldStorage[coords.x][coords.y] != null) space = grid.WorldStorage[coords.x][coords.y];
                        else
                        {
                            repeats++;
                            continue;
                        }

                        if (space.CurrentSpaceTags.Contains(GridSpaceTags.InBounds) && space.CurrentSpaceTags.Contains(GridSpaceTags.Pathable)
                            && !space.CurrentSpaceTags.Contains(GridSpaceTags.ContainsStructure))
                        {
                            empty = true;
                        }
                        repeats++;
                    } while (!empty);
                    step.gridX = coords.x;
                    step.gridY = coords.y;
                    if (step.locDesc == default(string)) step.locDesc = "the location:";
                    break;
                    
                default:
                    break;
            }
        }

        /// <summary>
        /// Builds the master quest dictionary from the built-in database
        /// </summary>
        public void BuildQuestDictionary()
        {
            masterQuestDictionary.Clear();
            List<Quest> questlist = new List<Quest>();
            int mainGenID = 0, sideGenID = 1000;
            if (JSONUtilities.JSONRetriever.RetrieveListFromDirectory<Quest>("res://GenerationAssets/Quests/", out questlist))
            {
                for (int i = 0; i < questlist.Count; i++)
                {
                    if (questlist[i].category == QuestCategory.Main)
                    {
                        if (mainGenID < questlist[i].ID) mainGenID = questlist[i].ID + 1;
                        while (masterQuestDictionary.ContainsKey(questlist[i].ID))
                        {
                            questlist[i].ID = mainGenID;
                            mainGenID += 1;
                        }
                    }
                    else
                    {
                        questlist[i].category = QuestCategory.Side;
                        if (sideGenID < questlist[i].ID) sideGenID = questlist[i].ID + 1;
                        while (masterQuestDictionary.ContainsKey(questlist[i].ID))
                        {
                            questlist[i].ID = sideGenID;
                            sideGenID += 1;
                        }
                    }
                    masterQuestDictionary.Add(questlist[i].ID, questlist[i]);
                    if (questlist[i].isKnown) ActivateQuest(questlist[i].ID);
                }
            }
            else GD.PrintErr("Could not retrieve quests from database.");
        }

        /// <summary>
        /// Saves the master quest dictionary to file.
        /// </summary>
        /// <param name="filepath">Filepath to save to</param>
        /// <param name="filename">Name of the file</param>
        public void SaveQuestsToFile(string filepath, string filename)
        {
            string finalPath = filepath + filename + ".json";
            File file = new File();
            file.Open(finalPath, File.ModeFlags.Write);
            JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };
            string output = JsonSerializer.Serialize(masterQuestDictionary, options);
            file.StoreLine(output);
            file.Close();
        }

        /// <summary>
        /// Loads the master quest dictionary from save data
        /// </summary>
        /// <param name="filepath">Filepath to save to</param>
        /// <param name="filename">Name of the file</param>
        public void LoadQuestDictionary(string filepath)
        {
            string finalPath = filepath + ".json";
            File file = new File();
            if (file.FileExists(finalPath))
            {
                file.Open(finalPath, File.ModeFlags.Read);
                string output = file.GetAsText();
                masterQuestDictionary = JsonSerializer.Deserialize<Dictionary<int, Quest>>(output);
                if (masterQuestDictionary.Count == 0) BuildQuestDictionary();
                else loadedFromSave = true;
            }
            else BuildQuestDictionary();

            //buildNextFrame = true;
        }
    }
}