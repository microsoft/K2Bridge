// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Request
{
    using K2Bridge.Visitors;

    /// <summary>
    /// An interface to note a visitable class.
    /// </summary>
    internal interface IVisitable
    {
        /// <summary>
        /// This method performs the action (i.e. the visit).
        /// </summary>
        /// <param name="visitor">The <see cref="IVisitor"/> implemented class performing the action.</param>
        void Accept(IVisitor visitor);
    }
}
