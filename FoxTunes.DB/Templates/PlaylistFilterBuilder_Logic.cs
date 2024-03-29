﻿using FoxDb.Interfaces;
using FoxTunes.Interfaces;
using System.Collections.Generic;

namespace FoxTunes.Templates
{
    public partial class PlaylistFilterBuilder
    {
        public static readonly IEnumerable<FilterParserEntryOperator> NumericOperators = new[]
        {
            FilterParserEntryOperator.Greater,
            FilterParserEntryOperator.GreaterEqual,
            FilterParserEntryOperator.Less,
            FilterParserEntryOperator.LessEqual
        };

        public PlaylistFilterBuilder(IDatabase database, IFilterParserResult filter)
        {
            this.Database = database;
            this.Filter = filter;
        }

        public IDatabase Database { get; private set; }

        public IFilterParserResult Filter { get; private set; }
    }
}
