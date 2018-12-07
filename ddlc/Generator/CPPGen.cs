using System.Collections.Generic;
using System.Text;

namespace ddlc.Generator
{
    public class CPPGen
    {
        private const string t1 = "    ";
        private List<DDLDecl> _headers = new List<DDLDecl>();
        private readonly List<ClassDecl> ClassDecls = new List<ClassDecl>();
        private readonly List<StructDecl> StructDecls = new List<StructDecl>();


        public CPPGen(List<ClassDecl> classDecls, List<StructDecl> structDecls)
        {
            ClassDecls = classDecls;
            StructDecls = structDecls;
        }
        
        
        public void GenerateHeader(StringBuilder sb, List<NamespaceDecl> namespaces, List<DDLDecl> decls)
        {
            var header = @"
//===----------------------------------------------------------------------===//
//                                                                              
//  vim: ft=cpp tw=80                                                           
//                                                                              
//  DDL Generated code, do not modify directly.                                 
//                                                                              
//===----------------------------------------------------------------------===//
#ifndef DDL_GENERATED_H
#define DDL_GENERATED_H
#include <stdint.h>
#include <iosfwd>
#include <vector>
#include <string>
";
            sb.Append(header);
            sb.Append("\n\n");
            sb.AppendLine("/// ----------------------------------------");
            sb.AppendLine("/// Forward declarations");
            sb.AppendLine("/// ----------------------------------------");
            CPPFwdDeclGen.Generate(sb, namespaces, decls);
            sb.Append("\n\n");
            CPPHeaderGen.Generate(sb, namespaces, decls);
            sb.AppendLine("#endif");
        }

        public void GenerateSource(StringBuilder sb, 
            string headerFilename,
            List<NamespaceDecl> namespaces, 
            List<DDLDecl> decls)
        {
            var header = @"
//===----------------------------------------------------------------------===//
//                                                                              
//  vim: ft=cpp tw=80                                                           
//                                                                              
//  DDL Generated code, do not modify directly.                                 
//                                                                              
//===----------------------------------------------------------------------===//
#include <__FILENAME__>
#include <json.hpp>
#include <assert.h>
#include <string.h>
#include <vector>
using json = nlohmann::json;
";
            header = header.Replace("__FILENAME__", headerFilename);
            
            sb.Append(header);
            sb.Append("\n\n");
            CPPSourceGen.Generate(sb, namespaces, decls, ClassDecls, StructDecls);
//            foreach (var n in namespaces)
//                Generate(n, "", sb, false, false);
//            foreach (var d in decls)
//                Generate(d, "", sb, false, false);
        }

        private void Generate(DDLDecl decl, string tab, StringBuilder sb, bool header, bool fwddecl)
        {
            var type = decl.GetType();
            if (type == typeof(NamespaceDecl))
                NamespaceGen(decl as NamespaceDecl, tab, sb, header, fwddecl);
            if (type == typeof(StructDecl))
                StructGen(decl as StructDecl, tab, sb, header, fwddecl);
            else if (type == typeof(ClassDecl))
                ClassGen(decl as ClassDecl, tab, sb, header, fwddecl);
            else if (type == typeof(EnumDecl))
                EnumGen(decl as EnumDecl, tab, sb, fwddecl);
        }

        private void NamespaceGen(NamespaceDecl decl, string tab, StringBuilder sb, bool header, bool fwddecl)
        {
            if (!fwddecl)
            {
                if (header)
                {
                    if (decl.bHeaderGenerated) return;
                    decl.bHeaderGenerated = true;
                }
            }
            if (header)
            {
                var nestCount = 0;
                sb.AppendFormat(tab + "namespace {0}\n", Utils.ToCPPNamespace(decl.Name, ref nestCount));
                sb.AppendLine(tab + "{");
                foreach (var child in decl.Childs)
                {
                    Generate(child, tab + "    ", sb, header, fwddecl);
                }
                for (var i = 0; i < nestCount; ++i)
                    sb.Append(tab + "};");
                sb.AppendLine();
            }
        }
        
        private void StructGen(StructDecl aggregateDecl, string tab, StringBuilder sb, bool header, bool fwddecl)
        {
            if (fwddecl)
            {
                sb.AppendFormat(tab + "struct {0};\n", aggregateDecl.Name);
                return;
            }
            if (header && aggregateDecl.bHeaderGenerated) return;
            if (header) aggregateDecl.bHeaderGenerated = true;
            if (!header && aggregateDecl.bSourceGenerated) return;
            if (!header) aggregateDecl.bSourceGenerated = true;
            if (header)
            {
                sb.AppendFormat(tab + "struct {0}\n", aggregateDecl.Name);
                sb.AppendLine(tab + "{");
                foreach (var child in aggregateDecl.Childs)
                {
                    Generate(child, tab + t1, sb, header, fwddecl);
                }

                foreach (var mem in aggregateDecl.Fields)
                {
                    AggregateFieldGen(mem, tab + t1, sb);
                }

                sb.AppendLine();
                sb.AppendLine(tab + t1 + "// JSON Serialization/Deserialization");
                sb.AppendFormat(tab + t1 + "static std::string to_json(const {0} * self);\n", aggregateDecl.Name);
                sb.AppendFormat(tab + t1 + "static bool from_json(const std::string & json, {0} * self);\n", aggregateDecl.Name);
                sb.AppendLine(tab + "};");
            }
            else
            {
            }
        }

        private void ClassGen(ClassDecl decl, string tab, StringBuilder sb, bool header, bool fwddecl)
        {
            if (fwddecl)
            {
                sb.AppendFormat(tab + "struct {0};\n", decl.Name);
                return;
            }
            if (header && decl.bHeaderGenerated) return;
            if (header) decl.bHeaderGenerated = true;
            if (!header && decl.bSourceGenerated) return;
            if (!header) decl.bSourceGenerated = true;
            if (header)
            {
                sb.AppendFormat(tab + "struct {0}\n", decl.Name);
                sb.AppendLine(tab + "{");
                foreach (var child in decl.Childs)
                {
                    Generate(child, tab + t1, sb, header, fwddecl);
                }

                foreach (var mem in decl.Fields)
                {
                    AggregateFieldGen(mem, tab + t1, sb);
                }

                sb.AppendLine();
                sb.AppendLine(tab + t1 + "// JSON Serialization/Deserialization");
                sb.AppendFormat(tab + t1 + "static std::string to_json(const {0} * self);\n", decl.Name);
                sb.AppendFormat(tab + t1 + "static bool from_json(const std::string & json, {0} * self);\n", decl.Name);
                sb.AppendLine(tab + "};");
            }
            else
            {
//                CPPGenJsonSerialization.WriteStructJsonSerialization(tab, decl, ClassDecls, sb);
//                CPPGenJsonDeserialization.WriteStructJsonDeserialization(tab, decl, ClassDecls, sb);
            }
        }

        private void EnumGen(EnumDecl decl, string tab, StringBuilder sb, bool fwddecl)
        {
            if (fwddecl)
            {
                sb.AppendFormat(tab + "enum class {0} : uint32_t;\n", decl.Name);
                return;
            }
            if (decl.bGenerated) return;
            decl.bGenerated = true;
            sb.AppendFormat(tab + "enum class {0} : uint32_t\n", decl.Name);
            sb.AppendLine(tab + "{");
            foreach (var f in decl.Fields)
            {
                if (!string.IsNullOrEmpty(f.Value))
                    sb.AppendFormat(tab + t1 + "{0} = {1},\n", f.Name, f.Value);
                else
                    sb.AppendFormat(tab + t1 + "{0} = {1},\n", f.Name, MurmurHash2.Hash(decl.Name + "." + f.Name));
            }
            sb.AppendLine(tab + "};");
        }

        private void AggregateFieldGen(AggregateField field, string tab, StringBuilder sb)
        {
            string msg = null;
            var type = Converter.DDLTypeToCPPType(field.Type, field.sType);
            if (field.ArrayType == EArrayType.SCALAR)
                msg = string.Format("{0} {1};\n", type, field.Name);
            if (field.ArrayType == EArrayType.DYNAMIC || field.ArrayType == EArrayType.LIST)
                msg = string.Format("std::vector<{0}> {1};\n", type, field.Name);
            if (field.ArrayType == EArrayType.FIXED)
                msg = string.Format("{0} {1}[{2}];\n", type, field.Name, field.Count);
            if (string.IsNullOrEmpty(msg))
                msg = "ERROR";
            msg = tab + msg;
            sb.Append(msg);
        }
    }
}