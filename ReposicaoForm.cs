using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;

public class ReposicaoForm : FormBase
{
    ComboBox combo = new ComboBox();
    TextBox txtQtd = new TextBox();

    Label lblInfo = new Label();

    Button btnAdd = new Button();

    List<string[]> produtos = new List<string[]>();

    public ReposicaoForm()
    {
        Text = "Reposição de Estoque 📦";
        Width = 380;
        Height = 260;

        BackColor = Color.FromArgb(45,45,45);
        ForeColor = Color.White;

        combo.SetBounds(10,20,340,25);

        txtQtd.SetBounds(10,60,150,25);
        txtQtd.PlaceholderText = "Quantidade";

        lblInfo.SetBounds(10,95,340,25);
        lblInfo.Font = new Font("Segoe UI", 10, FontStyle.Bold);

        btnAdd.Text = "➕ Adicionar Estoque";
        btnAdd.SetBounds(10,130,220,35);
        btnAdd.BackColor = Color.FromArgb(0, 120, 215);
        btnAdd.FlatStyle = FlatStyle.Flat;

        Controls.Add(combo);
        Controls.Add(txtQtd);
        Controls.Add(lblInfo);
        Controls.Add(btnAdd);

        btnAdd.Click += (s, e) => Adicionar();
        combo.SelectedIndexChanged += (s,e)=>AtualizarInfo();

        // UX melhor
        txtQtd.KeyDown += (s,e)=> { if(e.KeyCode == Keys.Enter) Adicionar(); };

        try { TemaHelper.Aplicar(this); } catch { }

        Carregar();
    }

    void Carregar()
    {
        combo.Items.Clear();
        produtos.Clear();

        if (!File.Exists("dados/produtos.csv")) return;

        var linhas = File.ReadAllLines("dados/produtos.csv").Skip(1);

        foreach (var l in linhas)
        {
            if (string.IsNullOrWhiteSpace(l)) continue;

            var p = l.Split(',');

            if (p.Length < 2) continue;

            produtos.Add(p);

            string nome = p[1];

            decimal estoque = 0;

            try
            {
                estoque = EstoqueService.ObterQuantidade(nome);
            }
            catch { }

            string status = estoque <= 0 ? "🔴" :
                            estoque <= 5 ? "🟡" : "🟢";

            combo.Items.Add($"{nome} ({status})");
        }
    }

    string[] ObterProduto()
    {
        if (combo.SelectedIndex < 0 || combo.SelectedIndex >= produtos.Count)
            return null;

        return produtos[combo.SelectedIndex];
    }

    void AtualizarInfo()
    {
        var p = ObterProduto();
        if (p == null) return;

        string nome = p[1];

        decimal estoque = 0;

        try
        {
            estoque = EstoqueService.ObterQuantidade(nome);
        }
        catch { }

        lblInfo.Text = $"📊 Estoque atual: {estoque}";

        // 🎨 Cor dinâmica
        if (estoque <= 0) lblInfo.ForeColor = Color.Red;
        else if (estoque <= 5) lblInfo.ForeColor = Color.Gold;
        else lblInfo.ForeColor = Color.LightGreen;
    }

    void Adicionar()
    {
        var p = ObterProduto();

        if (p == null)
        {
            MessageBox.Show("Selecione um produto!");
            return;
        }

        string texto = txtQtd.Text.Replace(",", ".");

        if (!decimal.TryParse(texto, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal qtd) || qtd <= 0)
        {
            MessageBox.Show("Quantidade inválida!");
            txtQtd.Focus();
            return;
        }

        string nome = p[1];

        try
        {
            // 🔥 INTEGRAÇÃO COM ESTOQUE
            EstoqueService.MovimentarEstoque(nome, qtd, "Entrada");

            MessageBox.Show("✅ Estoque atualizado com sucesso!");

            txtQtd.Clear();
            txtQtd.Focus();

            AtualizarInfo();
            Carregar();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Erro ao atualizar estoque:\n" + ex.Message);
        }
    }
}