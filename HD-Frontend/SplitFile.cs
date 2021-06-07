using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

internal class SplitFile
{
    public delegate void ProgressCb(string manifest);

    public static void Split(string path, int size, ProgressCb progressCb)
    {
        byte[] buffer = new byte[16384];
        using (Stream stream = File.OpenRead(path))
        {
            int num = 0;
            string text = path + ".manifest";
            while (stream.Position < stream.Length)
            {
                string path2 = path + "_part_" + num;
                using (Stream stream2 = File.Create(path2))
                {
                    int num2 = size;
                    int num3 = 0;
                    while (num2 > 0)
                    {
                        num3 = stream.Read(buffer, 0, Math.Min(num2, 16384));
                        if (num3 == 0)
                        {
                            break;
                        }
                        stream2.Write(buffer, 0, num3);
                        num2 -= num3;
                    }
                }
                string manifest = null;
                using (Stream stream3 = File.OpenRead(path2))
                {
                    string arg = SplitFile.CheckSum(stream3);
                    long length = stream3.Length;
                    manifest = Path.GetFileName(path2) + " " + length + " " + arg;
                }
                progressCb(manifest);
                num++;
            }
        }
    }

    public static string CheckSum(Stream stream)
    {
        SHA1CryptoServiceProvider sHA1CryptoServiceProvider = new SHA1CryptoServiceProvider();
        byte[] array = sHA1CryptoServiceProvider.ComputeHash(stream);
        StringBuilder stringBuilder = new StringBuilder(array.Length * 2);
        byte[] array2 = array;
        foreach (byte b in array2)
        {
            stringBuilder.AppendFormat("{0:x2}", b);
        }
        return stringBuilder.ToString();
    }
}
