using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ddlc.Generator
{
    public class QtSourceGen
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
            
            var tsb = new TabbedStringBuilder(sb);
            foreach (var n in namespaces)
                Generate(n, tsb);
            foreach (var d in decls)
                Generate(d, tsb);
        }
        
        private static int Generate(DDLDecl decl, TabbedStringBuilder sb)
        {
            var type = decl.GetType();
            if (type == typeof(StructDecl))
                return StructGen(decl as StructDecl, sb);
            if (type == typeof(ClassDecl))
                return ClassGen(decl as ClassDecl, sb);
            return 0;
        }
        
        private static int StructGen(StructDecl aggregateDecl, TabbedStringBuilder sb)
        {
            if (aggregateDecl.bSourceGenerated) return 0;
            aggregateDecl.bSourceGenerated = true;
            
            List<AggregateDecl> combined = new List<AggregateDecl>();
            combined.AddRange(StructDecls);
            combined.AddRange(ClassDecls);
            QtGenJsonSerialization.WriteStructJsonSerialization(aggregateDecl, combined, sb);
            QtGenJsonDeserialization.WriteStructJsonDeserialization(aggregateDecl, StructDecls, sb);
            return 1;
        }
        
        private static int ClassGen(ClassDecl decl, TabbedStringBuilder sb)
        {
            if (decl.bSourceGenerated) return 0;
            decl.bSourceGenerated = true;
            
            List<AggregateDecl> combined = new List<AggregateDecl>();
            combined.AddRange(StructDecls);
            combined.AddRange(ClassDecls);
            QtGenJsonSerialization.WriteStructJsonSerialization(decl, combined, sb);
            QtGenJsonDeserialization.WriteStructJsonDeserialization(decl, ClassDecls, sb);
            return 1;
        }
    }
}