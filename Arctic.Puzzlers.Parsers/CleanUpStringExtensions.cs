using System.Text.RegularExpressions;
namespace Arctic.Puzzlers.Parsers
{
    public static class CleanUpStringExtensions
    {
        public static string CleanUpName(this string name)
        {
            name = Regex.Replace(name, "( \\d+(pcs))", "");
            name = Regex.Replace(name, "( \\d+ (pcs))", "");
            name = Regex.Replace(name, "( \\d+x\\d+ (pcs))", "");
            name = Regex.Replace(name, "( \\d+x\\d+(pcs))", "");
            name = Regex.Replace(name, "( \\d+(pc))", "");
            name = Regex.Replace(name, "( \\d+ (pc))", "");
            name = Regex.Replace(name, "( \\d+x\\d+ (pc))", "");
            name = Regex.Replace(name, "( \\d+x\\d+(pc))", "");
            name = Regex.Replace(name, "( \\d+[p])", "");            
            name = Regex.Replace(name, "( \\d+ [p])", "");            
            name = Regex.Replace(name, "( \\d+x\\d+ [p])", "");
            name = Regex.Replace(name, "( \\d+x\\d+[p])", "");
            name = name.TrimEnd(' ');
            return name;
        }
    }
}
