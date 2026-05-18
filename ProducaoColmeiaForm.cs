using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;

public class ProducaoColmeiaForm : FormBase
{
    DataGridView grid = new DataGridView();
    Button btnAtualizar = new Button() { Text = "Atualizar", Top = 440, Left = 10, Width = 100 };
    Label lblTotal = new Label();
    Label lblTop = new Label();
    Label lblMediaGeral = new Label();
    Label lblAlerta = new Label();

    public ProducaoColmeiaForm()
    {
        Text = "Produção por Colmeia 🐝";
        Width = 650;
        Height = 480;
        StartPosition = FormStartPosition.CenterScreen;

        BackColor = Color.FromArgb(45, 45, 45);
        ForeColor = Color.White;

        // Grid
        grid.SetBounds(10, 10, 610, 250);
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.BackgroundColor = Color.FromArgb(30, 30, 30);
        grid.RowHeadersVisible = false;
        grid.ReadOnly = true;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.MultiSelect = true; 
        grid.DefaultCellStyle.SelectionBackColor = Color.DarkCyan;
        grid.DefaultCellStyle.SelectionForeColor = Color.White;

        // Labels
        lblTotal.SetBounds(10, 270, 400, 25);
        lblTop.SetBounds(10, 300, 500, 25);
        lblMediaGeral.SetBounds(10, 330, 400, 25);

        lblAlerta.SetBounds(10, 360, 600, 80);
        lblAlerta.ForeColor = Color.Gold;
        lblAlerta.Font = new Font("Segoe UI", 9, FontStyle.Bold);

        // Botão Atualizar
        btnAtualizar.BackColor = Color.FromArgb(0, 150, 200);
        btnAtualizar.ForeColor = Color.White;
        btnAtualizar.FlatStyle = FlatStyle.Flat;
        btnAtualizar.Font = new Font("Segoe UI", 9, FontStyle.Bold);
        btnAtualizar.Click += (s, e) => Carregar();

        Controls.AddRange(new Control[]
        {
            grid, lblTotal, lblTop, lblMediaGeral, lblAlerta, btnAtualizar
        });

        try { TemaHelper.Aplicar(this); } catch { }

        Carregar();
    }

    void Carregar()
    {
        if (!File.Exists("dados/manejo.csv"))
        {
            MessageBox.Show("Nenhum dado de produção encontrado!");
            return;
        }

        var linhas = File.ReadAllLines("dados/manejo.csv").Skip(1);
        var producao = new Dictionary<string, decimal>();
        var contagem = new Dictionary<string, int>();
        decimal totalGeral = 0;

        foreach (var l in linhas)
        {
            if (string.IsNullOrWhiteSpace(l)) continue;
            var c = l.Split(',');

            int offset = c.Length == 6 ? 1 : 0;
            if (c.Length < 5) continue;

            string tipo = c[2 + offset]?.Trim();
            if (!string.Equals(tipo, "Produção", StringComparison.OrdinalIgnoreCase))
                continue;

            string colmeia = c[1 + offset]?.Trim();
            if (!decimal.TryParse(c[3 + offset], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal qtd))
                continue;

            if (!producao.ContainsKey(colmeia))
            {
                producao[colmeia] = 0;
                contagem[colmeia] = 0;
            }

            producao[colmeia] += qtd;
            contagem[colmeia]++;
            totalGeral += qtd;
        }

        var lista = producao
            .Select(p => new
            {
                Colmeia = p.Key,
                Total = p.Value,
                Registros = contagem[p.Key],
                Media = contagem[p.Key] > 0 ? p.Value / contagem[p.Key] : 0
            })
            .OrderByDescending(x => x.Total)
            .ToList();

        var br = new CultureInfo("pt-BR");

        grid.DataSource = lista.Select(x => new
        {
            Colmeia = x.Colmeia,
            Produção_Total = $"{x.Total:N2}",
            Registros = x.Registros,
            Média = $"{x.Media:N2}"
        }).ToList();

        lblTotal.Text = $"🍯 Produção Total: {totalGeral:N2} Kg";

        if (lista.Count > 0)
        {
            var top = lista.First();
            var pior = lista.Last();
            lblTop.Text = $"🏆 Melhor: {top.Colmeia} ({top.Total:N2} Kg)";

            string alerta = $"⚠️ Atenção: {pior.Colmeia} ({pior.Total:N2} Kg)\n\n";
            foreach (var item in lista)
            {
                if (item.Media < 2)
                    alerta += $"⚠️ {item.Colmeia}: alimentar mais\n";
                if (item.Media > 5)
                    alerta += $"🚀 {item.Colmeia}: alta produção\n";
                if (item.Media < 1)
                    alerta += $"🔴 {item.Colmeia}: risco crítico\n";
            }
            lblAlerta.Text = alerta;
        }

        decimal mediaGeral = lista.Count > 0 ? lista.Average(x => x.Total) : 0;
        lblMediaGeral.Text = $"📊 Média por Colmeia: {mediaGeral:N2} Kg";

        // Colore as linhas baseado na produção
        foreach (DataGridViewRow row in grid.Rows)
        {
            if (row.Cells["Produção_Total"].Value != null)
            {
                if (decimal.TryParse(row.Cells["Produção_Total"].Value.ToString(), NumberStyles.Any, br, out decimal valor))
                {
                    if (valor < 2)
                        row.DefaultCellStyle.BackColor = Color.DarkRed;
                    else if (valor < 5)
                        row.DefaultCellStyle.BackColor = Color.DarkOrange;
                    else
                        row.DefaultCellStyle.BackColor = Color.DarkGreen;
                }
            }
        }
    }
}