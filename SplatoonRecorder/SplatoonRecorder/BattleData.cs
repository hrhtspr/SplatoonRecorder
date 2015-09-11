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
            string str = string.Join(",",Weapon,Stage,BattleType,IsTag,IsWin,Result,KaisenMikata,KaisenTeki
                ,BattleType== SplatoonRecorder.BattleType.ナワバリ?Nuri.ToString():"",Kill,Death);
            if (BattleType != SplatoonRecorder.BattleType.ナワバリ)
            {
                str +=","+ Udemae + "," + UdemaePoint;
            }
            else
            {
                str += ",,";
            }
            return str;
        }

        public const string DataLabel = "ブキ,ステージ,ルール,タッグ,勝敗,結果,自回線落ち,敵回線落ち,塗りポイント,キル,デス,ウデマエ,ポイント";
    }
    


    public enum BattleType:byte
    {
        ナワバリ,ガチエリア,ガチヤグラ,ガチホコ
    }
    
    public enum Result:byte
    {
        タイムアップ,ノックアウト,延長逆転
    }
}
