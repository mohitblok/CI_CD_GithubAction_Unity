[System.Serializable]
public class StringListDropdown
{
    public string[] strings = new string[] { };
    public int selectedIndex = 0;
    public string selectedString
    {
        get
        {
            if (selectedIndex > strings.Length - 1)
            {
                selectedIndex = 0;
            }
            return strings[selectedIndex];
        }
    }

    public StringListDropdown()
    {
        strings = new string[] { };
        selectedIndex = 0;
    }

    public StringListDropdown(string[] strings, int selectedIndex)
    {
        this.strings = strings;
        this.selectedIndex = selectedIndex;
    }

    public int GetIndexOfString(string targetString)
    {
        for (int i = 0; i < strings.Length; i++)
        {
            if (strings[i] == targetString)
            {
                return i;
            }
        }

        return -1;
    }

    public void SelectNext(bool wrap = false)
    {
        selectedIndex = (selectedIndex + 1) % strings.Length;
    }

    public void SelectPrevious(bool wrap = false)
    {
        selectedIndex--;
        if (selectedIndex < 0)
        {
            selectedIndex = strings.Length - 1;
        }
    }
}