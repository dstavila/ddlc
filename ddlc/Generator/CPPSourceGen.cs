using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ddlc.Generator
{
    public class CPPSourceGen
    {
        private const string t1 = "    ";
        private static List<StructDecl> StructDecls;
        private static List<ClassDecl> ClassDecls;
        
        public static void Generate(StringBuilder sb, 
            List<NamespaceDecl> namespaces, 
            List<DDLDecl> decls, 
            List<ClassDecl> classDecls,
            List<StructDecl> structDecls)
        {
            StructDecls = structDecls;
            ClassDecls = classDecls;
            
            foreach (var n in namespaces)
                Generate(n, "", sb);
            foreach (var d in decls)
                Generate(d, "", sb);
        }
        
        private static int Generate(DDLDecl decl, string tab, StringBuilder sb)
        {
            var type = decl.GetType();
            if (type == typeof(StructDecl))
                return StructGen(decl as StructDecl, tab, sb);
            if (type == typeof(ClassDecl))
                return ClassGen(decl as ClassDecl, tab, sb);
            return 0;
        }
        
        private static int StructGen(StructDecl aggregateDecl, string tab, StringBuilder sb)
        {
            if (aggregateDecl.bSourceGenerated) return 0;
            aggregateDecl.bSourceGenerated = true;
            
            List<AggregateDecl> combined = new List<AggregateDecl>();
            combined.AddRange(StructDecls);
            combined.AddRange(ClassDecls);
            CPPGenJsonSerialization.WriteStructJsonSerialization(tab, aggregateDecl, combined, sb);
            CPPGenJsonDeserialization.WriteStructJsonDeserialization(tab, aggregateDecl, StructDecls, sb);
            return 1;
        }
        
        private static int ClassGen(ClassDecl decl, string tab, StringBuilder sb)
        {
            if (decl.bSourceGenerated) return 0;
            decl.bSourceGenerated = true;
            
            List<AggregateDecl> combined = new List<AggregateDecl>();
            combined.AddRange(StructDecls);
            combined.AddRange(ClassDecls);
            CPPGenJsonSerialization.WriteStructJsonSerialization(tab, decl, combined, sb);
            CPPGenJsonDeserialization.WriteStructJsonDeserialization(tab, decl, ClassDecls, sb);
            return 1;
        }
    }
}