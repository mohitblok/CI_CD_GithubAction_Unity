public static class StringExtentions
{
    public static string MakeDatePathFriendly(this string str)
    {
        str = str.Replace("/", string.Empty);
        str = str.Replace("\\", string.Empty);
        str = str.Replace(" ", string.Empty);
        str = str.Replace(":", string.Empty);
        return str;
    }
}