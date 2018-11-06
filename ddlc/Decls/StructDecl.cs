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
                    f.Type = EType.SELECT;
                }
            }
        }
        
        public override void UnityGen(string tab, StringBuilder sb)
        {
            if (bGenerated) return;
            bGenerated = true;
            sb.AppendLine(tab + "[Serializable]");
            sb.AppendFormat(tab + "public struct {0}\n", Name);
            sb.AppendLine(tab + "{");
            foreach (var child in Childs)
            {
                child.UnityGen(tab + "    ", sb);
            }
            foreach (var mem in Fields)
            {
                mem.UnityGen(tab + "    ", sb);
            }
            sb.AppendLine(tab + "}");
        }
    }
}