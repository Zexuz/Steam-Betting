// Generated by the protocol buffer compiler.  DO NOT EDIT!
// source: protofiles/steamCommunity.proto
#region Designer generated code

using System;
using System.Threading;
using System.Threading.Tasks;
using grpc = global::Grpc.Core;

namespace RpcCommunicationSteamCommunity {
  /// <summary>
  ///these endpoints is on the nodejs server
  /// </summary>
  public static partial class ChatService
  {
    static readonly string __ServiceName = "SteamCommunity.ChatService";

    static readonly grpc::Marshaller<global::RpcCommunicationSteamCommunity.EmptyMessage> __Marshaller_EmptyMessage = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::RpcCommunicationSteamCommunity.EmptyMessage.Parser.ParseFrom);
    static readonly grpc::Marshaller<global::RpcCommunicationSteamCommunity.PriceHistoryForItemRequest> __Marshaller_PriceHistoryForItemRequest = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::RpcCommunicationSteamCommunity.PriceHistoryForItemRequest.Parser.ParseFrom);
    static readonly grpc::Marshaller<global::RpcCommunicationSteamCommunity.PriceHistoryForItemResponse> __Marshaller_PriceHistoryForItemResponse = grpc::Marshallers.Create((arg) => global::Google.Protobuf.MessageExtensions.ToByteArray(arg), global::RpcCommunicationSteamCommunity.PriceHistoryForItemResponse.Parser.ParseFrom);

    static readonly grpc::Method<global::RpcCommunicationSteamCommunity.EmptyMessage, global::RpcCommunicationSteamCommunity.EmptyMessage> __Method_Ping = new grpc::Method<global::RpcCommunicationSteamCommunity.EmptyMessage, global::RpcCommunicationSteamCommunity.EmptyMessage>(
        grpc::MethodType.Unary,
        __ServiceName,
        "Ping",
        __Marshaller_EmptyMessage,
        __Marshaller_EmptyMessage);

    static readonly grpc::Method<global::RpcCommunicationSteamCommunity.PriceHistoryForItemRequest, global::RpcCommunicationSteamCommunity.PriceHistoryForItemResponse> __Method_GetPriceHistoryForItem = new grpc::Method<global::RpcCommunicationSteamCommunity.PriceHistoryForItemRequest, global::RpcCommunicationSteamCommunity.PriceHistoryForItemResponse>(
        grpc::MethodType.Unary,
        __ServiceName,
        "GetPriceHistoryForItem",
        __Marshaller_PriceHistoryForItemRequest,
        __Marshaller_PriceHistoryForItemResponse);

    /// <summary>Service descriptor</summary>
    public static global::Google.Protobuf.Reflection.ServiceDescriptor Descriptor
    {
      get { return global::RpcCommunicationSteamCommunity.SteamCommunityReflection.Descriptor.Services[0]; }
    }

    /// <summary>Base class for server-side implementations of ChatService</summary>
    public abstract partial class ChatServiceBase
    {
      public virtual global::System.Threading.Tasks.Task<global::RpcCommunicationSteamCommunity.EmptyMessage> Ping(global::RpcCommunicationSteamCommunity.EmptyMessage request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

      public virtual global::System.Threading.Tasks.Task<global::RpcCommunicationSteamCommunity.PriceHistoryForItemResponse> GetPriceHistoryForItem(global::RpcCommunicationSteamCommunity.PriceHistoryForItemRequest request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

    }

    /// <summary>Client for ChatService</summary>
    public partial class ChatServiceClient : grpc::ClientBase<ChatServiceClient>
    {
      /// <summary>Creates a new client for ChatService</summary>
      /// <param name="channel">The channel to use to make remote calls.</param>
      public ChatServiceClient(grpc::Channel channel) : base(channel)
      {
      }
      /// <summary>Creates a new client for ChatService that uses a custom <c>CallInvoker</c>.</summary>
      /// <param name="callInvoker">The callInvoker to use to make remote calls.</param>
      public ChatServiceClient(grpc::CallInvoker callInvoker) : base(callInvoker)
      {
      }
      /// <summary>Protected parameterless constructor to allow creation of test doubles.</summary>
      protected ChatServiceClient() : base()
      {
      }
      /// <summary>Protected constructor to allow creation of configured clients.</summary>
      /// <param name="configuration">The client configuration.</param>
      protected ChatServiceClient(ClientBaseConfiguration configuration) : base(configuration)
      {
      }

      public virtual global::RpcCommunicationSteamCommunity.EmptyMessage Ping(global::RpcCommunicationSteamCommunity.EmptyMessage request, grpc::Metadata headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
      {
        return Ping(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual global::RpcCommunicationSteamCommunity.EmptyMessage Ping(global::RpcCommunicationSteamCommunity.EmptyMessage request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_Ping, null, options, request);
      }
      public virtual grpc::AsyncUnaryCall<global::RpcCommunicationSteamCommunity.EmptyMessage> PingAsync(global::RpcCommunicationSteamCommunity.EmptyMessage request, grpc::Metadata headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
      {
        return PingAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual grpc::AsyncUnaryCall<global::RpcCommunicationSteamCommunity.EmptyMessage> PingAsync(global::RpcCommunicationSteamCommunity.EmptyMessage request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_Ping, null, options, request);
      }
      public virtual global::RpcCommunicationSteamCommunity.PriceHistoryForItemResponse GetPriceHistoryForItem(global::RpcCommunicationSteamCommunity.PriceHistoryForItemRequest request, grpc::Metadata headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
      {
        return GetPriceHistoryForItem(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual global::RpcCommunicationSteamCommunity.PriceHistoryForItemResponse GetPriceHistoryForItem(global::RpcCommunicationSteamCommunity.PriceHistoryForItemRequest request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_GetPriceHistoryForItem, null, options, request);
      }
      public virtual grpc::AsyncUnaryCall<global::RpcCommunicationSteamCommunity.PriceHistoryForItemResponse> GetPriceHistoryForItemAsync(global::RpcCommunicationSteamCommunity.PriceHistoryForItemRequest request, grpc::Metadata headers = null, DateTime? deadline = null, CancellationToken cancellationToken = default(CancellationToken))
      {
        return GetPriceHistoryForItemAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual grpc::AsyncUnaryCall<global::RpcCommunicationSteamCommunity.PriceHistoryForItemResponse> GetPriceHistoryForItemAsync(global::RpcCommunicationSteamCommunity.PriceHistoryForItemRequest request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_GetPriceHistoryForItem, null, options, request);
      }
      /// <summary>Creates a new instance of client from given <c>ClientBaseConfiguration</c>.</summary>
      protected override ChatServiceClient NewInstance(ClientBaseConfiguration configuration)
      {
        return new ChatServiceClient(configuration);
      }
    }

    /// <summary>Creates service definition that can be registered with a server</summary>
    /// <param name="serviceImpl">An object implementing the server-side handling logic.</param>
    public static grpc::ServerServiceDefinition BindService(ChatServiceBase serviceImpl)
    {
      return grpc::ServerServiceDefinition.CreateBuilder()
          .AddMethod(__Method_Ping, serviceImpl.Ping)
          .AddMethod(__Method_GetPriceHistoryForItem, serviceImpl.GetPriceHistoryForItem).Build();
    }

  }
}
#endregion
