using System;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;

    

namespace ddlc
{
    public class NamespaceDecl : DDLDecl
    {
        private NamespaceDeclarationSyntax Node;

        public NamespaceDecl(NamespaceDeclarationSyntax node)
        {
            Node = node;
            sParentNode = node.Parent;
            sNode = Node;
        }

        public override void ParseDecl()
        {
            Name = Node.Name.ToString();
        }
    }
}