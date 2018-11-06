using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ddlc
{
    public class AggregateField
    {
        public string Name;
        public uint NameHash;
        public EArrayType ArrayType = EArrayType.SCALAR;
        public EType Type = EType.UNKNOWN;
        public uint Count = 0;
        public string Value;
        public string sType;

        public TypeSyntax TypeSyntax = null;


        public void UnityGen(string tab, StringBuilder sb)
        {
            string msg = null;
            var type = Converter.DDLTypeToCSharpType(Type, sType);
            if (ArrayType == EArrayType.SCALAR)
            {
                if (string.IsNullOrEmpty(Value))
                    msg = string.Format("public {0} {1};\n", type, Name);
                else 
                    msg = string.Format("public {0} {1} = {2};\n", type, Name, Value);
            }
            if (ArrayType == EArrayType.DYNAMIC)
                msg = string.Format("public {0}[] {1};\n", type, Name);
            if (ArrayType == EArrayType.LIST)
                msg = string.Format("public List<{0}> {1} = new List<{0}>();\n", type, Name);
            if (ArrayType == EArrayType.FIXED)
                msg = string.Format("public {0}[{2}] {1};\n", type, Name);
            
            msg = tab + msg;
            sb.Append(msg);
        }
    }
    public abstract class AggregateDecl : DDLDecl
    {
        public List<AggregateField> Fields = new List<AggregateField>();


        public override void ParseParent(List<DDLDecl> decls)
        {
        }

        public override void ParseType(DDLAssembly asm)
        {
            foreach (var f in Fields)
            {
                if (f.TypeSyntax is PredefinedTypeSyntax)
                {
                    f.sType = f.TypeSyntax.ToString();
                    f.Type = Converter.StringToDDLType(f.sType, asm);
                }
                else
                {
                    Console.WriteLine(f.TypeSyntax.ToString());
                }
            }
        }
        
        protected void parse_fields(MemberDeclarationSyntax m)
        {
            if (m is FieldDeclarationSyntax)
            {
                parse_field(m as FieldDeclarationSyntax);
            }
        }

        private void parse_field(FieldDeclarationSyntax field)
        {
            var result = new AggregateField();
            var decl = field.Declaration;
            if (decl.Type is ArrayTypeSyntax)
            {
                parse_array_field(result, field, decl.Type as ArrayTypeSyntax);
            }
            else
            {
                result.TypeSyntax = decl.Type;
                result.ArrayType = EArrayType.SCALAR;
                result.Count = 1;
            }

            if (decl.Variables.Count == 1)
            {
                var variable = decl.Variables[0];
                result.Name = variable.Identifier.Text;
                result.NameHash = MurmurHash2.Hash(result.Name);
                if (variable.Initializer != null)
                {
                    if (variable.Initializer.Value is LiteralExpressionSyntax)
                        result.Value = variable.Initializer.Value.ToString();
                    else
                    {
                        // NOTE(Dmitrii): That looks like hack :)
                        var syntax = (InitializerExpressionSyntax) variable.Initializer.Value;
                        foreach (var t in syntax.Expressions)
                            result.Value = t.ToString();
                    }
                }
            }
            Fields.Add(result);
        }

        private void parse_array_field(AggregateField result, FieldDeclarationSyntax field, ArrayTypeSyntax decl)
        {
            var rank = cleanup_array_rank(decl.RankSpecifiers.ToString());
            if (has_list_attribute(field.AttributeLists))
                result.ArrayType = EArrayType.LIST;
            else
            {
                if (string.IsNullOrEmpty(rank))
                    result.ArrayType = EArrayType.DYNAMIC;
                else
                {
                    result.ArrayType = EArrayType.FIXED;
                    result.Count = uint.Parse(rank);
                }
            }

            result.TypeSyntax = decl.ElementType;
        }


        private string cleanup_array_rank(string rank)
        {
            var r2 = rank.Remove(0, 1);
            r2 = r2.Remove(r2.Length - 1, 1);
            return r2;
        }

        private static bool has_list_attribute(SyntaxList<AttributeListSyntax> list)
        {
            foreach (var attrList in list)
            {
                foreach (var attr in attrList.Attributes)
                {
                    var name = (IdentifierNameSyntax) attr.Name;
                    var id = name.Identifier;
                    if (id.Text == "List")
                        return true;
                }
            }

            return false;
        }
    }
}