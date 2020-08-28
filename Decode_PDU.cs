using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace esp8266_smsResponse
{
    /*
     * 09			//短信中心号码长度
     * 91			//国际号码“+”
     * 58 62 54 20 01 00 01 F3	//号码
     * 24			//SMS_DELIVER的第一个八位
     * 0B			//发送方地址长度 11
     * D0
     * C7 F7 FB CC 2E 03		//7-bit编码Unicode"Google"
     * 00 			//TP-PID：普通GSM模式
     * 08			//UCS2编码
     * 02 80 70 22 80 93 23 	//接收日期、时间
     * 30			//16进制，代表文本长度
     * 0047002D0039003000390038003900310020662F60A8768400200047006F006F0067006C006500209A8C8BC178013002
     */
    public class Decode_PDU
    {
        /// <summary>
        /// 计算字符串的sha1哈希码
        /// </summary>
        /// <param name="Source_String">源字符串</param>
        /// <returns>对应字符串的sha1码</returns>
        public string SHA1_Encrypt(string Source_String)
        {
            byte[] StrRes = Encoding.Default.GetBytes(Source_String);
            HashAlgorithm iSHA = new SHA1CryptoServiceProvider();
            StrRes = iSHA.ComputeHash(StrRes);
            StringBuilder EnText = new StringBuilder();
            foreach (byte iByte in StrRes)
            {
                EnText.AppendFormat("{0:x2}", iByte);
                string rmp = Convert.ToString(iByte, 16);
            }
            return EnText.ToString().ToUpper();
        }
        /// <summary>
        /// PDU短信编码解码
        /// </summary>
        /// <param name="raw">PDU编码</param>
        /// <returns>短信内容字典</returns>
        public Dictionary<string, string> PduTosmsData(string raw)
        {
            int index = 0;
            raw = raw.Replace(" ", "").Replace("\"", "");
            //获取SMSC长度
            int SMSC_Length = StringToNumber(raw.Substring(index, 2)) * 2 - 2;
            index += 2;
            //构建SMSC号码
            StringBuilder SMSC_Builder = new StringBuilder();
            switch (raw.Substring(index, 2))
            {
                case "91":
                    SMSC_Builder.Append("+");
                    break;
                case "81":
                default:
                    return null;
            }
            index += 2;
            SMSC_Builder.Append(HexToString(raw.Substring(index, SMSC_Length)));
            string SMSC = SMSC_Builder.ToString();
            index += SMSC_Length;
            //跳过hex 24
            index += 2;
            //获取Sender_Length长度
            int Sender_Length = HexToDec(raw.Substring(index, 2));
            Sender_Length = Sender_Length % 2 == 0 ? Sender_Length : Sender_Length + 1;
            index += 2;
            //构建Sender
            StringBuilder Sender_Builder = new StringBuilder();
            switch (raw.Substring(index, 2))
            {
                case "91":
                    index += 2;
                    Sender_Builder.Append("+");
                    Sender_Builder.Append(HexToString(raw.Substring(index, Sender_Length)));
                    break;
                case "81":
                    index += 2;
                    Sender_Builder.Append(HexToString(raw.Substring(index, Sender_Length)));
                    break;
                case "D0":
                    index += 2;
                    Sender_Builder.Append(Bit_7ToString(raw.Substring(index, Sender_Length)));
                    break;
                default:
                    return null;
            }
            index += Sender_Length;
            //TP-PID
            string TP_PID = raw.Substring(index, 2);
            index += 2;
            //Data编码格式
            string CODE_TYPE = raw.Substring(index, 2);
            /*
             * "00":  //7bit编码
             * "04":  //8bit编码
             * "08":  //UCS2编码
             */
            //接收日期
            index += 2;
            string date_tmp = HexToString(raw.Substring(index, 14));
            StringBuilder date_format = new StringBuilder();
            for (int i = 0; i < 6; i += 2)
            {
                date_format.Append(date_tmp[i]).Append(date_tmp[i + 1]).Append("-");
            }
            date_format.Remove(date_format.Length - 1, 1).Append(" ");
            for (int i = 6; i < 12; i += 2)
            {
                date_format.Append(date_tmp[i]).Append(date_tmp[i + 1]).Append(":");
            }
            date_format.Remove(date_format.Length - 1, 1);
            index += 14;
            //内容长度HEX
            int data_Length = HexToDec(raw.Substring(index, 2)) * 2;
            index += 2;
            //内容正文
            string data = "";
            switch (CODE_TYPE)
            {
                case "00":  //7bit编码
                    data_Length /= 2;
                    data_Length = (data_Length % 8 + data_Length / 8 * 7) * 2;
                    data = Bit_7ToString(raw.Substring(index, data_Length));
                    break;
                case "04":  //8bit编码
                    break;
                case "08":   //UCS2编码
                    data = UnicodeToString(raw.Substring(index, data_Length));
                    break;
                default:
                    return null;
            }
            Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
            keyValuePairs.Add("from", SMSC);
            keyValuePairs.Add("date", date_format.ToString());
            keyValuePairs.Add("data", data);
            keyValuePairs.Add("hash", SHA1_Encrypt(SMSC + date_format.ToString() + data));
            keyValuePairs.Add("raw", raw);
            return keyValuePairs;
        }
        /// <summary>
        /// 将倒置的号码正序排列
        /// </summary>
        /// <param name="hex">hex编码字符串</param>
        /// <returns>正序字符串</returns>
        public string HexToString(string hex)
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < hex.Length; i += 2)
            {
                stringBuilder.Append(hex[i + 1]);
                stringBuilder.Append(hex[i]);
            }
            if (stringBuilder[stringBuilder.Length - 1] == 'F' || stringBuilder[stringBuilder.Length - 1] == 'f')
                stringBuilder.Remove(stringBuilder.Length - 1, 1);
            return stringBuilder.ToString();
        }
        /// <summary>
        /// 7-bit编码字符转换为可读字符串
        /// </summary>
        /// <param name="bit">7-bit字符串</param>
        /// <returns>可读字符串</returns>
        public string Bit_7ToString(string bit)
        {
            StringBuilder bitString = new StringBuilder();
            for (int i = bit.Length - 1; i > 0; i -= 2)
            {
                char tmpH = bit[i - 1];
                char tmpL = bit[i];
                bitString.Append(HexToBit(tmpH));
                bitString.Append(HexToBit(tmpL));
            }
            // bitString.Remove(0, bit.Length / 2);
            bitString.Remove(0, bit.Length / 2 % 8);

            StringBuilder decodeucd = new StringBuilder();
            string bitstring = bitString.ToString();
            for (int i = 0; i < bitstring.Length; i += 7)
            {
                char tmpH = BitToHex("0" + bitstring.Substring(i, 3));
                char tmpL = BitToHex(bitstring.Substring(i + 3, 4));
                decodeucd.Insert(0, UnicodeToString("00" + tmpH + tmpL));
            }
            return decodeucd.ToString();
        }
        /// <summary>
        /// unicode编码字符串转换为可读字符串
        /// </summary>
        /// <param name="unicode">unicode编码字符串</param>
        /// <returns>可读字符串</returns>
        public string UnicodeToString(string unicode)
        {
            StringBuilder ucdstr = new StringBuilder();
            for (int i = 0; i < unicode.Length; i += 4)
            {
                byte[] codes = new byte[2];
                codes[0] = (byte)Convert.ToInt32("" + unicode[i + 2] + unicode[i + 3], 16);
                codes[1] = (byte)Convert.ToInt32("" + unicode[i] + unicode[i + 1], 16);
                ucdstr.Append(Encoding.Unicode.GetString(codes));
            }
            return ucdstr.ToString();
        }
        /// <summary>
        /// 将二进制编码转换为十六进制编码
        /// </summary>
        /// <param name="bit">二进制 e.p.1010</param>
        /// <returns>返回十六进制 e.p. A</returns>
        public char BitToHex(string bit)
        {
            int dec = 0;
            foreach (char chr in bit)
            {
                dec = dec * 2 + (chr - '0');
            }
            char hex = (char)(dec > 9 ? 'A' + (dec - 10) : '0' + (dec));
            return hex;
        }
        /// <summary>
        /// 将字符串转换为数字
        /// </summary>
        /// <param name="str">源字符串</param>
        /// <returns>整数</returns>
        public int StringToNumber(string str)
        {
            int num = 0;
            foreach (char chr in str)
            {
                num = num * 10 + (chr - '0');
            }
            return num;
        }
        /// <summary>
        /// 将十六进制转换位十进制
        /// </summary>
        /// <param name="hex">十六进制形式的字符串</param>
        /// <returns>十进制整数</returns>
        public int HexToDec(string hex)
        {
            int num = 0;
            for (int i = 0; i < hex.Length; i++)
            {
                if (hex[i] >= '0' && hex[i] <= '9')
                    num += (hex[i] - '0') * Pow(16, hex.Length - i - 1);
                else if (hex[i] >= 'A' && hex[i] <= 'F')
                    num += (hex[i] - 'A' + 10) * Pow(16, hex.Length - i - 1);
            }
            return num;
        }
        /// <summary>
        /// 十六进制转换为十进制
        /// </summary>
        /// <param name="hex">十六进制字符串</param>
        /// <returns>十进制整数</returns>
        public int HexToDec(char hex)
        {
            int num = 0;
            if (hex >= '0' && hex <= '9')
                num += (hex - '0');
            else if (hex >= 'A' && hex <= 'F')
                num += (hex - 'A' + 10);
            return num;
        }
        /// <summary>
        /// 将十六进制编码转换为二进制编码
        /// </summary>
        /// <param name="hex">十六进制码 A </param>
        /// <returns>二进制编码 1010</returns>
        public string HexToBit(char hex)
        {
            StringBuilder bit = new StringBuilder();
            int tmp = 0;
            if (hex >= '0' && hex <= '9')
            {
                tmp = hex - '0';
            }
            else if (hex >= 'A' && hex <= 'F')
            {
                tmp = HexToDec(hex);
            }
            while (tmp > 0)
            {
                bit.Insert(0, tmp % 2);
                tmp /= 2;
            }
            while (bit.Length != 4)
            {
                bit.Insert(0, "0");
            }
            return bit.ToString();
        }
        /// <summary>
        /// 计算指数幂
        /// </summary>
        /// <param name="e">底数</param>
        /// <param name="i">指数</param>
        /// <returns>幂</returns>
        public int Pow(int e, int i)
        {
            int pw = 1;
            for (; i > 0; i--)
            {
                pw *= e;
            }
            return pw;
        }
    }
}