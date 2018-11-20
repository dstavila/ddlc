using System;
using System.IO;
using System.Text;

namespace ddlc
{
    public static class Utils
    {
        public static string ToCPPNamespace(string name)
        {
            var split = name.Split('.');
            var result = split[0];
            for (var i = 1; i < split.Length; ++i)
                result += " { " + string.Format("namespace {0}", split[i]);
            return result;
        }
        public static string BuildNamespace(DDLDecl decl)
        {
            if (decl.Parent == null)
                return decl.Name;
            return BuildNamespace(decl.Parent) + "::" + decl.Name;
        }

        public static string ExtraCommandNamespace(string space)
        {
            var list = space.Split('.');
            if (list.Length > 2)
            {
                if (list[0] == "DDL" && list[1] == "Commands")
                {
                    var sb = new StringBuilder();
                    var len = list.Length; 
                    for (var i = 2; i < len - 1; ++i)
                    {
                        sb.Append(list[i]);
                        sb.Append("_");
                    }
                    sb.Append(list[len - 1]);
                    return sb.ToString();
                }
                return null;
            }
            return null;
        }
        public static void Dos2Unix(string fileName)
        {
            const byte CR = 0x0D;
            const byte LF = 0x0A;
            byte[] data = File.ReadAllBytes(fileName);
            using (FileStream fileStream = File.OpenWrite(fileName))
            {
                BinaryWriter bw = new BinaryWriter(fileStream);
                int position = 0;
                int index = 0;
                do
                {
                    index = Array.IndexOf<byte>(data, CR, position);
                    if ((index >= 0) && (data[index + 1] == LF))
                    {
                        // Write before the CR
                        bw.Write(data, position, index - position);
                        // from LF
                        position = index + 1;
                    }
                }
                while (index >= 0);
                bw.Write(data, position, data.Length - position);
                fileStream.SetLength(fileStream.Position);
            }
        }
    }
}