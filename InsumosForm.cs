using System; 
using System.IO; 
using System.Linq; 
using System.Windows.Forms; 
using System.Collections.Generic; 
using System.Drawing; 
using System.Globalization; 

public class InsumosForm : FormBase 
{ 
    DataGridView grid = new DataGridView(); 
    Label lblNome = new Label(){Text="Insumo:", Top=220, Left=10, AutoSize = true}; 
    Label lblCusto = new Label(){Text="Custo (R$):", Top=220, Left=180, AutoSize = true}; 
    TextBox txtNome = new TextBox(){Top=240, Left=10, Width=150}; 
    TextBox txtCusto = new TextBox(){Top=240, Left=180, Width=100}; 
    Button btnNovo = new Button(){Text="Novo", Top=280, Left=10, Width=80}; 
    Button btnSalvar = new Button(){Text="Salvar", Top=280, Left=100, Width=80}; 
    Button btnExcluir = new Button(){Text="Excluir", Top=280, Left=190, Width=80}; 
    List<string[]> dados = new List<string[]>(); 
    int idSelecionado = 0; 

    public InsumosForm() 
    { 
        Text="ApiarioRD - Gerenciar Insumos 📦"; 
        Width=500; 
        Height=380; 
        StartPosition = FormStartPosition.CenterScreen; 
        BackColor = Color.FromArgb(45, 45, 45); 
        ForeColor = Color.White; 
        grid.SetBounds(10,10,460,200); 
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; 
        grid.ReadOnly = true; 
        grid.RowHeadersVisible = false; 
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect; 
        Controls.AddRange(new Control[] { grid, lblNome, lblCusto, txtNome, txtCusto, btnNovo, btnSalvar, btnExcluir }); 
        
        btnNovo.Click += (s,e)=>Limpar(); 
        btnSalvar.Click += (s,e)=>Salvar(); 
        btnExcluir.Click += (s,e)=>Excluir(); 
        grid.CellClick += (s,e)=>Selecionar(); 

        // 🔥 UX MELHOR 
        txtNome.KeyDown += (s,e)=> { if(e.KeyCode == Keys.Enter) txtCusto.Focus(); }; 
        txtCusto.KeyDown += (s,e)=> { if(e.KeyCode == Keys.Enter) Salvar(); }; 

        try { TemaHelper.Aplicar(this); } catch { } 
        Carregar(); 
    } 

    void Carregar() 
    { 
        Directory.CreateDirectory("dados"); 
        string arquivo = "dados/insumos.csv"; 
        if(!File.Exists(arquivo)) File.WriteAllText(arquivo,"Id,Nome,Custo,Estoque" + Environment.NewLine); 
        dados.Clear(); 
        var linhas = File.ReadAllLines(arquivo) 
            .Skip(1) 
            .Where(l => !string.IsNullOrWhiteSpace(l)); 
        foreach (var l in linhas) 
        { 
            var partes = l.Split(','); 
            if (partes.Length >= 4) dados.Add(partes); 
        } 
        AtualizarGrid(); 
    } 

    void AtualizarGrid() 
    { 
        grid.DataSource = null; 
        var dadosValidos = dados.Where(d => d.Length >= 4).Select(d => new { 
            Id = d[0], 
            Nome = d[1], 
            Custo = decimal.TryParse(d[2], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal p) ? p.ToString("C2", new CultureInfo("pt-BR")) : d[2], 
            Estoque = d[3] 
        }).ToList(); 
        grid.DataSource = dadosValidos; 
    } 

    void Salvar() 
    { 
        if(string.IsNullOrWhiteSpace(txtNome.Text)) { MessageBox.Show("Informe o nome do insumo!"); txtNome.Focus(); return; } 
        string valorTexto = txtCusto.Text.Replace(",", "."); 
        if(!decimal.TryParse(valorTexto, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal custo)) { MessageBox.Show("Custo inválido!"); txtCusto.Focus(); return; } 
        string nomeLimpo = txtNome.Text.Replace(",", " "); 

        if(idSelecionado == 0) // NOVO 
        { 
            int proxId = dados.Count > 0 ? dados.Max(d => int.Parse(d[0])) + 1 : 1; 
            dados.Add(new string[] { proxId.ToString(), nomeLimpo, custo.ToString(CultureInfo.InvariantCulture), "0" }); 

            // ➕ ACRESCIMO SEGURO: Alimenta automaticamente o arquivo caixa.csv com a despesa do insumo
            string arquivoCaixa = "dados/caixa.csv";
            if (!File.Exists(arquivoCaixa)) File.WriteAllText(arquivoCaixa, "Data,Produto,Tipo,Valor" + Environment.NewLine);
            string dataHoje = DateTime.Now.ToString("dd/MM/yyyy");
            string linhaCaixa = $"{dataHoje},Insumo: {nomeLimpo},INSUMOS,{custo.ToString(CultureInfo.InvariantCulture)}";
            File.AppendAllText(arquivoCaixa, linhaCaixa + Environment.NewLine);
        } 
        else // EDITAR 
        { 
            var item = dados.FirstOrDefault(d => d[0] == idSelecionado.ToString()); 
            if(item != null) 
            { 
                item[1] = nomeLimpo; 
                item[2] = custo.ToString(CultureInfo.InvariantCulture); 
            } 
        } 

        SalvarArquivo(); 
        Limpar(); 
        MessageBox.Show("Insumo salvo com sucesso!"); 
    } 

    void Selecionar() 
    { 
        if(grid.CurrentRow == null) return; 
        idSelecionado = int.Parse(grid.CurrentRow.Cells["Id"].Value.ToString()); 
        txtNome.Text = grid.CurrentRow.Cells["Nome"].Value.ToString(); 
        string valorGrid = grid.CurrentRow.Cells["Custo"].Value.ToString() .Replace("R$", "") .Trim(); 
        txtCusto.Text = valorGrid; 
        
        // 🔥 Destaque visual 
        txtNome.BackColor = Color.FromArgb(70, 70, 70); 
        txtCusto.BackColor = Color.FromArgb(70, 70, 70); 
    } 

    void Excluir() 
    { 
        if(idSelecionado == 0) { MessageBox.Show("Selecione um insumo na lista!"); return; } 
        if(MessageBox.Show("Deseja excluir este insumo?", "Confirmação", MessageBoxButtons.YesNo) == DialogResult.Yes) 
        { 
            dados.RemoveAll(d => d[0] == idSelecionado.ToString()); 
            SalvarArquivo(); 
            Limpar(); 
        } 
    } 

    void SalvarArquivo() 
    { 
        var linhas = new List<string>{"Id,Nome,Custo,Estoque"}; 
        foreach(var d in dados) linhas.Add(string.Join(",", d)); 
        File.WriteAllLines("dados/insumos.csv", linhas); 
        AtualizarGrid(); 
    } 

    void Limpar() 
    { 
        txtNome.Clear(); 
        txtCusto.Clear(); 
        txtNome.BackColor = Color.White; 
        txtCusto.BackColor = Color.White; 
        idSelecionado = 0; 
        txtNome.Focus(); 
    } 
}
