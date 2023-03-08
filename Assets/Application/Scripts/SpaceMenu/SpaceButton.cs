using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI Button for the Space Menu.
/// </summary>
public class SpaceButton : MonoBehaviour
{
    private Button button;
    private TMP_Text text;
    private string guid;
    
    /// <summary> The Init function is called when the object is created.</summary>
    /// <param name="dataEntry"> The data entry to be displayed.</param>
    public void Init(ContentDataEntry dataEntry)
    {
        GetComponents();
        text.text = dataEntry.name;
        guid = dataEntry.guid;
        AddButtonListeners();
    }

    private void GetComponents()
    {
        text = GetComponentInChildren<TMP_Text>();
        button = gameObject.GetComponent<Button>();
    }
    
    private void AddButtonListeners()
    {
        button.onClick.AddListener(() =>
        {
            LobbyUI.Instance.Open(guid);
        });
    }
}