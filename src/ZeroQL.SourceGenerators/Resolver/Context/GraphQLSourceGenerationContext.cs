﻿using Microsoft.CodeAnalysis;

namespace ZeroQL.SourceGenerators.Resolver.Context;

#pragma warning disable CS8618
public class GraphQLSourceGenerationContext
{
    public GraphQLSourceGenerationContext(
        string key,
        string? operationName,
        string operationType,
        string operationQuery,
        string operationHash,
        string queryTypeName,
        INamedTypeSymbol graphQLMethodInputSymbol,
        INamedTypeSymbol requestExecutorInputSymbol,
        INamedTypeSymbol uploadType,
        UploadInfoByType[] uploadProperties)
    {
        Key = key;
        UploadType = uploadType;
        OperationName = operationName;
        OperationType = operationType;
        OperationQuery = operationQuery;
        OperationHash = operationHash;
        QueryTypeName = queryTypeName;
        GraphQLMethodInputSymbol = graphQLMethodInputSymbol;
        RequestExecutorInputSymbol = requestExecutorInputSymbol;
        UploadProperties = uploadProperties;
    }

    public string Key { get; }

    public INamedTypeSymbol UploadType { get; }

    public string? OperationName { get; }

    public string OperationType { get; }

    public string OperationQuery { get; }

    public string OperationHash { get; }

    public string QueryTypeName { get; }

    public INamedTypeSymbol GraphQLMethodInputSymbol { get; }

    public INamedTypeSymbol RequestExecutorInputSymbol { get; }

    public UploadInfoByType[] UploadProperties { get; }
}