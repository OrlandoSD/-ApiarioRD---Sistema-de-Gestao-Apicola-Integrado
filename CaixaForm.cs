using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Globalization;
using System.Collections.Generic;
using System.Drawing;

public class CaixaForm : FormBase
{
    DataGridView grid = new DataGridView();
    ComboBox cmbTipo = new ComboBox() { Top = 220, Left = 10, Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
    TextBox txtDescricao = new TextBox() { Top = 220, Left = 140, Width = 150 };
    TextBox txtValor = new TextBox() { Top = 220, Left = 300, Width = 100 };

    Label lblTipo = new Label() { Text = "Tipo:", Top = 200, Left = 10, AutoSize = true };
    Label lblDescricao = new Label() { Text = "Descrição:", Top = 200, Left = 140, AutoSize = true };
    Label lblValor = new Label() { Text = "Valor (R$):", Top = 200, Left = 300, AutoSize = true };

    Button btnNovo = new Button() { Text = "Novo", Top = 260, Left = 10, Width = 80 };
    Button btnSalvar = new Button() { Text = "Salvar", Top = 260, Left = 100, Width = 150 };
    Button btnExcluir = new Button() { Text = "Excluir", Top = 260, Left = 260, Width = 140 };

    Label lblEntradas = new Label() { Top = 310, Left = 10, Width = 500 };
    Label lblSaidas = new Label() { Top = 330, Left = 10, Width = 500 };
    Label lblSaldo = new Label() { Top = 350, Left = 10, Width = 500, Font = new Font("Segoe UI", 11, FontStyle.Bold) };

    List<string[]> dados = new List<string[]>();
    int linhaSelecionada = -1;

    public CaixaForm()
    {
        Text = "ApiarioRD - Controle de Caixa 💰";
        Width = 550; Height = 450;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(45, 45, 45);
        ForeColor = Color.White;

        grid.SetBounds(10, 10, 510, 180);
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.RowHeadersVisible = false;
        grid.ReadOnly = true;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

        // 🔥 VISUAL MELHORADO
        grid.BackgroundColor = Color.FromArgb(30, 30, 30);
        grid.DefaultCellStyle.BackColor = Color.FromArgb(45, 45, 45);
        grid.DefaultCellStyle.ForeColor = Color.White;
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(55, 55, 55);

        cmbTipo.Items.AddRange(new string[] { "ENTRADA", "SAIDA" });
        txtDescricao.PlaceholderText = "Ex: Venda de mel";
        txtValor.PlaceholderText = "0,00";

        Controls.AddRange(new Control[] {
            grid, cmbTipo, txtDescricao, txtValor,
            lblTipo, lblDescricao, lblValor,
            btnNovo, btnSalvar, btnExcluir,
            lblEntradas, lblSaidas, lblSaldo
        });

        btnNovo.Click += (s, e) => Limpar();
        btnSalvar.Click += (s, e) => Adicionar();
        btnExcluir.Click += (s, e) => Excluir();
        grid.CellClick += (s, e) => Selecionar();

        try { TemaHelper.Aplicar(this); } catch { }

        Carregar();
    }

    void Carregar()
    {
        Directory.CreateDirectory("dados");
        string arquivo = "dados/caixa.csv";

        if (!File.Exists(arquivo))
            File.WriteAllLines(arquivo, new[] { "Data,Tipo,Descricao,Valor" });

        dados.Clear();

        var linhas = File.ReadAllLines(arquivo)
                         .Skip(1)
                         .Where(l => !string.IsNullOrWhiteSpace(l));

        foreach (var l in linhas)
            dados.Add(l.Split(','));

        Atualizar();
        CalcularSaldo();
    }

    void Atualizar()
    {
        grid.Columns.Clear();
        grid.Rows.Clear();

        grid.Columns.Add("Data", "Data");
        grid.Columns.Add("Tipo", "Tipo");
        grid.Columns.Add("Descricao", "Descrição");
        grid.Columns.Add("Valor", "Valor");
        grid.Columns.Add("Saldo", "Saldo");

        decimal saldo = 0;

        var listaOrdenada = dados
            .Where(d => d.Length >= 4)
            .OrderBy(d => DateTime.TryParse(d[0], out var dt) ? dt : DateTime.MinValue)
            .ToList();

        foreach (var d in listaOrdenada)
        {
            if (!decimal.TryParse(d[3], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal valor))
                continue;

            if (d[1] == "ENTRADA")
                saldo += valor;
            else
                saldo -= valor;

            grid.Rows.Add(
                d[0],
                d[1],
                d[2],
                valor.ToString("C2", new CultureInfo("pt-BR")),
                saldo.ToString("C2", new CultureInfo("pt-BR"))
            );
        }

        ColorirGrid();
    }

    void ColorirGrid()
    {
        foreach (DataGridViewRow row in grid.Rows)
        {
            if (row.Cells["Tipo"].Value?.ToString() == "ENTRADA")
                row.DefaultCellStyle.ForeColor = Color.LightGreen;
            else
                row.DefaultCellStyle.ForeColor = Color.Salmon;
        }
    }

    void Adicionar()
    {
        if (cmbTipo.SelectedItem == null || string.IsNullOrWhiteSpace(txtDescricao.Text))
        {
            MessageBox.Show("Preencha todos os campos!");
            return;
        }

        if (!decimal.TryParse(txtValor.Text, NumberStyles.Any, new CultureInfo("pt-BR"), out decimal valor) || valor <= 0)
        {
            MessageBox.Show("Valor inválido!");
            return;
        }

        if (linhaSelecionada != -1 && dados[linhaSelecionada][2] == "MANUTENCAO")
        {
            MessageBox.Show("Este lançamento vem da manutenção e não pode ser editado!");
            return;
        }

        string data = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        string tipo = cmbTipo.SelectedItem.ToString();
        string desc = txtDescricao.Text.Replace(",", " ");
        string valorSalvar = valor.ToString(CultureInfo.InvariantCulture);

        if (linhaSelecionada == -1)
            dados.Add(new string[] { data, tipo, desc, valorSalvar });
        else
            dados[linhaSelecionada] = new string[] { data, tipo, desc, valorSalvar };

        SalvarArquivo();
        MessageBox.Show("Lançamento gravado!");
        Limpar();
    }

    void Selecionar()
    {
        if (grid.CurrentRow == null || grid.CurrentRow.Index < 0) return;

        string data = grid.CurrentRow.Cells["Data"].Value.ToString();

        linhaSelecionada = dados.FindIndex(d => d[0] == data);

        if (linhaSelecionada == -1) return;

        var d = dados[linhaSelecionada];

        cmbTipo.SelectedItem = d[1];
        txtDescricao.Text = d[2];
        txtValor.Text = d[3].Replace(".", ",");
    }

    void Excluir()
    {
        if (grid.CurrentRow == null || linhaSelecionada == -1)
        {
            MessageBox.Show("Selecione um registro para excluir!");
            return;
        }

        if (dados[linhaSelecionada][2] == "MANUTENCAO")
        {
            MessageBox.Show("Este lançamento vem da manutenção e não pode ser excluído!");
            return;
        }

        if (MessageBox.Show("Deseja excluir este lançamento?", "Confirmação", MessageBoxButtons.YesNo) == DialogResult.Yes)
        {
            dados.RemoveAt(linhaSelecionada);
            SalvarArquivo();
            Limpar();
        }
    }

    void SalvarArquivo()
    {
        var linhas = new List<string> { "Data,Tipo,Descricao,Valor" };

        foreach (var d in dados)
            linhas.Add(string.Join(",", d));

        File.WriteAllLines("dados/caixa.csv", linhas);

        Atualizar();
        CalcularSaldo();
    }

    void CalcularSaldo()
    {
        decimal entrada = 0, saida = 0;

        foreach (var d in dados)
        {
            if (d.Length < 4) continue;

            if (decimal.TryParse(d[3], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal v))
            {
                if (d[1] == "ENTRADA") entrada += v;
                else saida += v;
            }
        }

        decimal saldo = entrada - saida;

        lblEntradas.Text = $"Total Entradas: {entrada.ToString("C2", new CultureInfo("pt-BR"))}";
        lblSaidas.Text = $"Total Saídas: {saida.ToString("C2", new CultureInfo("pt-BR"))}";
        lblSaldo.Text = $"Saldo Atual: {saldo.ToString("C2", new CultureInfo("pt-BR"))}";

        lblEntradas.ForeColor = Color.LightGreen;
        lblSaidas.ForeColor = Color.Salmon;
        lblSaldo.ForeColor = saldo >= 0 ? Color.LightGreen : Color.Salmon;
    }

    void Limpar()
    {
        cmbTipo.SelectedIndex = -1;
        txtDescricao.Clear();
        txtValor.Clear();
        linhaSelecionada = -1;
        txtDescricao.Focus();
    }
}