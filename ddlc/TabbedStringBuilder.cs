using System.Text;

public class TabbedStringBuilder
{
    private StringBuilder _sb;
    private string _tab;
    private int _tabsCount = 0;
    public TabbedStringBuilder(StringBuilder sb, string tab = "")
    {
        _sb = sb;
        _tab = tab;
    }
    

    public void PushTab()
    {
        _tabsCount++;
    }
    public void PopTab()
    {
        _tabsCount--;
    }


    private string GetTabOffsets()
    {
        var result = "";
        for (var i = 0; i < _tabsCount; ++i)
            result += "    ";
        return result;
    }
    public void WriteLine(string line)
    {
        _sb.AppendLine(_tab + GetTabOffsets() + line);
    }
    public void WriteNestedLine(string line)
    {
        PushTab();
        _sb.AppendLine(_tab + GetTabOffsets() + line);
        PopTab();
    }
}