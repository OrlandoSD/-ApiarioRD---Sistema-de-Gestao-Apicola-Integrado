using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Globalization;
using System.Collections.Generic;
using System.Drawing;

public class VendasForm : FormBase
{
    ComboBox combo = new ComboBox();
    TextBox txtQtd = new TextBox();
    Label lblPreco = new Label();
    Label lblTotal = new Label();
    Label lblProduto = new Label();
    Label lblQuantidade = new Label();
    Button btnVender = new Button();
    Button btnCancelar = new Button();

    List<string[]> produtosAtuais = new List<string[]>();

    public VendasForm()
    {
        Text = "ApiarioRD - Frente de Vendas 💰";
        Width = 450;
        Height = 380;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(45, 45, 45);
        ForeColor = Color.White;

        Panel painel = new Panel()
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(50, 50, 50)
        };
        Controls.Add(painel);

        lblProduto.Text = "Selecione o Produto:";
        lblProduto.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        lblProduto.ForeColor = Color.White;
        lblProduto.SetBounds(10, 10, 300, 25);

        lblQuantidade.Text = "Quantidade:";
        lblQuantidade.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        lblQuantidade.ForeColor = Color.White;
        lblQuantidade.SetBounds(10, 80, 200, 25);

        lblPreco.Text = "Preço Unitário: R$ 0,00";
        lblPreco.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        lblPreco.ForeColor = Color.LightBlue;
        lblPreco.SetBounds(10, 130, 300, 25);

        lblTotal.Text = "Total: R$ 0,00";
        lblTotal.Font = new Font("Segoe UI", 12, FontStyle.Bold);
        lblTotal.ForeColor = Color.LightGreen;
        lblTotal.SetBounds(10, 160, 300, 30);

        combo.SetBounds(10, 40, 410, 30);
        combo.DropDownStyle = ComboBoxStyle.DropDownList;
        txtQtd.SetBounds(10, 110, 100, 30);

        btnVender.Text = "Finalizar Venda";
        btnVender.SetBounds(10, 210, 200, 40);
        btnCancelar.Text = "Limpar";
        btnCancelar.SetBounds(220, 210, 200, 40);

        EstilizarBotao(btnVender, Color.FromArgb(0, 150, 0));
        EstilizarBotao(btnCancelar, Color.FromArgb(150, 0, 0));

        btnVender.Click += (s, e) => Vender();
        btnCancelar.Click += (s, e) => Limpar();
        combo.SelectedIndexChanged += (s, e) => AtualizarPreco();
        txtQtd.TextChanged += (s, e) => AtualizarTotal();

        painel.Controls.AddRange(new Control[] {
            lblProduto, combo, lblQuantidade, txtQtd,
            lblPreco, lblTotal, btnVender, btnCancelar
        });

        CarregarProdutos();
    }

    void EstilizarBotao(Button btn, Color cor)
    {
        btn.BackColor = cor;
        btn.ForeColor = Color.White;
        btn.FlatStyle = FlatStyle.Flat;
        btn.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        btn.MouseEnter += (s, e) => btn.BackColor = ControlPaint.Light(cor);
        btn.MouseLeave += (s, e) => btn.BackColor = cor;
    }

    void CarregarProdutos()
    {
        combo.Items.Clear();
        produtosAtuais.Clear();

        if (!File.Exists("dados/produtos.csv")) return;

        var linhas = File.ReadAllLines("dados/produtos.csv")
                         .Skip(1)
                         .Where(l => !string.IsNullOrWhiteSpace(l));

        foreach (var l in linhas)
        {
            var p = l.Split(',');

            if (p.Length < 4) continue;

            // 🔥 GARANTE COLUNA DE CUSTO
            if (p.Length < 5)
            {
                var novo = new string[5];
                Array.Copy(p, novo, p.Length);
                novo[4] = "0";
                p = novo;
            }

            produtosAtuais.Add(p);

            decimal estoque = EstoqueService.ObterQuantidade(p[1]);
            string status = estoque <= 0 ? "🔴" : (estoque <= 5 ? "🟡" : "🟢");

            combo.Items.Add($"{p[1]} - R$ {p[2]} | Estoque: {estoque} {status}");
        }
    }

    void AtualizarPreco()
    {
        if (combo.SelectedIndex < 0) return;

        var p = produtosAtuais[combo.SelectedIndex];
        lblPreco.Text = $"Preço Unitário: R$ {p[2]}";
        txtQtd.Focus();
        AtualizarTotal();
    }

    void AtualizarTotal()
    {
        if (combo.SelectedIndex < 0) return;

        var p = produtosAtuais[combo.SelectedIndex];

        if (!decimal.TryParse(txtQtd.Text, out decimal qtd))
        {
            lblTotal.Text = "Total: R$ 0,00";
            return;
        }

        if (decimal.TryParse(p[2], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal preco))
        {
            lblTotal.Text = $"Total: {(preco * qtd).ToString("C2", new CultureInfo("pt-BR"))}";
        }
    }

    void Vender()
    {
        if (combo.SelectedIndex < 0)
        {
            MessageBox.Show("Selecione um produto!");
            return;
        }

        var produto = produtosAtuais[combo.SelectedIndex];

        if (!decimal.TryParse(txtQtd.Text, out decimal qtdVenda) || qtdVenda <= 0)
        {
            MessageBox.Show("Informe a quantidade!");
            return;
        }

        decimal estoqueAtual = EstoqueService.ObterQuantidade(produto[1]);
        if (qtdVenda > estoqueAtual)
        {
            MessageBox.Show($"Estoque insuficiente! Disponível: {estoqueAtual}");
            return;
        }

        decimal preco = decimal.Parse(produto[2], CultureInfo.InvariantCulture);
        decimal total = preco * qtdVenda;

        // 🔥 CUSTO REAL DO PRODUTO
        decimal custoUnitario = 0;
        if (produto.Length >= 5)
        {
            decimal.TryParse(produto[4], NumberStyles.Any, CultureInfo.InvariantCulture, out custoUnitario);
        }

        decimal custoTotal = custoUnitario * qtdVenda;
        decimal lucro = total - custoTotal;

        if (MessageBox.Show($"Confirmar venda?\nTotal: R$ {total:N2}", "Confirmação", MessageBoxButtons.YesNo) != DialogResult.Yes)
            return;

        try
        {
            // 🔻 BAIXA NO ESTOQUE
            EstoqueService.MovimentarEstoque(produto[1], qtdVenda, "Saida");

            // 🔻 SALVA VENDA
            if (!File.Exists("dados/vendas.csv"))
                File.WriteAllText("dados/vendas.csv", "Id,Data,Produtoid,Quantidade,ValorTotal" + Environment.NewLine);

            string idVenda = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
            string data = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

            string linhaVenda = $"{idVenda},{data},{produto[0]},{qtdVenda},{total.ToString(CultureInfo.InvariantCulture)}";
            File.AppendAllText("dados/vendas.csv", linhaVenda + Environment.NewLine);

            // 🔥 CAIXA
            CaixaService.Adicionar(
                "ENTRADA",
                $"VENDA: {produto[1]}",
                total
            );

            MessageBox.Show(
                $"Venda realizada!\n\n" +
                $"Total: R$ {total:N2}\n" +
                $"Custo: R$ {custoTotal:N2}\n" +
                $"Lucro: R$ {lucro:N2}"
            );

            Limpar();
            CarregarProdutos();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Erro ao vender: " + ex.Message);
        }
    }

    void Limpar()
    {
        combo.SelectedIndex = -1;
        txtQtd.Clear();
        lblPreco.Text = "Preço Unitário: R$ 0,00";
        lblTotal.Text = "Total: R$ 0,00";
    }

    public void AbrirDashboard() => new DashboardForm().Show();
    public void AbrirInsumos() => new InsumosForm().Show();
    public void AbrirEstoque() => new EstoqueForm().Show();
    public void AbrirCaixa() => new CaixaForm().Show();
    public void AbrirManutencao() => new ManutencaoForm().Show();
}