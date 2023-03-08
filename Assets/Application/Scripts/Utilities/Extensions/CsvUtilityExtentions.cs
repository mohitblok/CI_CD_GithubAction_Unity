using System.Collections.Generic;
using System.Text;

public static class CsvUtilityExtentions
{
    public static void AppendCsvVarible(this StringBuilder sb, string valueName, string value, int cellIndent = 0)
    {
        for (int indentCount = 0; indentCount < cellIndent; indentCount++)
        {
            sb.Append(",");
        }

        sb.Append("\"");
        sb.Append(valueName);
        sb.Append("\"");
        sb.Append(",");
        sb.Append("\"");
        sb.Append(value);
        sb.Append("\"");
        sb.Append(",");
    }

    public static void AppendCsvCell(this StringBuilder sb, string value, int cellIndent = 0)
    {
        for (int indentCount = 0; indentCount < cellIndent; indentCount++)
        {
            sb.Append(",");
        }

        sb.Append("\"");
        sb.Append(value);
        sb.Append("\"");
        sb.Append(",");
    }
    public static void AppendCsvRepeatingCell(this StringBuilder sb, string value, int repeatAmount, int cellIndent = 0)
    {
        for (int indentCount = 0; indentCount < cellIndent; indentCount++)
        {
            sb.Append(",");
        }

        for (int repeatCount = 0; repeatCount < repeatAmount; repeatCount++)
        {
            sb.Append("\"\"");
            sb.Append(value);
            sb.Append("\"\"");
            sb.Append(",");
        }
    }

    public static void AddCsvLine(this List<string> csvLines, StringBuilder csvStringBuilder)
    {
        csvLines.Add(csvStringBuilder.ToString());
        csvStringBuilder.Clear();
    }

    public static void AddCsvLineSpace(this List<string> csvLines, int numberOfRows = 1)
    {
        for (int rowCount = 0; rowCount < numberOfRows; rowCount++)
        {
            csvLines.Add("");
        }
    }

    public static string StringListToCSV(List<string> list)
    {
        if (list == null || list.Count < 1)
            return "";

        string s = "";
        for (int i = 0; i < list.Count; i++)
            s += (i < list.Count - 1 ? list[i] + "," : list[i]); // Don't add trailing comma

        return s;
    }

    public static List<string> CSVToList(string csvString, bool stripEmpty = true)
    {
        if (string.IsNullOrEmpty(csvString))
            return new List<string>();

        string[] subStrings = csvString.Split(',');
        if (subStrings == null || subStrings.Length < 1)
            return new List<string>();

        if (stripEmpty)
        {
            List<string> list = new List<string>(); // Strip empty items
            for (int i = 0; i < subStrings.Length; i++)
                if (string.IsNullOrWhiteSpace(subStrings[i]) == false)
                    list.Add(subStrings[i]);

            return list;
        }
        else
            return new List<string>(subStrings);
    }
}