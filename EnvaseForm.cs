using System;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Generic;

public class EnvaseForm : FormBase 
{
    // Caixa de Texto para quantidades (Inputs)
    TextBox txt1kg = new TextBox() { Top = 30, Left = 150, Width = 80 };
    TextBox txt680g = new TextBox() { Top = 60, Left = 150, Width = 80 };
    TextBox txt500g = new TextBox() { Top = 90, Left = 150, Width = 80 };
    TextBox txt350g = new TextBox() { Top = 120, Left = 150, Width = 80 };
    TextBox txt300g = new TextBox() { Top = 150, Left = 150, Width = 80 };
    TextBox txt250g = new TextBox() { Top = 180, Left = 150, Width = 80 };
    TextBox txt200g = new TextBox() { Top = 210, Left = 150, Width = 80 };
    TextBox txt100g = new TextBox() { Top = 240, Left = 150, Width = 80 };
    TextBox txt40g = new TextBox() { Top = 270, Left = 150, Width = 80 }; // NOVO: Tamanho de 40g

    // Seleção de Tipo de Recipiente (Dropdowns)
    ComboBox cbTipo1kg = new ComboBox() { Top = 30, Left = 250, Width = 130 };
    ComboBox cbTipo680g = new ComboBox() { Top = 60, Left = 250, Width = 130 };
    ComboBox cbTipo500g = new ComboBox() { Top = 90, Left = 250, Width = 130 };
    ComboBox cbTipo350g = new ComboBox() { Top = 120, Left = 250, Width = 130 };
    ComboBox cbTipo300g = new ComboBox() { Top = 150, Left = 250, Width = 130 };
    ComboBox cbTipo250g = new ComboBox() { Top = 180, Left = 250, Width = 130 };
    ComboBox cbTipo200g = new ComboBox() { Top = 210, Left = 250, Width = 130 };
    ComboBox cbTipo100g = new ComboBox() { Top = 240, Left = 250, Width = 130 };
    ComboBox cbTipo40g = new ComboBox() { Top = 270, Left = 250, Width = 130 };

    Button btn = new Button() { Text = "Processar", Top = 320, Left = 50, Width = 150, Height = 35 };

    // Dicionários Internos para amarrar os Preços Fixos e Pesos Reais
    private Dictionary<string, decimal> tabelaPrecos;
    private Dictionary<string, decimal> tabelaPesos;

    public EnvaseForm() 
    {
        Text = "Envase de Mel 🍯 - ApiárioRD";
        Width = 420; 
        Height = 420;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(45, 45, 45);
        ForeColor = Color.White;

        // ALIMENTA A SUA TABELA COMERCIAL DE PREÇO FIXO FINAL
        tabelaPrecos = new Dictionary<string, decimal>() {
            { "1KG", 60.00m }, { "680G", 50.00m }, { "500G", 40.00m },
            { "350G", 38.00m }, { "300G", 35.00m }, { "250G", 25.00m },
            { "200G", 18.00m }, { "100G", 12.00m }, { "40G", 5.00m }
        };

        // PESOS REAIS EM KG PARA VÍNCULO DE ATUALIZAÇÃO DO MEL BRUTO
        tabelaPesos = new Dictionary<string, decimal>() {
            { "1KG", 1.00m }, { "680G", 0.68m }, { "500G", 0.50m },
            { "350G", 0.35m }, { "300G", 0.30m }, { "250G", 0.25m },
            { "200G", 0.20m }, { "100G", 0.10m }, { "40G", 0.04m }
        };

        // Adiciona as opções de frascos incluindo as Bisnagas Plásticas
        string[] tiposFrascos = new string[] { "Plastico", "Vidro", "Bisnaga Plastica" };
        
        ComboBox[] todosCombos = { cbTipo1kg, cbTipo680g, cbTipo500g, cbTipo350g, cbTipo300g, cbTipo250g, cbTipo200g, cbTipo100g, cbTipo40g };
        foreach (var cb in todosCombos)
        {
            cb.Items.AddRange(tiposFrascos);
            cb.SelectedIndex = 0;
        }

        // Adiciona os componentes visuais ao ecrã
        Controls.AddRange(new Control[] {
            new Label() { Text = "Potes 1kg:", Top = 30, Left = 20, Width = 120 },
            new Label() { Text = "Potes 680g:", Top = 60, Left = 20, Width = 120 },
            new Label() { Text = "Potes 500g:", Top = 90, Left = 20, Width = 120 },
            new Label() { Text = "Potes 350g:", Top = 120, Left = 20, Width = 120 },
            new Label() { Text = "Potes 300g:", Top = 150, Left = 20, Width = 120 },
            new Label() { Text = "Potes 250g:", Top = 180, Left = 20, Width = 120 },
            new Label() { Text = "Potes 200g:", Top = 210, Left = 20, Width = 120 },
            new Label() { Text = "Potes 100g:", Top = 240, Left = 20, Width = 120 },
            new Label() { Text = "Potes 40g:", Top = 270, Left = 20, Width = 120 },
            
            txt1kg, txt680g, txt500g, txt350g, txt300g, txt250g, txt200g, txt100g, txt40g,
            cbTipo1kg, cbTipo680g, cbTipo500g, cbTipo350g, cbTipo300g, cbTipo250g, cbTipo200g, cbTipo100g, cbTipo40g,
            btn
        });

        btn.BackColor = Color.DarkGreen;
        btn.ForeColor = Color.White;
        btn.Click += Processar;
    }

    string GerarSKU(string peso, string tipo) 
    {
        // Substitui espaços por underscores para manter o padrão de banco de dados
        return $"{peso.ToUpper()}-{tipo.ToUpper().Replace(" ", "_")}";
    }

    void Processar(object sender, EventArgs e) 
    {
        try 
        {
            var envase = new EnvaseService();
            decimal totalMelKg = 0;
            decimal faturamentoTotal = 0;

            // Dicionário auxiliar para mapear as caixas de texto e os dropdowns na leitura do loop
            var mapeamento = new Dictionary<string, (TextBox txt, ComboBox cb)>() {
                { "1KG", (txt1kg, cbTipo1kg) }, { "680G", (txt680g, cbTipo680g) },
                { "500G", (txt500g, cbTipo500g) }, { "350G", (txt350g, cbTipo350g) },
                { "300G", (txt300g, cbTipo300g) }, { "250G", (txt250g, cbTipo250g) }, // CORRIGIDO: Agora aponta para 250G real
                { "200G", (txt200g, cbTipo200g) }, { "100G", (txt100g, cbTipo100g) },
                { "40G", (txt40g, cbTipo40g) }
            };

            foreach (var item in mapeamento)
            {
                int q = string.IsNullOrWhiteSpace(item.Value.txt.Text) ? 0 : int.Parse(item.Value.txt.Text);
                if (q > 0)
                {
                    string skuFrasco = GerarSKU(item.Key, item.Value.cb.Text);
                    decimal pesoUnidade = tabelaPesos[item.Key];
                    decimal precoUnidade = tabelaPrecos[item.Key];

                    totalMelKg += (q * pesoUnidade);
                    faturamentoTotal += (q * precoUnidade);

                    envase.Itens.Add(new EnvaseItem { 
                        NomeProdutoFinal = $"Mel {item.Key}", 
                        NomePote = skuFrasco, 
                        PesoKg = pesoUnidade, 
                        Quantidade = q 
                    });
                }
            }

            if (totalMelKg == 0)
            {
                MessageBox.Show("Insira a quantidade de pelo menos um recipiente para processar!", "Aviso");
                return;
            }

            var resultado = envase.ProcessarComRetorno();

            // Mensagem de Retorno atualizada com a amarração dos seus Preços de Venda Fixo
            MessageBox.Show(
                $"Envase realizado com sucesso!\n\n" +
                $"• Total de Mel Bruto Utilizado: {totalMelKg:N2} kg\n" +
                $"• Faturamento Total Gerado: R$ {faturamentoTotal:N2}\n\n" +
                $"• Custos Operacionais de Fabricação:\n" +
                $"  - Custo do mel: R$ {resultado.CustoMel:N2}\n" +
                $"  - Custo embalagens: R$ {resultado.CustoEmbalagem:N2}\n" +
                $"  - Custo total: R$ {resultado.CustoTotal:N2}"
            );
            
            Limpar();
        } 
        catch (Exception ex) 
        {
            MessageBox.Show("Erro ao processar envase: " + ex.Message);
        }
    }

    void Limpar() 
    {
        txt1kg.Clear(); txt680g.Clear(); txt500g.Clear(); txt350g.Clear();
        txt300g.Clear(); txt250g.Clear(); txt200g.Clear(); txt100g.Clear(); txt40g.Clear();
    }
}
