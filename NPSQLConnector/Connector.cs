using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace NPSQLConnector
{
    public class Connector
    {
        public string HostName {get; set;}
        public int Port {get; set;}

        public string UserName {get; set;}

        public string Password {get; set;}

        public Socket ClientSocket {get; set;} // Consıder usıng a TcpClient

        public string InitialCatalog {get; set;}
        
        // Socket Connection
        public bool Connect() 
        {
            ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ClientSocket.Connect(HostName, Port);
            return true;
        }

        // WriteStartupMessage
        public Span<byte> Startup() 
        {
            // Kv pairs of parameters
            // Length - Protocol Version - k0 0 v0 0 k1 0 v1 0 ... 0

            Dictionary<string, string> kvPairs = new Dictionary<string, string>();

            kvPairs["client_encoding"] = "UTF8";
            kvPairs["user"] = this.UserName;
            kvPairs["database"] = this.InitialCatalog;
            // Calculate total length
            const int protocolVersion3 = 3 << 16; // 196608

            var len = sizeof(int) + sizeof(int) + sizeof(byte);

            foreach(var pair in kvPairs)
            {
                len += Encoding.UTF8.GetByteCount(pair.Key) + 1 +
                    Encoding.UTF8.GetByteCount(pair.Value) + 1;
            }

            // Implement a Endian-aware int32 writer and call it
            WriteBuffer wb = new WriteBuffer();
            wb.WriteInt32(len);
            wb.WriteInt32(protocolVersion3);
            foreach(var pair in kvPairs)
            {
                wb.WriteString(pair.Key);
                wb.WriteByte(0); // "\0"
                wb.WriteString(pair.Value);
                wb.WriteByte(0); // "\0"
            }
            wb.WriteByte(0);

            ClientSocket.Send(wb.FinalizeBuffer());
            
            var buffer = new byte[4096];
            var received = ClientSocket.Receive(buffer);

            Span<byte> receivedMessage = new Span<byte>(buffer);
            if(BinaryPrimitives.ReadInt32BigEndian(receivedMessage.Slice(5, 4)) == 5) 
            {
                return receivedMessage.Slice(9, 4);
            }
            return Span<byte>.Empty;
        }

        // AuthenticationCleartext
        public bool AuthClearText() 
        {
            var encoded = new byte[Encoding.UTF8.GetByteCount(Password) + 1];
            Encoding.UTF8.GetBytes(Password, 0, Password.Length, encoded, 0);

            WriteBuffer wb = new WriteBuffer();
            wb.WriteByteArray(Encoding.UTF8.GetBytes("p"));
            var len = encoded.Length + sizeof(int);
            wb.WriteInt32(len);
            wb.WriteByteArray(encoded);
            ClientSocket.Send(wb.FinalizeBuffer());
            
            var buffer = new byte[4096];
            var received = ClientSocket.Receive(buffer);

            return true;
        }

         public void AuthMD5(Span<byte> nonce) 
        {
            MD5 instance = MD5.Create();

            var notDigested = instance.ComputeHash(Encoding.UTF8.GetBytes(Password+UserName));
            StringBuilder sb = new StringBuilder();
            foreach(var token in notDigested)
                sb.Append(token.ToString("x2"));

            var hashed = Encoding.UTF8.GetBytes(sb.ToString());

            Span<byte> hashNonce = new Span<byte>(new byte[hashed.Length+nonce.Length]);
            hashed.CopyTo(hashNonce.Slice(0, hashed.Length));
            nonce.CopyTo(hashNonce.Slice(hashed.Length, nonce.Length));
            
            notDigested = instance.ComputeHash(hashNonce.ToArray());
            sb.Clear();
            foreach(var token in notDigested)
                sb.Append(token.ToString("x2"));
              
            hashed = Encoding.UTF8.GetBytes(sb.ToString());
            
            Span<byte> hashedSpan = new Span<byte>(new byte[hashed.Length+3]);
            Encoding.UTF8.GetBytes("md5").AsSpan().CopyTo(hashedSpan.Slice(0, 3));

            hashed.AsSpan().CopyTo(hashedSpan.Slice(3, hashed.Length));

            WriteBuffer wb = new WriteBuffer();
            wb.WriteByteArray(Encoding.UTF8.GetBytes("p"));
            var len = hashedSpan.Length + sizeof(int) + sizeof(byte);
            wb.WriteInt32(len);
            wb.WriteByteArray(hashedSpan.ToArray());
            wb.WriteByte(0);
            ClientSocket.Send(wb.FinalizeBuffer());
            
            var buffer = new byte[4096];
            var received = ClientSocket.Receive(buffer);
        }

        public string SendSimpleQuery(string sql) {
            var sentByteArray = Encoding.UTF8.GetBytes(sql);

            WriteBuffer wb = new WriteBuffer();
            wb.WriteByteArray(Encoding.UTF8.GetBytes("Q"));
            wb.WriteInt32(sentByteArray.Length + sizeof(int) + sizeof(byte));
            wb.WriteByteArray(sentByteArray);
            wb.WriteByte(0);
            var sentBytes = ClientSocket.Send(wb.FinalizeBuffer());

            var buffer = new byte[4096];
            var received = ClientSocket.Receive(buffer);
            // RowDescriptors and DataRows
            return Encoding.UTF8.GetString(buffer);
        }
    }
}
