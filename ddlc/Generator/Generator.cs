using System.Text;


namespace ddlc.Generator
{
    public interface IGenerator
    {
        void Generate(DDLDecl decl, string tab, StringBuilder sb);
    }
}