using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;


namespace ddlc
{
    public class BitfieldField
    {
        public string Description;
        public string Label;
        public string Name;
        public string Value = null;
        public bool Empty;
    }
    public class BitfieldDefinition
    {
        public string Name;
        public string Description;
        public string Label;
        public List<BitfieldField> Members = new List<BitfieldField>();
    }
    
    
    public static class Bitfield
    {
        public static void ParseValues(EnumDeclarationSyntax node, ref BitfieldDefinition seldef)
        {
            foreach (var member in node.Members)
            {
                bool processed = false;
                BitfieldField field = new BitfieldField();
                field.Name = member.Identifier.Text;
                var equalsValue = member.EqualsValue;
                if (equalsValue != null)
                    field.Value = equalsValue.Value.ToString();
                foreach (var attrList in member.AttributeLists)
                {
                    foreach (var attr in attrList.Attributes)
                    {
                        IdentifierNameSyntax name = (IdentifierNameSyntax)attr.Name;
                        SyntaxToken identifier = name.Identifier;
                        if (identifier.Text == "BitfieldValue")
                        {
                            ParseBitfieldValueAttributeArguments(attr.ArgumentList, ref field);
                            seldef.Members.Add(field);
                            processed = true;
                        }
                    }
                }
                if (!processed)
                    seldef.Members.Add(field);
            }
        }
        public static void ParseAttributes(AttributeArgumentListSyntax args, ref BitfieldDefinition def)
        {
            if (args == null)
                return;
            foreach (var argument in args.Arguments)
            {
                NameEqualsSyntax nameEqSyntax = argument.NameEquals;
                LiteralExpressionSyntax exprSyntax = (LiteralExpressionSyntax)argument.Expression;
                IdentifierNameSyntax argName = (IdentifierNameSyntax)nameEqSyntax.Name;

                if (argName.Identifier.Text == "Description")
                    def.Description = exprSyntax.Token.Text;
                else if (argName.Identifier.Text == "Label")
                    def.Label = exprSyntax.Token.Text;

                Console.Error.WriteLine("");
            }
        }
        private static void ParseBitfieldValueAttributeArguments(AttributeArgumentListSyntax args, ref BitfieldField field)
        {
            foreach (var argument in args.Arguments)
            {
                NameEqualsSyntax nameEqSyntax = argument.NameEquals;
                LiteralExpressionSyntax exprSyntax = (LiteralExpressionSyntax)argument.Expression;
                IdentifierNameSyntax argName = (IdentifierNameSyntax)nameEqSyntax.Name;

                if (argName.Identifier.Text == "Description")
                    field.Description = exprSyntax.Token.Text;
                else if (argName.Identifier.Text == "Label")
                    field.Label = exprSyntax.Token.Text;
                else if (argName.Identifier.Text == "Empty")
                    field.Empty = exprSyntax.Token.Text == "true" ? true : false;
            }
        }
    }
}