using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;

public class ManutencaoForm : FormBase
{
    DataGridView grid = new DataGridView();

    Label lblItem = new Label(){Text="Item:", Top=220, Left=10, AutoSize=true};
    Label lblDesc = new Label(){Text="Descrição:", Top=220, Left=150, AutoSize=true};
    Label lblValor = new Label(){Text="Valor (R$):", Top=220, Left=320, AutoSize=true};

    TextBox txtItem = new TextBox(){Top=240, Left=10, Width=120};
    TextBox txtDesc = new TextBox(){Top=240, Left=150, Width=150};
    TextBox txtValor = new TextBox(){Top=240, Left=320, Width=100};

    Button btnNovo = new Button(){Text="Novo", Top=280, Left=10, Width=80};
    Button btnSalvar = new Button(){Text="Salvar", Top=280, Left=100, Width=80};
    Button btnExcluir = new Button(){Text="Excluir", Top=280, Left=190, Width=80};

    List<string[]> dados = new List<string[]>();
    int idSelecionado = 0;

    string arquivo = "dados/manutencao.csv";

    public ManutencaoForm()
    {
        Text="ApiarioRD - Manutenção 🔧";
        Width=550; Height=380;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(45,45,45);
        ForeColor = Color.White;

        grid.SetBounds(10,10,510,200);
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.ReadOnly = true;
        grid.RowHeadersVisible = false;
        grid.ReadOnly = true;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.MultiSelect = true; 
        grid.DefaultCellStyle.SelectionBackColor = Color.DarkCyan;
        grid.DefaultCellStyle.SelectionForeColor = Color.White;

        Controls.AddRange(new Control[] {
            grid,
            lblItem, lblDesc, lblValor,
            txtItem, txtDesc, txtValor,
            btnNovo, btnSalvar, btnExcluir
        });

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

        if(!File.Exists(arquivo))
            File.WriteAllText(arquivo,"Id,Data,Item,Descricao,Valor" + Environment.NewLine);

        dados.Clear();

        var linhas = File.ReadAllLines(arquivo)
                         .Skip(1)
                         .Where(l => !string.IsNullOrWhiteSpace(l));

        foreach(var l in linhas)
            dados.Add(l.Split(','));

        AtualizarGrid();
    }

    void AtualizarGrid()
    {
        grid.DataSource = null;

        var dadosFormatados = dados.Where(d => d.Length >= 5).Select(d => new {
            Id = d[0],
            Data = d[1],
            Item = d[2],
            Descrição = d[3],
            Valor = decimal.TryParse(d[4], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal v)
                    ? v.ToString("C2", new CultureInfo("pt-BR"))
                    : d[4]
        }).ToList();

        grid.DataSource = dadosFormatados;
    }

    void Salvar()
    {
        if(string.IsNullOrWhiteSpace(txtItem.Text))
        {
            MessageBox.Show("Informe o item!");
            return;
        }

        string valorTexto = txtValor.Text.Replace(",", ".");
        if(!decimal.TryParse(valorTexto, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal valor))
        {
            MessageBox.Show("Valor inválido!");
            return;
        }

        string itemLimpo = txtItem.Text.Replace(",", " ");
        string descLimpo = txtDesc.Text.Replace(",", " ");

        if(idSelecionado == 0) // NOVO
        {
            int proxId = dados.Count > 0 ? dados.Max(d => int.Parse(d[0])) + 1 : 1;

            dados.Add(new string[] {
                proxId.ToString(),
                DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                itemLimpo,
                descLimpo,
                valor.ToString(CultureInfo.InvariantCulture)
            });

            // 💰 INTEGRAÇÃO COM CAIXA (mantida)
            File.AppendAllText("dados/caixa.csv",
                $"{DateTime.Now},SAIDA,MANUTENCAO,{valor}\n");
        }
        else // EDITAR
        {
            var item = dados.FirstOrDefault(d => d[0] == idSelecionado.ToString());
            if(item != null)
            {
                item[2] = itemLimpo;
                item[3] = descLimpo;
                item[4] = valor.ToString(CultureInfo.InvariantCulture);
            }
        }

        SalvarArquivo();
        Limpar();
    }

    void Selecionar()
    {
        if(grid.CurrentRow == null) return;

        idSelecionado = int.Parse(grid.CurrentRow.Cells["Id"].Value.ToString());

        txtItem.Text = grid.CurrentRow.Cells["Item"].Value.ToString();
        txtDesc.Text = grid.CurrentRow.Cells["Descrição"].Value.ToString();

        string valorGrid = grid.CurrentRow.Cells["Valor"].Value.ToString()
                            .Replace("R$", "").Trim();

        txtValor.Text = valorGrid;
    }

    void Excluir()
    {
        if(idSelecionado == 0)
        {
            MessageBox.Show("Selecione um registro!");
            return;
        }

        if(MessageBox.Show("Deseja excluir?", "Aviso", MessageBoxButtons.YesNo) == DialogResult.Yes)
        {
            dados.RemoveAll(d => d[0] == idSelecionado.ToString());
            SalvarArquivo();
            Limpar();
        }
    }

    void SalvarArquivo()
    {
        var linhas = new List<string>{"Id,Data,Item,Descricao,Valor"};

        foreach(var d in dados)
            linhas.Add(string.Join(",", d));

        File.WriteAllLines(arquivo, linhas);

        AtualizarGrid();
    }

    void Limpar()
    {
        txtItem.Clear();
        txtDesc.Clear();
        txtValor.Clear();
        idSelecionado = 0;
        txtItem.Focus();
    }
}