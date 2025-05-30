/*
	This file is part of the OdinMS Maple Story Server
    Copyright (C) 2008 Patrick Huy <patrick.huy@frz.cc>
		       Matthias Butz <matze@odinms.de>
		       Jan Christian Meyer <vimes@odinms.de>

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as
    published by the Free Software Foundation version 3 as published by
    the Free Software Foundation. You may not use, modify or distribute
    this program under any other version of the GNU Affero General Public
    License.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/


using Application.Utility;
using Application.Utility.Loggers;
using DotNetty.Buffers;
using Serilog;
using System.Security.Cryptography;

namespace Application.Shared.Net.Encryption;

public class MapleAESOFB
{
    private static ILogger log = LogFactory.GetLogger(LogType.MapleAESOFB);
    private static readonly byte[] skey = new byte[]
    {
        0x13, 0x00, 0x00, 0x00, 0x08, 0x00, 0x00, 0x00, 0x06, 0x00, 0x00, 0x00, 0xB4, 0x00, 0x00, 0x00,
        0x1B, 0x00, 0x00, 0x00, 0x0F, 0x00, 0x00, 0x00, 0x33, 0x00, 0x00, 0x00, 0x52, 0x00, 0x00, 0x00
    };

    private static byte[] funnyBytes = new byte[]{
             0xEC,  0x3F,  0x77,  0xA4,  0x45,  0xD0,  0x71,  0xBF,
             0xB7,  0x98,  0x20,  0xFC,  0x4B,  0xE9,  0xB3,  0xE1,
             0x5C,  0x22,  0xF7,  0x0C,  0x44,  0x1B,  0x81,  0xBD,
             0x63,  0x8D,  0xD4,  0xC3,  0xF2,  0x10,  0x19,  0xE0,
             0xFB,  0xA1,  0x6E,  0x66,  0xEA,  0xAE,  0xD6,  0xCE,
             0x06,  0x18,  0x4E,  0xEB,  0x78,  0x95,  0xDB,  0xBA,
             0xB6,  0x42,  0x7A,  0x2A,  0x83,  0x0B,  0x54,  0x67,
             0x6D,  0xE8,  0x65,  0xE7,  0x2F,  0x07,  0xF3,  0xAA,
             0x27,  0x7B,  0x85,  0xB0,  0x26,  0xFD,  0x8B,  0xA9,
             0xFA,  0xBE,  0xA8,  0xD7,  0xCB,  0xCC,  0x92,  0xDA,
             0xF9,  0x93,  0x60,  0x2D,  0xDD,  0xD2,  0xA2,  0x9B,
             0x39,  0x5F,  0x82,  0x21,  0x4C,  0x69,  0xF8,  0x31,
             0x87,  0xEE,  0x8E,  0xAD,  0x8C,  0x6A,  0xBC,  0xB5,
             0x6B,  0x59,  0x13,  0xF1,  0x04,  0x00,  0xF6,  0x5A,
             0x35,  0x79,  0x48,  0x8F,  0x15,  0xCD,  0x97,  0x57,
             0x12,  0x3E,  0x37,  0xFF,  0x9D,  0x4F,  0x51,  0xF5,
             0xA3,  0x70,  0xBB,  0x14,  0x75,  0xC2,  0xB8,  0x72,
             0xC0,  0xED,  0x7D,  0x68,  0xC9,  0x2E,  0x0D,  0x62,
             0x46,  0x17,  0x11,  0x4D,  0x6C,  0xC4,  0x7E,  0x53,
             0xC1,  0x25,  0xC7,  0x9A,  0x1C,  0x88,  0x58,  0x2C,
             0x89,  0xDC,  0x02,  0x64,  0x40,  0x01,  0x5D,  0x38,
             0xA5,  0xE2,  0xAF,  0x55,  0xD5,  0xEF,  0x1A,  0x7C,
             0xA7,  0x5B,  0xA6,  0x6F,  0x86,  0x9F,  0x73,  0xE6,
             0x0A,  0xDE,  0x2B,  0x99,  0x4A,  0x47,  0x9C,  0xDF,
             0x09,  0x76,  0x9E,  0x30,  0x0E,  0xE4,  0xB2,  0x94,
             0xA0,  0x3B,  0x34,  0x1D,  0x28,  0x0F,  0x36,  0xE3,
             0x23,  0xB4,  0x03,  0xD8,  0x90,  0xC8,  0x3C,  0xFE,
             0x5E,  0x32,  0x24,  0x50,  0x1F,  0x3A,  0x43,  0x8A,
             0x96,  0x41,  0x74,  0xAC,  0x52,  0x33,  0xF0,  0xD9,
             0x29,  0x80,  0xB1,  0x16,  0xD3,  0xAB,  0x91,  0xB9,
             0x84,  0x7F,  0x61,  0x1E,  0xCF,  0xC5,  0xD1,  0x56,
             0x3D,  0xCA,  0xF4,  0x05,  0xC6,  0xE5,  0x08,  0x49};

    private short mapleVersion;
    private ICryptoTransform cipher;
    private byte[] iv;

    byte mapleVersionHigh;
    byte mapleVersionLow;
    public MapleAESOFB(InitializationVector iv, short mapleVersion)
    {
        try
        {
            var aes = Aes.Create();
            aes.Mode = CipherMode.ECB;
            aes.Key = skey;
            cipher = aes.CreateEncryptor();
        }
        catch (Exception e)
        {
            log.Warning(e, "Cypher initialization error with skey: {Key}", skey);
            throw;
        }

        this.iv = iv.getBytes();
        this.mapleVersion = (short)(((mapleVersion >> 8) & 0xFF) | ((mapleVersion << 8) & 0xFF00));

        mapleVersionHigh = (byte)(this.mapleVersion >> 8);
        mapleVersionLow = (byte)this.mapleVersion;
    }

    private static byte[] multiplyBytes(byte[] inValue, int count, int mul)
    {
        int size = count * mul;
        byte[] ret = new byte[size];
        for (int x = 0; x < size; x++)
        {
            ret[x] = inValue[x % count];
        }
        return ret;
    }

    object cryptLock = new object();
    public byte[] crypt(byte[] data)
    {
        lock (cryptLock)
        {
            int remaining = data.Length;
            int llength = 0x5B0;
            int start = 0;
            while (remaining > 0)
            {
                byte[] myIv = multiplyBytes(this.iv, 4, 4);
                if (remaining < llength)
                {
                    llength = remaining;
                }
                for (int x = start; x < (start + llength); x++)
                {
                    if ((x - start) % myIv.Length == 0)
                    {
                        try
                        {
                            cipher.TransformBlock(myIv, 0, myIv.Length, myIv, 0);
                        }
                        catch (Exception e)
                        {
                            Log.Logger.Error(e.ToString());
                        }
                    }
                    data[x] ^= myIv[(x - start) % myIv.Length];
                }
                start += llength;
                remaining -= llength;
                llength = 0x5B4;
            }
            UpdateIV();
            return data;
        }
    }

    object updateIVLock = new object();
    private void UpdateIV()
    {
        lock (updateIVLock)
        {
            this.iv = getNewIv(this.iv);
        }
    }

    public byte[] GeneratePacketHeaderFromLength(int length)
    {
        int iiv = (iv[3]) & 0xFF;
        iiv |= (iv[2] << 8) & 0xFF00;
        iiv ^= mapleVersion;
        int mlength = ((length << 8) & 0xFF00) | (int)((uint)length >> 8);
        int xoredIv = iiv ^ mlength;
        byte[] ret = new byte[4];
        ret[0] = (byte)((uint)(iiv >> 8) & 0xFF);
        ret[1] = (byte)(iiv & 0xFF);
        ret[2] = (byte)((uint)(xoredIv >> 8) & 0xFF);
        ret[3] = (byte)(xoredIv & 0xFF);
        return ret;
    }

    public static int GetLengthFromPacketHeader(int packetHeader)
    {
        // 前16位和后16位异或
        int packetLength = (int)((uint)packetHeader >> 16) ^ (packetHeader & 0xFFFF);
        // 取后16位，并前后8位交换顺序
        packetLength = ((packetLength << 8) & 0xFF00) | (byte)((uint)packetLength >> 8);
        return packetLength;
    }

    public bool CheckPacketHeader(byte[] packet)
    {
        return ((((packet[0] ^ iv[2]) & 0xFF) == mapleVersionHigh) &&
                (((packet[1] ^ iv[3]) & 0xFF) == mapleVersionLow));
    }

    public bool CheckPacketHeader(int packetHeader)
    {
        return CheckPacketHeader([(byte)(packetHeader >> 24), (byte)(packetHeader >> 16)]);
    }
    public static byte[] getNewIv(byte[] oldIv)
    {
        byte[] inValue = { 0xf2, 0x53, 0x50, 0xc6 };
        for (int x = 0; x < 4; x++)
        {
            funnyShit(oldIv[x], inValue);
        }
        return inValue;
    }

    public override string ToString()
    {
        return "IV: " + HexTool.toHexString(this.iv);
    }

    private static byte[] funnyShit(byte inputByte, byte[] inValue)
    {
        inValue[0] += (byte)(funnyBytes[inValue[1]] - inputByte);

        inValue[1] -= (byte)(inValue[2] ^ funnyBytes[inputByte]);

        inValue[2] ^= (byte)(funnyBytes[inValue[3]] + inputByte);

        inValue[3] -= (byte)(inValue[0] - funnyBytes[inputByte]);

        // 以小端字节序转为int
        int merry = inValue[0];
        merry |= inValue[1] << 8;
        merry |= inValue[2] << 16;
        merry |= inValue[3] << 24;

        // 前29位和后3位交换位置
        int ret_value = (int)((uint)merry >> 29);
        merry = merry << 3;
        ret_value = ret_value | merry;

        inValue[0] = (byte)(ret_value);
        inValue[1] = (byte)(ret_value >> 8);
        inValue[2] = (byte)(ret_value >> 16);
        inValue[3] = (byte)(ret_value >> 24);
        return inValue;
    }

    public void Encrypt(IByteBuffer message, out byte[] data)
    {
        data = new byte[message.ReadableBytes];
        message.ReadBytes(data, 0, message.ReadableBytes);

        var buffer = Unpooled.Buffer();
        byte[] header = GeneratePacketHeaderFromLength(data.Length);
        buffer.WriteBytes(header);

        MapleCustomEncryption.encryptData(data);
        crypt(data);
        buffer.WriteBytes(data);

        data = buffer.Array;
    }

    /// <summary>
    /// 解码
    /// </summary>
    /// <param name="message"></param>
    /// <param name="data"></param>
    /// <returns>
    /// <para>1. 成功</para>
    /// <para>0. 数据包不完整</para>
    /// <para>-1. 解码失败</para>
    /// </returns>
    public int Decrypt(IByteBuffer message, out byte[] data)
    {
        data = Array.Empty<byte>();
        if (message.ReadableBytes < sizeof(int))
        {
            log.Debug($"Decode---> message.ReadableBytes = {message.ReadableBytes}");
            message.ResetReaderIndex();
            return 0;
        }

        message.MarkReaderIndex();
        int packetHeader = message.ReadInt();

        int retryCount = 20;
        while (retryCount > 0)
        {
            if (CheckPacketHeader(packetHeader))
                break;

            log.Error($"Decode---> CheckHeaderPacket fail, packetHeader = {packetHeader}, info: {this}");
            retryCount--;
            if (retryCount > 0)
                UpdateIV();
            else
            {
                log.Debug($"Decode---> Total packet: {string.Join(',', message.Array.Take(500))}");
                return -1;
            }
        }

        var packetLength = GetLengthFromPacketHeader(packetHeader);
        if (message.ReadableBytes < packetLength)
        {
            message.ResetReaderIndex();
            return 0;
        }

        byte[] decryptedPacket = new byte[packetLength];
        message.ReadBytes(decryptedPacket, 0, packetLength);

        crypt(decryptedPacket);
        MapleCustomEncryption.decryptData(decryptedPacket);

        data = decryptedPacket;
        return 1;
    }
}
