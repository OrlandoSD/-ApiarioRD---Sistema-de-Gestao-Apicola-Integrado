using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;

public class ProdutosForm : FormBase
{
    DataGridView grid = new DataGridView();

    Label lblNome = new Label(){Text="Produto:", Top=220, Left=10, AutoSize = true};
    Label lblPreco = new Label(){Text="Preço (R$):", Top=220, Left=180, AutoSize = true};

    TextBox txtNome = new TextBox(){Top=240, Left=10, Width=150};
    TextBox txtPreco = new TextBox(){Top=240, Left=180, Width=100};

    Button btnNovo = new Button(){Text="Novo", Top=280, Left=10, Width=80};
    Button btnSalvar = new Button(){Text="Salvar", Top=280, Left=100, Width=80};
    Button btnExcluir = new Button(){Text="Excluir", Top=280, Left=190, Width=80};

    List<string[]> dados = new List<string[]>();
    int idSelecionado = 0;

    public ProdutosForm()
    {
        Text="ApiarioRD - Gerenciar Produtos 🍯";
        Width=500; Height=380;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(45, 45, 45);
        ForeColor = Color.White;

        grid.SetBounds(10,10,460,200);
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.ReadOnly = true;
        grid.RowHeadersVisible = false;

        // 🔹 CONFIGURAÇÃO PARA SELEÇÃO COMPLETA DE LINHA
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect; // seleciona a linha inteira ao clicar
        grid.MultiSelect = true; // opcional: permite apenas uma linha selecionada por vez
        grid.DefaultCellStyle.SelectionBackColor = Color.DarkCyan; // cor de fundo da linha selecionada
        grid.DefaultCellStyle.SelectionForeColor = Color.White;   // cor da fonte da linha selecionada

        Controls.AddRange(new Control[] { grid, lblNome, lblPreco, txtNome, txtPreco, btnNovo, btnSalvar, btnExcluir });

        btnNovo.Click += (s,e)=>Limpar();
        btnSalvar.Click += (s,e)=>Salvar();
        btnExcluir.Click += (s,e)=>Excluir();
        grid.CellClick += (s,e)=>Selecionar();

        try { TemaHelper.Aplicar(this); } catch { }

        Carregar();
    }

    void Carregar()
    {
        Directory.CreateDirectory("dados");
        string arquivo = "dados/produtos.csv";

        if(!File.Exists(arquivo))
            File.WriteAllText(arquivo,"Id,Nome,Preco,Estoque" + Environment.NewLine);

        dados.Clear();
        var linhas = File.ReadAllLines(arquivo).Skip(1).Where(l => !string.IsNullOrWhiteSpace(l));
        
        foreach (var l in linhas) dados.Add(l.Split(','));

        GarantirProdutosPadrao(); // 🔥 adiciona isso
        AtualizarGrid();
    }

    void AtualizarGrid()
    {
        grid.DataSource = null;

        // Filtramos apenas linhas que tenham exatamente 4 colunas (Id, Nome, Preco, Estoque)
        // Isso evita o erro de "Index was outside the bounds"
        var dadosValidos = dados.Where(d => d.Length >= 4).Select(d => new {
            Id = d[0],
            Nome = d[1],
            Preço = decimal.TryParse(d[2], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal p) 
                    ? p.ToString("C2", new CultureInfo("pt-BR")) : d[2],
            Estoque = d[3]
        }).ToList();

        grid.DataSource = dadosValidos;
    }


    void Salvar()
    {
        if(string.IsNullOrWhiteSpace(txtNome.Text)) { MessageBox.Show("Informe o nome!"); return; }

        // Aceita vírgula ou ponto na digitação
        string valorTexto = txtPreco.Text.Replace(",", ".");
        if(!decimal.TryParse(valorTexto, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal preco))
        {
            MessageBox.Show("Preço inválido!");
            return;
        }

        string nomeLimpo = txtNome.Text.Replace(",", " "); // Protege o CSV

        if(idSelecionado == 0) // NOVO
        {
            int proxId = dados.Count > 0 ? dados.Max(d => int.Parse(d[0])) + 1 : 1;
            dados.Add(new string[] { proxId.ToString(), nomeLimpo, preco.ToString(CultureInfo.InvariantCulture), "0" });
        }
        else // EDITAR
        {
            var item = dados.FirstOrDefault(d => d[0] == idSelecionado.ToString());
            if(item != null) { item[1] = nomeLimpo; item[2] = preco.ToString(CultureInfo.InvariantCulture); }
        }

        SalvarArquivo();
        Limpar();
    }

    void Selecionar()
    {
        if(grid.CurrentRow == null) return;
        idSelecionado = int.Parse(grid.CurrentRow.Cells["Id"].Value.ToString());
        txtNome.Text = grid.CurrentRow.Cells["Nome"].Value.ToString();
        
        // Exibe o preço com vírgula para facilitar a edição
        string precoGrid = grid.CurrentRow.Cells["Preço"].Value.ToString().Replace("R$", "").Trim();
        txtPreco.Text = precoGrid;
    }

    void Excluir()
    {
        if(idSelecionado == 0) { MessageBox.Show("Selecione um produto na lista!"); return; }

        if(MessageBox.Show("Deseja excluir este produto?", "Aviso", MessageBoxButtons.YesNo) == DialogResult.Yes)
        {
            dados.RemoveAll(d => d[0] == idSelecionado.ToString());
            SalvarArquivo();
            Limpar();
        }
    }

    void SalvarArquivo()
    {
        var linhas = new List<string>{"Id,Nome,Preco,Estoque"};
        foreach(var d in dados) linhas.Add(string.Join(",", d));
        File.WriteAllLines("dados/produtos.csv", linhas);
        AtualizarGrid();
    }

    void Limpar()
    {
        txtNome.Clear();
        txtPreco.Clear();
        idSelecionado = 0;
        txtNome.Focus();
    }

    void GarantirProdutosPadrao()
    {
        string[] essenciais = new string[]
        {
            "Mel",
            "Pote 1kg",
            "Pote 500g",
            "Pote 250g",
            "Mel 1kg",
            "Mel 500g",
            "Mel 250g"
        };

        int proxId = dados.Count > 0 ? dados.Max(d => int.Parse(d[0])) + 1 : 1;

        foreach (var nome in essenciais)
        {
            bool existe = dados.Any(d => d.Length > 1 && d[1] == nome);

            if (!existe)
            {
                dados.Add(new string[]
                {
                    proxId.ToString(),
                    nome,
                    "0", // preço inicial
                    "0"
                });

                proxId++;
            }
        }

        SalvarArquivo();
    }



}
