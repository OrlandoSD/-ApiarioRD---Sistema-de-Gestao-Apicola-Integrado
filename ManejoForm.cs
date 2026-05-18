using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;

public class ManejoForm : FormBase
{
    ComboBox comboColmeia = new ComboBox() { DropDownStyle = ComboBoxStyle.DropDownList };
    ComboBox comboTipo = new ComboBox() { DropDownStyle = ComboBoxStyle.DropDownList };

    TextBox txtQtd = new TextBox();
    TextBox txtObs = new TextBox();

    Button btnSalvar = new Button();
    Button btnExcluir = new Button();
    Button btnNovo = new Button();

    DataGridView grid = new DataGridView();

    Label lblIA = new Label();

    List<string[]> dados = new List<string[]>();
    List<string[]> colmeias = new List<string[]>();

    string caminho = "dados/manejo.csv";

    int idSelecionado = -1;

    public ManejoForm()
    {
        Text = "Manejo e Produção 🐝";
        Width = 650;
        Height = 520;
        FormBorderStyle = FormBorderStyle.None;

        BackColor = Color.FromArgb(45,45,45);
        ForeColor = Color.White;

        comboColmeia.SetBounds(10, 20, 200, 25);
        comboTipo.SetBounds(220, 20, 150, 25);

        txtQtd.SetBounds(380, 20, 80, 25);
        txtQtd.PlaceholderText = "Qtd (Kg)";
        
        txtObs.SetBounds(470, 20, 150, 25);
        txtObs.PlaceholderText = "Observações...";

        btnSalvar.Text = "💾 Salvar";
        btnSalvar.SetBounds(10, 60, 120, 35);

        btnExcluir.Text = "🗑 Excluir";
        btnExcluir.SetBounds(140, 60, 120, 35);

        btnNovo.Text = "➕ Novo";
        btnNovo.SetBounds(270, 60, 120, 35);

        grid.SetBounds(10, 110, 610, 250);
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.BackgroundColor = Color.FromArgb(30, 30, 30);
        grid.RowHeadersVisible = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

        lblIA.SetBounds(10, 370, 610, 100);
        lblIA.ForeColor = Color.Gold;
        lblIA.Font = new Font("Segoe UI", 9, FontStyle.Bold);

        comboTipo.Items.AddRange(new string[]
        {
            "Produção",
            "Alimentação",
            "Revisão",
            "Troca de Rainha",
            "Tratamento"
        });

        Controls.AddRange(new Control[] {
            comboColmeia, comboTipo, txtQtd, txtObs,
            btnSalvar, btnExcluir, btnNovo,
            grid, lblIA
        });

        btnSalvar.Click += (s, e) => Salvar();
        btnExcluir.Click += (s, e) => Excluir();
        btnNovo.Click += (s, e) => Limpar();

        grid.CellClick += (s, e) => Selecionar();

        try { TemaHelper.Aplicar(this); } catch { }

        CarregarColmeias();
        CarregarHistorico();
    }

    void CarregarColmeias()
    {
        comboColmeia.Items.Clear();
        colmeias.Clear();

        if (!File.Exists("dados/colmeias.csv")) return;

        foreach (var l in File.ReadAllLines("dados/colmeias.csv").Skip(1))
        {
            if (string.IsNullOrWhiteSpace(l)) continue;

            var c = l.Split(',');

            if (c.Length < 2) continue;

            colmeias.Add(c);
            comboColmeia.Items.Add(c[1]);
        }
    }

    void Salvar()
    {
        if (comboColmeia.SelectedIndex < 0 || comboTipo.SelectedItem == null)
        {
            MessageBox.Show("Selecione a Colmeia e o Tipo!");
            return;
        }

        string qtdTexto = txtQtd.Text.Replace(",", ".");
        if (!decimal.TryParse(qtdTexto, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal qtd) || qtd <= 0)
        {
            MessageBox.Show("Quantidade inválida!");
            return;
        }

        Directory.CreateDirectory("dados");

        if (!File.Exists(caminho))
            File.WriteAllText(caminho, "Id,Data,Colmeia,Tipo,Quantidade,Observacao\n");

        string data = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        string obs = txtObs.Text.Replace(",", " ");

        if (idSelecionado == -1)
        {
            int novoId = dados.Count > 0 ? dados.Max(d => int.Parse(d[0])) + 1 : 1;

            dados.Add(new string[] {
                novoId.ToString(),
                data,
                comboColmeia.Text,
                comboTipo.Text,
                qtd.ToString(CultureInfo.InvariantCulture),
                obs
            });
        }
        else
        {
            var item = dados.FirstOrDefault(d => d[0] == idSelecionado.ToString());

            if (item != null)
            {
                item[2] = comboColmeia.Text;
                item[3] = comboTipo.Text;
                item[4] = qtd.ToString(CultureInfo.InvariantCulture);
                item[5] = obs;
            }
        }

        // 🔥 estoque automático
        if (comboTipo.Text == "Produção")
        {
            try { EstoqueService.MovimentarEstoque("Mel", qtd, "Entrada"); }
            catch { }
        }

        SalvarArquivo();
        Limpar();
    }

    void Excluir()
    {
        if (idSelecionado == -1)
        {
            MessageBox.Show("Selecione um registro!");
            return;
        }

        if (MessageBox.Show("Excluir registro?", "Confirmação", MessageBoxButtons.YesNo) == DialogResult.Yes)
        {
            dados.RemoveAll(d => d[0] == idSelecionado.ToString());
            SalvarArquivo();
            Limpar();
        }
    }

    void Selecionar()
    {
        if (grid.CurrentRow == null) return;

        idSelecionado = int.Parse(grid.CurrentRow.Cells["Id"].Value.ToString());

        var item = dados.First(d => d[0] == idSelecionado.ToString());

        comboColmeia.Text = item[2];
        comboTipo.Text = item[3];
        txtQtd.Text = item[4];
        txtObs.Text = item[5];
    }

    void CarregarHistorico()
    {
        dados.Clear();

        if (!File.Exists(caminho)) return;

        foreach (var l in File.ReadAllLines(caminho).Skip(1))
        {
            if (string.IsNullOrWhiteSpace(l)) continue;

            var c = l.Split(',');
            if (c.Length < 6) continue;

            dados.Add(c);
        }

        grid.DataSource = null;
        grid.DataSource = dados.Select(d => new
        {
            Id = d[0],
            Data = d[1],
            Colmeia = d[2],
            Tipo = d[3],
            Qtd = d[4],
            Observação = d[5]
        }).OrderByDescending(x => x.Data).ToList();

        AnalisarIA();
    }

    void SalvarArquivo()
    {
        var linhas = new List<string> { "Id,Data,Colmeia,Tipo,Quantidade,Observacao" };

        foreach (var d in dados)
            linhas.Add(string.Join(",", d));

        File.WriteAllLines(caminho, linhas);

        CarregarHistorico();
    }

    // 🧠 IA EMPRESARIAL MELHORADA
    void AnalisarIA()
    {
        var producao = dados
            .Where(d => d[3] == "Produção")
            .GroupBy(d => d[2])
            .Select(g => new
            {
                Colmeia = g.Key,
                Total = g.Sum(x => decimal.Parse(x[4], CultureInfo.InvariantCulture)),
                Media = g.Average(x => decimal.Parse(x[4], CultureInfo.InvariantCulture)),
                Registros = g.Count()
            })
            .OrderByDescending(x => x.Total)
            .ToList();

        if (producao.Count == 0)
        {
            lblIA.Text = "Sem dados de produção.";
            return;
        }

        string texto = "";

        var melhor = producao.First();
        var pior = producao.Last();

        texto += $"🏆 Melhor: {melhor.Colmeia} ({melhor.Total:N2}kg)\n";
        texto += $"⚠️ Pior: {pior.Colmeia} ({pior.Total:N2}kg)\n\n";

        foreach (var c in producao)
        {
            double previsao = (double)c.Media * 1.15;

            if (c.Media < 2)
                texto += $"⚠️ {c.Colmeia}: alimentar mais\n";

            if (c.Media > 5)
                texto += $"🚀 {c.Colmeia}: investir\n";

            texto += $"📊 {c.Colmeia}: previsão {previsao:N2}kg\n";
        }

        lblIA.Text = texto;
    }

    void Limpar()
    {
        txtQtd.Clear();
        txtObs.Clear();
        comboTipo.SelectedIndex = -1;
        idSelecionado = -1;
        comboColmeia.Focus();
    }
}