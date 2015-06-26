namespace WikiAccess {

    using System.Collections.Generic;

    public class WikidataIOErrorLog : IErrorLog {

        public WikidataIOErrorLog() {

            // ReSharper disable once UseObjectOrCollectionInitializer
            Errors = new List<ErrorMessage>();
#if DEBUG
            Errors.Add( new ErrorMessage( Module, 0, "WikidataIO module" ) );
#endif
        }

        public string Module => "D";

        public List<ErrorMessage> Errors {
            get; set;
        }

        public void UnableToRetrieveData() => this.Errors.Add( new ErrorMessage( this.Module, 1, "Unable to retrieve data" ) );
    }
}