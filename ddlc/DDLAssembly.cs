using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;


namespace ddlc
{
    public abstract class DDLDecl
    {
        public string SourceFilepath;

        public SyntaxNode sNode;
        public SyntaxNode sParentNode;
        public DDLDecl Parent;
        public string Name;
        public bool bGenerated = false;
        public bool bHeaderGenerated = false;
        public bool bSourceGenerated = false;
        public bool bFwdDeclared = false;
        public List<DDLDecl> Childs = new List<DDLDecl>();
        
        public string NamespaceChain = null;

        public abstract void ParseDecl();

        public virtual void ParseParent(List<DDLDecl> decls) { }
        public virtual void ParsePackage() { }
        public virtual void ParseType(DDLAssembly asm) { }
    }



    
    
    
    
    public class DDLAssembly
    {
        public List<DDLDecl> Decls = new List<DDLDecl>();
        private readonly List<NamespaceDecl> NamespaceDecls = new List<NamespaceDecl>();
        public List<ClassDecl> ClassDecls = new List<ClassDecl>();
        public List<StructDecl> StructDecls = new List<StructDecl>();
        public List<EnumDecl> EnumDecls = new List<EnumDecl>();
        public List<MethodDecl> MethodDecls = new List<MethodDecl>();
        
        

        public void AppendNamespace(NamespaceDecl decl)
        {
            NamespaceDecls.Add(decl);
            Decls.Add(decl);
        }
        public void AppendClass(ClassDecl decl)
        {
            ClassDecls.Add(decl);
            Decls.Add(decl);
        }
        public void AppendStruct(StructDecl aggregateDecl)
        {
            StructDecls.Add(aggregateDecl);
            Decls.Add(aggregateDecl);
        }
        public void AppendEnum(EnumDecl decl)
        {
            EnumDecls.Add(decl);
            Decls.Add(decl);
        }
        public void AppendMethod(MethodDecl decl)
        {
            MethodDecls.Add(decl);
            Decls.Add(decl);
        }
        
        

        public void Resolve()
        {
            foreach (var d in Decls)
                d.ParseDecl();
            resolve_parents();
//            foreach (var d in Decls)
//                d.ParseParent(Decls);
//            foreach (var d in Decls)
//                d.ParsePackage();
            foreach (var d in Decls)
                d.ParseType(this);
        }
        
        public void Generate(GeneratorContext ctx)
        {
            if (string.IsNullOrEmpty(ctx.language) || ctx.language == "cs")
            {
                foreach (var d in Decls)
                    d.bGenerated = false;
                var unityGen = new Generator.UnityGen();
                unityGen.DoGenerate(ctx.OutputPath, NamespaceDecls, Decls, MethodDecls, ctx.Assembly);
            }

            if (string.IsNullOrEmpty(ctx.language) || ctx.language == "cpp")
            {
                foreach (var d in Decls)
                    d.bGenerated = false;
                var cppGen = new Generator.CPPGen(ClassDecls, StructDecls);
                cppGen.DoGenerate(ctx, NamespaceDecls, Decls);
            }
        }
        

        private void resolve_parents()
        {
            foreach (var d in Decls)
            {
                if (d.sParentNode == null) 
                    continue;
                var child = find_decl_by_syntax_node(d.sNode);
                var parent = find_decl_by_syntax_node(d.sParentNode);
                if (parent == null)
                    continue;
                child.Parent = parent;
                parent.Childs.Add(child);
            }
        }

        public DDLDecl find_decl_by_name(string name)
        {
            foreach (var d in Decls)
            {
                if (d.Name == name)
                    return d;
            }
            return null;
        }

        public DDLDecl find_decl_by_syntax_node(SyntaxNode node)
        {
            string nodeName = null;
            var kind = node.Kind();
            if (kind == SyntaxKind.StructDeclaration)
            {
                var rr = node as StructDeclarationSyntax;
                nodeName = rr.Identifier.ToString();
                foreach (var d in Decls)
                {
                    if (d.sNode.Kind() == SyntaxKind.StructDeclaration)
                    {
                        var ds = d.sNode as StructDeclarationSyntax;
                        if (nodeName == ds.Identifier.ToString())
                            return d;
                    }
                    if (d.sNode.Kind() == SyntaxKind.ClassDeclaration)
                    {
                        var ds = d.sNode as ClassDeclarationSyntax;
                        if (nodeName == ds.Identifier.ToString())
                            return d;
                    }
                }
            }
            if (kind == SyntaxKind.ClassDeclaration)
            {
                var rr = node as ClassDeclarationSyntax;
                nodeName = rr.Identifier.ToString();
                foreach (var d in Decls)
                {
                    if (d.sNode.Kind() == SyntaxKind.StructDeclaration)
                    {
                        var ds = d.sNode as StructDeclarationSyntax;
                        if (nodeName == ds.Identifier.ToString())
                            return d;
                    }
                    if (d.sNode.Kind() == SyntaxKind.ClassDeclaration)
                    {
                        var ds = d.sNode as ClassDeclarationSyntax;
                        if (nodeName == ds.Identifier.ToString())
                            return d;
                    }
                }
            }
            if (kind == SyntaxKind.NamespaceDeclaration)
            {
                var rr = node as NamespaceDeclarationSyntax;
                nodeName = rr.Name.ToString();
                foreach (var d in Decls)
                {
                    if (d.sNode.Kind() == SyntaxKind.NamespaceDeclaration)
                    {
                        var ds = d.sNode as NamespaceDeclarationSyntax;
                        if (nodeName == ds.Name.ToString())
                            return d;
                    }
                }
            }
            if (kind == SyntaxKind.EnumDeclaration)
            {
                var rr = node as EnumDeclarationSyntax;
                nodeName = rr.Identifier.ToString();
                foreach (var d in Decls)
                {
                    if (d.sNode.Kind() == SyntaxKind.EnumDeclaration)
                    {
                        var ds = d.sNode as EnumDeclarationSyntax;
                        if (nodeName == ds.Identifier.ToString())
                            return d;
                    }
                }
            }
            if (kind == SyntaxKind.MethodDeclaration)
            {
                var rr = node as MethodDeclarationSyntax;
                nodeName = rr.Identifier.ToString();
                foreach (var d in Decls)
                {
                    if (d.sNode.Kind() == SyntaxKind.MethodDeclaration)
                    {
                        var ds = d.sNode as MethodDeclarationSyntax;
                        if (nodeName == ds.Identifier.ToString())
                            return d;
                    }
                }
            }
            return null;
        }
    }
}