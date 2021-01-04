// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.NET.TestFramework.ProjectConstruction
{
    public class TestItemSpec
    {
        public string Name { get; set; }

        public string Include { get; set; }

        public string Exclude { get; set; }

        public Dictionary<string, string> Attributes { get; } = new(StringComparer.Ordinal);
    }
}
