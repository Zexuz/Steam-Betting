protoc.exe -I ./ --csharp_out ./ProtoLib --grpc_out ./ProtoLib  ./protofiles/messages.proto --plugin=protoc-gen-grpc=grpc_csharp_plugin.exe
protoc.exe -I ./ --csharp_out ./ProtoLib --grpc_out ./ProtoLib  ./protofiles/chat.proto --plugin=protoc-gen-grpc=grpc_csharp_plugin.exe
protoc.exe -I ./ --csharp_out ./ProtoLib --grpc_out ./ProtoLib  ./protofiles/ticket.proto --plugin=protoc-gen-grpc=grpc_csharp_plugin.exe
protoc.exe -I ./ --csharp_out ./ProtoLib --grpc_out ./ProtoLib  ./ProtoLib/Discord.proto --plugin=protoc-gen-grpc=grpc_csharp_plugin.exe
protoc.exe -I ./ --csharp_out ./ProtoLib --grpc_out ./ProtoLib  ./ProtoLib/BettingHistory.proto --plugin=protoc-gen-grpc=grpc_csharp_plugin.exe
protoc.exe -I ./ --csharp_out ./ProtoLib --grpc_out ./ProtoLib  ./protofiles/steamCommunity.proto --plugin=protoc-gen-grpc=grpc_csharp_plugin.exe

exit