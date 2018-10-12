using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;


namespace ddlc
{
    public class SelectField
    {
        public string Description;
        public string Label;
        public string Name;
        public string Value = null;
        public bool Default;
    }

    public class SelectDefinition
    {
        public bool generated = false;
        public string Description;
        public string Label;
        public string Name;
        public int DefaultItem = -1;
        public List<SelectField> Members = new List<SelectField>();
    }


    public static class Select
    {
        public static void ParseAttributes(AttributeArgumentListSyntax args, ref rSelect seldef)
        {
            if (args == null)
                return;
            foreach (var argument in args.Arguments)
            {
                var nameEqSyntax = argument.NameEquals;
                var exprSyntax = (LiteralExpressionSyntax) argument.Expression;
                var argName = (IdentifierNameSyntax) nameEqSyntax.Name;

                if (argName.Identifier.Text == "Description")
                    seldef.Description = exprSyntax.Token.Text;
                else if (argName.Identifier.Text == "Label")
                    seldef.Label = exprSyntax.Token.Text;
            }
        }

        public static void ParseEnum(EnumDeclarationSyntax node, ref rSelect seldef)
        {
            foreach (var member in node.Members)
            {
                var field = new rSelectItem();
                field.Name = member.Identifier.Text;
                field.NameHash = MurmurHash2.Hash(field.Name);
                var equalsValue = member.EqualsValue;
                if (equalsValue != null)
                    field.Value = equalsValue.Value.ToString();
                foreach (var attrList in member.AttributeLists)
                {
                    foreach (var attr in attrList.Attributes)
                    {
                        IdentifierNameSyntax name = (IdentifierNameSyntax) attr.Name;
                        SyntaxToken identifier = name.Identifier;
                        if (identifier.Text == "SelectValue")
                        {
                            ParseSelectValueAttributeArguments(attr.ArgumentList, ref field);
                        }
                    }
                }

                seldef.Items.Add(field);
            }
        }

        private static void ParseSelectValueAttributeArguments(AttributeArgumentListSyntax args, ref rSelectItem selfld)
        {
            foreach (var argument in args.Arguments)
            {
                NameEqualsSyntax nameEqSyntax = argument.NameEquals;
                LiteralExpressionSyntax exprSyntax = (LiteralExpressionSyntax) argument.Expression;
                IdentifierNameSyntax argName = (IdentifierNameSyntax) nameEqSyntax.Name;

                if (argName.Identifier.Text == "Description")
                    selfld.Description = exprSyntax.Token.Text;
                else if (argName.Identifier.Text == "Label")
                    selfld.Label = exprSyntax.Token.Text;
//                else if (argName.Identifier.Text == "Default")
//                    selfld.Default = exprSyntax.Token.Text == "true" ? true : false;
            }
        }
    }
}