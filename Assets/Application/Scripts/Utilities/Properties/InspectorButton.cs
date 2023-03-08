using System;
using UnityEngine;

[Serializable]
public class InspectorButton
{
	#region Properties

	#region Colours

	public static readonly Color32 DEFAULT_COLOUR = Color.white;
	public static readonly Color32 PRESET_ON_COLOUR = new Color32(181, 230, 29, 255);
	public static readonly Color32 PRESET_OFF_COLOUR = new Color32(238, 205, 125, 255);
	public static readonly Color32 PRESET_CTA_COLOUR = new Color32(153, 217, 234, 255);

	#endregion Colours

	public string buttonText = "Please Specify In Constructor";
	public Color32 buttonColour = DEFAULT_COLOUR;
	private Action callback;

    #endregion Properties

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param label="buttonText">The Text to be displayed on the button</param>
    public InspectorButton(string buttonText)
	{
		this.buttonText = buttonText;
	}

	/// <summary>
	/// Constructor
	/// </summary>
	/// <param label="buttonText">The Text to be displayed on the button</param>
	public InspectorButton(string buttonText, Color32 buttonColour)
	{
		this.buttonText = buttonText;
		this.buttonColour = buttonColour;
	}

	public void Assign(Action Callback)
	{
		this.callback = Callback;
	}

    public void ButtonPress()
	{
		if (callback == null)
			Debug.LogError("No callback function has been assigned. Please do this in \"OnValidate\"");
		else callback();
	}
}