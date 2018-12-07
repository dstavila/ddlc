using System;
using System.Collections.Generic;
using System.IO;
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


        public void DoGenerate(GeneratorContext ctx, List<NamespaceDecl> namespaces, List<DDLDecl> decls)
        {
            var fullFilename = ctx.FullFilepath;
            string name;
            var fwdpath = Path.Combine(ctx.OutputPath, ctx.OutputName + "_fwd_generated");
            
            var sbfwd = new StringBuilder();
            GenerateForwardDeclarations(sbfwd, namespaces, decls);
            Console.WriteLine(sbfwd.ToString());
            File.WriteAllText(fwdpath + ".h", sbfwd.ToString());
            Utils.Dos2Unix(fwdpath + ".h");
            
            var sbh = new StringBuilder();
            GenerateHeader(sbh, namespaces, decls);
            Console.WriteLine(sbh.ToString());
            File.WriteAllText(fullFilename + ".h", sbh.ToString());
            Utils.Dos2Unix(fullFilename + ".h");

            var sbs = new StringBuilder();
            GenerateSource(sbs, ctx.OutputFilename + ".h", namespaces, decls);
            Console.WriteLine(sbs.ToString());
            File.WriteAllText(fullFilename + ".cpp", sbs.ToString());
            Utils.Dos2Unix(fullFilename + ".cpp");
        }

        private void GenerateForwardDeclarations(StringBuilder sb, List<NamespaceDecl> namespaces, List<DDLDecl> decls)
        {
            var header = 
@"//===----------------------------------------------------------------------===//
//                                                                              
//  vim: ft=cpp tw=80                                                           
//                                                                              
//  DDL Generated code, do not modify directly.                                 
//                                                                              
//===----------------------------------------------------------------------===//
#ifndef DDL_FORWARDDECL_GENERATED_H
#define DDL_FORWARDDECL_GENERATED_H
";
            sb.Append(header);
            sb.Append("\n\n");
            sb.AppendLine("/// ----------------------------------------");
            sb.AppendLine("/// Forward declarations");
            sb.AppendLine("/// ----------------------------------------");
            CPPFwdDeclGen.Generate(sb, namespaces, decls);
        }
        
        private void GenerateHeader(StringBuilder sb, List<NamespaceDecl> namespaces, List<DDLDecl> decls)
        {
            var header = 
@"//===----------------------------------------------------------------------===//
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
            CPPHeaderGen.Generate(sb, namespaces, decls);
            sb.AppendLine("#endif");
        }

        private void GenerateSource(StringBuilder sb, 
            string headerFilename,
            List<NamespaceDecl> namespaces, 
            List<DDLDecl> decls)
        {
            var header = 
@"//===----------------------------------------------------------------------===//
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
        }
    }
}