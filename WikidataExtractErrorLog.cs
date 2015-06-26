namespace WikiAccess {

    using System.Collections.Generic;

    public class WikidataExtractErrorLog : IErrorLog {

        public WikidataExtractErrorLog() {

            // ReSharper disable once UseObjectOrCollectionInitializer
            Errors = new List<ErrorMessage>();
#if DEBUG
            Errors.Add( new ErrorMessage( Module, 0, "WikidataExtract module" ) );
#endif
        }

        public string Module => "E";

        public List<ErrorMessage> Errors {
            get; set;
        }

        public void NotWikidata() => this.Errors.Add( new ErrorMessage( this.Module, 1, "Download not in expected format" ) );

        public void QcodeNotExist( string qcode ) => this.Errors.Add( new ErrorMessage( this.Module, 2, qcode + " not found on Wikidata" ) );
    }
}