namespace Core.Storage;

// Атомарний запис: спершу пишемо тимчасовий файл .tmp, потім одним кроком замінюємо ним основний.
// Якщо програма впаде посеред запису, основний файл лишиться цілим (пошкодиться лише .tmp).
internal static class AtomicFile
{
    public static void WriteAllText(string path, string content)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        string tmp = path + ".tmp";
        File.WriteAllText(tmp, content);
        File.Move(tmp, path, overwrite: true);
    }
}
