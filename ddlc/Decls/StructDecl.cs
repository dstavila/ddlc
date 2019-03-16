using System;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;


namespace ddlc
{
    public class StructDecl : AggregateDecl
    {
        private StructDeclarationSyntax Node;
        
        public StructDecl(StructDeclarationSyntax node)
        {
            Node = node;
            sParentNode = node.Parent;
            sNode = Node;
            NamespaceChain = Converter.BuildNamespaceChain(node);
        }
        
        public override void ParseDecl()
        {
            Name = Node.Identifier.Text;
            foreach (var m in Node.Members)
                parse_fields(m);
        }

        public override void ParseType(DDLAssembly asm)
        {
            foreach (var f in Fields)
            {
                if (f.TypeSyntax is PredefinedTypeSyntax)
                {
                    var ff = f.TypeSyntax as PredefinedTypeSyntax;
                    f.sType = ff.Keyword.ToString();
                    f.Type = Converter.StringToDDLType(f.sType, asm);
                }
                else if (f.TypeSyntax is IdentifierNameSyntax)
                {
                    var ff = f.TypeSyntax as IdentifierNameSyntax;
                    f.sType = ff.Identifier.ToString();
                    f.Type = EType.STRUCT;
                }
            }
        }
    } }