using System; 
using System.IO; 
using System.Linq; 
using System.Globalization; 
using System.Collections.Generic; 

public class ResultadoEnvase 
{ 
    public decimal CustoMel { get; set; } 
    public decimal CustoEmbalagem { get; set; } 
    public decimal CustoTotal { get; set; } 
    public decimal PrecoVenda { get; set; } 
} 

public class EnvaseItem 
{ 
    public string NomeProdutoFinal { get; set; } 
    public string NomePote { get; set; } 
    public decimal PesoKg { get; set; } 
    public int Quantidade { get; set; } 
} 

public class EnvaseService 
{ 
    public List<EnvaseItem> Itens { get; set; } = new List<EnvaseItem>(); 
    decimal custoMelKg = 60m; 

    // ➕ ACRESCIMO SEGURO: Dicionário com a sua tabela de preços comerciais fixos
    private readonly Dictionary<string, decimal> tabelaPrecosComerciais = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
    {
        { "Mel 1kg", 60.00m },
        { "Mel 680g", 50.00m },
        { "Mel 500g", 40.00m },
        { "Mel 350g", 38.00m },
        { "Mel 300g", 35.00m },
        { "Mel 250g", 25.00m },
        { "Mel 200g", 18.00m },
        { "Mel 100g", 12.00m },
        { "Mel 40g", 5.00m }
    };

    string NormalizarSKU(string nome) 
    { 
        if (string.IsNullOrWhiteSpace(nome)) return nome; 
        return nome 
            .Replace(" Vidro", "") 
            .Replace(" Plastico", "") 
            .Trim(); 
    } 

    decimal ObterCustoProduto(string nome) 
    { 
        if (!File.Exists("dados/produtos.csv")) return 0; 
        var linhas = File.ReadAllLines("dados/produtos.csv").Skip(1); 
        foreach (var l in linhas) 
        { 
            var p = l.Split(','); 
            if (p.Length < 3) continue; 
            if (p[1].Trim().Equals(nome, StringComparison.OrdinalIgnoreCase)) 
            { 
                if (decimal.TryParse(p[2], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal preco)) return preco; 
            } 
        } 
        return 0; 
    } 

    public ResultadoEnvase ProcessarComRetorno() 
    { 
        if (Itens.Count == 0 || Itens.All(i => i.Quantidade <= 0)) throw new Exception("Informe ao menos um item para envase!"); 
        
        decimal consumoMel = 0; 
        decimal custoEmbalagem = 0; 
        int totalUnidades = 0; 

        // ➕ ACRESCIMO SEGURO: Variável para somar o faturamento da sua tabela fixa
        decimal faturamentoTabelaFixa = 0;

        foreach (var item in Itens) 
        { 
            if (item.Quantidade <= 0) continue; 
            consumoMel += item.PesoKg * item.Quantidade; 
            decimal custoPote = ObterCustoProduto(NormalizarSKU(item.NomePote)); 
            custoEmbalagem += custoPote * item.Quantidade; 
            totalUnidades += item.Quantidade; 

            // ➕ ACRESCIMO SEGURO: Acumula o valor fixo de venda baseado no nome do produto
            if (tabelaPrecosComerciais.TryGetValue(item.NomeProdutoFinal, out decimal precoFixoUnitario))
            {
                faturamentoTabelaFixa += precoFixoUnitario * item.Quantidade;
            }
            else
            {
                // Caso use algum nome fora da tabela, calcula a margem padrão antiga de 1.5x por segurança
                faturamentoTabelaFixa += ( (item.PesoKg * custoMelKg) + custoPote ) * item.Quantidade * 1.5m;
            }

            string sku = NormalizarSKU(item.NomePote); 
            if (item.Quantidade > EstoqueService.ObterQuantidade(sku)) throw new Exception($"{sku} insuficiente no estoque!"); 
        } 

        decimal melDisponivel = EstoqueService.ObterQuantidade("Mel"); 
        if (consumoMel > melDisponivel) throw new Exception($"Mel insuficiente! Disponível: {melDisponivel} kg"); 

        decimal custoMel = consumoMel * custoMelKg; 
        decimal custoTotal = custoMel + custoEmbalagem; 
        
        // Mantemos a variável original intocada para os cálculos de arquivo internos
        decimal precoVenda = custoTotal * 1.5m; 
        decimal custoUnitario = totalUnidades > 0 ? custoTotal / totalUnidades : 0; 

        // 🔻 BAIXA MEL 
        EstoqueService.DarBaixa("Mel", consumoMel); 

        foreach (var item in Itens) 
        { 
            if (item.Quantidade <= 0) continue; 
            string sku = NormalizarSKU(item.NomePote); 
            
            // 🔻 BAIXA EMBALAGEM 
            EstoqueService.DarBaixa(sku, item.Quantidade); 
            
            // 🔺 ENTRADA PRODUTO FINAL 
            EstoqueService.DarEntrada(item.NomeProdutoFinal, item.Quantidade); 

            // ➕ ACRESCIMO SEGURO: Identifica o preço específico deste item na tabela para salvar no CSV
            decimal precoTabelaItem = tabelaPrecosComerciais.TryGetValue(item.NomeProdutoFinal, out decimal pFix) ? pFix : (custoUnitario * 1.5m);

            // Passa o preço real da sua tabela para a gravação do produtos.csv
            AtualizarProdutoFinal( 
                item.NomeProdutoFinal, 
                item.Quantidade, 
                custoUnitario, 
                precoTabelaItem 
            ); 
        } 

        RegistrarEnvase(consumoMel); 

        // 🔺 RETORNO MODIFICADO APENAS NOS CAMPOS FINAIS: Retorna o faturamento real somado da sua tabela comercial
        return new ResultadoEnvase 
        { 
            CustoMel = custoMel, 
            CustoEmbalagem = custoEmbalagem, 
            CustoTotal = custoTotal, 
            PrecoVenda = faturamentoTabelaFixa // Exibe o valor exato calculado (Ex: 500g = R$ 40,00)
        }; 
    } 

    void AtualizarProdutoFinal(string nome, int quantidade, decimal custoUnitario, decimal precoVenda) 
    { 
        string caminho = "dados/produtos.csv"; 
        if (!File.Exists(caminho)) File.WriteAllText(caminho, "Id,Nome,Preco,Estoque,Custo" + Environment.NewLine); 
        var linhas = File.ReadAllLines(caminho).ToList(); 
        bool encontrou = false; 
        for (int i = 1; i < linhas.Count; i++) 
        { 
            var p = linhas[i].Split(','); 
            if (p.Length < 5) 
            { 
                var novo = new string[5]; 
                Array.Copy(p, novo, p.Length); 
                novo[4] = "0"; 
                p = novo; 
            } 
            if (p[1].Trim().Equals(nome, StringComparison.OrdinalIgnoreCase)) 
            { 
                decimal estoqueAtual = decimal.Parse(p[3], CultureInfo.InvariantCulture); 
                decimal custoAntigo = decimal.Parse(p[4], CultureInfo.InvariantCulture); 
                decimal novoEstoque = estoqueAtual + quantidade; 
                decimal custoMedio = ((estoqueAtual * custoAntigo) + (quantidade * custoUnitario)) / (novoEstoque == 0 ? 1 : novoEstoque); 
                p[2] = precoVenda.ToString(CultureInfo.InvariantCulture); 
                p[3] = novoEstoque.ToString(CultureInfo.InvariantCulture); 
                p[4] = custoMedio.ToString(CultureInfo.InvariantCulture); 
                linhas[i] = string.Join(",", p); 
                encontrou = true; 
                break; 
            } 
        } 
        if (!encontrou) 
        { 
            int novoId = linhas.Count; 
            linhas.Add($"{novoId},{nome},{precoVenda.ToString(CultureInfo.InvariantCulture)},{quantidade},{custoUnitario.ToString(CultureInfo.InvariantCulture)}"); 
        } 
        File.WriteAllLines(caminho, linhas); 
    } 

    void RegistrarEnvase(decimal quantidadeMel) 
    { 
        string caminho = "dados/envase.csv"; 
        if (!File.Exists(caminho)) File.WriteAllText(caminho, "Data,QuantidadeMel" + Environment.NewLine); 
        string linha = $"{DateTime.Now:dd/MM/yyyy HH:mm},{quantidadeMel.ToString(CultureInfo.InvariantCulture)}"; 
        File.AppendAllText(caminho, linha + Environment.NewLine); 
    } 
}
