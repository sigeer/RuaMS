using Application.Core;
using Application.Shared.Constants;
using Application.Shared.Net;
using Application.Shared.Net.Encryption;
using Application.Utility;
using DotNetty.Buffers;
using Serilog;
using Serilog.Events;
using System.Text;
using tools;

namespace ServiceTest.Infrastructure
{
    public class MapleAESOFBTests
    {
        public MapleAESOFBTests()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Quartz", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();
        }

        [TestCase("TEST", ExpectedResult = "103,-1,-45,43")]
        [Test]
        public string Crypto_Test(string inputString)
        {
            var senderCypher = new MapleAESOFB(InitializationVector.generateSend(), ServerConstants.VERSION);

            var bytes = GlobalVariable.Encoding.GetBytes(inputString);
            Console.WriteLine(string.Join(',', bytes));

            MapleCustomEncryption.encryptData(bytes);
            var result = senderCypher.crypt(bytes);

            var strOutput = string.Join(',', result.Cast<sbyte>());
            Console.WriteLine(strOutput);
            return strOutput;
        }

        [TestCase("103,-1,-45,43", ExpectedResult = "25,-81,-115,-48,-80,-99,-34,62,124,54,36,94")]
        [Test]
        public string Decrypt_Test(string byteString)
        {
            // (byte)(Randomizer.nextDouble() * 255)
            var recvCypher = new MapleAESOFB(InitializationVector.generateReceive(), ServerConstants.VERSION);

            string output = string.Empty;
            foreach (var item in Enumerable.Range(0, 3))
            {
                var sbytes = byteString.Split(',').Select(x => sbyte.Parse(x)).ToArray();

                var bytes = sbytes.Cast<byte>().ToArray();
                var decrypted = recvCypher.crypt(bytes);
                MapleCustomEncryption.decryptData(bytes);

                var strOutput = string.Join(',', bytes.Cast<sbyte>());
                Console.WriteLine(strOutput);

                output += strOutput + ",";
            }
            return output.Substring(0, output.Length - 1);
        }

        [TestCase("TEST")]
        [Test]
        public void CryptAndDecrypt_Test(string inputString)
        {
            var iv = InitializationVector.generateReceive();
            var mockClientSenderCypher = new MapleAESOFB(iv, ServerConstants.VERSION);

            var bytes = PacketCreator.serverNotice(6, inputString).getBytes();
            Console.WriteLine("before crypt: " + string.Join(',', bytes));

            mockClientSenderCypher.Encrypt(Unpooled.WrappedBuffer(bytes), out var ret);
            Console.WriteLine("after crypt: " + string.Join(',', ret));

            var recvCypher = new MapleAESOFB(iv, ServerConstants.VERSION);
            var stats = recvCypher.Decrypt(Unpooled.WrappedBuffer(ret), out var decryptedData);
            // Console.WriteLine(stats + ": " + string.Join(',', decryptedData));
            if (stats == 1)
                Console.WriteLine("after decrypt: " + string.Join(',', decryptedData));
            Assert.That(stats == 1);
            Assert.That(string.Join(',', decryptedData) == string.Join(',', bytes));
        }

        [TestCase(704064503, ExpectedResult = false)]
        [Test]
        public bool HeaderValid_Test(int input)
        {
            var senderCypher = new MapleAESOFB(InitializationVector.generateSend(), (short)(0xFFFF - ServerConstants.VERSION));
            return senderCypher.CheckPacketHeader(input);
        }

        [TestCase(704064503, ExpectedResult = 2)]
        [Test]
        public int getPacketLength_Test(int header)
        {
            var len = MapleAESOFB.GetLengthFromPacketHeader(header);
            Console.WriteLine(len);
            return len;
        }

        [Test]
        public void HelloPacket_Test()
        {
            var p1 = new byte[4];
            var sender = InitializationVector.generateSend();
            var recv = InitializationVector.generateReceive();
            var packet = PacketCommon.getHello(ServerConstants.VERSION, sender, recv);
            var helloBytes = packet.ToString();
            Console.WriteLine(helloBytes);
            Assert.Pass();
        }
    }
}
