using Godot;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Linq;
using BMGridWorld;

namespace BMQuestSystem
{
    public class QuestManager : DragableWindow
    {
        private QuestTree questTree;
        private RichTextLabel descriptionArea;

        public Dictionary<int, Quest> masterQuestDictionary { get { return QuestSystem.instance.QuestDictionary; } }

        public override void _Ready()
        {
            questTree = GetNode<QuestTree>("PanelContainer/HBoxContainer/ScrollContainer/QuestTree");
            questTree.Manager = this;
            descriptionArea = GetNode<RichTextLabel>("PanelContainer/HBoxContainer/DescriptionArea/RichTextLabel");
            QuestSystem.instance.UILoaded();

            QuestSystem.instance.OnQuestStart += ActivateQuest;
        }

        /// <summary>
        /// Adds a quest in the master dictionary to the quest window.
        /// </summary>
        /// <param name="id">ID of the quest to activate</param>
        public void ActivateQuest(int id)
        {
            questTree.AddQuest(id);
        }

        /// <summary>
        /// Formats and shows the full quest description on the right side area of the quest window
        /// </summary>
        /// <param name="questID">ID of the quest to show</param>
        public void ShowDescription(int id)
        {
            StringBuilder sb = new StringBuilder();
            descriptionArea.Clear();
            Quest quest = masterQuestDictionary[id];

            //Title, centered and bold
            sb.Append($"[b][center]{quest.name}[/center][/b]\n\n[color=#cccccc]");
            //Client
            if (quest.client != default(string)) sb.Append($"Client: {quest.client}\n");
            if (!quest.isComplete) //Don't show this stuff when the quest is complete
            {   //Completion Requirements
                switch (quest.steps[quest.stepIndex].type)
                {
                    case QuestType.Fetch:
                        sb.Append($"Collect {quest.steps[quest.stepIndex].target}: {quest.steps[quest.stepIndex].currentNum}/{quest.steps[quest.stepIndex].number}\n");
                        break;
                    case QuestType.Kill:
                        sb.Append($"Defeat {quest.steps[quest.stepIndex].target}: {quest.steps[quest.stepIndex].currentNum}/{quest.steps[quest.stepIndex].number}\n");
                        break;
                    case QuestType.Move:
                        break;
                    case QuestType.Talk:
                        sb.Append($"Speak with {quest.steps[quest.stepIndex].target}\n");
                        break;
                    default:
                        GD.Print("No Quest Type specified for this step");
                        break;
                }
                //Location
                if (quest.steps[quest.stepIndex].locDesc != default(string) && quest.steps[quest.stepIndex].locType != LocationType.None)
                    sb.Append(GetLocationText(quest.steps[quest.stepIndex]) + "\n");
                //Remaining time
                if (quest.steps[quest.stepIndex].timeLimit != null)
                {
                    Date time = quest.steps[quest.stepIndex].remainingTime;
                    sb.Append("Time remaining:");
                    if (time.Years > 0)
                    {
                        sb.Append($" {time.Years} years");
                        if (time.Months > 0 || time.Days > 0 || time.Hours > 0) sb.Append(",");
                    }
                    if (time.Months > 0)
                    {
                        sb.Append($" {time.Months} months");
                        if (time.Days > 0 || time.Hours > 0) sb.Append(",");
                    }
                    if (time.Days > 0)
                    {
                        sb.Append($" {time.Days} days");
                        if (time.Hours > 0) sb.Append(",");
                    }
                    if (time.Hours > 0) sb.Append($" {time.Hours} hours");
                    sb.Append("\n");
                }
            }
            sb.Append("[/color]\n");
            //Quest description by step
            if (quest.stepIndex > 0)
            {
                sb.Append("[color=#aaaaaa]");
                for (int i = 0; i < quest.stepIndex; i++) //Write previous steps in darker grey
                {
                    sb.Append($"{quest.steps[i].description}\n\n");
                }
                sb.Append("[/color]");
            }
            sb.Append($"[color=#cccccc]{quest.steps[quest.stepIndex].description}[/color]");
            descriptionArea.AppendBbcode(sb.ToString());
        }

        /// <summary>
        /// Shows the text for and distance to a location
        /// </summary>
        /// <param name="step">The current step of the quest to show</param>
        /// <returns>A formatted string of the location</returns>
        private string GetLocationText(QuestStep step)
        {
            string output, location = "";
            int xoffset, yoffset;
            output = $"Go to {step.locDesc} ";
            xoffset = step.gridX - WorldGov.instance.ActivePlayer.CurrentGridPosition.x;
            yoffset = step.gridY - WorldGov.instance.ActivePlayer.CurrentGridPosition.y;

            if (xoffset > 0) location += $"{Mathf.Abs(xoffset)}E";
            else if (xoffset < 0) location += $"{Mathf.Abs(xoffset)}W";

            if (xoffset != 0 && yoffset != 0) location += ", ";

            if (yoffset > 0) location += $"{Mathf.Abs(yoffset)}N";
            else if (yoffset < 0) location += $"{Mathf.Abs(yoffset)}S";

            if (xoffset == 0 && yoffset == 0) location = "(current location)";
            return output + location;
        }

        /// <summary>
        /// Updates the quest in the quest tree. Setting sect to NONE will permanently delete the quest.
        /// </summary>
        /// <param name="id">ID of the quest to move</param>
        /// <param name="sect">Section to move the quest to</param>
        public void ChangeQuestState(int id, QuestCategory sect)
        {
            questTree.MoveQuest(id, sect);
        }

        /// <summary>
        /// Refreshes the quest window's description field
        /// </summary>
        public void RefreshTree()
        {
            questTree.SelectQuest();
        }

        /// <summary>
        /// Generates the Quest Tree UI
        /// </summary>
        public void BuildQuestTree()
        {
            questTree.ResetTree();
            foreach (KeyValuePair<int, Quest> pair in masterQuestDictionary)
            {
                if (pair.Value.isKnown)
                {
                    questTree.AddQuest(pair.Key);
                }
            }
        }

        public override void CloseWindow()
        {
            GlobalUIController.instance.QuestMenuActive = false;
        }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
    }
}
