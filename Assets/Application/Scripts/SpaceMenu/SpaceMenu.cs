using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Space Menu is a singleton that manages the Space Menu UI.
/// </summary>
public class SpaceMenu : MonoSingleton<SpaceMenu>
{
    private bool isOn;
    private CanvasGroup canvasGroup;
    private Canvas canvas;

    private void Start()
    {
        Init();
    }

    private void Init()
    {
       GetData<EnvironmentDataEntry>();
       canvas = GetComponentInChildren<Canvas>();
       canvasGroup = canvas.GetComponentInChildren<CanvasGroup>();
       MenuOff();
    }
    
    private void GetData<T>() where T : ContentDataEntry
    {
        var eventSystem = new GameObject("EventSystem");
        eventSystem.ForceComponent<EventSystem>();
        eventSystem.ForceComponent<StandaloneInputModule>();

        var buttonPrefab = transform.Search("SpaceButton");
        var parent = transform.Search("Content");

        ContentManager.Instance.GetData(list =>
        {
            foreach (var dataEntry in list)
            {
                var button = Instantiate(buttonPrefab,parent).gameObject;
                button.SetActive(true);
                var spaceButton = button.ForceComponent<SpaceButton>();
                spaceButton.Init(dataEntry);
            }
        }, new List<Type>() {typeof(T)});
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.M))
        {
            if (isOn)
            {
                MenuOff();
            }
            else
            {
                MenuOn();
            }
        }

        if (Input.GetKeyUp(KeyCode.C))
        {
            ContentManager.Instance.ClearData();
            Debug.Log("Cleared ContentManager Data Cache!");
        }
    }

    private void MenuOn()
    {
        isOn = true;
        canvas.SetActive(true);
        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        transform.position = Camera.main.transform.position + Camera.main.transform.forward * 1.1f;
        transform.rotation = Camera.main.transform.rotation;
    }

    /// <summary> The MenuOff function turns off the menu.</summary>
    public void MenuOff()
    {
        isOn = false;
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        canvas.SetActive(false);
    }
}
