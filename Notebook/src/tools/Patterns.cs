using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Notebook
{
    public class Patterns
    {
        public static bool IsURL(String url)
        {
            return (Regex.IsMatch(url, @"^(http|https)://[^ ]*$"));
        }

        public static string MatchWebPageTitle(String page)
        {
            return Regex.Match(
                page,
                "<title>(.*?)</title>",
                RegexOptions.IgnoreCase | RegexOptions.Singleline
            ).Groups[1].Value;
        }


        public static string MatchWebPageEncoding(String page)
        {
            return Regex.Match(
                page,
                "<meta.*?charset=['\"]?(?<Encoding>[^\"']+)['\"]?",
                RegexOptions.IgnoreCase
            ).Groups["Encoding"].Value;
        }


        public static string MatchWebPageRedirectUrl(String page)
        {
            return Regex.Match(
                page,
                "<meta.*?http-equiv=\"refresh\".*?(CONTENT|content)=[\"']\\d;\\s?(URL|url)=(?<url>.*?)([\"']\\s*\\/?>)",
                RegexOptions.IgnoreCase
            ).Groups["url"].Value;
        }
    }
}
