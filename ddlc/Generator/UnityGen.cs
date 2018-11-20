using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis;


namespace ddlc.Generator
{
    public class UnityGen
    {
        private const string t1 = "    ";

        private List<string> nodes_to_using_strings(List<string> fieldTypes, DDLAssembly asm)
        {
            var list = new List<string>();
            foreach (var n in fieldTypes)
            {
                var res = asm.find_decl_by_name(n);
                if (res != null && !string.IsNullOrEmpty(res.NamespaceChain) && !list.Contains(res.NamespaceChain))
                    list.Add(res.NamespaceChain);
            }
            return list;
        }

        public void DoGenerate(
            string outputPath,
            List<NamespaceDecl> namespaceDecls, 
            List<DDLDecl> decls,
            List<MethodDecl> methodDecls,
            DDLAssembly asm
            )
        {
            foreach (var n in namespaceDecls)
            {
                var felds = new List<string>();
                var sb = new StringBuilder();
                Generate(n, "", sb, felds);

                var usings = nodes_to_using_strings(felds, asm);
                var outer = new StringBuilder();
                GenerateHeader(outer, usings);
                outer.Append(sb);
                Console.WriteLine(outer.ToString());
                
                
                var name = n.Name;
                name = name.Replace('.', '_');
                var csfilename = Path.Combine(outputPath, name + "_generated.cs");
                File.WriteAllText(csfilename, outer.ToString());
                Utils.Dos2Unix(csfilename);
            }

            var todo = new List<DDLDecl>();
            foreach (var d in decls)
                if (d.bGenerated == false)
                    todo.Add(d);

            if (todo.Count != 0)
            {
                var fields = new List<string>();
                var sb2 = new StringBuilder();
                foreach (var d in todo)
                    Generate(d, "", sb2, fields);
                
                var usings = nodes_to_using_strings(fields, asm);
                var outer = new StringBuilder();
                GenerateHeader(outer, usings);
                outer.Append(sb2);

                var csfilename = Path.Combine(outputPath, "_generated.cs");
                File.WriteAllText(csfilename, outer.ToString());
                Utils.Dos2Unix(csfilename);
            }

            if (methodDecls.Count != 0)
            {
                var fields = new List<string>();
                var sb3 = new StringBuilder();
                GenerateCommands(methodDecls, "", sb3, fields);
                Console.WriteLine(sb3.ToString());
                
                var usings = nodes_to_using_strings(fields, asm);
                var outer = new StringBuilder();
                GenerateHeader(outer, usings);
                outer.Append(sb3);
                
                var csfilename = Path.Combine(outputPath, "_Commands_generated.cs");
                File.WriteAllText(csfilename, outer.ToString());
                Utils.Dos2Unix(csfilename);
            }
        }

        public void GenerateHeader(StringBuilder sb, List<string> usings)
        {
            sb.AppendLine("//===----------------------------------------------------------------------===//");
            sb.AppendLine("//                                                                              ");
            sb.AppendLine("//  vim: ft=cs tw=80                                                            ");
            sb.AppendLine("//                                                                              ");
            sb.AppendLine("//  DDL Generated code, do not modify directly.                                 ");
            sb.AppendLine("//                                                                              ");
            sb.AppendLine("//===----------------------------------------------------------------------===//");
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            if (usings != null)
            {
                foreach (var u in usings)
                    sb.AppendFormat("using {0};\n", u);
            }
            sb.Append("\n\n");
        }
        
        public void Generate(DDLDecl decl, string tab, StringBuilder sb, List<string> usings)
        {
            var type = decl.GetType();
            if (type == typeof(NamespaceDecl))
                NamespaceGen(decl as NamespaceDecl, tab, sb, usings);
            else if (type == typeof(StructDecl))
                StructGen(decl as StructDecl, tab, sb, usings);
            else if (type == typeof(ClassDecl))
                ClassGen(decl as ClassDecl, tab, sb, usings);
            else if (type == typeof(EnumDecl))
                EnumGen(decl as EnumDecl, tab, sb);
        }

        public void GenerateCommands(List<MethodDecl> decls, string tab, StringBuilder sb, List<string> fields)
        {
            sb.AppendLine(tab + "namespace DDL");
            sb.AppendLine(tab + "{");
            sb.AppendLine(tab + t1 + "public enum ECommands : uint");
            sb.AppendLine(tab + t1 + "{");
            sb.AppendFormat(tab + t1 + t1 + "kUnknown = 0,\n");
            foreach (var f in decls)
            {
                var name = f.PrettyName;
                var item = $"ECommands.{name}";
                sb.AppendFormat(tab + t1 + t1 + "{0} = {1},\n", name, MurmurHash2.Hash(item));
            }
            sb.AppendLine(tab + t1 + "}");
            var base_cmd_str = @"
    public class BaseCommand
    {
        public int TotalSteps = 1;
        public int Step = 0;
        public ECommands Id = ECommands.kUnknown;
        public string Name = null; // TODO(D1): IF DEBUG
    }";
            sb.AppendLine(base_cmd_str);
            sb.AppendLine(tab + "}");
            var dictionary = new Dictionary<string, List<MethodDecl>>();
            var nonamespace = new List<MethodDecl>();
            foreach(var d in decls)
            {
                if (string.IsNullOrEmpty(d.NamespaceChain))
                {
                    nonamespace.Add(d);
                    continue;
                }
                if (!dictionary.ContainsKey(d.NamespaceChain))
                    dictionary[d.NamespaceChain] = new List<MethodDecl>();
                dictionary[d.NamespaceChain].Add(d);
            }
            foreach (var kvp in dictionary)
            {
                sb.AppendFormat("namespace {0}\n", kvp.Key);
                sb.AppendLine("{");
                dump_methods_into_namespace(kvp.Value, tab + t1, sb, fields);
                sb.AppendLine("}");
            }
            dump_methods_into_namespace(nonamespace, tab, sb, fields);
        }

        private static void dump_methods_into_namespace(List<MethodDecl> decls, string tab, StringBuilder sb, List<string> fields)
        {
            foreach (var decl in decls)
            {
                sb.AppendLine(tab + "[Serializable]");
                sb.AppendFormat(tab + "public class {0} : BaseCommand\n", decl.Name);
                sb.AppendLine(tab + "{");
                foreach (var p in decl.Params)
                {
                    sb.AppendFormat(tab + t1 + "public {0} {1};\n", p.sType, p.Name);
                    if (!fields.Contains(p.sType))
                        fields.Add(p.sType);
                }

                sb.AppendFormat(tab + t1 + "public {0}()\n", decl.Name);
                sb.AppendLine(tab + t1 + "{");
                sb.AppendFormat(tab + t1 + t1 + "Id = ECommands.{0};\n", decl.PrettyName);
                sb.AppendFormat(tab + t1 + t1 + "Name = \"ECommands.{0}\";\n", decl.PrettyName);
                if (decl.TotalSteps != 1)
                    sb.AppendFormat(tab + t1 + t1 + "TotalSteps = {0};\n", decl.TotalSteps);
                sb.AppendLine(tab + t1 + "}");

                if (decl.Params.Count > 0)
                {
                    sb.AppendFormat(tab + t1 + "public {0}(", decl.Name);
                    var len = decl.Params.Count;
                    for (var i = 0; i < len - 1; ++i)
                    {
                        var p = decl.Params[i];
                        if (string.IsNullOrEmpty(p.Value))
                            sb.AppendFormat("{0} _{1}, ", p.sType, p.Name);
                        else 
                            sb.AppendFormat("{0} _{1} = {2}, ", p.sType, p.Name, p.Value);
                    }
                    {
                        var p = decl.Params[len - 1];
                        if (string.IsNullOrEmpty(p.Value))
                            sb.AppendFormat("{0} _{1}", p.sType, p.Name);
                        else 
                            sb.AppendFormat("{0} _{1} = {2}", p.sType, p.Name, p.Value);
                    }
                    sb.AppendLine(")");
                    sb.AppendLine(tab + t1 + "{");
                    foreach (var p in decl.Params)
                        sb.AppendFormat(tab + t1 + t1 + "{0} = _{1};\n", p.Name, p.Name);
                    sb.AppendFormat(tab + t1 + t1 + "Id = ECommands.{0};\n", decl.PrettyName);
                    sb.AppendFormat(tab + t1 + t1 + "Name = \"ECommands.{0}\";\n", decl.PrettyName);
                    if (decl.TotalSteps != 1)
                        sb.AppendFormat(tab + t1 + t1 + "TotalSteps = {0};\n", decl.TotalSteps);
                    sb.AppendLine(tab + t1 + "}");
                }

                sb.AppendLine(tab + "}");
            }
        }


        private void NamespaceGen(NamespaceDecl decl, string tab, StringBuilder sb, List<string> usings)
        {
            if (decl.bGenerated) return;
            decl.bGenerated = true;
            sb.AppendFormat(tab + "namespace {0}\n", decl.Name);
            sb.AppendLine(tab + "{");
            foreach (var child in decl.Childs)
            {
                Generate(child, tab + "    ", sb, usings);
            }
            sb.AppendLine(tab + "}");
        }

        private void StructGen(StructDecl decl, string tab, StringBuilder sb, List<string> usings)
        {
            if (decl.bGenerated) return;
            decl.bGenerated = true;
            sb.AppendLine(tab + "[Serializable]");
            sb.AppendFormat(tab + "public struct {0}\n", decl.Name);
            sb.AppendLine(tab + "{");
            foreach (var child in decl.Childs)
            {
                Generate(child, tab + "    ", sb, usings);
            }
            foreach (var mem in decl.Fields)
            {
                AggregateFieldGen(mem, tab + "    ", sb, usings);
            }
            sb.AppendLine(tab + "}");
        }

        private void ClassGen(ClassDecl decl, string tab, StringBuilder sb, List<string> usings)
        {
            if (decl.bGenerated) return;
            decl.bGenerated = true;
            sb.AppendLine(tab + "[Serializable]");
            sb.AppendFormat(tab + "public class {0}\n", decl.Name);
            sb.AppendLine(tab + "{");
            foreach (var child in decl.Childs)
            {
                Generate(child, tab + "    ", sb, usings);
            }
            foreach (var mem in decl.Fields)
            {
                AggregateFieldGen(mem, tab + "    ", sb, usings);
            }
            sb.AppendLine(tab + "}");
        }

        private void EnumGen(EnumDecl decl, string tab, StringBuilder sb)
        {
            if (decl.bGenerated) return;
            decl.bGenerated = true;
            sb.AppendFormat(tab + "public enum {0} : uint\n", decl.Name);
            sb.AppendLine(tab + "{");
            foreach (var f in decl.Fields)
            {
                if (!string.IsNullOrEmpty(f.Value))
                    sb.AppendFormat(tab + "    {0} = {1},\n", f.Name, f.Value);
                else
                    sb.AppendFormat(tab + "    {0} = {1},\n", f.Name, MurmurHash2.Hash(decl.Name + "." + f.Name));
            }
            sb.AppendLine(tab + "}");
        }

        private void AggregateFieldGen(AggregateField field, string tab, StringBuilder sb, List<string> usings)
        {
            string msg = null;
            var type = Converter.DDLTypeToCSharpType(field.Type, field.sType);
            if (field.Type == EType.STRUCT || field.Type == EType.SELECT)
            {
                if (!usings.Contains(field.sType))
                    usings.Add(field.sType);
            }
                
            if (field.ArrayType == EArrayType.SCALAR)
            {
                if (string.IsNullOrEmpty(field.Value))
                    msg = string.Format("public {0} {1};\n", type, field.Name);
                else 
                    msg = string.Format("public {0} {1} = {2};\n", type, field.Name, field.Value);
            }
            if (field.ArrayType == EArrayType.DYNAMIC)
                msg = string.Format("public {0}[] {1};\n", type, field.Name);
            if (field.ArrayType == EArrayType.LIST)
                msg = string.Format("public List<{0}> {1} = new List<{0}>();\n", type, field.Name);
            if (field.ArrayType == EArrayType.FIXED)
                msg = string.Format("public {0}[{2}] {1};\n", type, field.Name);
            
            msg = tab + msg;
            sb.Append(msg);
        }
    }
}