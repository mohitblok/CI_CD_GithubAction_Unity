public interface IUILoading 
{
    public void ShowLoading(System.Action<string> statusCallback, LoaderType type = LoaderType.DefaultUILoader, float fadeDuration = 1f);

    public void LoadSceneWithLoadingScreen(string sceneName, System.Action<string> statusCallback, LoaderType type = LoaderType.DefaultUILoader, float fadeDuration = 1f);

    public void DisableLoading();
}

public enum LoaderType
{
    DefaultUILoader,
    UILoader1,
    UILoader2,
    SphericalMeshLoader
}
