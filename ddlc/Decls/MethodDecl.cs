using Microsoft.CodeAnalysis.CSharp.Syntax;


namespace ddlc
{
    public class MethodDecl : DDLDecl
    {
        private MethodDeclarationSyntax Node;
        
        public MethodDecl(MethodDeclarationSyntax node)
        {
            Node = node;
            sNode = Node;
        }
        public override void ParseDecl()
        {
        }
    }
}