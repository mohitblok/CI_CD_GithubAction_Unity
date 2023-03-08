using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Utilities;
using WebSocketSharp;

namespace Bloktopia
{
    public class FusionNetworkManager : MonoSingleton<FusionNetworkManager>
    {
        [Tooltip("Show Network Activity and Status")]
        [SerializeField]
        private bool fusionStatWindow = false;

        [SerializeField]
        private string roomName = "default";

        [SerializeField]
        private int index = 0;

        [SerializeField]
        private string lobbyName = "default";

        [SerializeField]
        private int maxPlayers = 10;

        [SerializeField]
        private bool startOnAwake = true;

        [SerializeField]
        private GameObject runnerPrefab;

        [SerializeField]
        private GameObject lobbyRunnerPrefab;

        public GameObject GetLobbyRunnerPrefab => lobbyRunnerPrefab;

        private NetworkRunner networkRunner;
        public NetworkRunner GetRunner => networkRunner;

        [SerializeField]
        private GameMode startMode = GameMode.AutoHostOrClient;

        [SerializeField]
        private GameMode rejoinMode = GameMode.Single;

        [SerializeField]
        private bool disableNATPunchthrough = false;

        [SerializeField]
        private int tickRate = 60;

        [SerializeField]
        private int serverPacketInterval = 1;

        [SerializeField]
        private int clientPacketInterval = 1;

        public bool toBeDestroyed = false;

        private List<NetworkGroupData> networkDatas = new List<NetworkGroupData>();

        protected override void Awake()
        {
            base.Awake();

            Application.runInBackground = true;
        }

        private void Start()
        {
            if (startOnAwake)
            {
                Connect();
            }

            if (LoadingManager.Instance)
            {
                LoadingManager.Instance.OnEnvironmentLoaded += OnEnvironmentLoaded;
                LoadingManager.Instance.OnLeavingEnvironment += OnLeavingEnvironment;
            }
        }

        private void OnEnvironmentLoaded()
        {
            maxPlayers = LoadingManager.Instance.activeEnvironment.networkData.playerCount;
            lobbyName = LoadingManager.Instance.activeEnvironment.guid;
            Connect();
        }

        private void OnLeavingEnvironment()
        {
            index = 1;
            Disconnect();
            ClearNetworkData();
        }

        /// <summary> The Connect function connects to the server and starts the game.</summary>
        public void Connect()
        {
            toBeDestroyed = false;
            Connect(startMode);
        }


        /// <summary> The Rejoin function is used to rejoin the network if it has been lost.
        /// It will attempt to join the network and then call ReadyRejoin() if successful.</summary>
        public void Rejoin()
        {
            if (NetworkStabilityCheck.Instance)
            {
                NetworkStabilityCheck.Instance.OnWifiReady += ReadyRejoin;
                NetworkStabilityCheck.Instance.ForceCheck();
            }
            else
            {
                ReadyRejoin();
            }
        }

        private void ReadyRejoin()
        {
            if (NetworkStabilityCheck.Instance)
            {
                NetworkStabilityCheck.Instance.OnWifiReady -= ReadyRejoin;
            }

            Connect(rejoinMode);
        }

        /// <summary> The CheckInRightRoom function checks to see if the player is in the right room. If they are not, it shuts down the runner.</summary>
        public void CheckInRightRoom()
        {
            roomName = GetRoomName();

            if (networkRunner.LobbyInfo.Name != roomName)
            {
                ShutdownRunner();
            }
        }

        private string GetRoomName()
        {
            //default name
            string rn = $"{lobbyName}-{index}";

            //if we have a private key, use that instead
            if (!string.IsNullOrEmpty(GetPrivateKey()))
            {
                rn = $"{lobbyName}-{GetPrivateKey()}";
            }
            else if (!string.IsNullOrEmpty(GetNetworkDataRoomName()))
            {
                rn = $"{lobbyName}-{GetNetworkDataRoomName()}";
            }

            return rn;
        }

        private async void Connect(GameMode mode)
        {
            if (networkRunner && !networkRunner.IsShutdown)
            {
                return;
            }

            if (toBeDestroyed)
            {
                return;
            }

            roomName = GetRoomName();

            if (!networkRunner)
            {
                // Create the Fusion runnerPrefab and let it know that we will be providing user input
                networkRunner = Instantiate(runnerPrefab).GetComponent<NetworkRunner>();
                networkRunner.ProvideInput = true;
            }

            RunnerCallbacks.Instance.onShutdownCallback -= OnRunnerShutdown;
            RunnerCallbacks.Instance.onShutdownCallback += OnRunnerShutdown;
            RunnerCallbacks.Instance.onPlayerJoinCallback += OnPlayerJoined;

            var privateKey = GetPrivateKey();

            await networkRunner.StartGame(new StartGameArgs()
            {
                GameMode = mode,
                PlayerCount = maxPlayers,
                SessionName = string.IsNullOrEmpty(privateKey) ? roomName : $"{roomName}-{privateKey}",
                CustomLobbyName = string.IsNullOrEmpty(privateKey) ? lobbyName : null,
                DisableNATPunchthrough = disableNATPunchthrough,
            });
        }

        /// <summary> The Disconnect function disconnects the client from the server.</summary>
        public void Disconnect()
        {
            toBeDestroyed = true;
            ShutdownRunner();
        }

        private void ShutdownRunner()
        {
            if (networkRunner && !networkRunner.IsShutdown)
            {
                Debug.Log("Shutting Down Runner");
                networkRunner.Shutdown();
            }
            else
            {
                Cleanup();
            }
        }

        private void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            if (runner.LocalPlayer == player)
            {
                ConfigSimulation(runner);
                CreateFusionStats();
            }
        }

        private void ConfigSimulation(NetworkRunner runner)
        {
            runner.Simulation.Config.TickRate = tickRate;
            runner.Simulation.Config.ServerPacketInterval = serverPacketInterval;
            runner.Simulation.Config.ClientPacketInterval = clientPacketInterval;
        }

        private void CreateFusionStats()
        {
            if (fusionStatWindow)
            {
                FusionStats.Create(runner: networkRunner, screenLayout: FusionStats.DefaultLayouts.Right, objectLayout: FusionStats.DefaultLayouts.Right);
            }
        }

        private void OnDisconnectedFromServer(NetworkRunner runner)
        {
            ShutdownRunner();
        }

        private void OnRunnerShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            if (shutdownReason == ShutdownReason.GameIsFull)
            {
                GameFull();
            }
        }

        private void GameFull()
        {
            if (LoadingManager.Instance.activeEnvironment.networkData.instancingType == NetworkData.InstancingType.Multi)
            {
                index++;
                Disconnect();
                Invoke(nameof(Connect), 1);
            }
        }


        private void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            Cleanup();


            if (!toBeDestroyed)
            {
                Rejoin();
            }
        }

        // TODO: This cleanup should be handled by the ClearPools call below, but currently Fusion is not returning pooled objects on shutdown, so...
        private void Cleanup()
        {
            // Destroy all NOs
            NetworkObject[] nos = FindObjectsOfType<NetworkObject>();
            for (int i = 0; i < nos.Length; i++)
            {
                Destroy(nos[i].gameObject);
            }

            if (networkRunner)
            {
                Destroy(networkRunner.gameObject);
            }
        }

        private void OnDestroy()
        {
            toBeDestroyed = true;
            networkRunner.Shutdown();
            UnSubscribe();
        }

        private void UnSubscribe()
        {
            if (NetworkStabilityCheck.Instance)
            {
                NetworkStabilityCheck.Instance.OnWifiReady -= ReadyRejoin;
            }

            if (LoadingManager.Instance)
            {
                LoadingManager.Instance.OnEnvironmentLoaded -= OnEnvironmentLoaded;
                LoadingManager.Instance.OnLeavingEnvironment -= OnLeavingEnvironment;
            }

            if (RunnerCallbacks.Instance)
            {
                RunnerCallbacks.Instance.onShutdownCallback -= OnRunnerShutdown;
            }
        }


        /// <summary> The IsServer function returns true if the current client is a server, false otherwise.</summary>
        /// <returns> True if the game is running as a server, false otherwise.</returns>
        public bool IsServer()
        {
            return networkRunner.IsServer || networkRunner.IsSharedModeMasterClient;
        }


        /// <summary> The JoinLobby function is used to join a lobby. It will call the EnterLobby function in the LobbyManager script.</summary>
        [InspectorButton]
        public void JoinLobby()
        {
            LobbyManager.Instance.EnterLobby(LoadingManager.Instance.activeEnvironment.guid);
        }

        /// <summary> The LeaveLobby function is used to leave the current lobby.</summary>
        [InspectorButton]
        public void LeaveLobby()
        {
            LobbyManager.Instance.LeaveLobby();
        }

        /// <summary>
        /// networkData is used to store the network data for the current Group.
        /// </summary>
        public class NetworkGroupData
        {
            public string id;
            public string privateKey;
        }

        /// <summary> The AddNetworkData function adds a NetworkGroupData object to the networkDatas list.</summary>
        /// <param name="networkData"> The network data to remove.</param>
        public void AddNetworkData(NetworkGroupData networkData)
        {
            if (networkDatas.Contains(networkData))
                return;
            networkDatas.Add(networkData);
        }

        /// <summary> The RemoveNetworkData function removes the specified NetworkGroupData from the networkDatas list.</summary>
        /// <param name="networkData"> The network data to add.</param>
        public void RemoveNetworkData(NetworkGroupData networkData)
        {
            if (!networkDatas.Contains(networkData))
                return;
            networkDatas.Remove(networkData);
        }

        private void ClearNetworkData()
        {
            networkDatas.Clear();
        }


        /// <summary> The SetIndex function sets the index of the current node.</summary>
        /// <param name="index"> The index of the item to be removed</param>
        public void SetIndex(int index)
        {
            this.index = index;
        }

        private string GetPrivateKey()
        {
            string privateKey = "";

            foreach (var networkGroupData in networkDatas)
            {
                if (networkGroupData.privateKey != "")
                    privateKey = $"{privateKey}-{networkGroupData.privateKey}";
            }

            return privateKey;
        }

        private string GetNetworkDataRoomName()
        {
            string rn = "";

            foreach (var networkGroupData in networkDatas)
            {
                if (!networkGroupData.id.IsNullOrEmpty())
                    rn = $"{rn}-{networkGroupData.id}";
            }

            return rn;
        }
    }
}