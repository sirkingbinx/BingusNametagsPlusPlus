namespace BingusNametagsPlusPlus.Models;

public struct BGNametagData {
    public string Color = "#ffffff";
    public BGNametagStyle Style = BGNametagStyle.Bold | BGNametagStyle.Italic | BGNametagStyle.Underline;
}

[Flags]
public enum BGNametagStyle {
    Bold = 0,
    Italic,
    Underline
}