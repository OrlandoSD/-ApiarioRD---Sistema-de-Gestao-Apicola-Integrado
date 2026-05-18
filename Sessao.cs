using System;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

public static class Sessao
{
    // Armazena o nome do usuário logado (ex: "admin" ou "operador")
    public static string Usuario { get; set; }

    // Permissões baseadas no seu arquivo usuario.txt (0 ou 1)
    // Usamos bool para facilitar o uso de "if (Sessao.PodeExcluir)" nas telas
    public static bool PodeVer { get; set; }
    public static bool PodeInserir { get; set; }
    public static bool PodeEditar { get; set; }
    public static bool PodeExcluir { get; set; }

    // Método útil para limpar a sessão ao fazer Logout
    public static void Resetar()
    {
        Usuario = string.Empty;
        PodeVer = false;
        PodeInserir = false;
        PodeEditar = false;
        PodeExcluir = false;
    }
}
