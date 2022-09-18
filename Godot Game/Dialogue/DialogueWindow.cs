using Godot;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using CharacterUtilities;
using BMStringUtils;

public class DialogueWindow : Control
{
    [Export]
    public PackedScene messageObj;
    [Export]
    public PackedScene buttonObj;
    [Export]
    public int maxStoredMessages = 50;
    [Export]
    public bool autoEnabled = true;
    [Export]
    public float autoTimerScale = 1f;
    public static DialogueWindow instance;
    public override void _ExitTree() //FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF - Ben
    {
        instance = null;
    }

    private Dialogue m_currentDialogue;
    public Dialogue currentDialogue
    {
        get { return m_currentDialogue; }
        set { m_currentDialogue = value; }
    }

    private VBoxContainer messageParent;
    private HBoxContainer choiceParent;
    [Export]
    public NodePath scrollPath;
    private ScrollContainer scroll;
    private VScrollBar scroller;
    private Button closeButton;
    private double scrollerMax = 0.0f;
    private Queue<Node> messageQueue;
    private List<DialogueButton> buttonList;
    private string nextMessage;
    private bool isTimerActive = false;
    private bool awaitingInput = false; //Is the dialogue progression suspended until the player does something?
    private int messageSize;
    private float timeToNext, timeCounter;

    public event Action OnDialogueStart;
    public event Action OnDialogueEnd;

    public override void _Ready()
    {
        _Init(); //Should be called from a proper scene loader eventually
        //ReadyNextMessage("file#res://GenerationAssets/DialogueFiles/DialogueTestSimple.json#start", false);
    }

    /// <summary>
    /// Initializes the Dialogue Window. Call when done loading a scene.
    /// </summary>
    public void _Init()
    {
        if (instance == null) instance = this;

        currentDialogue = new Dialogue();
        messageQueue = new Queue<Node>();
        buttonList = new List<DialogueButton>();

        CharacterDataTools.FetchCharStats();
        messageParent = GetNode<VBoxContainer>("VBoxContainer/DialogueRegion/ScrollContainer/VBoxContainer");
        choiceParent = GetNode<HBoxContainer>("VBoxContainer/ChoiceRegion/HBoxContainer");

        closeButton = GetNode<Button>("VBoxContainer/ChoiceRegion/HBoxContainer/CloseButton");
        closeButton.Connect("pressed", this, "CloseWindow");
        closeButton.Hide();

        scroll = GetNode<ScrollContainer>(scrollPath);
        //scroll = GetNode<ScrollContainer>("VBoxContainer/DialogueRegion/ScrollContainer"); //Connect the scrollbar size change event to move it to the bottom when a new message begins
        scroller = scroll.GetVScrollbar();
        scroller.Connect("changed", this, "OnScrollbarChanged");
        scrollerMax = scroller.MaxValue;
        autoTimerScale *= 0.15f; //Tried to get this to a good point for a scale value of 1

        doUpdateNextFrame = false;
    }

    public bool doUpdateNextFrame;
    public bool advanceNextFrame;

    public override void _Process(float delta)
    {
        if (doUpdateNextFrame) { scroller.Value = scroller.Value - 1; doUpdateNextFrame = false; }
        if (advanceNextFrame) { AdvanceDialogue(); advanceNextFrame = false; }

        if (autoEnabled && isTimerActive)
        {
            if (timeCounter >= timeToNext)
            {
                isTimerActive = false;
                timeCounter = 0.0f;
                AdvanceDialogue();
                //OnScrollbarChanged();
                doUpdateNextFrame = true;
            }
            else timeCounter += delta;
        }

        if (Input.IsActionPressed("ui_up"))
        {
            scroller.Value -= 10;
        }
        if (Input.IsActionPressed("ui_down"))
        {
            scroller.Value += 10;
        }

        if (awaitingInput)
        {
            if (Input.IsActionJustPressed("dialogue_advance"))
            {
                if (buttonList.Count == 0)
                {
                    isTimerActive = false;
                    timeCounter = 0.0f;
                    AdvanceDialogue();
                    //OnScrollbarChanged();
                    doUpdateNextFrame = true;
                }
            }
        }
    }

    /// <summary>
    /// Shows the next message, sets up any flags, and sets up dialogue options or triggers the next message.
    /// </summary>
    public void AdvanceDialogue()
    {
        awaitingInput = false;
        MessageContents currentMessage = currentDialogue.dialogue[nextMessage];
        if (messageQueue.Count > maxStoredMessages) messageQueue.Dequeue().QueueFree(); //Dequeue old messages if there's a lot
        if (buttonList.Count > 0)
        {
            for (int i = 0; i < buttonList.Count; i++)
            {
                buttonList[i].QueueFree();
            }
            buttonList.Clear(); //Clear any choice buttons when a new message is added
        }
        var newMessage = (Message)messageObj.Instance(); //Spawn a new message and move it into place
        messageParent.AddChild(newMessage);
        messageQueue.Enqueue(newMessage);
        newMessage.GenerateText(currentMessage); //Set the text
        messageSize = newMessage.MessageSize;

        if (currentMessage.flagsToSet != null) //Set any flags, if necessary
        {
            foreach (KeyValuePair<string, bool> entry in currentMessage.flagsToSet)
            {
                GlobalDataManager.instance.FlagManagerInstance.SetFlag(entry.Key, entry.Value);
            }
        }

        if (currentMessage.functions != null) //Call any specified functions
        {
            for (int i = 0; i < currentMessage.functions.Length; i++)
            {
                StringToFunc.HandleFunction(currentMessage.functions[i]);
            }
        }

        if (currentMessage.next != default(string)) //Queue the next message if next has a value
        {
            ReadyNextMessage(currentMessage.next, true);
            return;
        }

        if (currentMessage.nextMessages != null) //Set the next message to go use
        {
            if (currentMessage.nextMessages.Length > 0) //If there's only one option, go with it
            {
                string strToUse = currentMessage.nextMessages[0].nextMessage;
                int highPriority = -1;
                foreach (AutoMessage entry in currentMessage.nextMessages) //Use a priority system to select the highest priority, available option
                {
                    if (entry.priority > highPriority)
                    {
                        bool available = true;
                        if (entry.flagsNeeded != null)
                        {
                            foreach (KeyValuePair<string, bool> pair in entry.flagsNeeded)
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
                            strToUse = entry.nextMessage;
                            highPriority = entry.priority;
                        }
                    }
                }
                nextMessage = strToUse;
            }
            else nextMessage = currentMessage.nextMessages[0].nextMessage;

            ReadyNextMessage(nextMessage, true);
            return;
        }
        else if (currentMessage.options != null)
        {
            foreach (DialogueOption choice in currentMessage.options) //Add all available choices to the scene as buttons
            {
                bool available = true;
                if (choice.flagsNeeded != null)
                {
                    foreach (KeyValuePair<string, bool> pair in choice.flagsNeeded)
                    {
                        if (!GlobalDataManager.instance.FlagManagerInstance.GetFlag(pair.Key))
                        {
                            available = false;
                            break;
                        }
                    }
                }
                if (available) //Instantiate buttons and give them the necessary information to set up
                {
                    var newButton = (DialogueButton)buttonObj.Instance();
                    choiceParent.AddChild(newButton);
                    newButton.Init(choice);
                    buttonList.Add(newButton);
                }
            }
            buttonList[0].Focus();
            awaitingInput = true;
        }
        else
        {
            EndDialogue();
        }
    }

    /// <summary>
    /// Ready the next message to be shown.
    /// </summary>
    /// <param name="messageName">The name of the next message. See dialogue formatting document to start in a new document.</param>
    /// <param name="startingTimer">Will start the timer and/or await input if true. If false, will advance the dialogue immediately.</param>
    public void ReadyNextMessage(string messageName, bool startingTimer)
    {
        closeButton.Hide();
        nextMessage = messageName;
        if (nextMessage.ToLower() == "<end>") //End dialogue if the next message name is <end>
        {
            EndDialogue();
            return;
        }

        if (nextMessage.Contains("file#")) //Check for markdown to go to a different file and set that as the active dialogue
        {
            string[] splits = nextMessage.Split('#');
            GetDialogueFromFile(splits[1]);
            nextMessage = splits[2];
            OnDialogueStart?.Invoke();
        }

        if (startingTimer)
        {
            if (autoEnabled)
            {
                timeToNext = messageSize * autoTimerScale;
                isTimerActive = true;
                GD.Print($"Waiting for {timeToNext} seconds.");
            }
            awaitingInput = true;
        }
        else
        {
            //AdvanceDialogue();
            advanceNextFrame = true;
        }
    }

    public void EndDialogue()
    {
        awaitingInput = false;
        if (buttonList.Count > 0)
        {
            for (int i = 0; i < buttonList.Count; i++)
            {
                buttonList[i].QueueFree();
            }
            buttonList.Clear(); //Clear any choice buttons when the dialogue ends
        }

        closeButton.Show();
        closeButton.GrabFocus();
    }

    public void GetDialogueFromFile(string filepath)
    {
        var file = new File();
        file.Open(filepath, File.ModeFlags.Read);
        string objAsString = file.GetAsText();
        currentDialogue = JsonSerializer.Deserialize<Dialogue>(objAsString);
        currentDialogue.path = filepath;
    }

    public void OnScrollbarChanged()
    {
        //if (!scroller.Visible) { scroller.Visible = true; }
        if (scrollerMax != scroller.MaxValue)
        {
            scroll.ScrollVertical = (int)scroller.MaxValue;
            scrollerMax = scroller.MaxValue;
        }
        //GD.Print("On Scrollbar Changed Called");
    }

    public void CloseWindow()
    {
        while (messageQueue.Count > 0)
        {
            messageQueue.Dequeue().QueueFree();
        }
        OnDialogueEnd?.Invoke();
        GlobalUIController.instance.DialogueWindowActive = false;
    }
}

public class Dialogue
{
    private Dictionary<string, MessageContents> m_dialogue;
    public Dictionary<string, MessageContents> dialogue
    {
        get { return m_dialogue; }
        set { m_dialogue = value; }
    }

    [JsonIgnore]
    public string path;
}
