using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;

public class FormBase : Form
{
    public FormBase()
    {
        this.Load += FormBase_Load;

        // ✔ Ícone aplicado corretamente
        string caminhoIcone = Path.Combine(
            Application.StartupPath,
            "iconeRD.ico"
        );

        if (File.Exists(caminhoIcone))
        {
            this.Icon = new Icon(caminhoIcone);
        }
    }

    private void FormBase_Load(object sender, EventArgs e)
    {
        TemaAplicador.Aplicar(this);
    }
}