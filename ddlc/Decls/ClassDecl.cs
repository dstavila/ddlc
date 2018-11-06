using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;


namespace ddlc
{
    public class ClassDecl : AggregateDecl
    {
        private ClassDeclarationSyntax Node;
        
        public ClassDecl(ClassDeclarationSyntax node)
        {
            Node = node;
            sParentNode = node.Parent;
            sNode = Node;
        }
        public override void ParseDecl()
        {
            Name = Node.Identifier.Text;
            foreach (var m in Node.Members)
                parse_fields(m);
        }
        
        public override void UnityGen(string tab, StringBuilder sb)
        {
            if (bGenerated) return;
            bGenerated = true;
            sb.AppendLine(tab + "[Serializable]");
            sb.AppendFormat(tab + "public class {0}\n", Name);
            sb.AppendLine(tab + "{");
            foreach (var child in Childs)
            {
                child.UnityGen(tab + "    ", sb);
            }
            sb.AppendLine(tab + "}");
        }
    }
}