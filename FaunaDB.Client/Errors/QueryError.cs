﻿using System.Collections.Generic;
using Newtonsoft.Json;

namespace FaunaDB.Errors
{
    public class QueryError
    {
        public IReadOnlyList<string> Position { get; private set; }

        public string Code { get; private set; }

        public string Description { get; private set; }

        public IReadOnlyList<ValidationFailure> Failures { get; private set; }

        [JsonConstructor]
        public QueryError(IReadOnlyList<string> position, string code, string description, IReadOnlyList<ValidationFailure> failures)
        {
            Position = position ?? new List<string>();
            Code = code;
            Description = description;
            Failures = failures ?? new List<ValidationFailure>();
        }
    }
}