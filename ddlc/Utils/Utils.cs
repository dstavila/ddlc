using System.Text;

namespace ddlc
{
    public static class Utils
    {
        public static string ToCPPNamespace(string name)
        {
            var split = name.Split('.');
            var result = split[0];
            for (var i = 1; i < split.Length; ++i)
                result += " { " + string.Format("namespace {0}", split[i]);
            return result;
        }
        public static string BuildNamespace(DDLDecl decl)
        {
            if (decl.Parent == null)
                return decl.Name;
            return BuildNamespace(decl.Parent) + "::" + decl.Name;
        }

        public static string ExtraCommandNamespace(string space)
        {
            var list = space.Split('.');
            if (list.Length > 2)
            {
                if (list[0] == "DDL" && list[1] == "Commands")
                {
                    var sb = new StringBuilder();
                    var len = list.Length; 
                    for (var i = 2; i < len - 1; ++i)
                    {
                        sb.Append(list[i]);
                        sb.Append("_");
                    }
                    sb.Append(list[len - 1]);
                    return sb.ToString();
                }
                return null;
            }
            return null;
        }
    }
}