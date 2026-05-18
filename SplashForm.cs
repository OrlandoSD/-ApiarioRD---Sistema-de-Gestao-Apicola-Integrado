using System;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

public class SplashForm : Form
{
    Timer timer = new Timer();

    public SplashForm()
    {
        Width = 400;
        Height = 250;
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.None;
        BackColor = Color.FromArgb(45, 45, 45);

        // Ícone
        string caminhoIcone = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "iconeRD.ico"
        );

        if (File.Exists(caminhoIcone))
            this.Icon = new Icon(caminhoIcone);

        // Título
        Label titulo = new Label()
        {
            Text = "ApiarioRD",
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 18, FontStyle.Bold),
            AutoSize = true,
            Top = 80,
            Left = 120
        };

        // Subtítulo
        Label sub = new Label()
        {
            Text = "Sistema de Gestão de Apiário",
            ForeColor = Color.Gainsboro,
            Font = new Font("Segoe UI", 10),
            AutoSize = true,
            Top = 130,
            Left = 70
        };

        Controls.Add(titulo);
        Controls.Add(sub);

        timer.Interval = 2000;
        timer.Tick += (s, e) =>
        {
            timer.Stop();
            new MenuForm().Show(); // ou EstoqueForm
            this.Hide();
        };

        timer.Start();
    }
}