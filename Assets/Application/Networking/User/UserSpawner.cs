using Fusion;
using Unity.Mathematics;
using UnityEngine;

public class UserSpawner : MonoBehaviour
{
    public GameObject networkedUser;
    
    private void Start()
    {
        RunnerCallbacks.Instance.onPlayerJoinCallback+= OnPlayerJoinCallback;
    }
    
    private void OnPlayerJoinCallback(NetworkRunner runner, PlayerRef player)
    {
        if (runner.LocalPlayer != player)
        {
            return;
        }

        var user = runner.Spawn(networkedUser,Vector3.zero,Quaternion.identity,player);
    }

    private void OnDestroy()
    {
        RunnerCallbacks.Instance.onPlayerJoinCallback-= OnPlayerJoinCallback;
    }
}
