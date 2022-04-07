/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
*/

using System;
using NodaTime;
using ProtoBuf;
using System.IO;
using QuantConnect.Data;
using System.Collections.Generic;
using Microsoft.VisualBasic.FileIO;
using System.Globalization;
using QuantConnect.Util;
using Newtonsoft.Json;
using static QuantConnect.StringExtensions;



namespace QuantConnect.DataSource
{
    /// <summary>
    /// Example custom data type
    /// </summary>
    [ProtoContract(SkipConstructor = true)]
    public class InsiderTrading : BaseData
    {

        /// <summary>
        /// Date
        /// </summary>
        [ProtoMember(11)]
        [JsonProperty(PropertyName = "Date")]
        [JsonConverter(typeof(DateTimeJsonConverter), "yyyy-MM-dd")]
        public DateTime? Date { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        [ProtoMember(12)]
        [JsonProperty(PropertyName = "Name")]
        public string Name { get; set; }

        /// <summary>
        /// Shares
        /// </summary>
        [ProtoMember(13)]
        [JsonProperty(PropertyName = "Shares")]
        public string Shares { get; set; }

        /// <summary>
        /// PricePerShare
        /// </summary>
        [ProtoMember(14)]
        [JsonProperty(PropertyName = "PricePerShare")]
        public decimal? PricePerShare { get; set; }

        /// <summary>
        /// SharesOwnedFollowing
        /// </summary>
        [ProtoMember(15)]
        [JsonProperty(PropertyName = "SharesOwnedFollowing")]
        public string SharesOwnedFollowing { get; set; }

        /// <summary>
        /// The period of time that occurs between the starting time and ending time of the data point
        /// </summary>
        [ProtoMember(14)]
        public TimeSpan Period { get; set; }

        /// <summary>
        /// The time the data point ends at and becomes available to the algorithm
        /// </summary>
        public override DateTime EndTime
        {
            get { return Time + Period; }
            set { Time = value - Period; }
        }

        /// <summary>
        /// Required for successful Json.NET deserialization
        /// </summary>
        public InsiderTrading()
        {
        }

        /// <summary>
        /// Creates a new instance of QuiverCongress from a CSV line
        /// </summary>
        /// <param name="csvLine">CSV line</param>
        public InsiderTrading(string csvLine)
        {
            //var csv = line.Split(',');
            TextFieldParser parser = new TextFieldParser(new StringReader(csvLine));
            parser.HasFieldsEnclosedInQuotes = true;
            parser.SetDelimiters(",");
            string[] csv = parser.ReadFields();

            var parsedDate = Parse.DateTimeExact(csv[2], "yyyyMMdd");//, "'yyyy-MM-dd\'T\'HH:mm:ss.SSS\'Z\''"      
            Name = csv[3];
            Shares = csv[4];
            PricePerShare = csv[5].IfNotNullOrEmpty<decimal?>(s => Parse.Decimal(s));
            SharesOwnedFollowing = csv[6];
            Symbol = new Symbol(SecurityIdentifier.Parse(csv[0]), csv[1]);
            Time = parsedDate;
            Period = TimeSpan.FromDays(1);
            
        }


        /// <summary>
        /// Return the URL string source of the file. This will be converted to a stream
        /// </summary>
        /// <param name="config">Configuration object</param>
        /// <param name="date">Date of this source file</param>
        /// <param name="isLiveMode">true if we're in live mode, false for backtesting mode</param>
        /// <returns>String URL of source file.</returns>
        public override SubscriptionDataSource GetSource(SubscriptionDataConfig config, DateTime date, bool isLiveMode)
        {
            return new SubscriptionDataSource(
                Path.Combine(
                    Globals.DataFolder,
                    "alternative",
                    "InsiderTrading",
                    $"{config.Symbol.Value.ToLowerInvariant()}.csv"
                ),
                SubscriptionTransportMedium.LocalFile
            );
        }

        /// <summary>
        /// Parses the data from the line provided and loads it into LEAN
        /// </summary>
        /// <param name="config">Subscription configuration</param>
        /// <param name="line">Line of data</param>
        /// <param name="date">Date</param>
        /// <param name="isLiveMode">Is live mode</param>
        /// <returns>New instance</returns>
        public override BaseData Reader(SubscriptionDataConfig config, string line, DateTime date, bool isLiveMode)
        {
            return new InsiderTrading(line)
            {
                Symbol = config.Symbol,
            };
        }

        /// <summary>
        /// Indicates whether the data source is tied to an underlying symbol and requires that corporate events be applied to it as well, such as renames and delistings
        /// </summary>
        /// <returns>false</returns>
        public override bool RequiresMapping()
        {
            return true;
        }

        /// <summary>
        /// Indicates whether the data is sparse.
        /// If true, we disable logging for missing files
        /// </summary>
        /// <returns>true</returns>
        public override bool IsSparseData()
        {
            return true;
        }

        /// <summary>
        /// Converts the instance to string
        /// </summary>
        public override string ToString()
        {
            return $"{Symbol} - {Name} - {Shares} - {PricePerShare} - {SharesOwnedFollowing} ";
        }

        /// <summary>
        /// Gets the default resolution for this data and security type
        /// </summary>
        public override Resolution DefaultResolution()
        {
            return Resolution.Daily;
        }

        /// <summary>
        /// Gets the supported resolution for this data and security type
        /// </summary>
        public override List<Resolution> SupportedResolutions()
        {
            return DailyResolution;
        }

        /// <summary>
        /// Specifies the data time zone for this data type. This is useful for custom data types
        /// </summary>
        /// <returns>The <see cref="T:NodaTime.DateTimeZone" /> of this data type</returns>
        public override DateTimeZone DataTimeZone()
        {
            return DateTimeZone.Utc;
        }
    }
}
