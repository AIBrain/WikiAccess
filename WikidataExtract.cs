// https: //github.com/JamesNK/Newtonsoft.Json

namespace WikiAccess {

    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Class to extract data from downloaded Wikidata This could have been part of WikidataIO.cs,
    /// but split out as it is large and cumbersome
    /// </summary>
    internal class WikidataExtract {
        private readonly WikidataCache _cache = new WikidataCache();

        public string[] ClaimsRequired {
            get;
        }

        public WikidataFields Fields {
            get;
        }

        public bool Success {
            get;
        }

        public WikidataExtractErrorLog WikidataExtractErrors {
            get;
        }

        private string Content {
            get;
        }

        public WikidataExtract( string content, string[] claimsrequired ) {
            WikidataExtractErrors = new WikidataExtractErrorLog();
            ClaimsRequired = claimsrequired;
            Fields = new WikidataFields();
            Content = content;
            Success = this.ExtractJson();
        }

        /// <summary>Note: This method requires Newtonsoft Json to be installed</summary>
        /// <returns></returns>
        private bool ExtractJson() {

            //Interpret the JSON - Basically read in a level at a time.
            var dataFromWiki = JObject.Parse( Content );
            var entities = ( JObject )dataFromWiki[ "entities" ];

            var entity = entities.Properties().First(); // Name is variable, so grab data by using first method
            var entityKey = entity.Name;

            var entityData = ( JObject )entity.Value;

            if ( entityKey == "-1" ) {
                WikidataExtractErrors.NotWikidata();
                return false;
            }

            var qcode = ( string )entityData[ "id" ];
            Fields.ID = Convert.ToInt32( qcode.Substring( 1 ) );
            var entityType = ( string )entityData[ "type" ];

            if ( entityType == null ) {
                WikidataExtractErrors.QcodeNotExist( entityKey );
                return false;
            }

            var descriptions = ( JObject )entityData[ "descriptions" ];
            var labels = ( JObject )entityData[ "labels" ];
            var wikipediaLinks = ( JObject )entityData[ "sitelinks" ];

            if ( labels != null ) {
                foreach ( var labelData in labels.Properties().Select( label => ( JObject )label.Value ) ) {
                    this.Fields.Labels.Add( ( string )labelData[ "language" ], ( string )labelData[ "value" ] );
                }
            }

            if ( descriptions != null ) {
                foreach ( var description in descriptions.Properties() ) {
                    var descriptionData = ( JObject )description.Value;
                    var key = ( string )descriptionData[ "language" ];
                    Fields.Description.Add( key, ( string )descriptionData[ "value" ] );
                }
            }

            if ( wikipediaLinks != null ) {
                foreach ( var wikipediaLinkData in wikipediaLinks.Properties().Select( wikipediaLink => ( JObject )wikipediaLink.Value ) ) {
                    this.Fields.WikipediaLinks.Add( ( string )wikipediaLinkData[ "site" ], ( string )wikipediaLinkData[ "title" ] );
                }
            }

            var claims = ( JObject )entityData[ "claims" ];
            if ( claims == null ) {
                return true;
            }

            //Now we get to loop through each claim property for that article
            foreach ( var claim in claims.Properties() ) {
                var claimKey = claim.Name;

                if ( Array.IndexOf( this.ClaimsRequired, claimKey ) == -1 ) {
                    continue;
                }

                var claimData = ( JArray )claim.Value;

                for ( var thisClaim = 0; thisClaim < claimData.Count(); thisClaim++ ) {

                    //claimData is an array - another loop

                    var thisClaimData = new WikidataClaim();

                    var mainSnak = ( JObject )claimData[ thisClaim ][ "mainsnak" ];
                    var snakType = ( string )mainSnak[ "snaktype" ];
                    var snakDataType = ( string )mainSnak[ "datatype" ];
                    var snakDataValue = ( JObject )mainSnak[ "datavalue" ];

                    if ( snakType == "novalue" || snakType == "somevalue" ) {
                        thisClaimData.ValueAsString = snakType;
                    }
                    else {
                        switch ( snakDataType ) {
                            case "string":
                            case "commonsMedia":
                            case "url":
                                thisClaimData.ValueAsString = ( string )snakDataValue[ "value" ];
                                break;

                            case "wikibase-item":
                                {
                                    var objectValue = ( JObject )snakDataValue[ "value" ];
                                    thisClaimData.Qcode = ( int )objectValue[ "numeric-id" ];
                                    thisClaimData.ValueAsString = this._cache.RetrieveLabel( thisClaimData.Qcode );
                                }
                                break;

                            case "time":
                                {
                                    var objectValue = ( JObject )snakDataValue[ "value" ];

                                    var valueTime = ( string )objectValue[ "time" ];

                                    var valueTimePrecision = ( string )objectValue[ "precision" ];
                                    var valueTimeCalendarModel = ( string )objectValue[ "calendarmodel" ];

                                    var julian = false;
                                    var gregorian = false;

                                    if ( valueTimeCalendarModel != "http://www.Wikidata.org/entity/Q1985727" ) {
                                        gregorian = true;
                                    }
                                    if ( valueTimeCalendarModel == "http://www.Wikidata.org/entity/Q1985786" ) {
                                        julian = true;
                                    }

                                    if ( valueTimePrecision == "11" || valueTimePrecision == "10" || valueTimePrecision == "9" || valueTimePrecision == "8" || valueTimePrecision == "7" || valueTimePrecision == "6" ) {
                                        var dateStart = valueTime.IndexOf( "-", 2 ) - 4;

                                        var thisDateString = ( valueTime.Substring( dateStart, 10 ) );
                                        thisDateString = thisDateString.Replace( "-00", "-01" ); // Occasionally get 1901-00-00 ?

                                        var validDate = true;
                                        DateTime thisDate;
                                        try {
                                            thisDate = DateTime.Parse( thisDateString, null, DateTimeStyles.RoundtripKind );
                                        }
                                        catch {
                                            thisDate = DateTime.MinValue;
                                            validDate = false;
                                        }
                                        if ( julian && valueTimePrecision == "11" ) {

                                            // All dates will be Gregorian Julian flag tells us to
                                            // display Julian date. JulianCalendar JulCal = new
                                            // JulianCalendar(); DateTime dta =
                                            // JulCal.ToDateTime(thisDate.Year, thisDate.Month,
                                            // thisDate.Day, 0, 0, 0, 0); thisDate = dta;
                                        }

                                        var precision = DatePrecision.Null;

                                        if ( validDate == false ) {
                                            precision = DatePrecision.Invalid;
                                        }
                                        else if ( valueTime.Substring( 0, 1 ) == "+" ) {
                                            switch ( valueTimePrecision ) {
                                                case "11":
                                                    precision = DatePrecision.Day;
                                                    break;

                                                case "10":
                                                    precision = DatePrecision.Month;
                                                    break;

                                                case "9":
                                                    precision = DatePrecision.Year;
                                                    break;

                                                case "8":
                                                    precision = DatePrecision.Decade;
                                                    break;

                                                case "7":
                                                    precision = DatePrecision.Century;
                                                    break;

                                                case "6":
                                                    precision = DatePrecision.Millenium;
                                                    break;
                                            }
                                        }
                                        else {
                                            precision = DatePrecision.Bce;
                                        }

                                        thisClaimData.ValueAsDateTime.ThisDate = thisDate;
                                        thisClaimData.ValueAsDateTime.ThisPrecision = precision;
                                    }
                                }
                                break;

                            case "monolingualtext":
                                {
                                    var objectValue = ( JObject )snakDataValue[ "value" ];
                                    var valueText = ( string )objectValue[ "text" ];
                                    var valueLanguage = ( string )objectValue[ "language" ];

                                    // TODO Multi language handling
                                    thisClaimData.ValueAsString = valueText + "(" + valueLanguage + ")";
                                }
                                break;

                            case "quantity":
                                {
                                    var objectValue = ( JObject )snakDataValue[ "value" ];
                                    var valueAmount = ( string )objectValue[ "amount" ];
                                    var valueUnit = ( string )objectValue[ "unit" ];
                                    var valueUpper = ( string )objectValue[ "upperBound" ];
                                    var valueLower = ( string )objectValue[ "lowerBound" ];

                                    thisClaimData.ValueAsString = "(" + valueLower + " to " + valueUpper + ") Unit " + valueUnit;
                                }
                                break;
                        }
                    }
                    this.Fields.Claims.Add( new KeyValuePair<int, WikidataClaim>( Convert.ToInt32( claimKey.Substring( 1 ) ), thisClaimData ) );
                }
            }
            return true;
        }
    }
}