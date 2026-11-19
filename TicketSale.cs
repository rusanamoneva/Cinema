namespace KinoSalon;

/// <summary>Запис за продажба на билети.</summary>
internal sealed class TicketSale
{
    public TicketSale(string movie, string zone, int ticketCount, bool has3DGlasses,
        bool hasStudentDiscount, decimal totalPrice)
    {
        Movie = movie;
        Zone = zone;
        TicketCount = ticketCount;
        Has3DGlasses = has3DGlasses;
        HasStudentDiscount = hasStudentDiscount;
        TotalPrice = totalPrice;
    }

    public string Movie { get; }
    public string Zone { get; }
    public int TicketCount { get; }
    public bool Has3DGlasses { get; }
    public bool HasStudentDiscount { get; }
    public decimal TotalPrice { get; }
}
