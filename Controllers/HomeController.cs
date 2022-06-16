using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SEOAnalyser.Models;

namespace SEOAnalyser.Controllers{
    public class HomeController : Controller{

        public JsonResult GetRecords(){
            return Json(new { data = new SeoResult() });
        }

        [HttpPost]
        public JsonResult GetUrlSource(string url, List<string> keywordList, bool disableAnalyse, bool disableExt, bool disableMeta, bool disableWord)
        {
            if (disableAnalyse) return Json(new {data = new SeoResult()});
            url = url.Substring(0, 4) != "http" ? "http://" + url : url;
            using (var client = new WebClient()){
                try{
                    var htmlCode = client.DownloadString(url);
                    var result = WordCount(htmlCode, keywordList, disableExt, disableMeta, disableWord);
                    var jsonData = new { data = result };
                    return Json(jsonData);
                }

                catch (Exception ex){
                    return new JsonResult( new { code = "URL not respond", errorMessage = ex.Message } );
                }
            }
        }

        private List<SeoResult> WordCount(string html, List<string> keywordList, bool disableExt, bool disableMeta, bool disableWord){
            var seoResult = new List<SeoResult>();
            var source = html.Split(new[] { '.', '?', '!', ' ', ';', ':', ',' }, StringSplitOptions.RemoveEmptyEntries);

            if(!disableWord){
                foreach (var keyword in keywordList)
                {
                    var matchQuery = from word in source
                        where word.ToLowerInvariant() == keyword.Trim().ToLowerInvariant()
                        select word;

                    var wordCount = matchQuery.Count();
                    seoResult.Add(new SeoResult{
                                        Keyword = keyword.Trim(),
                                        Occurance = wordCount
                                    });
                }
            }else{
                if(!disableExt){
                    var externalLinks = Regex.Matches(html, @"<(a|link).*?href=(""|')(.+?)(""|').*?>");
                    seoResult.Add(new SeoResult{
                                        Keyword = "External Link",
                                        Occurance = externalLinks.Count
                                    });
                }
                else{
                    if(!disableMeta){
                        var metaTag = new Regex(@"<meta\s*(?:(?:\b(\w|-)+\b\s*(?:=\s*(?:""[^""]*""|'" + @"[^']*'|[^""'<> ]+)\s*)?)*)/?\s*>", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
                        var metaInformation = new Dictionary<string, string>();

                        foreach (Match m in metaTag.Matches(html)){
                            var metaContentTag = new Regex(@"(?<name>\b(\w|-)+\b)\" +
                                                        @"s*=\s*(""(?<value>" +
                                                        @"[^""]*)""|'(?<value>[^']*)'" +
                                                        @"|(?<value>[^""'<> ]+)\s*)+",
                                                            RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
                            var matchs = metaContentTag.Match(m.Value.ToString());
                        }
                    }
                    else{
                        return Json(new {data = new SeoResult()});
                    }
                }
            }

            return seoResult;
        }
    }
}
