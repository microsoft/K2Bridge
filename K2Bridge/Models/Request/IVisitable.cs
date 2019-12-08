// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace K2Bridge.Models.Request
{
    using K2Bridge.Visitors;

    internal interface IVisitable
    {
        void Accept(IVisitor visitor);
    }
}
