using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Text;


namespace ddlc
{
    public static class UnityGen
    {
        public static string t1 = "    ";
        public static string t2 = "        ";
        public static string t3 = "            ";


        public static void Generate(
            string outDir,
            string sourceName,
            List<rNamespace> namespaces,
            List<rSelect> selects,
            List<rStruct> structs)
        {
            var sb = new StringBuilder();


            sb.AppendLine("//===----------------------------------------------------------------------===//");
            sb.AppendLine("//                                                                              ");
            sb.AppendLine("//  vim: ft=cs tw=80                                                            ");
            sb.AppendLine("//                                                                              ");
            sb.AppendLine("//  DDL Generated code, do not modify directly.                                 ");
            sb.AppendLine("//                                                                              ");
            sb.AppendLine("//===----------------------------------------------------------------------===//");
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.Append("\n\n");
            WriteNamespaces(sb, namespaces, selects, structs);


            Console.Error.WriteLine(sb.ToString());
            File.WriteAllText(Path.Combine(outDir, sourceName), sb.ToString());
            sb = null;
        }

        
        private static void WriteNamespaces(StringBuilder sb,
            List<rNamespace> Namespaces,
            List<rSelect> Selects,
            List<rStruct> Structs)
        {
            if (Namespaces.Count == 0)
                return;

            foreach (var n in Namespaces)
            {
                sb.AppendFormat("namespace {0}\n", n.Name);
                sb.AppendLine("{");
                foreach (string selectName in n.Selects)
                {
                    var sel = find_select_by_name(selectName, Selects);
                    if (sel != null)
                    {
                        sb.AppendFormat(t1 + "public enum {0} : uint\n", sel.Name);
                        sb.AppendLine(t1 + "{");
                        foreach (var memb in sel.Items)
                        {
                            sb.Append(buildSelectField(t2, sel.Name, memb.Name, memb.Value));
                        }

                        sb.AppendLine(t1 + "}");
                    }
                }

                foreach (var cn in n.Structs)
                {
                    var c = find_struct_by_name(cn, Structs);
                    if (c != null)
                    {
                        WriteClass(t1, c, sb, Structs);
                    }
                }

                sb.AppendLine("}");
            }
            sb.AppendLine("}");
        }

        private static void buildStructDictionary(string tab, StringBuilder sb, List<rStruct> Structs, string cn, string parentName)
        {
            var c = find_struct_by_name(cn, Structs);
            if (c != null)
            {
                foreach (var f in c.Fields)
                {
                    if (f.Type != EType.STRUCT)
                    {
                        string txt;
                        if (string.IsNullOrEmpty(parentName))
                            txt = f.Name;
                        else 
                            txt = parentName + "." + f.Name;
                        var fulltxt = string.Format("\"{0}\", {1}", txt, MurmurHash2.Hash(txt));
                        sb.AppendLine(tab + t2 + "{"+ fulltxt + "},");
                    }
                    else
                    {
                        buildStructDictionary(tab, sb, Structs, f.TypeName, f.Name);
                    }
                }
            }
        }

        private static string buildSelectField(string tab, string select, string field, string value)
        {
            string name = string.Format("{0}", field);
            if (!string.IsNullOrEmpty(value))
                return string.Format(tab + "{0} = {1},\n", name, value);
            return string.Format(tab + "{0} = {1},\n", name, MurmurHash2.Hash(select + "." + name));
        }

        private static void WriteClass(string tab, rStruct str, StringBuilder sb, List<rStruct> Structs)
        {
            sb.AppendLine(tab + "[Serializable]");
            sb.AppendFormat(tab + "public class {0}\n", str.Name);
            sb.AppendLine(tab + "{");

            foreach (var child in str.Childs)
            {
                if (child != null)
                    WriteClass(tab + t1, child, sb, Structs);
            }
            
            foreach (var field in str.Fields)
            {
                sb.Append(buildStructFieldHeader(tab + t1, field));
            }
            
            sb.AppendLine();
            sb.AppendLine(tab + t1 + "public static readonly Dictionary<string, uint> sFields = new Dictionary<string, uint> {");
            buildStructDictionary(tab, sb, Structs, str.Name, null);
            sb.AppendLine(tab + t1 + "}");
            sb.AppendLine(tab + "}");
        }

        private static string buildStructFieldHeader(string tab, rStructField m)
        {
            if (m.ArrayType == EArrayType.SCALAR)
            {
                if (string.IsNullOrEmpty(m.Value))
                    return string.Format(tab + "public {0} {1};\n", Converter.DDLTypeToCSharpType(m.Type, m.TypeName), m.Name);
                return string.Format(tab + "public {0} {1} = {2};\n", Converter.DDLTypeToCSharpType(m.Type, m.TypeName), m.Name, m.Value);
            }

            if (m.ArrayType == EArrayType.DYNAMIC)
            {
                return string.Format(tab + "public {0}[] {1};\n", Converter.DDLTypeToCSharpType(m.Type, m.TypeName), m.Name);
            }

            if (m.ArrayType == EArrayType.LIST)
            {
                return string.Format(tab + "public List<{0}> {1} = new List<{0}>();\n", Converter.DDLTypeToCSharpType(m.Type, m.TypeName), m.Name);
            }

            if (m.ArrayType == EArrayType.FIXED)
            {
                return string.Format(tab + "public {0}[{2}] {1};\n", Converter.DDLTypeToCSharpType(m.Type, m.TypeName), m.Name, m.Count);
            }

            return "ERROR";
        }


        private static rSelect find_select_by_name(string name, List<rSelect> Selects)
        {
            foreach (var s in Selects)
            {
                if (s.Name == name)
                    return s;
            }

            return null;
        }

        private static rStruct find_struct_by_name(string name, List<rStruct> Structs)
        {
            foreach (var s in Structs)
            {
                if (s.Name == name)
                    return s;
            }

            return null;
        }
    }
}