using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ddlc
{
    public static class Struct
    {
        public static void ParseStructField(FieldDeclarationSyntax field,
            ref rStructField sfield,
            List<rSelect> selects,
            List<rStruct> structs)
        {
            var decl = field.Declaration;
            if (decl.Type is ArrayTypeSyntax)
            {
                var arrdecl = decl.Type as ArrayTypeSyntax;
                var rank = arrdecl.RankSpecifiers;
                var r2 = rank.ToString();
                r2 = r2.Remove(0, 1);
                r2 = r2.Remove(r2.Length - 1, 1);
                if (IsListAttribute(field.AttributeLists))
                    sfield.ArrayType = EArrayType.LIST;
                else
                {
                    if (string.IsNullOrEmpty(r2))
                        sfield.ArrayType = EArrayType.DYNAMIC;
                    else
                    {
                        sfield.ArrayType = EArrayType.FIXED;
                        sfield.Count = UInt32.Parse(r2);
                    }
                }

                sfield.Type = Converter.StringToDDLType(arrdecl.ElementType.ToString(), selects, structs);
                sfield.TypeName = arrdecl.ElementType.ToString();
            }
            else
            {
                sfield.Type = Converter.StringToDDLType(decl.Type.ToString(), selects, structs);
                sfield.TypeName = decl.Type.ToString();
                sfield.ArrayType = EArrayType.SCALAR;
                sfield.Count = 1;
            }

            if (decl.Variables.Count == 1)
            {
                var variable = decl.Variables[0];
                sfield.Name = variable.Identifier.ToString();
                sfield.NameHash = MurmurHash2.Hash(sfield.Name);
                if (variable.Initializer != null)
                {
                    if (variable.Initializer.Value is LiteralExpressionSyntax)
                    {
                        sfield.Value = variable.Initializer.Value.ToString();
                    }
                    else
                    {
                        var initsyntax = (InitializerExpressionSyntax) variable.Initializer.Value;
                        foreach (var t in initsyntax.Expressions)
                        {
                            sfield.Value = t.ToString();
                        }
                    }
                }
            }
        }
        private static bool IsListAttribute(SyntaxList<AttributeListSyntax> attributeList)
        {
            foreach (var attrList in attributeList)
            {
                foreach (var attr in attrList.Attributes)
                {
                    IdentifierNameSyntax name = (IdentifierNameSyntax) attr.Name;
                    SyntaxToken identifier = name.Identifier;
                    if (identifier.Text == "List")
                        return true;
                }
            }

            return false;
        }
    }
}