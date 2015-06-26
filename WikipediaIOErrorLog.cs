namespace WikiAccess {

    using System.Collections.Generic;

    /// <summary>Class to store errors pertaining to Wikipedia IO</summary>
    public class WikipediaIOErrorLog : IErrorLog {

        public WikipediaIOErrorLog() {

            // ReSharper disable once UseObjectOrCollectionInitializer
            Errors = new List<ErrorMessage>();
#if DEBUG
            Errors.Add( new ErrorMessage( Module, 0, "WikipediaIO module" ) );
#endif
        }

        public string Module => "W";

        public List<ErrorMessage> Errors {
            get; set;
        }

        public void UnableToRetrieveData() => this.Errors.Add( new ErrorMessage( this.Module, 1, "Unable to retrieve data" ) );

        public void UnableToParseXML() => this.Errors.Add( new ErrorMessage( this.Module, 2, "Unable to parse XML" ) );

        public void ArticleNotExists() => this.Errors.Add( new ErrorMessage( this.Module, 3, "Wikipedia article does not exist" ) );

        public void UnbalancedHtmLcomment() => this.Errors.Add( new ErrorMessage( this.Module, 4, "Unbalanced HTML comments in article" ) );

        public void UnbalancedCategoryBrackets() => this.Errors.Add( new ErrorMessage( this.Module, 5, "Unbalanced Category brackets" ) );

        public void UnbalancedTemplateBrackets() => this.Errors.Add( new ErrorMessage( this.Module, 6, "Unbalanced Template brackets" ) );

        public void UnableToExtractTemplate( string templateName ) => this.Errors.Add( new ErrorMessage( this.Module, 7, "Unable to extract template " + templateName ) );
    }
}