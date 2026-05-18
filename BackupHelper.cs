using System;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

public static class BackupHelper
{
    public static void FazerBackup()
    {
        string pastaBase = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "ApiarioRD"
        );

        string pastaDados = pastaBase;
        string pastaBackup = Path.Combine(pastaBase, "Backup");

        if (!Directory.Exists(pastaDados)) return;

        Directory.CreateDirectory(pastaBackup);

        string data = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        foreach (var arquivo in Directory.GetFiles(pastaDados, "*.csv"))
        {
            string nome = Path.GetFileNameWithoutExtension(arquivo);
            string destino = Path.Combine(pastaBackup, $"{nome}_{data}.csv");

            File.Copy(arquivo, destino, true);
        }

        var arquivos = new DirectoryInfo(pastaBackup)
            .GetFiles("*.csv")
            .OrderByDescending(f => f.CreationTime)
            .Skip(20);

        foreach (var file in arquivos)
        {
            file.Delete();
        }
    }
}