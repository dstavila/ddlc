using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;


namespace ddlc
{
    public class EnumDecl : DDLDecl
    {
        public class EnumItem
        {
            public string Name;
            public string Value;
        }
        
        private EnumDeclarationSyntax Node;
        public List<EnumItem> Fields = new List<EnumItem>();
        
        
        public EnumDecl(EnumDeclarationSyntax node)
        {
            Node = node;
            sNode = Node;
            sParentNode = node.Parent;
        }
        public override void ParseDecl()
        {
            Name = Node.Identifier.Text.ToString();
            foreach (var mem in Node.Members)
            {
                var field = new EnumItem();
                field.Name = mem.Identifier.Text;
                if (mem.EqualsValue != null)
                    field.Value = mem.EqualsValue.ToString();
                Fields.Add(field);
            }
        }
    }
}