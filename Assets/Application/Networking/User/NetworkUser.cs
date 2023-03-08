using Fusion;

public class NetworkUser : NetworkBehaviour
{
    private Avatar avatar;
    public string avatarUrl = "https://api.readyplayer.me/v1/avatars/63d3b60923fe23d34bf8eded.glb";

    [Networked(OnChanged = nameof(OnAvatarUpdate)), Capacity(80)]
    public string syncedAvatar { get; private set; }

    private void Awake()
    {
        avatar = GetComponent<Avatar>();
    }

    public override void Spawned()
    {
        base.Spawned();
        syncedAvatar = avatarUrl;
    }

    private static void OnAvatarUpdate(Changed<NetworkUser> changed)
    {
        changed.Behaviour.LoadAvatar();
    }

    private void LoadAvatar()
    {
        avatar.AddAvatarLoadedCallback(OnAvatarLoadedCallback);
        avatar.LoadAvatar(syncedAvatar);
    }

    private void OnAvatarLoadedCallback()
    {
        avatar.RemoveAvatarLoadedCallback(OnAvatarLoadedCallback);
    }
}