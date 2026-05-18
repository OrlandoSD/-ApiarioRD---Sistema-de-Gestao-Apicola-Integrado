using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using System.Linq; // 🔥 ADICIONE ISSO

public static class CaixaService
{
    static string caminho = "dados/caixa.csv";

    public static void Adicionar(string tipo, string descricao, decimal valor)
    {
        Directory.CreateDirectory("dados");

        if (!File.Exists(caminho))
            File.WriteAllLines(caminho, new[] { "Data,Tipo,Descricao,Valor" });

        string data = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

        string linha = string.Join(",",
            data,
            tipo,
            descricao.Replace(",", " "),
            valor.ToString(CultureInfo.InvariantCulture)
        );

        File.AppendAllLines(caminho, new[] { linha });
    }

    public static decimal ObterSaldo()
    {
        if (!File.Exists(caminho))
            return 0;

        var linhas = File.ReadAllLines(caminho).Skip(1);

        decimal saldo = 0;

        foreach (var l in linhas)
        {
            var c = l.Split(',');
            if (c.Length < 4) continue;

            if (decimal.TryParse(c[3], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal valor))
            {
                if (c[1] == "ENTRADA")
                    saldo += valor;
                else
                    saldo -= valor;
            }
        }

        return saldo;
    }
}