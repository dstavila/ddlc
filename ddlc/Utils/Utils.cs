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
    }
}