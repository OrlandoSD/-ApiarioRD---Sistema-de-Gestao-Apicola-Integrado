using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;

public class LoginForm : Form
{
    TextBox txtUser = new TextBox() { Top = 30, Left = 100, Width = 150 };
    TextBox txtSenha = new TextBox() { Top = 70, Left = 100, Width = 150, PasswordChar = '*' };
    Button btnLogin = new Button() { Text = "Entrar", Top = 110, Left = 100, Width = 150, Height = 35 };
    Button btnAlterarSenha = new Button() { Text = "Alterar Senha", Top = 150, Left = 100, Width = 150, Height = 35 };

    public LoginForm()
    {
        Text = "ApiarioRD - Login 🔐";
        Width = 360;
        Height = 280;
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;

        // Estilização rápida
        this.BackColor = Color.FromArgb(45, 45, 45);
        this.ForeColor = Color.White;

        Label lblU = new Label() { Text = "Usuário:", Top = 30, Left = 20, AutoSize = true, ForeColor = Color.White };
        Label lblS = new Label() { Text = "Senha:", Top = 70, Left = 20, AutoSize = true, ForeColor = Color.White };

        Controls.AddRange(new Control[] { lblU, lblS, txtUser, txtSenha, btnLogin, btnAlterarSenha });

        // Eventos
        btnLogin.Click += (s, e) => Logar();
        btnAlterarSenha.Click += (s, e) => AbrirAlterarSenha();
        this.AcceptButton = btnLogin;

        try { TemaHelper.Aplicar(this); } catch { }

        CriarUsuariosPadrao();
    }

    void CriarUsuariosPadrao()
    {
        Directory.CreateDirectory("dados");
        string caminho = "dados/usuario.txt";

        if (!File.Exists(caminho))
        {
            File.WriteAllText(caminho,
                "admin;123;1,1,1,1" + Environment.NewLine +
                "operador;123;1,1,0,0" + Environment.NewLine +
                "// PERMISSOES: VER,INSERIR,EDITAR,EXCLUIR");
        }
    }

    void Logar()
    {
        string caminho = "dados/usuario.txt";
        if (!File.Exists(caminho)) { MessageBox.Show("Arquivo de usuários não encontrado!"); return; }

        var linhas = File.ReadAllLines(caminho);

        foreach (var l in linhas)
        {
            if (string.IsNullOrWhiteSpace(l) || l.StartsWith("//")) continue;

            var colunas = l.Split(';');
            if (colunas.Length < 3) continue;

            if (txtUser.Text == colunas[0] && txtSenha.Text == colunas[1])
            {
                Sessao.Usuario = colunas[0];

                var p = colunas[2].Split(',');
                Sessao.PodeVer = p.Length > 0 && p[0] == "1";
                Sessao.PodeInserir = p.Length > 1 && p[1] == "1";
                Sessao.PodeEditar = p.Length > 2 && p[2] == "1";
                Sessao.PodeExcluir = p.Length > 3 && p[3] == "1";

                this.Hide();
                new MenuForm().ShowDialog();
                this.Close();
                return;
            }
        }

        MessageBox.Show("Usuário ou senha inválidos!", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        txtSenha.Clear();
        txtUser.Focus();
    }

    void AbrirAlterarSenha()
    {
        if (string.IsNullOrWhiteSpace(txtUser.Text))
        {
            MessageBox.Show("Digite o nome do usuário antes de alterar a senha!", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtUser.Focus();
            return;
        }

        // Abre o UsuarioForm em modo alterar senha
        UsuarioForm uf = new UsuarioForm(true, txtUser.Text);
        uf.ShowDialog();
    }
}