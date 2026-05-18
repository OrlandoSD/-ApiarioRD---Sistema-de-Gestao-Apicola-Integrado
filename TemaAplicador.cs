using System;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;


public static class TemaAplicador
{
    public static void Aplicar(Form form)
    {
        form.BackColor = Tema.FundoForm;
        form.ForeColor = Tema.Texto;
        form.Font = Tema.FontePadrao;

        AplicarControles(form);
    }

    private static void AplicarControles(Control controle)
    {
        foreach (Control c in controle.Controls)
        {
            // Botões
            if (c is Button btn)
            {
                btn.BackColor = Tema.Primaria;
                btn.ForeColor = Tema.Texto;
                btn.FlatStyle = FlatStyle.Flat;
                btn.Font = Tema.FonteNegrito;
            }

            // Labels
            if (c is Label lbl)
            {
                lbl.ForeColor = Tema.Texto;
                lbl.Font = Tema.FontePadrao;
            }

            // TextBox
            if (c is TextBox txt)
            {
                txt.BackColor = Tema.FundoPainel;
                txt.ForeColor = Tema.Texto;
                txt.BorderStyle = BorderStyle.FixedSingle;
            }

            // ComboBox
            if (c is ComboBox cmb)
            {
                cmb.BackColor = Tema.FundoPainel;
                cmb.ForeColor = Tema.Texto;
            }

            // DataGridView
            if (c is DataGridView grid)
            {
                GridEstilo.Aplicar(grid);
            }

            // Recursivo (painéis, groupbox, etc.)
            if (c.HasChildren)
            {
                AplicarControles(c);
            }
        }
    }
}