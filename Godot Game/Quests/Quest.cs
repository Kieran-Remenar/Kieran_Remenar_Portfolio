using Godot;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using BMInventorySystem;
using BMGridWorld;

namespace BMQuestSystem
{
    public enum QuestType
    {
        None = 0,
        Fetch,
        Kill,
        Move,
        Talk,
        Flag,
    }

    public enum QuestCategory
    {
        None = 0,
        Main,
        Side,
        Complete,
        Failed,
    }

    public class Quest
    {
        public int ID { get; set; } //unique ID of the quest, used for sorting and indexing
        public QuestCategory category { get; set; } //Whether the quest is a main or side quest, or has been completed or failed.
        public bool isKnown { get; set; } //Is this quest known? Leave blank unless it is the very first quest that the player should start with.
        public bool isRandom { get; set; } //Leave blank when creating; will be set to true only for randomly generated quests.
        public bool isComplete { get; set; }
        public string name { get; set; } //name of the Quest
        public string client { get; set; } //name of the client issuing the quest, if any
        public string subcategory { get; set; } //Subcategory this quest belongs to, will be grouped together with the rest in the window

        /// <summary>
        /// An array of the individual steps of a quest
        /// </summary>
        /// <value></value>
        public QuestStep[] steps { get; set; }

        /// <summary>
        /// Index of the step the quest is currently on. Leave at default (0) when creating quests.
        /// </summary>
        /// <value></value>
        public int stepIndex { get; set; }

        /// <summary>
        /// An array of the rewards earned by completing the quest. Can be given selectively based on flags.
        /// </summary>
        /// <value></value>
        public QuestReward[] rewards { get; set; }

        public bool isActive { get; set; } //Leave this empty; will be filled out during generation and/or gameplay.

        /// <summary>
        /// Begins the quest, allowing it to be progressed.
        /// </summary>
        public void StartQuest()
        {
            AdvanceQuest();
        }

        /// <summary>
        /// Function for debugging. Forces the quest to a certain step.
        /// </summary>
        /// <param name="step">The step number. Zero-indexed.</param>
        public void GoToStep(int step)
        {
            stepIndex = step;
            foreach (string flag in steps[stepIndex].FlagsNeeded)
            {
                GlobalDataManager.instance.FlagManagerInstance.SetFlag(flag, false);
            }
            AdvanceQuest();
        }

        /// <summary>
        /// Begins counting for the time limit, if applicable.
        /// </summary>
        private void SetTimeLimit()
        {
            steps[stepIndex].remainingTime = steps[stepIndex].timeLimit;
            steps[stepIndex].timeLimit += GlobalDataManager.instance.ElapsedTime;
        }

        /// <summary>
        /// Sets the necessary event calls for the quest step when advancing.
        /// </summary>
        private void AdvanceQuest()
        {
            UnsubscribeAll();
            GlobalDataManager.instance.FlagManagerInstance.OnFlagsUpdate += CheckFlagSet;

            switch (steps[stepIndex].type)
            {
                case QuestType.Fetch:
                    if (!steps[stepIndex].stepStarted)
                    {
                        steps[stepIndex].FlagsNeeded.Add($"{ID}-{stepIndex}.fetch");
                        steps[stepIndex].FlagsNeeded.Add($"{ID}-{stepIndex}.talk");
                    }
                    Inventory.instance.OnInventoryContentsChange += CheckInventory;
                    CheckInventory();
                    break;
                case QuestType.Kill:
                    if (!steps[stepIndex].stepStarted) steps[stepIndex].FlagsNeeded.Add($"{ID}-{stepIndex}.kill");
                    //subscribe to the correct event later
                    break;
                case QuestType.Talk:
                    if (!steps[stepIndex].stepStarted) steps[stepIndex].FlagsNeeded.Add($"{ID}-{stepIndex}.talk");
                    DialogueWindow.instance.OnDialogueEnd += CheckDialogueEnd;
                    break;
                case QuestType.Move:
                    if (!steps[stepIndex].stepStarted) steps[stepIndex].FlagsNeeded.Add($"{ID}-{stepIndex}.move");
                    GlobalDataManager.instance.WorldInfoInstance.LoadedWorlds
                        [steps[stepIndex].worldName].WorldStorage[steps[stepIndex].gridX][steps[stepIndex].gridY].OnGridSpaceEnter += CheckDestinationReached;
                    break;
                default:
                    break;
            }

            if (steps[stepIndex].locType == LocationType.Point)
            {
                GlobalDataManager.instance.WorldInfoInstance.LoadedWorlds
                    [steps[stepIndex].worldName].WorldStorage[steps[stepIndex].gridX][steps[stepIndex].gridY].OnGridSpaceEnter -= CheckDestinationReached;
                GlobalDataManager.instance.WorldInfoInstance.LoadedWorlds
                    [steps[stepIndex].worldName].WorldStorage[steps[stepIndex].gridX][steps[stepIndex].gridY].OnGridSpaceEnter += CheckDestinationReached;
            }

            if (steps[stepIndex].timeLimit != null) GlobalDataManager.instance.ElapsedTime.OnDateChanged += CheckTimeLimit;

            if (!steps[stepIndex].stepStarted) //Only do this stuff the first time
            {
                if (steps[stepIndex].locType == LocationType.City) AddContextButton();
                if (steps[stepIndex].timeLimit != null) SetTimeLimit();

                if (steps[stepIndex].locType == LocationType.NewEncounter) //Set up a new encounter zone
                {
                    CreateTempEncounter();
                }
                steps[stepIndex].stepStarted = true;
            }
            QuestSystem.instance.AdvanceQuest(ID);
        }

        /// <summary>
        /// Checks the inventory for the required amount of the required item when the inventory contents are updated.
        /// </summary>
        public void CheckInventory()
        {
            steps[stepIndex].currentNum = Inventory.instance.CheckItemQuantity(steps[stepIndex].target);
            bool enough = (steps[stepIndex].currentNum >= steps[stepIndex].number);
            GlobalDataManager.instance.FlagManagerInstance.SetFlag($"{ID}-{stepIndex}.fetch", enough);

            DialogueWindow.instance.OnDialogueEnd -= CheckDialogueEnd;
            if (enough) DialogueWindow.instance.OnDialogueEnd += CheckDialogueEnd;
        }

        /// <summary>
        /// Checks the number of enemies of the specified type slain
        /// </summary>
        public void CheckEnemiesSlain()
        {
            //GlobalFlagManager.instance.FlagManagerInstance.SetFlag($"{ID}-{stepIndex}.kill", //ON HOLD UNTIL THERE IS COMBAT)
        }

        /// <summary>
        /// Checks if the required dialogue has been completed.
        /// </summary>
        public void CheckDialogueEnd()
        {
            string[] strings = steps[stepIndex].dialogueRef.Split('#');
            GlobalDataManager.instance.FlagManagerInstance.SetFlag($"{ID}-{stepIndex}.talk", (DialogueWindow.instance.currentDialogue.path == strings[1]));
        }

        /// <summary>
        /// Activates when the desired point is reached
        /// </summary>
        /// <param name="space">This is unnecessary, it is just passed by the event.</param>
        public void CheckDestinationReached(BMGridSpace space)
        {
            GlobalDataManager.instance.FlagManagerInstance.SetFlag($"{ID}-{stepIndex}.move", true);
            if (steps[stepIndex].dialogueRef != default(string))
            {
                GlobalUIController.instance.DialogueWindowActive = true;
                GlobalUIController.instance.DialogueWindow.ReadyNextMessage(steps[stepIndex].dialogueRef, false);
            }
        }

        /// <summary>
        /// Event function call for flag type quest
        /// </summary>
        public void CheckFlagSet((string Key, bool Value) flag)
        {
            if (CheckFailFlags())
            {
                FailQuest();
            }
            if (CheckFlags())
            {
                CompleteStep();
            }
        }

        /// <summary>
        /// If there is a time limit defined, this will end the quest if the time limit expires.
        /// </summary>
        public void CheckTimeLimit()
        {
            if (steps[stepIndex].timeLimit > GlobalDataManager.instance.ElapsedTime)
                steps[stepIndex].remainingTime = steps[stepIndex].timeLimit - GlobalDataManager.instance.ElapsedTime;
            else FailQuest();
        }

        /// <summary>
        /// Increases the step index and calls CompleteQuest if this was the last step.
        /// </summary>
        public bool CompleteStep()
        {
            if (category == QuestCategory.Failed) FailQuest();
            if (CheckFlags())
            {
                if (steps[stepIndex].locType == LocationType.City) RemoveContextButton();
                if (steps[stepIndex].type == QuestType.Fetch) Inventory.instance.RemoveItem(steps[stepIndex].target, steps[stepIndex].number);

                if (stepIndex + 1 >= steps.Length)
                {
                    CompleteQuest();
                    return true;
                }
                stepIndex++;
                AdvanceQuest();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Sets the quest to complete and puts the specified rewards into the inventory.
        /// </summary>
        private void CompleteQuest()
        {
            UnsubscribeAll();
            isComplete = true;
            GlobalUIController.instance.QuestMenu.ChangeQuestState(ID, isRandom ? QuestCategory.None : QuestCategory.Complete); //Delete if random

            if (rewards != null)
            {
                Dictionary<string, int> dic = new Dictionary<string, int>();
                foreach (QuestReward reward in rewards) //Allocate rewards based on flags
                {
                    bool available = true;
                    if (reward.flagsNeeded != null)
                    {
                        foreach (KeyValuePair<string, bool> pair in reward.flagsNeeded)
                        {
                            if (!GlobalDataManager.instance.FlagManagerInstance.GetFlag(pair.Key))
                            {
                                available = false;
                                break;
                            }
                        }
                    }
                    if (available)
                    {
                        if (dic.ContainsKey(reward.item)) dic[reward.item] += reward.number;
                        else dic.Add(reward.item, reward.number);
                    }
                }

                foreach (KeyValuePair<string, int> pair in dic) //Add rewards to the inventory
                {
                    Item toAdd = ItemDatabase.ItemDictionary[pair.Key];
                    Inventory.instance.AddItem(toAdd, pair.Value);
                }
            }
            QuestSystem.instance.EndQuest(ID);
        }

        /// <summary>
        /// Fails the quest, making it impossible to continue tracking steps.
        /// </summary>
        public void FailQuest()
        {
            UnsubscribeAll();
            isComplete = true;
            if (steps[stepIndex].type == QuestType.Fetch || steps[stepIndex].type == QuestType.Talk) RemoveContextButton();
            //category = QuestCategory.Failed;
            QuestSystem.instance.EndQuest(ID);
            GlobalUIController.instance.QuestMenu.ChangeQuestState(ID, isRandom ? QuestCategory.None : QuestCategory.Failed); //Delete if random
        }

        /// <summary>
        /// Checks the flags required to complete the step.
        /// </summary>
        /// <returns>Whether all flags have been set to true.</returns>
        private bool CheckFlags()
        {
            bool complete = true;
            foreach (string flag in steps[stepIndex].FlagsNeeded)
            {
                if (!GlobalDataManager.instance.FlagManagerInstance.GetFlag(flag))
                {
                    complete = false;
                    break;
                }
            }
            return complete;
        }

        /// <summary>
        /// Checks the flags required to fail the quest.
        /// </summary>
        /// <returns>True if the fail conditions have been met.</returns>
        private bool CheckFailFlags()
        {
            if (steps[stepIndex].failFlags == null) return false; //Return if no fail conditions

            bool safe = true;
            foreach (string flag in steps[stepIndex].failFlags)
            {
                if (!GlobalDataManager.instance.FlagManagerInstance.GetFlag(flag))
                {
                    safe = false;
                    break;
                }
            }
            return safe;
        }

        /// <summary>
        /// Unsubscribes the quest from all events, just to be safe.
        /// </summary>
        private void UnsubscribeAll()
        {
            Inventory.instance.OnInventoryContentsChange -= CheckInventory;
            DialogueWindow.instance.OnDialogueEnd -= CheckDialogueEnd;
            GlobalDataManager.instance.ElapsedTime.OnDateChanged -= CheckTimeLimit;
            GlobalDataManager.instance.FlagManagerInstance.OnFlagsUpdate -= CheckFlagSet;

            if (steps[stepIndex].locVicinity == LocationVicinity.None) return;
            GlobalDataManager.instance.WorldInfoInstance.LoadedWorlds
                        [steps[stepIndex].worldName].WorldStorage[steps[stepIndex].gridX][steps[stepIndex].gridY].OnGridSpaceEnter -= CheckDestinationReached;
        }

        /// <summary>
        /// Adds a context button to a city that opens the dialogue
        /// </summary>
        private void AddContextButton()
        {
            ContextButtonInfo info = new ContextButtonInfo();

            City city = GlobalDataManager.instance.WorldInfoInstance.LoadedWorlds[steps[stepIndex].worldName].Cities[steps[stepIndex].locRef];
            info.ActionType = ContextActionTypes.StartDialogue;

            if (steps[stepIndex].contextButtonText == default(string)) steps[stepIndex].contextButtonText = $"Talk to {client}";
            info.ButtonText = steps[stepIndex].contextButtonText;

            if (steps[stepIndex].type == QuestType.Fetch) //Don't show the button if the player hasn't collected sufficient items
            {
                info.RequiredFlagsForVisible = new List<string>();
                info.RequiredFlagsForVisible.Add($"{ID}-{stepIndex}.fetch");
            }
            info.DialoguePath = steps[stepIndex].dialogueRef;

            city.ContextButtons.Insert(city.ContextButtons.Count - 1, info);
        }

        private void RemoveContextButton()
        {
            City city = GlobalDataManager.instance.WorldInfoInstance.LoadedWorlds[steps[stepIndex].worldName].Cities[steps[stepIndex].locRef];
            foreach (ContextButtonInfo button in city.ContextButtons)
            {
                if (button.ButtonText == steps[stepIndex].contextButtonText)
                {
                    city.ContextButtons.Remove(button);
                    break;
                }
            }
        }

        /// <summary>
        /// Sets up a new encounter zone.
        /// </summary>
        private void CreateTempEncounter()
        {
            ZoneGrid grid = GlobalDataManager.instance.WorldInfoInstance.LoadedWorlds[steps[stepIndex].worldName];
            EncounterArea area = grid.EncounterAreas[0];

            grid.WorldStorage[steps[stepIndex].gridX][steps[stepIndex].gridY].CurrentSpaceTags.Add(GridSpaceTags.ContainsStructure);
            area.XCoord = steps[stepIndex].gridX;
            area.YCoord = steps[stepIndex].gridY;
            area.LocalSeed = new Random(WorldGov.instance.GlobalRandomSeed).Next();
            grid.EncounterAreas.Add(area);
            steps[stepIndex].locRef = grid.EncounterAreas.Count - 1;
            area.EmplacementBehavior(grid);
            area.ReadyBehavior(grid);
        }
    }

    public enum LocationVicinity
    {
        None = 0,      //None means that there is no specified location. Most fetch/kill quests will use this.
        PreviousQuest, //Use the same location as a previous quest. Specify step index as the locationArg and quest ID as the previousID.
        PreviousStep,  //Use the same location as a previous step. Specify step index as the locationArg.
        Local,         //Is on the local planet/region
        System,        //Is on a different planet than the one the player is on
        Custom,        //If you use custom, make sure to set the location data manually 
    }

    public enum LocationType
    {
        None = 0, //None means that there is no specified location. Most fetch/kill quests will use this.
        City,
        Encounter,
        NewEncounter, //Make a new temporary encounter for the duration of the quest. Will generate the same as a point.
        Point,
        Custom, //If you use custom, make sure to set the location data manually
    }

    public class QuestStep
    {
        public QuestType type { get; set; }
        public string description { get; set; }
        public string target { get; set; }
        public int number { get; set; } //Number of things required for fetch & kill quests.
        public int currentNum { get; set; } //This should be left blank when creating quests
        public string contextButtonText { get; set; } //This will override the text on a context button if you don't want it to say "Talk to (target)". Optional.

        /// <summary>
        /// Reference to the dialogue completing this step should open, if applicable. Remember to write in "file#(filepath)#(message name)" format.
        /// </summary>
        /// <value></value>
        public string dialogueRef { get; set; }
        public bool stepStarted { get; set; } //Has this step already been started? Used to make sure stuff doesn't generate twice.
        public List<string> flagsNeeded { get; set; } //List of flags needed, will auto-generate, but additional ones can be set if needed
        [JsonIgnore]
        public List<string> FlagsNeeded
        {
            get
            {
                if (flagsNeeded == null) flagsNeeded = new List<string>();
                return flagsNeeded;
            }
            set { flagsNeeded = value; }
        }

        public List<string> failFlags { get; set; } //List of flags that will fail the quest when true.

        public LocationVicinity locVicinity { get; set; } //Vicinity of location, used to determine world location
        public LocationType locType { get; set; } //Type of location, used to determine specific location data
        public int previousID { get; set; } //ID of a previous quest to reuse a location when vicinity is set to previous.
        public int previousStepNum { get; set; } //Number of the previous step to use when vicinity is set to previous. Zero-indexed.
        public int seedOffset { get; set; } //Optional argument for an argument in generation. Can be used to offset the random seed if you want more control over the RNG.
        public string worldName { get; set; } //Name of the world, used for accessing world data. Set at generation.
        public string locDesc { get; set; } //Description of the location, i.e. (City) on (Planet). Set at generation, but can be pre-set.
        public int locRef { get; set; } //Index of the world feature used in the step
        public int gridX { get; set; }
        public int gridY { get; set; }
        public string tempEncounterPath { get; set; } //Path to the encounter file you want to add to the map, leave blank for random.

        public Date timeLimit { get; set; } //Amount of time allowed to complete the step; optional.
        public Date remainingTime { get; set; } //Time left in the quest step, calculated at run time.
    }

    public class QuestReward
    {
        public string item { get; set; }
        public int number { get; set; }
        public Dictionary<string, bool> flagsNeeded { get; set; }
    }
}
