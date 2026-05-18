# 📊🐝 ApiarioRD - Sistema de Gestao Apicola Integrado

O **ApiárioRD** é um ecossistema ERP completo desenvolvido para a gestão técnica, comercial e financeira de apiários. O sistema automatiza o controlo de colmeias, monitorização de maneios técnicos (como controlo de Varroa), processos de envase de mel com tabelas comerciais dinâmicas e disponibiliza um Dashboard analítico em tempo real com indicadores de eficiência orientados por dados.

## 🚀 Funcionalidades Principais

*   **📊 Dashboard Profissional (LiveCharts + SkiaSharp):** Indicadores financeiros em tempo real (Receita, Custos, Lucro, Insumos, Manutenção), eficiência da colheita e insights inteligentes de capacidade produtiva.
*   **🍯 Módulo de Envase Inteligente (Vínculo de Estoque):** Interface que traduz regras de negócio complexas. Dá baixa automática no estoque de mel bruto e insumos (potes de plástico, vidro e bisnagas) aplicando tabelas de preço fixo comercial por peso (1kg, 500g, 300g, 250g, 200g, 40g).
*   **🐝 Gestão Técnica de Colmeias:** Registo, atualização e remoção de colmeias e históricos de maneio sem persistência de linhas fantasmas.
*   **💸 Fluxo de Caixa Integrado:** Lançamentos automatizados de entradas e saídas de capital com inteligência de parsing global e normalização de SKUs.
*   **💾 Sistema Automatizado de Backup:** Cópia de segurança instantânea do banco de dados para a Área de Trabalho com carimbo de data/hora (Timestamp).

## 🛠️ Tecnologias Utilizadas

*   **Linguagem Principal:** C# (.NET 8.0 / Windows Forms)
*   **Renderização de Gráficos:** LiveChartsCore & SkiaSharp
*   **Persistência de Dados:** Arquivos universais `.csv` (Agnóstico a Excel e LibreOffice Calc)
*   **IDE:** Visual Studio Code

## 💾 Estrutura do Banco de Dados (`/dados`)

O sistema armazena a informação em ficheiros estruturados de texto puro para garantir portabilidade absoluta:
*   `estoque.csv` - Controlo quantitativo de matéria-prima e insumos normalizados.
*   `movimentacao.csv` - Histórico cronológico de entradas e saídas do armazém.
*   `caixa.csv` - Fluxo financeiro categorizado.
*   `manejo.csv` - Registo unificado de colheitas, revisões e comportamento de colmeias.
*   `envase.csv` - Log de produção final envasada.

## 📦 Como Compilar e Gerar o Executável (.exe)

Para gerar uma build standalone (ficheiro único independente que não exige a instalação do .NET no cliente final), execute o seguinte comando no terminal:

```bash
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:PublishReadyToRun=true
```

O executável final será gerado na pasta:
`bin/Release/net8.0-windows/win-x64/publish/`

## 🏁 Instalação e Execução

1. Mova o ficheiro `ApiarioRD.exe` para a pasta de destino.
2. Certifique-se de que a pasta `dados/` contendo os ficheiros `.csv` está localizada no mesmo diretório do executável.
3. Execute o `ApiarioRD.exe`.

## 📄 Licença

Este projeto está sob a licença MIT - consulte o ficheiro [LICENSE](LICENSE) para detalhes.
