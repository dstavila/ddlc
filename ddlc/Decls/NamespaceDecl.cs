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

        public override void UnityGen(string tab, StringBuilder sb)
        {
            if (bGenerated) return;
            bGenerated = true;
            sb.AppendFormat(tab + "namespace {0}\n", Name);
            sb.AppendLine(tab + "{");
            foreach (var child in Childs)
            {
                child.UnityGen(tab + "    ", sb);
            }
            sb.AppendLine(tab + "}");
        }
    }
}