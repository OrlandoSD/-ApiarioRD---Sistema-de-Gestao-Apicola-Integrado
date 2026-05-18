using System;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

public static class TemaHelper
{
    public static void Aplicar(Control controle)
    {
        controle.BackColor = Color.FromArgb(45, 45, 45);
        controle.ForeColor = Color.White;

        foreach (Control c in controle.Controls)
        {
            if (c is Button btn)
            {
                btn.BackColor = Color.FromArgb(60, 60, 60);
                btn.ForeColor = Color.White;
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 0;
            }
            else if (c is TextBox txt)
            {
                txt.BackColor = Color.FromArgb(30, 30, 30);
                txt.ForeColor = Color.White;
                txt.BorderStyle = BorderStyle.FixedSingle;
            }
            else if (c is ComboBox combo)
            {
                combo.BackColor = Color.FromArgb(30, 30, 30);
                combo.ForeColor = Color.White;
            }

            // 🔁 recursivo (pega tudo)
            if (c.HasChildren)
                Aplicar(c);
        }
    }
}