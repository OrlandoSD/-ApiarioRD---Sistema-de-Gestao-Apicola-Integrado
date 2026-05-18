using System;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

public static class LayoutHelper
{
    public static void AplicarLayoutDuplo(Form form, DataGridView esquerda, DataGridView direita)
    {
        int margem = 20;

        int larguraTotal = form.ClientSize.Width - (margem * 3);
        int largura = larguraTotal / 2;

        int altura = form.ClientSize.Height - (margem * 2);

        esquerda.Left = margem;
        esquerda.Top = margem;
        esquerda.Width = largura;
        esquerda.Height = altura;

        direita.Left = esquerda.Left + largura + margem;
        direita.Top = margem;
        direita.Width = largura;
        direita.Height = altura;

        esquerda.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
        direita.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
    }
}