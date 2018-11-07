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
#include <stdbool.h>
#include <stddef.h>
#include <iosfwd>
";
            sb.Append(header);
            sb.Append("\n\n");
            foreach (var n in namespaces)
                Generate(n, "", sb, true);
            foreach (var d in decls)
                Generate(d, "", sb, true);
            sb.Append("\n\n");
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
#include <vector.h>
using json = nlohmann::json;
";
            header = header.Replace("__FILENAME__", headerFilename);
            
            sb.Append(header);
            sb.Append("\n\n");
            foreach (var n in namespaces)
                Generate(n, "", sb, false);
            foreach (var d in decls)
                Generate(d, "", sb, false);
        }

        private void Generate(DDLDecl decl, string tab, StringBuilder sb, bool header)
        {
            var type = decl.GetType();
            if (type == typeof(StructDecl))
                StructGen(decl as StructDecl, tab, sb, header);
            else if (type == typeof(ClassDecl))
                ClassGen(decl as ClassDecl, tab, sb, header);
            else if (type == typeof(EnumDecl))
                EnumGen(decl as EnumDecl, tab, sb);
        }
        
        private void StructGen(StructDecl decl, string tab, StringBuilder sb, bool header)
        {
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
                    Generate(child, tab + t1, sb, header);
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
                CPPGenJsonSerialization.WriteStructJsonSerialization(tab, decl, StructDecls, sb);
                CPPGenJsonDeserialization.WriteStructJsonDeserialization(tab, decl, StructDecls, sb);
            }
        }

        private void ClassGen(ClassDecl decl, string tab, StringBuilder sb, bool header)
        {
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
                    Generate(child, tab + t1, sb, header);
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
                CPPGenJsonSerialization.WriteStructJsonSerialization(tab, decl, ClassDecls, sb);
                CPPGenJsonDeserialization.WriteStructJsonDeserialization(tab, decl, ClassDecls, sb);
            }
        }

        private void EnumGen(EnumDecl decl, string tab, StringBuilder sb)
        {
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