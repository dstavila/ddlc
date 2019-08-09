using System.Collections.Generic;
using System.Text;

namespace ddlc
{
    public static class QtGenJsonSerialization
    {
        public static void WriteStructJsonSerialization(
            AggregateDecl decl, 
            List<AggregateDecl> Structs, 
            TabbedStringBuilder sb)
        {
            var nspace = Utils.BuildNamespace(decl);
            sb.WriteLine($"QJsonObject {nspace}::ToJsonObject() const");
            sb.WriteLine("{");
            sb.PushTab();
            sb.WriteLine("QJsonObject result;");
            foreach (var f in decl.Fields)
                write_member(f, sb);
            sb.WriteLine("return result;");
            sb.PopTab();
            sb.WriteLine("}");
        }

        private static void write_member(AggregateField f, TabbedStringBuilder sb)
        {
            if (f.ArrayType == EArrayType.SCALAR)
            {
                if (Converter.IsPOD(f.Type))
                    sb.WriteLine($"result.insert(\"{f.Name}\", {casted_member(f.Type, f.Name)});");
                else
                    sb.WriteLine($"result.insert(\"{f.Name}\", {f.Name}.ToJsonObject());");
            }
            else
            {
                var arrayName = $"{f.Name}Array";
                sb.WriteLine($"QJsonArray {arrayName};");
                sb.WriteLine($"for(int i = 0; i < {f.Name}.size(); ++i)");
                sb.WriteLine("{");
                sb.PushTab();
                if (Converter.IsPOD(f.Type))
                {
                    sb.WriteLine($"QJsonValue temp({casted_member(f.Type, f.Name + "[i]")});");
                    sb.WriteLine($"{arrayName}.append(temp);");
                }
                else
                {
                    sb.WriteLine($"QJsonObject value = {f.Name}[i].ToJsonObject();");
                    sb.WriteLine($"{arrayName}.append(value);");
                }
                sb.PopTab();
                sb.WriteLine("}");
                sb.WriteLine($"result.insert(\"{f.Name}\", {arrayName});");
            }
        }

        private static string casted_member(EType Type, string Name)
        {
            switch (Type)
            {
                case EType.UINT8:
                case EType.UINT16:
                case EType.UINT32:
                case EType.INT8:
                case EType.INT16:
                case EType.INT32:
                case EType.SELECT:
                case EType.BITFIELD:
                    return $"static_cast<int>({Name})";
                case EType.UINT64:
                case EType.INT64:
                    return $"QJsonValue({Name})";
                case EType.FLOAT32:
                case EType.FLOAT64:
                    return $"static_cast<double>({Name})";
                case EType.BOOLEAN:
                case EType.STRING:
                    return $"{Name}";
                default:
                    return "ERROR";
            }
        }
    }
}