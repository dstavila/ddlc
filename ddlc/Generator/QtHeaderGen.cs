using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ddlc.Generator
{
    public static class QtHeaderGen
    {
        private const string t1 = "    ";
        
        
        public static void Generate(StringBuilder sb, List<NamespaceDecl> namespaces, List<DDLDecl> decls)
        {
            foreach (var n in namespaces)
                Generate(n, "", sb);
            foreach (var d in decls)
                Generate(d, "", sb);
        }
        
        private static int Generate(DDLDecl decl, string tab, StringBuilder sb)
        {
            var type = decl.GetType();
            if (type == typeof(NamespaceDecl))
                return NamespaceGen(decl as NamespaceDecl, tab, sb);
            if (type == typeof(StructDecl))
                return StructGen(decl as StructDecl, tab, sb);
            if (type == typeof(ClassDecl))
                return ClassGen(decl as ClassDecl, tab, sb);
            if (type == typeof(EnumDecl))
                return EnumGen(decl as EnumDecl, tab, sb);
            return 0;
        }
        
        private static int NamespaceGen(NamespaceDecl decl, string tab, StringBuilder insb)
        {
            if (decl.bHeaderGenerated) return 0;
            decl.bHeaderGenerated = true;
            
            var sb = new StringBuilder();
            var nestCount = 0;
            sb.AppendFormat(tab + "namespace {0}\n", Utils.ToCPPNamespace(decl.Name, ref nestCount));
            sb.AppendLine(tab + "{");
            
            var genCount = 0;
            foreach (var child in decl.Childs)
                genCount = Generate(child, tab + "    ", sb);
            
            for (var i = 0; i < nestCount; ++i)
                sb.Append(tab + "};");
            sb.AppendLine();
            
            if (genCount != 0)
                insb.Append(sb);
            return genCount;
        }
        
        private static int StructGen(StructDecl aggregateDecl, string tab, StringBuilder sb)
        {
            if (aggregateDecl.bHeaderGenerated) return 0;
            aggregateDecl.bHeaderGenerated = true;
            sb.AppendFormat(tab + "struct {0}\n", aggregateDecl.Name);
            sb.AppendLine(tab + "{");
            
            var genCount = 0;
            foreach (var child in aggregateDecl.Childs)
                genCount = Generate(child, tab + t1, sb);

            foreach (var mem in aggregateDecl.Fields)
                AggregateFieldGen(mem, tab + t1, sb);

            sb.AppendLine();
            sb.AppendFormat(tab + t1 + "bool FromJson(const QByteArray& byteArray);\n");
            sb.AppendFormat(tab + t1 + "void FromJsonObject(const QJsonObject& jsObject);\n");
            sb.AppendFormat(tab + t1 + "QJsonObject ToJsonObject() const;\n");
            sb.AppendFormat(tab + t1 + "QByteArray ToJson() const;\n");
            sb.AppendLine(tab + "};");
            return genCount + 1;
        }
        
        private static int ClassGen(ClassDecl decl, string tab, StringBuilder sb)
        {
            if (decl.bHeaderGenerated) return 0;
            decl.bHeaderGenerated = true;
            sb.AppendFormat(tab + "struct {0}\n", decl.Name);
            sb.AppendLine(tab + "{");
            
            var genCount = 0;
            foreach (var child in decl.Childs)
                genCount = Generate(child, tab + t1, sb);

            foreach (var mem in decl.Fields)
                AggregateFieldGen(mem, tab + t1, sb);

            sb.AppendLine();
            sb.AppendFormat(tab + t1 + "bool FromJson(const QByteArray& byteArray);\n");
            sb.AppendFormat(tab + t1 + "void FromJsonObject(const QJsonObject& jsObject);\n");
            sb.AppendFormat(tab + t1 + "QJsonObject ToJsonObject() const;\n");
            sb.AppendFormat(tab + t1 + "QByteArray ToJson() const;\n");
            sb.AppendLine(tab + "};");
            return genCount + 1;
        }
        
        private static int EnumGen(EnumDecl decl, string tab, StringBuilder sb)
        {
            if (decl.bHeaderGenerated) return 0;
            decl.bHeaderGenerated = true;
            sb.AppendFormat(tab + "enum class {0} : uint32_t\n", decl.Name);
            sb.AppendLine(tab + "{");
            foreach (var f in decl.Fields)
            {
                if (!string.IsNullOrEmpty(f.Value))
                    sb.AppendFormat(tab + t1 + "{0} = {1},\n", f.Name, f.Value);
                else
                    sb.AppendFormat(tab + t1 + "{0} = {1},\n", f.Name, MurmurHash2.Hash(decl.Name + "." + f.Name));
            }
            sb.AppendLine(tab + "};");
            return 1;
        }
        
        private static void AggregateFieldGen(AggregateField field, string tab, StringBuilder sb)
        {
            string msg = null;
            var type = Converter.DDLTypeToQtType(field.Type, field.sType);
            if (field.Type == EType.SELECT)
                type = Utils.TypeWithNamespace(field);
            
            if (field.ArrayType == EArrayType.SCALAR)
                msg = string.Format("{0} {1};\n", type, field.Name);
            if (field.ArrayType == EArrayType.DYNAMIC || field.ArrayType == EArrayType.LIST)
                msg = string.Format("QList<{0}> {1};\n", type, field.Name);
            if (field.ArrayType == EArrayType.FIXED)
                msg = string.Format("{0} {1}[{2}];\n", type, field.Name, field.Count);
            if (string.IsNullOrEmpty(msg))
                msg = "ERROR";
            msg = tab + msg;
            sb.Append(msg);
        }
    }
}