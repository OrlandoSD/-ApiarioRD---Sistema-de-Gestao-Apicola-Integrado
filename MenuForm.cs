using System;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

public class MenuForm : FormBase
{
    Panel sidebar = new Panel();
    Panel content = new Panel();

    Panel painelOpcoes = new Panel(); // 🔥 submenu

    Button btnRelatorio;
    Button btnUsuarios;

    Button btnBackup = new Button()
    {
        Text = "Backup",
        Left = 10,
        Top = 620,
        Width = 120
    };

    Button CriarBotao(string texto, int top)
    {
        Button btn = new Button()
        {
            Text = texto,
            Width = 180,
            Height = 40,
            Top = top,
            Left = 10,
            BackColor = Color.FromArgb(70, 70, 70),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Cursor = Cursors.Hand
        };

        btn.FlatAppearance.BorderSize = 0;

        // 🔥 ANIMAÇÃO HOVER
        btn.MouseEnter += (s, e) => btn.BackColor = Color.FromArgb(90, 90, 90);
        btn.MouseLeave += (s, e) => btn.BackColor = Color.FromArgb(70, 70, 70);

        return btn;
    }

    public MenuForm()
    {
        // ✔ ÍCONE SEGURO (não quebra no EXE)
        string caminhoIcone = Path.Combine(Application.StartupPath, "iconeRD.ico");
        if (File.Exists(caminhoIcone))
        {
            this.Icon = new Icon(caminhoIcone);
        }

        // ✔ BACKUP AO FECHAR
        this.FormClosing += (s, e) => BackupHelper.FazerBackup();

        // ✔ BOTÃO BACKUP
        btnBackup.Click += (s, e) =>
        {
            BackupHelper.FazerBackup();
            MessageBox.Show("Backup realizado com sucesso!");
        };

        Text = $"ApiarioRD ERP - Logado como: {Sessao.Usuario}";
        WindowState = FormWindowState.Maximized;
        BackColor = Color.FromArgb(45, 45, 45);

        sidebar.Width = 200;
        sidebar.Dock = DockStyle.Right;
        sidebar.BackColor = Color.FromArgb(50, 50, 50);

        content.Dock = DockStyle.Fill;
        content.BackColor = Color.FromArgb(45, 45, 45);

        int y = 20;
        int esp = 45;

        var btnDashboard = CriarBotao("🏠 Dashboard", y); y += esp;
        var btnVendas = CriarBotao("💰 Vendas", y); y += esp;
        var btnProdutos = CriarBotao("📦 Produtos", y); y += esp;
        var btnInsumos = CriarBotao("🧪 Insumos", y); y += esp;
        var btnColmeias = CriarBotao("🐝 Colmeias", y); y += esp;
        var btnManejo = CriarBotao("🛠 Manejo", y); y += esp;
        var btnProducao = CriarBotao("🏭 Produção", y); y += esp;
        var btnReposicao = CriarBotao("🔄 Reposição", y); y += esp;
        var btnCaixa = CriarBotao("💵 Caixa", y); y += esp;
        var btnProducaoColmeias = CriarBotao("🐝 Prod. Colmeias", y); y += esp;
        var btnEstoque = CriarBotao("📦 Estoque", y); y += esp;
        var btnManutencao = CriarBotao("🔧 Manutenção", y); y += esp;
        var btnEnvase = CriarBotao("🍯 Envase", y); y += esp;

        btnRelatorio = CriarBotao("📊 Relatórios", y); y += esp;
        btnUsuarios = CriarBotao("👥 Usuários", y); y += esp;

        var btnIA = CriarBotao("🤖 IA", y); y += esp;

        var btnOpcoes = CriarBotao("⚙️ Opções", y);
        btnOpcoes.BackColor = Color.Maroon;

        // 🔥 SUBMENU
        painelOpcoes.Width = 180;
        painelOpcoes.Height = 0;
        painelOpcoes.Left = 10;
        painelOpcoes.Top = y + 45;
        painelOpcoes.BackColor = Color.FromArgb(60, 60, 60);
        painelOpcoes.Visible = false;

        Button btnTrocar = CriarBotao("🔄 Trocar Usuário", 0);
        Button btnSair = CriarBotao("❌ Sair do Sistema", 45);

        painelOpcoes.Controls.Add(btnTrocar);
        painelOpcoes.Controls.Add(btnSair);

        sidebar.Controls.AddRange(new Control[]
        {
            btnDashboard, btnVendas, btnProdutos, btnInsumos,
            btnColmeias, btnManejo, btnProducao,
            btnProducaoColmeias, btnEnvase,
            btnReposicao, btnCaixa, btnRelatorio,
            btnUsuarios, btnManutencao, btnEstoque,
            btnIA,
            btnOpcoes,
            painelOpcoes,
            btnBackup // ✔ agora no lugar certo
        });

        Controls.Add(content);
        Controls.Add(sidebar);

        // EVENTOS
        btnDashboard.Click += (s,e)=>AbrirForm(new DashboardForm());
        btnVendas.Click += (s,e)=>AbrirForm(new VendasForm());
        btnProdutos.Click += (s,e)=>AbrirForm(new ProdutosForm());
        btnInsumos.Click += (s,e)=>AbrirForm(new InsumosForm());
        btnColmeias.Click += (s,e)=>AbrirForm(new ColmeiasForm());
        btnManejo.Click += (s,e)=>AbrirForm(new ManejoForm());
        btnProducao.Click += (s,e)=>AbrirForm(new ProducaoColmeiaForm());
        btnReposicao.Click += (s,e)=>AbrirForm(new ReposicaoForm());
        btnCaixa.Click += (s,e)=>AbrirForm(new CaixaForm());
        btnManutencao.Click += (s,e)=>AbrirForm(new ManutencaoForm());
        btnRelatorio.Click += (s,e)=>AbrirForm(new RelatorioForm());
        btnUsuarios.Click += (s,e)=>AbrirForm(new UsuarioForm());
        btnProducaoColmeias.Click += (s,e)=>AbrirForm(new ProducaoColmeiaForm());
        btnEstoque.Click += (s,e)=>AbrirForm(new EstoqueForm());
        btnEnvase.Click += (s,e)=>AbrirForm(new EnvaseForm());

        // IA
        btnIA.Click += async (s, e) =>
        {
            try
            {
                AgenteEstoque agente = new AgenteEstoque();
                await agente.Executar();
                MessageBox.Show("Agente executado com sucesso!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro: " + ex.Message);
            }
        };

        // SUBMENU ANIMADO
        btnOpcoes.Click += (s, e) =>
        {
            Timer timer = new Timer();
            timer.Interval = 10;

            bool expandir = !painelOpcoes.Visible;
            painelOpcoes.Visible = true;

            timer.Tick += (s2, e2) =>
            {
                if (expandir)
                {
                    painelOpcoes.Height += 10;
                    if (painelOpcoes.Height >= 90)
                        timer.Stop();
                }
                else
                {
                    painelOpcoes.Height -= 10;
                    if (painelOpcoes.Height <= 0)
                    {
                        painelOpcoes.Visible = false;
                        timer.Stop();
                    }
                }
            };

            timer.Start();
        };

        btnTrocar.Click += (s, e) => TrocarUsuario();
        btnSair.Click += (s, e) => SairSistema();

        AplicarPermissoes();
        AbrirForm(new DashboardForm());
    }

    void AplicarPermissoes()
    {
        if (Sessao.Usuario != "admin" && !Sessao.PodeExcluir)
        {
            btnUsuarios.Enabled = false;
            btnUsuarios.BackColor = Color.FromArgb(40, 40, 40);
            btnUsuarios.Text = "🔒 Usuários";
        }

        if (!Sessao.PodeVer)
        {
            btnRelatorio.Enabled = false;
            btnRelatorio.BackColor = Color.FromArgb(40, 40, 40);
            btnRelatorio.Text = "🔒 Relatórios";
        }
    }

    void AbrirForm(Form form)
    {
        content.Controls.Clear();

        form.TopLevel = false;
        form.FormBorderStyle = FormBorderStyle.None;
        form.Dock = DockStyle.Fill;

        content.Controls.Add(form);
        form.Show();
    }

    void TrocarUsuario()
    {
        if (MessageBox.Show("Deseja trocar de usuário?", "Logout",
            MessageBoxButtons.YesNo) == DialogResult.Yes)
        {
            Sessao.Resetar();
            this.Hide();
            new LoginForm().ShowDialog();
            this.Close();
        }
    }

    void SairSistema()
    {
        if (MessageBox.Show("Deseja sair do sistema?", "Sair",
            MessageBoxButtons.YesNo) == DialogResult.Yes)
        {
            Application.Exit();
        }
    }
}