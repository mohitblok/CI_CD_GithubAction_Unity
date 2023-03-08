using System.Collections.Generic;
using Bloktopia;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

/// <summary>
/// LobbyUI is a singleton that manages the lobby UI.
/// </summary>
public class LobbyUI : MonoSingleton<LobbyUI>
{
    private State state;

    private GameObject[] views;

    private GameObject publicBtn;
    private GameObject publicContent;
    private GameObject privateInputField;
    private FusionNetworkManager.NetworkGroupData networkGroupData = new FusionNetworkManager.NetworkGroupData();

    EnvironmentDataEntry environmentDataEntry;
    private List<SessionInfo> sessionInfos = new List<SessionInfo>();

    /// <summary>
    ///  UI States
    /// </summary>
    public enum State
    {
        None = -1,
        Main = 0,
        Public = 1,
        Private = 2,
    }

    protected override void Awake()
    {
        base.Awake();

        GetViews();
        GetComponents();
        SetupButtonEvents();
        gameObject.SetActive(false);
    }

    private void GetViews()
    {
        views = new GameObject[3];
        views[0] = transform.Search("Main").gameObject;
        views[1] = transform.Search("Public").gameObject;
        views[2] = transform.Search("Private").gameObject;
    }

    private void GetComponents()
    {
        publicBtn = views[(int)State.Public].transform.Search("Public_btn").gameObject;
        publicContent = views[(int)State.Public].transform.Search("Content").gameObject;
        privateInputField = views[(int)State.Private].transform.Search("InputField").gameObject;
    }

    private void SetupButtonEvents()
    {
        views[(int)State.Main].transform.SearchComponent<Button>("Join_btn").onClick.AddListener((MainJoinPublic));
        views[(int)State.Main].transform.SearchComponent<Button>("Public_btn").onClick.AddListener((() => SetState(State.Public)));
        views[(int)State.Main].transform.SearchComponent<Button>("Private_btn").onClick.AddListener((() => SetState(State.Private)));
        views[(int)State.Main].transform.SearchComponent<Button>("Private_btn").onClick.AddListener((() => SetState(State.Private)));
        views[(int)State.Public].transform.SearchComponent<Button>("Back_btn").onClick.AddListener(LeaveLobby);
        views[(int)State.Private].transform.SearchComponent<Button>("Back_btn").onClick.AddListener((() => SetState(State.Main)));
        views[(int)State.Private].transform.SearchComponent<Button>("Join_btn").onClick.AddListener((JoinPrivate));
        transform.SearchComponent<Button>("Close_btn").onClick.AddListener(CloseMenu);
    }

    /// <summary>
    /// Open the lobby UI
    /// </summary>
    /// <param name="guid">guid of the lobby to join</param>
    public void Open(string guid)
    {
        ContentManager.Instance.GetData(guid,
            (data) =>
            {
                if (data == null)
                    return;
                environmentDataEntry = (EnvironmentDataEntry)data;
                OpenLobby();
            },
            (error => Debug.LogError(error)));
    }

    private void OpenLobby()
    {
        gameObject.transform.position = Camera.main.transform.position + Camera.main.transform.forward;
        gameObject.transform.rotation = Camera.main.transform.rotation;
        gameObject.SetActive(true);
        SetState(State.Main);
    }

    private void Close()
    {
        SetState(State.None);
        LeaveLobby();
        gameObject.SetActive(false);
    }

    private void CloseMenu()
    {
        SetState(State.None);
        gameObject.SetActive(false);
    }

    [InspectorButton]
    private void OnMainBtn()
    {
        SetState(State.Main);
    }

    [InspectorButton]
    private void OnPublicBtn()
    {
        SetState(State.Public);
    }

    [InspectorButton]
    private void OnPrivateBtn()
    {
        SetState(State.Private);
    }

    private void SetState(State state)
    {
        this.state = state;
        UpdateUI();

        switch (state)
        {
            case State.Main:
                break;
            case State.Public:
                JoinLobby();
                break;
            case State.Private:
                break;
        }
    }

    private void JoinLobby()
    {
        Debug.LogError("JoinLobby");
        sessionInfos.Clear();
        UpdateLobbyList();
        LobbyManager.Instance.AddOnSessionListUpdatedCallback(OnSessionListUpdated);
        LobbyManager.Instance.EnterLobby(environmentDataEntry.guid);
    }

    private void LeaveLobby()
    {
        Debug.LogError("LeaveLobby");
        LobbyManager.Instance.RemoveOnSessionListUpdatedCallback(OnSessionListUpdated);
        LobbyManager.Instance.LeaveLobby();
        sessionInfos.Clear();
        UpdateLobbyList();
        SetState(State.Main);
    }

    private void OnSessionListUpdated(List<SessionInfo> infos)
    {
        UpdateInfoList(infos);
        UpdateLobbyList();
    }

    private void UpdateInfoList(List<SessionInfo> infos)
    {
        //shallow copy info list
        sessionInfos.Clear();
        foreach (var info in infos)
        {
            sessionInfos.Add(info);
        }
    }

    void ClearPublicContent()
    {
        for (int i = publicContent.transform.childCount - 1; i >= 1; i--)
        {
            Destroy(publicContent.transform.GetChild(i).gameObject);
        }
    }

    private void UpdateLobbyList()
    {
        ClearPublicContent();

        if (environmentDataEntry.networkData.instancingType == NetworkData.InstancingType.Fixed)
        {
            UpdateFixedList();
        }
        else
        {
            UpdatePublicList();
        }
    }

    private void UpdatePublicList()
    {
        foreach (var info in sessionInfos)
        {
            var index = GetIndexFromRoomName(info.Name);

            GameObject btn = Instantiate(publicBtn, publicContent.transform);
            btn.transform.SearchComponent<TMP_Text>("RoomName").text = $"Room Index {index.ToString()}";
            btn.transform.SearchComponent<TMP_Text>("UserCount").text = info.PlayerCount + "/" + environmentDataEntry.networkData.playerCount;
            btn.GetComponent<Button>().onClick.AddListener(() => JoinPublic(index));
            btn.SetActive(true);
        }
    }

    private void UpdateFixedList()
    {
        ClearPublicContent();

        foreach (var roomName in environmentDataEntry.networkData.fixedInstance.roomNames)
        {
            var si = GetSessionByName(roomName);
            GameObject btn = Instantiate(publicBtn, publicContent.transform);
            btn.transform.SearchComponent<TMP_Text>("RoomName").text = roomName;
            string playerCount = si == null ? "0" : si.PlayerCount.ToString();
            btn.transform.SearchComponent<TMP_Text>("UserCount").text = $"{playerCount}/{environmentDataEntry.networkData.playerCount}";
            btn.GetComponentInChildren<TMP_Text>().text = roomName;
            btn.GetComponent<Button>().onClick.AddListener(() => JoinPublic(roomName));
            btn.SetActive(true);
        }
    }

    private SessionInfo GetSessionByName(string name)
    {
        return sessionInfos.Find(si => si.Name.Contains(name));
    }

    private void UpdateUI()
    {
        for (var index = 0; index < views.Length; index++)
        {
            var view = views[index];

            if (index == (int)state)
            {
                view.SetActive(true);
            }
            else
            {
                view.SetActive(false);
            }
        }
    }

    private void MainJoinPublic()
    {
        Debug.LogError("MainJoinPublic");
        JoinPublic(1);
        SpaceMenu.Instance.MenuOff();
        Close();
    }

    private void JoinPublic(int index)
    {
        Debug.LogError($"JoinPublic {index}");
        LoadingManager.Instance.ChangeEnvironment(environmentDataEntry.guid);
        FusionNetworkManager.Instance.RemoveNetworkData(networkGroupData);
        FusionNetworkManager.Instance.SetIndex(index);
        FusionNetworkManager.Instance.CheckInRightRoom();
        SpaceMenu.Instance.MenuOff();
        Close();
    }

    private void JoinPublic(string roomName)
    {
        Debug.LogError($"JoinPublic {roomName}");
        LoadingManager.Instance.ChangeEnvironment(environmentDataEntry.guid);
        networkGroupData.id = roomName;
        networkGroupData.privateKey = "";
        FusionNetworkManager.Instance.AddNetworkData(networkGroupData);
        FusionNetworkManager.Instance.CheckInRightRoom();
        SpaceMenu.Instance.MenuOff();
        Close();
    }

    private void JoinPrivate()
    {
        Debug.LogError("JoinPrivate");
        LoadingManager.Instance.ChangeEnvironment(environmentDataEntry.guid);
        networkGroupData.privateKey = privateInputField.GetComponent<TMP_InputField>().text;
        FusionNetworkManager.Instance.AddNetworkData(networkGroupData);
        FusionNetworkManager.Instance.CheckInRightRoom();
        SpaceMenu.Instance.MenuOff();
        Close();
    }

    private int GetIndexFromRoomName(string roomName)
    {
        Debug.LogError(roomName.Substring(environmentDataEntry.guid.Length + 1));
        return int.Parse(roomName.Substring(environmentDataEntry.guid.Length + 1));
    }
}