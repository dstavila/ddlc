using System.Text;


namespace ddlc.Generator
{
    public class UnityGen : IGenerator
    {
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