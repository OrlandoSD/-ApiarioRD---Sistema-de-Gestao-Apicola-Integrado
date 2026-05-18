using System; 
using System.Collections.Generic; 
using System.Drawing; 
using System.Globalization; 
using System.IO; 
using System.Linq; 
using System.Windows.Forms; 

public class EstoqueForm : FormBase 
{ 
    DataGridView dgvEstoque; 
    DataGridView dgvReservado; 
    Button btnInserir, btnRemover, btnEntrada, btnSaida, btnReservar, btnLiberar; 
    Timer timer = new Timer(); 

    public EstoqueForm() 
    { 
        Text = "Gestão de Estoque - ApiarioRD"; 
        Width = 1200; 
        Height = 700; 
        BackColor = Color.FromArgb(45, 45, 45); 

        // 🔹 DataGridView Estoque Principal 
        dgvEstoque = new DataGridView() { Left = 10, Top = 10, Width = 550, Height = 500, }; 
        GridEstilo.Aplicar(dgvEstoque); 

        // 🔹 DataGridView Estoque Reservado 
        dgvReservado = new DataGridView() { Left = 600, Top = 10, Width = 550, Height = 500, }; 
        GridEstilo.Aplicar(dgvReservado); 

        // 🔹 Botões 
        btnInserir = CriarBotao("Inserir Produto", 10, 520); 
        btnRemover = CriarBotao("Remover Produto", 150, 520); 
        btnEntrada = CriarBotao("Entrada", 10, 570); 
        btnSaida = CriarBotao("Saída", 150, 570); 
        btnReservar = CriarBotao("Reservar", 600, 520); 
        btnLiberar = CriarBotao("Liberar Reserva", 740, 520); 

        // 🔹 Eventos dos botões 
        btnInserir.Click += BtnInserir_Click; 
        btnRemover.Click += BtnRemover_Click; 
        btnEntrada.Click += BtnEntrada_Click; 
        btnSaida.Click += BtnSaida_Click; 
        btnReservar.Click += BtnReservar_Click; 
        btnLiberar.Click += BtnLiberar_Click; 

        Controls.AddRange(new Control[] { dgvEstoque, dgvReservado, btnInserir, btnRemover, btnEntrada, btnSaida, btnReservar, btnLiberar }); 

        // ➕ ACRESCIMO SEGURO: Garante estabilidade nos cliques para os métodos de Entrada/Saída/Reserva
        dgvEstoque.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvEstoque.MultiSelect = false;
        dgvEstoque.ReadOnly = true;

        dgvReservado.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvReservado.MultiSelect = false;
        dgvReservado.ReadOnly = true;

        timer.Interval = 60000; 
        timer.Tick += (s, e) => AtualizarGrids(); 
        timer.Start(); 
        AtualizarGrids(); 
    } 

    Button CriarBotao(string texto, int left, int top) 
    { 
        return new Button() { Text = texto, Left = left, Top = top, Width = 130, Height = 40, BackColor = Color.DarkOrange, ForeColor = Color.White, Font = new Font("Segoe UI", 9, FontStyle.Bold) }; 
    } 

    void AtualizarGrids() 
    { 
        // Estoque Principal 
        var estoque = EstoqueService.ListarTodos(); 
        dgvEstoque.DataSource = estoque.Select(x => new { Produto = x.Produto, Quantidade = x.Quantidade }).ToList(); 

        // Estoque Reservado 
        var reservado = CarregarReservado(); 
        dgvReservado.DataSource = reservado.Select(x => new { Produto = x.Produto, Quantidade = x.Quantidade }).ToList(); 
    } 

    List<(string Produto, decimal Quantidade)> CarregarReservado() 
    { 
        string path = Path.Combine("dados", "estoque_reservado.csv"); 
        if (!File.Exists(path)) File.WriteAllText(path, "Produto,Quantidade" + Environment.NewLine); 
        var linhas = File.ReadAllLines(path).Skip(1); 
        var lista = new List<(string Produto, decimal Quantidade)>(); 
        foreach (var l in linhas) 
        { 
            var c = l.Split(','); 
            if (c.Length < 2) continue; 
            if (decimal.TryParse(c[1], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal qtd)) lista.Add((c[0], qtd)); 
        } 
        return lista; 
    } 

    void SalvarReservado(List<(string Produto, decimal Quantidade)> lista) 
    { 
        string path = Path.Combine("dados", "estoque_reservado.csv"); 
        var linhas = new List<string> { "Produto,Quantidade" }; 
        linhas.AddRange(lista.Select(x => $"{x.Produto},{x.Quantidade.ToString(CultureInfo.InvariantCulture)}")); 
        File.WriteAllLines(path, linhas); 
    } 

    // 🔹 BOTÕES 
    void BtnInserir_Click(object sender, EventArgs e) 
    { 
        string produto = Microsoft.VisualBasic.Interaction.InputBox("Nome do produto:", "Inserir Produto"); 
        if (string.IsNullOrWhiteSpace(produto)) return; 
        decimal qtd = 0; 
        string qtdStr = Microsoft.VisualBasic.Interaction.InputBox("Quantidade inicial:", "Inserir Produto"); 
        decimal.TryParse(qtdStr, NumberStyles.Any, CultureInfo.InvariantCulture, out qtd); 
        var estoque = EstoqueService.ListarTodos().ToDictionary(x => x.Produto, x => x.Quantidade); 
        if (estoque.ContainsKey(produto)) { MessageBox.Show("Produto já existe!"); return; } 
        estoque[produto] = qtd; 
        EstoqueService.Salvar(estoque); 
        AtualizarGrids(); 
    } 

    void BtnRemover_Click(object sender, EventArgs e) 
    { 
        if (dgvEstoque.SelectedRows.Count == 0) return; 
        string produto = dgvEstoque.SelectedRows[0].Cells[0].Value.ToString(); 
        var estoque = EstoqueService.ListarTodos().ToDictionary(x => x.Produto, x => x.Quantidade); 
        if (estoque.ContainsKey(produto)) estoque.Remove(produto); 
        EstoqueService.Salvar(estoque); 
        AtualizarGrids(); 
    } 

    void BtnEntrada_Click(object sender, EventArgs e) 
    { 
        if (dgvEstoque.SelectedRows.Count == 0) return; 
        string produto = dgvEstoque.SelectedRows[0].Cells[0].Value.ToString(); 
        string qtdStr = Microsoft.VisualBasic.Interaction.InputBox("Quantidade de entrada:", "Entrada"); 
        if (!decimal.TryParse(qtdStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal qtd)) return; 
        EstoqueService.MovimentarEstoque(produto, qtd, "Entrada"); 
        AtualizarGrids(); 
    } 

    void BtnSaida_Click(object sender, EventArgs e) 
    { 
        if (dgvEstoque.SelectedRows.Count == 0) return; 
        string produto = dgvEstoque.SelectedRows[0].Cells[0].Value.ToString(); 
        string qtdStr = Microsoft.VisualBasic.Interaction.InputBox("Quantidade de saída:", "Saída"); 
        if (!decimal.TryParse(qtdStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal qtd)) return; 
        try { EstoqueService.MovimentarEstoque(produto, qtd, "Saida"); } 
        catch (Exception ex) { MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Warning); } 
        AtualizarGrids(); 
    } 

    void BtnReservar_Click(object sender, EventArgs e) 
    { 
        if (dgvEstoque.SelectedRows.Count == 0) return; 
        string produto = dgvEstoque.SelectedRows[0].Cells[0].Value.ToString(); 
        string qtdStr = Microsoft.VisualBasic.Interaction.InputBox("Quantidade a reservar:", "Reservar"); 
        if (!decimal.TryParse(qtdStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal qtd)) return; 
        decimal disponivel = EstoqueService.ObterQuantidade(produto); 
        if (disponivel < qtd) { MessageBox.Show($"Estoque insuficiente. Disponível: {disponivel}"); return; } 
        EstoqueService.MovimentarEstoque(produto, qtd, "Saida"); 
        var reservado = CarregarReservado(); 
        var item = reservado.FirstOrDefault(x => x.Produto == produto); 
        if (item.Produto == null) reservado.Add((produto, qtd)); 
        else { reservado.Remove(item); reservado.Add((produto, item.Quantidade + qtd)); } 
        SalvarReservado(reservado); 
        AtualizarGrids(); 
    } 

    void BtnLiberar_Click(object sender, EventArgs e) 
    { 
        if (dgvReservado.SelectedRows.Count == 0) return; 
        string produto = dgvReservado.SelectedRows[0].Cells[0].Value.ToString(); 
        string qtdStr = Microsoft.VisualBasic.Interaction.InputBox("Quantidade a liberar:", "Liberar Reserva"); 
        if (!decimal.TryParse(qtdStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal qtd)) return; 
        var reservado = CarregarReservado(); 
        var item = reservado.FirstOrDefault(x => x.Produto == produto); 
        if (item.Produto == null) return; 
        if (item.Quantidade < qtd) { MessageBox.Show($"Estoque reservado insuficiente. Reservado: {item.Quantidade}"); return; } 
        reservado.Remove(item); 
        if (item.Quantidade > qtd) reservado.Add((produto, item.Quantidade - qtd)); 
        SalvarReservado(reservado); 
        EstoqueService.MovimentarEstoque(produto, qtd, "Entrada"); 
        AtualizarGrids(); 
    } 
}
