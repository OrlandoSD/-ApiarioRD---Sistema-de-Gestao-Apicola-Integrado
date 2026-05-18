using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing;

public class ColmeiasForm : FormBase
{
    DataGridView grid = new DataGridView();

    Label lblNome = new Label(){Text="Nome:", Top=220, Left=10};
    Label lblLocal = new Label(){Text="Local:", Top=220, Left=150};
    Label lblSituacao = new Label(){Text="Situação:", Top=220, Left=290};

    TextBox txtNome = new TextBox(){Top=240, Left=10, Width=120};
    TextBox txtLocal = new TextBox(){Top=240, Left=150, Width=120};

    ComboBox comboSituacao = new ComboBox(){Top=240, Left=290, Width=120};

    Button btnAdicionar = new Button(){Text="Adicionar", Top=280, Left=10};
    Button btnEditar = new Button(){Text="Editar", Top=280, Left=110};
    Button btnExcluir = new Button(){Text="Excluir", Top=280, Left=210};
    Button btnLimpar = new Button(){Text="Limpar", Top=280, Left=310};

    List<string[]> dados = new List<string[]>();
    string caminho = "dados/colmeias.csv";

    public ColmeiasForm()
    {
        Text = "Colmeias 🐝";
        Width = 450;
        Height = 380;

        BackColor = Color.FromArgb(45,45,45);
        ForeColor = Color.White;

        grid.SetBounds(10,10,410,200);
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.ReadOnly = true;
        grid.RowHeadersVisible = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

        comboSituacao.DropDownStyle = ComboBoxStyle.DropDownList;
        comboSituacao.Items.AddRange(new string[]
        {
            "Ativa",
            "Fraca",
            "Em manejo"
        });

        Controls.AddRange(new Control[]
        {
            grid, lblNome, lblLocal, lblSituacao,
            txtNome, txtLocal, comboSituacao,
            btnAdicionar, btnEditar, btnExcluir, btnLimpar
        });

        btnAdicionar.Click += (s,e)=>Adicionar();
        btnEditar.Click += (s,e)=>Editar();
        btnExcluir.Click += (s,e)=>Excluir();
        btnLimpar.Click += (s,e)=>Limpar();

        grid.SelectionChanged += (s,e)=>CarregarSelecionado();

        try { TemaHelper.Aplicar(this); } catch { }

        Carregar();
    }

    void Carregar()
    {
        Directory.CreateDirectory("dados");

        if(!File.Exists(caminho))
            File.WriteAllText(caminho,"Id,Nome,Local,Situacao\n");

        dados.Clear();

        var linhas = File.ReadAllLines(caminho).Skip(1);

        foreach(var l in linhas)
        {
            if (string.IsNullOrWhiteSpace(l)) continue;

            var c = l.Split(',');

            // 🔒 PROTEÇÃO CONTRA ERRO
            if (c.Length < 4) continue;

            dados.Add(c);
        }

        Atualizar();
    }

    void Atualizar()
    {
        grid.DataSource = null;

        grid.DataSource = dados.Select(d => new {
            Id = d[0],
            Nome = d[1],
            Local = d[2],
            Situacao = d[3],
            Status = ObterEmoji(d[3])
        }).ToList();
    }

    string ObterEmoji(string situacao)
    {
        return situacao switch
        {
            "Ativa" => "🟢",
            "Fraca" => "🟡",
            "Em manejo" => "🔧",
            _ => ""
        };
    }

    void Adicionar()
    {
        if(string.IsNullOrWhiteSpace(txtNome.Text))
        {
            MessageBox.Show("Informe o nome da colmeia!");
            txtNome.Focus();
            return;
        }

        if(comboSituacao.SelectedItem == null)
        {
            MessageBox.Show("Selecione a situação!");
            return;
        }

        // 🔒 evita duplicado
        if(dados.Any(d => d[1].Equals(txtNome.Text, StringComparison.OrdinalIgnoreCase)))
        {
            MessageBox.Show("Colmeia já existe!");
            return;
        }

        int id = dados.Count > 0 ? dados.Max(d => int.Parse(d[0])) + 1 : 1;

        string nome = txtNome.Text.Replace(",", " ");
        string local = txtLocal.Text.Replace(",", " ");

        dados.Add(new string[]{
            id.ToString(),
            nome,
            local,
            comboSituacao.SelectedItem.ToString()
        });

        Salvar();
        Atualizar();
        Limpar();
    }

    void Editar()
    {
        if(grid.CurrentRow == null) return;

        if (MessageBox.Show("Deseja alterar esta colmeia?", "Confirmação", MessageBoxButtons.YesNo) != DialogResult.Yes)
            return;

        int index = grid.CurrentRow.Index;

        if (index < 0 || index >= dados.Count) return;

        dados[index][1] = txtNome.Text.Replace(",", " ");
        dados[index][2] = txtLocal.Text.Replace(",", " ");
        dados[index][3] = comboSituacao.SelectedItem?.ToString() ?? "";

        Salvar();
        Atualizar();
    }

    void Excluir()
    {
        if(grid.CurrentRow == null) return;

        int index = grid.CurrentRow.Index;

        if (index < 0 || index >= dados.Count) return;

        if (MessageBox.Show("Excluir colmeia?", "Confirmação", MessageBoxButtons.YesNo) == DialogResult.Yes)
        {
            dados.RemoveAt(index);

            Salvar();
            Atualizar();
            Limpar();
        }
    }

    void CarregarSelecionado()
    {
        if(grid.CurrentRow == null) return;

        int index = grid.CurrentRow.Index;

        if (index < 0 || index >= dados.Count) return;

        txtNome.Text = dados[index][1];
        txtLocal.Text = dados[index][2];
        comboSituacao.SelectedItem = dados[index][3];
    }

    void Salvar()
    {
        var linhas = new List<string>{"Id,Nome,Local,Situacao"};

        foreach(var d in dados)
            linhas.Add($"{d[0]},{d[1]},{d[2]},{d[3]}");

        File.WriteAllLines(caminho, linhas);
    }

    void Limpar()
    {
        txtNome.Clear();
        txtLocal.Clear();
        comboSituacao.SelectedIndex = -1;
        txtNome.Focus();
    }
}