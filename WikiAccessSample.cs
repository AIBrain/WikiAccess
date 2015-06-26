namespace WikiAccess {

    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class WikiAccessSample {

        private static void Main( string[] args ) {
            var qcode = 15818798;

            var wio = new WikidataIO {
                Action = "wbgetentities",
                Format = "json",
                Sites = "",
                Ids = qcode,
                Props = "claims|descriptions|labels|sitelinks",
                Languages = "",
                ClaimsRequired = new[] { "P31", "P27", "P21", "P569", "P570" }
            };

            var fields = wio.GetData();

            Console.WriteLine( "-----Errors-----" );
            var errors = new List<IErrorLog>( wio.GetErrors() );

            foreach ( var error in errors.Where( thisLog => thisLog != null ).SelectMany( thisLog => thisLog.Errors ) ) {
                Console.WriteLine( error.ToString() );
            }

            if ( fields == null ) {
                return;
            }

            string thisName;
            if ( !fields.Labels.TryGetValue( "en-gb", out thisName ) ) {
                fields.Labels.TryGetValue( "en", out thisName );
            }

            string thisDescription;
            if ( !fields.Description.TryGetValue( "en-gb", out thisDescription ) ) {
                fields.Description.TryGetValue( "en", out thisDescription );
            }

            string thisWikipedia;
            fields.WikipediaLinks.TryGetValue( "enwiki", out thisWikipedia );

            Console.WriteLine( thisName );
            Console.WriteLine( thisDescription );

            Console.WriteLine( "====================" );

            var wpio = new WikipediaIO {
                Action = "query",
                Export = "Yes",
                ExportNoWrap = "Yes",
                Format = "xml",
                Redirects = "yes",
                Titles = thisWikipedia
            };

            if ( wpio.GetData() ) {
                var templates = wpio.TemplatesUsed;
                var categories = wpio.CategoriesUsed;

                Console.WriteLine( wpio.PageTitle );
                Console.WriteLine( templates.Count() + " templates" );
                Console.WriteLine( categories.Count() + " categories" );
            }

            var errors2 = new List<IErrorLog>( wpio.GetErrors() );

            foreach ( var error in errors2.Where( thisLog => thisLog != null ).SelectMany( thisLog => thisLog.Errors ) ) {
                Console.WriteLine( error.ToString() );
            }

            Console.WriteLine( "Press enter to exit." );
            Console.ReadLine();
        }
    }
}