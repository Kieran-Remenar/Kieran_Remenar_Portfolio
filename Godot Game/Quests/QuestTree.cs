using Godot;
using System.Collections.Generic;
using BMQuestSystem;

public class QuestTree : Tree
{
    private QuestManager manager;
    public QuestManager Manager {get {return manager;} set {manager = value;}}

    private TreeItem root;
    private TreeItem[] sections = new TreeItem[4];
    private Dictionary<string, TreeItem> subsections = new Dictionary<string, TreeItem>();
    private Dictionary<int, TreeItem> quests = new Dictionary<int, TreeItem>();

    public enum Sections
    {
        NONE = 0,
        MAIN,
        SIDE,
        COMPLETE,
        FAILED
    }

    private void GenerateTreeBase()
    {
        Columns = 2;
        SetColumnMinWidth(0, 50);
        SetColumnMinWidth(1, 10);
        SetColumnExpand(0, true);
        root = CreateItem();
        HideRoot = true;

        for (int i = 0; i < sections.Length; i++)
        {
            sections[i] = CreateItem(root);
            sections[i].SetSelectable(0, false);
            sections[i].SetSelectable(1, false);
        }
        sections[0].SetText(0, "MAIN");
        sections[1].SetText(0, "SIDE");
        sections[2].SetText(0, "COMPLETED");
        sections[3].SetText(0, "FAILED");
    }

    /// <summary>
    /// Clears the tree, then generates the basic structure again.
    /// </summary>
    public void ResetTree()
    {
        Clear();
        quests.Clear();
        subsections.Clear();
        sections = new TreeItem[4];
        GenerateTreeBase();
    }

    /// <summary>
    /// Adds a given quest to the window.
    /// </summary>
    /// <param name="id">ID of the Quest to add to the window</param>
    /// <param name="isMain">puts the quest into the main category if true</param>
    public void AddQuest(int id)
    {
        Quest quest = manager.masterQuestDictionary[id];
        TreeItem newItem = CreateWithSubcategory(quest);
        newItem.SetText(0, quest.name);
        newItem.SetMetadata(0, id);
        quests.Add(id, newItem);
        if (quest.category == QuestCategory.Main) quest.isActive = true; //Set the new Quest to active if it is a main quest
    }

    /// <summary>
    /// Moves a quest to a determined section
    /// </summary>
    /// <param name="id">ID of the Quest to move to the determined section</param>
    /// <param name="sect">Section to move the Quest into. Pass Sections.NONE to delete the quest.</param>
    public void MoveQuest(int id, QuestCategory sect)
    {
        Quest quest = manager.masterQuestDictionary[id];
        bool leavingSect = false;
        string sectName = "";

        if (quest.subcategory != default(string)) //Is this quest leaving its subsection?
        {
            leavingSect = true;
            sectName = $"{quest.category.ToString()}-{quest.subcategory}";
        }
        if (sect == QuestCategory.None) //Remove and delete the quest if QuestCategory.None is passed
        {
            RemoveQuest(id, leavingSect);
            return;
        }

        quest.category = sect; //Switch the quest's category in the save data too
        quests[id].Free();
        quests.Remove(id);

        if (leavingSect) CheckRemoveSubcategory(sectName);
        AddQuest(id);
    }

    /// <summary>
    /// Removes the quest from the tree and deletes it from the master dictionary. Should only be used on quests that are not planned for keeping, like random ones.
    /// </summary>
    /// <param name="id">ID of the Quest to delete</param>
    public void RemoveQuest(int id, bool leavingSect)
    {
        quests[id].Free();
        quests.Remove(id);
        if (leavingSect) CheckRemoveSubcategory($"{manager.masterQuestDictionary[id].category.ToString()}-{manager.masterQuestDictionary[id].subcategory}");
        Manager.masterQuestDictionary.Remove(id);
    }

    /// <summary>
    /// Creates a new TreeItem with its subcategory already set if applicable.
    /// </summary>
    /// <param name="quest"></param>
    /// <returns></returns>
    private TreeItem CreateWithSubcategory(Quest quest)
    {
        Columns = 2;
        TreeItem newItem;
        if (quest.subcategory != default(string) && quest.category != QuestCategory.Failed) //Adds quest to its specified subcategory, creating it if it does not yet exist.
        {
            string sectName = $"{quest.category.ToString()}-{quest.subcategory}";
            if (!subsections.ContainsKey(sectName))
            {
                TreeItem subsection;
                subsection = CreateItem(sections[(int)quest.category - 1]);
                subsection.SetText(0, quest.subcategory);
                subsection.SetSelectable(0, false);
                subsection.SetSelectable(1, false);
                subsections.Add(sectName, subsection);
            }
            newItem = CreateItem(subsections[sectName]);
        }
        else newItem = CreateItem(sections[(int)quest.category - 1]);
        return newItem;
    }

    /// <summary>
    /// Check if the subcategory has any children, and if not, delete it.
    /// </summary>
    /// <param name="name"></param>
    private void CheckRemoveSubcategory(string name)
    {
        if (subsections[name].GetChildren() == null)
        {
            subsections[name].Free();
            subsections.Remove(name);
        }
    }

    /// <summary>
    /// Shows the quest description of the selected quest
    /// </summary>
    public void SelectQuest()
    {
        if (GetSelected() != null && GetSelected().IsSelected(0)) Manager.ShowDescription((int)GetSelected().GetMetadata(0));
    }
}
