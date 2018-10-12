using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;


namespace ddlc
{
    public class ClassMethod
    {
        public class Arg
        {
            public string Type;
            public string Name;
            public bool Array = false;
        }
        public string Name;
        public string Result;
        public List<Arg> Args = new List<Arg>();
    }
    public class ClassField
    {
        public string Name;
        public string Value;
        public string Type;
        public string Description;
        public bool Array = false;
        public int ArraySize = 0;
    }

    public class ClassDefinition
    {
        public string Name;
        public List<ClassField> Members = new List<ClassField>();
        public List<string> Childs = new List<string>();
        public List<ClassMethod> Methods = new List<ClassMethod>();
        public bool generated = false;
    }


    public static class Class
    {
        public static void ParseClassField(FieldDeclarationSyntax field, ref ClassField cls)
        {
            var decl = field.Declaration;
            if (decl.Type is ArrayTypeSyntax)
            {
                var arrdecl = decl.Type as ArrayTypeSyntax;
                cls.Type = arrdecl.ElementType.ToString();
                cls.Array = true;
            }
            else
            {
                cls.Type = decl.Type.ToString();
            }
            if (decl.Variables.Count == 1)
            {
                var variable = decl.Variables[0];
                cls.Name = variable.Identifier.ToString();
                if (variable.Initializer != null)
                    cls.Value = variable.Initializer.Value.ToString();
            }
            Console.Error.WriteLine("");
        }

        public static void ParseMethod(MethodDeclarationSyntax method, ref ClassMethod mth)
        {
            mth.Name = method.Identifier.ToString();
            mth.Result = method.ReturnType.ToString();
            foreach (var p in method.ParameterList.Parameters)
            {
                var arg = new ClassMethod.Arg();
                arg.Name = p.Identifier.ToString();
                if (p.Type is ArrayTypeSyntax)
                {
                    var arr = (ArrayTypeSyntax) p.Type;
                    arg.Type = arr.ElementType.ToString();
                    arg.Array = true;
                }
                else 
                    arg.Type = p.Type.ToString();
                mth.Args.Add(arg);
            }
            Console.Error.WriteLine("");
        }
    }
}