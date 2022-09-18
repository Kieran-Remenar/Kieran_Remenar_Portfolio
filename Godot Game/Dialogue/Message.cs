using Godot;
using System;
using System.Collections.Generic;
using System.Text;
using CharacterUtilities;
using BMStringUtils;

public class Message : Control
{
    private Label nameCard;
    private RichTextLabel messageContents;
    public int MessageSize {get {return messageContents.Text.Length;}}
    private Sprite portrait;

    public override void _Ready()
    {
        nameCard = (Label)GetNode("BG/HBoxContainer/VBoxContainer/Nameplate");
        messageContents = (RichTextLabel)GetNode("BG/HBoxContainer/Content");
        portrait = (Sprite)GetNode("BG/HBoxContainer/VBoxContainer/PortraitContainer/Portrait");
    }

    ///<summary>
    ///Uses a message from json to fill a dialogue card in Godot
    ///</summary>
    ///<param name="messageToUse">Message data</param>
    public void GenerateText(MessageContents messageToUse)
    {
        messageContents.Clear();
        string message, name;
        message = StringParse.Parse(messageToUse.message);

        bool usePortrait = false;

        switch (messageToUse.speaker.ToLower())
        {
        case "<player>":
            name = CharacterDataTools.CharacterStats["Player"].firstName;
            messageContents.AppendBbcode(message);
            nameCard.Text = name;
            messageContents.Raise();
            usePortrait = true;
            break;

        case default(string):
            goto case "<na>";
        case "<na>":
            messageContents.AppendBbcode(message);
            nameCard.Text = " ";
            GetNode<Control>("BG/HBoxContainer/VBoxContainer/PortraitContainer").Hide();
            GetNode<Control>("BG/HBoxContainer/VBoxContainer/Nameplate").Hide();
            messageContents.Raise();
            break;

        default:
            nameCard.Text = messageToUse.speaker;
            message = $"[right]{message}[/right]";
            messageContents.AppendBbcode(message);
            usePortrait = true;
            break;
        }

        if (usePortrait)
        {
            if (messageToUse.portraitPath != default(string)) portrait.Texture = (Texture)GD.Load($"res://Sprites/CharacterPortraits/{messageToUse.portraitPath}.png");
            else portrait.GetParent<Control>().Hide();
        }
    }
}

/// <summary>
/// The class used for formatting JSON info to a message in-game
/// </summary>
public class MessageContents
{
    public string speaker {get; set;}
    public string message {get; set;}

    /// <summary>
    /// The path to the character's portrait, written as {name/filename}
    /// </summary>
    /// <value>string path to the portrait image you want to use</value>
    public string portraitPath {get; set;}

    /// <summary>
    /// Functions to call by string in this message. See list of functions for formatting and parameters.
    /// </summary>
    /// <value></value>
    public string[] functions {get; set;}

    /// <summary>
    /// Flags that seeing this message should set.
    /// </summary>
    /// <value>string flag name, bool value</value>
    public Dictionary<string, bool> flagsToSet {get; set;}

    /// <summary>
    /// The string name of the next message. Use when there are no branching options.
    /// </summary>
    /// <value>string name of the next message.</value>
    public string next {get; set;}

    /// <summary>
    /// Messages this message could automatically advance into.
    /// </summary>
    /// <value></value>
    public AutoMessage[] nextMessages {get; set;}

    /// <summary>
    /// Player choice options.
    /// </summary>
    /// <value></value>
    public DialogueOption[] options {get; set;}
}

/// <summary>
/// Use these to create player choice options.
/// </summary>
public class DialogueOption
{
    public string text {get; set;}

    /// <summary>
    /// Flags needed to see this option. If conditions not met, advances to next messages
    /// </summary>
    /// <value>string flag name, bool value</value>
    public Dictionary<string, bool> flagsNeeded {get; set;}

    /// <summary>
    /// Flags that selecting this option should set.
    /// </summary>
    /// <value>string flag name, bool value</value>
    public Dictionary<string, bool> flagsToSet {get; set;}

    /// <summary>
    /// The string key for the next message; flags unneeded so not an AutoMessage
    /// </summary>
    /// <value></value>
    public string nextMessage {get; set;}
}

/// <summary>
/// Use these to advance the text automatically. Supports branching.
/// </summary>
public class AutoMessage
{
    public string nextMessage {get; set;}
    public int priority {get; set;}
    public Dictionary<string, bool> flagsNeeded {get; set;}
}