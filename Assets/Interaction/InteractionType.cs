public enum InteractionType
{
    Use,
    Close,
    Examine,
    Take,
    Locked,
    Activate,
    Sleep,
    Discard
}

public static class InteractionTypeHelper
{
    public static string GetCommandText(InteractionType type)
    {
        switch (type)
        {
            case InteractionType.Use: return "Öffnen";
            case InteractionType.Close: return "Schließen";
            case InteractionType.Examine: return "Untersuchen";
            case InteractionType.Take: return "Einsammeln";
            case InteractionType.Locked: return "Abgeschlossen";
            case InteractionType.Activate: return "Einschalten";
            case InteractionType.Sleep: return "Schlafen";
            case InteractionType.Discard: return "Müll wegwerfen";
            default: return "Benutzen";
        }
    }
}