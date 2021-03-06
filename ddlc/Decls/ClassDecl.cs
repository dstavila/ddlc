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
                    var typeDecl = asm.find_decl_by_name(f.sType);
                    if (typeDecl is EnumDecl)
                    {
                        f.Type = EType.SELECT;
                        f.TypeNamespace = typeDecl.NamespaceChain;
                    }
                    else
                        f.Type = EType.STRUCT;
                }
            }
        }
    }
}