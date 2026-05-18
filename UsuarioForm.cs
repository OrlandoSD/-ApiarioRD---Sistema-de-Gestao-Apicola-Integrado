using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

public class UsuarioForm : FormBase
{
    TextBox txtUser;
    TextBox txtSenha;
    CheckBox chkVer, chkInserir, chkEditar, chkExcluir;
    bool alterarSenha = false; // flag para saber se estamos alterando a senha
    string usuarioAlterar = ""; // usuário específico a alterar

    // Construtor normal (cadastro)
    public UsuarioForm() : this(false, "") { }

    // Construtor para alterar senha de um usuário específico
    public UsuarioForm(bool alterarSenha, string usuario)
    {
        this.alterarSenha = alterarSenha;
        this.usuarioAlterar = usuario;

        this.Text = alterarSenha ? $"Alterar Senha: {usuario}" : "Cadastro de Usuário";
        this.Size = new Size(350, 320);

        // 🎨 FUNDO MAIS AGRADÁVEL
        this.BackColor = Color.FromArgb(60, 63, 65);
        this.ForeColor = Color.White;

        // 🔤 LABELS
        Label lblUser = new Label()
        {
            Text = "Usuário",
            Top = 0,
            Left = 50,
            AutoSize = true,
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };

        Label lblSenha = new Label()
        {
            Text = "Senha",
            Top = 40,
            Left = 50,
            AutoSize = true,
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };

        // 🔢 CAMPOS
        txtUser = new TextBox() { Top = 20, Left = 50, Width = 230 };
        txtSenha = new TextBox() { Top = 60, Left = 50, Width = 230, PasswordChar = '*' };

        // ☑️ PERMISSÕES
        chkVer = new CheckBox() { Text = "Pode Ver", Top = 100, Left = 50 };
        chkInserir = new CheckBox() { Text = "Pode Inserir", Top = 130, Left = 50 };
        chkEditar = new CheckBox() { Text = "Pode Editar", Top = 160, Left = 50 };
        chkExcluir = new CheckBox() { Text = "Pode Excluir", Top = 190, Left = 50 };

        // 🔘 BOTÕES
        Button btnSalvar = new Button() { Text = alterarSenha ? "Alterar Senha" : "Salvar", Top = 230, Left = 50, Width = 110 };
        Button btnCancelar = new Button() { Text = "Cancelar", Top = 230, Left = 170, Width = 110 };

        // 🎨 APLICA ESTILO
        Estilizar(lblUser);
        Estilizar(lblSenha);
        Estilizar(txtUser);
        Estilizar(txtSenha);
        Estilizar(chkVer);
        Estilizar(chkInserir);
        Estilizar(chkEditar);
        Estilizar(chkExcluir);
        Estilizar(btnSalvar);
        Estilizar(btnCancelar);

        // EVENTOS
        btnSalvar.Click += (s, e) => Salvar();
        btnCancelar.Click += (s, e) => this.Close();

        Controls.AddRange(new Control[]
        {
            lblUser, lblSenha,
            txtUser, txtSenha,
            chkVer, chkInserir, chkEditar, chkExcluir,
            btnSalvar, btnCancelar
        });

        // Se for alterar senha, bloqueia edição do usuário e das permissões
        if (alterarSenha)
        {
            txtUser.Text = usuario;
            txtUser.Enabled = false;
            chkVer.Enabled = false;
            chkInserir.Enabled = false;
            chkEditar.Enabled = false;
            chkExcluir.Enabled = false;
        }
    }

    void Estilizar(Control c)
    {
        if (c is Label)
        {
            c.ForeColor = Color.White;
            c.BackColor = Color.Transparent;
        }
        else if (c is CheckBox chk)
        {
            chk.ForeColor = Color.White;
            chk.BackColor = Color.Transparent;
        }
        else if (c is Button btn)
        {
            btn.BackColor = Color.FromArgb(80, 80, 80);
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;

            btn.MouseEnter += (s, e) => btn.BackColor = Color.FromArgb(100, 100, 100);
            btn.MouseLeave += (s, e) => btn.BackColor = Color.FromArgb(80, 80, 80);
        }
        else
        {
            c.BackColor = Color.FromArgb(30, 30, 30);
            c.ForeColor = Color.White;
        }
    }

    void Salvar()
    {
        Directory.CreateDirectory("dados");
        string caminho = "dados/usuario.txt";

        if (string.IsNullOrWhiteSpace(txtUser.Text) || string.IsNullOrWhiteSpace(txtSenha.Text))
        {
            MessageBox.Show("Preencha usuário e senha!");
            return;
        }

        if (alterarSenha)
        {
            // Alterar apenas a senha do usuário
            if (!File.Exists(caminho))
            {
                MessageBox.Show("Arquivo de usuários não encontrado!");
                return;
            }

            var linhas = File.ReadAllLines(caminho).ToList();
            bool encontrado = false;

            for (int i = 0; i < linhas.Count; i++)
            {
                var l = linhas[i];
                if (l.StartsWith("//")) continue;
                var c = l.Split(';');
                if (c.Length < 2) continue;

                if (c[0] == usuarioAlterar)
                {
                    linhas[i] = $"{c[0]};{txtSenha.Text};{c[2]}"; // mantém permissões
                    encontrado = true;
                    break;
                }
            }

            if (!encontrado)
            {
                MessageBox.Show("Usuário não encontrado!");
                return;
            }

            File.WriteAllLines(caminho, linhas);
            MessageBox.Show("Senha alterada com sucesso!");
            this.Close();
            return;
        }

        // 🔎 EVITAR DUPLICADO
        if (File.Exists(caminho))
        {
            var linhas = File.ReadAllLines(caminho);

            foreach (var l in linhas)
            {
                if (l.StartsWith("//")) continue;

                var c = l.Split(';');

                if (c.Length > 0 && c[0] == txtUser.Text)
                {
                    MessageBox.Show("Usuário já existe!");
                    return;
                }
            }
        }

        string linha = $"{txtUser.Text};{txtSenha.Text};" +
                       $"{(chkVer.Checked ? 1 : 0)}," +
                       $"{(chkInserir.Checked ? 1 : 0)}," +
                       $"{(chkEditar.Checked ? 1 : 0)}," +
                       $"{(chkExcluir.Checked ? 1 : 0)}";

        File.AppendAllText(caminho, linha + Environment.NewLine);
        MessageBox.Show("Usuário salvo com sucesso!");
        Limpar();
    }

    void Limpar()
    {
        txtUser.Text = "";
        txtSenha.Text = "";
        chkVer.Checked = false;
        chkInserir.Checked = false;
        chkEditar.Checked = false;
        chkExcluir.Checked = false;
        txtUser.Focus();
    }
}