using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;


namespace ddlc
{
    public class DDLSyntaxWalker : CSharpSyntaxWalker
    {
        private DDLAssembly _assembly;
        private string _sourceFile;


        public DDLSyntaxWalker(Compilation compilation, string sourceFile, DDLAssembly assembly) : base(SyntaxWalkerDepth.StructuredTrivia)
        {
            _assembly = assembly;
            _sourceFile = sourceFile;
        }

        public override void VisitEnumDeclaration(EnumDeclarationSyntax node)
        {
            base.VisitEnumDeclaration(node);
            foreach (var attrList in node.AttributeLists)
            {
                foreach (var attr in attrList.Attributes)
                {
                    var name = (IdentifierNameSyntax) attr.Name;
                    var id = name.Identifier;
                    if (id.Text == "Select")
                    {
                        var decl = new EnumDecl(node);
                        decl.SourceFilepath = _sourceFile;
                        _assembly.AppendEnum(decl);
                    }
                }
            }
        }
        
        public override void VisitStructDeclaration(StructDeclarationSyntax node)
        {
            base.VisitStructDeclaration(node);
            var decl = new StructDecl(node);
            decl.SourceFilepath = _sourceFile;
            _assembly.AppendStruct(decl);
        }
        
        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            base.VisitClassDeclaration(node);
            var decl = new ClassDecl(node);
            decl.SourceFilepath = _sourceFile;
            _assembly.AppendClass(decl);
        }
        
        public override void VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
        {
            base.VisitNamespaceDeclaration(node);
            var decl = new NamespaceDecl(node);
            decl.SourceFilepath = _sourceFile;
            _assembly.AppendNamespace(decl);
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            base.VisitMethodDeclaration(node);
            foreach (var attrList in node.AttributeLists)
            {
                foreach (var attr in attrList.Attributes)
                {
                    var name = (IdentifierNameSyntax)attr.Name;
                    var id = name.Identifier;
                    if (id.Text == "Command")
                    {
                        var decl = new MethodDecl(node, 1);
                        decl.SourceFilepath = _sourceFile;
                        _assembly.AppendMethod(decl);
                    }
                    else if (id.Text == "CommandAttribute")
                    {
                        var steps = 0;
                        foreach (var arg in attr.ArgumentList.Arguments)
                        {
                            if (arg.NameEquals.Name.ToString() == "Steps")
                            {
                                int.TryParse(arg.Expression.ToString(), out steps);
                            }
                        }
                        var decl = new MethodDecl(node, steps);
                        decl.SourceFilepath = _sourceFile;
                        _assembly.AppendMethod(decl);
                    }
                }
            }
        }
    }
}