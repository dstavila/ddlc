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
    }
}