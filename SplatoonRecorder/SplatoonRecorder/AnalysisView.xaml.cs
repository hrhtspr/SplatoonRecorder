using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SplatoonRecorder
{
    /// <summary>
    /// AnalysisView.xaml の相互作用ロジック
    /// </summary>
    public partial class AnalysisView : Window
    {
        List<BattleData> Datas;
        List<BattleData> NowView;
        public AnalysisView(string name)
        {
            InitializeComponent();
            var bd = new Binding() { Source = MainWindow.Names };
            this.name.SetBinding(ComboBox.ItemsSourceProperty, bd);
            this.name.Text = name;
            Datas = new List<BattleData>();
            NowView = Datas.ToList();
            viewCount.ItemsSource = Enum.GetValues(typeof(ViewCount));
            viewCount.SelectedIndex = 0;
            tab.SelectedIndex = 0;
            var cv = new WinRatioConverter();
            winratiolist.Columns.Clear();
            winratiolist.Columns.Add(new DataGridTextColumn() { Binding = new Binding("Weapon") });
            foreach (var item in MainWindow.Stages.Concat(new[]{"すべて"}))
            {
                var mb = new MultiBinding() { Converter=cv};
                mb.Bindings.Add(new Binding("Datas[" + item + "].WinRatio"));
                mb.Bindings.Add(new Binding("Datas[" + item + "].Count"));
                winratiolist.Columns.Add(new DataGridTextColumn() { Header = item,Binding = mb });
                
            }
            ReadData();

        }

        public void ReadData()
        {

            var dataFileName = this.name.Text + ".csv";
            if (!File.Exists(dataFileName))
            {
                message.Content = "記録ファイルがありません";
                this.DataContext = null;
                return;
            }
            Datas.Clear();
            using (var sr = new StreamReader(dataFileName, Encoding.UTF8))
            {
                while (!sr.EndOfStream)
                {
                    var str = sr.ReadLine();
                    BattleData data;
                    if (BattleData.TryReadString(str, out data))
                    {
                        Datas.Add(data);
                    }
                }
            }
            NowView = Datas.ToList();

            weapon.ItemsSource = new[] { "すべて" }.Concat(MainWindow.Weapons.Intersect(Datas.Select(p => p.Weapon)));
            var r = Enum.GetNames(typeof(BattleType)).Intersect(Datas.Select(p => p.BattleType.ToString())).ToList();
            var i = r.FindIndex(p => p.StartsWith("ガチ"));
            if (i >= 0)
            {
                r.Insert(i, "ガチマッチ");
            }
            this.rule.ItemsSource = new[] { "すべて" }.Concat(r);
            stage.ItemsSource = new[] { "すべて" }.Concat(MainWindow.Stages.Intersect(Datas.Select(p => p.Stage)));
            weapon.SelectedIndex = rule.SelectedIndex = stage.SelectedIndex = 0;
            Reload();
        }

        public void Analysis()
        {
            var ad = new AnalysisData();
            var f = Datas.FindIndex(p => p.Kill >= 0 && p.Death >= 0);
            if (f >= 0)
            {
                ad.MaxKill = ad.MinKill = Datas[f].Kill;
                ad.MaxDeath = ad.MinDeath = Datas[f].Death;
                ad.MaxKillRatio = ad.MinKillRatio = Datas[f].Death > 0 ? 1.0 * Datas[f].Kill / Datas[f].Death : Datas[f].Kill;
            }
            f = Datas.FindIndex(p => p.BattleType == BattleType.ナワバリ);
            List<double> killdeaths = new List<double>();
            if (f >= 0)
            {
                ad.MaxNuri = ad.MinNuri = Datas[f].Nuri;
            }
            foreach (var data in NowView)
            {
                if (data.IsWin)
                {
                    ad.TotalWin++;
                }
                else
                {
                    ad.TotalLose++;
                }

                if (data.Kill >= 0 && data.Death >= 0)
                {
                    ad.KillDeathCount++;
                    ad.TotalKill += data.Kill;
                    ad.TotalDeath += data.Death;
                    ad.MaxKill = Math.Max(ad.MaxKill, data.Kill);
                    ad.MinKill = Math.Min(ad.MinKill, data.Kill);
                    ad.MaxDeath = Math.Max(ad.MaxDeath, data.Death);
                    ad.MinDeath = Math.Min(ad.MinDeath, data.Death);
                    var kilre = data.Death > 0 ? 1.0 * data.Kill / data.Death : data.Kill;
                    ad.MaxKillRatio = Math.Max(ad.MaxKillRatio, kilre);
                    ad.MinKillRatio = Math.Min(ad.MinKillRatio, kilre);
                    killdeaths.Add(kilre);
                }

                if (data.BattleType == BattleType.ナワバリ)
                {
                    ad.NawabariCount++;
                    ad.TotalNuri += data.Nuri;
                    ad.MaxNuri = Math.Max(ad.MaxNuri, data.Nuri);
                    ad.MinNuri = Math.Min(ad.MinNuri, data.Nuri);
                }
            }
            if (ad.KillDeathCount > 0)
            {
                killdeaths = killdeaths.OrderBy(p => p).ToList();
                if (killdeaths.Count % 2 == 0)
                {
                    ad.MedianKillRatio = (killdeaths[killdeaths.Count / 2] + killdeaths[killdeaths.Count / 2 - 1]) * 0.5;
                }
                else
                {
                    ad.MedianKillRatio = killdeaths[(killdeaths.Count - 1) / 2];
                }

                this.killdeath.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                this.killdeath.Visibility = System.Windows.Visibility.Collapsed;
            }

            if (ad.NawabariCount > 0)
            {
                this.nawabari.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                this.nawabari.Visibility = System.Windows.Visibility.Collapsed;
            }

            this.DataContext = ad;
        }
        public void CalculateWinRatios()
        {
            var ru = (string)rule.SelectedValue;
            var datas = Datas.Where(p =>
            {
                var re = true;
                if (ru != "すべて")
                {
                    if (ru != "ガチマッチ")
                    {
                        re &= p.BattleType.ToString() == ru;
                    }
                    else
                    {
                        re &= p.BattleType.ToString().StartsWith("ガチ");
                    }
                }
                return re;
            }).Select(p => new { p.Weapon, p.IsWin, p.Stage, p.BattleType }).ToList();
            var weps = datas.GroupBy(p => p.Weapon).ToDictionary(p => p.Key, p => p.ToList());
            weps.Add("すべて", datas);
            var westg = weps.Select(p =>
            {
                var t = p.Value.GroupBy(q => q.Stage).ToDictionary(q => q.Key, q => q.ToList());
                t.Add("すべて", p.Value);
                var winratios = t.Select(q =>
                {
                    var re = q.Value.ToList();
                    switch ((ViewCount)viewCount.SelectedItem)
                    {
                        case ViewCount.最近30試合:
                            if (re.Count > 30)
                            {
                                re.RemoveRange(0, re.Count - 30);
                            }
                            break;
                        case ViewCount.最近50試合:
                            if (re.Count > 50)
                            {
                                re.RemoveRange(0, re.Count - 50);
                            }
                            break;
                        case ViewCount.最近100試合:
                            if (re.Count > 100)
                            {
                                re.RemoveRange(0, re.Count - 100);
                            }
                            break;
                    }
                    return new { Stage = q.Key, WinRatio = 1.0 * re.Count(r => r.IsWin) / re.Count,Count=re.Count };
                }).ToList();
                foreach (var item in MainWindow.Stages)
                {
                    if (winratios.All(q => q.Stage != item))
                    {
                        winratios.Add(new { Stage = item, WinRatio = double.NaN,Count=0 });
                    }
                }
                return new { Weapon = p.Key, Datas = winratios.ToDictionary(q => q.Stage, q => new { q.WinRatio,q.Count }) };
            });
            winratiolist.ItemsSource = westg;
        }

        
        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            ReadData();
        }

        private void SelectionChangedAbst()
        {
            var wep = (string)weapon.SelectedValue;
            var ru = (string)rule.SelectedValue;
            var stg = (string)stage.SelectedValue;
            NowView = Datas.Where(p =>
            {
                bool re = true;
                if (wep != "すべて")
                {
                    re &= p.Weapon == wep;
                }
                if (ru != "すべて")
                {
                    if (ru != "ガチマッチ")
                    {
                        re &= p.BattleType.ToString() == ru;
                    }
                    else
                    {
                        re &= p.BattleType.ToString().StartsWith("ガチ");
                    }
                }
                if (stg != "すべて")
                {
                    re &= p.Stage == stg;
                }
                return re;
            }).ToList();

            switch ((ViewCount)viewCount.SelectedItem)
            {
                case ViewCount.最近30試合:
                    if (NowView.Count > 30)
                    {
                        NowView.RemoveRange(0, NowView.Count - 30);
                    }
                    break;
                case ViewCount.最近50試合:
                    if (NowView.Count > 50)
                    {
                        NowView.RemoveRange(0, NowView.Count - 50);
                    }
                    break;
                case ViewCount.最近100試合:
                    if (NowView.Count > 100)
                    {
                        NowView.RemoveRange(0, NowView.Count - 100);
                    }
                    break;
            }
            if (weapon.SelectedIndex == 0 && rule.SelectedIndex == 0 && stage.SelectedIndex == 0 && viewCount.SelectedIndex == 0)
            {
                message.Content = "全" + Datas.Count + "件";
            }
            else
            {
                message.Content = "全" + Datas.Count + "件中" + NowView.Count + "件";
            }
            Analysis();
        }
        public void Reload()
        {
            if (tab.SelectedIndex == 0)
            {
                SelectionChangedAbst();
            }
            else if (tab.SelectedIndex == 1)
            {
                CalculateWinRatios();
            }
        }

        private void tab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Reload();
        }

        private void viewCount_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Reload();
        }

        private void weapon_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tab.SelectedIndex == 0)
            {
                SelectionChangedAbst();
            }
        }

        private void stage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tab.SelectedIndex == 0)
            {
                SelectionChangedAbst();
            }
        }




    }

    public struct AnalysisData
    {
        public int TotalWin { get; set; }
        public int TotalLose { get; set; }
        public double WinRatio { get { return 1.0 * TotalWin / (TotalWin + TotalLose); } }

        public int TotalKill { get; set; }
        public int TotalDeath { get; set; }
        public int KillDeathCount { get; set; }
        public double AverageKill { get { return KillDeathCount > 0 ? 1.0 * TotalKill / KillDeathCount : 0; } }
        public double AverageDeath { get { return KillDeathCount > 0 ? 1.0 * TotalDeath / KillDeathCount : 0; } }
        public double KillRatio { get { return KillDeathCount > 0 ? (TotalDeath > 0 ? 1.0 * TotalKill / TotalDeath : 1.0 * TotalKill) : 0; } }
        public double MedianKillRatio { get; set; }
        public int MaxKill { get; set; }
        public int MinKill { get; set; }
        public int MaxDeath { get; set; }
        public int MinDeath { get; set; }
        public double MaxKillRatio { get; set; }
        public double MinKillRatio { get; set; }

        public int TotalNuri { get; set; }
        public int NawabariCount { get; set; }
        public double AverageNuri { get { return NawabariCount > 0 ? 1.0 * TotalNuri / NawabariCount : 0; } }
        public int MaxNuri { get; set; }
        public int MinNuri { get; set; }

    }

    public enum ViewCount
    {
        全試合, 最近30試合, 最近50試合, 最近100試合
    }
    public class WinRatioConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return values[1].Equals(0)?"":String.Format("{0:0.0%}({1}戦)", values[0], values[1]);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            return value.ToString().Split(':');
        }
    }
}
