using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;


namespace ddlc
{
    public class GeneratorContext
    {
        public string OutputName;
        public string OutputPath;
        public string SourcePath;
        public string language;
        public string OutputFilename;
        public string FullFilepath;
        public DDLAssembly Assembly = new DDLAssembly();
    }
    
    
    class Program
    {
        static void Main(string[] args)
        {
            var ctx = new GeneratorContext();
            string refArg = Path.Combine(GetExecutingDirectoryName(), "libddl.dll");
            string srcArg = null;
            string outArg = null;
            string nameArg = null;
            string langArg = null;
            for (int i = 0; i < args.Length; ++i)
            {
                if (args[i] == "-src")
                    srcArg = args[i + 1];
                else if (args[i] == "-out")
                    outArg = args[i + 1];
                else if (args[i] == "-name")
                    nameArg = args[i + 1];
                else if (args[i] == "-lang")
                    langArg = args[i + 1];
            }

            refArg = NormalizePath(refArg);
            srcArg = NormalizePath(srcArg);
            outArg = NormalizePath(outArg);
            nameArg = NormalizePath(nameArg);


            if (string.IsNullOrEmpty(srcArg))
            {
                Console.WriteLine("Error - Src arg is null or empty");
                return;
            }

            var files = new List<string>();
            var attr = File.GetAttributes(srcArg);
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
//                ctx.SourcePath = srcArg;
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

            ctx.OutputPath = outArg;
            ctx.language = langArg;

            var outAttrib = File.GetAttributes(outArg);
            if ((outAttrib & FileAttributes.Directory) != FileAttributes.Directory)
            {
                Console.WriteLine("[ERROR]: OutputPath should be path, not folder");
                return;
            }
            if (string.IsNullOrEmpty(nameArg))
            {
                ctx.OutputName = Path.GetFileName(ctx.OutputPath);
                var filename = ctx.OutputName + "_generated";
                ctx.FullFilepath = Path.Combine(ctx.OutputPath, filename);
                ctx.OutputFilename = filename;
            }
            else
            {
                ctx.OutputName = nameArg;
                var filename = nameArg + "_generated";
                ctx.FullFilepath = Path.Combine(ctx.OutputPath, filename);
                ctx.OutputFilename = filename;
            }


            var references = new List<MetadataReference>();
            using (var fs = File.OpenRead(refArg))
            {
                references.Add(MetadataReference.CreateFromStream(fs, filePath: refArg));
            }

            foreach (var source in files)
            {
                ParseDDLSyntax(references, source, ctx);
            }

            DoGenerate(langArg, outArg, ctx);
        }

        private static void ParseDDLSyntax(List<MetadataReference> references, string srcArg, GeneratorContext ctx)
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

            
            var walker = new DDLSyntaxWalker(compilation, srcArg, ctx.Assembly);
            foreach (var tree in syntaxTrees)
            {
                walker.Visit(tree.GetRoot());
            }
        }

        private static void DoGenerate(string langArg, string outArg, GeneratorContext ctx)
        {
            ctx.Assembly.Resolve();
            ctx.Assembly.Generate(ctx);
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