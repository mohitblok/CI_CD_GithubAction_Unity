//TODO: DAVE: Refactor to Bloktopia Style Guide

using System;
using UnityEngine;

[Serializable]
public class InspectorNote
{
    public string noteText = "Please Specify text In Constructor";
#pragma warning disable CS0414 // Value is used in Property Drawer
    [SerializeField] private bool useCustomColour;
#pragma warning restore CS0414 // Value is used in Property Drawer
    [SerializeField] private Color noteColour = Color.white;
    public readonly static Color32 COLOUR_GOOD = new Color32(34, 177, 76, 255);
    public readonly static Color32 COLOUR_BAD = Color.red;
    public readonly static Color32 COLOUR_DEFAULT = Color.black;
    public readonly static Color32 COLOUR_WARNING = new Color32(236, 255, 38, 255);

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param label="buttonText">The text to be displayed in the note</param>
    /// /// <param label="noteColour">Custom text colour</param>
    public InspectorNote(string noteText, Color noteColour)
    {
        this.noteText = noteText;
        this.noteColour = noteColour;
        useCustomColour = true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param label="noteText"></param>
    /// <param label="noteColour"></param>
    public InspectorNote(string noteText)
    {
        this.noteText = noteText;
        useCustomColour = false;
    }

    public void UseCustomColour(Color noteColour)
    {
        this.noteColour = noteColour;
        useCustomColour = true;
    }

    public void UseDefaultColour()
    {
        useCustomColour = false;
    }
}