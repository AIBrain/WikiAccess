namespace WikiAccess {

    using System.Collections.Generic;

    /// <summary>General interface to Wikidata</summary>
    public class WikidataIO : WikimediaApi {

        public string Action {
            get; set;
        }

        public string[] ClaimsRequired {
            get; set;
        }

        public string Format {
            get; set;
        }

        public int Ids {
            get; set;
        }

        public string Languages {
            get; set;
        }

        public string Props {
            get; set;
        }

        public string Sites {
            get; set;
        }

        protected override string ApIurl {
            get {
                return @"http://www.Wikidata.org/w/api.php?";
            }
        }

        protected override string Parameters {
            get {
                var param = "action=" + Action;
                if ( Format != "" ) {
                    param += "&format=" + Format;
                }
                if ( Sites != "" ) {
                    param += "&sites=" + Sites;
                }
                if ( Ids != 0 ) {
                    param += "&ids=Q" + this.Ids;
                }
                if ( Props != "" ) {
                    param += "&props=" + Props;
                }
                if ( Languages != "" ) {
                    param += "&languages=" + Languages;
                }

                return param;
            }
        }

        private IErrorLog ExternalErrors {
            get; set;
        }

        private WikidataIOErrorLog WikidataErrors {
            get;
        }

        public WikidataIO() {
            WikidataErrors = new WikidataIOErrorLog();
        }

        public WikidataFields GetData() {
            if ( GrabPage() ) {
                var item = new WikidataExtract( Content, ClaimsRequired );
                ExternalErrors = item.WikidataExtractErrors;
                return item.Success ? item.Fields : null;
            }
            this.WikidataErrors.UnableToRetrieveData();
            return null;
        }

        public IEnumerable<IErrorLog> GetErrors() {
            yield return APIErrors;
            yield return WikidataErrors;
            yield return ExternalErrors;
        }
    }
}