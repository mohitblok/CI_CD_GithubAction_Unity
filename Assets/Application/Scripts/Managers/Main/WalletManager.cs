using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalletManager : MonoSingleton<WalletManager>
{
    [field: SerializeField] public bool isConnected { get; private set; }
    [field: SerializeField] public bool isLoggedIn  { get; private set; }
    public bool isLive => isConnected && isLoggedIn;

    public void ConnectWallet() { }
    public void DisconnectWallet() { }

    // ???????????????????
}
