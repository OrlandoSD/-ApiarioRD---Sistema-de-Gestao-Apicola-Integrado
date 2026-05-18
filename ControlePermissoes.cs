using System;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

public static class ControlePermissoes
{
    public static void Aplicar(Form form)
    {
        foreach (Control c in form.Controls)
        {
            if (c is Button btn)
            {
                if (btn.Text.Contains("Salvar")) btn.Enabled = Sessao.PodeInserir;
                if (btn.Text.Contains("Excluir")) btn.Enabled = Sessao.PodeExcluir;
            }
            else if (c is CheckBox chk)
            {
                if (chk.Text.Contains("Ver")) chk.Enabled = Sessao.PodeVer;
                if (chk.Text.Contains("Inserir")) chk.Enabled = Sessao.PodeInserir;
                if (chk.Text.Contains("Editar")) chk.Enabled = Sessao.PodeEditar;
                if (chk.Text.Contains("Excluir")) chk.Enabled = Sessao.PodeExcluir;
            }
        }
    }
}