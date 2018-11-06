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
        private List<EnumItem> Fields = new List<EnumItem>();
        
        
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

        public override void UnityGen(string tab, StringBuilder sb)
        {
            if (bGenerated) return;
            bGenerated = true;
            sb.AppendFormat(tab + "public enum {0} : uint\n", Name);
            sb.AppendLine(tab + "{");
            foreach (var f in Fields)
            {
                if (!string.IsNullOrEmpty(f.Value))
                    sb.AppendFormat(tab + "    {0} = {1},\n", f.Name, f.Value);
                else
                    sb.AppendFormat(tab + "    {0} = {1},\n", f.Name, MurmurHash2.Hash(Name + "." + f.Name));
            }
            sb.AppendLine(tab + "}");
        }
    }
}