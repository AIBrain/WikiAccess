namespace WikiAccess {

    using System.Collections.Generic;

    public interface IErrorLog {

        List<ErrorMessage> Errors {
            get; set;
        }

        string Module {
            get;
        }
    }
}

/*
 * A = WikimediaAPI
 * B = WikidataBiography
 * C = Category
 * D = WikidataIO
 * E = WikidataExtract
 * G = WikipediaBiography
 * T = Template
 * W = WikipediaIO
 *
 */