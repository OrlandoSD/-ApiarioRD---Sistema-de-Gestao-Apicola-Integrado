using System; 
using System.IO; 
using System.Linq; 
using System.Collections.Generic; 
using System.Windows.Forms; 
using System.Globalization; 
// 🔥 LIVECHARTS 
using LiveChartsCore; 
using LiveChartsCore.SkiaSharpView; 
using LiveChartsCore.SkiaSharpView.WinForms; 
using LiveChartsCore.SkiaSharpView.Painting; 
using SkiaSharp; 

public static class EstoqueService 
{ 
    static string pasta = "dados"; 
    static string caminho = Path.Combine(pasta, "estoque.csv"); 
    static string caminhoMov = Path.Combine(pasta, "movimentacao.csv"); 

    // ===================================================== 
    // 🧠 INOVAÇÃO ERP: NORMALIZAÇÃO DE PRODUTO (ORIGINAL MANTIDO) 
    // ===================================================== 
    public static string NormalizarProduto(string produto) 
    { 
        if (string.IsNullOrWhiteSpace(produto)) return produto; 
        produto = produto.Trim().ToUpper(); 
        produto = produto.Replace("POTE ", ""); 
        produto = produto.Replace(" ", "-"); 
        return produto; 
    } 

    // ===================================================== 
    // 📦 CARREGAR ESTOQUE (ACRESCIDA VALIDAÇÃO DE CHAVE ERP) 
    // ===================================================== 
    public static Dictionary<string, decimal> Carregar() 
    { 
        Directory.CreateDirectory(pasta); 
        if (!File.Exists(caminho)) File.WriteAllText(caminho, "Produto,Quantidade" + Environment.NewLine); 
        
        var estoque = new Dictionary<string, decimal>(); 
        var linhas = File.ReadAllLines(caminho) 
            .Skip(1) 
            .Where(l => !string.IsNullOrWhiteSpace(l)); 

        foreach (var l in linhas) 
        { 
            var c = l.Split(',').Select(x => x.Trim()).ToArray(); 
            if (c.Length < 2) continue; 
            if (decimal.TryParse(c[1], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal qtd)) 
            { 
                // ➕ ACRESCIMO SEGURO: Normaliza a chave ao carregar do CSV para alinhar com o EnvaseService
                string nomeProduto = NormalizarProduto(c[0].Trim()); 
                
                if (estoque.ContainsKey(nomeProduto))
                    estoque[nomeProduto] += qtd; // Se houver duplicado antigo no CSV, soma para não perder saldo
                else
                    estoque[nomeProduto] = qtd; 
            } 
        } 
        return estoque; 
    } 

    // ===================================================== 
    // 💾 SALVAR ESTOQUE (ORIGINAL MANTIDO) 
    // ===================================================== 
    public static void Salvar(Dictionary<string, decimal> estoque) 
    { 
        var linhas = new List<string> { "Produto,Quantidade" }; 
        foreach (var item in estoque) 
            // ➕ ACRESCIMO SEGURO: Garante que salva a chave limpa e normalizada no arquivo físico
            linhas.Add($"{NormalizarProduto(item.Key)},{item.Value.ToString(CultureInfo.InvariantCulture)}"); 
        
        File.WriteAllLines(caminho, linhas); 
    } 

    // ===================================================== 
    // 🔥 MOVIMENTAÇÃO (COM INOVAÇÃO ERP - ORIGINAL MANTIDO) 
    // ===================================================== 
    public static void MovimentarEstoque(string produto, decimal quantidade, string tipo) 
    { 
        if (string.IsNullOrWhiteSpace(produto)) throw new Exception("Produto inválido"); 
        if (quantidade <= 0) throw new Exception("Quantidade deve ser maior que zero"); 

        produto = NormalizarProduto(produto); 
        var estoque = Carregar(); 
        
        if (!estoque.ContainsKey(produto)) estoque[produto] = 0; 
        
        if (tipo == "Entrada") 
        { 
            estoque[produto] += quantidade; 
        } 
        else if (tipo == "Saida") 
        { 
            if (estoque[produto] < quantidade) throw new Exception($"Estoque insuficiente para {produto}. Disponível: {estoque[produto]}"); 
            estoque[produto] -= quantidade; 
        } 
        
        Salvar(estoque); 
        RegistrarMovimentacao(produto, quantidade, tipo); 
        VerificarEstoqueBaixo(produto, estoque[produto]); 
    } 

    // ===================================================== 
    // 🧾 MOVIMENTAÇÃO LOG (ORIGINAL MANTIDO) 
    // ===================================================== 
    public static void RegistrarMovimentacao(string produto, decimal quantidade, string tipo) 
    { 
        if (!File.Exists(caminhoMov)) File.WriteAllText(caminhoMov, "Produto,Quantidade,Tipo,Data" + Environment.NewLine); 
        string data = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"); 
        string qtdTxt = quantidade.ToString(CultureInfo.InvariantCulture); 
        string linha = $"{NormalizarProduto(produto)},{qtdTxt},{tipo},{data}"; 
        File.AppendAllText(caminhoMov, linha + Environment.NewLine); 
    } 

    // ===================================================== 
    // ⚠️ ALERTA ESTOQUE BAIXO (ORIGINAL MANTIDO) 
    // ===================================================== 
    public static void VerificarEstoqueBaixo(string produto, decimal quantidade) 
    { 
        if (quantidade <= 5) 
        { 
            MessageBox.Show( 
                $"⚠️ ALERTA DE ESTOQUE\n\nO produto '{produto}' está com apenas {quantidade} unidades!", 
                "Aviso de Estoque Baixo", 
                MessageBoxButtons.OK, 
                MessageBoxIcon.Warning 
            ); 
        } 
    } 

    // ===================================================== 
    // 🔎 CONSULTA QUANTIDADE (INOVADO - ORIGINAL MANTIDO) 
    // ===================================================== 
    public static decimal ObterQuantidade(string produto) 
    { 
        var estoque = Carregar(); 
        produto = NormalizarProduto(produto); 
        return estoque.ContainsKey(produto) ? estoque[produto] : 0; 
    } 

    // ===================================================== 
    // 📋 LISTAGEM (MANTIDO - COM PROTEÇÃO DE CHAVE) 
    // ===================================================== 
    public static List<(string Produto, decimal Quantidade)> ListarTodos() 
    { 
        var estoque = Carregar(); 
        return estoque.Select(x => (NormalizarProduto(x.Key), x.Value)).ToList(); 
    } 

    // ===================================================== 
    // 🔻 BAIXA (ORIGINAL MANTIDO) 
    // ===================================================== 
    public static void DarBaixa(string produto, decimal quantidade) 
    { 
        MovimentarEstoque(produto, quantidade, "Saida"); 
    } 

    // ===================================================== 
    // 🔺 ENTRADA (ORIGINAL MANTIDO) 
    // ===================================================== 
    public static void DarEntrada(string produto, decimal quantidade) 
    { 
        MovimentarEstoque(produto, quantidade, "Entrada"); 
    } 

    // ===================================================== 
    // 🔍 EXISTE PRODUTO (INOVADO - ORIGINAL MANTIDO) 
    // ===================================================== 
    public static bool ExisteProduto(string produto) 
    { 
        var estoque = Carregar(); 
        produto = NormalizarProduto(produto); 
        return estoque.ContainsKey(produto); 
    } 
}
