
[System.Serializable]
public class GameSettings
{
    public int resolutionWidth = 1920;
    public int resolutionHeight = 1080;
    public int refreshRate = 60;
    public DisplayMode displayMode = DisplayMode.Fullscreen;
    public int languageIndex = 0;
    public float volume = 1f;

    public enum DisplayMode
    {
        Fullscreen = 0,
        Windowed = 1,
        Borderless = 2
    }
}
