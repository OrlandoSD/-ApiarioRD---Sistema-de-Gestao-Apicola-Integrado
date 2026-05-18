using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Globalization;
using System.Collections.Generic;
using System.Drawing;

public class RelatorioForm : FormBase
{
    ComboBox cmbMes = new ComboBox() { Top = 10, Left = 10, Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
    ComboBox cmbAno = new ComboBox() { Top = 10, Left = 140, Width = 100, DropDownStyle = ComboBoxStyle.DropDownList };

    Button btnGerar = new Button() { Text = "Gerar Extrato", Top = 40, Left = 10, Width = 120 };
    Button btnVendasDia = new Button() { Text = "Vendas por Dia", Top = 40, Left = 140, Width = 120 };
    Button btnExportar = new Button() { Text = "Exportar para Calc", Top = 40, Left = 270, Width = 150 };

    Label lblEntrada = new Label() { Top = 80, Left = 10, Width = 500 };
    Label lblSaida = new Label() { Top = 110, Left = 10, Width = 500 };
    Label lblSaldo = new Label() { Top = 140, Left = 10, Width = 500, Font = new Font("Segoe UI", 11, FontStyle.Bold) };

    DataGridView grid = new DataGridView();

    public RelatorioForm()
    {
        Text = "ApiarioRD - Relatórios 📋";
        Width = 600; Height = 520;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(45, 45, 45);
        ForeColor = Color.White;

        grid.SetBounds(10, 180, 560, 280);
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.BackgroundColor = Color.FromArgb(30, 30, 30);
        grid.RowHeadersVisible = false;
        grid.ReadOnly = true;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.MultiSelect = true; 
        grid.DefaultCellStyle.SelectionBackColor = Color.DarkCyan;
        grid.DefaultCellStyle.SelectionForeColor = Color.White;

        Controls.AddRange(new Control[] { cmbMes, cmbAno, btnGerar, btnVendasDia, btnExportar, lblEntrada, lblSaida, lblSaldo, grid });

        btnGerar.Click += (s, e) => GerarRelatorio(false);
        btnVendasDia.Click += (s, e) => GerarRelatorio(true);
        btnExportar.Click += (s, e) => ExportarParaCalc();

        try { TemaHelper.Aplicar(this); } catch { }

        CarregarFiltros();
    }

    void CarregarFiltros()
    {
        cmbMes.Items.AddRange(new string[] { "01","02","03","04","05","06","07","08","09","10","11","12" });

        int anoAtual = DateTime.Now.Year;
        for (int i = anoAtual; i >= anoAtual - 5; i--)
            cmbAno.Items.Add(i.ToString());

        cmbMes.SelectedIndex = DateTime.Now.Month - 1;
        cmbAno.SelectedItem = anoAtual.ToString();
    }

    void GerarRelatorio(bool porDia)
    {
        if (cmbMes.SelectedItem == null || cmbAno.SelectedItem == null) return;

        string mes = cmbMes.SelectedItem.ToString();
        string ano = cmbAno.SelectedItem.ToString();

        decimal entradas = 0, saidas = 0;

        var lista = new List<(DateTime Data, string Tipo, string Descricao, decimal Valor)>();
        CultureInfo br = new CultureInfo("pt-BR");

        // VENDAS
        if (File.Exists("dados/vendas.csv"))
        {
            foreach (var v in File.ReadAllLines("dados/vendas.csv").Skip(1))
            {
                if (string.IsNullOrWhiteSpace(v)) continue;

                var c = v.Split(',');
                if (c.Length < 5) continue;

                if (!DateTime.TryParse(c[1], br, DateTimeStyles.None, out DateTime data)) continue;

                if (data.Month.ToString("00") == mes && data.Year.ToString() == ano)
                {
                    if (decimal.TryParse(c[4], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal valor))
                    {
                        entradas += valor;
                        lista.Add((data, "ENTRADA", "VENDA", valor));
                    }
                }
            }
        }

        // CAIXA
        if (File.Exists("dados/caixa.csv"))
        {
            foreach (var l in File.ReadAllLines("dados/caixa.csv").Skip(1))
            {
                if (string.IsNullOrWhiteSpace(l)) continue;

                var c = l.Split(',');
                if (c.Length < 4) continue;

                if (!DateTime.TryParse(c[0], br, DateTimeStyles.None, out DateTime data)) continue;

                if (data.Month.ToString("00") == mes && data.Year.ToString() == ano)
                {
                    if (c[2].ToUpper().StartsWith("VENDA")) continue;

                    if (!decimal.TryParse(c[3], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal valor)) continue;

                    if (c[1] == "ENTRADA") entradas += valor;
                    else saidas += valor;

                    lista.Add((data, c[1], c[2], valor));
                }
            }
        }

        decimal lucro = entradas - saidas;

        lblEntrada.Text = $"💰 Receita: {entradas.ToString("C2", br)}";
        lblSaida.Text = $"💸 Custos: {saidas.ToString("C2", br)}";
        lblSaldo.Text = $"📊 Lucro: {lucro.ToString("C2", br)}";

        lblEntrada.ForeColor = Color.LightGreen;
        lblSaida.ForeColor = Color.Salmon;
        lblSaldo.ForeColor = lucro >= 0 ? Color.LightGreen : Color.Salmon;

        grid.DataSource = null;

        if (porDia)
        {
            grid.DataSource = lista
                .GroupBy(x => x.Data.Date)
                .Select(g => new
                {
                    Data = g.Key.ToString("dd/MM/yyyy"),
                    Total = g.Sum(x => x.Valor).ToString("C2", br)
                })
                .OrderBy(x => DateTime.Parse(x.Data))
                .ToList();
        }
        else
        {
            var dadosOrdenados = lista.OrderBy(x => x.Data).ToList();

            grid.DataSource = dadosOrdenados.Select(x => new
            {
                Data = x.Data.ToString("dd/MM/yyyy"),
                x.Tipo,
                x.Descricao,
                Valor = x.Valor.ToString("C2", br)
            }).ToList();

            ColorirGrid(dadosOrdenados);
        }
    }

    void ColorirGrid(List<(DateTime Data, string Tipo, string Descricao, decimal Valor)> dados)
    {
        for (int i = 0; i < grid.Rows.Count; i++)
        {
            if (dados[i].Tipo == "ENTRADA")
                grid.Rows[i].DefaultCellStyle.ForeColor = Color.LightGreen;
            else
                grid.Rows[i].DefaultCellStyle.ForeColor = Color.Salmon;
        }
    }

    void ExportarParaCalc()
    {
        if (grid.Rows.Count == 0)
        {
            MessageBox.Show("Gere o relatório primeiro!");
            return;
        }

        string pasta = "relatorios_exportados";
        Directory.CreateDirectory(pasta);

        string caminho = Path.Combine(pasta, $"Relatorio_{DateTime.Now:yyyyMMddHHmmss}.csv");

        var linhas = new List<string>();

        linhas.Add(string.Join(";", grid.Columns.Cast<DataGridViewColumn>().Select(c => c.HeaderText)));

        foreach (DataGridViewRow row in grid.Rows)
        {
            if (row.IsNewRow) continue;

            linhas.Add(string.Join(";", row.Cells.Cast<DataGridViewCell>().Select(c => c.Value?.ToString())));
        }

        File.WriteAllLines(caminho, linhas);

        MessageBox.Show("Exportado com sucesso!");
        System.Diagnostics.Process.Start("explorer.exe", pasta);
    }
}