// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Tests.UnitTests.Models.Response;

using K2Bridge.Models.Response.ElasticError;
using K2Bridge.Tests.UnitTests.JsonConverters;
using NUnit.Framework;

[TestFixture]
public class ElasticErrorResponseTests
{
    private const string ExpectedValidErrorResponse = @"
            {""responses"":[
                    {
                        ""error"":{
                            ""root_cause"":[
                            {
                                ""type"":""test-root-type"",
                                ""reason"":""test-root-reason"",
                                ""index_uuid"":""test-index"",
                                ""index"":""test-index""
                            }
                            ],
                            ""type"":""test-type"",
                            ""reason"":""test-reason"",
                            ""phase"":""test-phase""
                        },
                        ""status"":500
                    }
                ]
                }
        ";

    private const string ExpectedEmptyCauseErrorResponse = @"
            {""responses"":[
                    {
                        ""error"":{
                            ""root_cause"":[
                            ],
                            ""type"":""test-type"",
                            ""reason"":""test-reason"",
                            ""phase"":""test-phase""
                        },
                        ""status"":500
                    }
                ]
                }
        ";

    [Test]
    public void JsonSerialize_WithValidElasticErrorResponse_SerializeCorrectly()
    {
        var errorResponse = new ElasticErrorResponse("test-type", "test-reason", "test-phase");
        errorResponse.AddRootCause("test-root-type", "test-root-reason", "test-index");
        errorResponse.AssertJson(ExpectedValidErrorResponse);
    }

    [Test]
    public void JsonSerialize_WithEmptyRootCauseElasticErrorResponse_SerializeCorrectly()
    {
        var errorResponse = new ElasticErrorResponse("test-type", "test-reason", "test-phase");
        errorResponse.AssertJson(ExpectedEmptyCauseErrorResponse);
    }
}
