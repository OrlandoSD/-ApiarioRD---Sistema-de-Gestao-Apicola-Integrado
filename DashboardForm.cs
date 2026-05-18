using System; 
using System.IO; 
using System.Linq; 
using System.Windows.Forms; 
using System.Globalization; 
using System.Collections.Generic; 
using System.Drawing; 
// 🔥 LIVECHARTS 
using LiveChartsCore; 
using LiveChartsCore.SkiaSharpView; 
using LiveChartsCore.SkiaSharpView.WinForms; 
using LiveChartsCore.SkiaSharpView.Painting; 
using SkiaSharp; 

public class DashboardForm : FormBase 
{ 
    Label lblReceita, lblCustos, lblLucro, lblMargem; 
    Label lblProducao, lblInsumos, lblColmeias, lblManutencao; 
    Label lblPrevisaoProducao, lblPrevisaoLucro, lblIAColmeia; 
    ListBox lstAlertas, lstRecomendacoes; 
    CartesianChart chartProducao; 
    CartesianChart chartComparativo; 
    Timer timer = new Timer(); 
    Button btnEstoque; 
    Button btnBackup; 

    public DashboardForm() 
    { 
        Text = "ApiarioRD - Dashboard Profissional 📊🐝"; 
        Width = 1100; 
        Height = 850; 
        BackColor = Color.FromArgb(45, 45, 45); 

        lblReceita = CriarCard("Receita", 10, 10); 
        lblCustos = CriarCard("Custos", 210, 10); 
        lblLucro = CriarCard("Lucro", 410, 10); 
        lblManutencao = CriarCard("Manutenção", 610, 10); 
        
        lblProducao = CriarCard("Produção", 10, 100); 
        lblInsumos = CriarCard("Insumos", 210, 100); 
        lblColmeias = CriarCard("Colmeias", 410, 100); 
        lblPrevisaoProducao = CriarCard("Eficiência", 610, 100); 
        lblPrevisaoLucro = CriarCard("Prev. Lucro", 810, 100); 

        lblIAColmeia = new Label() { Top = 200, Left = 10, Width = 1000, ForeColor = Color.LightSkyBlue, Font = new Font("Segoe UI", 10, FontStyle.Italic) }; 
        lblMargem = new Label() { Top = 230, Left = 10, Width = 500, ForeColor = Color.White, Font = new Font("Segoe UI", 10, FontStyle.Bold) }; 

        btnEstoque = new Button() { Text = "📦 Estoque", Left = 10, Top = 260, Width = 120, Height = 40, BackColor = Color.DarkOrange, ForeColor = Color.White }; 
        btnEstoque.Click += (s, e) => new EstoqueForm().Show(); 

        btnBackup = new Button() { Text = "💾 Backup", Left = 140, Top = 260, Width = 120, Height = 40, BackColor = Color.DarkSlateBlue, ForeColor = Color.White }; 
        btnBackup.Click += BtnBackup_Click; 

        chartProducao = new CartesianChart() { Left = 10, Top = 310, Width = 1050, Height = 200 }; 
        chartComparativo = new CartesianChart() { Left = 10, Top = 520, Width = 1050, Height = 200 }; 

        lstAlertas = new ListBox() { Top = 730, Left = 10, Width = 520, Height = 80, BackColor = Color.FromArgb(30, 30, 30), ForeColor = Color.White }; 
        lstRecomendacoes = new ListBox() { Top = 730, Left = 540, Width = 520, Height = 80, BackColor = Color.FromArgb(30, 30, 30), ForeColor = Color.LightGreen }; 

        Controls.AddRange(new Control[] { lblReceita, lblCustos, lblLucro, lblManutencao, lblProducao, lblInsumos, lblColmeias, lblPrevisaoProducao, lblPrevisaoLucro, lblIAColmeia, lblMargem, btnEstoque, btnBackup, chartProducao, chartComparativo, lstAlertas, lstRecomendacoes }); 

        // ➕ ACRESCIMO SEGURO: Configuração básica de eixos para legibilidade do SkiaSharp no modo escuro
        chartProducao.XAxes = new Axis[] { new Axis { Name = "Histórico de Manejos", TextSize = 11, NamePaint = new SolidColorPaint(SKColors.LightGray) } };
        chartProducao.YAxes = new Axis[] { new Axis { Name = "Kg", TextSize = 11, NamePaint = new SolidColorPaint(SKColors.LightGray) } };
        chartComparativo.XAxes = new Axis[] { new Axis { Labels = new string[] { "Balanço Geral" }, TextSize = 11 } };

        timer.Interval = 5000; 
        timer.Tick += (s, e) => CarregarDados(); 
        timer.Start(); 
        CarregarDados(); 
    } 

    Label CriarCard(string titulo, int left, int top) 
    { 
        return new Label() { Text = $"{titulo}\nR$ 0,00", Left = left, Top = top, Width = 180, Height = 80, BackColor = Color.FromArgb(60, 60, 60), ForeColor = Color.White, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 10, FontStyle.Bold) }; 
    } 

    void CarregarDados() 
    { 
        lstAlertas.Items.Clear(); 
        lstRecomendacoes.Items.Clear(); 

        decimal receita = SomarArquivo("dados/vendas.csv", 3); 
        decimal custos = SomarArquivo("dados/caixa.csv", 3, "SAIDA"); 
        decimal lucro = receita - custos; 
        decimal producaoBruta = SomarArquivo("dados/manejo.csv", 3);
        // ALTERE APENAS ESTA LINHA:
        producaoBruta = SomarProducaoManejo("dados/manejo.csv");
 
        
        // ➕ ACRESCIMO SEGURO: Correção do desvio de leitura chamando a nova função flexível de 2 colunas para o envase.csv
        decimal producaoEnvase = SomarEnvaseSimples("dados/envase.csv"); 
        
        decimal manutencao = SomarArquivo("dados/caixa.csv", 3, "MANUTENCAO"); 
        decimal insumos = SomarArquivoFlexivel("dados/caixa.csv", "INSUMOS"); 
        decimal eficiencia = producaoBruta > 0 ? (producaoEnvase / producaoBruta) * 100 : 0; 
        decimal previsaoLucro = lucro * 1.10m; 

        // 🏠 UNIFICAÇÃO AQUI: Conta colmeias únicas a partir do arquivo de manejo/produção, eliminando o colmeias.csv
        int totalColmeias = 0;
        if (File.Exists("dados/colmeias.csv"))
        {
            totalColmeias = File.ReadAllLines("dados/colmeias.csv")
                .Skip(1)
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .Select(l => l.Split(',')[0].Trim())
                .Distinct()
                .Count();
        }

        // 🔥 CARDS 
        lblReceita.Text = $"💰 Receita\n{receita:C2}"; 
        lblCustos.Text = $"💸 Custos\n{custos:C2}"; 
        lblLucro.Text = $"📊 Lucro\n{lucro:C2}"; 
        lblManutencao.Text = $"🔧 Manutenção\n{manutencao:C2}"; 
        lblInsumos.Text = $"🧪 Insumos\n{insumos:C2}"; 
        lblProducao.Text = $"🐝 Produção\n{producaoBruta:N2} kg"; 
        lblColmeias.Text = $"🏠 Colmeias\n{totalColmeias}"; 
        lblPrevisaoProducao.Text = $"⚙️ Eficiência\n{eficiencia:F2}%"; 
        lblPrevisaoLucro.Text = $"📈 Prev. Lucro\n{previsaoLucro:C2}"; 

        // ➕ ACRESCIMO SEGURO: Alimentação das Labels de Análise de Margem e Insights de IA
        decimal margemLucro = receita > 0 ? (lucro / receita) * 100 : 0;
        lblMargem.Text = $"📊 Margem de Lucro Operacional: {margemLucro:F2}%";
        
        if (totalColmeias > 0)
            lblIAColmeia.Text = $"🤖 Insight IA: Média de produção estimada em {(producaoBruta / totalColmeias):N2} kg por colmeia ativa.";
        else
            lblIAColmeia.Text = "🤖 Insight IA: Aguardando cadastro de colmeias para projetar capacidade.";

        decimal perda = producaoBruta - producaoEnvase; 
        if (perda > 0) lstAlertas.Items.Add($"🍯 Perda de mel (Bruto x Envasado): {perda:N2} kg"); 
        if (eficiencia < 80 && producaoBruta > 0) lstAlertas.Items.Add("⚠️ Baixa eficiência no envase!"); 
        if (lucro < 0) lstAlertas.Items.Add("🚨 Sistema operando no prejuízo!"); 
        
        if (eficiencia >= 90) lstRecomendacoes.Items.Add("✅ Excelente eficiência de produção."); 
        if (lucro > 0) lstRecomendacoes.Items.Add("💰 Operação com lucro positivo."); 

        AtualizarGrafico(); 
        AtualizarGraficoComparativo(); 
    } 

    // ➕ ACRESCIMO SEGURO: Novo método para ler o CSV de duas colunas do envase sem travar
    decimal SomarEnvaseSimples(string arquivo)
    {
        if (!File.Exists(arquivo)) return 0;
        decimal total = 0;
        foreach (var l in File.ReadAllLines(arquivo).Skip(1))
        {
            var c = l.Split(',');
            if (c.Length < 2) continue;
            if (decimal.TryParse(c[1], NumberStyles.Any, CultureInfo.InvariantCulture, out decimal v))
                total += v;
        }
        return total;
    }

    void AtualizarGrafico() 
    { 
        if (!File.Exists("dados/manejo.csv")) return; 
        var valores = new List<double>(); 
        foreach (var l in File.ReadAllLines("dados/manejo.csv").Skip(1)) 
        { 
            if (string.IsNullOrWhiteSpace(l)) continue;
            var c = l.Split(',').Select(x => x.Trim()).ToArray(); 
            if (c.Length < 5) continue; 

            string tipoManejo = c[3].ToUpper().Replace("Ç", "C").Replace("Ã", "A").Replace("Õ", "O");

            if (tipoManejo.Contains("PRODUCAO")) 
            { 
                string valorTexto = c[4].Replace(",", ".");
                if (double.TryParse(valorTexto, NumberStyles.Any, CultureInfo.InvariantCulture, out double v)) 
                { 
                    valores.Add(v); 
                } 
            } 
        } 
        chartProducao.Series = new ISeries[] { 
            new LineSeries<double> { Name = "Produção Bruta (kg)", Values = valores, Stroke = new SolidColorPaint(SKColors.Gold, 3) } 
        }; 
    }

    void AtualizarGraficoComparativo() 
    { 
        double producao = 0; 
        double envase = 0; 

        if (File.Exists("dados/manejo.csv")) 
        { 
            foreach (var l in File.ReadAllLines("dados/manejo.csv").Skip(1)) 
            { 
                if (string.IsNullOrWhiteSpace(l)) continue;
                var c = l.Split(',').Select(x => x.Trim()).ToArray(); 
                if (c.Length < 5) continue;
                
                string tipoManejo = c[3].ToUpper().Replace("Ç", "C").Replace("Ã", "A").Replace("Õ", "O");
                if (tipoManejo.Contains("PRODUCAO")) 
                { 
                    string valorTexto = c[4].Replace(",", ".");
                    if (double.TryParse(valorTexto, NumberStyles.Any, CultureInfo.InvariantCulture, out double v)) producao += v; 
                } 
            } 
        } 

        if (File.Exists("dados/envase.csv")) 
        { 
            foreach (var l in File.ReadAllLines("dados/envase.csv").Skip(1)) 
            { 
                if (string.IsNullOrWhiteSpace(l)) continue;
                var c = l.Split(',').Select(x => x.Trim()).ToArray(); 
                if (c.Length < 2) continue; 
                
                string valorTexto = c[c.Length - 1].Replace(",", ".");
                if (double.TryParse(valorTexto, NumberStyles.Any, CultureInfo.InvariantCulture, out double v)) envase += v; 
            } 
        } 

        chartComparativo.Series = new ISeries[] { 
            new ColumnSeries<double> { Name = "Mel Bruto Colhido (kg)", Values = new double[] { producao }, Fill = new SolidColorPaint(SKColors.Gold) }, 
            new ColumnSeries<double> { Name = "Mel Envasado Final (kg)", Values = new double[] { envase }, Fill = new SolidColorPaint(SKColors.LimeGreen) } 
        }; 
    }



    decimal SomarArquivoFlexivel(string arquivo, string filtro) 
    { 
        if (!File.Exists(arquivo)) return 0; 
        decimal total = 0; 
        foreach (var l in File.ReadAllLines(arquivo).Skip(1)) 
        { 
            if (string.IsNullOrWhiteSpace(l)) continue;
            var c = l.Split(',').Select(x => x.Trim()).ToArray(); 
            if (c.Length < 2) continue; 
            
            string linhaTextoCompleta = string.Join(" ", c).ToUpper()
                .Replace("Í", "I").Replace("Á", "A").Replace("Ç", "C").Replace("Õ", "O"); 

            if (!linhaTextoCompleta.Contains(filtro.ToUpper())) continue; 
            
            // Pega sempre a última coluna da linha (onde fica o valor monetário)
            string valorTexto = c[c.Length - 1].Replace(",", ".");
            if (decimal.TryParse(valorTexto, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal v)) 
            { 
                total += v; 
            } 
        } 
        return total; 
    }


    decimal SomarArquivo(string arquivo, int index, string filtro = null) 
    { 
        if (!File.Exists(arquivo)) return 0; 
        decimal total = 0; 
        
        foreach (var l in File.ReadAllLines(arquivo).Skip(1)) 
        { 
            if (string.IsNullOrWhiteSpace(l)) continue;
            
            var c = l.Split(',').Select(x => x.Trim()).ToArray(); 
            if (c.Length < 2) continue; 
            
            if (filtro != null)
            {
                string linhaTextoCompleta = string.Join(" ", c).ToUpper()
                    .Replace("Í", "I").Replace("Á", "A");
                    
                string filtroLimpo = filtro.Trim().ToUpper()
                    .Replace("Í", "I").Replace("Á", "A");

                if (!linhaTextoCompleta.Contains(filtroLimpo)) continue;
            }

            int indiceAlvo = (index < c.Length) ? index : c.Length - 1;
            string valorTexto = c[indiceAlvo].Replace(",", ".");

            if (decimal.TryParse(valorTexto, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal v)) 
            { 
                total += v; 
            } 
        } 
        return total; 
    }
    decimal SomarProducaoManejo(string arquivo)
    {
        if (!File.Exists(arquivo)) return 0;
        decimal total = 0;
        
        foreach (var l in File.ReadAllLines(arquivo).Skip(1))
        {
            if (string.IsNullOrWhiteSpace(l)) continue;
            
            var c = l.Split(',').Select(x => x.Trim()).ToArray();
            // Garante que a linha tem pelo menos as 5 colunas obrigatórias (Id, Data, Colmeia, Tipo, Quantidade)
            if (c.Length < 5) continue;

            // Trata todas as variações possíveis de digitação da palavra Produção/Produçao
            string tipoManejo = c[3].ToUpper()
                .Replace("Ç", "C").Replace("Ã", "A").Replace("Õ", "O");

            if (tipoManejo.Contains("PRODUCAO"))
            {
                // Lê estritamente a coluna 'Quantidade' (índice 4), tratando a cultura numérica
                string valorTexto = c[4].Replace(",", ".");
                if (decimal.TryParse(valorTexto, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal v))
                {
                    total += v;
                }
            }
        }
        return total;
    }



    int ContarLinhas(string arquivo) 
    { 
        if (!File.Exists(arquivo)) return 0; 
        return File.ReadAllLines(arquivo).Skip(1).Count(); 
    } 

    void BtnBackup_Click(object sender, EventArgs e) 
    { 
        try 
        { 
            string origem = "dados"; 
            string destino = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Backup_ApiarioRD_" + DateTime.Now.ToString("ddMMyyyy_HHmmss")); 
            DirectoryCopy(origem, destino, true); 
            MessageBox.Show("Backup realizado com sucesso!", "ApiarioRD", MessageBoxButtons.OK, MessageBoxIcon.Information); 
        } 
        catch (Exception ex) 
        { 
            MessageBox.Show("Erro no backup: " + ex.Message); 
        } 
    } 

    void DirectoryCopy(string sourceDir, string destDir, bool copySubDirs) 
    { 
        DirectoryInfo dir = new DirectoryInfo(sourceDir); 
        if (!dir.Exists) return; 
        DirectoryInfo[] dirs = dir.GetDirectories(); 
        Directory.CreateDirectory(destDir); 
        FileInfo[] files = dir.GetFiles(); 
        foreach (FileInfo file in files) 
        { 
            string tempPath = Path.Combine(destDir, file.Name); 
            file.CopyTo(tempPath, true); 
        } 
        if (copySubDirs) 
        { 
            foreach (DirectoryInfo subdir in dirs) { string tempPath = Path.Combine(destDir, subdir.Name); DirectoryCopy(subdir.FullName, tempPath, true); } 
        } 
    } 
}
