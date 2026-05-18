using System;
using System.Collections.Generic;
using System.IO;

public class CsvService
{
    public List<Produto> LerProdutos(string caminho)
    {
        var lista = new List<Produto>();
        var linhas = File.ReadAllLines(caminho);

        for (int i = 1; i < linhas.Length; i++)
        {
            var col = linhas[i].Split(',');

            lista.Add(new Produto
            {
                Nome = col[0],
                Quantidade = int.Parse(col[1]),
                Minimo = int.Parse(col[2])
            });
        }

        return lista;
    }

    public void InserirPedido(string produto, int qtd)
    {
        string linha = $"{produto},{qtd},{DateTime.Now}";
        File.AppendAllLines("pedidos.csv", new[] { linha });
    }

    public void InserirLog(string produto, int qtd, int minimo, string decisao)
    {
        string linha = $"{produto},{qtd},{minimo},{decisao},{DateTime.Now}";
        File.AppendAllLines("log_agente.csv", new[] { linha });
    }
}
