namespace WikiAccess {

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Web;
    using System.Xml;

    /// <summary>Class to retrieve a Wikipedia page</summary>
    public class WikipediaIO : WikimediaApi {
        public List<string> CategoriesUsed;
        public List<string[]> TemplatesUsed;

        public string Action {
            get; set;
        }

        public string Article {
            get; set;
        }

        public string Export {
            get; set;
        }

        public string ExportNoWrap {
            get; set;
        }

        public string Format {
            get; set;
        }

        public string PageTitle {
            get; set;
        }

        public string Redirects {
            get; set;
        }

        public string Titles {
            get; set;
        }

        protected override string ApIurl {
            get {
                return @"http://en.wikipedia.org/w/api.php?";
            }
        }

        protected override string Parameters {
            get {
                var param = "action=" + Action;
                if ( Titles != "" ) {
                    param += "&titles=" + HttpUtility.UrlEncode( Titles );
                }
                if ( Format != "" ) {
                    param += "&format=" + Format;
                }
                if ( Redirects != "" ) {
                    param += "&redirects";
                }
                if ( Export != "" ) {
                    param += "&export";
                }
                if ( ExportNoWrap != "" ) {
                    param += "&exportnowrap";
                }

                return param;
            }
        }

        private WikipediaIOErrorLog WikipediaErrors {
            get;
        }

        public WikipediaIO() {
            WikipediaErrors = new WikipediaIOErrorLog();
            TemplatesUsed = new List<string[]>();
            CategoriesUsed = new List<string>();
        }

        /// <summary>Function to remove Wikipedia style [[links]]</summary>
        /// <param name="originalText"></param>
        /// <param name="revisedText"></param>
        /// <returns></returns>
        public static bool DelinkText( string originalText, out string revisedText ) {
            var thisText = originalText;

            var linkStart = 0;

            // Look for first [[ and ]], replace them with text repeat until no more [[ or ]]
            do {
                linkStart = thisText.IndexOf( "[[", StringComparison.Ordinal );
                if ( linkStart != -1 ) {

                    //Found start
                    var linkEnd = thisText.IndexOf( "]]", StringComparison.Ordinal );

                    if ( linkEnd == -1 || linkEnd < linkStart ) {

                        //Did not find close
                        revisedText = originalText;
                        return false;
                    }

                    // Found link, extract text
                    var link = thisText.Substring( linkStart, linkEnd - linkStart + 2 );
                    var newlink = link.Substring( 2, link.Length - 4 );

                    // If its piped, remove left side
                    var pipe = newlink.IndexOf( "|", StringComparison.Ordinal );
                    if ( pipe != -1 ) {
                        newlink = newlink.Substring( pipe + 1 );
                    }

                    // Replace [[link]] with newlink
                    thisText = thisText.Replace( link, newlink );
                }
                else {

                    //Did not find a [[, probably finished
                    //but first check there are no closing ]]
                    var linkClose = thisText.IndexOf( "]]", StringComparison.Ordinal );
                    if ( linkClose != -1 ) {
                        revisedText = originalText;
                        return false;
                    }
                }
            } while ( linkStart != -1 );

            revisedText = thisText;
            return true;
        }

        /// <summary>Download a page from Wikipedia and process</summary>
        /// <returns>True = success</returns>
        public bool GetData() {
            if ( GrabPage() ) {
                if ( ExtractXML() ) {
                    ExtractCategories();
                    ExtractTemplates();
                    return true;
                }
                this.WikipediaErrors.UnableToParseXML();
                return false;
            }
            this.WikipediaErrors.UnableToRetrieveData();
            return false;
        }

        public IEnumerable<IErrorLog> GetErrors() {
            yield return APIErrors;
            yield return WikipediaErrors;
        }

        private void ExtractCategories() {
            var catStart = Article.IndexOf( "[[Category:", StringComparison.Ordinal );

            while ( catStart > 0 ) {
                var catNextPipe = Article.IndexOf( "|", catStart, StringComparison.Ordinal );
                var catNextClose = Article.IndexOf( "]]", catStart, StringComparison.Ordinal );
                var catFinish = 0;

                if ( catNextPipe < catNextClose && catNextPipe > 0 ) {
                    catFinish = catNextPipe;
                }
                else {
                    catFinish = catNextClose;
                }

                if ( catStart != -1 && catFinish != -1 && catFinish > catStart ) {
                    CategoriesUsed.Add( Article.Substring( catStart + 11, catFinish - catStart - 11 ).ToLower().Trim() );
                    catStart = Article.IndexOf( "[[Category:", catFinish, StringComparison.Ordinal );
                }
                else {
                    WikipediaErrors.UnbalancedCategoryBrackets();
                    catStart = -99;
                }
            }
        }

        private void ExtractTemplates() {
            var tplStart = Article.IndexOf( "{{", StringComparison.Ordinal );

            while ( tplStart >= 0 ) {
                var tplNextPipe = Article.IndexOf( "|", tplStart, StringComparison.Ordinal );
                var tplNextClose = Article.IndexOf( "}}", tplStart, StringComparison.Ordinal );
                var tplFinish = 0;

                if ( tplNextPipe < tplNextClose && tplNextPipe > 0 ) {
                    tplFinish = tplNextPipe;
                }
                else {
                    tplFinish = tplNextClose;
                }

                if ( tplStart != -1 && tplFinish != -1 && tplFinish > tplStart ) {
                    var thisTemplate = new string[ 2 ];

                    thisTemplate[ 0 ] = Article.Substring( tplStart + 2, tplFinish - tplStart - 2 ).ToLower().Trim();
                    thisTemplate[ 1 ] = this.GetFullTemplate( tplStart );
                    TemplatesUsed.Add( thisTemplate );

                    tplStart = Article.IndexOf( "{{", tplFinish, StringComparison.Ordinal );
                }
                else {
                    WikipediaErrors.UnbalancedTemplateBrackets();
                    tplStart = -99;
                }
            }
        }

        /// <summary>Extract the article from the downloaded XML content</summary>
        /// <returns></returns>
        private bool ExtractXML() {
            var wikipediaArticleExists = false;

            using ( var reader = XmlReader.Create( new StringReader( Content ) ) ) {
                var thisElementName = new string[ 5 ];
                while ( reader.Read() ) {
                    switch ( reader.NodeType ) {
                        case XmlNodeType.Element:
                            thisElementName[ reader.Depth ] = reader.Name;
                            break;

                        case XmlNodeType.Text:
                            if ( thisElementName[ 0 ] == "mediawiki" ) {
                                if ( thisElementName[ 1 ] == "page" ) {
                                    if ( thisElementName[ 2 ] == "title" ) {
                                        PageTitle = reader.Value;
                                    }

                                    if ( thisElementName[ 2 ] == "revision" ) {
                                        if ( thisElementName[ 3 ] == "text" ) {
                                            wikipediaArticleExists = true;
                                            Article = this.RemoveHtmLcomments( ReplaceDash( RemoveTerminators( reader.Value ) ) );
                                        }
                                    }
                                }
                            }
                            break;
                    }
                }

                if ( !wikipediaArticleExists ) {
                    WikipediaErrors.ArticleNotExists();
                    return false;
                }
                return true;
            }
        }

        private string GetFullTemplate( int startpoint ) {
            var endpoint = 0;
            var leftBraceCount = 0;
            var rightBraceCount = 0;

            /* There might be a template within the template, so we just cant search for next }}
            * instead I count all further occurances of {{ or }}, when they balance I have the full original template.
            * In case the template is {{{{Hello}}wave}} I do an i++ to avoid matching 2+3 bracket.  */
            for ( var i = startpoint; i < Article.Length - 1; i++ ) {
                if ( Article.Substring( i, 2 ) == "{{" ) {
                    leftBraceCount++;
                    i++;
                }
                if ( Article.Substring( i, 2 ) == "}}" ) {
                    rightBraceCount++;
                    i++;
                }

                if ( leftBraceCount == rightBraceCount ) {
                    endpoint = i;
                    break;
                }
            }

            var templateText = "";

            if ( leftBraceCount != rightBraceCount ) {
                var pipePos = Article.IndexOf( "|", startpoint );
                WikipediaErrors.UnableToExtractTemplate( Article.Substring( startpoint + 2, pipePos - startpoint - 2 ) );
                return null;
            }

            if ( DelinkText( Article.Substring( startpoint + 2, endpoint - startpoint - 3 ), out templateText ) ) {
                return templateText;
            }
            return null;
        }

        private string RemoveHtmLcomments( string originalText ) {
            var thisText = originalText;
            var commentStart = 0;

            do {
                commentStart = thisText.IndexOf( "<!--", StringComparison.Ordinal );
                if ( commentStart != -1 ) {
                    var commentEnd = thisText.IndexOf( "-->", commentStart + 3, StringComparison.Ordinal );

                    if ( commentEnd == -1 || commentEnd < commentStart ) {
                        WikipediaErrors.UnbalancedHtmLcomment();
                        return thisText;
                    }
                    var comment = thisText.Substring( commentStart, commentEnd - commentStart + 3 );
                    thisText = thisText.Replace( comment, "" );
                }
            } while ( commentStart != -1 );

            return thisText;
        }

        private string RemoveTerminators( string originalText ) {
            var newText = originalText;
            newText = newText.Replace( "\u000a", string.Empty );
            newText = newText.Replace( "\u000b", string.Empty );
            newText = newText.Replace( "\u000c", string.Empty );
            newText = newText.Replace( "\u000d", string.Empty );
            newText = newText.Replace( "\u0085", string.Empty );
            newText = newText.Replace( "\u2028", string.Empty );
            newText = newText.Replace( "\u2029", string.Empty );
            return newText;
        }

        private string ReplaceDash( string original ) {
            var output = original;

            output = output.Replace( "\u058A", "-" );
            output = output.Replace( "\u05BE", "-" );
            output = output.Replace( "\u1400", "-" );
            output = output.Replace( "\u1806", "-" );
            output = output.Replace( "\u2010", "-" );

            output = output.Replace( "\u2011", "-" );
            output = output.Replace( "\u2012", "-" );
            output = output.Replace( "\u2013", "-" );
            output = output.Replace( "\u2014", "-" );
            output = output.Replace( "\u2015", "-" );

            output = output.Replace( "\u2E17", "-" );
            output = output.Replace( "\u2E1A", "-" );
            output = output.Replace( "\u2E3A", "-" );
            output = output.Replace( "\u2E3B", "-" );
            output = output.Replace( "\u301C", "-" );

            output = output.Replace( "\u3030", "-" );
            output = output.Replace( "\u30A0", "-" );
            output = output.Replace( "\uFE31", "-" );
            output = output.Replace( "\uFE32", "-" );
            output = output.Replace( "\uFE58", "-" );

            output = output.Replace( "\uFE63", "-" );
            output = output.Replace( "\uFF0D", "-" );

            return output;
        }
    }
}