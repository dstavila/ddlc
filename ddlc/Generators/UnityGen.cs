using System.Text;

namespace ddlc.Generators
{
    public class UnityGen
    {
        private static string t1 = "    ";
        
        
        public void Generate(DDLDecl decl)
        {
            if (decl is EnumDecl)
                Generate((EnumDecl)decl);
        }

        public void Generate(string tab, EnumDecl decl)
        {
            var sb = new StringBuilder();
            sb.AppendFormat(tab + "public enum {0} : uint\n", decl.Name);
            sb.AppendLine(tab + "{");
//            foreach (var memb in .Items)
//            {
//                if (memb.Value)
//                var name = string.Format("{0}")
//                sb.Append(buildSelectField(t2, sel.Name, memb.Name, memb.Value));
//            }

            sb.AppendLine(tab + "}");
        }
    }
}