using Godot;
using System;
using System.Collections.Generic;

public class DialogueButton : Control
{
    private string labelText;
    private string nextMessage;
    private Label text;
    private Button button;

    public override void _Ready()
    {
        text = GetNode<Label>("Button/Label");
        button = GetNode<Button>("Button");
        button.Connect("pressed", this, "OnButtonPressed");
    }

    public void Init(DialogueOption option)
    {
        labelText = option.text;
        text.Text = labelText;
        nextMessage = option.nextMessage;
        if (option.flagsToSet != null)
        {
            foreach (KeyValuePair<string, bool> pair in option.flagsToSet)
            {
                GlobalDataManager.instance.FlagManagerInstance.SetFlag(pair.Key, pair.Value);
            }
        }
    }

    public void Focus()
    {
        button.GrabFocus();
    }

    private void OnButtonPressed()
    {
        DialogueWindow.instance.ReadyNextMessage(nextMessage, false);
        DialogueWindow.instance.doUpdateNextFrame = true;
    }
}
