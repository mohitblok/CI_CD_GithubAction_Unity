using System;
using System.Collections;
using UnityEngine;

namespace Bloktopia
{
    public class NetworkStabilityCheck : MonoSingleton<NetworkStabilityCheck>
    {
        public Action OnWifiReady;
        public Action OnWifiDown;
        
        private const bool ALLOW_CARRIER_DATA_NETWORK = false;
        private const string PING_ADDRESS = "8.8.8.8"; // Google Public DNS server can swap if needed
        private const float WAITING_TIME = 2.0f;
        
        private bool isConnected;
        private int currentRetries = 0;
        private const int MAX_RETRIES = 3;
        private bool isCheckingConnection;

        public RunnerPingCheck runnerPingCheck;
        
        private void Start()
        {
            runnerPingCheck = new RunnerPingCheck(this);
            
            StartCoroutine(RunNetworkPolling());
        }

        private IEnumerator RunNetworkPolling()
        {
            yield return TestConnection();

            while (true)
            {
                yield return new WaitForSeconds(5);

                yield return TestConnection();
            }
        }

        /// <summary>
        /// will return true if a network connection is established
        /// </summary>
        public bool IsConnected => isConnected;

        /// <summary>
        /// Run Connection check without waiting for cooldown
        /// </summary>
        public void ForceCheck()
        {
            StartCoroutine(TestConnection());
        }

        private IEnumerator TestConnection()
        {
            //prevent overlapping Checks
            if (isCheckingConnection)
            {
                yield break;
            }

            isCheckingConnection = true;

            if (HasConnectionToRouter())
            {
                yield return PingTest();
            }
        
            isCheckingConnection = false;
        }
        
        private bool HasConnectionToRouter()
        {
            bool hasConnectionToRouter;
            switch (Application.internetReachability)
            {
                case NetworkReachability.ReachableViaLocalAreaNetwork:
                    hasConnectionToRouter = true;
                    break;
                case NetworkReachability.ReachableViaCarrierDataNetwork:
                    hasConnectionToRouter = ALLOW_CARRIER_DATA_NETWORK;
                    break;
                default:
                    hasConnectionToRouter = false;
                    break;
            }

            if (!hasConnectionToRouter)
            {
                InternetIsNotAvailable();
                return false;
            }

            return true;
        }
        
        private IEnumerator PingTest()
        {
            var ping = new Ping(PING_ADDRESS);
            var pingStartTime = Time.time;

            while (!ping.isDone)
            {
                yield return null;

                if (ping.isDone)
                {
                    if (ping.time >= 0)
                    {
                        InternetAvailable();
                        break;
                    }
                    else
                    {
                        InternetIsNotAvailable();
                        break;
                    }
                }
                else if (Time.time - pingStartTime > WAITING_TIME)
                {
                    InternetIsNotAvailable();
                    break;
                }
            }
        }


        private void InternetIsNotAvailable()
        {
            currentRetries++;
            if (currentRetries <= MAX_RETRIES)
            {
                StartCoroutine(TestConnection());
            }
            else
            {
                isConnected = false;
                OnWifiDown?.Invoke();
            }
        }

        private void InternetAvailable()
        {
            currentRetries = 0;
            isConnected = true;
            OnWifiReady?.Invoke();
        }
    }

    public class RunnerPingCheck
    {
        public event Action<double> OnPingUpdate;
        
        private readonly WaitForSeconds _waitForSecond = new(1);

        public RunnerPingCheck(MonoBehaviour monoBehaviour)
        {
            monoBehaviour.StartCoroutine(FusionPingTest());
        }

        private IEnumerator FusionPingTest()
        {
            while (true)
            {
                yield return _waitForSecond;
                var ping = Ping();
                OnPingUpdate?.Invoke(ping);
            }
        }
        
        private static double Ping()
        {
            if (!FusionNetworkManager.Instance||
                !FusionNetworkManager.Instance.GetRunner||
                !FusionNetworkManager.Instance.GetRunner.IsConnectedToServer)
            {
                return 0;
            }

            return FusionNetworkManager.Instance.GetRunner.GetPlayerRtt(FusionNetworkManager.Instance.GetRunner.LocalPlayer);
        }
    }
}
