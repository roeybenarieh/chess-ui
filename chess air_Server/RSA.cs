using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using chess_air_Server;

namespace chess
{
    public class RSA
    {
        private string my_privateKey;
        private string other_computer_publicKey;
        private UnicodeEncoding _encoder;
        private RSACryptoServiceProvider _rsa;
        private ManageClient mc;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mc"></param>
        public RSA(ManageClient mc)
        {
            this.mc = mc;
            _encoder = new UnicodeEncoding();
            _rsa = new RSACryptoServiceProvider(2048);

            my_privateKey = _rsa.ToXmlString(true);
            other_computer_publicKey = _rsa.ToXmlString(false);
            mc.SendMessage("public key:" + GetPublicKey(), incripted: false);
            mc._client.GetStream().BeginRead(mc.data,
                                                 0,
                                                 System.Convert.ToInt32(mc._client.ReceiveBufferSize),
                                                 ReceiveMessage,
                                                 null);
        }
        /// <summary>
        /// return PrivateKey
        /// </summary>
        /// <returns>PrivateKey</returns>
        public string GetPrivateKey()
        {
            return this.my_privateKey;
        }
        public void setPrivateKey(string key)
        {
            this.my_privateKey = key;
        }
        /// <summary>
        /// return PublicKey
        /// </summary>
        /// <returns>PublicKey</returns>
        public string GetPublicKey()
        {
            return this.other_computer_publicKey;
        }
        public void setPublicKey(string key)
        {
            this.other_computer_publicKey = key;
        }
        /// <summary>
        /// decript data by privateKey
        /// </summary>
        /// <param name="data">data to decript</param>
        /// /// <param name="privateKey">privateKey</param>
        /// <returns>decripted data</returns>
        public string Decrypt(string data)
        {

            var dataArray = data.Split(new char[] { ',' });
            byte[] dataByte = new byte[dataArray.Length];
            for (int i = 0; i < dataArray.Length; i++)
            {
                dataByte[i] = Convert.ToByte(dataArray[i]);
            }

            _rsa.FromXmlString(my_privateKey);
            var decryptedByte = _rsa.Decrypt(dataByte, false);
            return _encoder.GetString(decryptedByte);
        }
        /// <summary>
        /// Encrypt the data by public key
        /// </summary>
        /// <param name="data">data to encrypt</param>
        /// <param name="publicKey"></param>
        /// <returns>encripted data</returns>
        public string Encrypt(string data)
        {
            var rsa = new RSACryptoServiceProvider(2048);
            rsa.FromXmlString(other_computer_publicKey);
            var dataToEncrypt = _encoder.GetBytes(data);
            var encryptedByteArray = rsa.Encrypt(dataToEncrypt, false);
            var length = encryptedByteArray.Length;
            var item = 0;
            var sb = new StringBuilder();
            foreach (var x in encryptedByteArray)
            {
                item++;
                sb.Append(x);

                if (item < length)
                    sb.Append(",");
            }

            return sb.ToString();
        }
        /// <summary>
        /// recieve and handel incomming streem 
        /// Asynchrom
        /// </summary>
        /// <param name="ar"></param>
        private void ReceiveMessage(IAsyncResult ar)
        {
            int bytesRead;
            try
            {
                lock (mc._client.GetStream())
                {
                    // call EndRead to handle the end of an async read.
                    bytesRead = mc._client.GetStream().EndRead(ar);
                }
                // if bytesread<1 -> the client disconnected
                string messageReceived = System.Text.Encoding.ASCII.GetString(mc.data, 0, bytesRead); //the recieved message in string 

                if (bytesRead < 1)
                {
                    return;
                }
                else
                {
                    // invoke the delegate to display the recived data
                    string textFromServer = System.Text.Encoding.ASCII.GetString(mc.data, 0, bytesRead);

                    if (textFromServer.StartsWith("public key:"))
                    {
                        setPublicKey(textFromServer.Remove(0, 11));
                        Console.WriteLine("RSA initialized for the new connection");
                        return;
                    }
                }
            }
            catch (Exception)
            {
                // ignor the error... fired when the user loggs off
            }
        }

    }
}
