using System.Collections.Generic;
using System.Text;

namespace ddlc.Generator
{
    public static class CPPFwdDeclGen
    {
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
            else if (type == typeof(ClassDecl))
                return ClassGen(decl as ClassDecl, tab, sb);
            else if (type == typeof(EnumDecl))
                return EnumGen(decl as EnumDecl, tab, sb);
            return 0;
        }
        
        private static int NamespaceGen(NamespaceDecl decl, string tab, StringBuilder insb)
        {
            if (decl.bFwdDeclared) return 0;
            decl.bFwdDeclared = true;
            
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
            if (aggregateDecl.bFwdDeclared) return 0;
            aggregateDecl.bFwdDeclared = true;
            sb.AppendFormat(tab + "struct {0};\n", aggregateDecl.Name);
            // We're not allowing fwddecl for inner classes
            foreach (var child in aggregateDecl.Childs)
                child.bFwdDeclared = true;
            return 1;
        }
        
        private static int ClassGen(ClassDecl decl, string tab, StringBuilder sb)
        {
            if (decl.bFwdDeclared) return 0;
            decl.bFwdDeclared = true;
            sb.AppendFormat(tab + "struct {0};\n", decl.Name);
            // We're not allowing fwddecl for inner classes
            foreach (var child in decl.Childs)
                child.bFwdDeclared = true;
            return 1;
        }
        
        private static int EnumGen(EnumDecl decl, string tab, StringBuilder sb)
        {
            if (decl.bFwdDeclared) return 0;
            decl.bFwdDeclared = true;
            sb.AppendFormat(tab + "enum class {0} : uint32_t;\n", decl.Name);
            return 1;
        }
    }
}