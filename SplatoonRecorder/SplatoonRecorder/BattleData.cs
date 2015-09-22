using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SplatoonRecorder
{
    public struct BattleData
    {
        public string Weapon;
        public string Stage;
        public BattleType BattleType;
        public bool IsTag;
        public Result Result;
        public bool KaisenMikata;
        public bool KaisenTeki;
        public bool IsWin;
        public short Nuri;
        public sbyte Kill;
        public sbyte Death;
        public String Udemae;
        public sbyte UdemaePoint;
        public override string ToString()
        {
            string str = string.Join(",", Weapon, Stage, BattleType, IsTag, IsWin, Result, KaisenMikata, KaisenTeki
                , BattleType == SplatoonRecorder.BattleType.ナワバリ ? Nuri.ToString() : "", Kill, Death);
            if (BattleType != SplatoonRecorder.BattleType.ナワバリ&&BattleType!= SplatoonRecorder.BattleType.フェス)
            {
                str += "," + Udemae + "," + UdemaePoint;
            }
            else
            {
                str += ",,";
            }
            return str;
        }
        public static bool TryReadString(string str, out BattleData data)
        {
            var d = str.Split(',');
            data = new BattleData() { Weapon = d[0], Stage = d[1] };
            BattleType type;
            if (Enum.TryParse<BattleType>(d[2], out type))
            {
                data.BattleType = type;
            }
            else
            {
                return false;
            }
            bool b;
            if (bool.TryParse(d[3], out b))
            {
                data.IsTag = b;
            }
            else
            {
                return false;
            }
            if (bool.TryParse(d[4], out b))
            {
                data.IsWin = b;
            }
            else
            {
                return false;
            }
            Result result;
            if (Enum.TryParse<Result>(d[5], out result))
            {
                data.Result = result;
            }
            else
            {
                return false;
            }
            if (bool.TryParse(d[6], out b))
            {
                data.KaisenMikata = b;
            }
            else
            {
                return false;
            }
            if (bool.TryParse(d[7], out b))
            {
                data.KaisenTeki = b;
            }
            else
            {
                return false;
            }
            short s;
            if (short.TryParse(d[8], out s)|| data.BattleType != BattleType.ナワバリ)
            {
                data.Nuri = data.BattleType == BattleType.ナワバリ?s:(short)-1;
            }
            else
            {
                return false;
            }
            sbyte sb;
            if (sbyte.TryParse(d[9], out sb)||d[9]=="")
            {
                data.Kill = d[9] != "" ? sb : (sbyte)-1;
            }
            else
            {
                return false;
            }
            if (sbyte.TryParse(d[10], out sb) || d[10] == "")
            {
                data.Death = d[10] != "" ? sb : (sbyte)-1;
            }
            else
            {
                return false;
            }
            if (data.BattleType == SplatoonRecorder.BattleType.ナワバリ || data.BattleType == SplatoonRecorder.BattleType.フェス)
            {
                data.Udemae = "";
                data.UdemaePoint = (sbyte)-1;
            }
            else
            {
                data.Udemae = d[11];
                if (sbyte.TryParse(d[12], out sb))
                {
                    data.UdemaePoint =  sb;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        public const string DataLabel = "ブキ,ステージ,ルール,タッグ,勝敗,結果,自回線落ち,敵回線落ち,塗りポイント,キル,デス,ウデマエ,ポイント";
    }



    public enum BattleType : byte
    {
        ナワバリ, フェス, ガチエリア, ガチヤグラ, ガチホコ
    }

    public enum Result : byte
    {
        タイムアップ, ノックアウト, 延長逆転
    }
}
