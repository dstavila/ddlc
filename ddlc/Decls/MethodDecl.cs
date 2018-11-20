using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.CodeAnalysis.CSharp.Syntax;


namespace ddlc
{
    public class MethodDecl : DDLDecl
    {
        public List<AggregateField> Params = new List<AggregateField>();
        private MethodDeclarationSyntax Node;
        public int TotalSteps = 1;
        
        public MethodDecl(MethodDeclarationSyntax node, int totalSteps)
        {
            Node = node;
            sNode = Node;
            sParentNode = node.Parent;
            Name = Node.Identifier.Text;
            NamespaceChain = Converter.BuildNamespaceChain(node);
            TotalSteps = totalSteps;
        }

        public string PrettyName
        {
            get
            {
                if (string.IsNullOrEmpty(NamespaceChain))
                    return Name;
                
                var extra = Utils.ExtraCommandNamespace(NamespaceChain);
                if (extra != null)
                    return string.Format("{0}_{1}", extra, Name);
                return Name;
            }
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
                    if (par.Default != null)
                        param.Value = par.Default.Value.ToString();
                    Params.Add(param);
                }
            }
        }
    }
}