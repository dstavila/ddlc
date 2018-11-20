using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Text;


namespace ddlc.Generator
{
    public class UnityGen
    {
        private const string t1 = "    ";


        public void DoGenerate(
            string outputPath,
            List<NamespaceDecl> namespaceDecls, 
            List<DDLDecl> decls,
            List<MethodDecl> methodDecls)
        {
            foreach (var n in namespaceDecls)
            {
                var sb = new StringBuilder();
                GenerateHeader(sb);
                Generate(n, "", sb);

                var name = n.Name;
                name = name.Replace('.', '_');
                var csfilename = Path.Combine(outputPath, name + "_generated.cs");
                File.WriteAllText(csfilename, sb.ToString());
            }

            var todo = new List<DDLDecl>();
            foreach (var d in decls)
                if (d.bGenerated == false)
                    todo.Add(d);

            if (todo.Count != 0)
            {
                var sb2 = new StringBuilder();
                GenerateHeader(sb2);
                foreach (var d in todo)
                    Generate(d, "", sb2);
                var csfilename = Path.Combine(outputPath, "_generated.cs");
                File.WriteAllText(csfilename, sb2.ToString());
            }
        }
        
        
        public void GenerateHeader(StringBuilder sb)
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
            sb.Append("\n\n");
        }
        
        public void Generate(DDLDecl decl, string tab, StringBuilder sb)
        {
            var type = decl.GetType();
            if (type == typeof(NamespaceDecl))
                NamespaceGen(decl as NamespaceDecl, tab, sb);
            else if (type == typeof(StructDecl))
                StructGen(decl as StructDecl, tab, sb);
            else if (type == typeof(ClassDecl))
                ClassGen(decl as ClassDecl, tab, sb);
            else if (type == typeof(EnumDecl))
                EnumGen(decl as EnumDecl, tab, sb);
        }

        public void GenerateCommands(List<MethodDecl> decls, string tab, StringBuilder sb)
        {
            sb.AppendLine(tab + "public enum ECommands : uint");
            sb.AppendLine(tab + "{");
            sb.AppendFormat(tab + t1 + "kUnknown = 0,\n");
            foreach (var f in decls)
            {
                var item = $"ECommands.{f.Name}";
                sb.AppendFormat(tab + t1 + "{0} = {1},\n", f.Name, MurmurHash2.Hash(item));
            }
            sb.AppendLine(tab + "}");
            var base_cmd_str = @"
public class BaseCommand
{
    public int TotalSteps = 1;
    public int Step = 0;
    public ECommands Id = ECommands.kUnknown;
}";
            sb.AppendLine(base_cmd_str);
            foreach (var decl in decls)
            {
                sb.AppendLine(tab + "[Serializable]");
                sb.AppendFormat(tab + "public class {0} : BaseCommand\n", decl.Name);
                sb.AppendLine(tab + "{");
                foreach (var p in decl.Params)
                {
                    sb.AppendFormat(tab + t1 + "public {0} {1};\n", p.sType, p.Name);
                }
                sb.AppendFormat(tab + t1 + "public {0}()\n", decl.Name);
                sb.AppendLine(tab + t1 + "{");
                sb.AppendFormat(tab + t1 + t1 + "id = ECommands.{0};\n", decl.Name);
                sb.AppendFormat(tab + t1 + t1 + "name = \"ECommands.{0}\";\n", decl.Name);
                sb.AppendLine(tab + t1 + "}");
                
                sb.AppendFormat(tab + t1 + "public {0}(", decl.Name);
                foreach (var p in decl.Params)
                    sb.AppendFormat("{0} _{1} ", p.sType, p.Name);
                sb.AppendLine(")");
                sb.AppendLine(tab + t1 + "{");
                foreach (var p in decl.Params)
                    sb.AppendFormat(tab + t1 + t1 + "{0} = _{1};\n", p.Name, p.Name);
                sb.AppendFormat(tab + t1 + t1 + "id = ECommands.{0};\n", decl.Name);
                sb.AppendFormat(tab + t1 + t1 + "name = \"ECommands.{0}\";\n", decl.Name);
                sb.AppendLine(tab + t1 + "}");
                
                sb.AppendLine(tab + "}");
            }
        }


        private void NamespaceGen(NamespaceDecl decl, string tab, StringBuilder sb)
        {
            if (decl.bGenerated) return;
            decl.bGenerated = true;
            sb.AppendFormat(tab + "namespace {0}\n", decl.Name);
            sb.AppendLine(tab + "{");
            foreach (var child in decl.Childs)
            {
                Generate(child, tab + "    ", sb);
            }
            sb.AppendLine(tab + "}");
        }

        private void StructGen(StructDecl decl, string tab, StringBuilder sb)
        {
            if (decl.bGenerated) return;
            decl.bGenerated = true;
            sb.AppendLine(tab + "[Serializable]");
            sb.AppendFormat(tab + "public struct {0}\n", decl.Name);
            sb.AppendLine(tab + "{");
            foreach (var child in decl.Childs)
            {
                Generate(child, tab + "    ", sb);
            }
            foreach (var mem in decl.Fields)
            {
                AggregateFieldGen(mem, tab + "    ", sb);
            }
            sb.AppendLine(tab + "}");
        }

        private void ClassGen(ClassDecl decl, string tab, StringBuilder sb)
        {
            if (decl.bGenerated) return;
            decl.bGenerated = true;
            sb.AppendLine(tab + "[Serializable]");
            sb.AppendFormat(tab + "public class {0}\n", decl.Name);
            sb.AppendLine(tab + "{");
            foreach (var child in decl.Childs)
            {
                Generate(child, tab + "    ", sb);
            }
            foreach (var mem in decl.Fields)
            {
                AggregateFieldGen(mem, tab + "    ", sb);
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

        private void AggregateFieldGen(AggregateField field, string tab, StringBuilder sb)
        {
            string msg = null;
            var type = Converter.DDLTypeToCSharpType(field.Type, field.sType);
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