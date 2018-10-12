using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;


namespace ddlc
{
    public class DDLSyntaxWalker : CSharpSyntaxWalker
    {
        public List<rNamespace> Namespaces = new List<rNamespace>();
        public List<rSelect> Selects = new List<rSelect>();
        public List<rStruct> Structs = new List<rStruct>();


        public DDLSyntaxWalker(Compilation compilation) : base(SyntaxWalkerDepth.StructuredTrivia)
        {
        }

        public override void VisitEnumDeclaration(EnumDeclarationSyntax node)
        {
            base.VisitEnumDeclaration(node);
            foreach (var attrList in node.AttributeLists)
            {
                foreach (var attr in attrList.Attributes)
                {
                    IdentifierNameSyntax name = (IdentifierNameSyntax) attr.Name;
                    SyntaxToken identifier = name.Identifier;
                    if (identifier.Text == "Select")
                    {
                        var seldef = new rSelect();
                        seldef.Name = node.Identifier.Text;
                        seldef.NameHash = MurmurHash2.Hash(seldef.Name);
                        Select.ParseAttributes(attr.ArgumentList, ref seldef);
                        Select.ParseEnum(node, ref seldef);
                        Selects.Add(seldef);
                    }
                }
            }
        }


        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            var strDef = new rStruct();
            strDef.Name = node.Identifier.Text;
            
            List<string> namespaceChain = new List<string>();
            BuildFullNamespaceChain(node.Parent, namespaceChain);
            strDef.Namespace = namespaceChain;
            
            foreach (var m in node.Members)
                ProcessStructOrClass(strDef, m);
            Structs.Add(strDef);
            base.VisitClassDeclaration(node);
        }

        public override void VisitStructDeclaration(StructDeclarationSyntax node)
        {
            var strDef = new rStruct();
            strDef.Name = node.Identifier.Text;
            
            List<string> namespaceChain = new List<string>();
            BuildFullNamespaceChain(node.Parent, namespaceChain);
            strDef.Namespace = namespaceChain;
            
            foreach (var m in node.Members)
                ProcessStructOrClass(strDef, m);
            Structs.Add(strDef);
            base.VisitStructDeclaration(node);
        }

        private void ProcessStructOrClass(rStruct strDef, MemberDeclarationSyntax member)
        {
            if (member is FieldDeclarationSyntax)
            {
                var field = new rStructField();
                Struct.ParseStructField((FieldDeclarationSyntax)member, ref field, Selects, Structs);
                strDef.Fields.Add(field);
            }
            else if (member is ClassDeclarationSyntax)
            {
                var node = member as ClassDeclarationSyntax;
                var newstrDef = new rStruct();
                newstrDef.Name = node.Identifier.Text;

                List<string> namespaceChain = new List<string>();
                BuildFullNamespaceChain(node.Parent, namespaceChain);
                newstrDef.Namespace = namespaceChain;

                foreach (var m in node.Members)
                    ProcessStructOrClass(newstrDef, m);
                Structs.Add(newstrDef);
                strDef.Childs.Add(newstrDef);
            }
            else if (member is StructDeclarationSyntax)
            {
                var node = member as StructDeclarationSyntax;
                var newstrDef = new rStruct();
                newstrDef.Name = node.Identifier.Text;

                List<string> namespaceChain = new List<string>();
                BuildFullNamespaceChain(node.Parent, namespaceChain);
                newstrDef.Namespace = namespaceChain;

                foreach (var m in node.Members)
                    ProcessStructOrClass(newstrDef, m);
                Structs.Add(newstrDef);
                strDef.Childs.Add(newstrDef);
            }
        }
        
        private void BuildFullNamespaceChain(SyntaxNode node, List<string> list)
        {
            if (node is NamespaceDeclarationSyntax)
            {
                var ns = node as NamespaceDeclarationSyntax;
                list.Add(ns.Name.ToString());
                BuildFullNamespaceChain(node.Parent, list);
            }
            if (node is ClassDeclarationSyntax)
            {
                var cls = node as ClassDeclarationSyntax;
                list.Add(cls.Identifier.Text);
                BuildFullNamespaceChain(node.Parent, list);
            }
        }
        
        
        
        

        public override void VisitNamespaceDeclaration(NamespaceDeclarationSyntax node)
        {
            base.VisitNamespaceDeclaration(node);
            var namedef = new rNamespace();
            namedef.Name = node.Name.ToString();
            for (int i = 0; i < node.Members.Count; ++i)
            {
                var memb = node.Members[i];
                if (memb is EnumDeclarationSyntax)
                {
                    var m = memb as EnumDeclarationSyntax;
                    if (IsSelectAttribute(m.AttributeLists))
                    {
                        namedef.Selects.Add(m.Identifier.ToString());
                    }
//                    else if (IsBitfieldAttribute(m.AttributeLists))
//                    {
//                        namedef.Bitfields.Add(m.Identifier.ToString());
//                    }
                }
                else if (memb is ClassDeclarationSyntax)
                {
                    var m = memb as ClassDeclarationSyntax;
                    namedef.Structs.Add(m.Identifier.ToString());
                }
                else if (memb is StructDeclarationSyntax)
                {
                    var m = memb as StructDeclarationSyntax;
                    namedef.Structs.Add(m.Identifier.ToString());
                }
            }

            Namespaces.Add(namedef);
            Console.Error.WriteLine(node.Name);
        }

        private bool IsSelectAttribute(SyntaxList<AttributeListSyntax> attributeList)
        {
            foreach (var attrList in attributeList)
            {
                foreach (var attr in attrList.Attributes)
                {
                    IdentifierNameSyntax name = (IdentifierNameSyntax) attr.Name;
                    SyntaxToken identifier = name.Identifier;
                    if (identifier.Text == "Select")
                        return true;
                }
            }

            return false;
        }

        private bool IsBitfieldAttribute(SyntaxList<AttributeListSyntax> attributeList)
        {
            foreach (var attrList in attributeList)
            {
                foreach (var attr in attrList.Attributes)
                {
                    IdentifierNameSyntax name = (IdentifierNameSyntax) attr.Name;
                    SyntaxToken identifier = name.Identifier;
                    if (identifier.Text == "Bitfield")
                        return true;
                }
            }

            return false;
        }
    }
}