using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;


namespace ddlc
{
    public class MethodDecl : DDLDecl
    {
        public List<AggregateField> Params = new List<AggregateField>();
        private MethodDeclarationSyntax Node;
        
        public MethodDecl(MethodDeclarationSyntax node)
        {
            Node = node;
            sNode = Node;
            sParentNode = node.Parent;
            Name = Node.Identifier.Text;
            NamespaceChain = Converter.BuildNamespaceChain(node);
        }

        public override void ParseDecl() { }
        public override void ParseType(DDLAssembly asm)
        {
            if (Node.ParameterList != null)
            {
                foreach (var par in Node.ParameterList.Parameters)
                {
                    var param = new AggregateField();
                    param.Name = par.Identifier.Text;
                    param.sType = par.Type.ToString();
                    param.Type = Converter.StringToDDLType(param.sType, asm);
                    Params.Add(param);
                }
            }
        }
    }
}