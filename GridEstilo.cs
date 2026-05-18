using System;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

public static class GridEstilo
{
    public static void Aplicar(DataGridView grid)
    {
        // Comportamento
        grid.ReadOnly = true;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.MultiSelect = false;
        grid.RowHeadersVisible = false;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        // Fundo geral
        grid.BackgroundColor = Color.FromArgb(30, 30, 30);
        grid.BorderStyle = BorderStyle.Fixed3D;

        // Cabeçalho
        grid.EnableHeadersVisualStyles = false;
        grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(45, 45, 45);
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);

        // Linhas
        grid.RowsDefaultCellStyle.BackColor = Color.FromArgb(45, 45, 45);
        grid.RowsDefaultCellStyle.ForeColor = Color.White;

        // Linhas alternadas
        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(60, 60, 60);

        // Seleção
        grid.DefaultCellStyle.SelectionBackColor = Color.LightBlue;
        grid.DefaultCellStyle.SelectionForeColor = Color.Black;

        // Grid lines
        grid.GridColor = Tema.Linha2;
    }

    
}