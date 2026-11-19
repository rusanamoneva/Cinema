using System.Drawing;

namespace KinoSalon;

public sealed class MainForm : Form
{
    private const int InitialFreeSeats = 80;
    // Всяка прожекция е в отделна зала с начален капацитет 80 места.
    // Индексите съответстват на прожекциите в cmbMovie.
    private readonly int[] freeSeatsByMovie = { InitialFreeSeats, InitialFreeSeats, InitialFreeSeats };
    private readonly List<TicketSale> sales = new();

    private readonly ComboBox cmbMovie = new();
    private readonly RadioButton rbStandard = new();
    private readonly RadioButton rbVip = new();
    private readonly RadioButton rbBalcony = new();
    private readonly TextBox txtTicketCount = new();
    private readonly CheckBox chk3DGlasses = new();
    private readonly CheckBox chkStudentCard = new();
    private readonly ListBox lstSales = new();
    private readonly Label lblFreeSeats = new();
    private readonly Label lblTotalTickets = new();
    private readonly Label lblTotalRevenue = new();
    private readonly Label lblAveragePurchase = new();

    public MainForm()
    {
        InitializeInterface();
        UpdateStatistics();
    }

    private void InitializeInterface()
    {
        Text = "Киносалон – Русана";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(850, 660);
        MinimumSize = new Size(850, 660);
        Font = new Font("Segoe UI", 10F);

        var title = new Label
        {
            Text = "Продажба на билети за киносалон",
            Font = new Font("Segoe UI", 16F, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(25, 20)
        };
        Controls.Add(title);

        var inputGroup = new GroupBox
        {
            Text = "Данни за покупката", Location = new Point(25, 65), Size = new Size(380, 360)
        };
        Controls.Add(inputGroup);

        inputGroup.Controls.Add(new Label { Text = "Филмова прожекция:", AutoSize = true, Location = new Point(20, 35) });
        cmbMovie.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbMovie.Items.AddRange(new object[]
        {
            "Приключенски филм – 12.00 лв.",
            "Комедия – 14.00 лв.",
            "Научна фантастика – 16.00 лв."
        });
        cmbMovie.SelectedIndex = 0;
        cmbMovie.SelectedIndexChanged += MovieSelectionChanged;
        cmbMovie.Location = new Point(20, 60);
        cmbMovie.Size = new Size(330, 30);
        inputGroup.Controls.Add(cmbMovie);

        var zoneGroup = new GroupBox { Text = "Зона в залата", Location = new Point(20, 105), Size = new Size(330, 115) };
        rbStandard.Text = "Стандартен партер (базова цена)";
        rbStandard.Checked = true;
        rbStandard.AutoSize = true;
        rbStandard.Location = new Point(12, 25);
        rbVip.Text = "ВИП ложа (+5.00 лв. на билет)";
        rbVip.AutoSize = true;
        rbVip.Location = new Point(12, 52);
        rbBalcony.Text = "Балкон (-2.00 лв. на билет)";
        rbBalcony.AutoSize = true;
        rbBalcony.Location = new Point(12, 79);
        zoneGroup.Controls.AddRange(new Control[] { rbStandard, rbVip, rbBalcony });
        inputGroup.Controls.Add(zoneGroup);

        inputGroup.Controls.Add(new Label { Text = "Брой билети (1–10):", AutoSize = true, Location = new Point(20, 240) });
        txtTicketCount.Location = new Point(205, 237);
        txtTicketCount.Size = new Size(145, 30);
        inputGroup.Controls.Add(txtTicketCount);

        chk3DGlasses.Text = "Очила за 3D (+2.00 лв. на билет)";
        chk3DGlasses.AutoSize = true;
        chk3DGlasses.Location = new Point(20, 285);
        chkStudentCard.Text = "Ученическа/студентска карта (-20%)";
        chkStudentCard.AutoSize = true;
        chkStudentCard.Location = new Point(20, 315);
        inputGroup.Controls.AddRange(new Control[] { chk3DGlasses, chkStudentCard });

        var btnSell = new Button { Text = "Продай билети", Location = new Point(25, 445), Size = new Size(180, 42) };
        btnSell.Click += SellTickets_Click;
        var btnClear = new Button { Text = "Изчисти полетата", Location = new Point(225, 445), Size = new Size(180, 42) };
        btnClear.Click += ClearFields_Click;
        Controls.AddRange(new Control[] { btnSell, btnClear });

        var historyGroup = new GroupBox { Text = "История на продажбите", Location = new Point(430, 65), Size = new Size(395, 422) };
        lstSales.Location = new Point(15, 28);
        lstSales.Size = new Size(365, 370);
        lstSales.HorizontalScrollbar = true;
        historyGroup.Controls.Add(lstSales);
        Controls.Add(historyGroup);

        var statisticsGroup = new GroupBox { Text = "Обобщение", Location = new Point(25, 505), Size = new Size(800, 135) };
        // Показателите са подредени на отделни редове, за да не се припокриват.
        lblFreeSeats.Location = new Point(18, 25); lblFreeSeats.AutoSize = true;
        lblTotalTickets.Location = new Point(18, 50); lblTotalTickets.AutoSize = true;
        lblTotalRevenue.Location = new Point(18, 75); lblTotalRevenue.AutoSize = true;
        lblAveragePurchase.Location = new Point(18, 100); lblAveragePurchase.AutoSize = true;
        statisticsGroup.Controls.AddRange(new Control[] { lblFreeSeats, lblTotalTickets, lblTotalRevenue, lblAveragePurchase });
        Controls.Add(statisticsGroup);
    }

    private void SellTickets_Click(object? sender, EventArgs e)
    {
        if (!int.TryParse(txtTicketCount.Text.Trim(), out int count) || count < 1 || count > 10)
        {
            MessageBox.Show("Въведете цяло число от 1 до 10 за броя билети.", "Некоректен вход", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtTicketCount.Focus();
            return;
        }

        int movieIndex = cmbMovie.SelectedIndex;
        int freeSeatsForMovie = freeSeatsByMovie[movieIndex];
        if (count > freeSeatsForMovie)
        {
            MessageBox.Show($"Няма достатъчно свободни места за избраната прожекция. Остават само {freeSeatsForMovie}.", "Недостатъчно места", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        decimal pricePerTicket = CalculateTicketPrice();
        decimal total = pricePerTicket * count;
        string movie = cmbMovie.SelectedItem?.ToString() ?? "Непосочен филм";
        string zone = rbVip.Checked ? "ВИП ложа" : rbBalcony.Checked ? "Балкон" : "Стандартен партер";

        var sale = new TicketSale(movie, zone, count, chk3DGlasses.Checked, chkStudentCard.Checked, total);
        sales.Add(sale);
        freeSeatsByMovie[movieIndex] -= count;
        lstSales.Items.Add($"{sales.Count}. {movie}; {zone}; {count} бр. → {total:F2} лв.");
        UpdateStatistics();
        ClearFields();
    }

    // Основното пресмятане е отделено от обработчика на бутона.
    private decimal CalculateTicketPrice()
    {
        decimal price = cmbMovie.SelectedIndex switch { 0 => 12.00m, 1 => 14.00m, 2 => 16.00m, _ => 0m };
        if (rbVip.Checked) price += 5.00m;
        if (rbBalcony.Checked) price -= 2.00m;
        if (chk3DGlasses.Checked) price += 2.00m;
        if (chkStudentCard.Checked) price *= 0.80m;
        return price;
    }

    // Обобщението умишлено използва цикъл върху всички извършени продажби.
    private void UpdateStatistics()
    {
        int soldTickets = 0;
        decimal totalRevenue = 0m;
        foreach (TicketSale sale in sales)
        {
            soldTickets += sale.TicketCount;
            totalRevenue += sale.TotalPrice;
        }

        decimal averagePurchase = sales.Count == 0 ? 0m : totalRevenue / sales.Count;
        int selectedMovieIndex = cmbMovie.SelectedIndex;
        int selectedMovieFreeSeats = freeSeatsByMovie[selectedMovieIndex];
        lblFreeSeats.Text = $"Свободни места за тази прожекция: {selectedMovieFreeSeats}";
        lblTotalTickets.Text = $"Продадени билети: {soldTickets}";
        lblTotalRevenue.Text = $"Общ приход: {totalRevenue:F2} лв.";
        lblAveragePurchase.Text = $"Средна покупка: {averagePurchase:F2} лв.";
    }

    private void ClearFields_Click(object? sender, EventArgs e) => ClearFields();

    private void MovieSelectionChanged(object? sender, EventArgs e) => UpdateStatistics();

    private void ClearFields()
    {
        cmbMovie.SelectedIndex = 0;
        rbStandard.Checked = true;
        chk3DGlasses.Checked = false;
        chkStudentCard.Checked = false;
        txtTicketCount.Clear();
        txtTicketCount.Focus();
    }
}
