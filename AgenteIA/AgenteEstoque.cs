using System;
using System.Threading.Tasks;

public class AgenteEstoque
{
    public async Task Executar()
    {
        try
        {
            CsvService csv = new CsvService();
            GeminiService ia = new GeminiService();

            var produtos = csv.LerProdutos("produtos.csv");

            foreach (var p in produtos)
            {
                string prompt = $@"
Você é um sistema de controle de estoque.

Analise o produto abaixo:

Produto: {p.Nome}
Quantidade atual: {p.Quantidade}
Estoque mínimo: {p.Minimo}

Regra:
- Se a quantidade for menor que o mínimo, você deve recomendar compra
- Se não precisar comprar, responda apenas: OK

Se precisar comprar, responda exatamente neste formato:
COMPRAR:{p.Nome},{p.Minimo * 2}
";

                string resposta = await ia.PerguntarAsync(prompt);

                // 🔍 Tratamento simples da resposta
                string decisao = "OK";

                if (!string.IsNullOrEmpty(resposta) && resposta.Contains("COMPRAR"))
                {
                    try
                    {
                        var parte = resposta.Split("COMPRAR:")[1];
                        var dados = parte.Split(',');

                        string nome = dados[0].Trim();
                        int quantidade = int.Parse(dados[1].Trim());

                        csv.InserirPedido(nome, quantidade);

                        decisao = $"COMPRAR {quantidade}";
                    }
                    catch
                    {
                        decisao = "ERRO AO INTERPRETAR IA";
                    }
                }

                // 📝 Registrar log sempre
                csv.InserirLog(p.Nome, p.Quantidade, p.Minimo, decisao);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro no agente: " + ex.Message);
        }
    }
}
