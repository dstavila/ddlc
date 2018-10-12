using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;


namespace ddlc
{
    class Program
    {
        static void Main(string[] args)
        {
//            args = null;
//            args = new string[]
//            {
//                "-src",
//                "D:/Bully/rpc-ddl.git/example/PGP.ddl",
//                "-out",
//                "D:/Bully/rpc-ddl.git/example/",
//                "-lang",
//                "cs"
//            };
            string refArg = Path.Combine(GetExecutingDirectoryName(), "libddl.dll");
            string srcArg = null;
            string outArg = null;
            string langArg = null;
            for (int i = 0; i < args.Length; ++i)
            {
                if (args[i] == "-src")
                    srcArg = args[i + 1];
                else if (args[i] == "-out")
                    outArg = args[i + 1];
                else if (args[i] == "-lang")
                    langArg = args[i + 1];
            }

            refArg = NormalizePath(refArg);
            srcArg = NormalizePath(srcArg);
            outArg = NormalizePath(outArg);


            if (string.IsNullOrEmpty(srcArg))
            {
                Console.WriteLine("Error - Src arg is null or empty");
                return;
            }

            var files = new List<string>();
            FileAttributes attr = File.GetAttributes(srcArg);
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                if (!Directory.Exists(srcArg))
                {
                    Console.WriteLine("Requested source dir doesn't exist: " + srcArg);
                    return;
                }

                var dirInfo = new DirectoryInfo(srcArg);
                var fileList = dirInfo.GetFiles("*.ddl");
                foreach (var f in fileList)
                {
                    files.Add(f.FullName);
                }
            }
            else
            {
                if (!File.Exists(srcArg))
                {
                    Console.WriteLine("Requested source file doesn't exist: " + srcArg);
                    return;
                }
                files.Add(srcArg);
            }


            var references = new List<MetadataReference>();
            using (var fs = File.OpenRead(refArg))
            {
                references.Add(MetadataReference.CreateFromStream(fs, filePath: refArg));
            }

            foreach (var source in files)
            {
                DoGenerate(references, source, langArg, outArg);
            }
        }

        private static void DoGenerate(List<MetadataReference> references, string srcArg, string langArg, string outArg)
        {
            var syntaxTrees = new List<SyntaxTree>();
            {
                using (var fs = File.OpenRead(srcArg))
                {
                    var text = SourceText.From(fs);
                    syntaxTrees.Add(CSharpSyntaxTree.ParseText(text, path: srcArg));
                }
            }


            Compilation compilation = CSharpCompilation.Create(
                "DDL",
                syntaxTrees,
                references,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            DDLSyntaxWalker walker = new DDLSyntaxWalker(compilation);
            foreach (SyntaxTree tree in syntaxTrees)
            {
                walker.Visit(tree.GetRoot());
            }


            if (langArg == "cs")
            {
                var filename = Path.GetFileNameWithoutExtension(srcArg) + "_generated.cs";
                UnityGen.Generate(outArg, filename, walker.Namespaces, walker.Selects, walker.Structs);
            }
            else if (langArg == "cpp")
            {
                var cppfile = Path.GetFileNameWithoutExtension(srcArg) + "_generated";
                CPPGen.Generate(outArg, cppfile, walker.Namespaces, walker.Selects, walker.Structs);
            }
            else
            {
                var filename = Path.GetFileNameWithoutExtension(srcArg) + "_generated.cs";
                UnityGen.Generate(outArg, filename, walker.Namespaces, walker.Selects, walker.Structs);
                
                var cppfile = Path.GetFileNameWithoutExtension(srcArg) + "_generated";
                CPPGen.Generate(outArg, cppfile, walker.Namespaces, walker.Selects, walker.Structs);
            }
        }

        private static string NormalizePath(string path)
        {
            return path?.Trim();
        }

        private static string GetExecutingDirectoryName()
        {
            var location = new Uri(Assembly.GetEntryAssembly().GetName().CodeBase);
            return new FileInfo(location.AbsolutePath).Directory.FullName;
        }

    }
}