using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


namespace ddlc.Generator
{
    public class QtGenerator
    {
        private readonly List<ClassDecl> ClassDecls = new List<ClassDecl>();
        private readonly List<StructDecl> StructDecls = new List<StructDecl>();
        
        
        public QtGenerator(List<ClassDecl> classDecls, List<StructDecl> structDecls)
        {
            ClassDecls = classDecls;
            StructDecls = structDecls;
        }

        public void DoGenerate(GeneratorContext ctx, List<NamespaceDecl> namespaces, List<DDLDecl> decls)
        {
            var fullname = ctx.FullFilepath;
            var genName = "_fwd_generated";
            if (ctx.language == "qt")
                genName = "_Qt_fwd_generated";
            var fwdpath = Path.Combine(ctx.OutputPath, ctx.OutputName + genName);

            var sbfwd = new StringBuilder();
            GenerateForwardDeclarations(sbfwd, namespaces, decls, ctx.OutputName);
            Console.WriteLine(sbfwd.ToString());
            File.WriteAllText(fwdpath + ".h", sbfwd.ToString());
            Utils.Dos2Unix(fwdpath + ".h");
            
            var sbh = new StringBuilder();
            GenerateHeader(sbh, ctx.OutputName + genName + ".h", namespaces, decls, ctx.OutputName);
            Console.WriteLine(sbh.ToString());
            File.WriteAllText(fullname + ".h", sbh.ToString());
            Utils.Dos2Unix(fullname + ".h");

            var sbs = new StringBuilder();
            GenerateSource(sbs, ctx.OutputFilename + ".h", namespaces, decls);
            Console.WriteLine(sbs.ToString());
            File.WriteAllText(fullname + ".cpp", sbs.ToString());
            Utils.Dos2Unix(fullname + ".cpp");
        }



        private void GenerateForwardDeclarations(
            StringBuilder sb, 
            List<NamespaceDecl> namespaces, 
            List<DDLDecl> decls,
            string headerName)
        {
            var header = 
$@"//===----------------------------------------------------------------------===//
//                                                                              
//  vim: ft=cpp tw=80                                                           
//                                                                              
//  DDL Generated code, do not modify directly.                                 
//                                                                              
//===----------------------------------------------------------------------===//
#ifndef DDL_QT_{headerName.ToUpper()}_FWDDECL_GENERATED_H
#define DDL_QT_{headerName.ToUpper()}_FWDDECL_GENERATED_H
#include <stdint.h>
";
            sb.Append(header);
            sb.Append("\n\n");
            sb.AppendLine("/// ----------------------------------------");
            sb.AppendLine("/// Forward declarations");
            sb.AppendLine("/// ----------------------------------------");
            QtForwardDeclGen.Generate(sb, namespaces, decls);
            sb.AppendLine("#endif");
        }
        
        private void GenerateHeader(StringBuilder sb, string headerFilename, List<NamespaceDecl> namespaces, List<DDLDecl> decls, string headerName)
        {
            var header = 
$@"//===----------------------------------------------------------------------===//
//                                                                              
//  vim: ft=cpp tw=80                                                           
//                                                                              
//  DDL Generated code, do not modify directly.                                 
//                                                                              
//===----------------------------------------------------------------------===//
#ifndef DDL_QT_{headerName.ToUpper()}_GENERATED_H
#define DDL_QT_{headerName.ToUpper()}_GENERATED_H
#include <__FILENAME__>
#include <QString>
#include <QList>
#include <QJsonObject>
#include <QJsonArray>
#include <QJsonValue>
";
            header = header.Replace("__FILENAME__", headerFilename);
            sb.Append(header);
            sb.Append("\n\n");
            QtHeaderGen.Generate(sb, namespaces, decls);
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
#include <QJsonDocument>
#include <QVariant>
#include <assert.h>
";
            header = header.Replace("__FILENAME__", headerFilename);
            
            sb.Append(header);
            sb.Append("\n\n");
            QtSourceGen.Generate(sb, namespaces, decls, ClassDecls, StructDecls);
        }
    }
}