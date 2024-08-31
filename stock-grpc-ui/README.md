
```sh

# GLOBAL SETUP
npm install -g google-protobuf
npm install -g protoc-gen-js
npm install -g protoc-gen-grpc-web




# npm install @connectrpc/connect-web @connectrpc/connect
# npm install --save-dev @connectrpc/protoc-gen-connect-es

mkdir generated


# Old protoc command used for generating client - does not work for bidirectional streaming
# protoc --proto_path=../protos/ stock.proto --js_out=import_style=commonjs:./generated --grpc-web_out=import_style=typescript,mode=grpcwebtext:./generated

# This command does not work with windows?
# protoc --proto_path=../protos/ --plugin=protoc-gen-connect-es=./node_modules/.bin/protoc-gen-connect-es --connect-es_out=./generated --connect-es_opt=target=ts stock.proto




npm install -g @protobuf-ts/plugin
protoc --ts_out=./generated --proto_path=../protos/ stock.proto

npm install @protobuf-ts/grpcweb-transport
```
