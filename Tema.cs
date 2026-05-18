using System;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;


public static class Tema
{
    // 🎨 CORES PRINCIPAIS
    public static Color FundoForm = Color.FromArgb(45, 45, 45);
    public static Color FundoPainel = Color.FromArgb(30, 30, 30);
    public static Color FundoGrid = Color.FromArgb(30, 30, 30);

    // 🎨 CORES DE TEXTO
    public static Color Texto = Color.White;
    public static Color TextoSecundario = Color.Gainsboro;

    // 🎨 CORES DE DESTAQUE
    public static Color Primaria = Color.DarkOrange;
    public static Color Secundaria = Color.LightBlue;
    public static Color Sucesso = Color.Green;
    public static Color Alerta = Color.Gold;
    public static Color Erro = Color.Red;

    // 🎨 GRID
    public static Color Linha1 = Color.FromArgb(45, 45, 45);
    public static Color Linha2 = Color.FromArgb(60, 60, 60);
    public static Color Selecao = Color.LightBlue;

    // 🔤 FONTES
    public static Font FontePadrao = new Font("Segoe UI", 9);
    public static Font FonteNegrito = new Font("Segoe UI", 9, FontStyle.Bold);
}